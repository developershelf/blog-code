using System;
using System.Threading.Tasks;
using MyApp.IntegrationTests.Grabber;

namespace MyApp.IntegrationTests.Communications
{
    public class ServiceBusQueueProvider : IQueueProvider
    {
        public Task AddMessageAsync(Message message)
        {
            throw new NotImplementedException();
        }

        public Task<Message> ReceiveMessageAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteMessageAsync(string messageReceipt)
        {
            throw new NotImplementedException();
        }
    }
}