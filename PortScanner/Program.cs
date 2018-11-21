using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Xml.Linq;
using System.Linq;
using System.Reflection;
using System.IO;

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

        //https://stackoverflow.com/questions/8946808/can-console-clear-be-used-to-only-clear-a-line-instead-of-whole-console
        public static void ClearCurrentLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;

            //program variable initialisation
            string address = "";
            bool interactiveMode = false;

            //load port assignment data from executable folder.
            XDocument PortData = XDocument.Load(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\PortXMLData.xml");

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

                    //calculate percentage and output a progress bar
                    string percentage = (((double)port/maxport)*100).ToString("#.00")+"%";
                    Console.Write(ProgressBars.GenerateBar(port, maxport,
                                                           barprefix: "Trying port "+port+" [",
                                                           barsuffix: "] "+percentage,
                                                           barwidth: Console.WindowWidth
                                                           )
                                  );

                    //attempt connect to port. if after 1 second it doesn't work, declare the port as closed.
                    var result = tcp.BeginConnect(address, port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(0.1));

                    if (success)
                    {
                        //report an open port and keep track of it.
                        ClearCurrentLine();
                        Console.WriteLine("Port " + port + " is open!");
                        openports.Add(port);
                        tcp.EndConnect(result);
                    }

                    ClearCurrentLine();
                    port++; //move on to the next port.
                }
                catch (Exception err)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error: " + err.Message);
                    Console.ForegroundColor = ConsoleColor.White;
                    port++; //skip that port
                }
            }

            //port identification
            if (openports.Count > 0)
            {
                ClearCurrentLine();
                Console.WriteLine("\nOpen ports:");
                foreach (int port in openports)
                {
                    var addresses = from addr in PortData.Root.Elements("record")
                                    where addr.Element("port").Value.Contains(port.ToString())
                                    select addr.Value;

                    //error handling catches unknown ports. if an entry exists then display port name.
                    try { Console.WriteLine(port + ": " + addresses.ElementAt(0).Replace(port.ToString(), "")); }
                    catch (ArgumentOutOfRangeException) { Console.WriteLine(port + ": UNKNOWN"); }
                }
            }
            else { Console.WriteLine("\nNo open ports were found."); }

            end: //label for teleportation to the end. kill the ender dragon.
            //if in interactive mode, wait for keypress before exit.
            if (interactiveMode)
            {
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
            }
        }
    }
    
    
    
    //https://stackoverflow.com/questions/14353485/how-do-i-map-numbers-in-c-sharp-like-with-map-in-arduino
    public static class ExtensionMethods
    {
        public static decimal Map(this decimal value, decimal fromSource, decimal toSource, decimal fromTarget, decimal toTarget)
        {
            return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
        }
    }
    class ProgressBars
    {
        //This was converted by hand from Python code which does the same thing
        //(I made this aforementioned Python code)

        public static string GenerateBar(int value, int maxvalue,
                      string barprefix = "[", string filledchar = "=", string pointerchar = ">",
                      string emptychar = " ", string barsuffix = "]", int barwidth = 50)
        {

            //sanity checks to make sure nothing is going to break anything
            if (value > maxvalue) throw new Exception("Current value is bigger than max value!");
            if (value < 0) throw new Exception("Current value is smaller than 0!");

            //calculate how much space the non-changing parts of the bar will take up
            int length = (
                barprefix.Length +
                pointerchar.Length +
                barsuffix.Length +
                2 //necesscary padding to prevent cursor going onto next line if barwidth = width of consoles
            );

            //If that length is smaller than the barwidth, then there will literally be no space left for the bar to move. Oh well.
            //adjust the bar to fit the space it has, using the length we just calculated
            value = (int)ExtensionMethods.Map(value, 0, maxvalue, 0, barwidth - length);
            maxvalue = barwidth - length;

            //construct the bar as a string
            string bar = barprefix; //start with the bar prefix
            bar += String.Concat(Enumerable.Repeat(filledchar, value)); //the currently filled portion of the bar
            bar += pointerchar;
            bar += String.Concat(Enumerable.Repeat(emptychar, maxvalue - value)); //the empty portion of the bar
            bar += barsuffix + " "; //separator

            return bar; //send the finished thing back
        }
    }
}