using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.DataBaseModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.Struct;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.IDAL
{
    #region =========== 试验方案、试验数据 ===========
    /// <summary>
    /// 试验方案、试验数据管理类
    /// </summary>
    public class TrialItemsDataManage
    {
        public static readonly string DbConnString = @"Data Source=SQLiteSaiTerNew.db;Version=3;";

        static string SQLiteFilePath = System.AppDomain.CurrentDomain.BaseDirectory + "SQLiteSaiTerNew" + ".db";
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
                SQLiteConnection SQLiteConn = new SQLiteConnection("data source=" + SQLiteFilePath);

                SQLiteConn.Open();//打开数据库，若文件不存在会自动创建  
                SQLiteCommand cmd = SQLiteConn.CreateCommand();
                cmd.CommandText = "SELECT TRIALNAME, TRIALTYPE,TRIALORDER,TRIALMETHOD,RESULTPARAM,OTHERPARAMS FROM TRIALITEMS ORDER BY TRIALORDER";
                SQLiteDataReader reader = cmd.ExecuteReader();


                //SQLiteDataReader reader = CsharpSQLiteHelper.ExecuteReader(DbConnString, CommandType.Text, strSQL, null);
                if (reader == null || !reader.HasRows)
                    return false;
                while (reader.Read())
                {
                    StTrialItem trialScheme = new StTrialItem();

                    trialScheme.ItemName = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    trialScheme.TrialType = (EmTrialType)(reader.IsDBNull(1) ? 0 : reader.GetInt32(1));
                    trialScheme.TrialOrder = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                    trialScheme.TrialMethod = reader.IsDBNull(3) ? "" : reader.GetString(3);
                    trialScheme.ResultParams = reader.IsDBNull(4) ? "" : reader.GetString(4);
                    trialScheme.ItemParams = reader.IsDBNull(5) ? "" : reader.GetString(5);

                    lstTrialItems.Add(trialScheme);
                }
                return true;
            }
            catch (Exception ex) { return false; }

        }


        #endregion


        #region ---------------检测数据------------
        /// <summary>
        /// 保存试验数据(临时表)
        /// </summary>
        /// <param name="lstTrialData">试验数据集合</param>
        /// <returns>返回值(true-成功;false-失败)</returns>
        public static bool SaveTrialData(List<TrialDataModel> lstTrialData)
        {
            if (lstTrialData == null || lstTrialData.Count == 0)
                return false;

            SQLiteConnection SQLiteConn = new SQLiteConnection("data source=" + SQLiteFilePath);

            SQLiteConn.Open();//打开数据库，若文件不存在会自动创建  
            SQLiteCommand cmd = SQLiteConn.CreateCommand();
            foreach (TrialDataModel trialData in lstTrialData)
            {
                //先删除历史数据再插入
                string strDeleteSQL = string.Format("DELETE FROM TestItemResultTmp WHERE TestTYPE = {0:D} ",
                                         (int)trialData.TrialType);
                ExecuteNonQuery("data source=" + SQLiteFilePath, CommandType.Text, strDeleteSQL, null);
                string strSQL = string.Format("INSERT INTO TestItemResultTmp  VALUES ('{0:S}',{1:D},'{2:S}',{3:D})",
                    trialData.BarCode, (int)trialData.TrialResult, trialData.ExtentData, trialData.TrialType);

                if (ExecuteNonQuery("data source=" + SQLiteFilePath, CommandType.Text, strSQL, null) == 0)
                    return false;
            }
            return true;
        }

        #endregion




        /// <summary>
        /// 执行一个不需要返回值的SQLiteCommand命令，通过指定专用的连接字符串
        /// 使用参数数组形式提供参数列表
        /// </summary>
        /// <remarks>
        /// 使用示例：
        /// int result = ExecuteNonQuery(connString,CommandType.StoredProcedure,"PulishOrders",new SQLiteParameter("@prodid",24));
        /// </remarks>
        /// <param name="connectionString">一个有效的数据库连接字符串</param>
        /// <param name="cmdType">SQLiteCommand命令类型(存储过程'StoredProcedure'，表的名称'TableDirect'，
        /// T-SQL 文本命令语句'Text'(默认)，等等)</param>
        /// <param name="cmdText">存储过程的名字或者表的名字或者 T-SQL 文本命令语句</param>
        /// <param name="commandParamters">以数组形式提供SQLiteCommand命令中用到的参数列表</param>
        /// <returns>返回一个数值表示此SQLiteCommand命令执行后影响的行数</returns>
        public static int ExecuteNonQuery(string connectionString, CommandType cmdType, string cmdText, params SQLiteParameter[] commandParamters)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    //通过PrePareCommand方法将参数逐个加入到SQLiteCommand的参数集合中
                    PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParamters);
                    cmd.ExecuteNonQuery();
                    //清空SQLiteCommand中的参数列表
                    cmd.Parameters.Clear();
                    return 1;
                }
            }
            catch(Exception ex)
            {
                //数据库错误多为致命错误，所以暂时全部上抛处理.
                throw;
                return 0;
            }
        }
        /// <summary>
        /// 为执行命令准备参数
        /// </summary>
        /// <param name="cmd">SQLiteCommand命令</param>
        /// <param name="conn">已经存在的数据库连接</param>
        /// <param name="trans">数据库事物处理</param>
        /// <param name="cmdType">SQLiteCommand命令类型 (存储过程<StoredProcedure>，表的名称<TableDirect>， T-SQL 文本命令语句<Text>(默认)， 等等。)</param>
        /// <param name="cmdText">存储过程的名字或者表的名字或者 T-SQL 文本命令语句</param>
        /// <param name="cmdParms">以数组形式提供SQLiteCommand命令中用到的参数列表</param>
        private static void PrepareCommand(SQLiteCommand cmd, SQLiteConnection conn, SQLiteTransaction trans, CommandType cmdType, string cmdText, SQLiteParameter[] cmdParms)
        {
            //判断数据库连接状态
            if (conn.State != ConnectionState.Open)
                conn.Open();

            cmd.Connection = conn;
            cmd.CommandText = cmdText;

            //判断是否需要事务处理
            if (trans != null)
                cmd.Transaction = trans;

            cmd.CommandType = cmdType;

            if (cmdParms != null)
            {
                foreach (SQLiteParameter parm in cmdParms)
                    cmd.Parameters.Add(parm);
            }
        }
    }
    #endregion
}
