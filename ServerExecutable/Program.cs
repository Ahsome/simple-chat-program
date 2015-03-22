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
    class Program
    {
        private static List<string> loggedChat = new List<string>(); 
        static void Main(string[] args)
        {
            Console.WriteLine("Current IP?");
            var ipAd = IPAddress.Parse("192.168.0.3");
            var inputIp = Console.ReadLine();
            try
            {
                ipAd = IPAddress.Parse(inputIp);
            } catch
            {
                // ignored
            }
            Console.WriteLine("Successfully Connected to LAN:\nCurrent IP: {0}\n\nProcessing Incoming sockets...", ipAd);

            try
            {
                // use local m/c IP address, and 
                // use the same in the client

                /* Initializes the Listener */
                TcpListener myList = new TcpListener(ipAd, 8001);

                /* Start Listening at the specified port */
                while (true)
                {
                    myList.Start();
                    string clientInput = "";
                    Socket s = myList.AcceptSocket();
                    Thread loginThread = new Thread(() => LoginCode(s));
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

        static void ControlChat(Socket s, string clientInput)
        {
            ASCIIEncoding asen = new ASCIIEncoding();

            string[] clientInputSeparated = clientInput.Split(':');
            loggedChat.Add(clientInputSeparated[1]);

            s.Send(asen.GetBytes("JoinedChat"));
            Console.WriteLine("User {0} has successfully joined the chat", clientInputSeparated[1]);
        }

        private static void LoginCode(Socket s)
        {
            ASCIIEncoding asen = new ASCIIEncoding();
            s.Send(asen.GetBytes("LoginAccepted"));
            Console.WriteLine("A user is attempting to login into the system...");

            byte[] usernameBytes = new byte[100];
            byte[] passwordBytes = new byte[100];

            int usernameLength = s.Receive(usernameBytes);
            int passwordLength = s.Receive(passwordBytes);

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
                s.Send(asen.GetBytes("WrongCredentials"));
                return;
            }
            s.Send(asen.GetBytes("AcceptedCredentials"));

            Console.WriteLine("Account: {0} has successfully logged in. Password is {1}", usernameString, passwordString);

            //var password = usernameDatabase.Elements("account")
            //    .Elements("password")
            //    .Select(stuff => stuff.Value);


            //string passwordInput = usernameDatabase.Elements("password").FirstOrDefault().Value;
            //string usernameInput = "";
            //foreach (var element in usernameDatabase)
            //{
            //    usernameInput = (string)element;
            //}

            //if (new String(usernameChar) != usernameInput || new String(passwordChar) != passwordInput)
            //{
            //    Console.WriteLine(@"Wrong Username and\or password given. Kicked from system");
            //    return;
            //}
        }
    }
}
