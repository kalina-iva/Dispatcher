using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace WcfHost
{
    class Program
    {
        static string ip_server = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString();
        const int port_server = 13040;
        static string function = "wcf";

        static string name_disp = "dispatcher.ru";
        private const int port_disp = 9292;
        static void connect_disp()
        {
            try
            {                
                byte[] bytes = new byte[1024];
                IPHostEntry ipHost = Dns.GetHostEntry(name_disp);
                IPAddress ipAddr = ipHost.AddressList[0];                
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port_disp);
                Socket sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                sender.Connect(ipEndPoint);
                string message = ip_server + ":" + port_server + ":" + function + ":<Server>";
                Console.WriteLine("Сокет соединяется с {0} ", sender.RemoteEndPoint.ToString());
                byte[] msg = Encoding.UTF8.GetBytes(message);
                int bytesSent = sender.Send(msg);
                int bytesRec = sender.Receive(bytes);
                Console.WriteLine("Ответ от диспетчера: {0}\n", Encoding.UTF8.GetString(bytes, 0, bytesRec));
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Не удалось соединиться с диспетчером");
                //Console.WriteLine(ex.ToString());
            }
        }
        static void Main(string[] args)
        {
            connect_disp();
            ServiceMetadataBehavior behavior = new ServiceMetadataBehavior
            {
                HttpGetEnabled = true,
                MetadataExporter = { PolicyVersion = PolicyVersion.Policy15 }
            };
            //var host = new ServiceHost(typeof(WcfServiceLibrary.EchoService), new Uri("http: //localhost:13040/our/service"));
            using (var host = new ServiceHost(typeof(WcfServiceLibrary.EchoService), new Uri("http://" + 
                ip_server + ":" + port_server + "/our/service")))
            {
                host.Description.Behaviors.Add(behavior);
                host.AddServiceEndpoint(typeof(WcfServiceLibrary.IEchoService), new BasicHttpBinding(), "basic");
                //host.AddServiceEndpoint(typeof(WcfServiceLibrary.IEchoService), new WSHttpBinding(), "ws");
                //host.AddServiceEndpoint(typeof(WcfServiceLibrary.IEchoService), new NetTcpBinding(), "net.tcp://localhost:13054/our/service/tcp");
                //host.AddServiceEndpoint(typeof(WcfServiceLibrary.IEchoService), new NetNamedPipeBinding(), "net.pipe://localhost/our/service/pipe");
                host.Open();
                Console.WriteLine("Ожидание подключений через порт {0}:{1}", ip_server, port_server);
                Console.ReadLine();
            }
        }
    }
}
