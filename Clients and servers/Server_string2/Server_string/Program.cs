using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace Server_string
{ 
    class Program
    {
        static string name_disp = "localhost";
        private const int port_disp = 9292;
        //static string ip_disp = "127.0.0.1";

        static string ip_server = "127.0.0.1";
        const int port_server = 9111;
        static string function = "string";

        static TcpListener listener;
        static void connect_disp()
        {
            try
            {
                // Буфер для входящих данных
                byte[] bytes = new byte[1024];

                // Соединяемся с удаленным устройством
                // Устанавливаем удаленную точку для сокета
                IPHostEntry ipHost = Dns.GetHostEntry(name_disp);
                IPAddress ipAddr = ipHost.AddressList[0];
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port_disp);

                Socket sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Соединяем сокет с удаленной точкой
                sender.Connect(ipEndPoint);

                // Console.Write("Введите сообщение: ");
                string message = ip_server + ":" + port_server + ":" + function + ":<Server>";

                Console.WriteLine("Сокет соединяется с {0} ", sender.RemoteEndPoint.ToString());
                byte[] msg = Encoding.UTF8.GetBytes(message);

                // Отправляем данные через сокет
                int bytesSent = sender.Send(msg);

                // Получаем ответ от сервера
                int bytesRec = sender.Receive(bytes);

                Console.WriteLine("\nОтвет от сервера: {0}\n\n", Encoding.UTF8.GetString(bytes, 0, bytesRec));

                // Используем рекурсию для неоднократного вызова SendMessageFromSocket()
                //if (message.IndexOf("<TheEnd>") == -1)
                //    SendMessageFromSocket(port);

                // Освобождаем сокет
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.ToString());
            }
            //finally
            //{
            //    Console.ReadLine();
            //}
        }
        static void connect_client()
        {
            try
            {
                listener = new TcpListener(IPAddress.Parse(ip_server), port_server);
                listener.Start();
                Console.WriteLine("Ожидание подключений...");
 
                while(true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    ClientObject clientObject = new ClientObject(client);
 
                    // создаем новый поток для обслуживания нового клиента
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if(listener!=null)
                    listener.Stop();
            }
        }
        static void Main(string[] args)
        {
            connect_disp();
            connect_client();
        }        
    }
}