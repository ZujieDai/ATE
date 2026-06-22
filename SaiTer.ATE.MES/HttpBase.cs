using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace SaiTer.ATE.MES
{
    public class HttpBase
    {
        /// <summary>
        /// 设置请求 URL
        /// </summary>
        public string Url {  get; set; }

        public string HttpPost(string postData)
        {
            // 将数据转换为字节数组
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            // 创建 WebRequest 实例
            WebRequest request = WebRequest.Create(Url);
            request.Method = "POST";
            request.ContentType = "application/json";
            //request.ContentLength = byteArray.Length;

            // 获取请求流并写入数据
            using (Stream dataStream = request.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
            }

            // 发送请求并获取响应
            using (WebResponse response = request.GetResponse())
            {
                // 获取响应流
                using (Stream responseStream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        string responseText = reader.ReadToEnd();
                        if (responseText.Contains("\"success\":true"))
                        {
                            Log.Log.LogMessage("MES上传成功");
                        }
                        else
                            MessageBox.Show("MES上传失败：" + responseText);
                        return responseText;
                    }
                }
            }
        }

        public static string HttpPost(string Url, string postData)
        {
            try
            {
                Log.Log.LogMessage("发送数据" + Url + ": " + postData);
                // 将数据转换为字节数组
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                // 创建 WebRequest 实例
                WebRequest request = WebRequest.Create(Url);
                request.Method = "POST";
                request.ContentType = "application/json";
                //request.ContentLength = byteArray.Length;

                // 获取请求流并写入数据
                using (Stream dataStream = request.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }

                // 发送请求并获取响应
                using (WebResponse response = request.GetResponse())
                {
                    // 获取响应流
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(responseStream))
                        {
                            string responseText = reader.ReadToEnd();
                            Log.Log.LogMessage($"{responseText}");
                            return responseText;
                        }
                    }
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); return ""; }
        }

        public static async void HttpPostAsyn(string Url, string postData)
        {
            try
            {
                Log.Log.LogMessage("发送数据" + Url + ": " + postData);
                // 将数据转换为字节数组
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                // 创建 WebRequest 实例
                WebRequest request = WebRequest.Create(Url);
                request.Method = "POST";
                request.ContentType = "application/json";
                //request.ContentLength = byteArray.Length;

                // 获取请求流并写入数据
                using (Stream dataStream = await request.GetRequestStreamAsync())
                {
                    Log.Log.LogMessage("开始写入请求");
                    dataStream.WriteAsync(byteArray, 0, byteArray.Length);
                }

                // 发送请求并获取响应
                //using (var response = await request.GetResponseAsync())
                //{
                //    using (var reader = new StreamReader(response.GetResponseStream()))
                //    {
                //        return await reader.ReadToEndAsync();
                //    }
                //}
            }
            catch (Exception ex) { Log.Log.LogException(ex); /*return "";*/ }
        }

        /// <summary>
        /// 提交SOAP请求并返回响应结果
        /// </summary>
        /// <param name="xml">构建SOAP请求</param>
        /// <returns></returns>
        public string SOAPPost(string xml)
        {
            //重发三次
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    // 构建SOAP请求
                    string soapRequest = xml;

                    byte[] requestBytes = Encoding.UTF8.GetBytes(soapRequest);
                    // 创建WebRequest对象
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                    request.Method = "POST";
                    request.ContentType = "text/xml;charset=utf-8";
                    request.ContentLength = requestBytes.Length;

                    // 发送SOAP请求
                    using (Stream requestStream = request.GetRequestStream())
                    {
                        requestStream.Write(requestBytes, 0, requestBytes.Length);
                    }

                    // 接收响应
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    using (Stream responseStream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        string responseText = reader.ReadToEnd();
                        Log.Log.LogMessage("Web服务响应:\n" + responseText);
                        XmlDocument xmlDocument = new XmlDocument();
                        xmlDocument.LoadXml(responseText);
                        return xmlDocument.InnerText;
                    }
                }
                catch (Exception ex)
                {
                    Log.Log.LogException(ex, "MES内部异常");
                }
                CountDownTimeInfo($"注意：请勿操作！！\r\n接口通讯故障，重新上传。。{i + 1}/3", 10, 0);
                Thread.Sleep(1000);
            }
            CountDownTimeInfo("接口通讯故障，上传不成功，请联系系统管理员", 10, 0);
            return null;
        }


        /// <summary>
        /// 倒计时提示
        /// </summary>
        /// <param name="info">提示信息</param>
        /// <param name="time">时间(S)</param>
        /// <param name="type">提示类型 0-纯倒计时提示信息。 1-倒计时等待选择  2-一般检测等人工确认 倒计时等待选择枪位结论  3-等待用户输入数据</param>
        public void CountDownTimeInfo(string info, int time, int type)
        {
            SystemEvent.SendCountDownTimer(info, time, type);
        }
    }
}
