using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace ChatServer
{
    public class Server
    {
        static TcpListener tcpListener;
        List<ClientObject> clients = new List<ClientObject>(); // все подключения(онлайн)
        List<ClientObject> BusyClients = new List<ClientObject>();
        Dictionary<string, bool> xmlMutexes = new Dictionary<string, bool>();

        public void LockMutex(string id)//true-занят
        {
            if (!xmlMutexes.ContainsKey(id)) xmlMutexes.Add(id, false);
            while (xmlMutexes[id])
            {
                Thread.Sleep(1000);
                Debug.WriteLine("someone is waiting for xml with this id: " + id);
            }
            xmlMutexes[id] = true;
        }

        public void UnlockMutex(string id)
        {
            xmlMutexes[id] = false;
        }
        public List<ClientObject> GetClients
        {
            get { return clients; }
        }

        public List<string> GetClientsNames
        {
            get
            {
                List<string> ClientsNames = new List<string>();
                foreach (ClientObject obj in clients)
                {
                    ClientsNames.Add(obj.ClientName);
                }
                return ClientsNames;
            }
        }

        protected internal void AddBusyClient(ClientObject clientObject)
        {
            BusyClients.Add(clientObject);
        }

        protected internal void RemoveBusyClient(string id)
        {
            try
            {
                ClientObject client = BusyClients.FirstOrDefault(c => c.ClientName == id);
                if (client != null) BusyClients.Remove(client);
            }
            catch { }
        }

        protected internal bool IsBusy(string id)
        {
            
            ClientObject client = BusyClients.FirstOrDefault(c => c.ClientName == id);
            if (client == null) return false;
            else return true;
        }

        protected internal void AddConnection(ClientObject clientObject)
        {
            clients.Add(clientObject);
        }

        protected internal void RemoveConnection(string id)
        {
            ClientObject client = clients.FirstOrDefault(c => c.ClientName == id);
            if (client != null) clients.Remove(client);
        }

        void AcceptedClientThreadFunction(object Client)
        {
            TcpClient tcpClient = Client as TcpClient;
            List<string> InstructionArray;
            string message = "";
            try
            {
                message = GetMessage(tcpClient);
            }
            catch
            {
            }
            Console.WriteLine(message);
            InstructionArray = CommandTranslator.Parse(message);//0-команда(signup, login, online)
                                                                //1-логин
                                                                //2-пароль
                                                                //3-геолокация
            switch (InstructionArray[0])
            {
                case "!signup":
                    try
                    {
                        UserBaseDao.Write(InstructionArray[1], InstructionArray[2]);
                        SendMessage("!accepted" + CommandTranslator.Encode(GetClientsNames), tcpClient);//+список онлайна
                        ClientObject RegClientObject = new ClientObject(tcpClient, this, InstructionArray[1], InstructionArray[3]);
                        Thread RegClientThread = new Thread(new ThreadStart(RegClientObject.Process));
                        RegClientThread.Start();
                    }
                    catch
                    {
                        SendMessage("!unacceptedsignup", tcpClient);
                    }
                    //добавление в базу
                    break;
                case "!login":
                    if (UserBaseDao.Find(InstructionArray[1], InstructionArray[2]))
                    {
                        SendMessage("!accepted" + CommandTranslator.Encode(GetClientsNames), tcpClient);//+список онлайна
                        ClientObject clientObject = new ClientObject(tcpClient, this, InstructionArray[1], InstructionArray[3]);
                        Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                        clientThread.Start();
                    }
                    else
                    {
                        SendMessage("!unaccepted", tcpClient);
                        Console.WriteLine("!unaccepted");
                    }
                    break;
                case "!online":
                    Console.WriteLine("In online");
                    SendMessage(CommandTranslator.Encode(GetClientsNames), tcpClient);
                    break;
                case "!file":
                    Console.WriteLine("File!!!!");
                    break;
            }
        }

        private void OnlineBroadcast()
        {
            StringBuilder OnlineString;
            byte[] data;
            while (true)
            {
                foreach (ClientObject Client in clients)
                {
                    OnlineString = new StringBuilder();
                    OnlineString.Append("!online");
                    foreach (ClientObject OnlineClient in clients)
                    {
                        OnlineString.Append(":" + OnlineClient.ClientName);
                        if (IsBusy(OnlineClient.ClientName)) OnlineString.Append(",+");
                        else OnlineString.Append(",-");
                    }
                    data = Encoding.Unicode.GetBytes(OnlineString.ToString());
                    Client.Stream.Write(data, 0, data.Length);
                }
                Thread.Sleep(3000);
            }
        }

        protected internal void Start()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, 53010);
                tcpListener.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений...");
                //Thread OnlineBroadcastThread = new Thread(OnlineBroadcast);
                //OnlineBroadcastThread.Start();

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    Console.WriteLine("Accepted client");
                    Thread AcceptedClientThread = new Thread(new ParameterizedThreadStart(AcceptedClientThreadFunction));
                    AcceptedClientThread.Start(tcpClient as object);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Disconnect();
            }
        }

        private string GetMessage(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] data = new byte[64];
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (stream.DataAvailable);

            return builder.ToString();
        }

        protected internal void SendMessage(string message, TcpClient client)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);
        }

        protected internal void SendMessageById(string message, string id)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].ClientName == id)
                {
                    clients[i].Stream.Write(data, 0, data.Length);
                }
            }
        }

        protected internal void Disconnect()
        {
            tcpListener.Stop();

            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close();
            }
            Environment.Exit(0);
        }
    }
}