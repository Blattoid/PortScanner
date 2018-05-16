using System;
using System.Net.Sockets;

namespace PortScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            //program variable initialisation
            string address;
            int port = 1;
            bool interactiveMode = false;

            if (args.Length != 0)
            { address = args[0]; } //get address to target from command line
            else
            {
                interactiveMode = true; //since no parameters were passed, it it reasonable to assume that the program may not have been launched from the command prompt. Thus, interactive mode.
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

            //if in interactive mode, wait for keypress before exit.
            if (interactiveMode)
            {
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
            }
        }
    }
}
