using System.Collections.Generic;

namespace Cyberliberty.Guzzle
{
    /// <summary>
    /// 
    /// </summary>
    public class Response
    {

        /// <summary>
        /// 
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Method { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string HttpVersion { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        
        /// <summary>
        /// 
        /// </summary>
        public string Content { get; set; }
        public double TimeTaken { get; internal set; }

        /// <summary>
        /// Initialize a new instance of response.
        /// </summary>
        public Response() {}

        /// <summary>
        /// 
        /// </summary>
        public Response(string argUrl, Dictionary<string, string> argHeaders, string argContent)
        {
            Url = argUrl;
            Headers = argHeaders;
            Content = argContent;
        }

    }
}
