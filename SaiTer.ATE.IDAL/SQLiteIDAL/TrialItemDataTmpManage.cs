using SaiTer.ATE.IDAL.DBUtility;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.DataBaseModel;
using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace SaiTer.ATE.IDAL.SQLiteIDAL
{
    /// <summary>
    /// 检测项分项数据临时表管理类
    /// </summary>
    public class TrialItemDataTmpManage
    {
        #region ---------------检测项分项数据------------
        /// <summary>
        /// 保存试验数据(临时表)
        /// </summary>
        /// <param name="lstTrialData">试验数据集合</param>
        /// <returns>返回值(true-成功;false-失败)</returns>
        public static bool SaveTrialData(TrialDataModel trialData)
        {

            //先删除历史数据再插入
            string strDeleteSQL = string.Format("DELETE FROM TrialItemDataTmp WHERE TrialType = {0:D} AND BarCode = '{1:S}' AND TrialName ='{2:S}' AND ChargerID = {3:D} AND ItemName = '{4:S}'",
                                     (int)trialData.TrialType, trialData.BarCode, trialData.TrialName, trialData.ChargerId, trialData.ItemName);
            SQLiteHelper.ExecuteNonQuery(SQLiteHelper.DbConnString, CommandType.Text, strDeleteSQL, null);
            string strSQL = string.Format("INSERT INTO TrialItemDataTmp  VALUES ({0},{1},'{2}',{3},'{4}','{5}',{6},'{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}','{17}')",
             trialData.PKID, trialData.ChargerId, trialData.BarCode, (int)trialData.TrialType, trialData.TrialName, trialData.ItemName, trialData.SchemeID, trialData.SchemeName,
            trialData.Data1, trialData.Data2, trialData.Data3, trialData.TrialResult, trialData.SaveTime, trialData.UserSetParams, trialData.TrialCondition, trialData.RES1, trialData.RES2, trialData.RES3);

            //Log.Log.LogMessage(strSQL);
            if (SQLiteHelper.ExecuteNonQuery(SQLiteHelper.DbConnString, CommandType.Text, strSQL, null) == 0)
                return false;

            return true;
        }

        /// <summary>
        /// 查找指定充电枪测试不通过的试验数据
        /// </summary>
        /// <param name="ChargerID">枪位号</param>
        /// <param name="lstTrialData">试验结果</param>
        /// <returns></returns>
        public static bool SelectFailTrialData(string BarCode, int ChargerID, out List<TrialDataModel> lstTrialData)
        {
            lstTrialData = new List<TrialDataModel>();
            string strSQL = string.Format("SELECT PKID,ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,"
                          + "SchemeName,Data1,Data2,Data3,TrialResult,SaveTime ,UserSetParams,TrialCondition "
                          + "FROM TrialItemDataTmp WHERE BarCode = '{0:S}' And ChargerID ={1:D} And TrialResult = 'Fail'", BarCode, ChargerID);
            DataTable dt = SQLiteHelper.ExecuteDataTable(SQLiteHelper.DbConnString, CommandType.Text, strSQL, "TrialItemDataTmp", null);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    TrialDataModel trialData = new TrialDataModel();
                    trialData.PKID = DBConvert.ToInt64(dr["PKID"]);
                    trialData.BarCode = DBConvert.ToString(dr["BarCode"]);
                    trialData.ChargerId = ChargerID;
                    trialData.TrialType = (EmTrialType)DBConvert.ToInt32(dr["TrialType"]);
                    trialData.UserSetParams = DBConvert.ToString(dr["UserSetParams"]);
                    trialData.TrialCondition = DBConvert.ToString(dr["TrialCondition"]);
                    trialData.TrialName = DBConvert.ToString(dr["TrialName"]);
                    trialData.ItemName = DBConvert.ToString(dr["ItemName"]);
                    trialData.SchemeID = DBConvert.ToInt32(dr["SchemeID"]);
                    trialData.SchemeName = DBConvert.ToString(dr["SchemeName"]);
                    trialData.Data1 = DBConvert.ToString(dr["Data1"]);
                    trialData.Data2 = DBConvert.ToString(dr["Data2"]);
                    trialData.Data3 = DBConvert.ToString(dr["Data3"]);
                    if (dr["TRIALRESULT"].ToString().ToUpper() == "PASS")
                    {
                        trialData.TrialResult = EmTrialResult.Pass;
                    }

                    else if (dr["TRIALRESULT"].ToString().ToUpper() == "FAIL")
                    {
                        trialData.TrialResult = EmTrialResult.Fail;
                    }
                    else if (dr["TRIALRESULT"].ToString().ToUpper() == "NA")
                    {
                        trialData.TrialResult = EmTrialResult.NA;
                    }
                    trialData.ExtentData = DBConvert.ToString(dr["Data2"]);
                    trialData.SaveTime = DBConvert.ToString(dr["SaveTime"]);

                    //添加试验数据
                    lstTrialData.Add(trialData);
                }
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// 查找指定充电枪试验数据
        /// </summary>
        /// <param name="ChargerID">枪位号</param>
        /// <param name="lstTrialData">试验结果</param>
        /// <returns></returns>
        public static bool SelectTrialDataTmp(string BarCode, EmTrialType trialType, string trialName, int ChargerID, out List<TrialDataModel> lstTrialData)
        {
            lstTrialData = new List<TrialDataModel>();
            string strSQL = string.Format("SELECT PKID,ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,"
                          + "SchemeName,Data1,Data2,Data3,TrialResult,SaveTime ,UserSetParams,TrialCondition "
                          + "FROM TrialItemDataTmp WHERE BarCode = '{0:S}' AND TrialType={1:D} And TrialName = '{2:S}' And ChargerID ={3:D}", BarCode, (int)trialType, trialName, ChargerID);
            DataTable dt = SQLiteHelper.ExecuteDataTable(SQLiteHelper.DbConnString, CommandType.Text, strSQL, "TrialItemDataTmp", null);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    TrialDataModel trialData = new TrialDataModel();
                    trialData.PKID = DBConvert.ToInt64(dr["PKID"]);
                    trialData.BarCode = DBConvert.ToString(dr["BarCode"]);
                    trialData.ChargerId = ChargerID;
                    trialData.TrialType = trialType;
                    trialData.UserSetParams = DBConvert.ToString(dr["UserSetParams"]);
                    trialData.TrialCondition = DBConvert.ToString(dr["TrialCondition"]);
                    trialData.TrialName = DBConvert.ToString(dr["TrialName"]);
                    trialData.ItemName = DBConvert.ToString(dr["ItemName"]);
                    trialData.SchemeID = DBConvert.ToInt32(dr["SchemeID"]);
                    trialData.SchemeName = DBConvert.ToString(dr["SchemeName"]);
                    trialData.Data1 = DBConvert.ToString(dr["Data1"]);
                    trialData.Data2 = DBConvert.ToString(dr["Data2"]);
                    trialData.Data3 = DBConvert.ToString(dr["Data3"]);
                    if (dr["TRIALRESULT"].ToString().ToUpper() == "PASS")
                    {
                        trialData.TrialResult = EmTrialResult.Pass;
                    }

                    else if (dr["TRIALRESULT"].ToString().ToUpper() == "FAIL")
                    {
                        trialData.TrialResult = EmTrialResult.Fail;
                    }
                    else if (dr["TRIALRESULT"].ToString().ToUpper() == "NA")
                    {
                        trialData.TrialResult = EmTrialResult.NA;
                    }
                    trialData.ExtentData = DBConvert.ToString(dr["Data2"]);
                    trialData.SaveTime = DBConvert.ToString(dr["SaveTime"]);

                    //添加试验数据
                    lstTrialData.Add(trialData);
                }
                return true;
            }
            else
                return false;
        }
        /// <summary>
        /// 查找指定充电枪试验数据
        /// </summary>
        /// <param name="ChargerID">枪位号</param>
        /// <param name="lstTrialData">试验结果</param>
        /// <returns></returns>
        public static bool SelectTrialData(string BarCode, EmTrialType trialType, string trialName, int ChargerID, out List<TrialDataModel> lstTrialData)
        {
            lstTrialData = new List<TrialDataModel>();
            string strSQL = string.Format("SELECT PKID,ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,"
                          + "SchemeName,Data1,Data2,Data3,TrialResult,SaveTime ,UserSetParams,TrialCondition "
                          + "FROM TrialItemData WHERE BarCode = '{0:S}' AND TrialType={1:D} And TrialName = '{2:S}' And ChargerID ={3:D}", BarCode, (int)trialType, trialName, ChargerID);
            DataTable dt = SQLiteHelper.ExecuteDataTable(SQLiteHelper.DbConnString, CommandType.Text, strSQL, "TrialItemData", null);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    TrialDataModel trialData = new TrialDataModel();
                    trialData.PKID = DBConvert.ToInt64(dr["PKID"]);
                    trialData.BarCode = DBConvert.ToString(dr["BarCode"]);
                    trialData.ChargerId = ChargerID;
                    trialData.TrialType = trialType;
                    trialData.UserSetParams = DBConvert.ToString(dr["UserSetParams"]);
                    trialData.TrialCondition = DBConvert.ToString(dr["TrialCondition"]);
                    trialData.TrialName = DBConvert.ToString(dr["TrialName"]);
                    trialData.ItemName = DBConvert.ToString(dr["ItemName"]);
                    trialData.SchemeID = DBConvert.ToInt32(dr["SchemeID"]);
                    trialData.SchemeName = DBConvert.ToString(dr["SchemeName"]);
                    trialData.Data1 = DBConvert.ToString(dr["Data1"]);
                    trialData.Data1 = DBConvert.ToString(dr["Data1"]);
                    trialData.Data3 = DBConvert.ToString(dr["Data3"]);
                    if (dr["TRIALRESULT"].ToString().ToUpper() == "PASS")
                    {
                        trialData.TrialResult = EmTrialResult.Pass;
                    }

                    else if (dr["TRIALRESULT"].ToString().ToUpper() == "FAIL")
                    {
                        trialData.TrialResult = EmTrialResult.Fail;
                    }
                    else if (dr["TRIALRESULT"].ToString().ToUpper() == "NA")
                    {
                        trialData.TrialResult = EmTrialResult.NA;
                    }
                    trialData.ExtentData = DBConvert.ToString(dr["Data2"]);
                    trialData.SaveTime = DBConvert.ToString(dr["SaveTime"]);

                    //添加试验数据
                    lstTrialData.Add(trialData);
                }
                return true;
            }
            else
                return false;
        }
        /// <summary>
        /// 查找指定充电枪在指定方案中的试验数据
        /// </summary>
        /// <param name="lstTrialData">试验结果</param>
        /// <returns></returns>
        public static bool SelectTrialDataWhereSchemeName(string BarCode, EmTrialType trialType, string trialName, int ChargerID, string schemeName, out List<TrialDataModel> lstTrialData)
        {
            lstTrialData = new List<TrialDataModel>();
            string strSQL = string.Format("SELECT PKID,ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,"
                          + "SchemeName,Data1,Data2,Data3,TrialResult,SaveTime ,UserSetParams,TrialCondition "
                          + "FROM TrialItemDataTmp WHERE BarCode = '{0:S}' AND TrialType={1:D} And TrialName = '{2:S}' And ChargerID ={3:D} And SchemeName ='{4:S}'", BarCode, (int)trialType, trialName, ChargerID, schemeName);
            DataTable dt = SQLiteHelper.ExecuteDataTable(SQLiteHelper.DbConnString, CommandType.Text, strSQL, "TrialItemDataTmp", null);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    TrialDataModel trialData = new TrialDataModel();
                    trialData.PKID = DBConvert.ToInt64(dr["PKID"]);
                    trialData.BarCode = DBConvert.ToString(dr["BarCode"]);
                    trialData.ChargerId = ChargerID;
                    trialData.TrialType = trialType;
                    trialData.UserSetParams = DBConvert.ToString(dr["UserSetParams"]);
                    trialData.TrialCondition = DBConvert.ToString(dr["TrialCondition"]);
                    trialData.TrialName = DBConvert.ToString(dr["TrialName"]);
                    trialData.ItemName = DBConvert.ToString(dr["ItemName"]);
                    trialData.SchemeID = DBConvert.ToInt32(dr["SchemeID"]);
                    trialData.SchemeName = DBConvert.ToString(dr["SchemeName"]);
                    trialData.Data1 = DBConvert.ToString(dr["Data1"]);
                    trialData.Data2 = DBConvert.ToString(dr["Data2"]);
                    trialData.Data3 = DBConvert.ToString(dr["Data3"]);
                    if (dr["TRIALRESULT"].ToString().ToUpper() == "PASS")
                    {
                        trialData.TrialResult = EmTrialResult.Pass;
                    }

                    else if (dr["TRIALRESULT"].ToString().ToUpper() == "FAIL")
                    {
                        trialData.TrialResult = EmTrialResult.Fail;
                    }
                    else if (dr["TRIALRESULT"].ToString().ToUpper() == "NA")
                    {
                        trialData.TrialResult = EmTrialResult.NA;
                    }
                    trialData.ExtentData = DBConvert.ToString(dr["Data2"]);
                    trialData.SaveTime = DBConvert.ToString(dr["SaveTime"]);

                    //添加试验数据
                    lstTrialData.Add(trialData);
                }
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// 查找多个枪的所有试验数据
        /// </summary>
        /// <param name="lstCharger"></param>
        /// <returns></returns>
        public static List<TrialDataModel> GetTrialData_ALL(List<ChargerInfoModel> lstCharger)
        {

            List<TrialDataModel> lstResult = new List<TrialDataModel>();
            string barCodes = "";
            foreach (var item in lstCharger)
            {
                barCodes += "'" + item.BarCode + "',";
            }
            barCodes = barCodes.TrimEnd(',');
            string strSQL = string.Format("SELECT PKID,ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,"
                         + "SchemeName,Data1,Data2,Data3,TrialResult,SaveTime  ,UserSetParams,TrialCondition "
                         + "FROM TrialItemDataTmp WHERE BarCode in ({0:S}) And SchemeName = '{1:S}'", barCodes, lstCharger[0].SchemeName);
            strSQL = strSQL + " order by CAST(Data3 AS int) , CAST(SaveTime As DateTime), CAST(ItemName AS int) ";
            DataTable dt = SQLiteHelper.ExecuteDataTable(SQLiteHelper.DbConnString, CommandType.Text, strSQL, "TrialItemDataTmp", null);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    TrialDataModel trialData = new TrialDataModel();
                    trialData.PKID = DBConvert.ToInt64(dr["PKID"]);
                    trialData.BarCode = DBConvert.ToString(dr["BarCode"]);
                    trialData.ChargerId = DBConvert.ToInt32(dr["ChargerID"]);
                    trialData.TrialType = (EmTrialType)DBConvert.ToInt32(dr["TrialType"]); ;
                    trialData.TrialName = DBConvert.ToString(dr["TrialName"]);
                    trialData.ItemName = DBConvert.ToString(dr["ItemName"]);
                    trialData.UserSetParams = DBConvert.ToString(dr["UserSetParams"]);
                    trialData.TrialCondition = DBConvert.ToString(dr["TrialCondition"]);
                    trialData.SchemeID = DBConvert.ToInt32(dr["SchemeID"]);
                    trialData.SchemeName = DBConvert.ToString(dr["SchemeName"]);
                    trialData.Data1 = DBConvert.ToString(dr["Data1"]);
                    trialData.Data2 = DBConvert.ToString(dr["Data2"]);
                    trialData.Data3 = DBConvert.ToString(dr["Data3"]);

                    if (dr["TRIALRESULT"].ToString().ToUpper() == "PASS")
                    {
                        trialData.TrialResult = EmTrialResult.Pass;
                    }

                    else if (dr["TRIALRESULT"].ToString().ToUpper() == "FAIL")
                    {
                        trialData.TrialResult = EmTrialResult.Fail;
                    }
                    else if (dr["TRIALRESULT"].ToString().ToUpper() == "NA")
                    {
                        trialData.TrialResult = EmTrialResult.NA;
                    }
                    trialData.ExtentData = DBConvert.ToString(dr["Data2"]);
                    trialData.SaveTime = DBConvert.ToString(dr["SaveTime"]);

                    //添加试验数据
                    lstResult.Add(trialData);
                }
                return lstResult;
            }
            return lstResult;
        }


        /// <summary>
        /// 按PKID和方案名称查找多个枪、在多个方案下的所有数据
        /// </summary>
        /// <param name="lstPKID">多个PKID组合的数据库字符串</param>
        /// <param name="lstSchemeName">多个SchemeName组合的数据库字符串</param>
        /// <returns></returns>
        public static List<TrialDataModel> GetTrialDataFromPkidAndSchemeName(string lstPKID, string lstSchemeName)
        {

            List<TrialDataModel> lstResult = new List<TrialDataModel>();

            string strSQL = string.Format("SELECT PKID,ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,"
                         + "SchemeName,Data1,Data2,Data3,TrialResult,SaveTime  ,UserSetParams,TrialCondition "
                         + "FROM TrialItemData WHERE PKID in ({0:S}) And SchemeName in ({1:S})", lstPKID, lstSchemeName);
            strSQL = strSQL + " order by CAST(Data3 AS int), CAST(ItemName AS int)";
            DataTable dt = SQLiteHelper.ExecuteDataTable(SQLiteHelper.DbConnString, CommandType.Text, strSQL, "TrialItemData", null);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    TrialDataModel trialData = new TrialDataModel();
                    trialData.PKID = DBConvert.ToInt64(dr["PKID"]);
                    trialData.BarCode = DBConvert.ToString(dr["BarCode"]);
                    trialData.ChargerId = DBConvert.ToInt32(dr["ChargerID"]);
                    trialData.TrialType = (EmTrialType)DBConvert.ToInt32(dr["TrialType"]); ;
                    trialData.TrialName = DBConvert.ToString(dr["TrialName"]);
                    trialData.ItemName = DBConvert.ToString(dr["ItemName"]);
                    trialData.UserSetParams = DBConvert.ToString(dr["UserSetParams"]);
                    trialData.TrialCondition = DBConvert.ToString(dr["TrialCondition"]);
                    trialData.SchemeID = DBConvert.ToInt32(dr["SchemeID"]);
                    trialData.SchemeName = DBConvert.ToString(dr["SchemeName"]);
                    trialData.Data1 = DBConvert.ToString(dr["Data1"]);
                    trialData.Data2 = DBConvert.ToString(dr["Data2"]);
                    trialData.Data3 = DBConvert.ToString(dr["Data3"]);

                    if (dr["TRIALRESULT"].ToString().ToUpper() == "PASS")
                    {
                        trialData.TrialResult = EmTrialResult.Pass;
                    }

                    else if (dr["TRIALRESULT"].ToString().ToUpper() == "FAIL")
                    {
                        trialData.TrialResult = EmTrialResult.Fail;
                    }
                    else if (dr["TRIALRESULT"].ToString().ToUpper() == "NA")
                    {
                        trialData.TrialResult = EmTrialResult.NA;
                    }
                    trialData.ExtentData = DBConvert.ToString(dr["Data2"]);
                    trialData.SaveTime = DBConvert.ToString(dr["SaveTime"]);

                    //添加试验数据
                    lstResult.Add(trialData);
                }
                return lstResult;
            }
            return lstResult;
        }
        
        /// <summary>
        /// 按照唯一编号查找所有的历史数据
        /// </summary>
        /// <param name="PKID"></param>
        /// <param name="lstTrialData"></param>
        /// <returns></returns>
        public static List<TrialDataModel> SelectTrialData(string strPKID)
        {
            List<TrialDataModel> lstTrialData = new List<TrialDataModel>();
            string strSQL = string.Format("SELECT PKID,ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,"
                          + "SchemeName,Data1,Data2,Data3,TrialResult,SaveTime ,UserSetParams,TrialCondition "
                          + "FROM TrialItemData WHERE PKID = {0:S} ", strPKID);
            DataTable dt = SQLiteHelper.ExecuteDataTable(SQLiteHelper.DbConnString, CommandType.Text, strSQL, "TrialItemData", null);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    TrialDataModel trialData = new TrialDataModel();
                    trialData.PKID = DBConvert.ToInt64(dr["PKID"]);
                    trialData.BarCode = DBConvert.ToString(dr["BarCode"]);
                    trialData.ChargerId = DBConvert.ToInt32(dr["ChargerId"]);
                    trialData.TrialType = (EmTrialType)DBConvert.ToInt32(dr["TrialType"]);
                    trialData.UserSetParams = DBConvert.ToString(dr["UserSetParams"]);
                    trialData.TrialCondition = DBConvert.ToString(dr["TrialCondition"]);
                    trialData.TrialName = DBConvert.ToString(dr["TrialName"]);
                    trialData.ItemName = DBConvert.ToString(dr["ItemName"]);
                    trialData.SchemeID = DBConvert.ToInt32(dr["SchemeID"]);
                    trialData.SchemeName = DBConvert.ToString(dr["SchemeName"]);
                    trialData.Data1 = DBConvert.ToString(dr["Data1"]);
                    trialData.Data2 = DBConvert.ToString(dr["Data2"]);
                    trialData.Data3 = DBConvert.ToString(dr["Data3"]);
                    if (dr["TRIALRESULT"].ToString().ToUpper() == "PASS")
                    {
                        trialData.TrialResult = EmTrialResult.Pass;
                    }
                    else if (dr["TRIALRESULT"].ToString().ToUpper() == "FAIL")
                    {
                        trialData.TrialResult = EmTrialResult.Fail;
                    }
                    else if (dr["TRIALRESULT"].ToString().ToUpper() == "NA")
                    {
                        trialData.TrialResult = EmTrialResult.NA;
                    }
                    trialData.ExtentData = DBConvert.ToString(dr["Data2"]);
                    trialData.SaveTime = DBConvert.ToString(dr["SaveTime"]);

                    //添加试验数据
                    lstTrialData.Add(trialData);
                }
                return lstTrialData;
            }
            else
                return lstTrialData;
        }


        /// <summary>
        /// 按条件查找试验数据
        /// </summary>
        /// <param name="strConditionSQL">条件语句</param>
        /// <returns></returns>
        public static DataTable SelectTrialDataWhereSQL(string strConditionSQL)
        {

            // string strSQL = string.Format("SELECT PKID,ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,"
            //+ "SchemeName,Data1,Data2,Data3,TrialResult,SaveTime "
            //+ "FROM TrialItemDataTmp  " + strConditionSQL);
            //string strSQL = string.Format("SELECT PKID,BarCode,"
            //           + "SchemeName,TrialResult,SaveTime "
            //           + "FROM TrialItemDataTmp  " + strConditionSQL);
            string strSQL = "select PKID, BarCode, SchemeName, TrialResult, SaveTime, RES1, RES2, RES3 from TrialItemData where trialresult = 'Fail' " + strConditionSQL +
                " GROUP BY PKID";
            DataTable dt1 = SQLiteHelper.ExecuteDataTable(SQLiteHelper.DbConnString, CommandType.Text, strSQL, "TrialItemData", null);

            strSQL = " select PKID, BarCode, SchemeName, TrialResult, SaveTime, RES1, RES2, RES3 from TrialItemData where trialresult = 'Pass' and PKID not in " +
                "(select PKID from TrialItemData where trialresult = 'Fail' GROUP BY PKID)  " + strConditionSQL +
                " GROUP BY PKID";
            DataTable dt2 = SQLiteHelper.ExecuteDataTable(SQLiteHelper.DbConnString, CommandType.Text, strSQL, "TrialItemData", null);
            DataTable dt = dt1.Copy();
            foreach (DataRow item in dt2.Rows)
            {
                DataRow dr = dt.NewRow();
                dr.ItemArray = item.ItemArray;
                dt.Rows.Add(dr);
            }
            return dt;
        }

        #endregion

        /// <summary>
        /// 临时库检测项分项详细数据入正式库
        /// </summary>
        /// <returns></returns>
        public static bool InsertTrialItemDataToFormalTable(List<long> lstPKID, string schemeName)
        {
            string PKIDs = "";
            foreach (var item in lstPKID)
            {
                PKIDs += item + ",";
            }
            PKIDs = PKIDs.TrimEnd(',');
            string strSQL = string.Format("SELECT PKID,ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,"
                         + "SchemeName,Data1,Data2,Data3,TrialResult,SaveTime, UserSetParams,TrialCondition,RES1,RES2,RES3  "
                         + "FROM TrialItemDataTmp WHERE PKID in ({0:S}) And SchemeName = '{1:S}'", PKIDs, schemeName);
            strSQL = strSQL + " order by CAST(Data3 AS int) , CAST(SaveTime As DateTime), CAST(ItemName AS int) ";
            DataTable dt = SQLiteHelper.ExecuteDataTable(SQLiteHelper.DbConnString, CommandType.Text, strSQL, "TrialItemDataTmp", null);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {

                    string strParams = "";
                    foreach (object item in dr.ItemArray)
                    {
                        strParams += "'" + item.ToString() + "',";
                    }
                    strParams = strParams.TrimEnd(',');
                    string sql = string.Format("Insert Into TrialItemData  values ({0})", strParams);
                    if (SQLiteHelper.ExecuteNonQuery(SQLiteHelper.DbConnString, CommandType.Text, sql, null) == 0)
                        return false;

                }
            }
            return true;
        }


        public static void DeleteTrialData(string PKID )
        {
            string strSQL = string.Format("Delete from TrialItemDataTmp Where PKID ={0}", PKID);          
            SQLiteHelper.ExecuteNonQuery(SQLiteHelper.DbConnString, CommandType.Text, strSQL, null);
        }

        /// <summary>
        /// 删除试验数据(临时表)
        /// </summary>
        /// <param name="trialData">试验数据</param>
        /// <returns>返回值(true-成功;false-失败)</returns>
        public static bool DeleteTrialData(TrialDataModel trialData)
        {
            string strDeleteSQL = string.Format("DELETE FROM TrialItemDataTmp WHERE TrialType = {0:D} AND BarCode = '{1:S}' AND TrialName ='{2:S}' AND ChargerID = {3:D}",
                                     (int)trialData.TrialType, trialData.BarCode, trialData.TrialName, trialData.ChargerId);
            if (SQLiteHelper.ExecuteNonQuery(SQLiteHelper.DbConnString, CommandType.Text, strDeleteSQL, null) == 0)
                return false;

            return true;
        }

        #region 历史数据表

        /// <summary>
        /// 保存试验数据(历史数据表：此表无临时表和正式表)
        /// </summary>
        /// <param name="lstTrialData">试验数据集合</param>
        /// <returns>返回值(true-成功;false-失败)</returns>
        public static bool SaveTrialData_History(TrialDataModel trialData)
        {
            //直接插入数据，做多少次测试项目就有多少条数据，同一个测试项目可以有多条数据
            string strSQL = string.Format("INSERT INTO TrialItemData_History  VALUES ({0},{1},'{2}',{3},'{4}','{5}',{6},'{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}','{17}')",
             trialData.PKID, trialData.ChargerId, trialData.BarCode, (int)trialData.TrialType, trialData.TrialName, trialData.ItemName, trialData.SchemeID, trialData.SchemeName,
            trialData.Data1, trialData.Data2, trialData.Data3, trialData.TrialResult, trialData.SaveTime, trialData.UserSetParams, trialData.TrialCondition, trialData.RES1, trialData.RES2, trialData.RES3);

            if (SQLiteHelper.ExecuteNonQuery(SQLiteHelper.DbConnString, CommandType.Text, strSQL, null) == 0)
                return false;

            return true;
        }

        /// <summary>
        /// 删除试验数据(历史数据表)
        /// </summary>
        /// <param name="PKID">唯一编号</param>
        /// <returns>返回值(true-成功;false-失败)</returns>
        public static void DeleteTrialData_History(long PKID)
        {
            string strSQL = string.Format("Delete from TrialItemData_History Where PKID ={0}", PKID);
            SQLiteHelper.ExecuteNonQuery(SQLiteHelper.DbConnString, CommandType.Text, strSQL, null);
        }

        /// <summary>
        /// 按照唯一编号查找所有的历史数据
        /// </summary>
        /// <param name="PKID"></param>
        /// <param name="lstTrialData"></param>
        /// <returns></returns>
        public static bool SelectTrialData_History(long PKID, out List<TrialDataModel> lstTrialData)
        {
            lstTrialData = new List<TrialDataModel>();
            string strSQL = string.Format("SELECT PKID,ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,"
                          + "SchemeName,Data1,Data2,Data3,TrialResult,SaveTime ,UserSetParams,TrialCondition "
                          + "FROM TrialItemData_History WHERE PKID = {0:S} ", PKID);
            DataTable dt = SQLiteHelper.ExecuteDataTable(SQLiteHelper.DbConnString, CommandType.Text, strSQL, "TrialItemData_History", null);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    TrialDataModel trialData = new TrialDataModel();
                    trialData.PKID = DBConvert.ToInt64(dr["PKID"]);
                    trialData.BarCode = DBConvert.ToString(dr["BarCode"]);
                    trialData.ChargerId = DBConvert.ToInt32(dr["ChargerId"]);
                    trialData.TrialType = (EmTrialType)DBConvert.ToInt32(dr["TrialType"]);
                    trialData.UserSetParams = DBConvert.ToString(dr["UserSetParams"]);
                    trialData.TrialCondition = DBConvert.ToString(dr["TrialCondition"]);
                    trialData.TrialName = DBConvert.ToString(dr["TrialName"]);
                    trialData.ItemName = DBConvert.ToString(dr["ItemName"]);
                    trialData.SchemeID = DBConvert.ToInt32(dr["SchemeID"]);
                    trialData.SchemeName = DBConvert.ToString(dr["SchemeName"]);
                    trialData.Data1 = DBConvert.ToString(dr["Data1"]);
                    trialData.Data2 = DBConvert.ToString(dr["Data2"]);
                    trialData.Data3 = DBConvert.ToString(dr["Data3"]);
                    if (dr["TRIALRESULT"].ToString().ToUpper() == "PASS")
                    {
                        trialData.TrialResult = EmTrialResult.Pass;
                    }
                    else if (dr["TRIALRESULT"].ToString().ToUpper() == "FAIL")
                    {
                        trialData.TrialResult = EmTrialResult.Fail;
                    }
                    else if (dr["TRIALRESULT"].ToString().ToUpper() == "NA")
                    {
                        trialData.TrialResult = EmTrialResult.NA;
                    }
                    trialData.ExtentData = DBConvert.ToString(dr["Data2"]);
                    trialData.SaveTime = DBConvert.ToString(dr["SaveTime"]);

                    //添加试验数据
                    lstTrialData.Add(trialData);
                }
                return true;
            }
            else
                return false;
        }


        /// <summary>
        /// 按照唯一编号和测试项目编号查找所有的历史数据
        /// </summary>
        /// <param name="ChargerID">枪位号</param>
        /// <param name="lstTrialData">试验结果</param>
        /// <returns></returns>
        public static bool SelectTrialData_History(long PKID, EmTrialType trialType, string trialName, out List<TrialDataModel> lstTrialData)
        {
            lstTrialData = new List<TrialDataModel>();
            string strSQL = string.Format("SELECT PKID,ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,"
                          + "SchemeName,Data1,Data2,Data3,TrialResult,SaveTime ,UserSetParams,TrialCondition "
                          + "FROM TrialItemData_History WHERE PKID = {0:S} AND TrialType={1:D} And TrialName = '{2:S}' ", PKID, (int)trialType, trialName);
            DataTable dt = SQLiteHelper.ExecuteDataTable(SQLiteHelper.DbConnString, CommandType.Text, strSQL, "TrialItemData_History", null);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    TrialDataModel trialData = new TrialDataModel();
                    trialData.PKID = DBConvert.ToInt64(dr["PKID"]);
                    trialData.BarCode = DBConvert.ToString(dr["BarCode"]);
                    trialData.ChargerId = DBConvert.ToInt32(dr["ChargerId"]);
                    trialData.TrialType = trialType;
                    trialData.UserSetParams = DBConvert.ToString(dr["UserSetParams"]);
                    trialData.TrialCondition = DBConvert.ToString(dr["TrialCondition"]);
                    trialData.TrialName = DBConvert.ToString(dr["TrialName"]);
                    trialData.ItemName = DBConvert.ToString(dr["ItemName"]);
                    trialData.SchemeID = DBConvert.ToInt32(dr["SchemeID"]);
                    trialData.SchemeName = DBConvert.ToString(dr["SchemeName"]);
                    trialData.Data1 = DBConvert.ToString(dr["Data1"]);
                    trialData.Data2 = DBConvert.ToString(dr["Data2"]);
                    trialData.Data3 = DBConvert.ToString(dr["Data3"]);
                    if (dr["TRIALRESULT"].ToString().ToUpper() == "PASS")
                    {
                        trialData.TrialResult = EmTrialResult.Pass;
                    }

                    else if (dr["TRIALRESULT"].ToString().ToUpper() == "FAIL")
                    {
                        trialData.TrialResult = EmTrialResult.Fail;
                    }
                    else if (dr["TRIALRESULT"].ToString().ToUpper() == "NA")
                    {
                        trialData.TrialResult = EmTrialResult.NA;
                    }
                    trialData.ExtentData = DBConvert.ToString(dr["Data2"]);
                    trialData.SaveTime = DBConvert.ToString(dr["SaveTime"]);

                    //添加试验数据
                    lstTrialData.Add(trialData);
                }
                return true;
            }
            else
                return false;
        }

        #endregion

    }
}
