namespace MyApp.IntegrationTests.Grabber
{
    public class Message
    {
        public Message(string content)
        {
            this.Content = content;
        }

        private string Name { get; set; }

        public string Content { get; }
    }
}