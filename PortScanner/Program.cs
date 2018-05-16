using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            //program variable initialisation
            string address;
            int port = 1;

            if (args.Length != 0)
            { address = args[0]; } //get address to target from command line
            else
            {
                //if none is specified
                Console.Write("Usage: " + AppDomain.CurrentDomain.FriendlyName + " <address>\nEnter address to target: ");
                address = Console.ReadLine();
            }

            while (port <= 65535)
            {
                try
                {
                    TcpClient tcp = new TcpClient();
                    //connect to the server 
                    Console.WriteLine("Trying port " + port);
                    tcp.Connect(address, port); //if the connection is unsuccessful, the error is triggered.

                    //otherwise, report an open port.
                    Console.WriteLine("Port " + port + " is open!");
                    tcp.Close();
                }
                catch (Exception err)
                {
                    if (!err.Message.Contains("actively refused"))
                    {
                        Console.WriteLine("Error: " + err.Message);
                        break;
                    }
                }
                port++;
            }

        }
    }
}
