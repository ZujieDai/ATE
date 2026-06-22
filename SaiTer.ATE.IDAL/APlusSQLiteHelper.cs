using SaiTer.ATE.DataModel.DataBaseModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.Struct;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.IDAL
{
    public class APlusSQLiteHelper
    {
        public static SQLiteConnection SQLiteConn = null;
        // public static string SQLiteFilePath = "D:\\Program Files\\A+\\Debug\\Db\\saiterAP.sqlite";//
        public static string SQLiteFilePath = System.AppDomain.CurrentDomain.BaseDirectory + "Debug\\Db\\saiterAP.sqlite";//
        public static string SQLiteFilePathCADC = System.AppDomain.CurrentDomain.BaseDirectory + "DebugCADC\\Db\\saiterAP.sqlite";//

        /// <summary>
        /// 互操 测试项测试结果及测试时间读取
        /// </summary>
        /// <param name="TestNumber">测试项名称(如D0.1001)</param>
        /// <param name="LstItemData"></param>
        public static void CheckDataToTable_TestInterop(string TestNumber, List<TrialDataModel> LstItemData)
        {
            if (SQLiteConn == null)
            {
                SQLiteConn = new SQLiteConnection("data source=" + SQLiteFilePath);
            }
            if (SQLiteConn.State != System.Data.ConnectionState.Open)
            {
                SQLiteConn.Open();//打开数据库，若文件不存在会自动创建  
            }
            SQLiteCommand cmd = SQLiteConn.CreateCommand();

            cmd.CommandText = "SELECT * FROM TestInterop WHERE TestNumber=='" + TestNumber + "'";
            SQLiteDataReader sr = cmd.ExecuteReader();
            while (sr.Read())
            {
                LstItemData[0].SaveTime = sr[7].ToString();//测试项时间

            }
            sr.Close();


        }

        /// <summary>
        /// 互操 测试项参数读取
        /// </summary>
        /// <param name="TestNumber">测试项名称(如D0.1001)</param>
        /// <param name="LstItemData"></param>
        public static List<TrialDataModel> CheckDataToTable_InteropItems(string TestNumber, List<TrialDataModel> LstTrialData, string BarCode, long PKID)
        {
            if (SQLiteConn == null)
            {
                SQLiteConn = new SQLiteConnection("data source=" + SQLiteFilePath);
            }
            if (SQLiteConn.State != System.Data.ConnectionState.Open)
            {
                SQLiteConn.Open();//打开数据库，若文件不存在会自动创建  
            }
            SQLiteCommand cmd = SQLiteConn.CreateCommand();

            cmd.CommandText = "SELECT * FROM InteropItems WHERE FaPrjID=='" + TestNumber + "'" + "ORDER BY SortNum";
            SQLiteDataReader SQLstr = cmd.ExecuteReader();
            List<TrialDataModel> LstItemData = new List<TrialDataModel>();

            int index = 1;
            while (SQLstr.Read())
            {
                TrialDataModel item = new TrialDataModel
                {
                    SchemeID = LstTrialData[0].SchemeID,
                    BarCode = BarCode,
                    PKID = PKID,
                    TrialName = LstTrialData[0].TrialName,
                    TrialType = LstTrialData[0].TrialType,
                    SchemeName = LstTrialData[0].SchemeName,
                    SaveTime = LstTrialData[0].ToString(),
                    //ItemName = SQLstr[5].ToString(),
                    ItemName = (index++).ToString(),
                    TrialResult = SQLstr[4].ToString() == "合格" ? EmTrialResult.Pass : EmTrialResult.Fail,
                    //状态|数据名称|上限|下限|测量值|结果
                    ExtentData = $"{SQLstr[1]}|{SQLstr[2]}|-|-|{SQLstr[3]}"
                };
                item.Data2 = item.ExtentData;
                LstItemData.Add(item);
            }

            SQLstr.Close();
            return LstItemData;
        }

        /// <summary>
        /// 协议一致性 测试项参数读取
        /// </summary>
        /// <param name="TestNumber">测试项名称(DP1001)</param>
        /// <param name="TrialItem">检测试验项</param>
        /// <param name="BarCode">充电枪位条码号</param>
        /// <param name="PKID">唯一主键ID</param>
        public static List<TrialDataModel> CheckDataToTable_TestItemsReport(string TestNumber, StTrialItem TrialItem, string BarCode, long PKID)
        {
            List<TrialDataModel> LstItemData = new List<TrialDataModel>();
            if (SQLiteConn == null)
            {
                SQLiteConn = new SQLiteConnection("data source=" + SQLiteFilePath);
            }
            if (SQLiteConn.State != System.Data.ConnectionState.Open)
            {
                SQLiteConn.Open();//打开数据库，若文件不存在会自动创建  
            }
            SQLiteCommand cmd = SQLiteConn.CreateCommand();

            cmd.CommandText = "SELECT * FROM TestItemsReport WHERE ItemId=='" + TestNumber + "'";
            SQLiteDataReader SQLstr = cmd.ExecuteReader();
            while (SQLstr.Read())
            {
                int index = 1;
                //解析成单个项，数据库内容如下：
                /*充电机使用传输协议功能接收完成BRM报文 合格
                充电机停止发送SPN2560=00的CRM报文 合格
                CRM格式:合格 
                CRM最小周期:20 不合格 
                CRM最大周期:77 不合格 
                CRM长度:8 合格 
                */
                string[] testStrs = SQLstr["TestText1"].ToString().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string testStr in testStrs)
                {
                    if (string.IsNullOrWhiteSpace(testStr))
                        continue;
                    string strTestItem = testStr;

                    string min = "-", max = "-";
                    // CRM最小周期:251 合格 [251,251]
                    if (strTestItem.LastIndexOf(' ') > -1)
                    {
                        string[] testData = strTestItem.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                        if (testData[testData.Length - 1].IndexOf('[') > -1)
                        {
                            int dotIndex = testData[testData.Length - 1].IndexOf(',');
                            if (dotIndex > -1)
                            {
                                min = testData[testData.Length - 1].Substring(1, dotIndex - 1);
                                max = testData[testData.Length - 1].Substring(dotIndex + 1).Replace("]", "");
                                strTestItem = strTestItem.Replace($" [{min},{max}]", "");
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(min))
                        min = "-";
                    if (string.IsNullOrEmpty(max))
                        max = "-";

                    TrialDataModel item = new TrialDataModel
                    {
                        SchemeID = TrialItem.SchemeID,
                        BarCode = BarCode,
                        PKID = PKID,
                        TrialName = TrialItem.ItemName,
                        TrialType = TrialItem.TrialType,
                        SchemeName = TrialItem.SchemeName,
                        SaveTime = SQLstr["CreateTimestamp"].ToString(),
                        //ItemName = SQLstr[1].ToString(),
                        ItemName = (index++).ToString(),
                        TrialResult = strTestItem.Contains("不合格") ? EmTrialResult.Fail : EmTrialResult.Pass//测试结果
                    };
                    string testText = strTestItem.Replace("不", "").Replace("合格", " ");
                    if (testText.Trim().Split(new string[] { ":"}, StringSplitOptions.RemoveEmptyEntries).Length > 1)
                        item.ExtentData = $"{SQLstr["ItemId"]}|{testText.Split(':')[0].Trim()}|{min}|{max}|{testText.Split(':')[1].Trim()}";
                    else
                        item.ExtentData = $"{SQLstr["ItemId"]}|{testText.Split(':')[0].Trim()}|{min}|{max}|-";
                    item.Data2 = item.ExtentData;
                    item.Data3 = TrialItem.TrialOrder.ToString();
                    LstItemData.Add(item);
                }
            }

            SQLstr.Close();
            return LstItemData;
        }


        /// <summary>
        /// 协议一致性 测试项参数读取
        /// </summary>
        /// <param name="TestNumber">测试项名称(STCA1001)</param>
        /// <param name="TrialItem">检测试验项</param>
        /// <param name="BarCode">充电枪位条码号</param>
        /// <param name="PKID">唯一主键ID</param>
        public static List<TrialDataModel> CheckDataToTable_TestItemsReportCADC(string TestNumber, StTrialItem TrialItem, string BarCode, long PKID)
        {
            List<TrialDataModel> LstItemData = new List<TrialDataModel>();
            if (SQLiteConn == null)
            {
                SQLiteConn = new SQLiteConnection("data source=" + SQLiteFilePathCADC);
            }
            if (SQLiteConn.State != System.Data.ConnectionState.Open)
            {
                SQLiteConn.Open();//打开数据库，若文件不存在会自动创建  
            }
            SQLiteCommand cmd = SQLiteConn.CreateCommand();

            cmd.CommandText = "SELECT * FROM TestItemsReportCADC WHERE ItemId=='" + TestNumber + "'";
            SQLiteDataReader SQLstr = cmd.ExecuteReader();
            while (SQLstr.Read())
            {
                int index = 1;
                //解析成单个项，数据库内容如下：
                /*充电机使用传输协议功能接收完成BRM报文 合格
                充电机停止发送SPN2560=00的CRM报文 合格
                CRM格式:合格 
                CRM最小周期:20 不合格 
                CRM最大周期:77 不合格 
                CRM长度:8 合格 
                */
                string[] testStrs = SQLstr["TestText1"].ToString().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string testStr in testStrs)
                {
                    if (string.IsNullOrWhiteSpace(testStr))
                        continue;
                    string strTestItem = testStr;

                    string min = "-", max = "-";
                    // CRM最小周期:251 合格 [251,251]
                    if (strTestItem.LastIndexOf(' ') > -1)
                    {
                        string[] testData = strTestItem.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                        if (testData[testData.Length - 1].IndexOf('[') > -1)
                        {
                            int dotIndex = testData[testData.Length - 1].IndexOf(',');
                            if (dotIndex > -1)
                            {
                                min = testData[testData.Length - 1].Substring(1, dotIndex - 1);
                                max = testData[testData.Length - 1].Substring(dotIndex + 1).Replace("]", "");
                                strTestItem = strTestItem.Replace($" [{min},{max}]", "");
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(min))
                        min = "-";
                    if (string.IsNullOrEmpty(max))
                        max = "-";

                    TrialDataModel item = new TrialDataModel
                    {
                        SchemeID = TrialItem.SchemeID,
                        BarCode = BarCode,
                        PKID = PKID,
                        TrialName = TrialItem.ItemName,
                        TrialType = TrialItem.TrialType,
                        SchemeName = TrialItem.SchemeName,
                        SaveTime = SQLstr["CreateTimestamp"].ToString(),
                        //ItemName = SQLstr[1].ToString(),
                        ItemName = (index++).ToString(),
                        //TrialResult = strTestItem.Contains("不合格") ? EmTrialResult.Fail : EmTrialResult.Pass//测试结果
                        TrialResult = SQLstr["TestSummary"].ToString().Contains("不合格") ? EmTrialResult.Fail : EmTrialResult.Pass//测试结果
                    };
                    string testText = strTestItem.Replace("不", "").Replace("合格", " ");
                    if (testText.Trim().Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries).Length > 1)
                        item.ExtentData = $"{SQLstr["ItemId"]}|{testText.Split(':')[0].Trim()}|{min}|{max}|{testText.Split(':')[1].Trim()}";
                    else
                        item.ExtentData = $"{SQLstr["ItemId"]}|{testText.Split(':')[0].Trim()}|{min}|{max}|-";
                    item.Data2 = item.ExtentData;
                    item.Data3 = TrialItem.TrialOrder.ToString();
                    LstItemData.Add(item);
                }
            }

            SQLstr.Close();
            return LstItemData;
        }
    }
}
