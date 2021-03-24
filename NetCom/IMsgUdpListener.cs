using System;
using System.Threading.Tasks;
using NetComModels;

namespace NetCom
{
    public interface IMsgUdpListener : IDisposable
    {
        /// <summary>
        /// Запускает слушателся udp сообщений
        /// </summary>
        void Start();

        /// <summary>
        /// Возвращает сообщение MsgClient
        /// </summary>
        /// <param name="type"></param>
        /// <returns>null - если нет сообщений для требуемого типа или очередь пуста</returns>
        Packet GetPacket(MsgType type);

        /// <summary>
        /// Возвращает сообщение MsgClient, если нет новых сообщние больше timeout - кидает TimeoutException
        /// </summary>
        /// <param name="type"></param>
        /// <returns>null - если нет сообщений для требуемого типа или очередь пуста</returns>
        Task<Packet> GetPacketAsync(MsgType type, int timeout = 5000);

        /// <summary>
        /// Вызывается когда приходит новое сообщение
        /// </summary>
        event EventHandler<MsgType> NewMessageReceivedEvent;
    }
}