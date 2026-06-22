using SaiTer.ATE.IDAL.DBUtility;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.DataBaseModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SaiTer.ATE.DataModel.EnumModel;
using System.Data.SQLite;
using System.Data.OleDb;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity;
using Newtonsoft.Json.Linq;

namespace SaiTer.ATE.IDAL.SQLiteIDAL
{
    /// <summary>
    /// 检测结果数据库临时表管理类
    /// </summary>
    public class TrialItemResultTmpManage
    {
        #region ---------------检测数据------------
        /// <summary>
        /// 保存试验项总结论数据(临时表)
        /// </summary>
        /// <param name="lstTrialData">试验数据集合</param>
        /// <returns>返回值(true-成功;false-失败)</returns>
        public static bool SaveTrialFinalData(List<TrialDataModel> lstTrialData)
        {
            if (lstTrialData == null || lstTrialData.Count == 0)
                return false;
            foreach (TrialDataModel trialData in lstTrialData)
            {
                //先删除历史数据再插入
                string strDeleteSQL = string.Format("DELETE FROM TrialItemResultTmp WHERE TrialType = {0:D} AND BarCode = '{1:S}' AND TrialName ='{2:S}' AND ChargerID = {3:D}",
                                         (int)trialData.TrialType, trialData.BarCode, trialData.TrialName, trialData.ChargerId);
                SQLiteHelper.ExecuteNonQuery(SQLiteHelper.DbConnString, CommandType.Text, strDeleteSQL, null);
                string strSQL = string.Format("INSERT INTO TrialItemResultTmp  VALUES ({9},{0:D},'{1:S}','{2:S}',{3:D},'{4:S}','{5}','{6:S}',{7:D} ,'{8:S}')",
                   trialData.SchemeID, trialData.SchemeName, trialData.BarCode, (int)trialData.TrialType, trialData.TrialName, trialData.TrialFinalResult, trialData.Remarks, trialData.ChargerId, trialData.TrialCondition, trialData.PKID);

                if (SQLiteHelper.ExecuteNonQuery(SQLiteHelper.DbConnString, CommandType.Text, strSQL, null) == 0)
                    return false;
            }
            return true;
        }
        /// <summary>
        /// 根据条码获取某试验项的总结论
        /// </summary>
        /// <param name="barcode">条码</param>
        /// <param name="trialType">需要查找的试验项</param>
        /// <param name="resutl">总结论</param>
        /// <param name="TrialCondition">试验条件</param>
        /// <returns></returns>
        public static bool GetTrialFinalResultFromBarcode(string barcode, EmTrialType trialType, ref string resutl, ref string TrialCondition)
        {
            string strSql = string.Format("Select TrialResult , TrialCondition from TrialItemResult where BarCode = '{0:S}' and TrialType = {1:D}", barcode, (int)trialType);
            DataTable dt = SQLiteHelper.ExecuteDataTable(SQLiteHelper.DbConnString, CommandType.Text, strSql, "TrialItemResult", null);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    TrialDataModel trialData = new TrialDataModel();
                    resutl = dr["TrialResult"].ToString();
                    TrialCondition = dr["TrialCondition"].ToString();
                    if (dr["TrialResult"].ToString().ToUpper() == "PASS")
                    {
                        trialData.TrialResult = EmTrialResult.Pass;
                    }
                    else if (dr["TrialResult"].ToString().ToUpper() == "NA")
                    {
                        trialData.TrialResult = EmTrialResult.NA;
                    }
                    else
                    {
                        trialData.TrialResult = EmTrialResult.Fail;
                    }
                }
                return true;
            }
            else
                return false;
        }
        public static string GetTrialFinalResultFromData(string barCode, EmTrialType trialType, string trialName, int chargerId)
        {
            bool isOK = TrialItemDataTmpManage.SelectTrialData(barCode, trialType, trialName, chargerId, out var lstDataTmp);
            if (isOK && lstDataTmp.Count > 0)
            {
                int index = lstDataTmp.FindIndex(s => s.TrialResult == EmTrialResult.Fail);
                if (index >= 0)
                {
                    return "Fail";
                }
                else
                {
                    return "PASS";
                }
            }
            else
            {
                return "Fail";
            }
        }
        /// <summary>
        /// 根据条码+TrialType+TrialName 查询TrialItemData测试明细
        /// </summary>
        public static List<TrialDataModel> GetTrialItemDataByBarCode(string barCode, string trialType, string trialName)
        {
            List<TrialDataModel> list = new List<TrialDataModel>();
            string sql = $@"SELECT ItemName, TrialResult, Data2 
                        FROM TrialItemData 
                        WHERE BarCode='{barCode}' 
                        AND TrialType='{trialType}' 
                        AND TrialName='{trialName}'";

            DataTable dt = SQLiteHelper.ExecuteDataTable(SQLiteHelper.DbConnString, CommandType.Text, sql, "TrialItemData", null);
            if (dt == null || dt.Rows.Count == 0)
                return list;

            foreach (DataRow dr in dt.Rows)
            {
                string trialRes = DBConvert.ToString(dr["TrialResult"]).ToUpper();
                EmTrialResult res = trialRes != "Fail" ? EmTrialResult.Pass : EmTrialResult.Fail;
                list.Add(new TrialDataModel
                {
                    ItemName = DBConvert.ToString(dr["ItemName"]),
                    TrialResult = res,
                    Data2 = DBConvert.ToString(dr["Data2"])
                });
            }
            return list;
        }



        public static List<TrialDataModel> GetTrialResultFromPKID(string PKID, string tableName = null, string SchemeName = "")
        {
            List<TrialDataModel> lstData = new List<TrialDataModel>();
            try
            {
                string strSql = "";
                DataTable dt;
                if (tableName == null)
                {
                    strSql = string.Format("Select TrialType,TrialName,ChargerID,TrialResult from {0} Where PKID = '{1}' And SchemeName = '{2:S}'", "TrialItemResult", PKID, SchemeName);
                    dt = SQLiteHelper.ExecuteDataTable(SQLiteHelper.DbConnString, CommandType.Text, strSql, "TrialItemResult", null);
                }
                else
                {
                    strSql = string.Format("Select TrialType,TrialName,ChargerID,TrialResult from {0} Where PKID = '{1}' And SchemeName = '{2:S}'", "TrialItemResultTmp", PKID, SchemeName);
                    dt = SQLiteHelper.ExecuteDataTable(SQLiteHelper.DbConnString, CommandType.Text, strSql, "TrialItemResultTmp", null);
                }

                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        TrialDataModel trialData = new TrialDataModel();
                        if (dr["TrialResult"].ToString().ToUpper() == "PASS")
                        {
                            trialData.TrialFinalResult = EmTrialResult.Pass;
                        }
                        else if (dr["TrialResult"].ToString().ToUpper() == "NA")
                        {
                            trialData.TrialFinalResult = EmTrialResult.NA;
                        }
                        else
                        {
                            trialData.TrialFinalResult = EmTrialResult.Fail;
                        }
                        trialData.TrialType = (EmTrialType)Convert.ToInt32(dr["TrialType"]);
                        trialData.TrialName = dr["TrialName"].ToString();
                        lstData.Add(trialData);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
            return lstData;
        }

        public static List<TrialDataModel> GetTrialDataFromPKIDAndTrialType(string PKID, int TrialType)
        {
            List<TrialDataModel> lstData = new List<TrialDataModel>();
            try
            {
                string strSql = string.Format("Select Data2 ,TrialResult from TrialItemData Where PKID = '{0}' And TrialType = '{1}'", PKID, TrialType);
                DataTable dt = SQLiteHelper.ExecuteDataTable(SQLiteHelper.DbConnString, CommandType.Text, strSql, "TrialItemData", null);
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        TrialDataModel trialData = new TrialDataModel();
                        trialData.Data2 = dr["Data2"].ToString();
                        if (dr["TrialResult"].ToString().ToUpper() == "PASS")
                        {
                            trialData.TrialResult = EmTrialResult.Pass;
                        }
                        else if (dr["TrialResult"].ToString().ToUpper() == "NA")
                        {
                            trialData.TrialResult = EmTrialResult.NA;
                        }
                        else
                        {
                            trialData.TrialResult = EmTrialResult.Fail;
                        }
                        lstData.Add(trialData);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
            return lstData;
        }
        /// <summary>
        /// 获取历史数据记录
        /// </summary>
        /// <param name="PKID"></param>
        /// <param name="TrialType"></param>
        /// <returns></returns>
        public static List<TrialDataModel> GetTrialDataFromPKIDAndTrialType_History(string PKID, int TrialType)
        {
            List<TrialDataModel> lstData = new List<TrialDataModel>();
            try
            {
                string strSql = string.Format("Select Data2 ,TrialResult from TrialItemData_History Where PKID = '{0}' And TrialType = '{1}'", PKID, TrialType);
                DataTable dt = SQLiteHelper.ExecuteDataTable(SQLiteHelper.DbConnString, CommandType.Text, strSql, "TrialItemData_History", null);
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        TrialDataModel trialData = new TrialDataModel();
                        trialData.Data2 = dr["Data2"].ToString();
                        if (dr["TrialResult"].ToString().ToUpper() == "PASS")
                        {
                            trialData.TrialResult = EmTrialResult.Pass;
                        }

                        else if (dr["TrialResult"].ToString().ToUpper() == "FAIL")
                        {
                            trialData.TrialResult = EmTrialResult.Fail;
                        }
                        else if (dr["TRIALRESULT"].ToString().ToUpper() == "NA")
                        {
                            trialData.TrialResult = EmTrialResult.NA;
                        }
                        lstData.Add(trialData);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
            return lstData;
        }
        #endregion
        /// <summary>
        /// 临时库检测项总结论数据入正式库
        /// </summary>
        /// <returns></returns>
        public static bool InsertTrialItemResultToFormalTable(List<long> lstPKID, string schemeName)
        {
            string PKIDs = "";
            foreach (var item in lstPKID)
            {
                PKIDs += item + ",";
            }
            PKIDs = PKIDs.TrimEnd(',');
            string strSql = string.Format("Select * from TrialItemResultTmp where PKID In ({0:S}) And SchemeName = '{1:S}'", PKIDs, schemeName);
            DataTable dt = SQLiteHelper.ExecuteDataTable(SQLiteHelper.DbConnString, CommandType.Text, strSql, "TrialItemResultTmp", null);
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
                    string sql = string.Format("Insert Into TrialItemResult  values ({0})", strParams);
                    if (SQLiteHelper.ExecuteNonQuery(SQLiteHelper.DbConnString, CommandType.Text, sql, null) == 0)
                        return false;

                }
            }
            return true;
        }



        /// <summary>
        /// 保存信息到数据库正式表
        /// </summary>
        /// <returns>返回值(true-成功；false-失败)</returns>
        public static bool SaveTrialData()
        {
            SQLiteTransaction tran = null;
            try
            {
                SQLiteConnection conn;
                using (conn = new SQLiteConnection(SQLiteHelper.DbConnString))
                {
                    conn.Open();
                    tran = conn.BeginTransaction();
                    List<long> lstPKID = new List<long>();
                    string schemeName = "";
                    bool isGroupTwoCharger = false; //群充双枪需要特殊处理

                    #region  ===========枪信息临时表转入正式表=================


                    string strSQL = string.Format("SELECT PKID, ProductName,ProductModel,ProductBarcode,ChargerID,Operater,Auditor,ChargerType , " +
                        "SchemeName,CreateTime ,NominalVoltage,NominalCurrent,Freq ,MinAllowChargeVoltage,MaxOutputPower,MaxAllowChargeCurrent ," +
                        "TrialResult,RES1,RES2,RES3 FROM ChargerInfoTmp ");
                    SQLiteCommand command = new SQLiteCommand(strSQL, conn, tran);
                    DataTable dt = new DataTable();
                    SQLiteDataAdapter adp = new SQLiteDataAdapter();
                    command.Connection = conn;
                    adp.SelectCommand = command;
                    DataSet ds = new DataSet();
                    adp.Fill(ds, "ChargerInfoTmp");

                    command.Parameters.Clear();
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        dt = ds.Tables[0];
                    }
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DataRow dr = dt.Rows[i];
                            //如果是群充双枪的桩，需要合并AB的测试结果
                            isGroupTwoCharger = Convert.ToString(dr.ItemArray[18]) == "1";
                            schemeName = DBConvert.ToString(dr["SchemeName"]);
                            lstPKID.Add(DBConvert.ToInt64(dr["PKID"]));
                            //双枪信息合并
                            if (i % 2 == 1 && isGroupTwoCharger)
                                continue;
                            string strParams = "";
                            if (isGroupTwoCharger)
                            {
                                for (int j = 0; j < dr.ItemArray.Length; j++)
                                {
                                    //条码去掉AB
                                    if (j == 3)
                                        strParams += "'" + dr.ItemArray[j].ToString().Substring(0, dr.ItemArray[j].ToString().Length - 1) + "',";
                                    //ChargerId需要减半
                                    else if (j == 4)
                                        strParams += "'" + (Math.Ceiling(Convert.ToInt32(dr.ItemArray[j]) / 2.0)).ToString("F0") + "',";
                                    //AB枪结果合并
                                    else if (j == 16)
                                    {
                                        if (dr.ItemArray[16].ToString().ToUpper() == "PASS" && dt.Rows[i + 1].ItemArray[16].ToString().ToUpper() == "PASS")
                                            strParams += "'PASS',";
                                        else
                                            strParams += "'Fail',";

                                    }
                                    else
                                        strParams += "'" + dr.ItemArray[j].ToString() + "',";
                                }
                            }
                            else
                            {
                                foreach (object item in dr.ItemArray)
                                {
                                    strParams += "'" + item.ToString() + "',";
                                }
                            }
                            strParams = strParams.TrimEnd(',');
                            string sql = string.Format("Insert Into ChargerInfo(PKID, ProductName,ProductModel,ProductBarcode,ChargerID," +
                                "Operater,Auditor,ChargerType , SchemeName,CreateTime ,NominalVoltage,NominalCurrent,Freq," +
                                "MinAllowChargeVoltage,MaxOutputPower,MaxAllowChargeCurrent ,TrialResult,RES1,RES2,RES3)  values ({0})", strParams);

                            command.CommandText = sql;
                            int value = command.ExecuteNonQuery();
                            if (value != 1)
                            {
                                tran.Rollback();
                                return false;
                            }
                        }
                    }
                    #endregion

                    #region  ===========检测项分项详细数据临时表转入正式表=================
                    string PKIDs = "";
                    foreach (var item in lstPKID)
                    {
                        PKIDs += item + ",";
                    }
                    PKIDs = PKIDs.TrimEnd(',');
                    strSQL = string.Format("SELECT PKID,ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,"
                                 + "SchemeName,Data1,Data2,Data3,TrialResult,SaveTime ,UserSetParams,TrialCondition,RES1,RES2,RES3  "
                                 + "FROM TrialItemDataTmp WHERE PKID in ({0:S}) And SchemeName = '{1:S}'", PKIDs, schemeName);
                    strSQL += " order by CAST(Data3 AS int) , SaveTime ";
                    command = new SQLiteCommand(strSQL, conn, tran);
                    adp = new SQLiteDataAdapter
                    {
                        SelectCommand = command
                    };
                    ds = new DataSet();
                    adp.Fill(ds, "TrialItemDataTmp");
                    command.Parameters.Clear();
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        dt = ds.Tables[0];
                    }
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DataRow dr = dt.Rows[i];
                            string strParams = "";
                            //如果是群充双枪的桩，需要合并AB的测试结果
                            if (isGroupTwoCharger)
                            {
                                string AorB = "";
                                for (int j = 0; j < dr.ItemArray.Length; j++)
                                {
                                    //PKID因为合并所以双数的没有了
                                    if (j == 0)
                                        strParams += Convert.ToInt64(dr.ItemArray[j]) % 2 == 0 ? $"'{Convert.ToInt64(dr.ItemArray[j]) - 1}'," : "'" + dr.ItemArray[j].ToString() + "',";
                                    //ChargerId需要减半
                                    else if (j == 1)
                                        strParams += "'" + (Math.Ceiling(Convert.ToInt32(dr.ItemArray[j]) / 2.0)).ToString("F0") + "',";
                                    //条码去掉AB
                                    else if (j == 2)
                                    {
                                        strParams += "'" + dr.ItemArray[j].ToString().Substring(0, dr.ItemArray[j].ToString().Length - 1) + "',";
                                        AorB = "(" + dr.ItemArray[j].ToString().Substring(dr.ItemArray[j].ToString().Length - 1, 1) + "枪)";
                                    }
                                    //Data2加上AB标识，如一般检查|外观检查|-|-|-|报表(勿删) => 一般检查(A枪)|外观检查|-|-|-|报表(勿删)
                                    else if (j == 9)
                                    {
                                        strParams += "'" + dr.ItemArray[j].ToString().Insert(dr.ItemArray[j].ToString().IndexOf('|'), AorB) + "',";
                                    }
                                    else
                                        strParams += "'" + dr.ItemArray[j].ToString() + "',";
                                }
                            }
                            else
                            {
                                foreach (object item in dr.ItemArray)
                                {
                                    strParams += "'" + item.ToString() + "',";
                                }
                            }
                            strParams = strParams.TrimEnd(',');
                            string sql = string.Format("Insert Into TrialItemData  values ({0})", strParams);
                            command.CommandText = sql;
                            int value = command.ExecuteNonQuery();
                            if (value != 1)
                            {
                                tran.Rollback();
                                return false;
                            }
                        }
                    }
                    #endregion

                    #region  ===========检测项总结论数据临时表转入正式表=================

                    string strSql = string.Format("Select * from TrialItemResultTmp where PKID In ({0:S}) And SchemeName = '{1:S}' ", PKIDs, schemeName);
                    command = new SQLiteCommand(strSql, conn, tran);
                    adp = new SQLiteDataAdapter
                    {
                        SelectCommand = command
                    };
                    ds = new DataSet();
                    adp.Fill(ds, "TrialItemResultTmp");
                    command.Parameters.Clear();
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        dt = ds.Tables[0];
                    }


                    if (dt != null && dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            //双枪信息合并
                            if (i % 2 == 1 && isGroupTwoCharger)
                                continue;
                            DataRow dr = dt.Rows[i];
                            string strParams = "";
                            //如果是群充双枪的桩，需要合并AB的测试结果
                            if (isGroupTwoCharger)
                            {
                                for (int j = 0; j < dr.ItemArray.Length; j++)
                                {
                                    //条码去掉AB
                                    if (j == 3)
                                    {
                                        strParams += "'" + dr.ItemArray[j].ToString().Substring(0, dr.ItemArray[j].ToString().Length - 1) + "',";
                                    }
                                    //ChargerId需要减半
                                    else if (j == 8)
                                        strParams += "'" + (Math.Ceiling(Convert.ToInt32(dr.ItemArray[j]) / 2.0)).ToString("F0") + "',";
                                    //AB枪结果合并
                                    else if (j == 6)
                                    {
                                        if ((i + 1) < dt.Rows.Count)  //防止越界
                                        {
                                            if (dr.ItemArray[6].ToString().ToUpper() == "PASS" && dt.Rows[i + 1].ItemArray[6].ToString().ToUpper() == "PASS")

                                                strParams += "'PASS',";
                                            else
                                                strParams += "'Fail',";
                                        }
                                        else
                                        {
                                            if (dr.ItemArray[6].ToString().ToUpper() == "PASS")
                                                strParams += "'PASS',";
                                            else
                                                strParams += "'Fail',";
                                        }
                                    }
                                    else
                                        strParams += "'" + dr.ItemArray[j].ToString() + "',";
                                }
                            }
                            else
                            {
                                foreach (object item in dr.ItemArray)
                                {
                                    strParams += "'" + item.ToString() + "',";
                                }
                            }
                            strParams = strParams.TrimEnd(',');
                            string sql = string.Format("Insert Into TrialItemResult  values ({0})", strParams);
                            command.CommandText = sql;
                            int value = command.ExecuteNonQuery();
                            if (value != 1)
                            {
                                tran.Rollback();
                                return false;
                            }
                        }
                    }

                    #endregion

                    strSQL = string.Format("Delete From TrialItemDataTmp WHERE PKID in ({0:S}) And SchemeName = '{1:S}'", PKIDs, schemeName);
                    command = new SQLiteCommand(strSQL, conn, tran);
                    command.CommandText = strSQL;
                    command.ExecuteNonQuery();

                    strSQL = string.Format("Delete From TrialItemResultTmp WHERE PKID in ({0:S}) And SchemeName = '{1:S}'", PKIDs, schemeName);
                    command.CommandText = strSQL;
                    command.ExecuteNonQuery();


                    tran.Commit();
                    return true;
                }
            }

            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                tran.Rollback();
                return false;
            }
        }
        public static void DeleteTrialData(string PKID)
        {
            string strSQL = string.Format("Delete from TrialItemResultTmp Where PKID ={0}", PKID);
            SQLiteHelper.ExecuteNonQuery(SQLiteHelper.DbConnString, CommandType.Text, strSQL, null);
        }

        /// <summary>
        /// 删除试验项总结论数据(临时表)
        /// </summary>
        /// <param name="lstTrialData">试验数据</param>
        /// <returns>返回值(true-成功;false-失败)</returns>
        public static bool DeleteTrialFinalData(TrialDataModel trialData)
        {
            string strDeleteSQL = string.Format("DELETE FROM TrialItemResultTmp WHERE TrialType = {0:D} AND BarCode = '{1:S}' AND TrialName ='{2:S}' AND ChargerID = {3:D}",
                                     (int)trialData.TrialType, trialData.BarCode, trialData.TrialName, trialData.ChargerId);
            if (SQLiteHelper.ExecuteNonQuery(SQLiteHelper.DbConnString, CommandType.Text, strDeleteSQL, null) == 0)
                return false;
            return true;
        }

        /// <summary>
        /// 删除试验项总结论数据(临时表)
        /// </summary>
        /// <param name="lstTrialData">试验数据集合</param>
        /// <returns>返回值(true-成功;false-失败)</returns>
        public static bool DeleteTrialFinalData(List<TrialDataModel> lstTrialData)
        {
            if (lstTrialData == null || lstTrialData.Count == 0)
                return false;
            foreach (TrialDataModel trialData in lstTrialData)
            {
                string strDeleteSQL = string.Format("DELETE FROM TrialItemResultTmp WHERE TrialType = {0:D} AND BarCode = '{1:S}' AND TrialName ='{2:S}' AND ChargerID = {3:D}",
                                         (int)trialData.TrialType, trialData.BarCode, trialData.TrialName, trialData.ChargerId);
                if (SQLiteHelper.ExecuteNonQuery(SQLiteHelper.DbConnString, CommandType.Text, strDeleteSQL, null) == 0)
                    return false;
            }
            return true;
        }

        public static List<string> GetAllTrialResult(string PKID)
        {
            List<string> list = new List<string>();
            string strSQL = string.Format("Select TrialResult From TrialItemResultTmp Where PKID ='{0}'", PKID);
            DataTable dt = SQLiteHelper.ExecuteDataTable(SQLiteHelper.DbConnString, CommandType.Text, strSQL, "TrialItemResultTmp", null);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    list.Add(dr["TrialResult"].ToString());
                }
            }
            return list;
        }

        /// <summary>
        /// 导出数据到临时库
        /// </summary>
        /// <returns></returns>
        public static bool ExportTrialDataToTemp(string strPKID, string strPKID_New)
        {
            SQLiteTransaction tran = null;
            try
            {
                SQLiteConnection conn;
                using (conn = new SQLiteConnection(SQLiteHelper.DbConnString))
                {
                    conn.Open();
                    tran = conn.BeginTransaction();
                    List<long> lstPKID = new List<long>();
                    string schemeName = "";

                    string strSQL = string.Format("Delete From ChargerInfoTmp");
                    SQLiteCommand command = new SQLiteCommand(strSQL, conn, tran);
                    command.CommandText = strSQL;
                    command.ExecuteNonQuery();

                    strSQL = string.Format("Delete From TrialItemDataTmp");
                    command.CommandText = strSQL;
                    command.ExecuteNonQuery();


                    strSQL = string.Format("Delete From TrialItemResultTmp");
                    command.CommandText = strSQL;
                    command.ExecuteNonQuery();


                    #region  ===========枪信息从正式表转入临时表=================


                    strSQL = string.Format("SELECT PKID, ProductName,ProductModel,ProductBarcode,ChargerID,Operater,Auditor,ChargerType , " +
                        "SchemeName,CreateTime ,NominalVoltage,NominalCurrent,Freq ,MinAllowChargeVoltage,MaxOutputPower,MaxAllowChargeCurrent ," +
                        "TrialResult FROM ChargerInfo  Where PKID={0}", strPKID);
                    command = new SQLiteCommand(strSQL, conn, tran);
                    DataTable dt = new DataTable();
                    SQLiteDataAdapter adp = new SQLiteDataAdapter();
                    command.Connection = conn;
                    adp.SelectCommand = command;
                    DataSet ds = new DataSet();
                    adp.Fill(ds, "ChargerInfo");

                    command.Parameters.Clear();
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        dt = ds.Tables[0];
                    }
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            schemeName = DBConvert.ToString(dr["SchemeName"]);
                            dr["PKID"] = strPKID_New;
                            lstPKID.Add(DBConvert.ToInt64(strPKID_New));//这里需要新的ID
                            string strParams = "";
                            foreach (object item in dr.ItemArray)
                            {
                                strParams += "'" + item.ToString() + "',";
                            }
                            strParams = strParams.TrimEnd(',');
                            string sql = string.Format("Insert Into ChargerInfoTmp(PKID, ProductName,ProductModel,ProductBarcode,ChargerID," +
                                "Operater,Auditor,ChargerType , SchemeName,CreateTime ,NominalVoltage,NominalCurrent,Freq," +
                                "MinAllowChargeVoltage,MaxOutputPower,MaxAllowChargeCurrent ,TrialResult)  values ({0})", strParams);

                            command.CommandText = sql;
                            int value = command.ExecuteNonQuery();
                            if (value != 1)
                            {
                                tran.Rollback();
                                return false;
                            }

                            break;//这里只需要一个枪信息，不能多个枪一起导出
                        }
                    }
                    #endregion

                    #region  ===========检测项分项详细数据正式表转入临时表=================
                    string PKIDs = "";
                    foreach (var item in lstPKID)
                    {
                        PKIDs += item + ",";
                    }
                    PKIDs = PKIDs.TrimEnd(',');
                    strSQL = string.Format("SELECT PKID,ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,"
                                 + "SchemeName,Data1,Data2,Data3,TrialResult,SaveTime ,UserSetParams,TrialCondition,RES1,RES2,RES3  "
                                 + "FROM TrialItemData  Where PKID={0} ", strPKID);
                    //strSQL += " order by CAST(Data3 AS int) , SaveTime ";//数据库时间排序不了，这里不用
                    strSQL += " order by CAST(ItemName AS int) ";
                    command = new SQLiteCommand(strSQL, conn, tran);
                    adp = new SQLiteDataAdapter
                    {
                        SelectCommand = command
                    };
                    ds = new DataSet();
                    adp.Fill(ds, "TrialItemData");
                    command.Parameters.Clear();
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        dt = ds.Tables[0];
                    }
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {

                            dr["PKID"] = strPKID_New;
                            string strParams = "";
                            foreach (object item in dr.ItemArray)
                            {
                                strParams += "'" + item.ToString() + "',";
                            }
                            strParams = strParams.TrimEnd(',');
                            string sql = string.Format("Insert Into TrialItemDataTmp  values ({0})", strParams);
                            command.CommandText = sql;
                            int value = command.ExecuteNonQuery();
                            if (value != 1)
                            {
                                tran.Rollback();
                                return false;
                            }
                        }
                    }
                    #endregion

                    #region  ===========检测项总结论数据临时表转入正式表=================

                    string strSql = string.Format("Select * from TrialItemResult  Where PKID={0} ", strPKID);
                    command = new SQLiteCommand(strSql, conn, tran);
                    adp = new SQLiteDataAdapter
                    {
                        SelectCommand = command
                    };
                    ds = new DataSet();
                    adp.Fill(ds, "TrialItemResult");
                    command.Parameters.Clear();
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        dt = ds.Tables[0];
                    }


                    if (dt != null && dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            dr["PKID"] = strPKID_New;
                            string strParams = "";
                            foreach (object item in dr.ItemArray)
                            {
                                strParams += "'" + item.ToString() + "',";
                            }
                            strParams = strParams.TrimEnd(',');
                            string sql = string.Format("Insert Into TrialItemResultTmp  values ({0})", strParams);
                            command.CommandText = sql;
                            int value = command.ExecuteNonQuery();
                            if (value != 1)
                            {
                                tran.Rollback();
                                return false;
                            }
                        }
                    }

                    #endregion


                    tran.Commit();
                    return true;
                }
            }

            catch (Exception ex)
            {
                tran.Rollback();
                Log.Log.LogException(ex);
                return false;
            }
        }


        /// <summary>
        /// 获取指定PKID的检测结果
        /// </summary>
        /// <param name="PKID"></param>
        /// <param name="iResultType">结果类型：0：所有，1：Pass，2：Fail</param>
        /// <returns></returns>
        public static List<TrialDataModel> GetTestItemResultFromPKID(string PKID, int iResultType = 0)
        {
            List<TrialDataModel> lstData = new List<TrialDataModel>();
            try
            {
                string strSql = "";
                DataTable dt;
                strSql = string.Format("Select BarCode,TrialType,TrialName,ChargerID,TrialResult from {0} Where PKID = '{1}'", "TrialItemResult", PKID);
                if (iResultType == 1)
                {
                    strSql += " and trialresult = 'Pass'";
                }
                else if (iResultType == 2)
                {
                    strSql += " and trialresult = 'Fail'";
                }
                dt = SQLiteHelper.ExecuteDataTable(SQLiteHelper.DbConnString, CommandType.Text, strSql, "TrialItemResult", null);


                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        TrialDataModel trialData = new TrialDataModel();
                        if (dr["TrialResult"].ToString().ToUpper() == "PASS")
                        {
                            trialData.TrialFinalResult = EmTrialResult.Pass;
                        }

                        else if (dr["TrialResult"].ToString().ToUpper() == "FAIL")
                        {
                            trialData.TrialFinalResult = EmTrialResult.Fail;
                        }
                        else if (dr["TRIALRESULT"].ToString().ToUpper() == "NA")
                        {
                            trialData.TrialFinalResult = EmTrialResult.NA;
                        }
                        trialData.TrialType = (EmTrialType)Convert.ToInt32(dr["TrialType"]);
                        trialData.TrialName = dr["TrialName"].ToString();
                        trialData.BarCode = dr["BarCode"].ToString();
                        lstData.Add(trialData);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
            return lstData;
        }
    }
}