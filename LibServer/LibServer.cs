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
            string Configcontent = File.ReadAllText(@"../ClientServerConfig.json");
            var settings = JsonSerializer.Deserialize<Setting>(Configcontent);

            //todo: implement the body. Add extra fields and methods to the class if it is needed

            int maxBuffSize = 1000;

            byte[] buffer = new byte[maxBuffSize];
            byte[] msg = Encoding.ASCII.GetBytes("From server: Your message delivered\n");
            string data = null;
            Message reply = null;


            IPAddress ipAddress = IPAddress.Parse(settings.ServerIPAddress);

            IPEndPoint localEndpoint = new IPEndPoint(ipAddress, settings.ServerPortNumber);

            Socket sock = new Socket(AddressFamily.InterNetwork,
                        SocketType.Stream, ProtocolType.Tcp);


            sock.Bind(localEndpoint);
            bool bonk = true;

            sock.Listen(settings.ServerListeningQueue);

            while (bonk)
            {
                Console.WriteLine("\n Waiting for clients..");
                Socket newSock = sock.Accept();

                while (IsConnected(newSock))
                {
                    int b = newSock.Receive(buffer);
                    data = Encoding.ASCII.GetString(buffer, 0, b);
                    Console.Write(data);

                    Message Msg;

                    if (data != "")
                    {
                        Msg = JsonSerializer.Deserialize<Message>(data);
                    }
                    else continue;

                    switch (Msg.Type)
                    {
                        case MessageType.Hello:
                            newSock.Send(AssembleMsgWelcome());
                            break;

                        case MessageType.EndCommunication:
                            Console.WriteLine("Closing the socket and helper sockets");
                            BookHelperConnect(Msg);
                            UserHelperConnect(Msg);
                            newSock.Shutdown(SocketShutdown.Both);
                            bonk = false;
                            break;  

                        case MessageType.BookInquiry:
                            reply = BookHelperConnect(Msg);
                            Console.WriteLine(JsonSerializer.Serialize<Message>(reply));
                            newSock.Send(Encoding.ASCII.GetBytes(JsonSerializer.Serialize<Message>(reply)));
                            break;

                        case MessageType.UserInquiry:
                            reply = UserHelperConnect(Msg);
                            Console.WriteLine(JsonSerializer.Serialize<Message>(reply));
                            newSock.Send(Encoding.ASCII.GetBytes(JsonSerializer.Serialize<Message>(reply)));
                            break;

                        default:
                            Console.WriteLine("" + data);
                            data = null;
                            newSock.Send(msg);
                            break;
                    }

                }

            }

        }

        public bool IsConnected(Socket socket)
        {
            try
            {
                return !(socket.Poll(1000, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException)
            {
                return false;
            }
        }

        public Message BookHelperConnect(Message BookMsg) 
        {
            string Configcontent = File.ReadAllText(@"../ClientServerConfig.json");
            var settings = JsonSerializer.Deserialize<Setting>(Configcontent);

            var BookMsgJson = JsonSerializer.Serialize<Message>(BookMsg);
            byte[] BookData = Encoding.ASCII.GetBytes(BookMsgJson);

            int maxBuffSize = 1000;

            byte[] buffer = new byte[maxBuffSize];
            byte[] msg = new byte[maxBuffSize];
            string data = null;
            Message Msg = null;

            IPAddress BookipAddress = IPAddress.Parse(settings.BookHelperIPAddress);
            IPEndPoint BookServerEndpoint = new IPEndPoint(BookipAddress, settings.BookHelperPortNumber);

            Socket sockBook = new Socket(AddressFamily.InterNetwork,
                                     SocketType.Stream, ProtocolType.Tcp);

            sockBook.Connect(BookServerEndpoint);
            sockBook.Send(BookData);

            int bookInqReplySize = sockBook.Receive(buffer);
            data = Encoding.ASCII.GetString(buffer, 0, bookInqReplySize);

            if (data != "")
            {
                Msg = JsonSerializer.Deserialize<Message>(data);
            }

            return Msg;
        }

        public Message UserHelperConnect(Message UserMsg)
        {
            string Configcontent = File.ReadAllText(@"../ClientServerConfig.json");
            var settings = JsonSerializer.Deserialize<Setting>(Configcontent);

            var UserMsgJson = JsonSerializer.Serialize<Message>(UserMsg);
            byte[] UserData = Encoding.ASCII.GetBytes(UserMsgJson);

            int maxBuffSize = 1000;

            byte[] buffer = new byte[maxBuffSize];
            byte[] msg = new byte[maxBuffSize];
            string data = null;
            Message Msg = null;

            IPAddress UseripAddress = IPAddress.Parse(settings.UserHelperIPAddress);
            IPEndPoint UserServerEndpoint = new IPEndPoint(UseripAddress, settings.UserHelperPortNumber);

            Socket sockUser = new Socket(AddressFamily.InterNetwork,
                                     SocketType.Stream, ProtocolType.Tcp);

            sockUser.Connect(UserServerEndpoint);
            sockUser.Send(UserData);

            int userInqReplySize = sockUser.Receive(buffer);
            data = Encoding.ASCII.GetString(buffer, 0, userInqReplySize);

            if(data != "")
            {
                Msg = JsonSerializer.Deserialize<Message>(data);
            }

            return Msg;
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