using NPOI.OpenXmlFormats.Wordprocessing;
using NPOI.OpenXmlFormats.Wordprocessing;
using NPOI.SS.Formula.Functions;
using NPOI.XWPF.Model;
using NPOI.XWPF.UserModel;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.DataBaseModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.Struct;
using SaiTer.ATE.IDAL.DBUtility;
using SaiTer.ATE.IDAL.SQLiteIDAL;
using SQLiteSugar;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static NPOI.HSSF.Util.HSSFColor;
using static SaiTer.ATE.IDAL.SQLiteIDAL.TrialItemDataTmpManage;

namespace SaiTer.ATE.IDAL
{

    /// <summary>
    /// 测试报告导出
    /// </summary>
    public class ExportReport
    {
        static string strTitleDescripe = "测试描述";
        static string strTitleStandard = "技术要求";
        static string strUserParams = "用户设置参数";
        static string strTrialCondition = "测试条件记录";
        private static List<StTrialItem> lstTrialItems = new List<StTrialItem>();
        private static List<TrialDataModel> _LstTrialData = new List<TrialDataModel>();
        private static List<ChargerInfoModel> LstChargerInfo = new List<ChargerInfoModel>();

        public static List<TrialDataModel> LstTrialData
        {
            get { return _LstTrialData; }
            set
            {
                _LstTrialData = value;
                DicTrialTypeData = ParseData(_LstTrialData);
            }
        }
        /// <summary>
        /// (试验项枚举值，该项的检测数据>)
        /// </summary>
        static Dictionary<int, List<TrialDataModel>> DicTrialTypeData = new Dictionary<int, List<TrialDataModel>>();
        private static Dictionary<int, List<TrialDataModel>> ParseData(List<TrialDataModel> lstTrialData)
        {
            Dictionary<int, List<TrialDataModel>> dicTemp = new Dictionary<int, List<TrialDataModel>>();

            foreach (var item in lstTrialData)
            {
                if (!dicTemp.Keys.Contains((int)item.TrialType))
                {
                    dicTemp.Add((int)item.TrialType, new List<TrialDataModel>());
                }
                dicTemp[(int)item.TrialType].Add(item);
            }
            return dicTemp;
        }

        private static void SelectTrialFinalResult()
        {

        }
        public static bool CreateFile(List<TrialDataModel> lstTrialDatas, ChargerInfoModel chargerInfos, List<string> lstSchemeName, ref string WordPath)
        {
            try
            {
                string strSchemeNames = "";
                for (int i = 0; i < lstSchemeName.Count; i++)
                {
                    strSchemeNames += "'" + lstSchemeName[i] + "',";
                }
                strSchemeNames = strSchemeNames.TrimStart('\'').TrimEnd(new char[] { ',', '\'' });
                lstTrialItems.Clear();
                TrialItemsManage.GetTrialSchemeFromSchemeName(strSchemeNames, ref lstTrialItems);
                LstTrialData = lstTrialDatas;

                //指定Word文档的路径和名称

                // string path = "docx\\demo.docx";
                string Pkid = lstTrialDatas[0].PKID.ToString();
                string GetNowLongData = System.DateTime.Now.ToString("yyyy-MM-dd");
                string path = System.AppDomain.CurrentDomain.BaseDirectory + "报表(勿删)\\Word" + "\\" + GetNowLongData;
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }

                WordPath = path + "\\" + Pkid + strSchemeNames + ".docx";
                //自动化生成数据
                XWPFDocument doc = CreateHead(chargerInfos);

                for (int k = 0; k < doc.Document.body.GetTblArray().Count(); k++)
                {
                    CT_Tbl m_CTTbl = doc.Document.body.GetTblArray()[k];
                    m_CTTbl.AddNewTblPr().jc = new CT_Jc();
                    m_CTTbl.AddNewTblPr().jc.val = ST_Jc.center;//表在页面水平居中
                }
                FileStream out1 = new FileStream(WordPath, FileMode.Create);
                doc.Write(out1);
                out1.Close();
               return true;
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                return false;
            }
        }
        #region------------基于模板生成报告，只填书签，不自动生成表格-------------------
        /// <summary>
        ///基于模板生成报告，只填书签，不自动生成表格
        /// </summary>
        //private static void CreateHeadToKS(XWPFDocument doc, ChargerInfoModel chargerModel)
        //{
        //    try
        //    {
        //        // --------------------------
        //        // 一次性查询所有规则
        //        // --------------------------
        //        string sqlAllRules = @"
        //        SELECT TrialType, TrialName, ItamName, DataContent, DataType, Bookmark, other
        //        FROM TemplateRule";

        //        DataTable dtRules = SQLiteHelper.ExecuteDataTable(
        //            SQLiteHelper.DbConnString,
        //            CommandType.Text,
        //            sqlAllRules,
        //            "TemplateRule",
        //            null);

        //        if (dtRules == null || dtRules.Rows.Count == 0)
        //        {
        //            // Log.Log.Warn("TemplateRule 表中未找到任何数据");
        //            return;
        //        }

        //        var allRules = new List<TemplateRuleModel>();
        //        foreach (DataRow dr in dtRules.Rows)
        //        {
        //            allRules.Add(new TemplateRuleModel
        //            {
        //                TrialType = DBConvert.ToString(dr["TrialType"]),
        //                TrialName = DBConvert.ToString(dr["TrialName"]),
        //                ItamName = DBConvert.ToString(dr["ItamName"]),
        //                DataContent = DBConvert.ToString(dr["DataContent"]),
        //                DataType = DBConvert.ToString(dr["DataType"]).ToLower(),
        //                Bookmark = DBConvert.ToString(dr["Bookmark"]),
        //                Other = DBConvert.ToString(dr["Other"])
        //            });
        //        }

        //        // --------------------------
        //        // 分组查询数据
        //        // --------------------------
        //        var groupedRules = allRules
        //            .GroupBy(r => new
        //            {
        //                TrialType = r.TrialType,
        //                TrialName = r.TrialName
        //            });

        //        var dicTrialData = new Dictionary<(string trialType, string trialName), List<TrialDataModel>>();

        //        foreach (var group in groupedRules)
        //        {
        //            string trialType = group.Key.TrialType;
        //            string trialName = group.Key.TrialName;

        //            string sqlTrialData = $@"
        //            SELECT ItemName, TrialResult, Data2 
        //            FROM TrialItemData 
        //            WHERE BarCode='{chargerModel.BarCode}' 
        //            AND TrialType='{trialType}' 
        //            AND TrialName='{trialName}'";

        //            DataTable dtTrial = SQLiteHelper.ExecuteDataTable(
        //                SQLiteHelper.DbConnString,
        //                CommandType.Text,
        //                sqlTrialData,
        //                "TrialItemData",
        //                null);

        //            var list = new List<TrialDataModel>();
        //            if (dtTrial != null && dtTrial.Rows.Count > 0)
        //            {
        //                foreach (DataRow dr in dtTrial.Rows)
        //                {
        //                    list.Add(new TrialDataModel
        //                    {
        //                        ItemName = DBConvert.ToString(dr["ItemName"]),
        //                        TrialResult = DBConvert.ToString(dr["TrialResult"]).ToUpper() == "PASS"
        //                            ? EmTrialResult.Pass
        //                            : EmTrialResult.Fail,
        //                        Data2 = DBConvert.ToString(dr["Data2"])
        //                    });
        //                }
        //            }
                   
        //            dicTrialData.Add((trialType, trialName), list);
        //        }

        //        // --------------------------
        //        //  生成结果 + 收集每个规则的 allPass 原始状态
        //        // --------------------------
        //        Dictionary<string, string> dicResult = new Dictionary<string, string>();

        //        // 存储每个 rule 的原始检测结果（Pass/Fail/未检测）
        //        Dictionary<TemplateRuleModel, bool?> ruleAllPassDict = new Dictionary<TemplateRuleModel, bool?>();

        //        foreach (var rule in allRules)
        //        {
        //            var key = (rule.TrialType, rule.TrialName);
        //            if (!dicTrialData.TryGetValue(key, out var dataList) || !dataList.Any())
        //            {
        //                dicResult[rule.Bookmark] = "未检测";
        //                ruleAllPassDict[rule] = null; // 未检测
        //                continue;
        //            }

        //            string[] itemNames = rule.ItamName.Split(',').Select(s => s.Trim()).ToArray();
        //            var filteredData = dataList.Where(d => itemNames.Contains(d.ItemName)).ToList();

        //            if (!filteredData.Any())
        //            {
        //                dicResult[rule.Bookmark] = "未检测";
        //                ruleAllPassDict[rule] = null; // 未检测
        //                continue;
        //            }

        //            // 用这个 allPass 作为最终判断依据
        //            bool allPass = filteredData.All(d => d.TrialResult == EmTrialResult.Pass);
        //            ruleAllPassDict[rule] = allPass;

        //            string result;
        //            if (rule.DataType == "bool")
        //            {
        //                var parts = rule.DataContent.Split('|');
        //                result = allPass ? parts.FirstOrDefault() ?? "合格" : parts.Skip(1).FirstOrDefault() ?? "不合格";
        //            }
        //            else if (rule.DataType == "string")
        //            {
        //                int index = DBConvert.ToInt32(rule.Other);
        //                List<string> finalValues = new List<string>();

        //                foreach (var data in filteredData)
        //                {
        //                    if (string.IsNullOrWhiteSpace(data.Data2))
        //                    {
        //                        finalValues.Add("");
        //                        continue;
        //                    }

        //                    string[] data2Parts = data.Data2.Split(new[] { '|' }, StringSplitOptions.None);
        //                    string value = "";
        //                    if (index >= 0 && index < data2Parts.Length)
        //                    {
        //                        value = data2Parts[index].Trim();
        //                    }

        //                    finalValues.Add(value);
        //                }

        //                object[] args = finalValues.Cast<object>().ToArray();
        //                result = string.Format(rule.DataContent, args);
        //            }
        //            else
        //            {
        //                result = rule.DataContent;
        //            }

        //            dicResult[rule.Bookmark] = result;
        //        }

        //        // ==========================================================
        //        // 使用 allPass 判断
        //        // ==========================================================
        //        Dictionary<string, string> dicMergeResult = new Dictionary<string, string>();
        //        Dictionary<string, string> dicTestResult = new Dictionary<string, string>();
        //        var trialGroups = allRules.GroupBy(r => r.TrialName);
        //        foreach (var group in trialGroups)
        //        {
        //            var rulesInGroup = group.ToList();
        //            string firstBk = rulesInGroup.First().Bookmark;
        //            string resTag = ConvertToResTag(firstBk);
        //            string testResultTag = ConvertToTestResultTag(firstBk);
        //            string finalResult = "";

        //            // 取出当前 TrialName 下所有规则的 原始Pass状态
        //            var allAllPass = rulesInGroup
        //                .Where(r => ruleAllPassDict.ContainsKey(r))
        //                .Select(r => ruleAllPassDict[r])
        //                .ToList();

        //            // 有效结果
        //            var validResults = allAllPass.Where(x => x.HasValue).ToList();
        //            int totalCount = allAllPass.Count;        // 总项目数
        //            int validCount = validResults.Count;       // 已检测数量

        //            // ======================
        //            // 基于原始 allPass
        //            // ======================
        //            if (totalCount == 1)
        //            {
        //                // 只有1个项目：严格返回 合格/不合格/未检测
        //                var val = allAllPass.First();
        //                if (val == null) finalResult = "未检测";
        //                else if (val == true) finalResult = "合格";
        //                else finalResult = "不合格";
        //            }
        //            else if (totalCount > 1)
        //            {
        //                // 多个项目：忽略未检测
        //                bool hasFail = validResults.Any(x => x == false);
        //                finalResult = hasFail ? "不合格" : "合格";
        //            }
        //            else
        //            {
        //                finalResult = "未检测";
        //            }

        //            dicMergeResult[resTag] = finalResult;
        //            dicTestResult[testResultTag] = finalResult;
        //        }

        //        // --------------------------
        //        // 写入所有书签到WORD
        //        // --------------------------
        //        foreach (var kv in dicResult)
        //        {
        //            ReplaceBookmark(doc, kv.Key, kv.Value);
        //        }

        //        foreach (var kv in dicMergeResult)
        //        {
        //            ReplaceBookmark(doc, kv.Key, kv.Value);
        //        }
        //        foreach (var kv in dicTestResult)
        //        {
        //            ReplaceBookmark(doc, kv.Key, kv.Value);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Log.LogException(ex);
        //    }
        //}
        #endregion
        private static void CreateHeadToKS(XWPFDocument doc, ChargerInfoModel chargerModel)
        {
            try
            {
                //拿全部规则
                var allRules = TemplateRuleManage.GetAllTemplateRule();
                if (allRules.Count == 0)
                {
                    //Log.Log.Warn("TemplateRule 表中未找到任何数据");
                    return;
                }

                //按TrialType+TrialName分组
                var groupedRules = allRules
                    .GroupBy(r => new { TrialType = r.TrialType, TrialName = r.TrialName });

                var dicTrialData = new Dictionary<(string trialType, string trialName), List<TrialDataModel>>();

                foreach (var group in groupedRules)
                {
                    string trialType = group.Key.TrialType;
                    string trialName = group.Key.TrialName;
                    // 返回List<TrialDataModel>
                    var list = TrialItemResultTmpManage.GetTrialItemDataByBarCode(chargerModel.BarCode, trialType, trialName);
                    dicTrialData.Add((trialType, trialName), list);
                }

                // --------------------------
                // 生成明细结果
                // --------------------------
                Dictionary<string, string> dicResult = new Dictionary<string, string>();
                Dictionary<TemplateRuleModel, bool?> ruleAllPassDict = new Dictionary<TemplateRuleModel, bool?>();

                foreach (var rule in allRules)
                {
                    var key = (rule.TrialType, rule.TrialName);
                    if (!dicTrialData.TryGetValue(key, out var dataList) || !dataList.Any())
                    {
                        dicResult[rule.Bookmark] = "未检测";
                        ruleAllPassDict[rule] = null;
                        continue;
                    }

                    string[] itemNames = rule.ItamName.Split(',').Select(s => s.Trim()).ToArray();
                    var filteredData = dataList.Where(d => itemNames.Contains(d.ItemName)).ToList();

                    if (!filteredData.Any())
                    {
                        dicResult[rule.Bookmark] = "未检测";
                        ruleAllPassDict[rule] = null;
                        continue;
                    }

                    bool allPass = filteredData.All(d => d.TrialResult == EmTrialResult.Pass);
                    ruleAllPassDict[rule] = allPass;
                    string result;
                    List<string> finalValues = new List<string>();
                    List<string> finalValues_Index4 = new List<string>();
                    string[] data2Parts =Array.Empty<string>();
                    string value = string.Empty;
                    object[] args = null;
                    if (rule.DataType == "bool")
                    {
                        var parts = rule.DataContent.Split('|');
                        result = allPass ? parts.FirstOrDefault() ?? "合格" : parts.Skip(1).FirstOrDefault() ?? "不合格";
                    }
                    else if (rule.DataType == "string")
                    {
                        int index = DBConvert.ToInt32(rule.Other);
                        #region-------foreach版本，后续优化成GetFinalValues方法，避免重复代码--------
                        //foreach (var data in filteredData)
                        //{
                        //    if (string.IsNullOrWhiteSpace(data.Data2))
                        //    {
                        //        finalValues.Add("");
                        //        continue;
                        //    }
                        //    data2Parts = data.Data2.Split(new[] { '|' }, StringSplitOptions.None);
                        //    value = index >= 0 && index < data2Parts.Length ? data2Parts[index].Trim() : "";
                        //    finalValues.Add(value);
                        //}
                        #endregion
                        finalValues = GetFinalValues(filteredData, index);
                        args = finalValues.Cast<object>().ToArray();
                        result = string.Format(rule.DataContent, args);
                    }
                    else if (rule.DataType == "custom")
                    {
                        string ruleOtheType = rule.Other;
                        double num1 = 0;
                        double num2 = 0;
                        double num3 = 0;
                        string strTemp = string.Empty;
                        if (ruleOtheType == "xxxx={0}A中的{0}为数据，特殊处理")
                        {
                            finalValues = GetFinalValues(filteredData, 0);
                            num1 = GetResultValue(finalValues.First(), 'A');
                            // args = finalValues.Cast<object>().ToArray();
                            result = string.Format(rule.DataContent, num1);
                        }
                        //小于30A用设定值判断
                        else if (ruleOtheType == "小于30A是±0.3A，大于等于30A是±1%")
                        {
                            finalValues = GetFinalValues(filteredData, 0);
                            num1 = Math.Round(GetResultValue(finalValues.First(), 'A'),2);
                            result = num1 < 30 ? string.Format(rule.DataContent, "0.3A") : string.Format(rule.DataContent, "1.0%");
                        }
                        else if (ruleOtheType == "小于30A是{4}减设定电流单位A，大于等于30A是{4}减设定电流除以设定电流单位%" || ruleOtheType == "小于30A：({4}-设定电流) 单位A，大于等于30A：({4}-设定电流)/设定电流 单位%")
                        {
                            finalValues = GetFinalValues(filteredData, 0);
                            finalValues_Index4 = GetFinalValues(filteredData, 4);
                            num1 = GetResultValue(finalValues.First(), 'A');
                            num2 = DBConvert.ToDouble(finalValues_Index4.First());
                            num3 = Math.Round(num2 - num1, 2);
                            strTemp= (Math.Round((num2 - num1) / num1 * 100, 2)).ToString();
                            result = num1 < 30? string.Format(rule.DataContent, num3.ToString()+ "A") : string.Format(rule.DataContent, strTemp + "%");
                        }
                        else if (ruleOtheType == "({4}-设定电压)/设定电压 单位%")
                        {
                            finalValues = GetFinalValues(filteredData, 0);
                            finalValues_Index4 = GetFinalValues(filteredData, 4);
                            num1 = GetResultValue(finalValues.First(), 'V');
                            num2 = DBConvert.ToDouble(finalValues_Index4.First());
                            num3 = Math.Round((num2 - num1) / num1 * 100, 2);
                            //args = finalValues.Cast<object>().ToArray();
                            result = string.Format(rule.DataContent, num3);
                        }
                        else result = rule.DataContent;
                    }
                    else 
                    {
                        result = rule.DataContent;
                    }
                    dicResult[rule.Bookmark] = result;
                }
              
                // --------------------------
                // 汇总RES_XX结果
                // --------------------------
                Dictionary<string, string> dicMergeResult = new Dictionary<string, string>();
                Dictionary<string, string> dicTestResult = new Dictionary<string, string>();
                var trialGroups = allRules.GroupBy(r => r.TrialName);

                foreach (var group in trialGroups)
                {
                    var rulesInGroup = group.ToList();
                    string firstBk = rulesInGroup.First().Bookmark;
                    string resTag = ConvertToResTag(firstBk);
                    string testResultTag = ConvertToTestResultTag(firstBk);
                    string finalResult = "";

                    var allAllPass = rulesInGroup
                        .Where(r => ruleAllPassDict.ContainsKey(r))
                        .Select(r => ruleAllPassDict[r])
                        .ToList();
                    var validResults = allAllPass.Where(x => x.HasValue).ToList();
                    int totalCount = allAllPass.Count;

                    if (totalCount == 1)
                    {
                        var val = allAllPass.First();
                        if (val == null) finalResult = "未检测";
                        else if (val == true) finalResult = "合格";
                        else finalResult = "不合格";
                    }
                    else if (totalCount > 1)
                    {
                        bool hasFail = validResults.Any(x => x == false);
                        finalResult = hasFail ? "不合格" : "合格";
                    }
                    else
                    {
                        finalResult = "未检测";
                    }

                    dicMergeResult[resTag] = finalResult;
                    dicTestResult[testResultTag] = finalResult;
                }

                //写入Word书签
                foreach (var kv in dicResult) ReplaceBookmark(doc, kv.Key, kv.Value);
                foreach (var kv in dicMergeResult) ReplaceBookmark(doc, kv.Key, kv.Value);
                foreach (var kv in dicTestResult) ReplaceBookmark(doc, kv.Key, kv.Value);
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }
        public static double GetResultValue(string str,char unit)
        {
            if (string.IsNullOrWhiteSpace(str))
                return 0;

            // 先找=
            string[] split1 = str.Split('=');
            if (split1.Length < 2)
                return 0;

            string right = split1[1];
            string numStr = right.Split(unit)[0].Trim();

            if (double.TryParse(numStr, out double val))
                return val;

            return 0;
        }
        public static List<string> GetFinalValues(List<TrialDataModel> filteredData,int index)
        {
            List<string> finalValues = new List<string>();
            string[] data2Parts = Array.Empty<string>();
            string value = string.Empty;
            foreach (var data in filteredData)
            {
                if (string.IsNullOrWhiteSpace(data.Data2))
                {
                    finalValues.Add("");
                    continue;
                }
                data2Parts = data.Data2.Split(new[] { '|' }, StringSplitOptions.None);
                value = index >= 0 && index < data2Parts.Length ? data2Parts[index].Trim() : "";
                finalValues.Add(value);
            }
            return finalValues;
        }
        /// <summary>
        /// 书签转换：KS_1_001 → RES_1
        /// </summary>
        /// <param name="bookmark"></param>
        /// <returns></returns>
        private static string ConvertToResTag(string bookmark)
        {
            try
            {
                string[] parts = bookmark.Split('_');
                if (parts.Length >= 2)
                {
                    return $"RES_{parts[1]}";
                }
            }
            catch { }

            return "RES_NONE";
        }
        /// <summary>
        /// 书签转换：KS_1_001 → TestResult_1
        /// </summary>
        /// <param name="bookmark"></param>
        /// <returns></returns>
        private static string ConvertToTestResultTag(string bookmark)
        {
            try
            {
                string[] parts = bookmark.Split('_');
                if (parts.Length >= 2)
                {
                    return $"TestResult_{parts[1]}";
                }
            }
            catch { }

            return "TestResult_NONE";
        }
        /// <summary>
        /// KS客户要求的报告生成方法：基于Word模板，直接替换书签，不自动生成表格。最终结果严格按照你的要求来判断（单项目=合格/不合格/未检测，多项目=只要有一个不合格就是不合格）。注意：前提是数据库中的 TemplateRule 表必须正确配置好每个规则对应的书签、数据类型、数据内容等信息。
        /// </summary>
        /// <param name="lstTrialDatas"></param>
        /// <param name="chargerInfos"></param>
        /// <param name="lstSchemeName"></param>
        /// <param name="WordPath"></param>
        /// <returns></returns>
        public static bool CreateFileToKS(List<TrialDataModel> lstTrialDatas, ChargerInfoModel chargerInfos, List<string> lstSchemeName, ref string WordPath)
        {
            try
            {
                string strSchemeNames = "";
                for (int i = 0; i < lstSchemeName.Count; i++)
                {
                    strSchemeNames += "'" + lstSchemeName[i] + "',";
                }
                strSchemeNames = strSchemeNames.TrimStart('\'').TrimEnd(new char[] { ',', '\'' });
                lstTrialItems.Clear();
                TrialItemsManage.GetTrialSchemeFromSchemeName(strSchemeNames, ref lstTrialItems);
                LstTrialData = lstTrialDatas;

                string Pkid = lstTrialDatas[0].PKID.ToString();
                string GetNowLongData = DateTime.Now.ToString("yyyy-MM-dd");
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "报表(勿删)", "Word", GetNowLongData);
                string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "报表(勿删)","TestTemp.docx");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                if (!File.Exists(templatePath))
                {
                    MessageBox.Show("模板文件不存在！");
                    return false;
                }
                
                WordPath = Path.Combine(path, $"{Pkid}{strSchemeNames}.docx");
                File.Copy(templatePath, WordPath, true);
                XWPFDocument doc ;
                using (FileStream fs = new FileStream(WordPath, FileMode.Open, FileAccess.ReadWrite))
                {
                    doc = new XWPFDocument(fs); // 读取现有模板
                }
                //核心逻辑：直接基于模板书签生成报告
                CreateHeadToKS(doc, chargerInfos);

                // 直接保存
                using (FileStream out1 = new FileStream(WordPath, FileMode.Create))
                {
                    doc.Write(out1);
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                return false;
            }
        }


        private class clsItemMergeCell
        {
            public int StartRowIndex = 0;
            public int EndRowIndex = 0;
        }
        
        private static XWPFDocument CreateHead(ChargerInfoModel chargerModel)
        {
            try
            {
                XWPFDocument doc = new XWPFDocument(); //文档

                #region ---封面---
                XWPFParagraph p1 = doc.CreateParagraph(); //段落
                p1.Alignment = ParagraphAlignment.CENTER; //字体居中
                p1.IndentationFirstLine = (int)100; //首行缩进

                XWPFRun r1 = p1.CreateRun();                //向该段落中添加文字
                XWPFTableRow m_Row = null;
                #region 江阴代码（封面不同）
                string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
                if (Customer != null && Customer.Contains("FRGK"))
                {
                    r1.SetText("江阴市富仁高科股份有限公司");
                    r1.FontSize = 16;
                    XWPFParagraph p2 = doc.CreateParagraph();
                    p2.Alignment = ParagraphAlignment.CENTER;

                    r1.AddBreak(BreakType.TEXTWRAPPING);//换行
                    XWPFRun r2 = p2.CreateRun();
                    r2.SetText("电动汽车直流充电桩检测报告");
                    r2.FontSize = 22;


                    r2.AddBreak(BreakType.TEXTWRAPPING);//换行
                    r2.AddBreak(BreakType.TEXTWRAPPING);//换行
                    XWPFRun r3 = p2.CreateRun();
                    r3.SetText("No.:");
                    r3.FontSize = 10;

                    XWPFTable table = doc.CreateTable(1, 2);
                    table.RemoveRow(0);//去掉第一行空白的
                    table.GetCTTbl().AddNewTblPr().tblLayout = new CT_TblLayoutType();
                    table.SetColumnWidth(0, 2200);//1920分辨率的总宽度是8522
                    table.SetColumnWidth(1, 5000);




                    m_Row = CreateRows("产品名称", 1, table);
                    string txtValue = "电动汽车直流充电桩";// chargerModel.ProductName;
                    CreateColumn(m_Row, 1, txtValue);

                    m_Row = CreateRows("产品型号", 1, table);
                    txtValue = chargerModel.ProductModel;
                    CreateColumn(m_Row, 1, txtValue);

                    m_Row = CreateRows("产品编号", 1, table);
                    txtValue = chargerModel.BarCode;
                    CreateColumn(m_Row, 1, txtValue);



                    m_Row = CreateRows("校验员", 1, table);

                    txtValue = chargerModel.Operater;
                    CreateColumn(m_Row, 1, "");

                    m_Row = CreateRows("审核", 1, table);
                    txtValue = chargerModel.Auditor;
                    CreateColumn(m_Row, 1, "");

                    m_Row = CreateRows("检验日期时间", 1, table);
                    txtValue = LstTrialData[0].SaveTime;
                    CreateColumn(m_Row, 1, txtValue);
                }




                #endregion



                #region  通用代码
                else
                {
                    r1.SetText("检  验  报  告");
                    r1.FontSize = 32;
                    XWPFParagraph p2 = doc.CreateParagraph();
                    p2.Alignment = ParagraphAlignment.CENTER;

                    r1.AddBreak(BreakType.TEXTWRAPPING);//换行
                    r1.AddBreak(BreakType.TEXTWRAPPING);//换行
                    r1.AddBreak(BreakType.TEXTWRAPPING);//换行
                    XWPFRun r2 = p2.CreateRun();
                    r2.SetText("No.:");
                    r2.FontSize = 10;

                    XWPFTable table = doc.CreateTable(1, 2);
                    table.RemoveRow(0);//去掉第一行空白的
                    table.GetCTTbl().AddNewTblPr().tblLayout = new CT_TblLayoutType();
                    table.SetColumnWidth(0, 2200);//1920分辨率的总宽度是8522
                    table.SetColumnWidth(1, 5000);


                    if (Customer != null && Customer.Contains("ZD"))
                    {
                        m_Row = CreateRows("测试标准", 1, table);
                        CreateColumn(m_Row, 1, "");
                    }

                    m_Row = CreateRows("产品名称", 1, table);
                    string txtValue = chargerModel.ProductName;
                    CreateColumn(m_Row, 1, txtValue);

                    m_Row = CreateRows("产品型号", 1, table);
                    txtValue = chargerModel.ProductModel;
                    CreateColumn(m_Row, 1, txtValue);

                    if (Customer != null && Customer.Contains("ZD"))
                    {
                        m_Row = CreateRows("产品电气参数", 1, table);
                        CreateColumn(m_Row, 1, "");
                    }

                    if (Customer != null && Customer.Contains("HYQCP"))
                    {
                        m_Row = CreateRows("样品编号", 1, table);
                        txtValue = chargerModel.BarCode;
                        CreateColumn(m_Row, 1, txtValue);

                        m_Row = CreateRows("委托单编号", 1, table);
                        txtValue = chargerModel.RES1;
                        CreateColumn(m_Row, 1, txtValue);
                    }
                    else
                    {
                        m_Row = CreateRows("产品编号", 1, table);
                        txtValue = chargerModel.BarCode;
                        CreateColumn(m_Row, 1, txtValue);

                        m_Row = CreateRows("订单号", 1, table);
                        txtValue = "";
                        CreateColumn(m_Row, 1, txtValue);
                    }

                    m_Row = CreateRows("操作人", 1, table);
                    txtValue = chargerModel.Operater;
                    CreateColumn(m_Row, 1, txtValue);

                    m_Row = CreateRows("审核人", 1, table);
                    txtValue = chargerModel.Auditor;
                    CreateColumn(m_Row, 1, txtValue);

                    m_Row = CreateRows("检验日期时间", 1, table);
                    //txtValue = LstTrialData[0].SaveTime;
                    //去掉毫秒
                    txtValue = LstTrialData[0].SaveTime.Remove(LstTrialData[0].SaveTime.Length - 3);
                    CreateColumn(m_Row, 1, txtValue);
                }


                #endregion

                #endregion

                NextPage(doc);
                Dictionary<string, string> dicResult = new Dictionary<string, string>();
                foreach (var item in _LstTrialData)
                {
                    if (!dicResult.Keys.Contains(item.TrialName))
                    {
                        string finalResult = "";
                        string trialCondition = "";
                        TrialItemResultTmpManage.GetTrialFinalResultFromBarcode(chargerModel.BarCode, (EmTrialType)item.TrialType, ref finalResult, ref trialCondition);
                        //finalResult = TrialItemResultTmpManage.GetTrialFinalResultFromData(chargerModel.BarCode, (EmTrialType)item.TrialType, item.TrialName, item.ChargerId);
                        if (string.IsNullOrEmpty(finalResult))
                            finalResult = TrialItemResultTmpManage.GetTrialFinalResultFromData(chargerModel.BarCode, (EmTrialType)item.TrialType, item.TrialName, item.ChargerId);
                        dicResult.Add(item.TrialName, finalResult);
                    }
                }
                #region ---- 第二页    序号  名称  结果----


                XWPFTable table1 = doc.CreateTable(1, 3);
                table1.RemoveRow(0);//去掉第一行空白的
                table1.GetCTTbl().AddNewTblPr().tblLayout = new CT_TblLayoutType();

                table1.SetColumnWidth(0, 1400);
                table1.SetColumnWidth(1, 4400);
                table1.SetColumnWidth(2, 1400);

                //第一行 第一列
                XWPFTableRow row = CreateRows("序号", 1, table1);
                CreateColumn(row, 1, "名称");
                CreateColumn(row, 1, "结果");

                int index = 1;
                if (lstTrialItems.Count <= 0)
                {
                    //根据数据顺序来排序
                    foreach (var item in dicResult)
                    {
                        row = CreateRows(index.ToString().PadLeft(2, '0'), 1, table1);
                        index++;
                        CreateColumn(row, 1, item.Key);
                        CreateColumn(row, 1, item.Value);
                    }
                }
                else
                {
                    //下面是根据方案顺序来排序
                    foreach (StTrialItem lti in lstTrialItems)
                    {
                        foreach (var item in dicResult)
                        {
                            if (lti.ItemName == item.Key)
                            {
                                row = CreateRows(index.ToString().PadLeft(2, '0'), 1, table1);
                                index++;
                                CreateColumn(row, 1, item.Key);
                                CreateColumn(row, 1, item.Value);
                                break;
                            }
                        }
                    }
                }
                #endregion

                NextPage(doc);

                #region ----------第三页至最后    检测项具体数据表格------------

                int columnsCount = 6;//列数量
                int count = 1;

                if (lstTrialItems.Count <= 0)//没有方案
                {
                    foreach (var item in DicTrialTypeData)
                    {
                        string finalResult = "";
                        string trialCondition = "";
                        TrialItemResultTmpManage.GetTrialFinalResultFromBarcode(chargerModel.BarCode, (EmTrialType)item.Key, ref finalResult, ref trialCondition);
                        int val = columnsCount;
                        XWPFTable tbDetailData = doc.CreateTable(1, columnsCount);

                        tbDetailData.RemoveRow(0);//去掉第一行空白的              

                        tbDetailData.GetCTTbl().AddNewTblPr().tblLayout = new CT_TblLayoutType();

                        for (int i = 0; i < columnsCount; i++)
                        {
                            tbDetailData.SetColumnWidth(i, (ulong)(8200 / columnsCount));
                        }

                        //第一行 第一列  测试项名称
                        CreateRows(count.ToString().PadLeft(2, '0') + " " + item.Value[0].TrialName, val, tbDetailData);

                        //第二行   第一列 “测试项描述”
                        m_Row = CreateRows(strTitleDescripe, 1, tbDetailData);

                        StTrialItem st = lstTrialItems.Find(s => s.TrialType == item.Value[0].TrialType);
                        //第二行   第二列       测试项具体描述        
                        CreateColumn(m_Row, val - 1, st.TrialMethod);

                        //第三行   第一列 “参考标准”
                        m_Row = CreateRows(strTitleStandard, 1, tbDetailData);
                        //第三行 第二列           “参考标准” 具体描述      
                        CreateColumn(m_Row, val - 1, st.DecideStandard);


                        //第四行  第一列 “用户设置”

                        int fromRowIndex = 0;
                        int toRowIndex = 0;
                        if (st.ResultParams.Trim() != "")
                        {
                            string[] UserParams = st.ResultParams.Split('|');
                            int paramsLastRowCellCount = UserParams.Length % (columnsCount - 1); //例如12个参数，那么前两行5个单元格，最后一行只有两个单元格

                            int paramsRowCount = UserParams.Length / (columnsCount - 1);//一行5个参数。 例如12个参数，则创建3行
                            if (paramsLastRowCellCount != 0)//刚好是5的倍数时不加1
                            {
                                paramsRowCount += 1;
                            }
                            for (int i = 0; i < paramsRowCount; i++)
                            {
                                m_Row = CreateRows(strUserParams, 1, tbDetailData);
                                int StartIndex = i * (columnsCount - 1);
                                int LastRowCellCount;
                                if (paramsRowCount - 1 == i)//最后一行
                                {
                                    LastRowCellCount = paramsLastRowCellCount == 0 ? (columnsCount - 1) : paramsLastRowCellCount;//余数为0则添加5格，
                                }
                                else
                                {
                                    LastRowCellCount = (columnsCount - 1);
                                }
                                for (int j = 0; j < LastRowCellCount; j++)
                                {
                                    string strText = UserParams[j + StartIndex].Split('=')[0];
                                    CreateColumn(m_Row, 1, strText);
                                }
                                if (paramsRowCount - 1 == i && paramsLastRowCellCount != 0)//最后一行 且单元格没添加满，则用空单元补齐
                                {
                                    for (int j = 0; j < 5 - paramsLastRowCellCount; j++)
                                    {
                                        CreateColumn(m_Row, 1, "");
                                    }
                                }
                                m_Row = CreateRows(strUserParams, 1, tbDetailData);
                                for (int j = 0; j < LastRowCellCount; j++)
                                {
                                    string strText = UserParams[j + StartIndex].Split('=')[1];
                                    CreateColumn(m_Row, 1, strText);
                                }
                                if (paramsRowCount - 1 == i && paramsLastRowCellCount != 0)//最后一行 且单元格没添加满，则用空单元补齐
                                {
                                    for (int j = 0; j < 5 - paramsLastRowCellCount; j++)
                                    {
                                        CreateColumn(m_Row, 1, "");
                                    }
                                }
                            }

                            fromRowIndex = 3;
                            toRowIndex = fromRowIndex - 1 + paramsRowCount * 2;
                            MYMergeCells(tbDetailData, 0, 0, fromRowIndex, toRowIndex);
                        }
                        else
                        {
                            fromRowIndex = 3;
                            toRowIndex = fromRowIndex - 1;
                        }

                        //第六行  第一列 “测试条件记录”
                        if (trialCondition.Trim() != "")
                        {
                            List<string> Conditions = trialCondition.Split('|').ToList();

                            if (Conditions.Count > 1)
                            {
                                int iCounter = Conditions.Count; // 用于处理没有重复的问题
                                do
                                {
                                    int CountNum = Conditions.Count;
                                    string srcCompareContent = Conditions[CountNum - 1];
                                    for (int i = 0; i < CountNum - 1; i++)
                                    {
                                        if (srcCompareContent == Conditions[i])
                                        {
                                            Conditions.RemoveAt(CountNum - 1);
                                            break;
                                        }
                                    }
                                    //if (Conditions[CountNum - 2] == Conditions[0] && Conditions[CountNum - 1] == Conditions[1])
                                    //{
                                    //    Conditions.RemoveAt(CountNum - 1);
                                    //    Conditions.RemoveAt(CountNum - 2);
                                    //}
                                    iCounter--;
                                } while (Conditions.Count > 1 && iCounter > 1);
                            }

                            int conditionRowCount = Conditions.Count / (columnsCount - 1) + 1;//一行5个参数。 例如12个参数，则创建3行
                            int conditionLastRowCellCount = Conditions.Count % (columnsCount - 1); //例如12个参数，那么前两行5个单元格，最后一行只有两个单元格
                            for (int i = 0; i < conditionRowCount; i++)
                            {
                                m_Row = CreateRows(strTrialCondition, 1, tbDetailData);
                                int StartIndex = i * (columnsCount - 1);
                                int LastRowCellCount;
                                if (conditionRowCount - 1 == i)//最后一行
                                {
                                    LastRowCellCount = conditionLastRowCellCount == 0 ? (columnsCount - 1) : conditionLastRowCellCount;//余数为0则添加5格，
                                }
                                else
                                {
                                    LastRowCellCount = (columnsCount - 1);
                                }
                                for (int j = 0; j < LastRowCellCount; j++)
                                {
                                    if (Conditions.Count() > 0)
                                    {
                                        string strText = Conditions[j + StartIndex].Split('=')[0];
                                        CreateColumn(m_Row, 1, strText);
                                    }
                                }
                                if (conditionRowCount - 1 == i && conditionLastRowCellCount != 0)//最后一行 且单元格没添加满，则用空单元补齐
                                {
                                    for (int j = 0; j < 5 - conditionLastRowCellCount; j++)
                                    {
                                        CreateColumn(m_Row, 1, "");
                                    }
                                }
                                m_Row = CreateRows(strTrialCondition, 1, tbDetailData);
                                for (int j = 0; j < LastRowCellCount; j++)
                                {
                                    if (Conditions.Count() > 0)
                                    {
                                        string strText = "";
                                        if (Conditions[j + StartIndex] == "")
                                        {
                                            strText = "";
                                        }
                                        else
                                        {
                                            strText = Conditions[j + StartIndex].Split('=')[1];
                                        }
                                        CreateColumn(m_Row, 1, strText);
                                    }
                                }
                                if (conditionRowCount - 1 == i && conditionLastRowCellCount != 0)//最后一行 且单元格没添加满，则用空单元补齐
                                {
                                    for (int j = 0; j < 5 - conditionLastRowCellCount; j++)
                                    {
                                        CreateColumn(m_Row, 1, "");
                                    }
                                }
                            }
                            fromRowIndex = toRowIndex + 1;
                            toRowIndex = fromRowIndex - 1 + conditionRowCount * 2;
                            MYMergeCells(tbDetailData, 0, 0, fromRowIndex, toRowIndex);
                        }

                        //第八行 第一列 测试阶段
                        m_Row = CreateRows("测试阶段", 1, tbDetailData);
                        CreateColumn(m_Row, 1, "测试项");
                        CreateColumn(m_Row, 1, "Min");
                        CreateColumn(m_Row, 1, "Max");
                        CreateColumn(m_Row, 1, "实测值");
                        CreateColumn(m_Row, 1, "结果");
                        toRowIndex += 1;

                        //---------------以上为固定行，以下为测试数据，根据数据库里的数据动态加载行数，每个测试项行数不同----------

                        int rowIndex = toRowIndex + 1;
                        string strStartContent = string.Empty;
                        bool bStartContent = false;
                        List<clsItemMergeCell> clsItemMergeCells = new List<clsItemMergeCell>();
                        //Dictionary<string, int[]> dicTrialPhase = new Dictionary<string, int[]>();//状态描述的起始行，用以存储合并单元格的起始行号
                        Dictionary<string, string> dicImagePath = new Dictionary<string, string>();//包含示波器截图的路径 key:截图的测试项点位  value-截图路径
                        foreach (var temp in item.Value)
                        {

                            string[] strDatas = temp.Data2.Split('|');
                            string strDescripe = strDatas[0];
                            //if (!dicTrialPhase.ContainsKey(strDescripe))
                            //{
                            //    dicTrialPhase.Add(strDescripe, rowIndex);
                            //}
                            //else
                            //{
                            //    dicTrialPhase[strDescripe] = rowIndex;
                            //}
                            if (bStartContent)
                            {
                                if (strStartContent != strDescripe)
                                {
                                    // 起始行和结束行判断
                                    clsItemMergeCells.Add(new clsItemMergeCell());
                                    clsItemMergeCells[clsItemMergeCells.Count - 1].StartRowIndex = rowIndex;
                                    clsItemMergeCells[clsItemMergeCells.Count - 1].EndRowIndex = rowIndex;
                                    strStartContent = strDescripe;
                                }
                                else
                                {
                                    clsItemMergeCells[clsItemMergeCells.Count - 1].EndRowIndex = rowIndex;
                                }
                            }
                            else
                            {
                                bStartContent = true;
                                clsItemMergeCells.Add(new clsItemMergeCell());
                                clsItemMergeCells[clsItemMergeCells.Count - 1].StartRowIndex = rowIndex;
                                clsItemMergeCells[clsItemMergeCells.Count - 1].EndRowIndex = rowIndex;
                                strStartContent = strDescripe;
                            }

                            m_Row = CreateRows(strDatas[0], 1, tbDetailData);
                            for (int i = 1; i < strDatas.Length; i++)
                            {
                                if (!strDatas[i].Contains("报表(勿删)"))
                                {
                                    CreateColumn(m_Row, 1, strDatas[i]);
                                }

                                else if (strDatas[i].Contains("报表(勿删)\\Image\\"))
                                {
                                    string key = strDatas[0] + ":" + strDatas[1];
                                    if (dicImagePath.ContainsKey(key))
                                    {
                                        key = temp.ItemName + strDatas[0] + ":" + strDatas[1];
                                    }
                                    dicImagePath.Add(key, strDatas[i]);
                                }
                            }
                            CreateColumn(m_Row, 1, temp.TrialResult.ToString());
                            rowIndex++;
                        }

                        //对相同的测试项描述合并行 
                        //foreach (var kvp in dicTrialPhase)
                        //{
                        //    MYMergeCells(tbDetailData, 0, 0, rowIndex, kvp.Value);
                        //    rowIndex = kvp.Value + 1;
                        //}

                        //对相同的测试项描述合并行 
                        foreach (var kvp in clsItemMergeCells)
                        {
                            MYMergeCells(tbDetailData, 0, 0, kvp.StartRowIndex, kvp.EndRowIndex);
                        }

                        if (Customer != null && Customer.Contains("ZD"))
                        {
                            m_Row = CreateRows("测试日期", 1, tbDetailData);
                            CreateColumn(m_Row, 2, "");
                            CreateColumn(m_Row, 1, "测试人员");
                            CreateColumn(m_Row, 2, "");

                            m_Row = CreateRows("被测样品", 1, tbDetailData);
                            CreateColumn(m_Row, 2, "");
                            CreateColumn(m_Row, 1, "环境条件");
                            CreateColumn(m_Row, 2, "");

                            m_Row = CreateRows("测试设备", 1, tbDetailData);
                            CreateColumn(m_Row, 5, "");
                        }

                        foreach (var kvp in dicImagePath)
                        {
                            m_Row = CreateRows("示波器截图", 1, tbDetailData);
                            CreateColumn(m_Row, 1, kvp.Key);
                            XWPFTableCell cell = CreateColumn(m_Row, columnsCount - 2, "");
                            CT_Tc tc = cell.GetCTTc();
                            CT_P ctp = tc.GetPList()[0];
                            XWPFParagraph gp = new XWPFParagraph(ctp, doc);//创建段落
                            XWPFRun gr = gp.CreateRun();//创建run

                            var widthEmus = (int)(350.0 * 9525);//图片的宽度
                            var heightEmus = (int)(300.0 * 9525);//图片的高度

                            if (File.Exists(kvp.Value))//确认文件是否存在
                            {
                                using (FileStream picData = new FileStream(kvp.Value, FileMode.Open, FileAccess.Read))
                                {
                                    //图片的文件流   图片类型   图片名称   设置的宽度以及高度
                                    gr.AddPicture(picData, (int)PictureType.JPEG, kvp.Value.Split('\\')[3], widthEmus, heightEmus);
                                }
                            }
                            else
                            {

                            }
                        }

                        if (Customer != null && Customer.Contains("FRGK"))
                        {
                            NextPage(doc);    //江阴代码  一个测试项数据占一页
                        }
                        else
                        {
                            NextTable(doc);
                        }


                        //if (count < _LstTrialData.Count - 1)
                        //{
                        //    if (count % 2 == 0)
                        //    {
                        //        NextTable(doc);
                        //    }
                        //    else
                        //    {
                        //        NextPage(doc);
                        //    }
                        //}
                        count++;
                    }
                }
                else
                {
                    foreach (var lti in lstTrialItems)
                    {
                        foreach (var item in DicTrialTypeData)
                        {
                            if (lti.ItemName == item.Value[0].TrialName)
                            {
                                string finalResult = "";
                                string trialCondition = "";
                                TrialItemResultTmpManage.GetTrialFinalResultFromBarcode(chargerModel.BarCode, (EmTrialType)item.Key, ref finalResult, ref trialCondition);
                                int val = columnsCount;
                                XWPFTable tbDetailData = doc.CreateTable(1, columnsCount);

                                tbDetailData.RemoveRow(0);//去掉第一行空白的              

                                tbDetailData.GetCTTbl().AddNewTblPr().tblLayout = new CT_TblLayoutType();

                                for (int i = 0; i < columnsCount; i++)
                                {
                                    tbDetailData.SetColumnWidth(i, (ulong)(8200 / columnsCount));
                                }

                                //第一行 第一列  测试项名称
                                CreateRows(count.ToString().PadLeft(2, '0') + " " + item.Value[0].TrialName, val, tbDetailData);

                                //第二行   第一列 “测试项描述”
                                m_Row = CreateRows(strTitleDescripe, 1, tbDetailData);

                                StTrialItem st = lstTrialItems.Find(s => s.TrialType == item.Value[0].TrialType);
                                if (st == null)
                                    continue;
                                //第二行   第二列       测试项具体描述        
                                CreateColumn(m_Row, val - 1, st.TrialMethod);

                                //第三行   第一列 “参考标准”
                                m_Row = CreateRows(strTitleStandard, 1, tbDetailData);
                                //第三行 第二列           “参考标准” 具体描述      
                                CreateColumn(m_Row, val - 1, st.DecideStandard);


                                //第四行  第一列 “用户设置”

                                int fromRowIndex = 0;
                                int toRowIndex = 0;
                                if (st.ResultParams.Trim() != "" || st.ResultParams.IndexOf("=") > -1)
                                {
                                    string[] UserParams = st.ResultParams.Split('|');
                                    int paramsLastRowCellCount = UserParams.Length % (columnsCount - 1); //例如12个参数，那么前两行5个单元格，最后一行只有两个单元格

                                    int paramsRowCount = UserParams.Length / (columnsCount - 1);//一行5个参数。 例如12个参数，则创建3行
                                    if (paramsLastRowCellCount != 0)//刚好是5的倍数时不加1
                                    {
                                        paramsRowCount += 1;
                                    }
                                    for (int i = 0; i < paramsRowCount; i++)
                                    {
                                        m_Row = CreateRows(strUserParams, 1, tbDetailData);
                                        int StartIndex = i * (columnsCount - 1);
                                        int LastRowCellCount;
                                        if (paramsRowCount - 1 == i)//最后一行
                                        {
                                            LastRowCellCount = paramsLastRowCellCount == 0 ? (columnsCount - 1) : paramsLastRowCellCount;//余数为0则添加5格，
                                        }
                                        else
                                        {
                                            LastRowCellCount = (columnsCount - 1);
                                        }
                                        for (int j = 0; j < LastRowCellCount; j++)
                                        {
                                            if (j + StartIndex >= UserParams.Length || UserParams[j + StartIndex].Split('=').Length < 1)
                                                break;
                                            string strText = UserParams[j + StartIndex].Split('=')[0];
                                            CreateColumn(m_Row, 1, strText);
                                        }
                                        if (paramsRowCount - 1 == i && paramsLastRowCellCount != 0)//最后一行 且单元格没添加满，则用空单元补齐
                                        {
                                            for (int j = 0; j < 5 - paramsLastRowCellCount; j++)
                                            {
                                                CreateColumn(m_Row, 1, "");
                                            }
                                        }
                                        m_Row = CreateRows(strUserParams, 1, tbDetailData);
                                        for (int j = 0; j < LastRowCellCount; j++)
                                        {
                                            string strText = UserParams[j + StartIndex].Split('=')[1];
                                            CreateColumn(m_Row, 1, strText);
                                        }
                                        if (paramsRowCount - 1 == i && paramsLastRowCellCount != 0)//最后一行 且单元格没添加满，则用空单元补齐
                                        {
                                            for (int j = 0; j < 5 - paramsLastRowCellCount; j++)
                                            {
                                                CreateColumn(m_Row, 1, "");
                                            }
                                        }
                                    }

                                    fromRowIndex = 3;
                                    toRowIndex = fromRowIndex - 1 + paramsRowCount * 2;
                                    MYMergeCells(tbDetailData, 0, 0, fromRowIndex, toRowIndex);
                                }
                                else
                                {
                                    fromRowIndex = 3;
                                    toRowIndex = fromRowIndex - 1;
                                }

                                //第六行  第一列 “测试条件记录”
                                if (trialCondition.Trim() != "")
                                {
                                    List<string> Conditions = trialCondition.Split('|').ToList();
                                    int itmp = 0;

                                    if (Conditions.Count > 2)
                                    {
                                        do
                                        {
                                            int CountNum = Conditions.Count;
                                            if ((Conditions[CountNum - 2] == Conditions[0] && Conditions[CountNum - 1] == Conditions[1])
                                                || (Conditions[CountNum - 2].Split('=')[0] == Conditions[0].Split('=')[0] && Conditions[CountNum - 1].Split('=')[0] == Conditions[1].Split('=')[0]))
                                            {
                                                Conditions.RemoveAt(CountNum - 1);
                                                Conditions.RemoveAt(CountNum - 2);
                                            }
                                            if (++itmp > Conditions.Count)
                                            {
                                                break;
                                            }
                                        } while (Conditions.Count > 2);
                                    }

                                    int conditionRowCount = Conditions.Count / (columnsCount - 1) + 1;//一行5个参数。 例如12个参数，则创建3行
                                    int conditionLastRowCellCount = Conditions.Count % (columnsCount - 1); //例如12个参数，那么前两行5个单元格，最后一行只有两个单元格
                                    for (int i = 0; i < conditionRowCount; i++)
                                    {
                                        m_Row = CreateRows(strTrialCondition, 1, tbDetailData);
                                        int StartIndex = i * (columnsCount - 1);
                                        int LastRowCellCount;
                                        if (conditionRowCount - 1 == i)//最后一行
                                        {
                                            LastRowCellCount = conditionLastRowCellCount == 0 ? (columnsCount - 1) : conditionLastRowCellCount;//余数为0则添加5格，
                                        }
                                        else
                                        {
                                            LastRowCellCount = (columnsCount - 1);
                                        }
                                        for (int j = 0; j < LastRowCellCount; j++)
                                        {
                                            if (Conditions.Count() > 0)
                                            {
                                                string strText = Conditions[j + StartIndex].Split('=')[0];
                                                CreateColumn(m_Row, 1, strText);
                                            }
                                        }
                                        if (conditionRowCount - 1 == i && conditionLastRowCellCount != 0)//最后一行 且单元格没添加满，则用空单元补齐
                                        {
                                            for (int j = 0; j < 5 - conditionLastRowCellCount; j++)
                                            {
                                                CreateColumn(m_Row, 1, "");
                                            }
                                        }
                                        m_Row = CreateRows(strTrialCondition, 1, tbDetailData);
                                        for (int j = 0; j < LastRowCellCount; j++)
                                        {
                                            if (Conditions.Count() > 0)
                                            {
                                                string strText = "";
                                                if (Conditions[j + StartIndex] == "")
                                                {
                                                    strText = "";
                                                }
                                                else
                                                {
                                                    strText = Conditions[j + StartIndex].Split('=')[1];
                                                }
                                                CreateColumn(m_Row, 1, strText);
                                            }
                                        }
                                        if (conditionRowCount - 1 == i && conditionLastRowCellCount != 0)//最后一行 且单元格没添加满，则用空单元补齐
                                        {
                                            for (int j = 0; j < 5 - conditionLastRowCellCount; j++)
                                            {
                                                CreateColumn(m_Row, 1, "");
                                            }
                                        }
                                    }
                                    fromRowIndex = toRowIndex + 1;
                                    toRowIndex = fromRowIndex - 1 + conditionRowCount * 2;
                                    MYMergeCells(tbDetailData, 0, 0, fromRowIndex, toRowIndex);
                                }

                                //第八行 第一列 测试阶段
                                m_Row = CreateRows("测试阶段", 1, tbDetailData);
                                CreateColumn(m_Row, 1, "测试项");
                                CreateColumn(m_Row, 1, "Min");
                                CreateColumn(m_Row, 1, "Max");
                                CreateColumn(m_Row, 1, "实测值");
                                CreateColumn(m_Row, 1, "结果");
                                toRowIndex += 1;

                                //---------------以上为固定行，以下为测试数据，根据数据库里的数据动态加载行数，每个测试项行数不同----------

                                int rowIndex = toRowIndex + 1;
                                string strStartContent = string.Empty;
                                bool bStartContent = false;
                                List<clsItemMergeCell> clsItemMergeCells = new List<clsItemMergeCell>();
                                //Dictionary<string, int[]> dicTrialPhase = new Dictionary<string, int[]>();//状态描述的起始行，用以存储合并单元格的起始行号
                                Dictionary<string, string> dicImagePath = new Dictionary<string, string>();//包含示波器截图的路径 key:截图的测试项点位  value-截图路径
                                foreach (var temp in item.Value)
                                {

                                    string[] strDatas = temp.Data2.Split('|');
                                    string strDescripe = strDatas[0];
                                    //if (!dicTrialPhase.ContainsKey(strDescripe))
                                    //{
                                    //    dicTrialPhase.Add(strDescripe, rowIndex);
                                    //}
                                    //else
                                    //{
                                    //    dicTrialPhase[strDescripe] = rowIndex;
                                    //}
                                    if (bStartContent)
                                    {
                                        if (strStartContent != strDescripe)
                                        {
                                            // 起始行和结束行判断
                                            clsItemMergeCells.Add(new clsItemMergeCell());
                                            clsItemMergeCells[clsItemMergeCells.Count - 1].StartRowIndex = rowIndex;
                                            clsItemMergeCells[clsItemMergeCells.Count - 1].EndRowIndex = rowIndex;
                                            strStartContent = strDescripe;
                                        }
                                        else
                                        {
                                            clsItemMergeCells[clsItemMergeCells.Count - 1].EndRowIndex = rowIndex;
                                        }
                                    }
                                    else
                                    {
                                        bStartContent = true;
                                        clsItemMergeCells.Add(new clsItemMergeCell());
                                        clsItemMergeCells[clsItemMergeCells.Count - 1].StartRowIndex = rowIndex;
                                        clsItemMergeCells[clsItemMergeCells.Count - 1].EndRowIndex = rowIndex;
                                        strStartContent = strDescripe;
                                    }

                                    m_Row = CreateRows(strDatas[0], 1, tbDetailData);
                                    for (int i = 1; i < strDatas.Length; i++)
                                    {
                                        if (!strDatas[i].Contains("报表(勿删)"))
                                        {
                                            CreateColumn(m_Row, 1, strDatas[i]);
                                        }

                                        else if (strDatas[i].Contains("报表(勿删)\\Image\\"))
                                        {
                                            string key = strDatas[0] + ":" + strDatas[1];
                                            if (dicImagePath.ContainsKey(key))
                                            {
                                                key = temp.ItemName + strDatas[0] + ":" + strDatas[1];
                                                if(dicImagePath.ContainsKey(key))
                                                    key = temp.Data3 + temp.ItemName +  strDatas[0] + ":" + strDatas[1];
                                            }
                                            dicImagePath.Add(key, strDatas[i]);
                                        }
                                    }
                                    CreateColumn(m_Row, 1, temp.TrialResult.ToString());
                                    rowIndex++;
                                }

                                //对相同的测试项描述合并行 
                                //foreach (var kvp in dicTrialPhase)
                                //{
                                //    MYMergeCells(tbDetailData, 0, 0, rowIndex, kvp.Value);
                                //    rowIndex = kvp.Value + 1;
                                //}

                                //对相同的测试项描述合并行 
                                foreach (var kvp in clsItemMergeCells)
                                {
                                    MYMergeCells(tbDetailData, 0, 0, kvp.StartRowIndex, kvp.EndRowIndex);
                                }

                                if (Customer != null && Customer.Contains("ZD"))
                                {
                                    m_Row = CreateRows("测试日期", 1, tbDetailData);
                                    CreateColumn(m_Row, 2, "");
                                    CreateColumn(m_Row, 1, "测试人员");
                                    CreateColumn(m_Row, 2, "");

                                    m_Row = CreateRows("被测样品", 1, tbDetailData);
                                    CreateColumn(m_Row, 2, "");
                                    CreateColumn(m_Row, 1, "环境条件");
                                    CreateColumn(m_Row, 2, "");

                                    m_Row = CreateRows("测试设备", 1, tbDetailData);
                                    CreateColumn(m_Row, 5, "");
                                }

                                foreach (var kvp in dicImagePath)
                                {
                                    m_Row = CreateRows("示波器截图", 1, tbDetailData);
                                    CreateColumn(m_Row, 1, kvp.Key);
                                    XWPFTableCell cell = CreateColumn(m_Row, columnsCount - 2, "");
                                    CT_Tc tc = cell.GetCTTc();
                                    CT_P ctp = tc.GetPList()[0];
                                    XWPFParagraph gp = new XWPFParagraph(ctp, doc);//创建段落
                                    XWPFRun gr = gp.CreateRun();//创建run

                                    var widthEmus = (int)(350.0 * 9525);//图片的宽度
                                    var heightEmus = (int)(300.0 * 9525);//图片的高度

                                    if (File.Exists(kvp.Value))//确认文件是否存在
                                    {
                                        using (FileStream picData = new FileStream(kvp.Value, FileMode.Open, FileAccess.Read))
                                        {
                                            //图片的文件流   图片类型   图片名称   设置的宽度以及高度
                                            gr.AddPicture(picData, (int)PictureType.JPEG, kvp.Value.Split('\\')[3], widthEmus, heightEmus);
                                        }
                                    }
                                    else
                                    {

                                    }
                                }

                                if (Customer != null && Customer.Contains("FRGK"))
                                {
                                    NextPage(doc);    //江阴代码  一个测试项数据占一页
                                }
                                else
                                {
                                    NextTable(doc);
                                }


                                //if (count < _LstTrialData.Count - 1)
                                //{
                                //    if (count % 2 == 0)
                                //    {
                                //        NextTable(doc);
                                //    }
                                //    else
                                //    {
                                //        NextPage(doc);
                                //    }
                                //}
                                count++;
                            }
                        }
                    }
                }
                #endregion

                #region 页眉页脚设置
                if (Customer != null && Customer.Contains("ZD"))
                {
                    // 页眉 页脚
                    doc.Document.body.sectPr = new CT_SectPr();
                    CT_SectPr m_SectPr = doc.Document.body.sectPr;

                    //创建页脚
                    CT_Ftr m_ftr = new CT_Ftr();

                    string strPageFoot = "        ";
                    strPageFoot += "测试人员 :";
                    strPageFoot += "________________________";
                    strPageFoot += "        ";
                    strPageFoot += "审核人员 :";
                    strPageFoot += "________________________";

                    m_ftr.AddNewP().AddNewR().AddNewT().Value = strPageFoot;
                    //创建页脚关系（footern.xml）
                    XWPFRelation Frelation = XWPFRelation.FOOTER;
                    XWPFFooter m_f = (XWPFFooter)doc.CreateRelationship(Frelation, XWPFFactory.GetInstance(), doc.FooterList.Count + 1);

                    //设置页脚
                    m_f.SetHeaderFooter(m_ftr);
                    CT_HdrFtrRef m_HdrFtr1 = m_SectPr.AddNewFooterReference();

                    m_HdrFtr1.type = ST_HdrFtr.@default;
                    m_HdrFtr1.id = m_f.GetPackageRelationship().Id;
                }
                #endregion 页眉页脚设置

                return doc;
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                return null;
            }

        }





        #region =================== 操作word写入书签 =====================
        public static void ReplaceBookmark(XWPFDocument doc, string bookmarkName, string text)
        {
            if (doc == null) return;

            try
            {
                // ==========================================
                // 先处理 表格里的书签
                // ==========================================
                foreach (XWPFTable table in doc.Tables)
                {
                    foreach (XWPFTableRow row in table.Rows)
                    {
                        foreach (XWPFTableCell cell in row.GetTableCells())
                        {
                            foreach (XWPFParagraph para in cell.Paragraphs)
                            {
                                foreach (var bookmarkStart in para.GetCTP().GetBookmarkStartList())
                                {
                                    if (bookmarkStart.name == bookmarkName)
                                    {
                                        // 清空原有内容
                                        para.RemoveRun(0);
                                        while (para.Runs.Count > 0) para.RemoveRun(0);

                                        // 插入新内容
                                        XWPFRun run = para.CreateRun();
                                        run.SetText(text ?? "");
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }

                // ==========================================
                // 普通段落书签
                // ==========================================
                foreach (XWPFParagraph para in doc.Paragraphs)
                {
                    foreach (var bookmarkStart in para.GetCTP().GetBookmarkStartList())
                    {
                        if (bookmarkStart.name == bookmarkName)
                        {
                            // 清空原有内容
                            while (para.Runs.Count > 0) para.RemoveRun(0);

                            XWPFRun run = para.CreateRun();
                            run.SetText(text ?? "");
                            return;
                        }
                    }
                }
            }
            catch{}
        }
        #endregion


        public static bool CreateFile_List(List<TrialDataModel> lstTrialDatas, ChargerInfoModel chargerInfos, List<string> lstSchemeName, ref string WordPath)
        {
            try
            {
                string strSchemeNames = "";
                for (int i = 0; i < lstSchemeName.Count; i++)
                {
                    strSchemeNames += "'" + lstSchemeName[i] + "',";
                }
                strSchemeNames = strSchemeNames.TrimStart('\'').TrimEnd(new char[] { ',', '\'' });
                lstTrialItems.Clear();
                TrialItemsManage.GetTrialSchemeFromSchemeName(strSchemeNames, ref lstTrialItems);
                LstTrialData = lstTrialDatas;

                //指定Word文档的路径和名称

                // string path = "docx\\demo.docx";
                string Pkid = lstTrialDatas[0].PKID.ToString();
                string GetNowLongData = System.DateTime.Now.ToString("yyyy-MM-dd");
                string path = System.AppDomain.CurrentDomain.BaseDirectory + "报表(勿删)\\Word" + "\\" + GetNowLongData;
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }

                WordPath = path + "\\" + Pkid + strSchemeNames + "_List" + ".docx";

                XWPFDocument doc = CreateItemList(chargerInfos);

                for (int k = 0; k < doc.Document.body.GetTblArray().Count(); k++)
                {
                    CT_Tbl m_CTTbl = doc.Document.body.GetTblArray()[k];
                    m_CTTbl.AddNewTblPr().jc = new CT_Jc();
                    m_CTTbl.AddNewTblPr().jc.val = ST_Jc.center;//表在页面水平居中
                }
                FileStream out1 = new FileStream(WordPath, FileMode.Create);
                doc.Write(out1);
                out1.Close();


                return true;
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                return false;
            }
        }

        /// <summary>
        /// 创建测试项目列表（目前XJ在用）
        /// </summary>
        /// <param name="chargerModel"></param>
        /// <returns></returns>
        private static XWPFDocument CreateItemList(ChargerInfoModel chargerModel)
        {
            try
            {
                XWPFDocument doc = new XWPFDocument(); //文档
                #region 增加页眉
                XWPFHeaderFooterPolicy policy = doc.CreateHeaderFooterPolicy();
                XWPFHeader header = policy.CreateHeader(XWPFHeaderFooterPolicy.DEFAULT); 
                var cpHeader = header.CreateParagraph();
                cpHeader.Alignment = ParagraphAlignment.CENTER;
                cpHeader.CreateRun().SetText(" 许继电源有限公司                                                          产品出厂试验报告");
                cpHeader.BorderBottom = Borders.Single;
                #endregion

                #region ---封面---
                XWPFParagraph p1 = doc.CreateParagraph(); //段落
                p1.Alignment = ParagraphAlignment.CENTER; //字体居中
                p1.IndentationFirstLine = (int)100; //首行缩进

                XWPFRun r1 = p1.CreateRun();                //向该段落中添加文字
                XWPFTableRow m_Row = null;
                #region 江阴代码（封面不同）
                string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
                if (Customer != null && Customer.Contains("FRGK"))
                {
                    r1.SetText("江阴市富仁高科股份有限公司");
                    r1.FontSize = 16;
                    XWPFParagraph p2 = doc.CreateParagraph();
                    p2.Alignment = ParagraphAlignment.CENTER;

                    r1.AddBreak(BreakType.TEXTWRAPPING);//换行
                    XWPFRun r2 = p2.CreateRun();
                    r2.SetText("电动汽车直流充电桩检测报告");
                    r2.FontSize = 22;


                    r2.AddBreak(BreakType.TEXTWRAPPING);//换行
                    r2.AddBreak(BreakType.TEXTWRAPPING);//换行
                    XWPFRun r3 = p2.CreateRun();
                    r3.SetText("No.:");
                    r3.FontSize = 10;

                    XWPFTable table = doc.CreateTable(1, 2);
                    table.RemoveRow(0);//去掉第一行空白的
                    table.GetCTTbl().AddNewTblPr().tblLayout = new CT_TblLayoutType();
                    table.SetColumnWidth(0, 2200);//1920分辨率的总宽度是8522
                    table.SetColumnWidth(1, 5000);




                    m_Row = CreateRows("产品名称", 1, table);
                    string txtValue = "电动汽车直流充电桩";// chargerModel.ProductName;
                    CreateColumn(m_Row, 1, txtValue);

                    m_Row = CreateRows("产品型号", 1, table);
                    txtValue = chargerModel.ProductModel;
                    CreateColumn(m_Row, 1, txtValue);

                    m_Row = CreateRows("产品编号", 1, table);
                    txtValue = chargerModel.BarCode;
                    CreateColumn(m_Row, 1, txtValue);



                    m_Row = CreateRows("校验员", 1, table);

                    txtValue = chargerModel.Operater;
                    CreateColumn(m_Row, 1, "");

                    m_Row = CreateRows("审核", 1, table);
                    txtValue = chargerModel.Auditor;
                    CreateColumn(m_Row, 1, "");

                    m_Row = CreateRows("检验日期时间", 1, table);
                    txtValue = LstTrialData[0].SaveTime;
                    CreateColumn(m_Row, 1, txtValue);
                }




                #endregion



                #region  通用代码
                else
                {
                    r1.SetText("出  厂  试  验  报  告");
                    r1.FontSize = 30;
                    r1.IsBold = true;
                    XWPFParagraph p2 = doc.CreateParagraph();
                    p2.Alignment = ParagraphAlignment.CENTER;

                    r1.AddBreak(BreakType.TEXTWRAPPING);//换行
                    r1.AddBreak(BreakType.TEXTWRAPPING);//换行
                    r1.AddBreak(BreakType.TEXTWRAPPING);//换行
                    XWPFRun r2 = p2.CreateRun();
                    r2.SetText("No.:");
                    r2.FontSize = 10;
                    r2.IsBold = true;

                    XWPFTable table = doc.CreateTable(1, 2);
                    table.RemoveRow(0);//去掉第一行空白的
                    table.GetCTTbl().AddNewTblPr().tblLayout = new CT_TblLayoutType();
                    table.SetColumnWidth(0, 2200);//1920分辨率的总宽度是8522
                    table.SetColumnWidth(1, 5000);


                    m_Row = CreateRows("产品名称", 1, table);
                    string txtValue = chargerModel.ProductName;
                    CreateColumn(m_Row, 1, txtValue);
                    

                    m_Row = CreateRows("产品型号", 1, table);
                    txtValue = chargerModel.ProductModel;
                    CreateColumn(m_Row, 1, txtValue);


                    m_Row = CreateRows("合同编号", 1, table);
                    txtValue = chargerModel.RES1;
                    CreateColumn(m_Row, 1, txtValue);

                    m_Row = CreateRows("制造编号", 1, table);
                    txtValue = chargerModel.BarCode;
                    CreateColumn(m_Row, 1, txtValue);

                    XWPFParagraph p6 = doc.CreateParagraph();
                    p6.Alignment = ParagraphAlignment.CENTER;
                    XWPFRun r6 = p6.CreateRun();
                    r6.SetText("本产品按有关标准（或技术条件）规定的考核指标检验，试验结果全部合格，准予出厂。");
                    r6.FontSize = 10;
                    r6.IsBold = true;

                    r6.AddBreak(BreakType.TEXTWRAPPING);//换行
                    r6.AddBreak(BreakType.TEXTWRAPPING);//换行

                    XWPFParagraph p7 = doc.CreateParagraph();
                    p7.Alignment = ParagraphAlignment.RIGHT;
                    XWPFRun r7 = p7.CreateRun();
                    DateTime dtTestTime= DateTime.Now;
                    DateTime.TryParse(LstTrialData[0].SaveTime, out dtTestTime);
                    r7.SetText(dtTestTime.ToString("yyyy年MM月dd日") + "          ");
                    r7.FontSize = 12;
                    r7.IsBold = true;

                    r7.AddBreak(BreakType.TEXTWRAPPING);//换行
                    r7.AddBreak(BreakType.TEXTWRAPPING);//换行
                    r7.AddBreak(BreakType.TEXTWRAPPING);//换行

                    XWPFParagraph p8 = doc.CreateParagraph();
                    p8.Alignment = ParagraphAlignment.RIGHT;
                    XWPFRun r8 = p8.CreateRun();
                    r8.SetText("调试员：    " + chargerModel.Operater + "                                   ");
                    r8.FontSize = 12;
                    r8.IsBold = true;



                    XWPFParagraph p9 = doc.CreateParagraph();
                    p9.Alignment = ParagraphAlignment.RIGHT;
                    XWPFRun r9 = p9.CreateRun();
                    r9.SetText("检查员：    " + chargerModel.Auditor + "                                   ");
                    r9.FontSize = 12;
                    r9.IsBold = true;



                    if (Customer.Contains("XJ"))
                    {
                        XWPFParagraph p3 = doc.CreateParagraph();
                        p3.Alignment = ParagraphAlignment.CENTER;
                        XWPFRun r3 = p3.CreateRun();
                        r3.SetText("");
                        r3.FontSize = 15;

                        r3.AddBreak(BreakType.TEXTWRAPPING);//换行
                        r3.AddBreak(BreakType.TEXTWRAPPING);//换行
                        r3.AddBreak(BreakType.TEXTWRAPPING);//换行
                        r3.AddBreak(BreakType.TEXTWRAPPING);//换行
                        r3.AddBreak(BreakType.TEXTWRAPPING);//换行

                        XWPFParagraph p4 = doc.CreateParagraph();
                        p4.Alignment = ParagraphAlignment.CENTER;
                        XWPFRun r4 = p4.CreateRun();
                        r4.SetText("中华人民共和国");
                        r4.FontSize = 20;
                        r4.IsBold = true;

                        XWPFParagraph p5 = doc.CreateParagraph();
                        p5.Alignment = ParagraphAlignment.CENTER;
                        XWPFRun r5 = p5.CreateRun();
                        r5.SetText("许继电源有限公司");
                        r5.FontSize = 20;
                        r5.IsBold = true;
                    }
                }

                #endregion

                #endregion

                NextPage(doc);
                Dictionary<string, string> dicResult = new Dictionary<string, string>();
                foreach (var item in _LstTrialData)
                {
                    if (!dicResult.Keys.Contains(item.TrialName))
                    {
                        string finalResult = "";
                        string trialCondition = "";
                        TrialItemResultTmpManage.GetTrialFinalResultFromBarcode(chargerModel.BarCode, (EmTrialType)item.TrialType, ref finalResult, ref trialCondition);
                        dicResult.Add(item.TrialName, finalResult);
                    }
                }
                #region ---- 第二页    序号  名称  结果----


                XWPFParagraph p100 = doc.CreateParagraph(); //段落
                p100.Alignment = ParagraphAlignment.CENTER; //字体居中
                p100.IndentationFirstLine = (int)100; //首行缩进
                XWPFRun r100 = p100.CreateRun();                //向该段落中添加文字
                r100.SetText("检验记录单（电动汽车充电机）");
                r100.FontSize = 20;
                r100.IsBold = true;

                r100.AddBreak(BreakType.TEXTWRAPPING);//换行



                XWPFTable table1 = doc.CreateTable(1, 4);
                table1.RemoveRow(0);//去掉第一行空白的
                table1.GetCTTbl().AddNewTblPr().tblLayout = new CT_TblLayoutType();

                table1.SetColumnWidth(0, 1400);
                table1.SetColumnWidth(1, 4400);
                table1.SetColumnWidth(2, 1400);
                table1.SetColumnWidth(3, 1400);

                //第一行 第一列
                XWPFTableRow row = CreateRows("序号", 1, table1);
                CreateColumn(row, 1, "名称");
                CreateColumn(row, 1, "结果");
                CreateColumn(row, 1, "备注");

                int index = 1;
                if (lstTrialItems.Count <= 0)
                {
                    //根据数据顺序来排序
                    foreach (var item in dicResult)
                    {
                        row = CreateRows(index.ToString().PadLeft(2, '0'), 1, table1);
                        index++;
                        CreateColumn(row, 1, item.Key);
                        CreateColumn(row, 1, item.Value);
                        CreateColumn(row, 1, "");
                    }
                }
                else
                {
                    //下面是根据方案顺序来排序
                    foreach (var lti in lstTrialItems)
                    {
                        foreach (var item in dicResult)
                        {
                            if (lti.ItemName == item.Key)
                            {
                                row = CreateRows(index.ToString().PadLeft(2, '0'), 1, table1);
                                index++;
                                CreateColumn(row, 1, item.Key);
                                CreateColumn(row, 1, item.Value);
                                CreateColumn(row, 1, "");
                                break;
                            }
                        }
                    }
                }
                #endregion

                return doc;
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                return null;
            }

        }

        /// <summary>
        /// 创建单元格
        /// </summary>
        /// <param name="m_Row">要创建单元格的行</param>
        /// <param name="val">需要合并的列数量</param>
        /// <param name="txtValue">单元格文本</param>
        private static XWPFTableCell CreateColumn(XWPFTableRow m_Row, int val, string txtValue)
        {
            XWPFTableCell cell = m_Row.CreateCell();//创建单元格
            CT_Tc tc = cell.GetCTTc();
            CT_TcPr tcpr2 = tc.AddNewTcPr();
            tcpr2.gridSpan = new CT_DecimalNumber();
            tcpr2.gridSpan.val = val.ToString(); //合并列
            cell.SetVerticalAlignment(XWPFTableCell.XWPFVertAlign.CENTER);
            tc.GetPList()[0].AddNewPPr().AddNewJc().val = ST_Jc.center;
            tc.GetPList()[0].AddNewR().AddNewT().Value = txtValue;
            if (txtValue != null)
            {
                if (txtValue.ToUpper().Contains("FAIL"))
                {
                    cell.SetColor("red");
                }
            }
            return cell;
        }
        /// <summary>
        /// 给表格创建一行
        /// </summary>
        /// <param name="txtValue">第一行第一列的文本</param>
        /// <param name="val">需要合并的单元格数量</param>
        /// <param name="tbDetailData">表格名称</param>
        /// <returns></returns>
        private static XWPFTableRow CreateRows(string txtValue, int val, XWPFTable tbDetailData)
        {
            CT_Row m_NewRow = new CT_Row();
            XWPFTableRow m_Row = new XWPFTableRow(m_NewRow, tbDetailData);
            m_Row.GetCTRow().AddNewTrPr().AddNewTrHeight().val = (ulong)500;
            tbDetailData.AddRow(m_Row);
            XWPFTableCell cell = m_Row.CreateCell();
            CT_Tc cttc = cell.GetCTTc();
            CT_TcPr ctPr = cttc.AddNewTcPr();
            ctPr.gridSpan = new CT_DecimalNumber();
            ctPr.gridSpan.val = val.ToString(); //合并列
            cell.SetVerticalAlignment(XWPFTableCell.XWPFVertAlign.CENTER);
            cttc.GetPList()[0].AddNewPPr().AddNewJc().val = ST_Jc.center;
            cttc.GetPList()[0].AddNewR().AddNewT().Value = txtValue;
            return m_Row;
        }
        /// <summary>
        /// 切换到下一页
        /// </summary>
        /// <param name="doc"></param>
        private static void NextPage(XWPFDocument doc)
        {
            XWPFParagraph space = doc.CreateParagraph(); //段落
            XWPFRun temp = space.CreateRun();
            temp.SetText("    ");
            temp.AddBreak(BreakType.PAGE);//换页
        }
        /// <summary>
        /// 插入空格行
        /// </summary>
        /// <param name="doc"></param>
        private static void NextTable(XWPFDocument doc)
        {
            XWPFParagraph space = doc.CreateParagraph(); //段落
            XWPFRun temp = space.CreateRun();
            temp.SetText("    ");
            temp.AddBreak(BreakType.TEXTWRAPPING);//换行
            temp.AddBreak(BreakType.TEXTWRAPPING);//换行
            temp.AddBreak(BreakType.TEXTWRAPPING);//换行

        }


        /*
         * 表格中换行
         * var run= paragraph.CreateRun();
run.SetText(contends[i]);
run.FontSize = 11;
run.SetFontFamily("宋体", FontCharRange.None); 
run.AddBreak(BreakType.TEXTWRAPPING);//换行
         * */




        //读段落
        public string ExcuteWordText()
        {
            StringBuilder sb = new StringBuilder();
            using (FileStream stream = File.OpenRead(AppDomain.CurrentDomain.BaseDirectory + "\\" + "demo.docx"))
            {
                XWPFDocument doc = new XWPFDocument(stream);
                foreach (var para in doc.Paragraphs)
                {
                    string text = para.ParagraphText; //获得文本
                    var runs = para.Runs;
                    string styleid = para.Style;
                    for (int i = 0; i < runs.Count; i++)
                    {
                        var run = runs[i];
                        text = run.ToString(); //获得run的文本
                        sb.Append(text + ",");
                    }
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 设置字体格式
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="table"></param>
        /// <param name="setText"></param>
        /// <returns></returns>
        public XWPFParagraph SetCellText(XWPFDocument doc, XWPFTable table, string setText)
        {
            //table中的文字格式设置
            CT_P para = new CT_P();
            XWPFParagraph pCell = new XWPFParagraph(para, table.Body);
            pCell.Alignment = ParagraphAlignment.CENTER;//字体居中
            pCell.VerticalAlignment = TextAlignment.CENTER;//字体居中

            XWPFRun r1c1 = pCell.CreateRun();
            r1c1.SetText(setText);
            r1c1.FontSize = 12;
            r1c1.SetFontFamily("华文楷体", FontCharRange.None);//设置雅黑字体
                                                           //r1c1.SetTextPosition(20);//设置高度

            return pCell;
        }

        /// <summary>
        /// 设置单元格格式
        /// </summary>
        /// <param name="doc">doc对象</param>
        /// <param name="table">表格对象</param>
        /// <param name="setText">要填充的文字</param>
        /// <param name="align">文字对齐方式</param>
        /// <param name="textPos">rows行的高度</param>
        /// <returns></returns>
        public XWPFParagraph SetCellText(XWPFDocument doc, XWPFTable table, string setText, ParagraphAlignment align, int textPos)
        {
            CT_P para = new CT_P();
            XWPFParagraph pCell = new XWPFParagraph(para, table.Body);
            //pCell.Alignment = ParagraphAlignment.LEFT;//字体
            pCell.Alignment = align;

            XWPFRun r1c1 = pCell.CreateRun();
            r1c1.SetText(setText);
            r1c1.FontSize = 12;
            r1c1.SetFontFamily("华文楷体", FontCharRange.None);//设置雅黑字体
            //r1c1.SetTextPosition(textPos);//设置高度

            return pCell;
        }
        /// <summary>
        /// 合并单元格
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="fromCol">起始列</param>
        /// <param name="toCol">结束列</param>
        /// <param name="fromRow">起始行</param>
        /// <param name="toRow">结束行</param>
        /// <returns></returns>
        public static XWPFTableCell MYMergeCells(XWPFTable table, int fromCol, int toCol, int fromRow, int toRow)
        {
            try
            {
                if (fromRow >= table.Rows.Count() || (toRow >= table.Rows.Count())) return null;
                for (int rowIndex = fromRow; rowIndex <= toRow; rowIndex++)
                {
                    if (fromCol < toCol)
                    {
                        table.GetRow(rowIndex).MergeCells(fromCol, toCol);
                    }
                    XWPFTableCell rowcell = table.GetRow(rowIndex).GetCell(fromCol);
                    CT_Tc cttc = rowcell.GetCTTc();
                    if (cttc.tcPr == null)
                    {
                        cttc.AddNewTcPr();
                    }
                    if (rowIndex == fromRow)
                    {
                        // The first merged cell is set with RESTART merge value  
                        rowcell.GetCTTc().tcPr.AddNewVMerge().val = ST_Merge.restart;
                    }
                    else
                    {
                        // Cells which join (merge) the first one, are set with CONTINUE  
                        rowcell.GetCTTc().tcPr.AddNewVMerge().val = ST_Merge.@continue;
                    }
                }
                table.GetRow(fromRow).GetCell(fromCol).SetVerticalAlignment(XWPFTableCell.XWPFVertAlign.CENTER);
                table.GetRow(fromRow).GetCell(fromCol).Paragraphs[0].Alignment = ParagraphAlignment.CENTER;
                return table.GetRow(fromRow).GetCell(fromCol);
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                return null;
            }
        }
    }
}
