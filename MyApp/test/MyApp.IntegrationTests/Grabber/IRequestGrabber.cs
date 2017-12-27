using System.Collections.Generic;

namespace MyApp.IntegrationTests.Grabber
{
    /// <summary>
    /// Interface for a message tester which retains the payload for use when comparing output data.
    /// </summary>
    public interface IRequestGrabber
    {
        /// <summary>
        /// Gets any data that was sent and saved under a specific key in the internal data structure
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        (bool hasPayload, RequestData payload) GetPayload(string key);

        /// <summary>
        /// Gets all the keys used in the internal data structure
        /// </summary>
        /// <returns></returns>
        List<string> GetKeys();

        /// <summary>
        /// Adds a message to the internal data structure under a specific key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="message"></param>
        void AddRequest(string key, string message);

        /// <summary>
        /// Adds a message to the internal data structure under a specific key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="headers"></param>
        /// <param name="message"></param>
        void AddRequest(string key, Dictionary<string, string> headers, string message);

        /// <summary>
        /// Resets the grabber
        /// </summary>
        void Reset();
    }
}