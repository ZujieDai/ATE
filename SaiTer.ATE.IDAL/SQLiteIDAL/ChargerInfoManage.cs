using SaiTer.ATE.IDAL.DBUtility;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.DataBaseModel;
using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SaiTer.ATE.IDAL.SQLiteIDAL
{
    /// <summary>
    /// 充电枪信息表管理类
    /// </summary>
    public class ChargerInfoManage
    {

        /// <summary>
        /// 查找最后插入的充电枪信息
        /// </summary>       
        /// <returns></returns>
        public static bool SelectChargerInfo(out List<ChargerInfoModel> lstChargerInfo)
        {
            lstChargerInfo = new List<ChargerInfoModel>();
            //string strSQL = string.Format("SELECT ProductName,ProductModel,ProductBarcode,ChargerID,Operater,Auditor,ChargerType , SchemeName,CreateTime ,NominalVoltage,NominalCurrent,Freq, max(CreateTime)  FROM ChargerInfoTmp GROUP BY ChargerID");
            string strSQL = string.Format("SELECT PKID, ProductName,ProductModel,ProductBarcode,ChargerID,Operater,Auditor,ChargerType , SchemeName,CreateTime ,NominalVoltage,NominalCurrent,Freq ,MinAllowChargeVoltage,MaxOutputPower,MaxAllowChargeCurrent,CWHightVoltH,CWHightVoltL,CWLowerVoltL,CWLowerVoltH,TrialResult,RES1,RES2,RES3 FROM ChargerInfoTmp");

            DataTable dt = SQLiteHelper.ExecuteDataTable(SQLiteHelper.DbConnString, CommandType.Text, strSQL, "ChargerInfoTmp", null);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    ChargerInfoModel ChargerInfo = new ChargerInfoModel();
                    ChargerInfo.PKID = DBConvert.ToInt64(dr["PKID"]);
                    ChargerInfo.ProductName = DBConvert.ToString(dr["ProductName"]);
                    ChargerInfo.ProductModel = DBConvert.ToString(dr["ProductModel"]);
                    ChargerInfo.BarCode = DBConvert.ToString(dr["ProductBarcode"]);
                    ChargerInfo.ChargerId = DBConvert.ToInt32(dr["ChargerID"]);
                    ChargerInfo.Operater = DBConvert.ToString(dr["Operater"]);
                    ChargerInfo.Auditor = DBConvert.ToString(dr["Auditor"]);
                    ChargerInfo.ChargerType = (EmChargerType)DBConvert.ToInt32(dr["ChargerType"]);
                    ChargerInfo.IsCheck = true;
                    ChargerInfo.CheckResult = DataModel.EnumModel.EmTrialResult.Wait;
                    ChargerInfo.CreateTime = DBConvert.ToString(dr["CreateTime"]);
                    ChargerInfo.SchemeName = DBConvert.ToString(dr["SchemeName"]);
                    ChargerInfo.NominalVoltage = DBConvert.ToDouble(dr["NominalVoltage"]);
                    ChargerInfo.NominalCurrent = DBConvert.ToDouble(dr["NominalCurrent"]);
                    ChargerInfo.Frequency = DBConvert.ToDouble(dr["Freq"]);
                    ChargerInfo.MinAllowChargeVoltage = DBConvert.ToDouble(dr["MinAllowChargeVoltage"]);
                    ChargerInfo.MaxOutputPower = DBConvert.ToDouble(dr["MaxOutputPower"]);
                    ChargerInfo.MaxAllowChargeCurrent = DBConvert.ToDouble(dr["MaxAllowChargeCurrent"]);
                    ChargerInfo.CWHightVoltL = DBConvert.ToDouble(dr["CWHightVoltL"]);
                    ChargerInfo.CWHightVoltH = DBConvert.ToDouble(dr["CWHightVoltH"]);
                    ChargerInfo.CWLowerVoltL = DBConvert.ToDouble(dr["CWLowerVoltL"]);
                    ChargerInfo.CWLowerVoltH = DBConvert.ToDouble(dr["CWLowerVoltH"]);
                    ChargerInfo.RES1 = DBConvert.ToString(dr["RES1"]);
                    ChargerInfo.RES2 = DBConvert.ToString(dr["RES2"]);
                    ChargerInfo.RES3 = DBConvert.ToString(dr["RES3"]);
                    if (dr["TrialResult"].ToString().ToUpper().Equals("PASS"))
                    {
                        ChargerInfo.CheckResult = EmTrialResult.Pass;
                    }
                    else if (dr["TrialResult"].ToString().ToUpper().Equals("FAIL"))
                    {
                        ChargerInfo.CheckResult = EmTrialResult.Fail;
                    }
                    else if (dr["TrialResult"].ToString().ToUpper().Equals("NA"))
                    {
                        ChargerInfo.CheckResult = EmTrialResult.NA;
                    }
                    else
                    {
                        ChargerInfo.CheckResult = EmTrialResult.Wait;
                    }
                    lstChargerInfo.Add(ChargerInfo);
                }
                return true;
            }
            else
                return true;
        }
        /// <summary>
        /// 查找正式库中是否已经存在该条码
        /// </summary>
        /// <param name="barCode">需要查询的条码</param>
        /// <returns></returns>
        public static bool SelectIsHasChargerInfo(string barCode, ref long PKID)
        {
            string strSQL = string.Format("Select PKID from ChargerInfo Where ProductBarCode in ({0})", barCode);
            DataTable dt = SQLiteHelper.ExecuteDataTable(SQLiteHelper.DbConnString, CommandType.Text, strSQL, "ChargerInfo", null);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    PKID = DBConvert.ToInt64(dr["PKID"]);
                }
            }
            if (dt != null && dt.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool InsertChargerInfo(List<ChargerInfoModel> lstChargerInfo)
        {

            //先删除历史数据再插入
            string strDeleteSQL = string.Format("DELETE FROM ChargerInfoTmp ");
            SQLiteHelper.ExecuteNonQuery(SQLiteHelper.DbConnString, CommandType.Text, strDeleteSQL, null);
            foreach (ChargerInfoModel model in lstChargerInfo)
            {
                string strSQL = string.Format("INSERT INTO  ChargerInfoTmp (PKID,ProductName,ProductModel,ProductBarCode,ChargerID,Operater,Auditor,ChargerType,CreateTime,SchemeName,NominalVoltage," +
                    "NominalCurrent, Freq, MaxAllowChargeCurrent,MaxOutputPower,MinAllowChargeVoltage,CWHightVoltH,CWHightVoltL,CWLowerVoltH,CWLowerVoltL,RES1,RES2,RES3)  " +
                    "VALUES ({12}, '{0:S}','{1:S}','{2:S}',{3:D},'{4:S}','{5:S}',{6:D},'{7:S}','{8:S}','{9}','{10}','{11}','{13}','{14}','{15}','{16}','{17}','{18}','{19}','{20}','{21}','{22}')",
                    model.ProductName, model.ProductModel, model.BarCode, model.ChargerId, model.Operater, model.Auditor, (int)model.ChargerType, model.CreateTime, model.SchemeName,
                    model.NominalVoltage, model.NominalCurrent, model.Frequency, model.PKID, model.MaxAllowChargeCurrent, model.MaxOutputPower, model.MinAllowChargeVoltage,
                    model.CWHightVoltH, model.CWHightVoltL, model.CWLowerVoltH, model.CWLowerVoltL, model.RES1, model.RES2, model.RES3);

                if (SQLiteHelper.ExecuteNonQuery(SQLiteHelper.DbConnString, CommandType.Text, strSQL, null) == 0)
                    return false;
            }
            return true;

        }
        /// <summary>
        /// 更新枪信息中所使用的方案名称
        /// </summary>
        /// <param name="lstChargerInfo"></param>
        /// <param name="schemeName"></param>
        /// <returns></returns>
        public static bool UpdateChargerInfo(List<ChargerInfoModel> lstChargerInfo, string schemeName)
        {
            foreach (ChargerInfoModel model in lstChargerInfo)
            {
                string strSQL = string.Format("Update ChargerInfoTmp Set SchemeName = '{0:S}' Where PKID = {1}", schemeName, model.PKID);
                if (SQLiteHelper.ExecuteNonQuery(SQLiteHelper.DbConnString, CommandType.Text, strSQL, null) == 0)
                    return false;
            }
            return true;
        }
        /// <summary>
        /// 更新枪信息中所使用的信息
        /// </summary>
        /// <param name="lstChargerInfo"></param>
        /// <returns></returns>
        public static bool UpdateChargerInfo(List<ChargerInfoModel> lstChargerInfo, string NominalVoltage, string NominalCurrent, string SchemeName)
        {
            foreach (ChargerInfoModel model in lstChargerInfo)
            {
                string strSQL = string.Format("Update ChargerInfoTmp Set NominalVoltage = '{0:S}', NominalCurrent = '{1:S}', SchemeName = '{2:S}' Where PKID = {3}", NominalVoltage, NominalCurrent, SchemeName, model.PKID);
                if (SQLiteHelper.ExecuteNonQuery(SQLiteHelper.DbConnString, CommandType.Text, strSQL, null) == 0)
                    return false;
            }
            return true;
        }
        /// <summary>
        /// 临时库枪信息入正式库
        /// </summary>
        /// <returns></returns>
        public static bool InsertChargerInfoToFormalTable(ref List<long> lstPKID, ref string schemeName)
        {
            lstPKID.Clear();
            string strSQL = string.Format("SELECT PKID, ProductName,ProductModel,ProductBarcode,ChargerID,Operater,Auditor,ChargerType , SchemeName,CreateTime ,NominalVoltage,NominalCurrent,Freq ,MinAllowChargeVoltage,MaxOutputPower,MaxAllowChargeCurrent,CWHightVoltH,CWHightVoltL,CWLowerVoltL,CWLowerVoltH,TrialResult FROM ChargerInfoTmp ");

            DataTable dt = SQLiteHelper.ExecuteDataTable(SQLiteHelper.DbConnString, CommandType.Text, strSQL, "ChargerInfoTmp", null);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    schemeName = DBConvert.ToString(dr["SchemeName"]);
                    lstPKID.Add(DBConvert.ToInt64(dr["PKID"]));
                    string strParams = "";
                    foreach (object item in dr.ItemArray)
                    {
                        strParams += "'" + item.ToString() + "',";
                    }
                    strParams = strParams.TrimEnd(',');
                    string sql = string.Format("Insert Into ChargerInfo(PKID, ProductName,ProductModel,ProductBarcode,ChargerID,Operater,Auditor,ChargerType , SchemeName,CreateTime ,NominalVoltage,NominalCurrent,Freq,MinAllowChargeVoltage,MaxOutputPower,MaxAllowChargeCurrent,CWHightVoltH,CWHightVoltL,CWLowerVoltL,CWLowerVoltH,TrialResult)  values ({0})", strParams);
                    if (SQLiteHelper.ExecuteNonQuery(SQLiteHelper.DbConnString, CommandType.Text, sql, null) == 0)
                        return false;

                }
            }
            return true;
        }

        public static ChargerInfoModel GetChargerInfoFromFomalTable(string PKID)
        {
            ChargerInfoModel ChargerInfo = new ChargerInfoModel();
            string sql = string.Format("SELECT PKID, ProductName,ProductModel,ProductBarcode,ChargerID,Operater,Auditor,ChargerType , SchemeName,CreateTime ,NominalVoltage,NominalCurrent," +
                "Freq ,MinAllowChargeVoltage,MaxOutputPower,MaxAllowChargeCurrent,CWHightVoltH,CWHightVoltL,CWLowerVoltL,CWLowerVoltH,RES1,RES2,RES3,TrialResult FROM ChargerInfo  Where PKID={0}", PKID);
            DataTable dt = SQLiteHelper.ExecuteDataTable(SQLiteHelper.DbConnString, CommandType.Text, sql, "ChargerInfo", null);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {

                    ChargerInfo.PKID = DBConvert.ToInt64(dr["PKID"]);
                    ChargerInfo.ProductName = DBConvert.ToString(dr["ProductName"]);
                    ChargerInfo.ProductModel = DBConvert.ToString(dr["ProductModel"]);
                    ChargerInfo.BarCode = DBConvert.ToString(dr["ProductBarcode"]);
                    ChargerInfo.ChargerId = DBConvert.ToInt32(dr["ChargerID"]);
                    ChargerInfo.Operater = DBConvert.ToString(dr["Operater"]);
                    ChargerInfo.Auditor = DBConvert.ToString(dr["Auditor"]);
                    ChargerInfo.ChargerType = (EmChargerType)DBConvert.ToInt32(dr["ChargerType"]);
                    ChargerInfo.IsCheck = true;
                    ChargerInfo.CheckResult = DataModel.EnumModel.EmTrialResult.Wait;
                    ChargerInfo.CreateTime = DBConvert.ToString(dr["CreateTime"]);
                    ChargerInfo.SchemeName = DBConvert.ToString(dr["SchemeName"]);
                    ChargerInfo.NominalVoltage = DBConvert.ToDouble(dr["NominalVoltage"]);
                    ChargerInfo.NominalCurrent = DBConvert.ToDouble(dr["NominalCurrent"]);
                    ChargerInfo.Frequency = DBConvert.ToDouble(dr["Freq"]);
                    ChargerInfo.MinAllowChargeVoltage = DBConvert.ToDouble(dr["MinAllowChargeVoltage"]);
                    ChargerInfo.MaxOutputPower = DBConvert.ToDouble(dr["MaxOutputPower"]);
                    ChargerInfo.MaxAllowChargeCurrent = DBConvert.ToDouble(dr["MaxAllowChargeCurrent"]);
                    ChargerInfo.CWHightVoltL = DBConvert.ToDouble(dr["CWHightVoltL"]);
                    ChargerInfo.CWHightVoltH = DBConvert.ToDouble(dr["CWHightVoltH"]);
                    ChargerInfo.CWLowerVoltL = DBConvert.ToDouble(dr["CWLowerVoltL"]);
                    ChargerInfo.CWLowerVoltH = DBConvert.ToDouble(dr["CWLowerVoltH"]);
                    ChargerInfo.RES1 = DBConvert.ToString(dr["RES1"]);
                    ChargerInfo.RES2 = DBConvert.ToString(dr["RES2"]);
                    ChargerInfo.RES3 = DBConvert.ToString(dr["RES3"]);
                    if (dr["TrialResult"].ToString().ToUpper().Equals("PASS"))
                    {
                        ChargerInfo.CheckResult = EmTrialResult.Pass;
                    }
                    else if (dr["TrialResult"].ToString().ToUpper().Equals("FAIL"))
                    {
                        ChargerInfo.CheckResult = EmTrialResult.Fail;
                    }
                    else if (dr["TrialResult"].ToString().ToUpper().Equals("NA"))
                    {
                        ChargerInfo.CheckResult = EmTrialResult.NA;
                    }
                    else
                    {
                        ChargerInfo.CheckResult = EmTrialResult.Wait;
                    }
                }


            }
            return ChargerInfo;
        }
        /// <summary>
        /// 更新枪信息的测试结果
        /// </summary>
        /// <param name="PKID"></param>
        public static void UpdateChargerTrialResult(string PKID)
        {
            try
            {
                List<string> lst = TrialItemResultTmpManage.GetAllTrialResult(PKID);
                int index = lst.FindIndex(s => s.ToUpper().Equals("FAIL"));
                string trialResult = "Fail";
                if (index < 0)
                {
                    trialResult = "Pass";
                }
                string strSQL = string.Format("Update ChargerInfoTmp Set TrialResult ='{0}' Where PKID = '{1}'", trialResult, PKID);
                SQLiteHelper.ExecuteNonQuery(SQLiteHelper.DbConnString, CommandType.Text, strSQL, null);
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        /// <summary>
        /// 查询当日检测的所有桩数量(正式表)
        /// </summary>
        /// <param name="PassCount">合格桩数量</param>
        /// <returns></returns>
        public static int GetTestChargerCount(out int PassCount)
        {
            string strSQL = "select count(*) from chargerInfo where CreateTime >= date('now','start of day')";
            int total = Convert.ToInt32(SQLiteHelper.ExecuteScalar(SQLiteHelper.DbConnString, CommandType.Text, strSQL, null));
            strSQL = "select count(*) from chargerInfo where CreateTime >= date('now','start of day') and trialresult ='Pass'";
            PassCount = Convert.ToInt32(SQLiteHelper.ExecuteScalar(SQLiteHelper.DbConnString, CommandType.Text, strSQL, null));
            return total;
        }

        /// <summary>
        /// 获取指定时间段内检测数量
        /// </summary>
        /// <param name="sStartTime">开始时间</param>
        /// <param name="sEndTime">结束时间</param>
        /// <param name="iResultType">结果类型：0：所有，1：Pass，2：Fail</param>
        /// <returns></returns>
        public static int GetTestCount(string sStartTime, string sEndTime, int iResultType = 0)
        {
            string strSQL = "select count(*) from chargerInfo " +
                "where CreateTime >= datetime('" + sStartTime + "') and CreateTime <=datetime('" + sEndTime + "')";
            if (iResultType == 1)
            {
                strSQL += " and trialresult = 'Pass'";
            }
            else if (iResultType == 2)
            {
                strSQL += " and trialresult = 'Fail'";
            }

            int total = Convert.ToInt32(SQLiteHelper.ExecuteScalar(SQLiteHelper.DbConnString, CommandType.Text, strSQL, null));
            return total;
        }

        /// <summary>
        /// 获取指定时间段内充电枪信息
        /// </summary>
        /// <param name="sStartTime">开始时间</param>
        /// <param name="sEndTime">结束时间</param>
        /// <param name="sModel">型号</param>
        /// <param name="iResultType">结果类型：0：所有，1：Pass，2：Fail</param>
        /// <returns></returns>
        public static List<ChargerInfoModel> GetTestChargerInfo(string sStartTime, string sEndTime,string sModel="", int iResultType = 0)
        {
            List<ChargerInfoModel> lstChargerInfo = new List<ChargerInfoModel>();  
            string strSQL = string.Format("SELECT PKID, ProductName,ProductModel,ProductBarcode,ChargerID,Operater,Auditor,ChargerType , SchemeName,CreateTime ,NominalVoltage,NominalCurrent,Freq ,MinAllowChargeVoltage,MaxOutputPower,MaxAllowChargeCurrent,CWHightVoltH,CWHightVoltL,CWLowerVoltL,CWLowerVoltH ,TrialResult,RES1,RES2,RES3 FROM ChargerInfo ") +
                "where CreateTime >= datetime('" + sStartTime + "') and CreateTime <= datetime('" + sEndTime + "')";
            if (iResultType == 1)
            {
                strSQL += " and trialresult = 'Pass'";
            }
            else if (iResultType == 2)
            {
                strSQL += " and trialresult = 'Fail'";
            }

            if (sModel.Trim() != "")
            {
                strSQL += " and ProductModel = '" + sModel + "'";
            }

            DataTable dt = SQLiteHelper.ExecuteDataTable(SQLiteHelper.DbConnString, CommandType.Text, strSQL, "ChargerInfoTmp", null);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    ChargerInfoModel ChargerInfo = new ChargerInfoModel();
                    ChargerInfo.PKID = DBConvert.ToInt64(dr["PKID"]);
                    ChargerInfo.ProductName = DBConvert.ToString(dr["ProductName"]);
                    ChargerInfo.ProductModel = DBConvert.ToString(dr["ProductModel"]);
                    ChargerInfo.BarCode = DBConvert.ToString(dr["ProductBarcode"]);
                    ChargerInfo.ChargerId = DBConvert.ToInt32(dr["ChargerID"]);
                    ChargerInfo.Operater = DBConvert.ToString(dr["Operater"]);
                    ChargerInfo.Auditor = DBConvert.ToString(dr["Auditor"]);
                    ChargerInfo.ChargerType = (EmChargerType)DBConvert.ToInt32(dr["ChargerType"]);
                    ChargerInfo.IsCheck = true;
                    ChargerInfo.CheckResult = DataModel.EnumModel.EmTrialResult.Wait;
                    ChargerInfo.CreateTime = DBConvert.ToString(dr["CreateTime"]);
                    ChargerInfo.SchemeName = DBConvert.ToString(dr["SchemeName"]);
                    ChargerInfo.NominalVoltage = DBConvert.ToDouble(dr["NominalVoltage"]);
                    ChargerInfo.NominalCurrent = DBConvert.ToDouble(dr["NominalCurrent"]);
                    ChargerInfo.Frequency = DBConvert.ToDouble(dr["Freq"]);
                    ChargerInfo.MinAllowChargeVoltage = DBConvert.ToDouble(dr["MinAllowChargeVoltage"]);
                    ChargerInfo.MaxOutputPower = DBConvert.ToDouble(dr["MaxOutputPower"]);
                    ChargerInfo.MaxAllowChargeCurrent = DBConvert.ToDouble(dr["MaxAllowChargeCurrent"]);
                    ChargerInfo.CWHightVoltL = DBConvert.ToDouble(dr["CWHightVoltL"]);
                    ChargerInfo.CWHightVoltH = DBConvert.ToDouble(dr["CWHightVoltH"]);
                    ChargerInfo.CWLowerVoltL = DBConvert.ToDouble(dr["CWLowerVoltL"]);
                    ChargerInfo.CWLowerVoltH = DBConvert.ToDouble(dr["CWLowerVoltH"]);
                    ChargerInfo.RES1 = DBConvert.ToString(dr["RES1"]);
                    ChargerInfo.RES2 = DBConvert.ToString(dr["RES2"]);
                    ChargerInfo.RES3 = DBConvert.ToString(dr["RES3"]);
                    if (dr["TrialResult"].ToString().ToUpper().Equals("PASS"))
                    {
                        ChargerInfo.CheckResult = EmTrialResult.Pass;
                    }
                    else if (dr["TrialResult"].ToString().ToUpper().Equals("FAIL"))
                    {
                        ChargerInfo.CheckResult = EmTrialResult.Fail;
                    }
                    else if (dr["TrialResult"].ToString().ToUpper().Equals("NA"))
                    {
                        ChargerInfo.CheckResult = EmTrialResult.NA;
                    }
                    else
                    {
                        ChargerInfo.CheckResult = EmTrialResult.Wait;
                    }
                    lstChargerInfo.Add(ChargerInfo);
                }
                return lstChargerInfo;
            }
            else
                return lstChargerInfo;
        }
    }
}
