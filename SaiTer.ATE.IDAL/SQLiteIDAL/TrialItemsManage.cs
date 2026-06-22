using NPOI.SS.Formula.Functions;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.Struct;
using SaiTer.ATE.IDAL.DBUtility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.IDAL.SQLiteIDAL
{
    /// <summary>
    /// 试验方案数据库管理类
    /// </summary>
    public class TrialItemsManage
    {

        #region  ----------------试验项目管理-------------------
        /// <summary>
        /// 获取数据库中的试验方案
        /// </summary>
        /// <param name="lstTrialItems">方案数据结构对象集合</param>
        /// <returns>返回值(true-成功；false-失败)</returns>
        public static bool GetTrialScheme(ref List<StTrialItem> lstTrialItems)
        {
            try
            {
                if (lstTrialItems == null)
                {
                    lstTrialItems = new List<StTrialItem>();
                }

                string strSQL = "SELECT TRIALNAME, TRIALTYPE,TRIALORDER,TRIALMETHOD,DECIDESTANDARD,RESULTPARAM,OTHERPARAMS FROM TRIALITEMS ORDER BY TRIALORDER";
                SQLiteDataReader reader = SQLiteHelper.ExecuteReader(SQLiteHelper.DbConnString, CommandType.Text, strSQL, null);
                if (reader == null || !reader.HasRows)
                    return false;
                while (reader.Read())
                {
                    StTrialItem trialScheme = new StTrialItem();

                    trialScheme.ItemName = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    trialScheme.TrialType = (EmTrialType)(reader.IsDBNull(1) ? 0 : reader.GetInt32(1));
                    trialScheme.TrialOrder = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                    trialScheme.TrialMethod = reader.IsDBNull(3) ? "" : reader.GetString(3);
                    trialScheme.DecideStandard = reader.IsDBNull(4) ? "" : reader.GetString(4);
                    trialScheme.ResultParams = reader.IsDBNull(5) ? "" : reader.GetString(5);
                    trialScheme.ItemParams = reader.IsDBNull(6) ? "" : reader.GetString(6);

                    lstTrialItems.Add(trialScheme);
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex, "数据库异常");
                return false;
            }
        }
        /// <summary>
        /// 获取数据库中指定方案名称的试验方案
        /// </summary>
        /// <param name="lstTrialItems">方案数据结构对象集合</param>
        /// <returns>返回值(true-成功；false-失败)</returns>
        public static bool GetTrialSchemeFromSchemeName(string schemeName, ref List<StTrialItem> lstTrialItems)
        {
            try
            {
                if (lstTrialItems == null)
                {
                    lstTrialItems = new List<StTrialItem>();
                }

                string strSQL = "SELECT SCHEMEID,SCHEMENAME, TRIALNAME, TRIALTYPE,TRIALORDER,TRIALMETHOD,DECIDESTANDARD,RESULTPARAM,OTHERPARAMS FROM TRIALITEMS WHERE SCHEMENAME in ('" + schemeName + "') AND ENABLE = 1  ORDER BY TRIALORDER";
                SQLiteDataReader reader = SQLiteHelper.ExecuteReader(SQLiteHelper.DbConnString, CommandType.Text, strSQL, null);
                if (reader == null || !reader.HasRows)
                    return false;
                while (reader.Read())
                {
                    StTrialItem trialScheme = new StTrialItem();
                    trialScheme.SchemeID = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    trialScheme.SchemeName = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    trialScheme.ItemName = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    trialScheme.TrialType = (EmTrialType)(reader.IsDBNull(3) ? 0 : reader.GetInt32(3));
                    trialScheme.TrialOrder = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);
                    trialScheme.TrialMethod = reader.IsDBNull(5) ? "" : reader.GetString(5);
                    trialScheme.DecideStandard = reader.IsDBNull(6) ? "" : reader.GetString(6);
                    trialScheme.ResultParams = reader.IsDBNull(7) ? "" : reader.GetString(7);
                    trialScheme.ItemParams = reader.IsDBNull(8) ? "" : reader.GetString(8);

                    lstTrialItems.Add(trialScheme);
                }
                reader.Close();
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }

        public static int UpdateTrialScheme(string schemeName, string trialName, EmTrialType trialType, string resultParam, string otherParams)
        {
            try
            {
                //string strSQL = string.Format("update trialItems set resultParam = '{0:S}' , otherParams = '{1:s}' where schemeName = '{2:S}' and trialName = '{3:S}' and trialType ={4:D}", resultParam, otherParams, schemeName, trialName, (int)trialType);
                //string strSQL = string.Format("update trialItems set resultParam = '{0:S}'  where schemeName = '{2:S}' and trialName = '{3:S}' and trialType ={4:D}", resultParam, otherParams, schemeName, trialName, (int)trialType);
                //int result = SQLiteHelper.ExecuteNonQuery(SQLiteHelper.DbConnString, CommandType.Text, strSQL, null);
                string strSQL = string.Format("update trialItems set resultParam = @resultParam  where schemeName = '{1:S}' and trialName = '{2:S}' and trialType ={3:D}", otherParams, schemeName, trialName, (int)trialType);
                var p1 = new SQLiteParameter("@resultParam", resultParam ?? (object)DBNull.Value);
                int result = SQLiteHelper.ExecuteNonQuery(SQLiteHelper.DbConnString, CommandType.Text, strSQL, new SQLiteParameter[] { p1 });
                return result;
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex, "数据库异常");
                return 0;
            }
        }

        /// <summary>
        /// 插入一个完整的方案测试项
        /// </summary>
        /// <param name="lstTrialItems"></param>
        /// <returns></returns>
        public static bool InsertTrialItems(List<StTrialItem> lstTrialItems)
        {
            try
            {
                foreach (StTrialItem TrialItems in lstTrialItems)
                {
                    string strSQL = string.Format("Insert Into  TrialItems Values({0},'{1}','{2}','{3}','{4}','{5}','{6}',@ResultParams,'{8}',1)",
                      TrialItems.SchemeID, TrialItems.SchemeName, TrialItems.ItemName, (int)TrialItems.TrialType, TrialItems.TrialOrder, TrialItems.TrialMethod, TrialItems.DecideStandard, TrialItems.ResultParams, TrialItems.ItemParams);
                    var p1 = new SQLiteParameter("@ResultParams", TrialItems.ResultParams ?? (object)DBNull.Value);
                    int result = SQLiteHelper.ExecuteNonQuery(SQLiteHelper.DbConnString, CommandType.Text, strSQL, new SQLiteParameter[] { p1 });
                    if (result == 0)
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex, "数据库异常");
                return false;
            }
        }

        public static void DeleteTrialItems(string schemeName)
        {
            try
            {
                string strSQL = string.Format("Delete From TrialItems Where SchemeName = '{0}'", schemeName);
                SQLiteHelper.ExecuteNonQuery(SQLiteHelper.DbConnString, CommandType.Text, strSQL, null);
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex, "数据库异常");

            }
        }

        #endregion
    }
}
