using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.MES
{
    public class TBHttpMES : HttpBase
    {
        public string EMP;     //工号
        public string Res;     //工序
        public string MachineNo; //设备编码
        public string Fixture; //治具编码
        public int PostWay;     //上传方式（0-部分，1-全部）
        public bool IsPost;     //是否上传

        private TBHttpMES() { }
        private static TBHttpMES instance;
        public static TBHttpMES GetInstance()
        {
            if(instance == null)
                instance = new TBHttpMES();
            if (File.Exists("MESSet.txt"))
            {
                string content = File.ReadAllText("MESSet.txt", UTF8Encoding.Default);
                string[] strSet = content.Split('|');
                foreach (string str in strSet)
                {
                    var item = str.Split('=');
                    if (item[0].Equals("Url")) instance.Url = item[1];
                    if (item[0].Equals("EMP")) instance.EMP = item[1];
                    if (item[0].Equals("Res")) instance.Res = item[1];
                    if (item[0].Equals("MachineNo")) instance.MachineNo = item[1];
                    if (item[0].Equals("Fixture")) instance.Fixture = item[1];
                    if (item[0].Equals("PostWay")) instance.PostWay = Convert.ToInt32(item[1]);
                    if (item[0].Equals("IsPost")) instance.IsPost = Convert.ToBoolean(item[1]);
                }
            }
            return instance;
        }

        #region MES接口
        /// <summary>
        /// 检查SN
        /// </summary>
        /// <param name="SN">序号</param>
        public bool CheckSN(string SN, out string result)
        {
            string TestData = $"01;{EMP};{SN};{Res};{MachineNo};{Fixture};";
            string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
    <soap:Body>
        <ATECommandCode xmlns=""http://mesateapi.com/"">
            <commandString>"+ TestData + @"</commandString>
        </ATECommandCode>
    </soap:Body>
</soap:Envelope>";
            result = SOAPPost(xml);
            return result?.IndexOf("OK") > -1;
        }

        /// <summary>
        /// 提交检测数据
        /// </summary>
        /// <param name="SN">序号</param>
        /// <param name="startTime">测试开始时间，如20181002131019</param>
        /// <param name="values">测量值集合</param>
        public void PostTestValue(string SN, string startTime, List<string> values)
        {
            string TestData = $"04;{EMP};{SN};{Res};{MachineNo};{Fixture};{startTime.Replace("-", "").Replace(" ", "").Replace(":", "")};";
            //注意：每次最多可以传7个测试项，超过则重新组一个04来传。依此类推
            //DZJ-20250507 字节长度限制是50条40个字节，可能会超过长度导致MES系统数据录入失败，改为3个一包
            List<string> xmls = new List<string>();
            int xmls_index = 0;
            for(int index = 0; index < values.Count; index++)
            {
                for(int i = 0; i < 3; i++)
                {
                    //不一定都有这么多测试项，超过就跳出循环
                    if (xmls_index * 3 + i > index)
                        break;
                    if (i == 0)
                    {
                        xmls.Add(TestData + values[index]);
                    }
                    else
                        xmls[xmls_index] += "," + values[index];
                    if (index < values.Count - 1 && i < 2)
                        index++;
                }
                xmls_index++;
            }
            foreach (string command in xmls)
            {
                Log.Log.LogMessage("提交监测数据：" + command);
                string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
    <soap:Body>
        <ATECommandCode xmlns=""http://mesateapi.com/"">
            <commandString>" + command + @"</commandString>
        </ATECommandCode>
    </soap:Body>
</soap:Envelope>";
                string result = SOAPPost(xml);
            }
        }

        /// <summary>
        /// 提交检测结果
        /// </summary>
        /// <param name="SN">序号</param>
        /// <param name="testResult">检测结果</param>
        public string PostTestResult(string SN, string testResult)
        {
            string TestData = $"05;{EMP};{SN};{Res};{testResult}";
            Log.Log.LogMessage("提交检测结果：" + TestData);
            string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
    <soap:Body>
        <ATECommandCode xmlns=""http://mesateapi.com/"">
            <commandString>" + TestData + @"</commandString>
        </ATECommandCode>
    </soap:Body>
</soap:Envelope>";
            string result = SOAPPost(xml);
            return result;
        }
        #endregion
    }
}
