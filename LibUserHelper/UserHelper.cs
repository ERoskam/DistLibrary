using System;
using System.Text;
using System.Text.Json;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
using LibData;

namespace UserHelper
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
            string Configcontent = File.ReadAllText(@"../ClientServerConfig.json");
            var settings = JsonSerializer.Deserialize<Setting>(Configcontent);

            int maxBuffSize = 1000;

            byte[] buffer = new byte[maxBuffSize];
            byte[] msg = Encoding.ASCII.GetBytes("From server: Your message delivered\n");
            string data = null;

            IPAddress ipAddress = IPAddress.Parse(settings.UserHelperIPAddress);
            IPEndPoint localEndpoint = new IPEndPoint(ipAddress, settings.UserHelperPortNumber);

            Socket sock = new Socket(AddressFamily.InterNetwork,
                        SocketType.Stream, ProtocolType.Tcp);

            sock.Bind(localEndpoint);

            var bonk = true;

            sock.Listen(settings.ServerListeningQueue);

            while (bonk)
            {
                Console.WriteLine("\n Waiting for clients..");
                Socket newSock = sock.Accept();

                while (IsConnected(newSock))
                {
                    int b = newSock.Receive(buffer);
                    data = Encoding.ASCII.GetString(buffer, 0, b);
                    Message Msg = JsonSerializer.Deserialize<Message>(data);

                    if (Msg.Type.Equals(MessageType.EndCommunication))
                    {
                        newSock.Disconnect(false);
                        bonk = false;
                    }
                    else
                    {
                        newSock.Send(AssembleUserInqReply(Msg));
                    }
                    break;
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

        public byte[] AssembleUserInqReply(Message Msg)
        {
            string userJson = File.ReadAllText(@"Users.json");
            UserData[] users = JsonSerializer.Deserialize<UserData[]>(userJson);
            UserData foundUser = null;

            foreach (UserData user in users)
            {
                if (user.User_id.Equals(Msg.Content))
                {
                    foundUser = user;
                }
            }

            Message replyJsonData = null;

            if (foundUser != null)
            {
                replyJsonData = new Message
                {
                    Type = MessageType.UserInquiryReply,
                    Content = JsonSerializer.Serialize<UserData>(foundUser)
                };
            }
            else
            {
                replyJsonData = new Message
                {
                    Type = MessageType.NotFound,
                    Content = Msg.Content
                };
            }

            string msg = JsonSerializer.Serialize(replyJsonData);
            byte[] msgNew = Encoding.ASCII.GetBytes(msg);

            return msgNew;
        }
    }
}
