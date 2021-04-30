using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Web;
using System.Threading;
using System.Collections.Specialized;
using HtmlAgilityPack;
using System.Drawing;

namespace Links
{
    public class HttpUtils
    {
        public static string Get(string Url)
        {
            //System.GC.Collect();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Proxy = null;
            request.KeepAlive = false;
            request.Method = "GET";
            request.ContentType = "application/json; charset=UTF-8";
            request.AutomaticDecompression = DecompressionMethods.GZip;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);
            string retString = myStreamReader.ReadToEnd();

            myStreamReader.Close();
            myResponseStream.Close();

            if (response != null)
            {
                response.Close();
            }
            if (request != null)
            {
                request.Abort();
            }

            return retString;
        }

        public static Image GetVercode(string Url)
        {
            //System.GC.Collect();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Proxy = null;
            request.KeepAlive = false;
            request.Method = "GET";
            //request.ContentType = "application/json; charset=UTF-8";
            //request.AutomaticDecompression = DecompressionMethods.GZip;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            Image rImage = Image.FromStream(myResponseStream, true);
            myResponseStream.Close();

            if (response != null)
            {
                response.Close();
            }
            if (request != null)
            {
                request.Abort();
            }

            return rImage;
        }

        /// <summary>
        /// 默认表单提交
        /// </summary>
        /// <param name="requestUri">提交路径</param>
        /// <param name="postData">提交数据</param>
        /// <param name="cookie">Cookie容器对象</param>
        /// <returns>字符串结果</returns>
        public static string PostForm(string requestUri, NameValueCollection postData)
        {
            string cookie = string.Empty;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
                request.Method = "post";
                request.ContentType = "application/x-www-form-urlencoded";
                //request.CookieContainer = cookie;
                request.KeepAlive = false;
                request.Timeout = 15000;
                StringBuilder stringBuilder = new StringBuilder();
                foreach (string key in postData.Keys)
                {
                    stringBuilder.AppendFormat("&{0}={1}", key, postData.Get(key));
                }
                byte[] buffer = Encoding.UTF8.GetBytes(stringBuilder.ToString().Trim('&'));
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(buffer, 0, buffer.Length);
                requestStream.Close();
                WebResponse response = request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string html = reader.ReadToEnd();//读取数据
                if (!string.IsNullOrEmpty(html) && html.Contains("登录成功"))
                    cookie = response.Headers.Get("Set-Cookie").Split(';')[0];
            }
            catch { }
            return cookie;
        }

        /// <summary>
        /// 提交
        /// </summary>
        /// <param name="requestUri"></param>
        /// <param name="postData"></param>
        /// <returns></returns>
        public static bool LinkPost(string requestUri, NameValueCollection postData, CookieContainer cc)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = "post";
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = cc;
            request.Timeout = 20000;
            request.KeepAlive = false;
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string key in postData.Keys)
            {
                stringBuilder.AppendFormat("&{0}={1}", key, postData.Get(key));
            }
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(stringBuilder.ToString().Trim('&'));
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(buffer, 0, buffer.Length);
                requestStream.Close();
                string cookie = string.Empty;

                WebResponse response = request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string html = reader.ReadToEnd();//读取数据
                if (!string.IsNullOrEmpty(html) && html.Contains("友情链接保存成功"))
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 删除友链
        /// </summary>
        /// <param name="requestUri"></param>
        /// <param name="postData"></param>
        /// <returns></returns>
        public static bool deletePost(string requestUri, NameValueCollection postData, CookieContainer cc)
        {
            if (postData != null)
            {

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
                request.Method = "post";
                request.ContentType = "application/x-www-form-urlencoded";
                request.CookieContainer = cc;
                request.Timeout = 10000;
                request.KeepAlive = false;
                StringBuilder stringBuilder = new StringBuilder();

                if (postData.Count > 0)
                {
                    string _ids = string.Empty;
                    foreach (string key in postData.Keys)
                    {
                        if (key.Contains("alllinkid"))
                        {
                            stringBuilder.AppendFormat("&{0}={1}", "alllinkid[]", postData.Get(key));
                            _ids += postData.Get(key) + ',';
                        }
                        else
                            stringBuilder.AppendFormat("&{0}={1}", key, postData.Get(key));
                    }
                    try
                    {
                        byte[] buffer = Encoding.UTF8.GetBytes(stringBuilder.ToString().Trim('&'));
                        Stream requestStream = request.GetRequestStream();
                        requestStream.Write(buffer, 0, buffer.Length);
                        requestStream.Close();
                        string cookie = string.Empty;
                        WebResponse response = request.GetResponse();
                        StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                        string html = reader.ReadToEnd();//读取数据
                        if (!string.IsNullOrEmpty(html) && html.Contains("链接成功删除"))
                        {
                            Console.WriteLine(requestUri + "友链删除成功！删除条数" + (postData.Count - 1) + "个,ID=[" + _ids + "]");
                            return true;
                        }
                        else
                        {
                            Console.WriteLine(requestUri + "友链删除失败");
                            return false;
                        }
                    }
                    catch
                    {
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine(requestUri + "无符合条件友链！");
                    return true;
                }
            }
            else
                return false;

        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="requestUri"></param>
        /// <param name="postData"></param>
        /// <returns></returns>
        public static System.Collections.Specialized.NameValueCollection ListPost(string requestUri, NameValueCollection postData, CookieContainer cc)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
                request.Method = "post";
                request.ContentType = "application/x-www-form-urlencoded";
                request.CookieContainer = cc;
                request.Timeout = 10000;
                request.KeepAlive = false;
                StringBuilder stringBuilder = new StringBuilder();
                foreach (string key in postData.Keys)
                {
                    stringBuilder.AppendFormat("&{0}={1}", key, postData.Get(key));
                }
                byte[] buffer = Encoding.UTF8.GetBytes(stringBuilder.ToString().Trim('&'));
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(buffer, 0, buffer.Length);
                requestStream.Close();

                WebResponse response = request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string html = reader.ReadToEnd();//读取数据
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);

                System.Collections.Specialized.NameValueCollection dic = CrawlerCenter.GetDeleteIds(doc);
                return dic;
            }
            catch(Exception e1)
            {
                Console.WriteLine("ListPost异常:"+e1.Message);
                return null;
            }
        }


        public static HtmlAgilityPack.HtmlDocument HtmlPost(string requestUri, NameValueCollection postData, CookieContainer cc)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
                request.Method = "post";
                request.ContentType = "application/x-www-form-urlencoded";
                request.CookieContainer = cc;
                request.Timeout = 10000;
                request.KeepAlive = false;
                StringBuilder stringBuilder = new StringBuilder();
                foreach (string key in postData.Keys)
                {
                    stringBuilder.AppendFormat("&{0}={1}", key, postData.Get(key));
                }
                byte[] buffer = Encoding.UTF8.GetBytes(stringBuilder.ToString().Trim('&'));
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(buffer, 0, buffer.Length);
                requestStream.Close();

                WebResponse response = request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string html = reader.ReadToEnd();//读取数据
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                return doc;
            }
            catch(Exception e)
            {
                Console.WriteLine("HtmlPost异常:"+e.Message);
                return null;
            }
        }

        /// <summary>
        /// 根据url下载内容  之前是GB2312
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string DownloadUrl(string url, string cookie)
        {
            return DownloadHtml(url, Encoding.UTF8, cookie);
        }

        /// <summary>
        /// 下载html
        /// http://tool.sufeinet.com/HttpHelper.aspx
        /// HttpWebRequest功能比较丰富，WebClient使用比较简单
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string DownloadHtml(string url, Encoding encode, string cookie)
        {
            string html = string.Empty;
            try
            {
                //https可以下载--
                //ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback((object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) =>
                //{
                //    return true; //总是接受  
                //});
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;//模拟请求
                request.Timeout = 30 * 1000;//设置30s的超时
                request.KeepAlive = true;
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.106 Safari/537.36";
                //request.UserAgent = "User - Agent:Mozilla / 5.0(iPhone; CPU iPhone OS 7_1_2 like Mac OS X) App leWebKit/ 537.51.2(KHTML, like Gecko) Version / 7.0 Mobile / 11D257 Safari / 9537.53";
                request.ContentType = "text/html; charset=utf-8";// "text/html;charset=gbk";// 
                request.Headers.Add("Cookie", cookie);
                //request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                //request.Headers.Add("Accept-Encoding", "gzip, deflate, sdch");
                //request.Headers.Add("Referer", "http://list.yhd.com/c0-0/b/a-s1-v0-p1-price-d0-f0-m1-rt0-pid-mid0-kiphone/");
                //Encoding enc = Encoding.GetEncoding("GB2312"); // 如果是乱码就改成 utf-8 / GB2312

                //如何自动读取cookie
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)//发起请求
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        //logger.Warn(string.Format("抓取{0}地址返回失败,response.StatusCode为{1}", url, response.StatusCode));
                    }
                    else
                    {
                        try
                        {
                            //string sessionValue = response.Cookies["ASP.NET_SessionId"].Value;//2 读取cookie
                            StreamReader sr = new StreamReader(response.GetResponseStream(), encode);
                            html = sr.ReadToEnd();//读取数据
                            sr.Close();
                        }
                        catch (Exception ex)
                        {
                            //logger.Error(string.Format($"DownloadHtml抓取{url}失败"), ex);
                            html = null;
                        }
                    }
                }
            }
            catch (System.Net.WebException ex)
            {
                if (ex.Message.Equals("远程服务器返回错误: (306)。"))
                {
                    //logger.Error("远程服务器返回错误: (306)。", ex);
                    html = null;
                }
            }
            catch (Exception ex)
            {
                //logger.Error(string.Format("DownloadHtml抓取{0}出现异常", url), ex);
                html = null;
            }
            return html;
        }



    }
}
