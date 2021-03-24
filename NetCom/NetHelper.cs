using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace NetCom
{
    public static class NetHelper
    {
        public static IPAddress GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }

            throw new Exception("Сетевой адаптер не найден");
        }

        public static bool CheckAvailableServerPort(int port)
        {
            return IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().All(endpoint => endpoint.Port != port);
        }

        public static int GetRandomServerPort(int minPort = 30000, int maxPort = 40000)
        {
            var rnd = new Random();
            var rndPort = rnd.Next(minPort, maxPort);

            for (var i = 0; i < 50; i++)
            {
                if (CheckAvailableServerPort(rndPort))
                {
                    return rndPort;
                }

                rndPort = rnd.Next(minPort, maxPort);
            }
            
            throw new Exception("Не удалось получить случайный порт");
        }
    }
}