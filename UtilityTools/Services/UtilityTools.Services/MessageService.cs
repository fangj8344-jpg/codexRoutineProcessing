using UtilityTools.Services.Interfaces;

namespace UtilityTools.Services
{
    public class MessageService : IMessageService
    {
        public string GetMessage()
        {
            return "Hello from the Message Service";
        }
    }
}
