using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using NLog;

namespace AutoMH.Auth.Network
{
    internal class HttpClient
    {
        /// <summary>
        /// Maximum reconnect attempts
        /// </summary>
        private const int MaxReconnectAttempts = 0;

        private CookieContainer _cookies;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public HttpClient()
        {
            _cookies = new CookieContainer();
        }

        public bool IsDebug { get; set; }

        /// <summary>
        /// Send a GET request to a web page. Returns the contents of the page.
        /// </summary>
        /// <param name="url">The address to GET.</param>
        public string Get(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.CookieContainer = _cookies;
            var response = request.GetResponse();
            var responseStream = response.GetResponseStream();
            if (null == responseStream)
            {
                throw new Exception();
            }

            var sr = new StreamReader(responseStream, Encoding.UTF8);
            var result = sr.ReadToEnd();
            sr.Close();
            response.Close();

            return result;
        }

        public string Post(string url, List<KeyValuePair<string, string>> data)
        {
            return Request("post", url, data, true);
        }

        /// <summary>
        ///     Making request
        /// </summary>
        /// <param name="method">Request method</param>
        /// <param name="uri">Request url</param>
        /// <param name="data">Request data</param>
        /// <param name="isNeedToRedirect">Indicates whether is need to redirect</param>
        /// <param name="additionHeaders">Additional request headers</param>
        /// <param name="referrer">Request referrer</param>
        /// <returns>Obtained html page</returns>
        private string Request(
            string method,
            string uri,
            List<KeyValuePair<string, string>> data = null,
            bool isNeedToRedirect = false,
            Dictionary<string, string> additionHeaders = null,
            string referrer = "")
        {
            int reconnectAttempt = 0;

            while (true)
            {
                try
                {
                    if (IsDebug)
                    {
                        var append = "XDEBUG_SESSION_START=xdebug";
                        if (uri.IndexOf('?') > 0)
                        {
                            append = '&' + append;
                        }
                        else
                        {
                            append = '?' + append;
                        }
                        uri += append;
                    }
                    var request = (HttpWebRequest)WebRequest.Create(uri);
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.AllowAutoRedirect = isNeedToRedirect;
                    var uriObj = new Uri(uri);
                    request.Host = uriObj.Host;
                    request.CookieContainer = _cookies;

                    if (null != additionHeaders)
                    {
                        foreach (var additionHeader in additionHeaders)
                        {
                            request.Headers.Add(additionHeader.Key, additionHeader.Value);
                        }
                    }

                    if (method == "GET")
                    {
                        Logger.Debug("Getting uri [{0}]", uri);
                        request.Method = "GET";
                    }
                    else
                    {
                        Logger.Debug("Posting uri [{0}]", uri);
                        request.Method = "post";
                        var stringBuilder = new StringBuilder();
                        if (null != data)
                        {
                            foreach (var keyValue in data)
                            {
                                if (string.IsNullOrEmpty(keyValue.Key) || string.IsNullOrEmpty(keyValue.Value))
                                {
                                    continue;
                                }

                                if (0 != stringBuilder.Length)
                                {
                                    stringBuilder.Append("&");
                                }

                                stringBuilder.Append(keyValue.Key);
                                stringBuilder.Append("=");
                                stringBuilder.Append(HttpUtility.UrlEncode(keyValue.Value, Encoding.UTF8));
                            }
                        }

                        byte[] postData = Encoding.GetEncoding(1251).GetBytes(stringBuilder.ToString());
                        request.ContentLength = postData.Length;

                        using (Stream sendStream = request.GetRequestStream())
                        {
                            sendStream.Write(postData, 0, postData.Length);
                        }
                    }

                    var response = (HttpWebResponse)request.GetResponse();

                    using (Stream stream = response.GetResponseStream())
                    {
                        if (null == stream)
                        {
                            throw new Exception();
                        }

                        using (var streamReader = new StreamReader(stream))
                        {
                            string result = streamReader.ReadToEnd();

                            string realLocation = response.Headers["Location"];
                            if (isNeedToRedirect)
                            {
                                if (null != realLocation && "\\" != realLocation)
                                {
                                    return Request("GET", realLocation, null, isNeedToRedirect);
                                }
                            }

                            //Thread.Sleep(
                            //    Randomizer.GetRandom(TimeoutConfig.DecisionTimeOutMin, TimeoutConfig.DecisionTimeOutMax));

                            return result;
                        }
                    }
                }
                catch (WebException exception)
                {
                    reconnectAttempt++;
                    if (reconnectAttempt > MaxReconnectAttempts)
                    {
                        using (WebResponse response = exception.Response)
                        {
                            HttpWebResponse httpResponse = (HttpWebResponse)response;
                            Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                            using (var dataStream = response.GetResponseStream())
                            using (var reader = new StreamReader(dataStream))
                            {
                                var text = reader.ReadToEnd();
                                //Console.WriteLine(text);
                            }
                        }
                        // TODO: Raise event
                        //return null;
                        throw;
                    }
                    Logger.ErrorException(
                        string.Format(
                            "Got exception during calling uri [{0}], reconnect attempt {1}",
                            uri,
                            reconnectAttempt),
                        exception);
                    // Thread.Sleep(Randomizer.GetRandom(TimeoutConfig.NetworkTimeOutMin, TimeoutConfig.NetworkTimeOutMax));
                }
            }
        }

    }
}
