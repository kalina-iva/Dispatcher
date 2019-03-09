using System;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.IO;

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
        static void socketForServer()
        {
            Console.Write("Введите свое имя:");
            string userName = Console.ReadLine();
            TcpClient client = null;
            try
            {
                client = new TcpClient(ip_server, port_server);
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
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                client.Close();
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

                Console.WriteLine("Сокет соединяется с {0} ", sender.RemoteEndPoint.ToString());
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
                get_ip_port();
                // Освобождаем сокет
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                get_ip_port();
            }
            //finally
            //{
            //    Console.ReadLine();
            //}
        }
        static bool get_ip_port()
        {
            bool flag = false;
            if (File.Exists(fileName))
            {
                using (StreamReader sr = File.OpenText(fileName))
                {
                    string s = "";
                    while ((s = sr.ReadLine()) != null)
                    {
                        if(s.Contains(function))
                        {
                            ip_server = s.Split(':')[0];
                            port_server = Int32.Parse(s.Split(':')[1]);
                            flag = true;
                        }
                        //Console.WriteLine(ip_server + " " + port_server.ToString());
                    }
                }
            }
            return flag;
        }
        static void Main(string[] args)
        {
            if (!get_ip_port())
            {
                connect_disp();
            }
            socketForServer();
        }
    }
}