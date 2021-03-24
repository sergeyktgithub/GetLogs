using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using NetComModels;
using Serilog;

namespace NetCom
{
    public class MessageUdp
    {
        private string _sourceName;
        private TwoEndPoints _twoEndPoints;
        
        public MessageUdp(string sourceName, TwoEndPoints twoEndPoints)
        {
            _sourceName = sourceName;
            _twoEndPoints = twoEndPoints;
        }

        public MessageUdp(string sourceName, IPEndPoint source, IPEndPoint destination)
        {
            _sourceName = sourceName;
            _twoEndPoints = new TwoEndPoints(source, destination);
        }

        public static void SendText(TwoEndPoints twoEndPoints, string text)
        {
            Log.Debug($"Отправка текста: text = {text}, net = {twoEndPoints}");

            MessageUdp messageUdp = new MessageUdp(Environment.MachineName, twoEndPoints);
            var msgText = new MsgText(text);
            messageUdp.Send(msgText);
        }

        public static void Send<T>(TwoEndPoints twoEndPoints, T packet) where T : Msg
        {
            Log.Debug($"Отправка пакета: net = {twoEndPoints}");

            MessageUdp messageUdp = new MessageUdp(Environment.MachineName, twoEndPoints);
            messageUdp.Send(packet);
        }

        public void Send<T>(T packet) where T : Msg
        {
            var json = JsonSerializer.Serialize(packet);

            var msgClient = new Packet(_sourceName, json, packet.MsgType, _twoEndPoints.Source);
            SendInternal(_twoEndPoints.Destination, msgClient);
        }

        private static void SendInternal(IPEndPoint dstNetEndPoint, Packet packet)
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                var msg = JsonSerializer.Serialize(packet);

                byte[] sendbuf = Encoding.ASCII.GetBytes(msg);
                IPEndPoint ep = dstNetEndPoint;

                socket.SendTo(sendbuf, ep);

                Log.Debug($"Отправил сообщение: msg = {msg}");
            }
        }
    }
}