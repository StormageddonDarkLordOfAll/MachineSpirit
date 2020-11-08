using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using ClassLibrary1;
using System.Timers;


namespace MachineSpirit
{

    class Program
    {

        public static void OnTimedEvent(object source, ElapsedEventArgs e, ZeGame gra, NetworkStream stream) //stare
        {
            gra.Tim(stream);
        }
        static void Main(string[] args)
        {
           
            ZeGameAPM gra = new ZeGameAPM(IPAddress.Parse("127.0.0.1"), 2048); //utworzenie gry
            gra.Start();
     

            /* ZeGame gra = new ZeGame(); //utworzenie gry
            TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 2048);
            listener.Start();
            int start = 0;//flaga startu gry
            TcpClient client = listener.AcceptTcpClient();
            NetworkStream stream = client.GetStream();

            Timer tim1 = new Timer(1000); //interwał timera
            tim1.Enabled = true;//uruchomienie timera
            byte[] buffer = new byte[1024];
            byte[] buffer2 = new byte[1024];
            while (true)
            {
                tim1.Elapsed += (sender, e) => OnTimedEvent(sender, e, gra, stream);
                if (start == 0)
                {
                    gra.InitializeGame(stream);
                }
                start = 1;
                stream.Read(buffer, 0, 1024);

                //wykonanie gry
                gra.Proceed(buffer,stream); 


            }*/

        }
    }
}
