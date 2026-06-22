using SaiTer.ATE.InterFace;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaiTer.ATE.MES
{
    /// <summary>
    /// 惠州TB自动化产线协议（中科摩通对接）
    /// </summary>
    public class TBHttpMESAutomation: HttpBase
    {
        public string ServerUrl;     //b本地服务的IP端口
        public string PostUrl;     //提交产线的IP端口
        public string MasterMaterialBarCode;     //设备SN码
        public string TestProgramName;     //方案名称，建议采用字母+数字，比如：A1，表示工位A的第一个测试方案
        public bool IsPost;     //是否上传
        HttpServer httpServer;
        public Action<string, string> actionresultDo;

        private TBHttpMESAutomation() { }
        private static TBHttpMESAutomation instance;
        public static TBHttpMESAutomation GetInstance()
        {
            if (instance == null)
            {
                instance = new TBHttpMESAutomation();
                if (File.Exists("AutomationSet.txt"))
                {
                    string content = File.ReadAllText("AutomationSet.txt", UTF8Encoding.Default);
                    string[] strSet = content.Split('|');
                    foreach (string str in strSet)
                    {
                        var item = str.Split('=');
                        if (item[0].Equals("ServerUrl")) instance.ServerUrl = item[1];
                        if (item[0].Equals("PostUrl")) instance.PostUrl = item[1];
                        if (item[0].Equals("MasterMaterialBarCode")) instance.MasterMaterialBarCode = item[1];
                        if (item[0].Equals("TestProgramName")) instance.TestProgramName = item[1];
                        if (item[0].Equals("IsPost")) instance.IsPost = Convert.ToBoolean(item[1]);
                    }
                }
                //instance.InitServer();
            }
            return instance;
        }

        private bool InitServer()
        {
            try
            {
                httpServer = new HttpServer(instance.ServerUrl);
                httpServer.StartAsync();

                httpServer.AddRoute("/autoline/start", async request =>
                {
                    try
                    {
                        using (var reader = new StreamReader(request.InputStream))
                        {
                            var json = await reader.ReadToEndAsync();
                            Log.Log.LogMessage(json);
                            var data = Json.Deserialize<Dictionary<string, object>>(json);
                            if (data == null || data.Count == 0)
                            {
                                // 构建数据对象
                                return Json.Serialize(new
                                {
                                    message = "数据为空",
                                    reqCode = ""
                                });
                            }
                            data.TryGetValue("MasterMaterialBarCode", out object code);     //待测设备SN码
                            MasterMaterialBarCode = code.ToString();
                            data.TryGetValue("TestProgramName", out object name);           //方案名称，建议采用字母+数字，比如：A1，表示工位A的第一个测试方案
                            TestProgramName = name.ToString();
                            //MessageBox.Show($"启动测试，SN码：{code}，方案名：{name}");
                            using (var stream = File.Open("AutomationSet.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                            {
                                stream.Seek(0, SeekOrigin.Begin);
                                stream.SetLength(0);
                                string content = $"ServerUrl={ServerUrl}|PostUrl={PostUrl}|MasterMaterialBarCode={code}|TestProgramName={name}|IsPost={IsPost}";
                                var array = UTF8Encoding.Default.GetBytes(content);
                                stream.Write(array, 0, array.Length);
                            }

                            bool res = int.TryParse(name.ToString().Substring(1), out int schemeIndex);
                            if (res)
                            {
                                SystemEvent.SetChargerInfo(code.ToString(), schemeIndex);
                                Thread.Sleep(300);
                                SystemEvent.SetScheme(schemeIndex);
                                Thread.Sleep(1500);
                                SystemEvent.StartTest();
                            }
                            else
                            {
                                // 构建数据对象
                                return Json.Serialize(new
                                {
                                    message = "方案名称错误",
                                    reqCode = code
                                });
                            }
                            // 构建数据对象
                            var resData = new
                            {
                                message = "OK",
                                reqCode = code
                            };
                            string resJson = Json.Serialize(resData);
                            return resJson;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Log.LogException(ex);
                        // 构建数据对象
                        var resData = new
                        {
                            message = "Server Error",
                            reqCode = ""
                        };
                        string resJson = Json.Serialize(resData);
                        return resJson;
                    }
                });
                httpServer.AddRoute("/autoline/stop", async request =>
                {
                    try
                    {
                        using (var reader = new StreamReader(request.InputStream))
                        {
                            var json = await reader.ReadToEndAsync();
                            Log.Log.LogMessage(json);
                            var data = Json.Deserialize<Dictionary<string, object>>(json);
                            if (data == null || data.Count == 0)
                            {
                                // 构建数据对象
                                return Json.Serialize(new
                                {
                                    message = "数据为空",
                                    reqCode = ""
                                });
                            }
                            data.TryGetValue("MasterMaterialBarCode", out object code);     //待测设备SN码
                            MasterMaterialBarCode = code.ToString();
                            data.TryGetValue("TestProgramName", out object name);           //方案名称，建议采用字母+数字，比如：A1，表示工位A的第一个测试方案
                            TestProgramName = name.ToString();
                            //MessageBox.Show($"启动测试，SN码：{code}，方案名：{name}");
                            using (var stream = File.Open("AutomationSet.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                            {
                                stream.Seek(0, SeekOrigin.Begin);
                                stream.SetLength(0);
                                string content = $"ServerUrl={ServerUrl}|PostUrl={PostUrl}|MasterMaterialBarCode={code}|TestProgramName={name}|IsPost={IsPost}";
                                var array = UTF8Encoding.Default.GetBytes(content);
                                stream.Write(array, 0, array.Length);
                            }

                            SystemEvent.StopTest();
                            // 构建数据对象
                            var resData = new
                            {
                                message = "OK",
                                reqCode = code
                            };
                            string resJson = Json.Serialize(resData);
                            return resJson;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Log.LogException(ex);
                        // 构建数据对象
                        var resData = new
                        {
                            message = "Server Error",
                            reqCode = ""
                        };
                        string resJson = Json.Serialize(resData);
                        return resJson;
                    }
                });
                httpServer.AddRoute("/autoline/action", async request =>
                {
                    try
                    {
                        using (var reader = new StreamReader(request.InputStream))
                        {
                            var json = await reader.ReadToEndAsync();
                            Log.Log.LogMessage(json);
                            var jData = Json.Deserialize<Dictionary<string, object>>(json);
                            if (jData == null || jData.Count == 0)
                            {
                                // 构建数据对象
                                return Json.Serialize(new
                                {
                                    message = "数据为空",
                                    reqCode = ""
                                });
                            }
                            jData.TryGetValue("MasterMaterialBarCode", out object code);     //待测设备SN码
                            jData.TryGetValue("Data", out object Data);
                            Dictionary<string, object> kk = (Dictionary<string, object>)Data;
                            //var dicData = Json.Deserialize<Dictionary<string, object>>(Data.ToString().Replace("{", "").Replace("}", ""));
                            kk.TryGetValue("ActionType", out object ActionType);
                            kk.TryGetValue("ActionResult", out object ActionResult);

                            //执行回馈动作
                            actionresultDo?.Invoke(ActionType.ToString(), ActionResult.ToString());

                            Log.Log.LogMessage($"启动测试，SN码：{code}，执行动作：{ActionType}，执行结果：{ActionResult}");

                            // 构建数据对象
                            var resData = new
                            {
                                message = "OK",
                                reqCode = code
                            };
                            string resJson = Json.Serialize(resData);
                            return resJson;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Log.LogException(ex);
                        // 构建数据对象
                        var resData = new
                        {
                            message = "Server Error",
                            reqCode = ""
                        };
                        string resJson = Json.Serialize(resData);
                        return resJson;
                    }
                });
            }
            catch(Exception ex)
            {
                Log.Log.LogException(ex);
                return false;
            }
            return true;
        }

        public bool Run()
        {
            return InitServer();
        }

        public bool Stop()
        {
            try
            {
                if (httpServer == null || !httpServer.IsRunning) return true;
                return httpServer.Stop();
            }
            catch(Exception ex) { Log.Log.LogException(ex); return false; }
            finally
            {
                instance = null;
            }
        }
    }
}
