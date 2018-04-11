using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.IO;

namespace ChatServer
{
    public class Server
    {
        static TcpListener tcpListener; // сервер для прослушивания
        List<ClientObject> clients = new List<ClientObject>(); // все подключения(онлайн)
        List<(ClientObject FirstClient, ClientObject SecondClient)> ActiveChats = new List<(ClientObject FirstClient, ClientObject SecondClient)>();//активные чаты

        public List<ClientObject> GetClients
        {
            get { return clients; }
        }

        protected internal void AddConnection(ClientObject clientObject)
        {
            clients.Add(clientObject);
        }

        protected internal void RemoveConnection(string id)
        {
            // получаем по id закрытое подключение
            ClientObject client = clients.FirstOrDefault(c => c.ClientName == id);
            // и удаляем его из списка подключений
            if (client != null)
                clients.Remove(client);
        }

        // прослушивание входящих подключений
        protected internal void Start()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 11111);
                tcpListener.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    switch(GetMessage(tcpClient))//cкорее всего и его нужно в отдельный поток(но это не точно) вроде там листенера нету
                    {
                        case "0":
                            //регистрация
                            //новый поток для регистрации и тд...
                            break;
                        case "1"://логин
                            if (true)//(проверка логина и пароля) {создание потока для обработки клиента}
                            {
                                string LoginName = "распаршенный логин";
                                ClientObject clientObject = new ClientObject(tcpClient, this, LoginName);
                                Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                                clientThread.Start();
                                SendMessage("login accepted", tcpClient);//+список онлайна
                            }
                            else
                            {
                                SendMessage("login unaccepted", tcpClient);
                            }
                            break;
                        case "2":
                            //список онлайна
                            break;
                    }
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
            byte[] data = new byte[64]; // буфер для получаемых данных
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

        // отправка сообщения микрочелу
        protected internal void BroadcastMessage(string message, string id)// надо переделать
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].ClientName != id) 
                {
                    clients[i].Stream.Write(data, 0, data.Length); //передача данных
                }
            }
        }

        // отключение всех клиентов
        protected internal void Disconnect()
        {
            tcpListener.Stop(); //остановка сервера

            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close(); //отключение клиента
            }
            Environment.Exit(0); //завершение процесса
        }
    }
}