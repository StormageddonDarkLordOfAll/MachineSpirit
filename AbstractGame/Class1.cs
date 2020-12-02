using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbstractGame
{
    public abstract class ZeGame
    {
        //zmienne które są flagami odpowiedzialnymi za stan symulowanej maszyny
        int machineState_prayer = -1; //indykator tego która modlitwa była ostatnio wymawiana
        int machineState_calibrated = 0, machineState_back_power = 0, machineState_power = 0, machineState_back_valves = 0, machineState_valves = 0, machineState_turned_on = 0, machineState_angry = 0; //By gracz mógł uruchomić pewne funkcje, inne muszą być już włączone - flagi
        int machine_errnum = 2128; //zmienna potrzebna do efektu kosmetycznego 
        public int time = 0; //czas rozgrywki
        public int ruchy = 0; //ilosc ruchow
        int victory = 0; //flaga wygranej
        public Timer tim1 = new Timer(1000); //interwał timera

        int buffer_size = 1024;
        int endtime = 10000; //czas do konca gry
        string tmptime;//zmienna pomocnicza
        bool running;
        IPAddress ip;
        public int start = 0;
        int port;
        public ZeGame(IPAddress Ip, int Port)
        {
            ip = Ip;
            port = Port;
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
            get => buffer_size; set

            {

                if (value < 0 || value > 1024 * 1024 * 64) throw new Exception("błędny rozmiar pakietu");

                if (!running) buffer_size = value; else throw new Exception("nie można zmienić rozmiaru pakietu kiedy serwer jest uruchomiony");

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
        public int Port //dd
        {
            get => port; set

            {
                int tmp = port;
                if (!running) port = value; else throw new Exception("nie można zmienić portu kiedy serwer jest uruchomiony");
                if (!checkPort())
                {
                    port = tmp;
                    throw new Exception("błędna wartość portu");
                }
            }
        }

        bool CompareStringToBuffer(string str, byte[] buffer)
        {
            int fine = 1;
            string converted = Encoding.ASCII.GetString(buffer, 0, buffer.Length);
            for (int i = 0; i < str.Length; i++) //petla porownujaca kolejne znaki szukanego stringu z wyszukiwana fraza
            {
                if (str[i] != converted[i]) fine = 0;
            }
            if (fine == 1)
                return true;
            else return false; //niestety ten algortym zaakceptuje jesli uzytkownik majacy podać "alamakota" poda "alamakotaimasło"
        }
        /// <summary>
        /// metoda wypisująca podany tekst na ekran klienta
        /// </summary>
        /// <param name="stream">strumień klienta</param>
        /// <param name="s">co wypisać</param>
        void writStr(NetworkStream stream, string s)
        {
            byte[] buffer = new byte[1024]; //bufor
            buffer = Encoding.ASCII.GetBytes(s); //przerzucenie stringa do bufora
            stream.Write(buffer, 0, buffer.Length); //wypisanie bufora
        }
        /// <summary>
        /// metoda wypisująca na ekran instrukcję z komendami
        /// </summary>
        /// <param name="stream">strumień klienta</param>
        void PrintManual(NetworkStream stream)
        {
            writStr(stream, "Void Shield Generator \"Glimmer\" Pattern User Manual\r\n\r\n\r\n");
            writStr(stream, "Calibrate the Void Shields:ctvs\r\nClose Power-valves for the Backup Power Generator:cpftbpg\r\n");
            writStr(stream, "Close Power-valves for the Generator:cpftg\r\nConnect the Backup Power Generator:ctbpg\r\n");
            writStr(stream, "Connect the Power Generator:ctpg\r\nDisconnect the Backup Power Generator:dtbpg\r\n");
            writStr(stream, "Disconnect the Power Generator:dtpg\r\nOpen Power-valves for the Backup Power Generator:opftbpg\r\n");
            writStr(stream, "Open Power-valves for the Generator:opftg\r\nOpen Prayerbook:/p\r\n");
            writStr(stream, "Restart:/r\r\nReset Settings:/R\r\n");
            writStr(stream, "Status:/s\r\nTurn on the Void Shields:Tontvs\r\n");
            writStr(stream, "Turn off the Void Shields:Tofftvs\r\n\r\n");
        }
        /// <summary>
        /// metoda wypluwająca na ekran modlitwy
        /// </summary>
        /// <param name="stream">strumień klienta</param>
        void PrintPrayerbook(NetworkStream stream)
        {
            writStr(stream, "Void Shield Generator \"Glimmer\" Pattern Prayerbook\r\n");
            writStr(stream, "\r\n<<prayer>> notes\r\n\r\n");
            writStr(stream, "Prayer of the Knowledge-Seeker\r\n<<Let knowledge of thy fall upon me>>\r\n\r\n");
            writStr(stream, "Chant of the Awakening\r\n<<Blessed be thy o holy Machine let me stay in your mighty presence>> some priests skip this line\r\n<<Open your arms to your holy brethren>>\r\n<<Might your hevenly parts thrive with power>>\r\n<<Truely crude is my flesh; let your might protect it!>>\r\n\r\n");
            writStr(stream, "Book of Calibration 6:1\r\n<<O mighty Machine allow me to touch your blessed switches and gaze upon your holy dials>>\r\n");
            writStr(stream, "Chant of the Eternal Slumber\r\n<<Blessed be thy o holy Machine let me stay in your mighty presence>>some priests skip this line\r\n<<Your strength has saved servants of the Omnissiah you may rest now>>\r\n<<Disperse your power>>\r\n<<Say farewell to your brothers>>");
            writStr(stream, "Chant of Reversion\r\n<<O mighty Machine, be as your fabricator-father has once made you>>\r\n");
            writStr(stream, "Psalm of the Guilty\r\n<<Blessed Machine, forgive me the sins I commited in your almighty presence>>\r\n\r\n");
        }
        /// <summary>
        /// metoda wypisująca na ekran klienta aktualny stan
        /// </summary>
        /// <param name="stream">strumień klienta</param>
        void MachineDisplayState(NetworkStream stream)
        {

            if (machineState_valves == 0) writStr(stream, "Power-valves:  closed\r\n"); else writStr(stream, "Power-valves:  opened\r\n");
            if (machineState_back_valves == 0) writStr(stream, "Backup Power-valves:  closed\r\n"); else writStr(stream, "Backup Power-valves:  opened\r\n");
            if (machineState_power == 0) writStr(stream, "Power Generator:  disconnected\r\n"); else writStr(stream, "Power Generator:  connected\r\n");
            if (machineState_back_power == 0) writStr(stream, "Backup Power Generator:  disconnected\r\n"); else writStr(stream, "Backup Power Generator:  connected\r\n");
            if (machineState_turned_on == 0) writStr(stream, "Void Shields:  off\r\n"); else writStr(stream, "Void Shields:  on\r\n");
        }
        /// <summary>
        /// metoda podająca aktualny stan jeśli maszyna jest zagniewana (angry wynosi 1)
        /// </summary>
        /// <param name="stream">strumień klienta</param>
        void MachineDisplayStateWrong(NetworkStream stream)
        {

            if (machineState_valves == 0) writStr(stream, "Power-Valves: #1^4da\r\n "); else writStr(stream, "Power-Valves: #><gg\r\n");
            if (machineState_back_valves == 0) writStr(stream, "Er00r Power-valves:  0x11\r\n"); else writStr(stream, "Backup -g-ajkgfa:  1120\r\n");
            if (machineState_power == 0) writStr(stream, " :  disconnected\r\n"); else writStr(stream, "Error(Error)==false\r\n");
            if (machineState_back_power == 0) writStr(stream, "Error unknown variable type\r\n"); else writStr(stream, "Error unknown function \"error(2127)\"\r\n");
            if (machineState_turned_on == 0) writStr(stream, "E  r r:  \r\n"); else writStr(stream, "on =  on\r\n");
        }
        /// <summary>
        /// metoda która restetuje wszystko do "ustawień fabrycznych"
        /// Zależnie od wartości message wypluwa o tym informacje w konsoli lub nie
        /// </summary>
        /// <param name="stream">strumień klienta</param>
        /// <param name="message">czy informować klienta o resecie</param>
        void MachineRestart(NetworkStream stream, bool message)
        {
            if (message == true)
            {
                writStr(stream, "System restarting...\r\n");
                writStr(stream, "Complete\r\n");
            }
            machineState_prayer = -1; machineState_calibrated = 0; machineState_back_power = 0; machineState_power = 0; machineState_back_valves = 0; machineState_valves = 0; machineState_turned_on = 0; machineState_angry = 0;
        }
        /// <summary>
        /// metoda która resetuje ustawienia ale nie sprawia, iż maszyna przestanie się na nas gniewać
        /// </summary>
        /// <param name="stream">strumień klienta</param>
        public void MachineReset(NetworkStream stream)
        {
            writStr(stream, "Settings reset.\r\n");
            machineState_calibrated = 0; machineState_back_power = 0; machineState_power = 0; machineState_back_valves = 0; machineState_valves = 0; machineState_turned_on = 0;
        }
        /// <summary>
        /// metoda odpowiedzialna za pojedyncze tyknięcie zegara gry
        /// </summary>
        /// <param name="stream">strumień klienta</param>
        public void Tim(NetworkStream stream)
        {
            time++;
            if (time >= endtime) //koniec gry
            {
                if (victory == 0)
                {
                    writStr(stream, "Report: multiple damage indicators on\r\nSystem failure\r\n");
                }
            }
        }
        public void OnTimedEvent(object source, ElapsedEventArgs e, NetworkStream stream) //dd
        {
            Tim(stream);
        }
        /// <summary>
        /// metoda odpowiedzialna za wypisanie na ekran gracza wiadomosci startowej, restart gry
        /// </summary>
        /// <param name="stream">strumień klienta</param>
        public void InitializeGame(NetworkStream stream) //
        {
            writStr(stream, "Planet: Ridyria Secundus.\r\n");
            writStr(stream, "Date: 493.992.M41.\r\n");
            writStr(stream, "Void Shield Generator Control Chapel.\r\n");
            writStr(stream, "Message from Magos lexmechanicus Saturninus:\r\nA fleet has emerged into the realspace a few hours ago.\r\nLack of communication indicates The Great Enemy. Condition of my\r\n");
            writStr(stream, "flesh once more has proven the superiority of mechanical parts I installed\r\nin myself and forced me into medicae. I cannot join you here to TURN ON the VOID\r\n");
            writStr(stream, "SHIELDS above the capital.\r\nIt is cruicial that you do it in my stead. Talk to the Spirit of the \r\nvoid shield generator and convince it to turn itself on. Remember about \r\n");
            writStr(stream, "the Prayers!\r\n\r\n");
            writStr(stream, "PS To consult the Manual type /m. Any other command will be treated by \r\nthe Spirit as a communication attempt. Caution! This Machine Spirit is \r\nvery case sensitive! It gets really moody if you do not USE APPROPRIATE PRAYERS \r\nBEFORE COMMANDS.\r\n");
            writStr(stream, "May the Omnissiah guide you or this planet will be doomed.\r\n");
            writStr(stream, "\r\n");
            writStr(stream, "Loading interface...\r\n\r\nInterface loaded.\r\n==Void Shield Generator \"Glimmer\" Pattern== \r\n ");
            time = 0;
            ruchy = 0;
            MachineRestart(stream, false);
        }
        /// <summary>
        /// Funkcja dokonujaca zmian we flagach maszyny na podstawie podanej komendy oraz informujaca, czy dana komenda zostala rozpoznana
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public int MachineAnalyzeCommands(byte[] buffer, NetworkStream stream)
        { //zwraca 1 jak odczytal komende, 0 jak nie ogarnia co dostal
            //jako, że otrzymać możemy jedną instrukcję naraz, po jej zidentyfikowaniu udajemy się do części końcowej funkcji (Finish)
            // tutaj ustawiamy wartość zmiennej prayer
            if (CompareStringToBuffer("Let knowledge of thy fall upon me", buffer) == true) { machineState_prayer = 1; return 1; } //porównaj wpisaną wartość do kolejnych modlitw, gdybym modlitwy umieścił w tablicy to mógłbym to zrobić pętlą for
            if (CompareStringToBuffer("Blessed be thy o holy Machine let me stay in your mighty presence", buffer) == true) { writStr(stream, "Err r wrong operating component.\r\n"); machineState_angry = 1; machineState_prayer = 2; return 1; }
            if (CompareStringToBuffer("Open your arms to your holy brethren", buffer) == true) { machineState_prayer = 3; return 1; }
            if (CompareStringToBuffer("Might your hevenly parts thrive with power", buffer) == true) { machineState_prayer = 4; return 1; }
            if (CompareStringToBuffer("Truely crude is my flesh; let your might protect it!", buffer) == true) { machineState_prayer = 5; return 1; }
            if (CompareStringToBuffer("O mighty Machine allow me to touch your blessed switches and gaze upon your holy dials", buffer) == true) { machineState_prayer = 6; return 1; }
            if (CompareStringToBuffer("Your strength has saved servants of the Omnissiah you may rest now", buffer) == true) { machineState_prayer = 7; return 1; }
            if (CompareStringToBuffer("Disperse your power", buffer) == true) { machineState_prayer = 8; return 1; }
            if (CompareStringToBuffer("Say farewell to your brothers", buffer) == true) { machineState_prayer = 9; return 1; }
            if (CompareStringToBuffer("O mighty Machine, be as your fabricator-father has once made you", buffer) == true) { machineState_prayer = 10; return 1; }
            if (CompareStringToBuffer("Blessed Machine, forgive me the sins I commited in your almighty presence", buffer) == true) { machineState_prayer = 11; machineState_angry = 0; return 1; } //maszyna przebacza nasze niegodne zachowanie
                                                                                                                                                                                                    //zagniewana maszyna zaakceptuje wyłącznie modlitwy oraz prośby o instrukcję, prayerbook lub reset opcji
            if (CompareStringToBuffer("/m", buffer) == true)  //instrukcja
            {
                PrintManual(stream);
                return 1;
            }
            if (CompareStringToBuffer("/p", buffer) == true)  //prayerbook
            {
                PrintPrayerbook(stream);
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
                else { writStr(stream, "Error:Access denied.\r\nError:Access denied.\r\nError:Access denied.\r\nError:Access denied.\r\nError:Access denied.\r\nError:Access denied.\r\nError:Access denied.\r\n"); machineState_angry = 1; }
                return 1;
            }
            if (machineState_angry == 1) { writStr(stream, "Error " + machine_errnum + ".\r\n"); machine_errnum++; return 1; }
            //tutaj sprawdzamy, czy podany bufor jest komendą, oraz czy jej wykonanie się powiedzie (czy wypowiedziano poprawną modlitwę)
            //int calibrated = 0, back_power = 0, power = 0, back_valves = 0, valves = 0, turned_on = 0;
            if (CompareStringToBuffer("ctvs", buffer) == true)
            { //jesli to ta komenda
                if (machineState_prayer == 6) { if (machineState_turned_on == 1) machineState_calibrated = 1; return 1; } else { writStr(stream, "Void Shield system error 2123.\r\n"); machineState_calibrated = 0; machineState_angry = 1; return 1; } //zaleznie czy dobra modlitwa wykonaj lub zdenerwuj maszyne
            }
            if (CompareStringToBuffer("cpftbpg", buffer) == true)
            { //jesli to ta komenda
                if (machineState_prayer == 9) { machineState_back_valves = 0; machineState_back_power = 0; return 1; } else { writStr(stream, "Void Shield system error 1273.\r\n"); machineState_back_valves = 1; machineState_angry = 1; return 1; } //zaleznie czy dobra modlitwa wykonaj lub zdenerwuj maszyne
            }
            if (CompareStringToBuffer("cpftg", buffer) == true)
            { //jesli to ta komenda
                if (machineState_prayer == 9) { machineState_valves = 0; machineState_power = 0; return 1; } else { writStr(stream, "Void Shield system error 1273.\r\n"); machineState_valves = 1; machineState_angry = 1; return 1; } //zaleznie czy dobra modlitwa wykonaj lub zdenerwuj maszyne
            }
            if (CompareStringToBuffer("ctbpg", buffer) == true)
            { //jesli to ta komenda
                if (machineState_prayer == 4) { if (machineState_back_valves == 1) machineState_back_power = 1; return 1; } else { writStr(stream, "Void Shield system error 1777.\r\n"); machineState_back_power = 0; machineState_angry = 1; return 1; } //zaleznie czy dobra modlitwa wykonaj lub zdenerwuj maszyne
            }
            if (CompareStringToBuffer("ctpg", buffer) == true)
            { //jesli to ta komenda
                if (machineState_prayer == 4) { if (machineState_valves == 1) machineState_power = 1; return 1; } else { writStr(stream, "Void Shield system error 1777.\r\n"); machineState_power = 0; machineState_angry = 1; return 1; } //zaleznie czy dobra modlitwa wykonaj lub zdenerwuj maszyne
            }
            if (CompareStringToBuffer("dtbpg", buffer) == true)
            { //jesli to ta komenda
                if (machineState_prayer == 8) { machineState_back_power = 0; return 1; } else { writStr(stream, "Void Shield system error 87.\r\n"); if (machineState_back_valves == 1) machineState_back_power = 1; machineState_angry = 1; return 1; } //zaleznie czy dobra modlitwa wykonaj lub zdenerwuj maszyne
            }
            if (CompareStringToBuffer("dtpg", buffer) == true)
            { //jesli to ta komenda
                if (machineState_prayer == 8) { machineState_power = 0; return 1; } else { writStr(stream, "Void Shield system error 87.\r\n"); if (machineState_valves == 1) machineState_power = 1; machineState_angry = 1; return 1; } //zaleznie czy dobra modlitwa wykonaj lub zdenerwuj maszyne
            }
            if (CompareStringToBuffer("opftbpg", buffer) == true)
            { //jesli to ta komenda
                if (machineState_prayer == 3) { machineState_back_valves = 1; return 1; } else { writStr(stream, "Void Shield system error 1234.\r\n"); machineState_back_valves = 0; machineState_angry = 1; return 1; } //zaleznie czy dobra modlitwa wykonaj lub zdenerwuj maszyne
            }
            if (CompareStringToBuffer("opftg", buffer) == true)
            { //jesli to ta komenda
                if (machineState_prayer == 3) { machineState_valves = 1; return 1; } else { writStr(stream, "Void Shield system error 1234.\r\n"); machineState_valves = 0; machineState_angry = 1; return 1; } //zaleznie czy dobra modlitwa wykonaj lub zdenerwuj maszyne
            }
            if (CompareStringToBuffer("Tontvs", buffer) == true)
            { //jesli to ta komenda
                if (machineState_prayer == 5) { if (machineState_power == 1 && machineState_back_power == 1) machineState_turned_on = 1; return 1; } else { writStr(stream, "Void Shield system error 7.\r\n"); machineState_turned_on = 0; machineState_angry = 1; return 1; } //zaleznie czy dobra modlitwa wykonaj lub zdenerwuj maszyne
            }
            if (CompareStringToBuffer("Tofftvs", buffer) == true)
            { //jesli to ta komenda
                if (machineState_prayer == 7) { machineState_turned_on = 0; return 1; } else { writStr(stream, "Void Shield system error 5.\r\n"); if (machineState_power == 1 && machineState_back_power == 1) machineState_turned_on = 1; machineState_angry = 1; return 1; } //zaleznie czy dobra modlitwa wykonaj lub zdenerwuj maszyne
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
            //sprawdzenie czy rozgrywka sie nie skonczyla:
            if (time <= endtime)
            {

                if (MachineAnalyzeCommands(buffer, stream) == 1)
                {

                    if (machineState_calibrated == 1 && machineState_back_power == 1 && machineState_power == 1 && machineState_back_valves == 1 && machineState_valves == 1 && machineState_turned_on == 1) //koniec gry, zwyciestwo
                    {
                        victory = 1;
                        tmptime = (endtime - time).ToString();
                        writStr(stream, "All systems operational; 0 issues detected.\r\n Your result:");
                        writStr(stream, tmptime);
                        writStr(stream, " (time left to impact)\r\n");
                        time = endtime - 10;
                        //InitializeGame(stream);
                    }
                    else
                    {
                        ruchy += 1;
                        tmptime = (endtime - time).ToString();
                        writStr(stream, "reply no." + ruchy + "; " + tmptime);
                        writStr(stream, " (time left to impact)\r\n");
                    }
                }
                //Finish2:; //sytuacja gdy wiadomosc zostala nierozpoznana przez program
            }
            else //jesli czas sie zakonczyl
            {
                InitializeGame(stream);
            }
        }
    }
}
