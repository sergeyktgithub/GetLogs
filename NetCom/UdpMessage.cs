using System;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using NetComModels;
using NetComModels.Messages;
using Serilog;

namespace NetCom
{
    public class UdpMessage : IUdpMessage
    {
        private TwoEndPoints _twoEndPoints;
        
        public UdpMessage(TwoEndPoints twoEndPoints)
        {
            _twoEndPoints = twoEndPoints;
        }

        public UdpMessage(IPEndPoint source, IPEndPoint destination)
        {
            _twoEndPoints = new TwoEndPoints(source, destination);
        }

        public static void Send<T>(TwoEndPoints twoEndPoints, T msg) where T : Msg
        {
            UdpMessage udpMessage = new UdpMessage(twoEndPoints);
            udpMessage.SendInternal(msg); 
        }

        public void Send<T>(T msg) where T : Msg
        {
            SendInternal(msg);
        }

        private void SendInternal<T>(T msg) where T : Msg
        {
            Log.Debug($"Отправка пакета: net = {_twoEndPoints}");

            var json = JsonSerializer.Serialize(msg);

            SendPackageInternal(_twoEndPoints.Destination,
                new Package(json, msg.MsgType, _twoEndPoints.Source.Port));
        }

        private static void SendPackageInternal(IPEndPoint dst, Package package)
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                var msg = JsonSerializer.Serialize(package);

                byte[] sendbuf = Encoding.ASCII.GetBytes(msg);
                socket.SendTo(sendbuf, dst);

                Log.Debug($"Отправил: package = {package}");
            }
        }
    }
}