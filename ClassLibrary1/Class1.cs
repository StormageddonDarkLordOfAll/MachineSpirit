using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using System.IO;
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
                //callback style

                transmissionDelegate.BeginInvoke(Stream, TransmissionCallback, tcpClient);

                // async result style

                //IAsyncResult result = transmissionDelegate.BeginInvoke(Stream, null, null);

                ////operacje......

                //while (!result.IsCompleted) ;

                ////sprzątanie

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
                        tim1.Enabled = true;//uruchomienie timera
                    }
                    start = 1;
                    tim1.Elapsed += (sender, e) => OnTimedEvent(sender, e, stream);
                    int message_size = stream.Read(buffer, 0, Buffer_size);
                    Proceed(buffer, stream);
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

            //transmission starts within the accept function

            AcceptClient();

        }


    }
    /// <summary>
    /// Klasa gry
    /// </summary>
    public abstract class ZeGame
    {
        int prayer = -1; //indykator tego która modlitwa była ostatnio wymawiana
        int calibrated = 0, back_power = 0,power=0,back_valves =0, valves=0,turned_on =0, angry =0; //By gracz mógł uruchomić pewne funkcje, inne muszą być już włączone - flagi
        int errnum = 2128; //zmienna potrzebna do efektu kosmetycznego 
        public int time = 0; //czas rozgrywki
        int buffer_size = 1024;//dd
        int endtime = 10000; //czas do konca gry
        string tmptime;//zmienna pomocnicza
        int victory = 0; //flaga wygranej
        NetworkStream stream; //dd (dodane)
        TcpListener tcpListener;//dd
        TcpClient tcpClient;//dd
        public int ruchy = 0; //ilosc ruchow //dd
        bool running;//dd
        IPAddress ip;//dd
        public Timer tim1 = new Timer(1000); //interwał timera //dd
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
        public int Buffer_size //dd
        {
            get => buffer_size; set

            {

                if (value < 0 || value > 1024 * 1024 * 64) throw new Exception("błędny rozmiar pakietu");

                if (!running) buffer_size = value; else throw new Exception("nie można zmienić rozmiaru pakietu kiedy serwer jest uruchomiony");

            }

        }
        protected NetworkStream Stream { get => stream; set => stream = value; } //dd
        protected abstract void BeginDataTransmission(NetworkStream stream); //dd
        protected TcpListener TcpListener { get => tcpListener; set => tcpListener = value; }//dd
        public abstract void Start();//dd
        public IPAddress IPAddress //DD
        {
            get => ip; set
            {
                if (!running) ip = value;
                else throw new Exception("Nie mozna zmienic adresu IP kiedy serwer jest uruchomiony");
            }
        }

        protected void StartListening() //dd

        {
            TcpListener = new TcpListener(IPAddress, Port);
            TcpListener.Start();
        }
        protected TcpClient TcpClient { get => tcpClient; set => tcpClient = value; }//dd
        protected bool checkPort() //dd

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
        
        bool comp(string str, byte[] buffer) 
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
        /// metoda wypisująca podany tekst na ekran klienta
        /// </summary>
        /// <param name="stream">strumień klienta</param>
        /// <param name="s">co wypisać</param>
        void writStr(NetworkStream stream,string s) 
        {
            byte[] buffer = new byte[1024]; //bufor
            buffer = Encoding.ASCII.GetBytes(s); //przerzucenie stringa do bufora
            stream.Write(buffer, 0, buffer.Length); //wypisanie bufora
        }
      /// <summary>
      /// metoda wypisująca na ekran instrukcję z komendami
      /// </summary>
      /// <param name="stream">strumień klienta</param>
         void Manual(NetworkStream stream) 
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
         void Prayerbook(NetworkStream stream) 
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
         void State(NetworkStream stream) 
        {
            
            if(valves == 0) writStr(stream,"Power-valves:  closed\r\n"); else writStr(stream, "Power-valves:  opened\r\n");
            if (back_valves == 0) writStr(stream, "Backup Power-valves:  closed\r\n"); else writStr(stream, "Backup Power-valves:  opened\r\n");
            if (power == 0) writStr(stream, "Power Generator:  disconnected\r\n"); else writStr(stream, "Power Generator:  connected\r\n");
            if (back_power == 0) writStr(stream, "Backup Power Generator:  disconnected\r\n"); else writStr(stream, "Backup Power Generator:  connected\r\n");
            if (turned_on == 0) writStr(stream, "Void Shields:  off\r\n"); else writStr(stream, "Void Shields:  on\r\n");
        }
        /// <summary>
        /// metoda podająca aktualny stan jeśli maszyna jest zagniewana (angry wynosi 1)
        /// </summary>
        /// <param name="stream">strumień klienta</param>
        void StateWrong(NetworkStream stream)
        {

            if (valves == 0) writStr(stream, "Power-Valves: #1^4da\r\n "); else writStr(stream, "Power-Valves: #><gg\r\n");
            if (back_valves == 0) writStr(stream, "Er00r Power-valves:  0x11\r\n"); else writStr(stream, "Backup -g-ajkgfa:  1120\r\n");
            if (power == 0) writStr(stream, " :  disconnected\r\n"); else writStr(stream, "Error(Error)==false\r\n");
            if (back_power == 0) writStr(stream, "Error unknown variable type\r\n"); else writStr(stream, "Error unknown function \"error(2127)\"\r\n");
            if (turned_on == 0) writStr(stream, "E  r r:  \r\n"); else writStr(stream, "on =  on\r\n");
        }
        /// <summary>
        /// metoda która restetuje wszystko do "ustawień fabrycznych"
        /// </summary>
        /// <param name="stream">strumień klienta</param>
        /// <param name="message">czy informować klienta o resecie</param>
        void Restart(NetworkStream stream,bool message) 
        {
            if (message == true)
            {
                writStr(stream, "System restarting...\r\n");
                writStr(stream, "Complete\r\n");
            }
            prayer = -1; calibrated = 0; back_power = 0; power = 0; back_valves = 0; valves = 0; turned_on = 0; angry = 0;
        }
        /// <summary>
        /// metoda która resetuje ustawienia ale nie sprawia, iż maszyna przestanie się na nas gniewać
        /// </summary>
        /// <param name="stream">strumień klienta</param>
        public void Reset(NetworkStream stream) 
        {
            writStr(stream, "Settings reset.\r\n");
            calibrated = 0; back_power = 0; power = 0; back_valves = 0; valves = 0; turned_on = 0; 
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
                if (victory==0)
                {
                    writStr(stream, "Report: multiple damage indicators on\r\nSystem failure\r\n");
                }
            }
        }
        public  void OnTimedEvent(object source, ElapsedEventArgs e, NetworkStream stream) //dd
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
            Restart(stream,false);
        }
        /// <summary>
        /// metoda analizująca otrzymane przez serwer polecenie
        /// </summary>
        /// <param name="buffer">wysłany przez klienta bufor</param>
        /// <param name="stream">strumień klienta</param>
        public void Proceed(byte[] buffer, NetworkStream stream) 
        {
            //sprawdzenie czy rozgrywka sie nie skonczyla:
            if (time <= endtime)
            {
                //jako, że otrzymać możemy jedną instrukcję naraz, po jej zidentyfikowaniu udajemy się do części końcowej funkcji (Finish)
                // tutaj ustawiamy wartość zmiennej prayer
                if (comp("Let knowledge of thy fall upon me", buffer) == true) { prayer = 1; goto Finish; } //porównaj wpisaną wartość do kolejnych modlitw, gdybym modlitwy umieścił w tablicy to mógłbym to zrobić pętlą for
                if (comp("Blessed be thy o holy Machine let me stay in your mighty presence", buffer) == true) { writStr(stream, "Err r wrong operating component.\r\n"); angry = 1; prayer = 2; goto Finish; }
                if (comp("Open your arms to your holy brethren", buffer) == true) { prayer = 3; goto Finish; }
                if (comp("Might your hevenly parts thrive with power", buffer) == true) { prayer = 4; goto Finish; }
                if (comp("Truely crude is my flesh; let your might protect it!", buffer) == true) { prayer = 5; goto Finish; }
                if (comp("O mighty Machine allow me to touch your blessed switches and gaze upon your holy dials", buffer) == true) { prayer = 6; goto Finish; }
                if (comp("Your strength has saved servants of the Omnissiah you may rest now", buffer) == true) { prayer = 7; goto Finish; }
                if (comp("Disperse your power", buffer) == true) { prayer = 8; goto Finish; }
                if (comp("Say farewell to your brothers", buffer) == true) { prayer = 9; goto Finish; }
                if (comp("O mighty Machine, be as your fabricator-father has once made you", buffer) == true) { prayer = 10; goto Finish; }
                if (comp("Blessed Machine, forgive me the sins I commited in your almighty presence", buffer) == true) { prayer = 11; angry = 0; goto Finish; } //maszyna przebacza nasze niegodne zachowanie
                                                                                                                                                                //zagniewana maszyna zaakceptuje wyłącznie modlitwy oraz prośby o instrukcję, prayerbook lub reset opcji
                if (comp("/m", buffer) == true)  //instrukcja
                {
                    Manual(stream);
                    goto Finish;
                }
                if (comp("/p", buffer) == true)  //prayerbook
                {
                    Prayerbook(stream);
                    goto Finish;
                }
                if (comp("/R", buffer) == true)  //reset ustawien
                {
                    Reset(stream);
                    goto Finish;
                }
                if (comp("/s", buffer) == true)  //stan
                {
                    if (prayer == 1)
                        State(stream);
                    else { StateWrong(stream); angry = 1; }
                    goto Finish;
                }
                if (comp("/r", buffer) == true)  //restart systemu
                {
                    if (prayer == 10)
                        Restart(stream,true);
                    else { writStr(stream, "Error:Access denied.\r\nError:Access denied.\r\nError:Access denied.\r\nError:Access denied.\r\nError:Access denied.\r\nError:Access denied.\r\nError:Access denied.\r\n"); angry = 1; }
                    goto Finish;
                }
                if (angry == 1) { writStr(stream, "Error " + errnum + ".\r\n"); errnum++; goto Finish; }
                //tutaj sprawdzamy, czy podany bufor jest komendą, oraz czy jej wykonanie się powiedzie (czy wypowiedziano poprawną modlitwę)
                //int calibrated = 0, back_power = 0, power = 0, back_valves = 0, valves = 0, turned_on = 0;
                if (comp("ctvs", buffer) == true)
                { //jesli to ta komenda
                    if (prayer == 6) { if(turned_on==1)calibrated = 1; goto Finish; } else { writStr(stream, "Void Shield system error 2123.\r\n"); calibrated = 0; angry = 1; goto Finish; } //zaleznie czy dobra modlitwa wykonaj lub zdenerwuj maszyne
                }
                if (comp("cpftbpg", buffer) == true)
                { //jesli to ta komenda
                    if (prayer == 9) { back_valves = 0; back_power = 0; goto Finish; } else { writStr(stream, "Void Shield system error 1273.\r\n"); back_valves = 1; angry = 1; goto Finish; } //zaleznie czy dobra modlitwa wykonaj lub zdenerwuj maszyne
                }
                if (comp("cpftg", buffer) == true)
                { //jesli to ta komenda
                    if (prayer == 9) { valves = 0; power = 0; goto Finish; } else { writStr(stream, "Void Shield system error 1273.\r\n"); valves = 1; angry = 1; goto Finish; } //zaleznie czy dobra modlitwa wykonaj lub zdenerwuj maszyne
                }
                if (comp("ctbpg", buffer) == true)
                { //jesli to ta komenda
                    if (prayer == 4) { if (back_valves == 1) back_power = 1; goto Finish; } else { writStr(stream, "Void Shield system error 1777.\r\n"); back_power = 0; angry = 1; goto Finish; } //zaleznie czy dobra modlitwa wykonaj lub zdenerwuj maszyne
                }
                if (comp("ctpg", buffer) == true)
                { //jesli to ta komenda
                    if (prayer == 4) { if (valves == 1) power = 1; goto Finish; } else { writStr(stream, "Void Shield system error 1777.\r\n"); power = 0; angry = 1; goto Finish; } //zaleznie czy dobra modlitwa wykonaj lub zdenerwuj maszyne
                }
                if (comp("dtbpg", buffer) == true)
                { //jesli to ta komenda
                    if (prayer == 8) { back_power = 0; goto Finish; } else { writStr(stream, "Void Shield system error 87.\r\n"); if (back_valves == 1) back_power = 1; angry = 1; goto Finish; } //zaleznie czy dobra modlitwa wykonaj lub zdenerwuj maszyne
                }
                if (comp("dtpg", buffer) == true)
                { //jesli to ta komenda
                    if (prayer == 8) { power = 0; goto Finish; } else { writStr(stream, "Void Shield system error 87.\r\n"); if (valves == 1) power = 1; angry = 1; goto Finish; } //zaleznie czy dobra modlitwa wykonaj lub zdenerwuj maszyne
                }
                if (comp("opftbpg", buffer) == true)
                { //jesli to ta komenda
                    if (prayer == 3) { back_valves = 1; goto Finish; } else { writStr(stream, "Void Shield system error 1234.\r\n"); back_valves = 0; angry = 1; goto Finish; } //zaleznie czy dobra modlitwa wykonaj lub zdenerwuj maszyne
                }
                if (comp("opftg", buffer) == true)
                { //jesli to ta komenda
                    if (prayer == 3) { valves = 1; goto Finish; } else { writStr(stream, "Void Shield system error 1234.\r\n"); valves = 0; angry = 1; goto Finish; } //zaleznie czy dobra modlitwa wykonaj lub zdenerwuj maszyne
                }
                if (comp("Tontvs", buffer) == true)
                { //jesli to ta komenda
                    if (prayer == 5) { if (power == 1 && back_power == 1) turned_on = 1; goto Finish; } else { writStr(stream, "Void Shield system error 7.\r\n"); turned_on = 0; angry = 1; goto Finish; } //zaleznie czy dobra modlitwa wykonaj lub zdenerwuj maszyne
                }
                if (comp("Tofftvs", buffer) == true)
                { //jesli to ta komenda
                    if (prayer == 7) { turned_on = 0; goto Finish; } else { writStr(stream, "Void Shield system error 5.\r\n"); if (power == 1 && back_power == 1) turned_on = 1; angry = 1; goto Finish; } //zaleznie czy dobra modlitwa wykonaj lub zdenerwuj maszyne
                }
                goto Finish2;
            Finish:;

                if (calibrated == 1 && back_power == 1 && power == 1 && back_valves == 1 && valves == 1 && turned_on == 1) //koniec gry, zwyciestwo
                {
                    victory = 1;
                    tmptime = (endtime - time).ToString();
                    writStr(stream, "All systems operational; 0 issues detected.\r\n Your result:");
                    writStr(stream, tmptime);
                    writStr(stream, " (time left to impact)\r\n");
                    time = endtime-10;
                    //InitializeGame(stream);
                }
                else
                {
                    ruchy += 1;
                    tmptime = (endtime - time).ToString();
                    writStr(stream,"reply no."+ruchy+"; "+tmptime);
                    writStr(stream, " (time left to impact)\r\n");
                }
                Finish2:; //sytuacja gdy wiadomosc zostala nierozpoznana przez program
            }
            else //jesli czas sie zakonczyl
            {
                InitializeGame(stream);
            }
        }
    }
}
