using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace ChatServer
{
    class FileListener
    {
        static TcpListener tcpListener;

        public void Start()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, 53011);
                tcpListener.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                while (true)
                {
                    //Console.WriteLine("begin");
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    Console.WriteLine("Accepted client");
                    //Thread AcceptedClientThread = new Thread(new ParameterizedThreadStart(AcceptedClientThreadFunction));
                    //AcceptedClientThread.Start(tcpClient as object);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Disconnect();
            }
        }

        protected internal void Disconnect()
        {
            tcpListener.Stop(); //остановка сервера
        }
    }
}
