using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using System.IO;
using Print.Prayer;
/*
* -------------------------------------Gra tekstowa The Machine Spirit------------------------------------------------------------------------------------------------------------
* Gra osadzona w uniwersum Warhammera 40k, gracz wciela się w kapłana maszyny próbującego uruchomić generator pola siłowego. 
* W tym celu musi jednak okiełznać interface - Ducha Maszyny, który jest niezwykle kapryśny i jeśli nie będzie się go traktowało
* z odpowiednim szacunkiem i wymawiało odpowiednich modlitw, będzie się buntował.
* 
* Gra kończy się, gdy w pełni uruchomimy generator, o czym informują flagi calibrated, back_power, power, back_valves, valves oraz turned_on. 
* Gdy wszystkie mają wartość 1 gracz osiągnął sukces. Może zmieniać te wartości podając odpowiednie komendy, ponadto wpisywać może modlitwy,
* które modyfikują  zmienną prayer. Zależnie od wartości tej zmiennej określone polecenia zadziałają, nie zadziałają, lub też zostaną wykonane
* na opak. Dwie ostatnie sytuacje mogą zagniewać maszynę (angry = 1) co też ma wpływ na to jak wykona polecenia.
* */
namespace ClassLibrary1
{
    public class ZeGameAPM : ZeGame

    {

        public delegate void TransmissionDataDelegate(NetworkStream stream);

        public ZeGameAPM(IPAddress IP, int port) : base(IP, port)

        {

        }

         void AcceptClient()

        {

            while (true)

            {
                TcpClient tcpClient = TcpListener.AcceptTcpClient();
               Stream = tcpClient.GetStream();
                TransmissionDataDelegate transmissionDelegate = new TransmissionDataDelegate(BeginDataTransmission);
                transmissionDelegate.BeginInvoke(Stream, TransmissionCallback, tcpClient);

            }

        }


        private void TransmissionCallback(IAsyncResult ar)

        {

            // sprzątanie

        }

        protected override void BeginDataTransmission(NetworkStream stream)

        {

            byte[] buffer = new byte[Buffer_size];

            while (true)

            {

                try

                {
                    if (start == 0)
                    {
                        InitializeGame(stream);
                        timerInterval.Enabled = true;//uruchomienie timera
                    }
                    start = 1;
                    timerInterval.Elapsed += (sender, e) => OnTimedEvent(sender, e, stream);
                    int message_size = stream.Read(buffer, 0, Buffer_size);
                    MachineProceed(buffer, stream);
                    stream.Write(buffer, 0, message_size);

                }

                catch (IOException e)

                {

                    break;

                }

            }

        }

        public override void Start()

        {

            StartListening();

            AcceptClient();

        }


    }
    /// <summary>
    /// Klasa gry
    /// </summary>
    public abstract class ZeGame
    {
        //zmienne które są flagami odpowiedzialnymi za stan symulowanej maszyny
        int machineState_prayer = -1; //indykator tego która modlitwa była ostatnio wymawiana
        int machineState_calibrated = 0, machineState_back_power = 0,machineState_power=0,machineState_back_valves =0, machineState_valves=0,machineState_turned_on =0, machineState_angry =0; //By gracz mógł uruchomić pewne funkcje, inne muszą być już włączone - flagi
        int machine_errnum = 2128; //zmienna potrzebna do efektu kosmetycznego 
       
        public int ruchy = 0; //ilosc ruchow
        bool victoryFlag = false;
        public Timer timerInterval; 

        int bufferSize;
        public int gameTimeLasted;
        int MAX_GAME_TIME_LIMIT;

        string tmptime;//zmienna pomocnicza
        bool running;
        IPAddress ip;
        public int start = 0;
        int port;
        public ZeGame(IPAddress Ip, int Port)
        {
            ip = Ip;
            port = Port;
            bufferSize = 1024;
            gameTimeLasted = 0;
            MAX_GAME_TIME_LIMIT = 10000;
            timerInterval  = new Timer(1000); 
        }
        public ZeGame() { }
        /// <summary>
        /// procedura sprawdzajaca, czy dany string znajduje sie na poczatku bufora
        /// </summary>
        /// <param name="str">string z którym porównujemy</param>
        /// <param name="buffer">to co porównamy ze stringiem</param>
        /// <returns></returns>
        public int Buffer_size
        {
            get => bufferSize; set

            {

                if (value < 0 || value > bufferSize * bufferSize * 64) throw new Exception("Błędny rozmiar pakietu");

                if (!running) bufferSize = value; else throw new Exception("Nie można zmienić rozmiaru pakietu kiedy serwer jest uruchomiony");

            }

        }

        protected NetworkStream Stream { get; set; }
        protected abstract void BeginDataTransmission(NetworkStream stream);
        protected TcpListener TcpListener { get; set; }
        public abstract void Start();
        public IPAddress IPAddress
        {
            get => ip; set
            {
                if (!running) ip = value;
                else throw new Exception("Nie mozna zmienic adresu IP kiedy serwer jest uruchomiony");
            }
        }

        protected void StartListening()

        {
            TcpListener = new TcpListener(IPAddress, Port);
            TcpListener.Start();
        }
        protected TcpClient TcpClient { get; set; }
        protected bool checkPort()

        {
            if (port < 1024 || port > 49151) return false;
            return true;
        }
        public int Port 
        {
            get => port; set

            {
                int tmp = port;
                if (!running) port = value; else throw new Exception("Nie można zmienić portu kiedy serwer jest uruchomiony");
                if (!checkPort())
                {
                    port = tmp;
                    throw new Exception("Błędna wartość portu");
                }
            }
        }
        
        bool CompareStringToBuffer(string str, byte[] buffer) 
        {
            int fine = 1;
            string converted = Encoding.ASCII.GetString(buffer, 0, buffer.Length);
            for(int i=0; i < str.Length; i++) //petla porownujaca kolejne znaki szukanego stringu z wyszukiwana fraza
            {
                if (str[i] != converted[i]) fine = 0;
            }
            if (fine == 1)
                return true;
            else return false; //niestety ten algortym zaakceptuje jesli uzytkownik majacy podać "alamakota" poda "alamakotaimasło"
        }


        /// <summary>
        /// metoda wypisująca na ekran klienta aktualny stan
        /// </summary>
        /// <param name="stream">strumień klienta</param>
         void MachineDisplayState(NetworkStream stream) 
        {
            
            if(machineState_valves == 0) Printer.writeStringToConsole(stream,"Power-valves:  closed\r\n"); else Printer.writeStringToConsole(stream, "Power-valves:  opened\r\n");
            if (machineState_back_valves == 0) Printer.writeStringToConsole(stream, "Backup Power-valves:  closed\r\n"); else Printer.writeStringToConsole(stream, "Backup Power-valves:  opened\r\n");
            if (machineState_power == 0) Printer.writeStringToConsole(stream, "Power Generator:  disconnected\r\n"); else Printer.writeStringToConsole(stream, "Power Generator:  connected\r\n");
            if (machineState_back_power == 0) Printer.writeStringToConsole(stream, "Backup Power Generator:  disconnected\r\n"); else Printer.writeStringToConsole(stream, "Backup Power Generator:  connected\r\n");
            if (machineState_turned_on == 0) Printer.writeStringToConsole(stream, "Void Shields:  off\r\n"); else Printer.writeStringToConsole(stream, "Void Shields:  on\r\n");
        }
        /// <summary>
        /// metoda podająca aktualny stan jeśli maszyna jest zagniewana (angry wynosi 1)
        /// </summary>
        /// <param name="stream">strumień klienta</param>
        void MachineDisplayStateWrong(NetworkStream stream)
        {

            if (machineState_valves == 0) Printer.writeStringToConsole(stream, "Power-Valves: #1^4da\r\n "); else Printer.writeStringToConsole(stream, "Power-Valves: #><gg\r\n");
            if (machineState_back_valves == 0) Printer.writeStringToConsole(stream, "Er00r Power-valves:  0x11\r\n"); else Printer.writeStringToConsole(stream, "Backup -g-ajkgfa:  1120\r\n");
            if (machineState_power == 0) Printer.writeStringToConsole(stream, " :  disconnected\r\n"); else Printer.writeStringToConsole(stream, "Error(Error)==false\r\n");
            if (machineState_back_power == 0) Printer.writeStringToConsole(stream, "Error unknown variable type\r\n"); else Printer.writeStringToConsole(stream, "Error unknown function \"error(2127)\"\r\n");
            if (machineState_turned_on == 0) Printer.writeStringToConsole(stream, "E  r r:  \r\n"); else Printer.writeStringToConsole(stream, "on =  on\r\n");
        }
        /// <summary>
        /// metoda która restetuje wszystko do "ustawień fabrycznych"
        /// Zależnie od wartości message wypluwa o tym informacje w konsoli lub nie
        /// </summary>
        /// <param name="stream">strumień klienta</param>
        /// <param name="message">czy informować klienta o resecie</param>
        void MachineRestart(NetworkStream stream,bool message) 
        {
            if (message == true)
            {
                Printer.writeStringToConsole(stream, "System restarting...\r\n");
                Printer.writeStringToConsole(stream, "Complete\r\n");
            }
            machineState_prayer = -1; machineState_calibrated = 0; machineState_back_power = 0; machineState_power = 0; machineState_back_valves = 0; machineState_valves = 0; machineState_turned_on = 0; machineState_angry = 0;
        }
        /// <summary>
        /// metoda która resetuje ustawienia ale nie sprawia, iż maszyna przestanie się na nas gniewać
        /// </summary>
        /// <param name="stream">strumień klienta</param>
        public void MachineReset(NetworkStream stream) 
        {
            Printer.writeStringToConsole(stream, "Settings reset.\r\n");
            machineState_calibrated = 0; machineState_back_power = 0; machineState_power = 0; machineState_back_valves = 0; machineState_valves = 0; machineState_turned_on = 0; 
        }
        /// <summary>
        /// metoda odpowiedzialna za pojedyncze tyknięcie zegara gry
        /// </summary>
        /// <param name="stream">strumień klienta</param>
        public void TimerTicker(NetworkStream stream)
        {
            gameTimeLasted++;
            if (IsMaxTimePassed())
            {
                CheckIfGameIsWon(stream);
            }
        }
        public  void OnTimedEvent(object source, ElapsedEventArgs e, NetworkStream stream) //dd
        {
            TimerTicker(stream);
        }
        /// <summary>
        /// metoda odpowiedzialna za wypisanie na ekran gracza wiadomosci startowej, restart gry
        /// </summary>
        /// <param name="stream">strumień klienta</param>
        public void InitializeGame(NetworkStream stream) //
        {
            Printer.InitializeMenu(stream);
            gameTimeLasted = 0;
            ruchy = 0;
            MachineRestart(stream,false);
        }
        /// <summary>
        /// Funkcja dokonujaca zmian we flagach maszyny na podstawie podanej komendy oraz informujaca, czy dana komenda zostala rozpoznana
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public int MachineAnalyzeCommands(byte[] buffer, NetworkStream stream) { //zwraca 1 jak odczytal komende, 0 jak nie ogarnia co dostal
            //jako, że otrzymać możemy jedną instrukcję naraz, po jej zidentyfikowaniu udajemy się do części końcowej funkcji (Finish)
            // tutaj ustawiamy wartość zmiennej prayer
            if (CompareStringToBuffer("Let knowledge of thy fall upon me", buffer) == true) { machineState_prayer = 1; return 1; } //porównaj wpisaną wartość do kolejnych modlitw, gdybym modlitwy umieścił w tablicy to mógłbym to zrobić pętlą for
            if (CompareStringToBuffer("Blessed be thy o holy Machine let me stay in your mighty presence", buffer) == true) { Printer.writeStringToConsole(stream, "Err r wrong operating component.\r\n"); machineState_angry = 1; machineState_prayer = 2; return 1; }
            if (CompareStringToBuffer("Open your arms to your holy brethren", buffer) == true) { machineState_prayer = 3; return 1; }
            if (CompareStringToBuffer("Might your hevenly parts thrive with power", buffer) == true) { machineState_prayer = 4; return 1; }
            if (CompareStringToBuffer("Truely crude is my flesh; let your might protect it!", buffer) == true) { machineState_prayer = 5; return 1; }
            if (CompareStringToBuffer("O mighty Machine allow me to touch your blessed switches and gaze upon your holy dials", buffer) == true) { machineState_prayer = 6; return 1; }
            if (CompareStringToBuffer("Your strength has saved servants of the Omnissiah you may rest now", buffer) == true) { machineState_prayer = 7; return 1; }
            if (CompareStringToBuffer("Disperse your power", buffer) == true) { machineState_prayer = 8;  return 1; }
            if (CompareStringToBuffer("Say farewell to your brothers", buffer) == true) { machineState_prayer = 9;  return 1; }
            if (CompareStringToBuffer("O mighty Machine, be as your fabricator-father has once made you", buffer) == true) { machineState_prayer = 10; return 1; }
            if (CompareStringToBuffer("Blessed Machine, forgive me the sins I commited in your almighty presence", buffer) == true) { machineState_prayer = 11; machineState_angry = 0; return 1; } //maszyna przebacza nasze niegodne zachowanie
                                                                                                                                                                                                       //zagniewana maszyna zaakceptuje wyłącznie modlitwy oraz prośby o instrukcję, prayerbook lub reset opcji
            if (CompareStringToBuffer("/m", buffer) == true)  //instrukcja
            {
                Printer.PrintManual(stream);
                return 1;
            }
            if (CompareStringToBuffer("/p", buffer) == true)  //prayerbook
            {
                Printer.PrintPrayerbook(stream);
                return 1;
            }
            if (CompareStringToBuffer("/R", buffer) == true)  //reset ustawien
            {
                MachineReset(stream);
                return 1;
            }
            if (CompareStringToBuffer("/s", buffer) == true)  //stan
            {
                if (machineState_prayer == 1)
                    MachineDisplayState(stream);
                else { MachineDisplayStateWrong(stream); machineState_angry = 1; }
                return 1;
            }
            if (CompareStringToBuffer("/r", buffer) == true)  //restart systemu
            {
                if (machineState_prayer == 10)
                    MachineRestart(stream, true);
                else { Printer.writeStringToConsole(stream, "Error:Access denied.\r\nError:Access denied.\r\nError:Access denied.\r\nError:Access denied.\r\nError:Access denied.\r\nError:Access denied.\r\nError:Access denied.\r\n"); machineState_angry = 1; }
                return 1;
            }
            if (machineState_angry == 1) { Printer.writeStringToConsole(stream, "Error " + machine_errnum + ".\r\n"); machine_errnum++; return 1; }
            //tutaj sprawdzamy, czy podany bufor jest komendą, oraz czy jej wykonanie się powiedzie (czy wypowiedziano poprawną modlitwę)
            //int calibrated = 0, back_power = 0, power = 0, back_valves = 0, valves = 0, turned_on = 0;
            if (CompareStringToBuffer("ctvs", buffer) == true)
            { //jesli to ta komenda
                if (machineState_prayer == 6) { if (machineState_turned_on == 1) machineState_calibrated = 1; return 1; } else { Printer.writeStringToConsole(stream, "Void Shield system error 2123.\r\n"); machineState_calibrated = 0; machineState_angry = 1; return 1; } //zaleznie czy dobra modlitwa wykonaj lub zdenerwuj maszyne
            }
            if (CompareStringToBuffer("cpftbpg", buffer) == true)
            { //jesli to ta komenda
                if (machineState_prayer == 9) { machineState_back_valves = 0; machineState_back_power = 0; return 1; } else { Printer.writeStringToConsole(stream, "Void Shield system error 1273.\r\n"); machineState_back_valves = 1; machineState_angry = 1; return 1; } //zaleznie czy dobra modlitwa wykonaj lub zdenerwuj maszyne
            }
            if (CompareStringToBuffer("cpftg", buffer) == true)
            { //jesli to ta komenda
                if (machineState_prayer == 9) { machineState_valves = 0; machineState_power = 0; return 1; } else { Printer.writeStringToConsole(stream, "Void Shield system error 1273.\r\n"); machineState_valves = 1; machineState_angry = 1; return 1; } //zaleznie czy dobra modlitwa wykonaj lub zdenerwuj maszyne
            }
            if (CompareStringToBuffer("ctbpg", buffer) == true)
            { //jesli to ta komenda
                if (machineState_prayer == 4) { if (machineState_back_valves == 1) machineState_back_power = 1; return 1; } else { Printer.writeStringToConsole(stream, "Void Shield system error 1777.\r\n"); machineState_back_power = 0; machineState_angry = 1; return 1; } //zaleznie czy dobra modlitwa wykonaj lub zdenerwuj maszyne
            }
            if (CompareStringToBuffer("ctpg", buffer) == true)
            { //jesli to ta komenda
                if (machineState_prayer == 4) { if (machineState_valves == 1) machineState_power = 1; return 1; } else { Printer.writeStringToConsole(stream, "Void Shield system error 1777.\r\n"); machineState_power = 0; machineState_angry = 1; return 1; } //zaleznie czy dobra modlitwa wykonaj lub zdenerwuj maszyne
            }
            if (CompareStringToBuffer("dtbpg", buffer) == true)
            { //jesli to ta komenda
                if (machineState_prayer == 8) { machineState_back_power = 0; return 1; } else { Printer.writeStringToConsole(stream, "Void Shield system error 87.\r\n"); if (machineState_back_valves == 1) machineState_back_power = 1; machineState_angry = 1; return 1; } //zaleznie czy dobra modlitwa wykonaj lub zdenerwuj maszyne
            }
            if (CompareStringToBuffer("dtpg", buffer) == true)
            { //jesli to ta komenda
                if (machineState_prayer == 8) { machineState_power = 0; return 1; } else { Printer.writeStringToConsole(stream, "Void Shield system error 87.\r\n"); if (machineState_valves == 1) machineState_power = 1; machineState_angry = 1; return 1; } //zaleznie czy dobra modlitwa wykonaj lub zdenerwuj maszyne
            }
            if (CompareStringToBuffer("opftbpg", buffer) == true)
            { //jesli to ta komenda
                if (machineState_prayer == 3) { machineState_back_valves = 1; return 1; } else { Printer.writeStringToConsole(stream, "Void Shield system error 1234.\r\n"); machineState_back_valves = 0; machineState_angry = 1; return 1; } //zaleznie czy dobra modlitwa wykonaj lub zdenerwuj maszyne
            }
            if (CompareStringToBuffer("opftg", buffer) == true)
            { //jesli to ta komenda
                if (machineState_prayer == 3) { machineState_valves = 1; return 1; } else { Printer.writeStringToConsole(stream, "Void Shield system error 1234.\r\n"); machineState_valves = 0; machineState_angry = 1; return 1; } //zaleznie czy dobra modlitwa wykonaj lub zdenerwuj maszyne
            } 
            if (CompareStringToBuffer("Tontvs", buffer) == true)
            { //jesli to ta komenda
                if (machineState_prayer == 5) { if (machineState_power == 1 && machineState_back_power == 1) machineState_turned_on = 1; return 1; } else { Printer.writeStringToConsole(stream, "Void Shield system error 7.\r\n"); machineState_turned_on = 0; machineState_angry = 1; return 1; } //zaleznie czy dobra modlitwa wykonaj lub zdenerwuj maszyne
            }
            if (CompareStringToBuffer("Tofftvs", buffer) == true)
            { //jesli to ta komenda
                if (machineState_prayer == 7) { machineState_turned_on = 0; return 1; } else { Printer.writeStringToConsole(stream, "Void Shield system error 5.\r\n"); if (machineState_power == 1 && machineState_back_power == 1) machineState_turned_on = 1; machineState_angry = 1; return 1; } //zaleznie czy dobra modlitwa wykonaj lub zdenerwuj maszyne
            }
            return 0; //skoro te ify sie nie ten to to znaczy ze wiadomosc badana jest pewno jakims ack od klienta
        }
        /// <summary>
        /// metoda analizująca otrzymane przez serwer polecenie
        /// </summary>
        /// <param name="buffer">wysłany przez klienta bufor</param>
        /// <param name="stream">strumień klienta</param>
        public void MachineProceed(byte[] buffer, NetworkStream stream) 
        {
        
            if (IsMaxTimePassed())
            {
                // todo correct that function because nobody knows what the hell is inside
                if (MachineAnalyzeCommands(buffer, stream) == 1)
                {

                    if (IsGameWon())
                    {
                        GameSummary(stream);
                       
                    }
                    else
                    {
                        ContinueGame(stream);
                    }
                }
               
            }
            else
            {
                InitializeGame(stream);
            }
        }

        //todo check the conditional
        private bool IsMaxTimePassed()
        {
            return gameTimeLasted >= MAX_GAME_TIME_LIMIT;  
        }

        private bool IsGameWon()
        {
            return (machineState_calibrated == 1 && machineState_back_power == 1 && machineState_power == 1 && machineState_back_valves == 1 && machineState_valves == 1 && machineState_turned_on == 1);
        }

        private void ContinueGame(NetworkStream stream)
        {
            ruchy += 1;
            tmptime = (MAX_GAME_TIME_LIMIT - gameTimeLasted).ToString();
            Printer.writeStringToConsole(stream, "reply no." + ruchy + "; " + tmptime);
            Printer.writeStringToConsole(stream, " (time left to impact)\r\n");
        }
        private void GameSummary(NetworkStream stream)
        {
            victoryFlag = true;
            tmptime = (MAX_GAME_TIME_LIMIT - gameTimeLasted).ToString();
            Printer.writeStringToConsole(stream, "All systems operational; 0 issues detected.\r\n Your result:");
            Printer.writeStringToConsole(stream, tmptime);
            Printer.writeStringToConsole(stream, " (time left to impact)\r\n");
            gameTimeLasted = MAX_GAME_TIME_LIMIT - 10;
        }
        private void CheckIfGameIsWon(NetworkStream stream)
        {
            if (victoryFlag == true)
            {
                Printer.writeStringToConsole(stream, "Report: multiple damage indicators on\r\nSystem failure\r\n");
            }
        }
    }
}
