using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace NagiBot {
    public class Tools {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static RequestResponse Request(RequestArgs e) {
            var res = new RequestResponse();

            try {
                var request = WebRequest.Create(e.URL) as HttpWebRequest;

                if (request == null) {
                    throw new Exception("Could not create web request");
                }

                request.Method = e.Method;

                if (e.Headers != null) {
                    foreach (var header in e.Headers) {
                        request.Headers.Add(header.Key, header.Value);
                    }
                }

                Stream stream;

                if (e.Payload != null) {
                    if (e.AutoAddJsonHeaderWithPayload &&
                        !request.Headers.AllKeys.Contains("Content-Type")) {

                        request.Headers.Add("Content-Type", "application/json; charset=utf-8");
                    }

                    var json = JsonConvert.SerializeObject(e.Payload);
                    var bytes = Encoding.UTF8.GetBytes(json);

                    stream = request.GetRequestStream();
                    stream.Write(bytes, 0, bytes.Length);
                }

                try {
                    var response = request.GetResponse() as HttpWebResponse;

                    if (response == null) {
                        throw new Exception("Could not get response");
                    }

                    res.StatusCode = response.StatusCode;

                    if (response.Headers != null) {
                        res.Headers = new Dictionary<string, string>();

                        foreach (var key in response.Headers.AllKeys) {
                            res.Headers.Add(key, response.Headers[key]);
                        }
                    }

                    stream = response.GetResponseStream();

                    if (stream == null) {
                        throw new Exception("Could not get stream from response");
                    }

                    var reader = new StreamReader(stream);

                    res.JSON = reader.ReadToEnd();
                }
                catch (WebException ex) {
                    var response = ex.Response as HttpWebResponse;

                    if (response == null) {
                        throw new Exception("Could not get response");
                    }

                    res.StatusCode = response.StatusCode;

                    if (response.Headers != null) {
                        res.Headers = new Dictionary<string, string>();

                        foreach (var key in response.Headers.AllKeys) {
                            res.Headers.Add(key, response.Headers[key]);
                        }
                    }

                    stream = response.GetResponseStream();

                    if (stream == null) {
                        throw new Exception("Could not get stream from response");
                    }

                    var reader = new StreamReader(stream);

                    res.JSON = reader.ReadToEnd();
                }
            }
            catch (Exception ex) {
                res.Exception = ex;
            }

            return res;
        }

        #region Helper classes

        public class RequestArgs {
            public string URL { get; set; }
            public string Method = "GET";
            public object Payload { get; set; }
            public Dictionary<string, string> Headers { get; set; }
            public bool AutoAddJsonHeaderWithPayload = true;
        }

        public class RequestResponse {
            public HttpStatusCode StatusCode { get; set; }
            public string JSON { get; set; }
            public Dictionary<string, string> Headers { get; set; }
            public Exception Exception { get; set; }

            public T CastTo<T>() {
                return JsonConvert.DeserializeObject<T>(this.JSON);
            }
        }

        #endregion
    }
}