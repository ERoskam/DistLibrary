using System;
using System.Text;
using System.Text.Json;
using System.Net;
using System.Net.Sockets;
using System.IO;
using LibData;


namespace LibServer
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
    public class SequentialServer
    {
        public SequentialServer()
        {
            //todo: implement the body. Add extra fields and methods to the class if it is needed
        }

        public void start()
        {
            string Configcontent = File.ReadAllText(@"../../../../ClientServerConfig.json");
            var settings = JsonSerializer.Deserialize<Setting>(Configcontent);

            //todo: implement the body. Add extra fields and methods to the class if it is needed

            int maxBuffSize = 1000;

            byte[] buffer = new byte[maxBuffSize];
            byte[] msg = Encoding.ASCII.GetBytes("From server: Your message delivered\n");
            string data = null;


            IPAddress ipAddress = IPAddress.Parse(settings.ServerIPAddress);

            IPEndPoint localEndpoint = new IPEndPoint(ipAddress, settings.ServerPortNumber);

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

                    switch (Msg.Type)
                    {
                        case MessageType.Hello:
                            newSock.Send(AssembleMsgWelcome());
                            break;

                        case MessageType.EndCommunication:
                            newSock.Close();
                            Console.WriteLine("Closing the socket..");
                            sock.Close();
                            break;

                        case MessageType.BookInquiry:

                            break;

                        default:
                            Console.WriteLine("" + data);
                            data = null;
                            newSock.Send(msg);
                            break;
                    }
                    break;

                }

            }

        }

        public byte[] AssembleMsgWelcome()
        {
            Message replyJsonData = new Message
            {
                Type = MessageType.Welcome,
                Content = "I think you're really cute"
            };

            string msg = JsonSerializer.Serialize(replyJsonData);
            byte[] msgNew = Encoding.ASCII.GetBytes(msg);

            return msgNew;
        }
    }

}



