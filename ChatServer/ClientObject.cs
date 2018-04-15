using System;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

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
        }

        public void Process()
        {
            bool Locker = true;
            List<string> InstructionArray;
            while (Locker)
            {
                try
                {
                    Stream = client.GetStream();
                    string message = GetMessage();
                    InstructionArray = CommandTranslator.Parse(message);
                    Console.WriteLine(message);

                    switch (InstructionArray[0])
                    {
                        case "newchat":// если клиент захотел создать чат(он прислал имя Челика с которым хочет чатиться)
                            foreach (ClientObject clientObj in server.GetClients)// тогда ищем в онлайне этого челика и отправляем ему запрос
                            {
                                if (InstructionArray[1] == clientObj.ClientName) server.SendMessage("newchat?:" + ClientName, clientObj.client);//**либо переделать отправку
                            }
                            string answer = GetMessage();
                            if (answer == "yes")// если согласен
                            {
                                CompanionName = InstructionArray[1];
                                // в бесконечном цикле получаем сообщения от клиента
                                while (true)
                                {
                                    try
                                    {
                                        message = GetMessage();// получаем мессаге от клиента(что-то в духе: я хочу отправить сообщение)
                                        if (message == "exitchat")
                                        {
                                            server.BroadcastMessage(message, CompanionName);
                                            CompanionName = null;
                                            break;
                                        }
                                        else if (message == "exitchataccept")
                                        {
                                            CompanionName = null;
                                            break;
                                        }
                                        message = String.Format("{0}: {1}", ClientName, message);// форматируем мессаге ф понятную для клиенткого приложения форму
                                        //Console.WriteLine(message);
                                        server.BroadcastMessage(message, CompanionName);// и отправляем мессаге рофлочелику(2ой клиент)
                                    }
                                    catch
                                    {
                                        message = "exitchat";
                                        server.BroadcastMessage(message, CompanionName);
                                        CompanionName = null;
                                        Locker = false;//отправляем мессаге рофлочелику(2ой клиент) что клиент(собеседник) ливнул
                                        server.RemoveConnection(ClientName);
                                        Close();
                                        break;
                                    }
                                }
                            }
                            else//если не согласен
                            {
                                CompanionName = null;
                                //Process();
                            }
                            break;
                        case "yes"://сюда придёт ещё имя//**либо здесь обрабатывать ответ на вопрос о чате
                            foreach (ClientObject clientObj in server.GetClients)// ищем в онлайне этого челика и отправляем ему запрос
                            {
                                if (InstructionArray[1] == clientObj.ClientName)
                                {
                                    server.SendMessage("yes", clientObj.client);//********
                                    Console.WriteLine("Sended yes from " + ClientName + " to " + clientObj.ClientName);
                                }//ищем его имя и отправляем ответ "ДА"
                            }
                            CompanionName = InstructionArray[1];// имя собеседника 
                                                                    // в бесконечном цикле получаем сообщения от клиента
                            while (true)
                            {
                                try
                                {
                                    message = GetMessage();// получаем мессаге от клиента(что-то в духе: я хочу отправить сообщение)
                                    if (message == "exitchat")
                                    {
                                        server.BroadcastMessage(message, CompanionName);
                                        CompanionName = null;
                                        break;
                                    }
                                    else if (message == "exitchataccept")
                                    {
                                        CompanionName = null;
                                        break;
                                    }
                                    message = String.Format("{0}: {1}", ClientName, message);// форматируем мессаге ф понятную для клиенткого приложения форму
                                    server.BroadcastMessage(message, CompanionName);// и отправляем мессаге рофлочелику(2ой клиент)
                                }
                                catch
                                {
                                    message = "exitchat";
                                    server.BroadcastMessage(message, CompanionName);
                                    CompanionName = null;
                                    Locker = false;//отправляем мессаге рофлочелику(2ой клиент) что клиент(собеседник) ливнул
                                    server.RemoveConnection(ClientName);
                                    Close();
                                    break;
                                }
                            }
                            break;
                        case "no":
                            {
                                foreach (ClientObject clientObj in server.GetClients)// тогда ищем в онлайне этого челика и отправляем ему запрос
                                {
                                    if (InstructionArray[1] == clientObj.ClientName) server.SendMessage("no", clientObj.client);//ищем его имя и отправляем ответ "НЕТ"
                                }
                            }
                            break;
                        case "exit":
                            Console.WriteLine("Disconnected");
                            Locker = false;
                            server.RemoveConnection(ClientName);
                            Close();
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            //finally
            //{
            //    Console.WriteLine("Disconnected");
            //    // в случае выхода из цикла закрываем ресурсы
            //    //server.RemoveConnection(this.ClientName);
            //    Close();
            //}
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