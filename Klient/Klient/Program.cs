using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Klient
{
    class Program
    {
        static void Main(string[] args)
        {

            byte[] message = new byte[1024];
            TcpClient client = new TcpClient();
            client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2048));
            NetworkStream stream = client.GetStream();
            string line;
            string converted;

            Console.ForegroundColor = ConsoleColor.Green;
            while (true)
            {
                    stream.Read(message, 0, message.Length);
                    converted = Encoding.ASCII.GetString(message, 0, message.Length);
                    Console.Write(converted);
                    //if ((line = Console.ReadLine()) != null)

                if (stream.DataAvailable == false)
                {
                    Console.Write("\n");
                    line = Console.ReadLine();
                    message = new ASCIIEncoding().GetBytes(line);
                    client.GetStream().Write(message, 0, message.Length);
                    Array.Clear(message, 0, message.Length);
                    converted = "";
                }
            }
        }
    }
}
