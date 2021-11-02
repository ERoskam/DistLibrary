using System;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json;
using LibData;


namespace LibClient
{
    // Note: Do not change this class 
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

    // Note: Do not change this class 
    public class Output
    {
        public string Client_id { get; set; } // the id of the client that requests the book
        public string BookName { get; set; } // the name of the book to be reqyested
        public string Status { get; set; } // final status received from the server
        public string BorrowerName { get; set; } // the name of the borrower in case the status is borrowed, otherwise null
        public string BorrowerEmail { get; set; } // the email of the borrower in case the status is borrowed, otherwise null
    }

    // Note: Complete the implementation of this class. You can adjust the structure of this class.
    public class SimpleClient
    {
        // some of the fields are defined. 
        public Output result;
        public Socket clientSocket;
        public IPEndPoint serverEndPoint;
        public IPAddress ipAddress;
        public Setting settings;
        public string client_id;
        private string bookName;
        // all the required settings are provided in this file
        public string configFile = @"../ClientServerConfig.json";
        //public string configFile = @"../../../../ClientServerConfig.json"; // for debugging

        // todo: add extra fields here in case needed 

        /// <summary>
        /// Initializes the client based on the given parameters and setting file.
        /// </summary>
        /// <param name="id">id of the clients provided by the simulator</param>
        /// <param name="bookName">name of the book to be requested from the server, provided by the simulator</param>
        public SimpleClient(int id, string bookName)
        {
            //todo: extend the body if needed.
            this.bookName = bookName;
            this.client_id = "Client " + id.ToString();
            this.result = new Output();
            result.BookName = bookName;
            result.Client_id = this.client_id;
            // read JSON directly from a file
            try
            {
                string configContent = File.ReadAllText(configFile);
                this.settings = JsonSerializer.Deserialize<Setting>(configContent);
                this.ipAddress = IPAddress.Parse(settings.ServerIPAddress);
            }
            catch (Exception e)
            {
                Console.Out.WriteLine("[Client Exception] {0}", e.Message);
            }
        }

        /// <summary>
        /// Establishes the connection with the server and requests the book according to the specified protocol.
        /// Note: The signature of this method must not change.
        /// </summary>
        /// <returns>The result of the request</returns>
        public Output start()
        {

            string Configcontent = File.ReadAllText(@"../../../../ClientServerConfig.json");
            var settings = JsonSerializer.Deserialize<Setting>(Configcontent);
            // todo: implement the body to communicate with the server and requests the book. Return the result as an Output object.
            // Adding extra methods to the class is permitted. The signature of this method must not change.

            int maxBuffSize = 1000;

            byte[] buffer = new byte[maxBuffSize];
            byte[] msg = new byte[maxBuffSize];
            string data = null;

            IPAddress ipAddress = IPAddress.Parse(settings.ServerIPAddress);

            IPEndPoint serverEndpoint = new IPEndPoint(ipAddress, settings.ServerPortNumber);


            Socket sock = new Socket(AddressFamily.InterNetwork,
                                     SocketType.Stream, ProtocolType.Tcp);


            sock.Connect(serverEndpoint);

            sock.Send(AssembleMsgHello());

            int Welcomesize = sock.Receive(buffer);
            data = Encoding.ASCII.GetString(buffer, 0, Welcomesize);
            Message Msg = JsonSerializer.Deserialize<Message>(data);

            switch( Msg.Type ) 
            {
                case MessageType.Welcome:
                    if (this.client_id.Equals("Client -1")) {
                        sock.Send(AssembleMsgEndCommunication());
                    }
                    else {
                        sock.Send(AssembleMsgBookInquiry());                   
                    }
                    break;
            }

            return result;
        }

/*        data = Encoding.ASCII.GetString(buffer, 0, bookInquiryReplyMsgInt);
        Message bookInquiryMsg = JsonSerializer.Deserialize<Message>(data);
        BookData recievedBookData = JsonSerializer.Deserialize<BookData>(bookInquiryMsg.Content);*/

        public byte[] AssembleMsgHello()
        {
            Message replyJsonData = new Message
            {
                Type = MessageType.Hello,
                Content = this.client_id
            };

            string msg = JsonSerializer.Serialize(replyJsonData);
            byte[] msgNew = Encoding.ASCII.GetBytes(msg);

            return msgNew;
        }

        public byte[] AssembleMsgEndCommunication()
        {
            Message replyJsonData = new Message
            {
                Type = MessageType.EndCommunication,
                Content = "Content filler"
            };

            string msg = JsonSerializer.Serialize(replyJsonData);
            byte[] msgNew = Encoding.ASCII.GetBytes(msg);

            return msgNew;
        }

        public byte[] AssembleMsgBookInquiry()
        {
            Message replyJsonData = new Message
            {
                Type = MessageType.BookInquiry,
                Content = this.bookName
            };

            string msg = JsonSerializer.Serialize(replyJsonData);
            byte[] msgNew = Encoding.ASCII.GetBytes(msg);

            return msgNew;
        }

        /*public byte[] AssembleMsgUserInquiry()
        {
            Message replyJsonData = new Message
            {
                Type = MessageType.UserInquiry,
                Content = recievedBookData.BorrowedBy
            };

            string msg = JsonSerializer.Serialize(replyJsonData);
            byte[] msgNew = Encoding.ASCII.GetBytes(msg);

            return msgNew;
        }

        public byte[] AssembleMsgError()
        {
            Message replyJsonData = new Message
            {
                Type = MessageType.Error,
                Content = "Error occured somewhere"
            };

            string msg = JsonSerializer.Serialize(replyJsonData);
            byte[] msgNew = Encoding.ASCII.GetBytes(msg);

            return msgNew;
        }*/
    }
}
