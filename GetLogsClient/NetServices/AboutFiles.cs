using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GetLogsClient.Models;
using NetCom;
using NetCom.Extensions;
using NetCom.Helpers;
using NetComModels;
using NetComModels.ErrorMessages;
using NetComModels.Messages;
using TimeoutException = System.TimeoutException;

namespace GetLogsClient.NetServices
{
    public abstract class AboutFiles<T> where T : SearchByAccIdMsg
    {
        private string[] funnyWait = {
            "ждем ...",
            "снова ждем  ...",
            "ожидание  ...",
            "очень много ждем ...",
            "Грёбаная пробка в Суэцком канале. Теперь опять подорожает ЖКХ, хлеб и яйца...",
            "Эффект плацебо в медицине - это когда тебе озвучивают стоимость лечения, и твой мозг решает, что у тебя уже ничего не болит.",
            "Когда-то в Советском Союзе весной была добрая традиция — сажать деревья, сегодня — губернаторов.",
            "...",
            "... ....",
            "... .... ..........",
            "Ремонт закончен не тогда, когда ушел последний работник, а когда инет перестал тебе подсовывать рекламу обоев, плитки и окон.",
            "Семья программистов. — Зайди сюда! — Куда, нет ссылки?! — НА КУХНЮ ЗАЙДИ!!!",
            "Директору пивзавода от группы программистов. Заявление. — Просим Вас предоставить выделенную линию со скоростью 0,5 л/сек.",
            "Хуже всего приходится программистам из Microsoft. — Им, бедолагам, в случае чего и обругать—то некого.",
            "Один программист любил компьютеры, и однажды его застукали за этим делом."
        };

        private readonly IPackageQueue _packageQueue;
        private readonly List<TwoEndPoints> _twoEndPointsList;

        public event EventHandler<string> ProcessMsgEvent;

        public AboutFiles(List<TwoEndPoints> twoEndPointsList, IPackageQueue packageQueue)
        {
            _packageQueue = packageQueue ?? throw new ArgumentNullException(nameof(packageQueue));
            _twoEndPointsList = twoEndPointsList;
        }

        public async Task<List<PreparedArchive>> CheckExistFilesAsync(T msg, int timeout = 1000 * 60 * 1)
        {
            SendMessages(msg, out var numberAvailableEndPoints);
            return await ReceiveAnswer(msg, numberAvailableEndPoints, timeout);
        }

        private void SendMessages(T msg, out int numberAvailableEndPoints)
        {
            var tasks = new List<Task>();
            var lambdaNumberAvailableEndPoints = 0;

            foreach (var twoEndPoints in _twoEndPointsList)
            {
                var task = Task.Run(async () =>
                {
                    try
                    {
                        UdpMessage.Send(twoEndPoints, new PingMsg());
                        if (await _packageQueue.WaitPong(twoEndPoints.Destination))
                        {
                            UdpMessage.Send(twoEndPoints, msg);
                            OnProcessMsgEvent(
                                $"{twoEndPoints.Destination} доступен");

                            lambdaNumberAvailableEndPoints++;
                        }
                        else
                        {
                            OnProcessMsgEvent(
                                $"{twoEndPoints.Destination} недоступен");
                        }
                    }
                    catch (TimeoutException ex)
                    {
                        OnProcessMsgEvent(
                            $"{twoEndPoints.Destination} недоступен, {ex.Message}");
                    }
                });

                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());

            numberAvailableEndPoints = lambdaNumberAvailableEndPoints;
        }

        private async Task<List<PreparedArchive>> ReceiveAnswer(T msg, int numberAvailableEndPoints, int timeout)
        {
            var preparedArchives = new List<PreparedArchive>();
            var interval = 3000;
            var startDt = DateTime.Now;
            var random = new Random();

            while ((DateTime.Now - startDt).TotalMilliseconds < timeout)
            {
                try
                {
                    PreparedArchive preparedArchive = null;

                    try
                    {
                        preparedArchive = await WaitAnswerAsync(msg.AccId, interval);
                    }
                    catch (AccIdNotFoundException)
                    {
                        numberAvailableEndPoints--;
                    }
                    catch (Exception ex)
                    {
                        if (preparedArchives.Any(x => x.IsEmpty == false))
                        {
                            break;
                        }
                    }

                    if (preparedArchive != null)
                    {
                        preparedArchives.Add(preparedArchive);

                        if (preparedArchives.Count >= numberAvailableEndPoints)
                        {
                            break;
                        }
                    }

                    OnProcessMsgEvent(funnyWait[random.Next(0, funnyWait.Length - 1)]);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return preparedArchives;
        }

        protected async Task<PreparedArchive> WaitAnswerAsync(string accId, int timeout)
        {
            do
            {
                var package = await _packageQueue.WaitAnyPackageAsync(timeout);

                switch (package.Type)
                {
                    case MsgType.Ping:
                        break;

                    case MsgType.Begin:
                        OnProcessMsgEvent($"Поиск в {package.SourceEndPoint}");
                        break;

                    case MsgType.FilesFound:
                        return CreatePreparedArchive(package);
                    
                    case MsgType.AccIdNotFoundError:
                        throw new AccIdNotFoundException(package.SourceEndPoint, accId);

                    case MsgType.Error:
                        var errorMsg = package.Deserialize<ErrorMsg>();
                        throw new Exception(errorMsg.Message);
                    
                    case MsgType.Text:
                        var textMsg = package.Deserialize<TextMsg>();
                        OnProcessMsgEvent(textMsg.Msg);
                        break;

                    default:
                        throw new Exception("Пришло не обработанное сообщение " + Enum.GetName(typeof(MsgType), package.Type));
                }
            } while (true);
        }

        private static PreparedArchive CreatePreparedArchive(Package package)
        {
            return new PreparedArchive(package.SourceEndPoint, JsonSerializer.Deserialize<FoundFilesMsg>(package.GetMessageText()));
        }

        protected virtual void OnProcessMsgEvent(string msg)
        {
            ProcessMsgEvent?.Invoke(this, msg);
        }

    }
}