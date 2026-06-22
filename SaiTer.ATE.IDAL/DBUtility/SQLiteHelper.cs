using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.IDAL.DBUtility
{
    // <summary>
    /// SQLite数据库的通用访问类(静态类)
    /// </summary>
    public static class SQLiteHelper
    {
        /// <summary>
        /// 获取数据库连接字符串，其属于静态变量且只读，项目中所有文档可以直接使用，但不能修改
        /// </summary>
        public static readonly string DbConnString = @"Data Source=SQLiteSaiTerNew.db;Version=3;";

        static string SQLiteFilePath = System.AppDomain.CurrentDomain.BaseDirectory + "SQLiteSaiTerNew" + ".db";
        /// <summary>
        /// 哈希表用来存储缓存的参数信息，哈希表可以存储任意类型的参数
        /// </summary>
        private static Hashtable parmCache = Hashtable.Synchronized(new Hashtable());

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
            catch
            {
                //数据库错误多为致命错误，所以暂时全部上抛处理.
                throw;
                return 0;
            }
        }

        /// <summary>
        /// 执行一个不需要返回值的SQLiteCommand命令，通过一个已经存在的数据库连接
        /// 使用参数数组提供参数
        /// </summary>
        /// <remarks>
        /// 使用示例：
        /// int result = ExecuteNonQuery(conn,CommandType.StoredProcedure,"PulishOrders",new SQLiteParameter("@prodid",24));
        /// </remarks>
        /// <param name="connection">一个现有的数据库连接</param>
        /// <param name="cmdType">SQLiteCommand命令类型(存储过程'StoredProcedure'，表的名称'TableDirect'，
        /// T-SQL 文本命令语句'Text'(默认)， 等等)</param>
        /// <param name="cmdText">存储过程的名字或者表的名字或者 T-SQL 文本命令语句</param>
        /// <param name="commandParameters">以数组形式提供SQLiteCommand命令中用到的参数列表</param>
        /// <returns>返回一个数值表示此SQLiteCommand命令执行后影响的行数</returns>
        public static int ExecuteNonQuery(SQLiteConnection connection, CommandType cmdType, string cmdText, params SQLiteParameter[] commandParameters)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            try
            {
                PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
                cmd.ExecuteNonQuery();

                //清空SQLiteCommand中的参数列表
                cmd.Parameters.Clear();
                return 1;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 执行一个不需要返回值的SQLiteCommand命令，通过一个已经存在的数据库事务处理
        /// 使用参数数组提供参数
        /// </summary>
        /// <remarks>
        /// 使用示例：
        /// int result = ExecuteNonQuery(trans,CommandType.StoredProcedure,"PulishOrders",new SQLiteParameter("@prodid",24));
        /// </remarks>
        /// <param name="trans">一个存在的SQLite事务处理</param>
        /// <param name="cmdType">SQLiteCommand命令类型(存储过程'StoredProcedure'，表的名称'TableDirect'，
        /// T-SQL 文本命令语句'Text'(默认)， 等等)</param>
        /// <param name="cmdText">存储过程的名字或者表的名字或者 T-SQL 文本命令语句</param>
        /// <param name="commandParameters">以数组形式提供SQLiteCommand命令中用到的参数列表</param>
        /// <returns>返回一个数值表示此SQLiteCommand命令执行后影响的行数</returns>
        public static int ExecuteNonQuery(SQLiteTransaction trans, CommandType cmdType, string cmdText, params SQLiteParameter[] commandParameters)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            try
            {
                PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, commandParameters);
                cmd.ExecuteNonQuery();

                //清空SQLiteCommand中的参数列表
                cmd.Parameters.Clear();
                return 1;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 返回DATASET
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="cmdType">SQLiteCommand命令类型</param>
        /// <param name="cmdText">存储过程的名字或者表的名字或者 T-SQL 文本命令语句</param>
        /// <param name="dataname">返回的DataSet中DataTable的名字</param>
        /// <param name="commandParameters">参数列表</param>
        /// <returns>返回DataSet</returns>
        public static DataSet ExecuteDataset(string connectionString, CommandType cmdType, string cmdText, string dataname, params SQLiteParameter[] commandParameters)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            SQLiteConnection conn = new SQLiteConnection(connectionString);
            //SqlDataReader rdr = null;

            /**********
            在这里使用try/catch处理是因为如果方法出现异常，则SqlDataReader就不存在，
            CommandBehavior.CloseConnection的语句就不会执行，触发的异常由catch捕获。
            关闭数据库连接，并通过throw再次引发捕捉到的异常。
            **********/
            try
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                SQLiteDataAdapter adp = new SQLiteDataAdapter();
                cmd.Connection = conn;
                adp.SelectCommand = cmd;
                DataSet ds = new DataSet();
                adp.Fill(ds, dataname);

                cmd.Parameters.Clear();
                return ds;
            }
           
            catch (Exception ex)
            {
                conn.Close();
                //数据库错误多为致命错误，所以暂时全部上抛处理.
                throw ex;
            }
        }

        /// <summary>
        /// 输入一条 查询的SQL语句，返回一个DataTable的结果集
        /// </summary>
        /// <param name="cmdText"></param>
        /// <returns></returns>
        public static DataTable ExecuteDataTable(string ConnString, string cmdText)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(ConnString))
                {
                    DataSet ds = new DataSet();
                    SQLiteDataAdapter odda = new SQLiteDataAdapter(cmdText, conn);
                    odda.Fill(ds, "table");
                    return ds.Tables[0];
                }
            }
            catch (Exception ex)
            {                
                //数据库错误多为致命错误，所以暂时全部上抛处理.
                throw ex;
            }
        }

        /// <summary>
        /// 返回DATATABLE
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="cmdType">SQLiteCommand命令类型</param>
        /// <param name="cmdText">存储过程的名字或者表的名字或者 T-SQL 文本命令语句</param>
        /// <param name="dataname">返回的DataSet中DataTable的名字</param>
        /// <param name="commandParameters">参数列表</param>
        /// <returns>返回DataSet</returns>
        public static DataTable ExecuteDataTable(string connectionString, CommandType cmdType, string cmdText, string dataname, params SQLiteParameter[] commandParameters)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            SQLiteConnection conn = new SQLiteConnection(connectionString);
            //SqlDataReader rdr = null;

            /**********
            在这里使用try/catch处理是因为如果方法出现异常，则SqlDataReader就不存在，
            CommandBehavior.CloseConnection的语句就不会执行，触发的异常由catch捕获。
            关闭数据库连接，并通过throw再次引发捕捉到的异常。
            **********/
            try
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                SQLiteDataAdapter adp = new SQLiteDataAdapter();
                cmd.Connection = conn;
                adp.SelectCommand = cmd;
                DataSet ds = new DataSet();
                adp.Fill(ds, dataname);

                cmd.Parameters.Clear();
                if (ds != null && ds.Tables.Count > 0)
                {
                    return ds.Tables[0];
                }
                return null;
            }
            /*
            catch (SQLiteException dbEx)
            {
                conn.Close();
                throw dbEx;
            }
            */
            catch (Exception ex)
            {
                conn.Close();
                //数据库错误多为致命错误，所以暂时全部上抛处理.
                throw ex;
            }
        }

        /// <summary>
        /// 执行一条返回结果集的SQLiteCommand命令，通过专用的连接字符串
        /// 使用参数数组提供参数
        /// </summary>
        /// <remarks>
        /// 使用示例：  
        ///  SQLiteDataReader r = ExecuteReader(connString, CommandType.StoredProcedure, "PublishOrders", new SQLiteParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">一个有效的数据库连接字符串</param>
        /// <param name="cmdType">SQLiteCommand命令类型(存储过程<StoredProcedure>，表的名称<TableDirect>， T-SQL 文本命令语句<Text>(默认)， 等等。)</param>
        /// <param name="cmdText">存储过程的名字或者表的名字或者 T-SQL 文本命令语句</param>
        /// <param name="commandParameters">以数组形式提供SQLiteCommand命令中用到的参数列表</param>
        /// <returns>返回一个包含结果的SQLiteDataReader</returns>
        public static SQLiteDataReader ExecuteReader(string connectionString, CommandType cmdType, string cmdText, params SQLiteParameter[] commandParameters)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            SQLiteConnection conn = new SQLiteConnection(connectionString);
            SQLiteDataReader rdr = null;

            /**********
            在这里使用try/catch处理是因为如果方法出现异常，则SQLiteDataReader就不存在，
            CommandBehavior.CloseConnection的语句就不会执行，触发的异常由catch捕获。
            关闭数据库连接，并通过throw再次引发捕捉到的异常。
            **********/
            try
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return rdr;
            }           
            catch (Exception ex)
            {
                conn.Close();
                //数据库错误多为致命错误，所以暂时全部上抛处理.
                throw ex;
            }
        }


        /// <summary>
        /// 执行一条返回第一条记录第一列的SQLiteCommand命令，通过专用的连接字符串
        /// 使用参数数组提供参数
        /// </summary>
        /// <remarks>
        /// 使用示例：  
        ///  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new SQLiteParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">一个有效的数据库连接字符串</param>
        /// <param name="cmdType">SQLiteCommand命令类型 (存储过程<StoredProcedure>，表的名称<TableDirect>， T-SQL 文本命令语句<Text>(默认)， 等等。)</param>
        /// <param name="cmdText">存储过程的名字或者表的名字或者 T-SQL 文本命令语句</param>
        /// <param name="commandParameters">以数组形式提供SQLiteCommand命令中用到的参数列表</param>
        /// <returns>返回一个object类型的数据，可以通过 Convert.To{Type}方法转换类型</returns>
        public static object ExecuteScalar(string connectionString, CommandType cmdType, string cmdText, params SQLiteParameter[] commandParameters)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
                    object val = cmd.ExecuteScalar();
                    cmd.Parameters.Clear();
                    return val;
                }
            }
            catch (Exception ex)
            {
                //数据库错误多为致命错误，所以暂时全部上抛处理.
                throw ex;
            }
        }


        /// <summary>
        /// 执行一条返回第一条记录第一列的SQLiteCommand命令，通过已经存在的数据库连接
        /// 使用参数数组提供参数
        /// </summary>
        /// <remarks>
        /// 使用示例： 
        ///  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new SQLiteParameter("@prodid", 24));
        /// </remarks>
        /// <param name="conn">一个已经存在的数据库连接</param>
        /// <param name="commandType">SQLiteCommand命令类型 (存储过程<StoredProcedure>，表的名称<TableDirect>， T-SQL 文本命令语句<Text>(默认)， 等等。)</param>
        /// <param name="commandText">存储过程的名字或者表的名字或者 T-SQL 文本命令语句</param>
        /// <param name="commandParameters">以数组形式提供SQLiteCommand命令中用到的参数列表</param>
        /// <returns>返回一个object类型的数据，可以通过 Convert.To{Type}方法转换类型</returns>
        public static object ExecuteScalar(SQLiteConnection connection, CommandType cmdType, string cmdText, params SQLiteParameter[] commandParameters)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            try
            {
                PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
                object val = cmd.ExecuteScalar();
                cmd.Parameters.Clear();
                return val;
            }
            catch (Exception ex)
            {
                //数据库错误多为致命错误，所以暂时全部上抛处理.
                throw ex;
            }
        }


        /// <summary>
        /// 缓存参数数组
        /// </summary>
        /// <param name="cacheKey">参数缓存的键值</param>
        /// <param name="commandParameters">被缓存的参数列表</param>
        public static void CacheParameters(string cacheKey, params SQLiteParameter[] commandParameters)
        {
            parmCache[cacheKey] = commandParameters;
        }


        /// <summary>
        /// 获取被缓存的参数
        /// </summary>
        /// <param name="cacheKey">用于查找参数的KEY值</param>
        /// <returns>返回缓存的参数数组</returns>
        public static SQLiteParameter[] GetCachedParameters(string cacheKey)
        {
            SQLiteParameter[] cachedParms = (SQLiteParameter[])parmCache[cacheKey];

            if (cachedParms == null)
                return null;

            //新建一个参数的克隆列表
            SQLiteParameter[] clonedParms = new SQLiteParameter[cachedParms.Length];

            //通过循环为克隆参数列表赋值
            for (int i = 0, j = cachedParms.Length; i < j; i++)
                //使用clone方法复制参数列表中的参数
                clonedParms[i] = (SQLiteParameter)((ICloneable)cachedParms[i]).Clone();

            return clonedParms;
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
}
