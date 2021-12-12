using System;
using System.Threading.Tasks;

namespace Cyberliberty.Guzzle
{
    /// <summary>
    /// 
    /// </summary>
    public interface IGuzzle
    {

        #region Public Properties

        /// <summary>
        /// </summary>
        string Url { get; set; }

        /// <summary>
        /// </summary>
        bool AllowAutoRedirect { get; set; }

        /// <summary>
        /// </summary>
        bool UseProxy { get; set; }

        /// <summary>
        /// </summary>
        string Proxy { get; set; }

        /// <summary>
        /// </summary>
        bool BypassProxyOnLocal { get; set; }

        #endregion

        #region Utility Methods

        /// <summary>
        /// </summary>
        /// <param name="argName"></param>
        /// <param name="argValue"></param>
        void AddHeader(string argName, string argValue);

        /// <summary>
        /// </summary>
        /// <param name="argName"></param>
        /// <param name="argValue"></param>
        void AddParam(string argName, string argValue);

        /// <summary>
        /// </summary>
        /// <param name="argName">the param name.</param>
        /// <param name="argValue">the param value.</param>
        void AddJson(string argName, object argValue);

        /// <summary>
        /// </summary>
        /// <param name="argBody">the body content</param>
        void RawBody(string argBody);

        /// <summary>
        /// </summary>
        /// <param name="argFilename"></param>
        void AddFile(string argName, string argFilename);

        /// <summary>
        /// </summary>
        /// <returns></returns>
        Response GetResponse();

        #endregion

        #region Public Methods

        /// <summary>
        /// </summary>
        Response Head();

        /// <summary>
        /// </summary>
        /// <returns></returns>
        Task<Response> HeadAsync();

        /// <summary>
        /// </summary>
        /// <returns></returns>
        void HeadAsync(Action<Response> callback);

        /// <summary>
        /// </summary>
        Response Get();

        /// <summary>
        /// </summary>
        /// <returns></returns>
        Task<Response> GetAsync();

        /// <summary>
        /// </summary>
        /// <returns></returns>
        void GetAsync(Action<Response> callback);

        /// <summary>
        /// </summary>
        Response Post();

        /// <summary>
        /// </summary>
        /// <returns></returns>
        Task<Response> PostAsync();

        /// <summary>
        /// </summary>
        /// <returns></returns>
        void PostAsync(Action<Response> callback);

        #endregion

    }
}
