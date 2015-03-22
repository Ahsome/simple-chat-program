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
    class Program
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
                        InitiateLogin(ipAd);
                    }
                    if (option == "CHAT" && isLoggedIn)
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
                Console.WriteLine("You have successfully joined the chat!");
                Console.ReadKey();
                return;
            }
            else
            {
                //TODO: Do code if error comes up when trying to join chat
                return;
            }
        }

        static void InitiateLogin(IPAddress inpAddress)
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(inpAddress, 8001);
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
