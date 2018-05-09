using System;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Xml.Linq;
using System.IO;
using System.Xml;
using System.Threading;

namespace ChatServer
{
    public class ClientObject
    {
        protected internal string ClientName { get; private set; }//его принимаем за уникальное имя
        protected internal NetworkStream Stream { get; private set; }
        string CompanionName;
        string ip;
        string HistoryPath = "";
        TcpClient client { get; }
        Server server; // сервер

        public ClientObject(TcpClient tcpClient, Server serverObject, string ClientName, string ip)
        {
            this.ClientName = ClientName;
            client = tcpClient;
            server = serverObject;
            serverObject.AddConnection(this);
            this.ip = ip;
        }

        void SendHistory()
        {
            
            if (Directory.Exists(Directory.GetCurrentDirectory() + @"\ServerData\" + ClientName + "_" + CompanionName))
            {
                HistoryPath = Directory.GetCurrentDirectory() + @"\ServerData\" + ClientName + "_" + CompanionName;
            }
            else if (Directory.Exists(Directory.GetCurrentDirectory() + @"\ServerData\" + CompanionName + "_" + ClientName))
            {
                HistoryPath = Directory.GetCurrentDirectory() + @"\ServerData\" + CompanionName + "_" + ClientName;
            }
            else
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\ServerData\" + ClientName + "_" + CompanionName);
                HistoryPath = Directory.GetCurrentDirectory() + @"\ServerData\" + ClientName + "_" + CompanionName;
                //создать директорию
            }
            try
            {
                var xDoc = XDocument.Load(HistoryPath + @"\ChatHistory.xml");
                server.SendMessageById("!ChatHistory:" + xDoc.ToString(), ClientName);
            }
            catch
            {
                XmlDocument document = new XmlDocument();
                document.CreateXmlDeclaration("1.0", "utf-8", null);
                XmlNode root = document.CreateElement("chathistory");
                document.AppendChild(root);
                document.Save(HistoryPath + @"\ChatHistory.xml");
                server.SendMessageById("!ChatHistory:null", ClientName);
            }
        }

        void FileTransfering()
        {
            server.SendMessageById("!file", CompanionName);
            server.SendMessageById("!acceptfilelisten", ClientName);
            string fullPath = FileProcessing.Receive(HistoryPath);
            string ip;
            if (fullPath != "error")
            {
                List<string> paths = new List<string>();
                paths.Add(fullPath);
                foreach (ClientObject clientObj in server.GetClients)
                {
                    if (CompanionName == clientObj.ClientName)
                    {
                        ip = clientObj.ip;
                        FileProcessing.Send(paths, ip);
                    }
                }
            }
            else { server.SendMessageById("!errorfilesending", CompanionName); }
        }

        public void Process()
        {
            Console.WriteLine(ClientName + " logged in");
            //server.BroadcastMessage("!ip", ClientName);
            bool Locker = true;
            List<string> InstructionArray;
            while (Locker)
            {
                try
                {
                    //Console.WriteLine("In Client try");
                    Stream = client.GetStream();
                    string message = GetMessage();
                    InstructionArray = CommandTranslator.Parse(message);
                    Console.WriteLine(message);

                    switch (InstructionArray[0])
                    {
                        case "!newchat":// если клиент захотел создать чат(он прислал имя Челика с которым хочет чатиться)
                            foreach (ClientObject clientObj in server.GetClients)
                            {
                                if (InstructionArray[1] == clientObj.ClientName) server.SendMessage("!newchat?:" + ClientName, clientObj.client);//**либо переделать отправку
                            }
                            string answer = GetMessage();
                            if (answer == "!yes")// если согласен
                            {
                                CompanionName = InstructionArray[1];

                                SendHistory();
                                //Console.WriteLine("History sended to " + ClientName);
                                Console.WriteLine(HistoryPath);

                                while (true)
                                {
                                    try
                                    {
                                        message = GetMessage();
                                        if (message == "!exitchat")
                                        {
                                            server.SendMessageById(message, CompanionName);
                                            CompanionName = null;
                                            break;
                                        }
                                        else if (message == "!exitchataccept")
                                        {
                                            CompanionName = null;
                                            break;
                                        }
                                        else if (message == "!file")
                                        {
                                            Thread fileThread = new Thread(FileTransfering);
                                            fileThread.Start();
                                        }
                                        XmlProcessing.WriteXML(HistoryPath, message, ClientName, DateTime.Now.ToString());
                                        //Console.WriteLine("Wrote message from " + ClientName);
                                        server.SendMessageById(message, CompanionName);
                                    }
                                    catch
                                    {
                                        message = "!exitchat";
                                        server.SendMessageById(message, CompanionName);
                                        CompanionName = null;
                                        Locker = false;
                                        server.RemoveConnection(ClientName);
                                        Close();
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                CompanionName = null;
                            }
                            break;
                        case "!yes"://либо здесь обрабатывать ответ на вопрос о чате
                            foreach (ClientObject clientObj in server.GetClients)// ищем в онлайне этого челика и отправляем ему запрос
                            {
                                if (InstructionArray[1] == clientObj.ClientName)
                                {
                                    server.SendMessage("!yes", clientObj.client);
                                    //Console.WriteLine("Sended yes from " + ClientName + " to " + clientObj.ClientName);
                                }
                            }
                            CompanionName = InstructionArray[1];
                            Thread.Sleep(100);
                            SendHistory();
                            //Console.WriteLine("History sended to " + ClientName);

                            while (true)
                            {
                                try
                                {
                                    message = GetMessage();
                                    if (message == "!exitchat")
                                    {
                                        server.SendMessageById(message, CompanionName);
                                        CompanionName = null;
                                        break;
                                    }
                                    else if (message == "!exitchataccept")
                                    {
                                        CompanionName = null;
                                        break;
                                    }
                                    else if (message == "!file")
                                    {
                                        Thread fileThread = new Thread(FileTransfering);
                                        fileThread.Start();
                                    }
                                    XmlProcessing.WriteXML(HistoryPath, message, ClientName, DateTime.Now.ToString());
                                    //Console.WriteLine("Wrote message from " + ClientName);
                                    server.SendMessageById(message, CompanionName);
                                }
                                catch
                                {
                                    message = "!exitchat";
                                    server.SendMessageById(message, CompanionName);
                                    CompanionName = null;
                                    Locker = false;
                                    server.RemoveConnection(ClientName);
                                    Console.WriteLine(ClientName + " disconnected");
                                    Close();
                                    break;
                                }
                            }
                            break;
                        case "!no":
                            {
                                foreach (ClientObject clientObj in server.GetClients)
                                {
                                    if (InstructionArray[1] == clientObj.ClientName) server.SendMessage("!no", clientObj.client);
                                }
                            }
                            break;
                        case "!exit":
                            Console.WriteLine(ClientName + " disconnected");
                            Locker = false;
                            server.RemoveConnection(ClientName);
                            Close();
                            break;
                    }
                }
                catch
                {
                    Console.WriteLine(ClientName + " disconnected");
                    Locker = false;
                    server.RemoveConnection(ClientName);
                    Close();
                }
            }
        }

        private string GetMessage()
        {
            byte[] data = new byte[64];
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);

            return builder.ToString();
        }

        protected internal void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (client != null)
                client.Close();
        }
    }
}