using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UpYun.SDK.Internal;
using UpYun.SDK.Model;

namespace UpYun.SDK
{
    public class UpYunClient
    {
        public string BucketName { get; }
        public string UserName { get; }
        public string Password { get; }

        public string ApiHost { get; } = "v0.api.upyun.com";

        static UpYunClient()
        {
        }
        public UpYunClient(string bucketName, string userName, string password, string apiHost = null)
        {
            this.BucketName = bucketName;
            this.UserName = userName;
            this.Password = password;
            if (!String.IsNullOrEmpty(apiHost))
                this.ApiHost = apiHost;
        }
        public async Task Upload(string path, byte[] datas, Dictionary<string, string> headers = null)
        {
            headers = headers ?? new Dictionary<string, string>();
            headers = new Dictionary<string, string>(headers, StringComparer.OrdinalIgnoreCase);
            path = "/" + this.BucketName + path;
            await this.Request(path, HttpMethod.Post, headers, datas);
        }
        public async Task<MulitUploadResModel> MulitUploadInitiate(string path, int fileSize, string fileMime, Dictionary<string, string> headers = null)
        {
            headers = headers ?? new Dictionary<string, string>();
            headers = new Dictionary<string, string>(headers, StringComparer.OrdinalIgnoreCase);
            headers["X-Upyun-Multi-Stage"] = "initiate";
            headers["X-Upyun-Multi-Type"] = fileMime;
            headers["X-Upyun-Multi-Length"] = fileSize.ToString();

            var url = "/" + this.BucketName + path;
            await this.Request(url, HttpMethod.Put, headers: headers, datas: new byte[] { });
            var result = new MulitUploadResModel()
            {
                FileId = headers["X-Upyun-Multi-Uuid"],
                PartId = headers["X-Upyun-Next-Part-Id"],
                PartSize = int.Parse(headers["X-Upyun-Next-Part-Size"])
            };
            return result;
        }

        public async Task<MulitUploadResModel> MulitUploadPart(string path, MulitUploadResModel model, byte[] datas)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                { "X-Upyun-Multi-Stage","upload"},
                { "X-Upyun-Multi-Uuid",model.FileId},
                { "X-Upyun-Part-Id",model.PartId}
            };
            var url = "/" + this.BucketName + path;
            await this.Request(url, HttpMethod.Put, headers, datas);
            var result = new MulitUploadResModel()
            {
                FileId = headers["X-Upyun-Multi-Uuid"],
                PartId = headers["X-Upyun-Next-Part-Id"],
                PartSize = int.Parse(headers["X-Upyun-Next-Part-Size"])
            };
            return result;
        }
        public async Task<MulitUploadResModel> MulitUploadComplete(string path, MulitUploadResModel model)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                { "X-Upyun-Multi-Stage","complete"},
                { "X-Upyun-Multi-Uuid",model.FileId}
            };
            var url = "/" + this.BucketName + path;
            await this.Request(url, HttpMethod.Put, headers, new byte[] { });
            var result = new MulitUploadResModel()
            {
                FileId = headers["X-Upyun-Multi-Uuid"]
            };
            return result;
        }

        public async Task<string> Request(string path, HttpMethod method, Dictionary<string, string> headers, byte[] datas = null)
        {
            var now = DateTime.UtcNow;
            string nowStr = now.ToString("ddd, dd MMM yyyy HH':'mm':'ss 'GMT'", CultureInfo.CreateSpecificCulture("en-US"));

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://" + this.ApiHost + path);
            request.Method = method.Method;
            request.KeepAlive = true;
            request.Date = now;

            if (datas != null)
                request.ContentLength = datas.Length;
            if (headers != null)
            {
                foreach (var item in headers)
                {
                    request.Headers[item.Key] = item.Value;
                }
            }

            var sign = "";
            var password = MD5Helper.GetMD5(this.Password);
            //sign = $"{password},{method.Method}&{path}&{nowStr}";
            //var signBytes = MD5Helper.GetMD5Bytes(sign);
            //sign = Convert.ToBase64String(signBytes);
            //request.Headers["Authorization"] = $"UpYun {this.UserName}:{sign}";

            //var baseAuth = this.UserName + ":" + this.Password;
            //baseAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes(baseAuth));
            //request.Headers["Authorization"] = $"Basic {baseAuth}";


            sign = $"{method.Method}&{path}&{nowStr}&{Math.Max(0, request.ContentLength)}&{password}";
            sign = MD5Helper.GetMD5(sign);
            request.Headers["Authorization"] = $"UpYun {this.UserName}:{sign}";

            if (datas != null)
            {
                using (var stream = await request.GetRequestStreamAsync())
                {
                    await stream.WriteAsync(datas, 0, datas.Length);
                }

            }

            try
            {
                using (var response = (HttpWebResponse)(await request.GetResponseAsync()))
                {
                    headers.Clear();
                    foreach (string key in response.Headers)
                    {
                        headers[key] = response.Headers[key];
                    }

                    using (var stream = response.GetResponseStream())
                    {
                        using (var streamReader = new StreamReader(stream, Encoding.UTF8))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                using (var stream = ex.Response.GetResponseStream())
                {
                    using (var streamReader = new StreamReader(stream, Encoding.UTF8))
                    {
                        var msg = streamReader.ReadToEnd();
                        throw new Exception("又拍云返回错误：" + (ex.Response as HttpWebResponse).StatusCode + "," + msg);
                    }
                }
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //public Task CreateDir()
    }
}
