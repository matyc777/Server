using System;
using System.Net.Sockets;
using System.Text;

namespace ChatServer
{
    public class ClientObject
    {
        protected internal string ClientName { get; private set; }//его принимаем за уникальное имя
        protected internal NetworkStream Stream { get; private set; }
        string CompanionName;
        TcpClient client { get; }
        Server server; // сервер

        public ClientObject(TcpClient tcpClient, Server serverObject, string ClientName)
        {
            this.ClientName = ClientName;
            client = tcpClient;
            server = serverObject;
            serverObject.AddConnection(this);
            Stream = client.GetStream();//возможно лучше засунуть в метод Process()
        }

        public void Process()
        {
            try
            {
                string message = GetMessage();
                //тут должен быть парсинг сообщения
                string Name = "";//допустим мы получили имя с парсинга для отправки запроса на чаттинг

                switch(message)
                {
                    case "0":// если клиент захотел создать чат(он прислал имя Челика с которым хочет чатиться)
                        foreach(ClientObject clientObj in server.GetClients)// тогда ищем в онлайне этого челика и отправляем ему запрос
                        {
                            if (Name == clientObj.ClientName) server.SendMessage("I wanna chatting with you" + ClientName, clientObj.client);
                        }
                        string answer = GetMessage();
                        if (answer == "yes")// если согласен
                        {
                            CompanionName = message;
                            // в бесконечном цикле получаем сообщения от клиента
                            while (true)
                            {
                                try
                                {
                                    message = GetMessage();// получаем мессаге от клиента(что-то в духе: я хочу отправить сообщение)
                                    message = String.Format("{0}: {1}", CompanionName, message);// форматируем мессаге ф понятную для клиенткого приложения форму
                                    //Console.WriteLine(message);
                                    server.BroadcastMessage(message, this.CompanionName);// и отправляем мессаге рофлочелику(2ой клиент)
                                }
                                catch
                                {
                                    message = String.Format("{0}: покинул чат", CompanionName);
                                    Console.WriteLine(message);
                                    server.BroadcastMessage(message, this.ClientName);//отправляем мессаге рофлочелику(2ой клиент) что клиент(собеседник) ливнул
                                    break;
                                }
                            }
                        }
                        else//если не согласен
                        {
                            CompanionName = null;
                            Process();
                        }
                        break;
                    case "I wanna chating with you"://сюда придёт ещё имя
                        server.SendMessage("Someone wanna chat with you + имя которое пришло", client);
                        string name="";//имя чела который хочет чатиться(надо будет распарсить)
                        string answer2 = GetMessage();
                        if (answer2 == "yes")// отправляем тому кто запрашивал ответ клиента
                        {
                            foreach (ClientObject clientObj in server.GetClients)// ищем в онлайне этого челика и отправляем ему запрос
                            {
                                if (name == clientObj.ClientName) server.SendMessage("Yes, I wanna chat too" + ClientName, clientObj.client);//ищем его имя и отправляем ответ "ДА"
                            }
                            CompanionName = name;// имя собеседника 
                            // в бесконечном цикле получаем сообщения от клиента
                            while (true)
                            {
                                try
                                {
                                    message = GetMessage();// получаем мессаге от клиента(что-то в духе: я хочу отправить сообщение)
                                    message = String.Format("{0}: {1}", CompanionName, message);// форматируем мессаге ф понятную для клиенткого приложения форму
                                    //Console.WriteLine(message);
                                    server.BroadcastMessage(message, this.CompanionName);// и отправляем мессаге рофлочелику(2ой клиент)
                                }
                                catch
                                {
                                    message = String.Format("{0}: покинул чат", CompanionName);
                                    Console.WriteLine(message);
                                    server.BroadcastMessage(message, this.ClientName);//отправляем мессаге рофлочелику(2ой клиент) что клиент(собеседник) ливнул
                                    break;
                                }
                            }
                        }
                        else
                        {
                            foreach (ClientObject clientObj in server.GetClients)// тогда ищем в онлайне этого челика и отправляем ему запрос
                            {
                                if (name == clientObj.ClientName) server.SendMessage("No, i dont wanna chat" + ClientName, clientObj.client);//ищем его имя и отправляем ответ "НЕТ"
                            }
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                // в случае выхода из цикла закрываем ресурсы
                server.RemoveConnection(this.ClientName);
                Close();
            }
        }

        // чтение входящего сообщения
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

        // закрытие подключения
        protected internal void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (client != null)
                client.Close();
        }
    }
}