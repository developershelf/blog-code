using System.Collections.Generic;

namespace MyApp.IntegrationTests.Grabber
{
    /// <summary>
    /// Holds a request, body and any headers/message attributes
    /// </summary>
    public class RequestData
    {
        public RequestData()
        {
            this.Headers = new Dictionary<string, string>();
        }

        public Dictionary<string, string> Headers { get; set; }

        public string Body { get; set; }
    }
}