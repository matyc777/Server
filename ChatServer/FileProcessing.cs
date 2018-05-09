using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using System.Media;
//using Microsoft.DirectX.AudioVideoPlayback;

namespace ChatServer
{
    class FileProcessing
    {
        static public void Send(List<string> paths, string ip)//0-путь, 1-ip
        {
            FileStream stream = null;
            BinaryReader f = null;
            byte[] fNumb = new byte[1] { 0 }; //Кол-во файлов
            byte[] bFName = new byte[512]; //Имя файла
            byte[] bFSize = new byte[512]; //Размер файла
            byte[] buffer = new byte[1024]; //Буфер для файла
            byte[] ping = new byte[1] { 0 }; //Синхронизация
            string fName = ""; //Имя ф-ла
            string host = ""; //Имя конечной точки
            ulong fSize = 0; //Размер ф-ла

            try
            {
                host = ip;
                fNumb[0] = (byte)paths.Count;
                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ipAddress = Dns.GetHostEntry(host).AddressList[0];
                IPEndPoint Addr = new IPEndPoint(ipAddress, 7070);
                s.Connect(Addr);
                s.Send(fNumb);
                s.Receive(ping); // 1
                for (byte i = 0; i < paths.Count; i++)
                {
                    fName = Path.GetFileName(paths[i]);
                    bFName = Encoding.UTF8.GetBytes(fName);
                    //s.Send(bFName, fName.Length, SocketFlags.None); //Передаем имя
                    s.Send(bFName); //Передаем имя
                    s.Receive(ping); // 2
                    stream = new FileStream(paths[i], FileMode.Open, FileAccess.Read);
                    f = new BinaryReader(stream);
                    fSize = (ulong)stream.Length;
                    bFSize = Encoding.UTF8.GetBytes(Convert.ToString(stream.Length));
                    s.Send(bFSize); //Передаем размер
                    s.Receive(ping); // 3
                    int bytes = 1024;
                    ulong processed = 0; //Байт передано
                    while (processed < fSize) //Передаем файл
                    {
                        if ((fSize - processed) < 1024)
                        {
                            bytes = (int)(fSize - processed);
                            byte[] buf = new byte[bytes];
                            f.Read(buf, 0, bytes);
                            s.Send(buf);
                        }
                        else
                        {
                            f.Read(buffer, 0, bytes);
                            s.Send(buffer);
                        }
                        //s.Receive(ping); // 4
                        processed = processed + 1024;
                    }
                    s.Receive(ping); // 5
                    if (f != null)
                    { f.Close(); }
                }
                s.Receive(ping);
                s.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static public string Receive(string path)
        {
            FileStream stream = null;
            BinaryWriter f = null;
            byte[] fNumb = new byte[1] { 0 }; //Кол-во файлов
            byte[] bFName = new byte[512]; //Имя файла
            byte[] bFSize = new byte[512]; //Размер файла
            byte[] buffer = new byte[1024]; //Буфер для файла
            byte[] ping = new byte[1] { 0 }; //Синхронизация
            string fName = ""; //Имя ф-ла
            string fullPath = ""; //Полный путь
            ulong fSize = 0; //Размер ф-ла

            try
            {
                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ipAddress = Dns.GetHostEntry("localhost").AddressList[0];
                // IPAddress ipAddress = new IPAddress(0xFFFFFFFF);
                IPEndPoint Addr = new IPEndPoint(IPAddress.Any, 7070);
                s.Bind(Addr);
                s.Listen(1);

                Socket cl = s.Accept(); //Коннектнутый сокет
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path); //Создаем каталог
                }
                cl.Receive(fNumb); //в [0] лежит кол-во файлов
                cl.Send(ping); // 1


                for (byte i = 0; i < fNumb[0]; i++)
                {
                    cl.Receive(bFName); //Принимаем имя
                    cl.Send(ping); // 2
                    fName = Encoding.UTF8.GetString(bFName);
                    for (int k = 0; k < bFName.Length; k++)
                    {
                        bFName[k] = 0;
                    }
                    fName = fName.TrimEnd('\0');
                    if (fName == "")
                    { fName = " "; }
                    fullPath = path + fName;
                    while (File.Exists(fullPath))
                    {
                        int dotPos = fullPath.LastIndexOf('.');
                        if (dotPos == -1)
                        {
                            fullPath += "[1]";
                        }
                        else
                        {
                            fullPath = fullPath.Insert(dotPos, "[1]");
                        }
                    }
                    cl.Receive(bFSize); //Принимаем размер
                    cl.Send(ping); // 3
                    fSize = Convert.ToUInt64(Encoding.UTF8.GetString(bFSize));
                    stream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write);
                    f = new BinaryWriter(stream);
                    ulong processed = 0; //Байт принято
                    while (processed < fSize) //Принимаем файл
                    {
                        if ((fSize - processed) < 1024)
                        {
                            int bytes = (int)(fSize - processed);
                            byte[] buf = new byte[bytes];
                            bytes = cl.Receive(buf);
                            f.Write(buf, 0, bytes);
                        }
                        else
                        {
                            int bytes = cl.Receive(buffer);
                            f.Write(buffer, 0, bytes);
                        }
                        //cl.Send(ping); // 4
                        processed = processed + 1024;
                    }
                    cl.Send(ping); // 5
                    if (f != null)
                    { f.Close(); }
                }
                cl.Send(ping);
                s.Close();
                cl.Close();
                return fullPath;
            }
            catch (Exception e)
            {
                return "error";
                //Console.WriteLine(e.Message);
            }
        }

        //static void Main(string[] args)
        //{
        //    List<string> paths = new List<string>();
        //    if (args.Length == 0)
        //    {
        //        ConsoleKeyInfo key = Console.ReadKey(true);
        //        switch (key.Key)
        //        {
        //            case ConsoleKey.Spacebar:
        //                string path = Console.ReadLine();
        //                if (path == "")
        //                { path = @"D:\"; }
        //                Receive(path);
        //                break;
        //            default:
        //                paths.Add(Console.ReadLine());
        //                Send(paths);
        //                break;
        //        }
        //    }
        //    else
        //    {
        //        paths.AddRange(args);
        //        Send(paths);
        //    }
        //    Console.ReadKey();
        //}
    }
}