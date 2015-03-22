using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ServerExecutable
{
    class MainServer
    {
        private static List<string> membersInChat = new List<string>();
        private static List<string> isLoggedIn = new List<string>();
        private static List<string> currentChatList = new List<string>();
        private static int chatLines = 5;

        static void Main(string[] args)
        {
            //TODO: Make a way to add multiply things to list easily, without continuous ADD
            currentChatList.Add("WELCOME TO CHAT");
            currentChatList.Add("HOPE YOU LIKE YOUR STAY");
            currentChatList.Add("DEBUGGING IS A BITCH");
            chatLines = currentChatList.Count;

            Console.WriteLine("Current IP?");
            var ipAddress = IPAddress.Parse("192.168.0.3");
            var inputIp = Console.ReadLine();
            try
            {
                ipAddress = IPAddress.Parse(inputIp);
            } catch
            {
                // ignored
            }
            Console.WriteLine("Successfully Connected to LAN:\nCurrent IP: {0}\n\nProcessing Incoming sockets...", ipAddress);

            try
            {
                TcpListener myList = new TcpListener(ipAddress, 8001);
                while (true)
                {
                    myList.Start();
                    string clientInput = "";
                    Socket s = myList.AcceptSocket();
                    Thread loginThread = new Thread(() => LoginUser(s));
                    Thread chatThread = new Thread(() => ControlChat(s, clientInput));

                    byte[] b = new byte[100];
                    int k = s.Receive(b);
                    char[] test = new char[k];
                    for (int i = 0; i < k; i++)
                        test[i] = (Convert.ToChar(b[i]));
                    clientInput = new string(test);
                    if (clientInput == "LOGIN")
                    {
                        loginThread.Start();
                        continue;
                    }
                    if (clientInput.Contains("ENTERCHAT"))
                    {
                        chatThread.Start();
                        continue;
                    }

                    Console.WriteLine("\nSent Acknowledgement");
                    /* clean up */
                    s.Close();
                    loginThread.Abort();
                    myList.Stop();
                }

            } catch (Exception e)
            {
                Console.WriteLine("Error..... " + e.StackTrace);
            }
        }

        /// <summary>
        /// Adds the current user to the chat list
        /// </summary>
        /// <param name="clientSocket">The client's socket.</param>
        /// <param name="clientInput">The client's input.</param>
        private static void ControlChat(Socket clientSocket, string clientInput)
        {
            ASCIIEncoding asen = new ASCIIEncoding();

            string[] clientInputSeparated = clientInput.Split(':');
            membersInChat.Add(clientInputSeparated[1]);

            clientSocket.Send(asen.GetBytes("JoinedChat"));
            Console.WriteLine("User {0} has successfully joined the chat", clientInputSeparated[1]);

            byte[] givenInput = new byte[100];
            int k = clientSocket.Receive(givenInput);
            Console.WriteLine("Got some input");
            char[] test = new char[k];
            for (int i = 0; i < k; i++)
                test[i] = (Convert.ToChar(givenInput[i]));

            clientInput = new string(test);
            Console.WriteLine("Input was: {0}",clientInput);
            if (clientInput != "RECEIVECHATNUMBER")
            {
                Console.WriteLine("Error in accessing Chat Number");
                return;
            }

            asen = new ASCIIEncoding();
            clientSocket.Send(asen.GetBytes(chatLines.ToString()));
            Console.WriteLine("Sent the ChatLine number, {0}",chatLines.ToString());

            givenInput = new byte[100];
            k = clientSocket.Receive(givenInput);
            test = new char[k];
            for (int i = 0; i < k; i++)
                test[i] = (Convert.ToChar(givenInput[i]));

            clientInput = new string(test);
            if (clientInput != "RECEIVEPREVIOUSCHAT")
            {
                Console.WriteLine("Error in accessing Previous Chat");
                return;
            }

            foreach (var currentChatLine in currentChatList)
            {
                clientSocket.Send(asen.GetBytes(currentChatLine));
                givenInput = new byte[100];
                k = clientSocket.Receive(givenInput);
                test = new char[k];
                for (int i = 0; i < k; i++)
                    test[i] = (Convert.ToChar(givenInput[i]));

                clientInput = new string(test);
                if (clientInput == "RECEIVEDPREVIOUSCHAT")
                {
                    //TODO: Add error handling;
                    continue;
                }
            }
        }

        //private static void AddToChat(Socket clientSocket)
        //{
        //    {
        //    }
        //}

        /// <summary>
        /// Loges the user in, if credentials are accepted
        /// </summary>
        /// <param name="clientSocket">The client's socket.</param>
        private static void LoginUser(Socket clientSocket)
        {
            ASCIIEncoding asen = new ASCIIEncoding();
            clientSocket.Send(asen.GetBytes("LoginAccepted"));
            Console.WriteLine("A user is attempting to login into the system...");

            byte[] usernameBytes = new byte[100];
            byte[] passwordBytes = new byte[100];

            int usernameLength = clientSocket.Receive(usernameBytes);
            int passwordLength = clientSocket.Receive(passwordBytes);

            char[] usernameChar = new char[usernameLength];
            char[] passwordChar = new char[passwordLength];

            for (int i = 0; i < usernameLength; i++)
                usernameChar[i] = (Convert.ToChar(usernameBytes[i]));

            for (int i = 0; i < passwordLength; i++)
                passwordChar[i] = (Convert.ToChar(passwordBytes[i]));

            string usernameString = new string(usernameChar);
            string passwordString = new string(passwordChar);

            var classData = XElement.Load(@"LoginDetails.xml");
            var usernameDatabase =
                classData.Elements("account").Where(r => (string)r.Attribute("username") == usernameString);

            var passwordDatabase = usernameDatabase.Elements("password").Select(r => r.Value);
            string passwordInput = "";

            foreach (var element in passwordDatabase)
            {
                passwordInput = element;
            }

            if (passwordInput != passwordString)
            {
                Console.WriteLine("Failure attempt in logging in. Canceling login process");
                clientSocket.Send(asen.GetBytes("WrongCredentials"));
                return;
            }
            clientSocket.Send(asen.GetBytes("AcceptedCredentials"));
            isLoggedIn.Add(usernameString);

            Console.WriteLine("Account: {0} has successfully logged in. Password is {1}", usernameString, passwordString);
        }
    }
}
