using System;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.IO;
using System.Collections;

namespace ConsoleClient
{
    class Program
    {
        static string function = "string";

        static string name_disp = "localhost";
        const int port_disp = 9292;

        //const int port_server = 8888;
        //const string ip_server = "127.0.0.1";
        static int port_server;
        static string ip_server;

        static string fileName = "servers.txt";
        static ArrayList servers = new ArrayList();

        static bool total = false;
        static void socketForServer()
        {
            // TcpClient client = null;
            try
            {
                using (var client = new TcpClient(ip_server, port_server))
                {
                    Console.Write("Введите свое имя:");
                    string userName = Console.ReadLine();
                    NetworkStream stream = client.GetStream();

                    while (true)
                    {
                        Console.Write(userName + ": ");
                        // ввод сообщения
                        string message = Console.ReadLine();
                        message = String.Format("{0}: {1}", userName, message);
                        // преобразуем сообщение в массив байтов
                        byte[] data = Encoding.Unicode.GetBytes(message);
                        // отправка сообщения
                        stream.Write(data, 0, data.Length);

                        // получаем ответ
                        data = new byte[64]; // буфер для получаемых данных
                        StringBuilder builder = new StringBuilder();
                        int bytes = 0;
                        do
                        {
                            bytes = stream.Read(data, 0, data.Length);
                            builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                        }
                        while (stream.DataAvailable);

                        message = builder.ToString();
                        Console.WriteLine("Сервер: {0}", message);
                        total = true;
                    }
                }
            }
            catch (Exception ex)
            {
                total = false;
                string server;
                for(int i=0; i<servers.Count; i++)
                {
                    server = (string)servers[i];
                    if (server.Contains(ip_server+":"+port_server))
                    {
                        servers[i]+="<unavailable>";
                        //total = true;
                        break;
                    }
                }
                //Console.WriteLine(ex.Message);
                get_ip_port_list();
            }
        }
        static void connect_disp()
        {
            try
            {
                // Соединяемся с удаленным устройством
                // Устанавливаем удаленную точку для сокета
                IPHostEntry ipHost = Dns.GetHostEntry(name_disp);
                IPAddress ipAddr = ipHost.AddressList[0];
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port_disp);

                Socket sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Соединяем сокет с удаленной точкой
                sender.Connect(ipEndPoint);

                //Console.WriteLine("Сокет соединяется с {0} ", sender.RemoteEndPoint.ToString());
                byte[] msg = Encoding.UTF8.GetBytes("<Client>");

                // Отправляем данные через сокет
                int bytesSent = sender.Send(msg);
                byte[] fileData = new byte[1024];
                // Получаем ответ от сервера
                int bytesRec = sender.Receive(fileData);
                using (FileStream fs = File.Create(fileName))
                {
                    fs.Write(fileData, 0, bytesRec);
                }
                if(get_ip_port_file())
                {
                    get_ip_port_list();
                }
                // Освобождаем сокет
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.ToString());
                Console.WriteLine("Невозможно установить соединение с сервером");
                Console.Read();
            }
        }
        static bool get_ip_port_file()
        {
            string ip;
            servers.Clear();
            int port;
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
            foreach(string server in servers)
            {
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
        static void Main(string[] args)
        {
            // читаем адреса нужных серверов из файла
            if (!get_ip_port_file())
            {
                // если по каким-то причинам не удалось прочитать адреса из файла, обращаемся к диспетчеру
                connect_disp();
            }
            else
            {
                // получаем ip и порт сервера
                get_ip_port_list();
            }
            while (total)
            {
                // если получили адрес сервера, пробуем соединиться с ним
                socketForServer();
            }
            //если соединение упало, проверяем вдруг у диспетчера появилось что-то новенькое
            connect_disp();
            while (total)
            {
                // снова коннектимся с сервером
                socketForServer();
            }
            if (!total)
            {
                // сервера не отвечают
                Console.WriteLine("Невозможно установить соединение с сервером");
                Console.Read();
            }
        }
    }
}