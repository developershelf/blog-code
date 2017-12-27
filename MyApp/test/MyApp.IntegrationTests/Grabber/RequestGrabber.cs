using System.Collections.Generic;

namespace MyApp.IntegrationTests.Grabber
{
    /// <inheritdoc />
    /// <summary>
    /// </summary>
    public class RequestGrabber : IRequestGrabber
    {
        private readonly Dictionary<string, RequestData> data;
        private readonly Dictionary<string, int> counter;

        public RequestGrabber()
        {
            data = new Dictionary<string, RequestData>();
            counter = new Dictionary<string, int>();
        }

        public (bool hasPayload, RequestData payload) GetPayload(string key)
        {
            var result = this.data.TryGetValue(key.ToUpper(), out var request);

            return (result, request);
        }

        public List<string> GetKeys()
        {
            return new List<string>(this.data.Keys);
        }

        public void AddRequest(string key, string message)
        {
            var request = new RequestData
            {
                Body = message
            };

            InsertData(key, request);
        }

        public void AddRequest(string key, Dictionary<string, string> headers, string message)
        {
            var request = new RequestData
            {
                Headers = headers,
                Body = message
            };

            InsertData(key, request);
        }

        protected virtual void InsertData(string key, RequestData requestData)
        {
            var count = 1;
            var result = this.counter.ContainsKey(key);
            if (result)
            {
                count = this.counter[key];
                count++;
            }

            this.data.Add($"{key.ToUpper()}-{count}", requestData);
            this.counter[key] = count;
        }

        public void Reset()
        {
            this.data.Clear();
            this.counter.Clear();
        }
    }
}