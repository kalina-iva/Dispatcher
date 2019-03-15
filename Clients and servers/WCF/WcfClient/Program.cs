using System;
using System.Collections.Generic;
using WcfClient.EchoServiceReference;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Collections;

namespace WcfClient
{
    class Program
    {
        static string name_disp = "dispatcher.ru";
        const int port_disp = 9292;

        static int port_server;
        static string ip_server;
        static string function = "wcf";

        static string fileName = "servers.txt";
        static bool total = false;
        static ArrayList servers = new ArrayList();
        static void connect_disp()
        {
            try
            {
                Console.WriteLine("Соединяемся с диспетчером");
                IPHostEntry ipHost = Dns.GetHostEntry(name_disp);
                IPAddress ipAddr = ipHost.AddressList[0];
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port_disp);
                Socket sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                sender.Connect(ipEndPoint);
                byte[] msg = Encoding.UTF8.GetBytes("<Client>");
                int bytesSent = sender.Send(msg);
                byte[] fileData = new byte[1024];
                int bytesRec = sender.Receive(fileData);
                // преобразуем поток байтов в текстовый файл
                using (FileStream fs = File.Create(fileName))
                {
                    fs.Write(fileData, 0, bytesRec);
                }
                // получим адрес сервера
                if (get_ip_port_file())
                {
                    get_ip_port_list();
                }
                // Освобождаем сокет
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка соединения с диспетчером");
                //Console.WriteLine(ex.ToString());
                Console.WriteLine("Невозможно установить соединение с сервером");
                Console.Read();
            }
        }
        static bool get_ip_port_file()
        {
            servers.Clear();
            bool flag = false;
            if (File.Exists(fileName))
            {
                using (StreamReader sr = File.OpenText(fileName))
                {
                    string s = "";
                    while ((s = sr.ReadLine()) != null)
                    {
                        if (s.Contains(function))
                        {
                            servers.Add(s);
                            flag = true;
                        }
                    }
                }
            }
            return flag;
        }
        static void get_ip_port_list()
        {
            // смотрим в списке адресов
            foreach (string server in servers)
            {
                // если адрес не использовался при подключении к серверу
                if (!server.Contains("<unavailable>"))
                {
                    ip_server = server.Split(':')[0];
                    port_server = Int32.Parse(server.Split(':')[1]);
                    total = true;
                    return;
                }
            }
            total = false;
        }
        static void ForServer()
        {
            try
            {
                var myType = new WcfServiceLibrary.CompositeType();
                myType.BoolValue = true;
                myType.StringValue = "Строка передана: ";
                myType.IntValue = 1;
                Console.WriteLine(new EchoServiceClient("BasicHttpBinding_IEchoService",
                    "http://" + ip_server + ":" + port_server + "/our/service/basic").GetDataUsingDataContract(myType).StringValue);
                Console.WriteLine("Запрос был отправлен серверу");
                Console.ReadLine();
            }
            catch(Exception)
            {
                Console.WriteLine("Ошибка соединения с сервером");
                total = false;
                string server;
                for (int i = 0; i < servers.Count; i++)
                {
                    server = (string)servers[i];
                    if (server.Contains(ip_server + ":" + port_server))
                    {
                        servers[i] += "<unavailable>";
                        //total = true;
                        break;
                    }
                }
                get_ip_port_list();
            }
        }
        private static void Main(string[] args)
        {
            if (!get_ip_port_file())
                connect_disp();
            else
                get_ip_port_list();
            while (total)
            {
                ForServer();
            }
            connect_disp();
            while (total)
            {                
                ForServer();
            }
            if (!total)
            {
                Console.WriteLine("Невозможно установить соединение с сервером");
                Console.Read();
            }
            
        }
    }
}