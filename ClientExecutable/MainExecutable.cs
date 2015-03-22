using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace ClientExecutable
{
    class MainExecutable
    {
        static bool isLoggedIn;
        static string usernameCredential = "";
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine(" - Connecting to Main Server -");
                Console.WriteLine("Please enter the server IP Address");
                var ipAd = IPAddress.Parse("192.168.0.3");
                var inputIp = Console.ReadLine();
                try
                {
                    Console.Clear();
                    ipAd = IPAddress.Parse(inputIp);
                } catch
                {
                    Console.WriteLine(
                        "Sorry, but the IP Address you provided does not work.\nDefault selected: {0}\n\nPress any key to continue",
                        ipAd);
                    Console.ReadKey(true);
                    Console.Clear();
                }
                while (true)
                {
                    Console.WriteLine("What would you like to do?");

                    String option = Console.ReadLine();
                    if (option == "LOGIN" && !isLoggedIn)
                    {
                        LoginToServer(ipAd);
                    }
                    else if (option == "CHAT" && isLoggedIn)
                    {
                        InitiateChat(ipAd);
                    }
                    Console.Clear();

                }
            } catch (Exception e)
            {
                Console.WriteLine("Error..... " + e.StackTrace);
            }
        }

        /// <summary>
        /// Initiates the chat.
        /// </summary>
        /// <param name="ipAddress">The ip address.</param>
        static void InitiateChat(IPAddress ipAddress)
        {
            Console.Clear();
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(ipAddress, 8001);
            Stream stm = tcpClient.GetStream();

            ASCIIEncoding asen = new ASCIIEncoding();
            byte[] ba = asen.GetBytes(string.Format("ENTERCHAT:{0}", usernameCredential));

            stm.Write(ba, 0, ba.Length);

            byte[] bb = new byte[100];
            int k = stm.Read(bb, 0, 100);

            //TODO: Make this an own method, simply call and return the given value
            char[] receivedInput = new char[k];
            for (int i = 0; i < k; i++)
                receivedInput[i] = (Convert.ToChar(bb[i]));

            if (new String(receivedInput) == "JoinedChat")
            {
                Console.WriteLine("You have successfully joined the chat! Press any key to continue");
                Console.ReadKey();
                ActivateChat(ipAddress,stm);
                return;
            }
            else
            {
                //TODO: Do code if error comes up when trying to join chat
                return;
            }
        }

        private static void ActivateChat(IPAddress ipAddress, Stream stm)
        {
            //TODO: GUI of the program
            Console.Clear();
            Console.WriteLine("Welcome to Chat!\n");

            ASCIIEncoding asen = new ASCIIEncoding();
            byte[] ba = asen.GetBytes("RECEIVECHATNUMBER");

            stm.Write(ba, 0, ba.Length);
            Console.WriteLine("Sent ReceiveChatNumber");

            byte[] bb = new byte[100];
            int k = stm.Read(bb, 0, 100);

            char[] tempChatLines = new char[k];

            for (int i = 0; i < k; i++)
                tempChatLines[i] = (Convert.ToChar(bb[i]));
            Console.WriteLine("Number is {0}",new String(tempChatLines));

            //TODO: Check for exception thrown 
            string optionString = new string(tempChatLines);
            int numberOfChatLines = int.Parse(optionString);

            Console.WriteLine("Successfully turned CHAR ARRAY to {0}",numberOfChatLines);
            ba = asen.GetBytes("RECEIVEPREVIOUSCHAT");
            stm.Write(ba, 0, ba.Length);

            string[] chatLines = new string[numberOfChatLines - 1];
            for (int i = 0; i < numberOfChatLines; i++)
            {
                tempChatLines = new char[k];
                bb = new byte[10000];
                k = stm.Read(bb, 0, 10000);
                for (int r = 0; r < k; r++)
                {
                    tempChatLines[k] = (Convert.ToChar(bb[k]));
                }
                string currentChatString = new string(tempChatLines);
                chatLines[i] = currentChatString;

                ba = asen.GetBytes("RECEIVEDPREVIOUSCHAT");
                stm.Write(ba, 0, ba.Length);
            }

            foreach (var chatLine in chatLines)
            {
                Console.WriteLine("> {0}", chatLine);
            }
            //TODO: Do the actual logic
            while (true)
            {

            }
        }

        /// <summary>
        /// Logins to the main server.
        /// </summary>
        /// <param name="ipAddress">The ip address of the server.</param>
        static void LoginToServer(IPAddress ipAddress)
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(ipAddress, 8001);
            Stream stm = tcpClient.GetStream();

            ASCIIEncoding asen = new ASCIIEncoding();
            byte[] ba = asen.GetBytes("LOGIN");

            stm.Write(ba, 0, ba.Length);

            byte[] bb = new byte[100];
            int k = stm.Read(bb, 0, 100);

            //TODO: Make this an own method, simply call and return the given value
            char[] receivedInput = new char[k];
            for (int i = 0; i < k; i++)
                receivedInput[i] = (Convert.ToChar(bb[i]));

            if (new string(receivedInput) != "LoginAccepted")
            {
                Console.WriteLine("Sorry, but you are unable to make an account at this time");
                tcpClient.Close();
                return;
            }
            Console.WriteLine("What is your username?");
            string username = Console.ReadLine();
            Console.WriteLine("What is your password?");
            string password = Console.ReadLine();

            ba = asen.GetBytes(username);
            stm.Write(ba, 0, ba.Length);

            byte[] b2a = asen.GetBytes(password);
            stm.Write(b2a, 0, b2a.Length);

            bb = new byte[100];
            k = stm.Read(bb, 0, 100);

            receivedInput = new char[k];
            for (int i = 0; i < k; i++)
                receivedInput[i] = (Convert.ToChar(bb[i]));

            if (new string(receivedInput) == "WrongCredentials")
            {
                Console.WriteLine("Sorry, but the details you provided were not correct");
            }
            else
            {
                Console.WriteLine("LOGIN SUCESSFULL");
                isLoggedIn = true;
                usernameCredential = username;
            }
            Console.ReadKey(true);
        }
    }
}
