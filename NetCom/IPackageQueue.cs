using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NetComModels;
using NetComModels.Messages;

namespace NetCom
{
    public interface IPackageQueue : IDisposable
    {
        /// <summary>
        /// Отменяет ожидания сообщений
        /// </summary>
        void CancelWaitPackage();

        /// <summary>
        /// Очищает очередь сообщений
        /// </summary>
        void Clear();

        /// <summary>
        /// Возвращает true, если очередь содержит сообщение type и false в противном случае
        /// </summary>
        /// <param name="type">тип ожидаемого сообщения</param>
        /// <returns></returns>
        bool Contains(MsgType type);

        /// <summary>
        /// Возвращает true, если очередь сообщений пуста
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Возвращает сообщение MsgClient
        /// </summary>
        /// <param name="type"></param>
        /// <returns>null - если нет сообщений для требуемого типа или очередь пуста</returns>
        bool TryGetPackage(MsgType type, out Package package);

        /// <summary>
        /// Ждет ответ pong в очереди на сообщение ping
        /// </summary>
        /// <param name="endPoint">откуда должен прийти ответ</param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        Task<bool> WaitPong(IPEndPoint endPoint, int timeout = 3000);

        /// <summary>
        /// Ждет новое сообщение указанного типа type, если timeout вышел, кидает TimeoutException
        /// </summary>
        /// <param name="type">тип ожидаемого сообщения</param>
        /// <param name="timeout">время ожидания сообщения</param>
        Task<Package> WaitPackageByTypeAsync(MsgType type, int timeout = 3000);

        /// <summary>
        /// Ждет любого нового сообщения и сразу возвращает, если timeout вышел, кидает TimeoutException
        /// </summary>
        /// <param name="timeout">время ожидания сообщения</param>
        /// <returns></returns>
        Task<Package> WaitAnyPackageAsync(int timeout = 3000);

        /// <summary>
        /// Вызывается когда приходит новое сообщение
        /// </summary>
        event EventHandler<MsgType> NewMessageReceivedEvent;
    }
}