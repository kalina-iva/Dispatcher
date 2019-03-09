using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.IO;

namespace Dispatcher
{
    public struct Server
    {
        public string ip;
        public string port;
        public string function;
        public Server(string IP, string Port, string Function)
        {
            ip = IP;
            port = Port;
            function = Function;
        }
    }
    class Program
    {
        static List<Server> servers = new List<Server>();
        static string fileName = "servers.txt";
        static void Listening()
        {
            IPHostEntry ipHost = Dns.GetHostEntry("localhost");
            IPAddress ipAddr = ipHost.AddressList[0];
            const int port_disp = 9292;
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port_disp);

            // Создаем сокет Tcp/Ip
            Socket sListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Назначаем сокет локальной конечной точке и слушаем входящие сокеты
            try
            {
                sListener.Bind(ipEndPoint);
                sListener.Listen(20); //сколько клиентов может подцепиться

                // Начинаем слушать соединения
                while (true)
                {
                    Console.WriteLine("Ожидаем соединение через порт {0}", ipEndPoint);
                    // Программа приостанавливается, ожидая входящее соединение
                    Socket handler = sListener.Accept();
                    string data = null;
                    // Мы дождались клиента, пытающегося с нами соединиться
                    byte[] bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);

                    data += Encoding.UTF8.GetString(bytes, 0, bytesRec);
                    if (data.IndexOf("<Server>") > -1)
                    {
                        if(!servers.Contains(new Server(data.Split(':')[0], data.Split(':')[1], data.Split(':')[2])))
                        {
                            servers.Add(new Server(data.Split(':')[0], data.Split(':')[1], data.Split(':')[2]));
                            serversToFile(data.Split(':')[0], data.Split(':')[1], data.Split(':')[2]);
                        }// Отправляем ответ клиенту
                        string reply = "Спасибо за запрос в " + data.Length.ToString() + " символов";
                        byte[] msg = Encoding.UTF8.GetBytes(reply);
                        handler.Send(msg);
                    }
                    if (data.IndexOf("<Client>") > -1)
                    {
                        byte[] fileData = File.ReadAllBytes(fileName);
                        handler.Send(fileData);
                    }
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                Console.ReadLine();
            }
        }
        static void serversToFile(string ip, string port, string function)
        {
            using (StreamWriter fs = File.AppendText(fileName))
            {
                fs.WriteLine(ip + ":" + port + ":" + function);
            }
        }
        static void ServersFromFile()
        {
            if (File.Exists(fileName))
            {
                using (StreamReader sr = File.OpenText(fileName))
                {
                    string s = "";
                    while ((s = sr.ReadLine()) != null)
                    {
                        servers.Add(new Server(s.Split(':')[0], s.Split(':')[1], s.Split(':')[2]));
                    }
                }
            }
        }
        static void Main(string[] args)
        {
            ServersFromFile();
            Listening();
        }
    }
}