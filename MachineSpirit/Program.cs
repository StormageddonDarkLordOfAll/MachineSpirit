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
            //gra.TimerTicker(stream);
        }
        static void Main(string[] args)
        {
           
            ZeGameAPM gra = new ZeGameAPM(IPAddress.Parse("127.0.0.1"), 2048); //utworzenie gry
            gra.Start();

        }
    }
}
