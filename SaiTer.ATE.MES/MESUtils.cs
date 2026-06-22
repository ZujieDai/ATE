using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.DataBaseModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.IDAL.SQLiteIDAL;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SaiTer.ATE.MES
{
    public static class MESUtils
    {
        #region 惠州拓邦
        /// <summary>
        /// 判断当前项目是否为惠州拓邦并且开启了MES（用于SN检测）
        /// </summary>
        /// <returns></returns>
        public static bool IsMES_TB()
        {
            //如果不上传MES直接返回false
            if (File.Exists("MESSet.txt"))
            {
                string content = File.ReadAllText("MESSet.txt", UTF8Encoding.Default);
                string[] strSet = content.Split('|');
                if (strSet.Length > 6 && !Convert.ToBoolean(strSet[6].Split('=')[1]))
                    return false;
            }
            else
                return false;

            bool isMESSet = false, isTB = false;
            string strMESSet = ConfigurationManager.AppSettings["isMESSet"];
            if (strMESSet != null)
            {
                isMESSet = bool.Parse(strMESSet);
            }
            
            string Customer = ConfigurationManager.AppSettings["Customer"];
            if (!string.IsNullOrEmpty(Customer) && Customer.Equals("TB"))
                isTB = true;
            return isMESSet && isTB;
        }

        public static void PostTestValue(List<ChargerInfoModel> lstCharger)
        {
            if (lstCharger == null || lstCharger.Count < 1)
                return;

            if (IsMES_TB())
            {
                if (File.Exists("MESSet.txt"))
                {
                    string content = File.ReadAllText("MESSet.txt", UTF8Encoding.Default);
                    string[] strSet = content.Split('|');
                    if (strSet.Length > 6)
                    {
                        TBHttpMES tBHttp = TBHttpMES.GetInstance();
                        //获取上传方式是全部还是部分
                        if (strSet[5].Split('=')[1].Equals("1"))
                        {
                            //需要从数据库查询
                            List<TrialDataModel> LstTrialData = TrialItemDataTmpManage.GetTrialData_ALL(lstCharger);
                            List<string> strsValue = new List<string>();
                            //上传全部，接口04
                            foreach (var item in LstTrialData)
                            {
                                string status = item.ExtentData.Split('|')[0];
                                string name = item.ExtentData.Split('|')[1];
                                string value = item.ExtentData.Split('|')[4];
                                string sMin = item.ExtentData.Split('|')[2];
                                string sMax = item.ExtentData.Split('|')[3];
                                string sResult = (item.TrialResult == EmTrialResult.Pass || item.TrialResult == EmTrialResult.NA) ? "PASS" : "FAIL";

                                //直流测试难修改去掉一段，不然字节过长
                                string PostMESMode = ConfigurationManager.AppSettings["PostMESMode"];
                                if(PostMESMode != null && PostMESMode.Contains("1"))
                                    strsValue.Add($"{status}({name}):{value}:{sMin}~{sMax}:{sResult}");
                                else if (item.TrialType >= EmTrialType.绝缘电阻_输入对输出 && item.TrialType <= EmTrialType.接地试验)
                                    strsValue.Add($"{item.TrialName}:{value}:{sMin}~{sMax}:{sResult}");
                                else
                                    strsValue.Add($"{item.TrialName}_{status}({name}):{value}:{sMin}~{sMax}:{sResult}");
                            }
                            //tBHttp.PostTestValue(LstTrialData[0].BarCode, LstTrialData[0].SaveTime, strsValue);
                            //改为上传时间
                            tBHttp.PostTestValue(LstTrialData[0].BarCode, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss").Replace("-", ""), strsValue);
                        }
                    }
                }
            }
        }

        public static void PostTestResult(string SN, int ChargerId, EmTrialResult CheckResult)
        {
            if (IsMES_TB())
            {
                if (File.Exists("MESSet.txt"))
                {
                    string content = File.ReadAllText("MESSet.txt", UTF8Encoding.Default);
                    string[] strSet = content.Split('|');
                    if (strSet.Length > 6)
                    {
                        TBHttpMES tBHttp = TBHttpMES.GetInstance();
                        //上传接口05
                        string result = CheckResult == EmTrialResult.Pass ? "OK" : $"NG;";
                        if (CheckResult == EmTrialResult.Fail)
                        {
                            TrialItemDataTmpManage.SelectFailTrialData(SN, ChargerId, out var lstTrialData);
                            var data = lstTrialData.FirstOrDefault();
                            if (data != null)
                                result += $"{data.ItemName} 上限{data.ExtentData.Split('|')[2]} "
                                    + $"下限{data.ExtentData.Split('|')[3]} 测量值{data.ExtentData.Split('|')[4]}超出范围";
                        }
                        string retRes = tBHttp.PostTestResult(SN, result);
                        // NG等情况
                        if (retRes?.IndexOf("OK") < 0)
                        {
                            // 加载XML字符串到XmlDocument对象中
                            XmlDocument xmlDoc = new XmlDocument();
                            xmlDoc.LoadXml(retRes);

                            // 解析XML文档，考虑到默认命名空间
                            XmlNamespaceManager nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
                            nsManager.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");

                            // 查找并获取指定节点的内容
                            XmlNode resultNode = xmlDoc.SelectSingleNode("//soap:Body/ATECommandCodeResponse/ATECommandCodeResult", nsManager);
                            string resultCode = resultNode.InnerText;
                            SystemEvent.MessageInfo(true, "MES上传异常！！\r\n" + resultCode);
                        }
                    }
                }
            }
        }
        #endregion
    }
}
