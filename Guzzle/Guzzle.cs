#region Libraries
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Cache;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
#endregion

namespace Cyberliberty.Guzzle
{
    /// <summary>
    /// A simplified wrapper class for performing http requests.
    /// Supported methods: HEAD, GET, POST
    /// </summary>
    public class Guzzle : IGuzzle
    {

        #region Private Members

        /// <summary>
        /// </summary>
        private HttpWebRequest mRequest;

        /// <summary>
        /// </summary>
        private Response mResponse;

        /// <summary>
        /// </summary>
        private RequestBodyType BodyType = RequestBodyType.NONE;

        /// <summary>
        /// A property holding body data
        /// </summary>
        private Dictionary<string, string> mParams = new Dictionary<string, string>();

        /// <summary>
        /// A property holding json body
        /// </summary>
        private Dictionary<string, object> mJsonBody = new Dictionary<string, object>();

        /// <summary>
        /// A property holding raw body.
        /// </summary>
        private string mRawBody = string.Empty;

        /// <summary>
        /// A property holding files.
        /// </summary>
        private Dictionary<string, string> mFiles = new Dictionary<string, string>();

        /// <summary>
        /// A list of error messages besides the ones sent by target server.
        /// </summary>
        private List<string> ErrorMessages = new List<string>() {
            "The remote name could not be resolved", "Unable to connect to the remote server",
            "The proxy name could not be resolved", "The underlying connection was closed"
        };

        #endregion

        #region Private Properties

        /// <summary>
        /// Gets or sets request headers.
        /// </summary>
        private Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets request method.
        /// </summary>
        private string Method { get; set; }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the request url.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets whether to allow auto redirections.
        /// </summary>
        public bool AllowAutoRedirect { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to use proxy.
        /// </summary>
        public bool UseProxy { get; set; }

        /// <summary>
        /// Gets or sets the proxy to use with the request.
        /// </summary>
        public string Proxy { get; set; }

        /// <summary>
        /// Gets or sets whether to bypass proxy for local address urls.
        /// </summary>
        public bool BypassProxyOnLocal { get; set; }
        
        #endregion

        #region Public Events

        public delegate void OnResponse(Response response);
        public delegate void ErrorOccured(string reason);

        /// <summary>
        /// Triggers when a reponse is received.
        /// </summary>
        public event OnResponse ResponseReceived;

        /// <summary>
        /// </summary>
        /// <param name="reason"></param>

        /// <summary>
        /// Triggers when an error occurs.
        /// </summary>
        public event ErrorOccured OnError;

        #endregion

        #region Private Methods

        /// <summary>
        /// Generate a HttpWebRequest object.
        /// </summary>
        /// <param name="argMethod">The request method to be used.</param>
        private void Make(string argMethod = "GET")
        {
            try
            {
                /* instantiate a http request */
                mRequest = (HttpWebRequest)WebRequest.Create(Url);

                /* set method and headers (if any) */
                mRequest.Method = argMethod;
                mRequest.Accept = "*/*";
                mRequest.AllowAutoRedirect = AllowAutoRedirect;
                mRequest.AuthenticationLevel = AuthenticationLevel.None;
                mRequest.AutomaticDecompression = DecompressionMethods.Deflate;
                mRequest.CachePolicy = new RequestCachePolicy(RequestCacheLevel.Revalidate);

                /* if SSL */
                if (Url.StartsWith("https"))
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    ServicePointManager.ServerCertificateValidationCallback = AcceptAllCerts;
                    mRequest.ClientCertificates.Add(new X509Certificate());
                }

                /* use proxy if needed */
                if (UseProxy)
                    mRequest.Proxy = new WebProxy(Proxy, BypassProxyOnLocal);

                foreach (var header in Headers)
                {
                    if (header.Key == "User-Agent") { mRequest.UserAgent = header.Value; continue; }
                    if (header.Key == "Content-Type") { mRequest.ContentType = header.Value; continue; }
                    if (header.Key == "Content-Length") { mRequest.ContentLength = long.Parse(header.Value); continue; }
                    if (header.Key == "Host") { mRequest.Host = header.Value; continue; }
                    if (header.Key == "Referer") { mRequest.Referer = header.Value; continue; }
                    if (header.Key == "Transfer-Encoding") { mRequest.TransferEncoding = header.Value; continue; }
                    mRequest.Headers.Add(header.Key, header.Value);
                }
            }
            catch (Exception ex) { ParseException(ex); }
        }

        /// <summary>
        /// </summary>
        private void Submit()
        {
            try
            {
                /* get response */
                HttpWebResponse _response = (HttpWebResponse)mRequest.GetResponse();

                mResponse = new Response()
                {
                    Url = Url,
                    HttpVersion = _response.ProtocolVersion.ToString(),
                    Method = mRequest.Method,
                    Status = _response.StatusCode.GetHashCode(),
                    Content = new StreamReader(_response.GetResponseStream()).ReadToEnd(),
                };

                /* collect headers */
                foreach (string header in _response.Headers)
                    mResponse.Headers.Add(header, _response.Headers[header]);

                /* trigger event */
                if (ResponseReceived != null) ResponseReceived(mResponse);
            }
            catch (Exception ex)
            {
                mResponse = ParseException(ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private async Task<Response> SubmitAsync()
        {
            try
            {
                /* get response */
                HttpWebResponse _response = (HttpWebResponse)(await mRequest.GetResponseAsync());

                mResponse = new Response()
                {
                    Url = Url,
                    HttpVersion = _response.ProtocolVersion.ToString(),
                    Method = mRequest.Method,
                    Status = _response.StatusCode.GetHashCode(),
                    Content = new StreamReader(_response.GetResponseStream()).ReadToEnd(),
                };

                /* collect headers */
                foreach (string header in _response.Headers)
                    mResponse.Headers.Add(header, _response.Headers[header]);

                /* trigger event */
                if (ResponseReceived != null) ResponseReceived(mResponse);

                /* return response */
                return mResponse;
            }
            catch (Exception ex) { return ParseException(ex); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns></returns>
        private bool AcceptAllCerts
            (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ex"></param>
        private Response ParseException(Exception ex)
        {
            /* is connection related error? */
            var isNonServerError = ErrorMessages.Where(errMsg => errMsg.StartsWith(ex.Message)).Count() > 0;
            if (isNonServerError)
            {
                if (OnError != null)
                    OnError(ex.Message);
                return null;
            }

            try
            {
                var _response = ((HttpWebResponse)(
                    (WebException)ex.GetBaseException()).Response
                );

                Response response = new Response {
                    Url = Url,
                    Status = _response.StatusCode.GetHashCode(),
                    Method = _response.Method.ToUpper(),
                    Content = new StreamReader(_response.GetResponseStream()).ReadToEnd(),
                };

                /* response headers */
                foreach (string h in _response.Headers.Keys)
                    response.Headers.Add(h, _response.Headers[h]);

                /* return response */
                return response;
            }
            catch (Exception) { return null; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="includeParams"></param>
        private void WriteFilesToStream(Stream stream, bool includeParams = false)
        {
            /* set boundry */
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = Encoding.ASCII.GetBytes(
                string.Format("\r\n--{0}\r\n", boundary)
            );

            /* change request content-type */
            mRequest.ContentType = string.Format("multipart/form-data; boundary={0}", boundary);
            mRequest.KeepAlive = true;

            /* write params to stream */
            if (includeParams)
            {
                mParams.ToList().ForEach(param =>
                {
                    stream.Write(boundarybytes, 0, boundarybytes.Length);
                    string newParam = string.Format(
                        "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}", param.Key, param.Value
                    );
                    byte[] paramBuffer = Encoding.UTF8.GetBytes(newParam);
                    stream.Write(paramBuffer, 0, paramBuffer.Length);
                });
            }

            /* opening boundary */
            stream.Write(boundarybytes, 0, boundarybytes.Length);

            /* header something */
            string headerTemplate = string.Empty;
            string header = "";

            headerTemplate += "Content-Disposition: ";
            headerTemplate += "form-data; name=\"{0}\"; ";
            headerTemplate += "filename=\"{1}\"\r\n";
            headerTemplate += "Content-Type: {2}\r\n\r\n";

            /* add files' buffer */
            mFiles.ToList().ForEach(file =>
            {
                /* file headers */
                var cType = new System.Net.Mime.ContentType() { Name = Path.GetFileName(file.Value) }.ToString();
                header = string.Format(headerTemplate, file.Key, Path.GetFileName(file.Value), cType);

                /* file buffer */
                byte[] headerbytes = Encoding.UTF8.GetBytes(header);
                stream.Write(headerbytes, 0, headerbytes.Length);

                /* write file buffer to the stream */
                using (FileStream fileStream = new FileStream(file.Value, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead = 0;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                        stream.Write(buffer, 0, bytesRead);
                }

                /* closing boundary */
                stream.Write(boundarybytes, 0, boundarybytes.Length);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="includeParams"></param>
        /// <returns></returns>
        private async Task WriteFilesToStreamAsync(Stream stream, bool includeParams = false)
        {
            /* set boundry */
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = Encoding.ASCII.GetBytes(
                string.Format("\r\n--{0}\r\n", boundary)
            );

            /* change request content-type */
            mRequest.ContentType = string.Format("multipart/form-data; boundary={0}", boundary);
            mRequest.KeepAlive = true;

            /* write params to stream */
            if (includeParams)
            {
                mParams.ToList().ForEach(async (param) =>
                {
                    await stream.WriteAsync(boundarybytes, 0, boundarybytes.Length);
                    string newParam = string.Format(
                        "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}", param.Key, param.Value
                    );
                    byte[] paramBuffer = System.Text.Encoding.UTF8.GetBytes(newParam);
                    await stream.WriteAsync(paramBuffer, 0, paramBuffer.Length);
                });
            }

            /* closing boundary */
            await stream.WriteAsync(boundarybytes, 0, boundarybytes.Length);

            /* header something */
            string headerTemplate = string.Empty;
            string header = "";

            headerTemplate += "Content-Disposition: ";
            headerTemplate += "form-data; name=\"{0}\"; ";
            headerTemplate += "filename=\"{1}\"\r\n";
            headerTemplate += "Content-Type: {2}\r\n\r\n";

            /* add files' buffer */
            mFiles.ToList().ForEach(async (file) =>
            {
                var cType = new System.Net.Mime.ContentType() { Name = Path.GetFileName(file.Value) }.ToString();
                header = string.Format(headerTemplate, file.Key, Path.GetFileName(file.Value), cType);

                /* header bytes */
                byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
                await stream.WriteAsync(headerbytes, 0, headerbytes.Length);

                /* file bytes */
                using (FileStream fileStream = new FileStream(file.Value, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead = 0;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                        stream.Write(buffer, 0, bytesRead);
                }

                /* closing boundary */
                await stream.WriteAsync(boundarybytes, 0, boundarybytes.Length);
            });
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Add a header to the request.
        /// </summary>
        /// <param name="argName">the header name.</param>
        /// <param name="argValue">the header value.</param>
        public void AddHeader(string argName, string argValue)
        {
            if (Headers == null)
                Headers = new Dictionary<string, string>();

            /* add header */
            Headers.Add(argName, argValue);
        }

        /// <summary>
        /// Add new form data param.
        /// </summary>
        /// <param name="argName">The parameter name.</param>
        /// <param name="argValue">The parameter value.</param>
        public void AddParam(string argName, string argValue)
        {
            if (mParams == null)
                mParams = new Dictionary<string, string>();

            /* add entry */
            mParams.Add(argName, argValue);
            BodyType = (mFiles.Count > 0) ? RequestBodyType.PARAMETERS_AND_FILES : RequestBodyType.PARAMETERS;
        }

        /// <summary>
        /// Add a json body param
        /// </summary>
        /// <param name="argName">the param name.</param>
        /// <param name="argValue">the param value.</param>
        public void AddJson(string argName, object argValue)
        {
            if (mJsonBody == null)
                mJsonBody = new Dictionary<string, object>();

            /* add json entry */
            mJsonBody.Add(argName, argValue);
            BodyType = RequestBodyType.JSON;
        }

        /// <summary>
        /// Set raw body content.
        /// </summary>
        /// <param name="argBody">the body content</param>
        public void RawBody(string argBody)
        {
            mRawBody = argBody;
            BodyType = RequestBodyType.RAW;
        }

        /// <summary>
        /// Add a file to be uploaded.
        /// </summary>
        /// <param name="argName">the parameter name.</param>
        /// <param name="argFilename">the parameter value.</param>
        public void AddFile(string argName, string argFilename)
        {
            if (mFiles == null)
                mFiles = new Dictionary<string, string>();

            /* add entry */
            mFiles.Add(argName, argFilename);
            BodyType = (mParams.Count > 0) ? RequestBodyType.PARAMETERS_AND_FILES : RequestBodyType.FILES;
        }

        /// <summary>
        /// Get the <see cref="Response"/> object of the request sent.
        /// </summary>
        /// <returns></returns>
        public Response GetResponse() => mResponse;

        #endregion

        #region Public Methods

        /// <summary>
        /// Send a head request.
        /// </summary>
        /// <returns>returns a <see cref="Response"/> object.</returns>
        public Response Head()
        {
            /* generate request object */
            Make(WebRequestMethods.Http.Head);

            /* submit */
            Submit();

            /* return response */
            return mResponse;
        }

        /// <summary>
        /// Send an asynchronous head request.
        /// </summary>
        /// <returns>returns a <see cref="Response"/>.</returns>
        public async Task<Response> HeadAsync()
        {
            /* generate request object */
            Make(WebRequestMethods.Http.Head);

            /* submit */
            return await SubmitAsync();
        }

        /// <summary>
        /// Send an asynchronous head request.
        /// </summary>
        /// <param name="callback">the response callback</param>
        public async void HeadAsync(Action<Response> callback)
        {
            /* generate request object */
            Make(WebRequestMethods.Http.Head);

            /* submit */
            callback(await SubmitAsync());
        }

        /// <summary>
        /// Send a get request.
        /// </summary>
        /// <returns>returns a <see cref="Response"/> object.</returns>
        public Response Get()
        {
            /* build request */
            Make(WebRequestMethods.Http.Get);

            /* submit */
            Submit();

            /* return */
            return mResponse;
        }

        /// <summary>
        /// Send an asynchronous get request.
        /// </summary>
        /// <returns>returns a <see cref="Response"/>.</returns>
        public async Task<Response> GetAsync()
        {
            /* generate request object */
            Make(WebRequestMethods.Http.Get);

            /* submit */
            return await SubmitAsync();
        }

        /// <summary>
        /// Send an asynchronous get request.
        /// </summary>
        /// <param name="callback">response callback</param>
        public async void GetAsync(Action<Response> callback)
        {
            /* generate request object */
            Make(WebRequestMethods.Http.Get);

            /* submit */
            callback(await SubmitAsync());
        }

        /// <summary>
        /// Send a post request.
        /// </summary>
        /// <returns>returns a <see cref="Response"/>.</returns>
        public Response Post()
        {
            /* generate request object */
            Make(WebRequestMethods.Http.Post);

            /* add body/files if any */
            using (Stream stream = mRequest.GetRequestStream())
            {
                /* Raw Body Content */
                if (BodyType == RequestBodyType.RAW)
                {
                    byte[] _bufferByte = Encoding.UTF8.GetBytes(mRawBody);
                    stream.Write(_bufferByte, 0, _bufferByte.Length);
                }

                /* Json Content */
                if (BodyType == RequestBodyType.JSON)
                {

                    string jsonBody = string.Empty;
                    string[] jsonProps = mJsonBody.ToList()
                        .ConvertAll<string>(jPair => $"\"{jPair.Key}\":\"{jPair.Value}\"").ToArray();

                    byte[] _bufferByte = Encoding.UTF8.GetBytes(string.Concat("{", string.Join(", ", jsonProps), "}"));
                    stream.Write(_bufferByte, 0, _bufferByte.Length);
                }

                /* Body Params */
                if (BodyType == RequestBodyType.PARAMETERS)
                {
                    string[] _bufferPairs = mParams.ToList()
                    .ConvertAll<string>(dPair => string.Concat(dPair.Key, "=", dPair.Value))
                    .ToArray();

                    byte[] _bufferByte = Encoding.UTF8.GetBytes(string.Join("&", _bufferPairs));
                    stream.Write(_bufferByte, 0, _bufferByte.Length);
                }

                /* Body Files */
                if (BodyType == RequestBodyType.FILES) WriteFilesToStream(stream);

                /* Body Params and Files */
                if (BodyType == RequestBodyType.PARAMETERS_AND_FILES) WriteFilesToStream(stream, true);

                /* flush stream */
                stream.Flush();
            }

            /* submit request */
            Submit();

            /* return */
            return mResponse;
        }

        /// <summary>
        /// Send an asynchronous post request.
        /// </summary>
        /// <returns>returns a <see cref="Response"/>.</returns>
        public async Task<Response> PostAsync()
        {
            /* generate request object */
            Make(WebRequestMethods.Http.Post);

            /* add body/files if any */
            using (Stream stream = await mRequest.GetRequestStreamAsync())
            {
                var check = mParams.Count() > 0 && mFiles.Count() > 0;

                /* Body and Files */
                if (check) await WriteFilesToStreamAsync(stream, true);

                /* Raw */
                if (BodyType == RequestBodyType.RAW)
                {
                    byte[] _bufferByte = Encoding.UTF8.GetBytes(mRawBody);
                    stream.Write(_bufferByte, 0, _bufferByte.Length);
                }

                /* Json */
                if (BodyType == RequestBodyType.JSON)
                {
                    string jsonBody = string.Empty;
                    string[] jsonProps = mJsonBody.ToList()
                        .ConvertAll<string>(jPair => $"{jPair.Key}: {jPair.Value}").ToArray();

                    byte[] _bufferByte = Encoding.UTF8.GetBytes(string.Concat("{ ", string.Join(", ", jsonProps), " }"));
                    stream.Write(_bufferByte, 0, _bufferByte.Length);
                }

                /* Params */
                if (BodyType == RequestBodyType.PARAMETERS)
                {
                    string[] _bufferPairs = mParams.ToList()
                    .ConvertAll<string>(dPair => string.Concat(dPair.Key, "=", dPair.Value))
                    .ToArray();

                    byte[] _bufferByte = Encoding.UTF8.GetBytes(string.Join("&", _bufferPairs));
                    stream.Write(_bufferByte, 0, _bufferByte.Length);
                }

                /* Files */
                if (BodyType == RequestBodyType.FILES) await WriteFilesToStreamAsync(stream);

                /* Params and Files */
                if (BodyType == RequestBodyType.PARAMETERS_AND_FILES) await WriteFilesToStreamAsync(stream, true);

                /* flush stream */
                stream.Flush();
            }

            /* submit */
            return (await SubmitAsync());
        }

        /// <summary>
        /// Send an asynchronous post request.
        /// </summary>
        /// <param name="callback">the response callback.</param>
        public async void PostAsync(Action<Response> callback)
        {
            /* generate request object */
            Make(WebRequestMethods.Http.Post);

            /* add body/files if any */
            using (Stream stream = await mRequest.GetRequestStreamAsync())
            {
                var check = mParams.Count() > 0 && mFiles.Count() > 0;

                /* Body and Files */
                if (check) await WriteFilesToStreamAsync(stream, true);

                /* Raw */
                if (BodyType == RequestBodyType.RAW)
                {
                    byte[] _bufferByte = Encoding.UTF8.GetBytes(mRawBody);
                    stream.Write(_bufferByte, 0, _bufferByte.Length);
                }

                /* Json */
                if (BodyType == RequestBodyType.JSON)
                {
                    string jsonBody = string.Empty;
                    string[] jsonProps = mJsonBody.ToList()
                        .ConvertAll<string>(jPair => $"{jPair.Key}: {jPair.Value}").ToArray();

                    byte[] _bufferByte = Encoding.UTF8.GetBytes(string.Concat("{ ", string.Join(", ", jsonProps), " }"));
                    stream.Write(_bufferByte, 0, _bufferByte.Length);
                }

                /* Params */
                if (BodyType == RequestBodyType.PARAMETERS)
                {
                    string[] _bufferPairs = mParams.ToList()
                    .ConvertAll<string>(dPair => string.Concat(dPair.Key, "=", dPair.Value))
                    .ToArray();

                    byte[] _bufferByte = Encoding.UTF8.GetBytes(string.Join("&", _bufferPairs));
                    stream.Write(_bufferByte, 0, _bufferByte.Length);
                }

                /* Files */
                if (BodyType == RequestBodyType.FILES) await WriteFilesToStreamAsync(stream);

                /* Params and Files */
                if (BodyType == RequestBodyType.PARAMETERS_AND_FILES) await WriteFilesToStreamAsync(stream, true);

                /* flush stream */
                stream.Flush();
            }

            /* submit */
            callback(await SubmitAsync());
        }

        #endregion

        #region Constructors
        
        /// <summary>
        /// Initialize new instance of <see cref="Guzzle"/>.
        /// </summary>
        public Guzzle() { }

        /// <summary>
        /// Initialize new instance of <see cref="Guzzle"/>.
        /// </summary>
        /// <param name="argUrl">the target url.</param>
        public Guzzle(string argUrl) { Url = argUrl; }

        #endregion

    }
}
