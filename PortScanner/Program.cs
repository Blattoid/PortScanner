using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Xml.Linq;
using System.Linq;

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
        public static List<int> openports = new List<int>(new int[] { });
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;

            //program variable initialisation
            string address = "";
            bool interactiveMode = false;
            XDocument doc = XDocument.Load("PortXMLData.xml"); //load port assignment data

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
                    //make a new connection for every port
                    TcpClient tcp = new TcpClient();

                    //attempt connect to port. if after 1 second it doesn't work, declare the port as closed.
                    Console.Write("Trying port " + port);
                    var result = tcp.BeginConnect(address, port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(0.1));

                    if (success)
                    {
                        //report an open port and keep track of it.
                        Console.CursorLeft = 0;
                        Console.WriteLine("Port " + port + " is open!");
                        openports.Add(port);
                        tcp.EndConnect(result);
                    }

                    Console.CursorLeft = 0;
                    port++; //move on to the next port.
                }
                catch (Exception err)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error: " + err.Message);
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }

            //port identification
            if (openports.Count > 0)
            {
                Console.WriteLine("Open ports:");
                foreach (int port in openports)
                {
                    var addresses = from addr in doc.Root.Elements("record")
                                    where addr.Element("port").Value.Contains(port.ToString())
                                    select addr.Value;
                    Console.WriteLine(port+": "+addresses.ElementAt(0).Replace(port.ToString(), ""));
                }
            }
            else { Console.WriteLine("No open ports were found."); }

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