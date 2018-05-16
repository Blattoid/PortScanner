using System;
using System.Text.RegularExpressions;
using System.Net.Sockets;

namespace PortScanner
{
    class Program
    {
        public static void parsePortRange(string input)
        {                 
            //get port range
            try
            {
                port = Convert.ToInt16(Regex.Split(input, ":")[0]);
                maxport = Convert.ToInt16(Regex.Split(input, ":")[1]);

                //idiotproofing
                if (port > maxport) { throw new ArgumentException("Start port is bigger than end port."); }
                if (port < 1) { throw new ArgumentException("Start port is smaller than 1."); }
                if (port > 65536) { throw new ArgumentException("Start port is bigger than 65535."); }
                if (maxport > 65536) { throw new ArgumentException("End port is bigger than 65535."); }
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public static int port = 1;
        public static int maxport = 65535;
        static void Main(string[] args)
        {
            //program variable initialisation
            string address = "";
            bool interactiveMode = false;

            if (args.Length > 0)
            { address = args[0]; } //get address to target from command line
            if (args.Length > 1)
            {
                try
                {
                    parsePortRange(args[1]);
                }
                catch (Exception err)
                {
                    Console.WriteLine("Error parsing port range: " + err.Message);
                    goto end;
                }
            }

            if (args.Length == 0)
            {
                interactiveMode = true; //since no parameters were passed, it it reasonable to assume that the program may not have been launched from the command prompt. Thus, interactive mode.
                //if none is specified
                Console.Write("Usage: " + AppDomain.CurrentDomain.FriendlyName + " <address> [startport:endport]\nEnter address to target: ");
                address = Console.ReadLine();

                //port range
                Console.Write("Enter port range (leave blank for none): ");
                string range = Console.ReadLine();
                if (range != "")
                {
                    try
                    {
                        parsePortRange(range);
                    }
                    catch (Exception err)
                    {
                        Console.WriteLine("Error parsing port range: " + err.Message);
                        goto end;
                    }
                }
            }

            while (port <= maxport)
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

            end: //label for teleportation to the end. kill the ender dragon.
            //if in interactive mode, wait for keypress before exit.
            if (interactiveMode)
            {
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
            }
        }
    }
}