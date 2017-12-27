using System.Threading.Tasks;
using MyApp.IntegrationTests.Grabber;

namespace MyApp.IntegrationTests.Communications
{
    public interface IQueueProvider
    {
        Task AddMessageAsync(Message message);

        Task<Message> ReceiveMessageAsync();

        Task<bool> DeleteMessageAsync(string messageReceipt);
    }
}