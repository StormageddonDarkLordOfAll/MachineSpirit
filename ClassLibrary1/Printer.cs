using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Timers;
using System.Net.Sockets;

namespace PrintText
{
    public class Printer
    {

        /// <summary>
        /// metoda wypisująca na ekran instrukcję z komendami
        /// </summary>
        /// <param name="stream">strumień klienta</param>
        static public void PrintManual(NetworkStream stream)
        {
            writeStringToConsole(stream, "Void Shield Generator \"Glimmer\" Pattern User Manual\r\n\r\n\r\n");
            writeStringToConsole(stream, "Calibrate the Void Shields:ctvs\r\nClose Power-valves for the Backup Power Generator:cpftbpg\r\n");
            writeStringToConsole(stream, "Close Power-valves for the Generator:cpftg\r\nConnect the Backup Power Generator:ctbpg\r\n");
            writeStringToConsole(stream, "Connect the Power Generator:ctpg\r\nDisconnect the Backup Power Generator:dtbpg\r\n");
            writeStringToConsole(stream, "Disconnect the Power Generator:dtpg\r\nOpen Power-valves for the Backup Power Generator:opftbpg\r\n");
            writeStringToConsole(stream, "Open Power-valves for the Generator:opftg\r\nOpen Prayerbook:/p\r\n");
            writeStringToConsole(stream, "Restart:/r\r\nReset Settings:/R\r\n");
            writeStringToConsole(stream, "Status:/s\r\nTurn on the Void Shields:Tontvs\r\n");
            writeStringToConsole(stream, "Turn off the Void Shields:Tofftvs\r\n\r\n");
        }
        /// <summary>
        /// metoda wypluwająca na ekran modlitwy
        /// </summary>
        /// <param name="stream">strumień klienta</param>
        static public void PrintPrayerbook(NetworkStream stream)
        {
            writeStringToConsole(stream, "Void Shield Generator \"Glimmer\" Pattern Prayerbook\r\n");
            writeStringToConsole(stream, "\r\n<<prayer>> notes\r\n\r\n");
            writeStringToConsole(stream, "Prayer of the Knowledge-Seeker\r\n<<Let knowledge of thy fall upon me>>\r\n\r\n");
            writeStringToConsole(stream, "Chant of the Awakening\r\n<<Blessed be thy o holy Machine let me stay in your mighty presence>> some priests skip this line\r\n<<Open your arms to your holy brethren>>\r\n<<Might your hevenly parts thrive with power>>\r\n<<Truely crude is my flesh; let your might protect it!>>\r\n\r\n");
            writeStringToConsole(stream, "Book of Calibration 6:1\r\n<<O mighty Machine allow me to touch your blessed switches and gaze upon your holy dials>>\r\n");
            writeStringToConsole(stream, "Chant of the Eternal Slumber\r\n<<Blessed be thy o holy Machine let me stay in your mighty presence>>some priests skip this line\r\n<<Your strength has saved servants of the Omnissiah you may rest now>>\r\n<<Disperse your power>>\r\n<<Say farewell to your brothers>>");
            writeStringToConsole(stream, "Chant of Reversion\r\n<<O mighty Machine, be as your fabricator-father has once made you>>\r\n");
            writeStringToConsole(stream, "Psalm of the Guilty\r\n<<Blessed Machine, forgive me the sins I commited in your almighty presence>>\r\n\r\n");
        }

        static public void InitializeMenu(NetworkStream stream) { 
        writeStringToConsole(stream, "Planet: Ridyria Secundus.\r\n");
        writeStringToConsole(stream, "Date: 493.992.M41.\r\n");
        writeStringToConsole(stream, "Void Shield Generator Control Chapel.\r\n");
        writeStringToConsole(stream, "Message from Magos lexmechanicus Saturninus:\r\nA fleet has emerged into the realspace a few hours ago.\r\nLack of communication indicates The Great Enemy. Condition of my\r\n");
        writeStringToConsole(stream, "flesh once more has proven the superiority of mechanical parts I installed\r\nin myself and forced me into medicae. I cannot join you here to TURN ON the VOID\r\n");
        writeStringToConsole(stream, "SHIELDS above the capital.\r\nIt is cruicial that you do it in my stead. Talk to the Spirit of the \r\nvoid shield generator and convince it to turn itself on. Remember about \r\n");
        writeStringToConsole(stream, "the Prayers!\r\n\r\n");
        writeStringToConsole(stream, "PS To consult the Manual type /m. Any other command will be treated by \r\nthe Spirit as a communication attempt. Caution! This Machine Spirit is \r\nvery case sensitive! It gets really moody if you do not USE APPROPRIATE PRAYERS \r\nBEFORE COMMANDS.\r\n");
        writeStringToConsole(stream, "May the Omnissiah guide you or this planet will be doomed.\r\n");
        writeStringToConsole(stream, "\r\n");
        writeStringToConsole(stream, "Loading interface...\r\n\r\nInterface loaded.\r\n==Void Shield Generator \"Glimmer\" Pattern== \r\n ");
        }
        /// <summary>
        /// metoda wypisująca podany tekst na ekran klienta
        /// </summary>
        /// <param name="stream">strumień klienta</param>
        /// <param name="s">co wypisać</param>
        /// 
        static public void writeStringToConsole(NetworkStream stream, string s)
        {
            byte[] buffer = new byte[1024]; //bufor
            buffer = Encoding.ASCII.GetBytes(s); //przerzucenie stringa do bufora
            stream.Write(buffer, 0, buffer.Length); //wypisanie bufora
        }
    }
}

