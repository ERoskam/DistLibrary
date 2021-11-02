using System;
using System.Text;
using System.Text.Json;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
using LibData;

namespace BookHelper
{
    // Note: Do not change this class.
    public class Setting
    {
        public int ServerPortNumber { get; set; }
        public int BookHelperPortNumber { get; set; }
        public int UserHelperPortNumber { get; set; }
        public string ServerIPAddress { get; set; }
        public string BookHelperIPAddress { get; set; }
        public string UserHelperIPAddress { get; set; }
        public int ServerListeningQueue { get; set; }
    }

    // Note: Complete the implementation of this class. You can adjust the structure of this class.
    public class SequentialHelper
    {
        public SequentialHelper()
        {
            //todo: implement the body. Add extra fields and methods to the class if needed
        }

        public void start()
        {
            //todo: implement the body. Add extra fields and methods to the class if needed
            string Configcontent = File.ReadAllText(@"../../../../ClientServerConfig.json");
            var settings = JsonSerializer.Deserialize<Setting>(Configcontent);

            //todo: implement the body. Add extra fields and methods to the class if it is needed

            int maxBuffSize = 1000;

            byte[] buffer = new byte[maxBuffSize];
            byte[] msg = Encoding.ASCII.GetBytes("From server: Your message delivered\n");
            string data = null;


            IPAddress ipAddress = IPAddress.Parse(settings.BookHelperIPAddress);

            IPEndPoint localEndpoint = new IPEndPoint(ipAddress, settings.BookHelperPortNumber);

            while (true)
            {
                Socket sock = new Socket(AddressFamily.InterNetwork,
                                        SocketType.Stream, ProtocolType.Tcp);


                sock.Bind(localEndpoint);
                sock.Listen(settings.ServerListeningQueue);
                Console.WriteLine("\n Waiting for clients..");
                Socket newSock = sock.Accept();

                while (true)
                {
                    int b = newSock.Receive(buffer);
                    data = Encoding.ASCII.GetString(buffer, 0, b);
                    Message Msg = JsonSerializer.Deserialize<Message>(data);

                    //actions

                }

            }

        }
    }
}
