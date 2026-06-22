using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.Struct;
using SaiTer.ATE.IDAL.DBUtility;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.IDAL.SQLiteIDAL
{
    /// <summary>
    /// 方案名称信息列表数据库表管理类
    /// </summary>
    public class SchemeInfoManage
    {
        /// <summary>
        /// 获取所有方案名称信息(不包含用于编辑方案的标准测试项)
        /// </summary>
        /// <param name="lstSchemeInfo"></param>
        /// <returns></returns>
        public static bool GetSchemeInfo(ref List<StSchemeInfo> lstSchemeInfo)
        {
            try
            {
                if (lstSchemeInfo == null)
                {
                    lstSchemeInfo = new List<StSchemeInfo>();
                }

                string strSQL = "SELECT SCHEMEID,SCHEMENAME,REMARKS,CREATETIME,RES1,RES2,RES3 FROM SCHEMEINFO WHERE SCHEMENAME NOT LIKE '%全部测试项%'";
                SQLiteDataReader reader = SQLiteHelper.ExecuteReader(SQLiteHelper.DbConnString, CommandType.Text, strSQL, null);
                if (reader == null || !reader.HasRows)
                    return false;
                while (reader.Read())
                {
                    StSchemeInfo schemeInfo = new StSchemeInfo();

                    schemeInfo.SchemeID = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    schemeInfo.SchemeName = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    schemeInfo.Remarks = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    schemeInfo.CreatTime = reader.IsDBNull(3) ? "" : reader.GetString(3);
                    schemeInfo.RES1 = reader.IsDBNull(4) ? "" : reader.GetString(4);
                    schemeInfo.RES2 = reader.IsDBNull(5) ? "" : reader.GetString(5);
                    schemeInfo.RES3 = reader.IsDBNull(6) ? "" : reader.GetString(6);
                    lstSchemeInfo.Add(schemeInfo);
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex, "数据库异常");
                return false;
            }
        }

        public static List<StSchemeInfo> GetStandardScheme()
        {
            try
            {
                List<StSchemeInfo> lstSchemeInfo = new List<StSchemeInfo>();

                string strSQL = "SELECT SCHEMEID,SCHEMENAME,REMARKS,CREATETIME,RES1,RES2,RES3 FROM SCHEMEINFO WHERE SCHEMENAME LIKE '%全部测试项%'";
                SQLiteDataReader reader = SQLiteHelper.ExecuteReader(SQLiteHelper.DbConnString, CommandType.Text, strSQL, null);
                if (reader == null || !reader.HasRows)
                    return null;
                while (reader.Read())
                {
                    StSchemeInfo schemeInfo = new StSchemeInfo();

                    schemeInfo.SchemeID = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    schemeInfo.SchemeName = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    schemeInfo.Remarks = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    schemeInfo.CreatTime = reader.IsDBNull(3) ? "" : reader.GetString(3);
                    schemeInfo.RES1 = reader.IsDBNull(4) ? "" : reader.GetString(4);
                    schemeInfo.RES2 = reader.IsDBNull(5) ? "" : reader.GetString(5);
                    schemeInfo.RES3 = reader.IsDBNull(6) ? "" : reader.GetString(6);
                    lstSchemeInfo.Add(schemeInfo);
                }

                return lstSchemeInfo;
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex, "数据库异常");
                return null;
            }
        }
        /// <summary>
        /// 插入一个方案
        /// </summary>
        public static bool InsertScheme(StSchemeInfo info)
        {
            try
            {
                string strSQL = string.Format("Insert Into SchemeInfo Values({0},'{1:S}','{2}','{3}','{4}','{5}','{6}')", info.SchemeID, info.SchemeName, info.Remarks, info.CreatTime, info.RES1, info.RES2, info.RES3);
                int result = SQLiteHelper.ExecuteNonQuery(SQLiteHelper.DbConnString, CommandType.Text, strSQL, null);
                if (result == 0)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex, "数据库异常");
                return false;
            }
        }
        public static void DeleteScheme(string schemeName)
        {
            try
            {
                string strSQL = string.Format("Delete From SchemeInfo where SchemeName = '{0}'", schemeName);
                SQLiteHelper.ExecuteNonQuery(SQLiteHelper.DbConnString, CommandType.Text, strSQL, null);




                //using (SQLiteConnection conn = new SQLiteConnection(SQLiteHelper.DbConnString))
                //{
                //    conn.Open();
                //    using (SQLiteCommand cmd = new SQLiteCommand(SQLiteHelper.DbConnString, conn))
                //    {
                //        cmd.ExecuteNonQuery();
                //    }
                //    conn.Close();

                //}
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex, "数据库异常");
            }
        }
    }
}
