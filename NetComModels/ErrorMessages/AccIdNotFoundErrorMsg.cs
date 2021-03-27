using NetComModels.Messages;

namespace NetComModels.ErrorMessages
{
    public class AccIdNotFoundErrorMsg : ErrorMsg
    {
        public string AccId { get; set; }

        public AccIdNotFoundErrorMsg()
        {
        }

        public AccIdNotFoundErrorMsg(string accId) 
            : base(accId + " не найден", MsgType.AccIdNotFoundError)
        {
            AccId = accId;
        }
    }
}