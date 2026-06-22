
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace SaiTer.ATE.IDAL
{

    public  partial class CsharpSQLiteHelper
    {       
        public static  SQLiteConnection SQLiteConn = null;
        public static string SQLiteFilePath = System.AppDomain.CurrentDomain.BaseDirectory + "SQLiteSaiTerNew" + ".db";



        //3---打开数据库
        public static void OpenDB()
        {
            try
            {
                SQLiteConn = new SQLiteConnection("data source=" + SQLiteFilePath);
                SQLiteConn.Open();

            }
            catch (Exception ex)
            {
                // throw new Exception("打开数据库：" + FilePath + "的连接失败：" + ex.Message);
            }

        }
        //关闭数据库
        public static void CloseDB()
        {
            try
            {
                if (SQLiteConn!=null)
                {
                    SQLiteConn.Close();//关闭
                }
               

            }
            catch (Exception ex)
            {
                // throw new Exception("打开数据库：" + FilePath + "的连接失败：" + ex.Message);
            }

        }

        //3---创建数据库
        public  static void CreateDB()
        {
           
            SQLiteConn = new SQLiteConnection("data source=" + SQLiteFilePath);
            SQLiteConn.Open();//打开数据库，若文件不存在会自动创建  
            SQLiteConn.Close();
        }

        //4---删除数据库
        public static void DeleteDB(string path)
        {
            //string path = @"d:\test\123.sqlite";
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }
    

        //6---删除表
        public static void DeleteTable(string  path)
        {
            //string path = @"d:\test\123.sqlite";
            SQLiteConnection cn = new SQLiteConnection("data source=" + path);
            if (SQLiteConn.State != System.Data.ConnectionState.Open)
            {
                SQLiteConn.Open();
                SQLiteCommand cmd = new SQLiteCommand();
                cmd.Connection = SQLiteConn;
                //删除名字叫TestTable的表格
                cmd.CommandText = "DROP TABLE IF EXISTS TestTable";
                cmd.ExecuteNonQuery();
            }
            SQLiteConn.Close();
        }
        //7-2---遍历查询一个表结构
        public static void QueryOneTableInfo()
        {
            //string path = @"d:\test\123.sqlite";
            //SQLiteConnection cn = new SQLiteConnection("data source=" + SQLiteFilePath);
            if (SQLiteConn.State != System.Data.ConnectionState.Open)
            {
                SQLiteConn.Open();
               
            }
            SQLiteCommand cmd = SQLiteConn.CreateCommand();
            //遍历名字叫TestTable的表格
            cmd.CommandText = "PRAGMA table_info('TestTable')";
            //写法二：用DataReader，这个效率高些
            SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    Console.Write($"{reader[i]},");
                }
                Console.WriteLine();
            }
            reader.Close();

        }
        //8---更改表名
        public static void ChangeTableName()
        {
            //SQLiteConnection cn = new SQLiteConnection("data source=" + SQLiteFilePath);
            //cn.Open();
            if (SQLiteConn.State != System.Data.ConnectionState.Open)
            {
                SQLiteConn.Open();

            }
            SQLiteCommand cmd = SQLiteConn.CreateCommand();
            cmd.CommandText = "ALTER TABLE TestTable RENAME TO TestTable2";
            cmd.ExecuteNonQuery();
        }

        public static void DeleteDataToTable()
        {
            //SQLiteConnection cn = new SQLiteConnection("data source=" + SQLiteFilePath);
            //cn.Open();
            if (SQLiteConn.State != System.Data.ConnectionState.Open)
            {
                SQLiteConn.Open();

            }
            SQLiteCommand cmd = SQLiteConn.CreateCommand();
            cmd.CommandText = "DELETE FROM TestTable WHERE id='19'";
            cmd.ExecuteNonQuery();
        }


        // 检查数据表是否存在，不存在创建        
        public static bool CheckDataTable(Enum tEnum,string tSQLiteFilePath)
        {
            try
            {

                    if (SQLiteConn == null)
                    {
                        SQLiteConn = new SQLiteConnection("data source=" + tSQLiteFilePath);
                    }
                    if (SQLiteConn.State != System.Data.ConnectionState.Open)
                    {
                        SQLiteConn.Open();//打开数据库，若文件不存在会自动创建  
                    }

                    SQLiteCommand cmd = SQLiteConn.CreateCommand();
                    cmd.CommandText = "SELECT count(*) FROM sqlite_master WHERE type = 'table' AND name = '"+ tEnum.ToString()+"TestTable'";//
                    object ob = cmd.ExecuteScalar();
                    long tableCount = Convert.ToInt64(ob);
                     cmd.ExecuteNonQuery();
                    SQLiteConn.Close();
                    if (tableCount == 0)
                    {
                         //数据表不存在
                         //此语句返回结果为0
                        return false;
                    }
                    else
                    {
                        return true;
                    }

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static void CreateTable(Enum tEnum)
        {
            //string path = @"d:\test\123.sqlite";
            //SQLiteConnection cn = new SQLiteConnection("data source=" + SQLiteFilePath);
            if (SQLiteConn.State != System.Data.ConnectionState.Open)
            {
                SQLiteConn.Open();
            }
            SQLiteCommand cmd = new SQLiteCommand();
            cmd.Connection = SQLiteConn;
            //创建名字叫TestTable的表格
            //cmd.CommandText = "CREATE TABLE "+tEnum.ToString()+ "TestTable(TestTime varchar(20),TestProjectTime varchar(20),TestName varchar(20),TestResult varchar(20),TestStatus varchar(20),TestData0 varchar(20),TestData1 varchar(20),TestData2 varchar(20)" +
            //  ",TestData3 varchar(20),TestData4 varchar(20),TestData5 varchar(20),TestData6 varchar(20),TestData7 varchar(20),TestData8 varchar(20),TestData9 varchar(20),TestData10 varchar(20),TestData11 varchar(20))";



            cmd.CommandText = "CREATE TABLE " + tEnum.ToString() + "TestTable(TestTime varchar(20),TestProjectTime varchar(20),TestName varchar(20),TestResult varchar(20),TestStatus varchar(20),TestData0 varchar(20),TestData1 varchar(20),TestData2 varchar(20)" +
                              ",TestData3 varchar(20),TestData4 varchar(20),TestData5 varchar(20),TestData6 varchar(20),TestData7 varchar(20),TestData8 varchar(20),TestData9 varchar(20),TestData10 varchar(20),TestData11 varchar(20)" +
                              ",TestDataColour0 varchar(20),TestDataColour1 varchar(20),TestDataColour2 varchar(20),TestDataColour3 varchar(20),TestDataColour4 varchar(20),TestDataColour5 varchar(20),TestDataColour6 varchar(20),TestDataColour7 varchar(20)" +
                              ",TestDataColour8 varchar(20),TestDataColour9 varchar(20),TestDataColour10 varchar(20),TestDataColour11 varchar(20))";

            cmd.ExecuteNonQuery();

            SQLiteConn.Close();
        }

        public static void InsertDataToTable(Enum tEnum, string tTestTime, string tTestProjectTime, string tTestName, string tTestResult, string tTestStatus, List<string> tLstData, List<bool> tLstColour)
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

            //cmd.CommandText = "SELECT * FROM " + tEnum.ToString() + "TestTable WHERE TestTime=" + "'" + tTestTime.ToString() + "'" + " AND TestName=" + "'" + tTestName.ToString() + "'";
            //SQLiteDataReader sr = cmd.ExecuteReader();
            //if (sr.Read())
            //{
            //    sr.Close();
            //    cmd.CommandText = "delete from " + tEnum.ToString() + "TestTable where TestTime=" + "'" + tTestTime.ToString() + "'" + " AND TestName=" + "'" + tTestName.ToString() + "'";

            //    cmd.ExecuteNonQuery();

            //}
            //else
            //{
            //    sr.Close();
            //}


            //cmd.CommandText = "INSERT OR REPLACE INTO " + tEnum.ToString() + "TestTable(TestTime,TestProjectTime,TestName,TestResult,TestStatus,TestData0," +
            //       "TestData1,TestData2,TestData3,TestData4,TestData5,TestData6,TestData7,TestData8,TestData9,TestData10,TestData11)VALUES" +
            //        "(@TestTime,@TestProjectTime,@TestName,@TestResult,@TestStatus," +
            //        "@TestData0,@TestData1,@TestData2,@TestData3,@TestData4,@TestData5,@TestData6,@TestData7,@TestData8,@TestData9,@TestData10,@TestData11)";


                cmd.CommandText = "INSERT OR REPLACE INTO " + tEnum.ToString() + "TestTable(TestTime,TestProjectTime,TestName,TestResult,TestStatus," +
            "TestData0,TestData1,TestData2,TestData3,TestData4,TestData5,TestData6,TestData7,TestData8,TestData9,TestData10,TestData11,TestDataColour0,TestDataColour1," +
            "TestDataColour2,TestDataColour3,TestDataColour4,TestDataColour5,TestDataColour6,TestDataColour7,TestDataColour8,TestDataColour9,TestDataColour10,TestDataColour11)VALUES" +
            "(@TestTime,@TestProjectTime,@TestName,@TestResult,@TestStatus,@TestData0,@TestData1,@TestData2,@TestData3,@TestData4,@TestData5,@TestData6,@TestData7,@TestData8,@TestData9,@TestData10,@TestData11," +
            "@TestDataColour0,@TestDataColour1,@TestDataColour2,@TestDataColour3,@TestDataColour4,@TestDataColour5,@TestDataColour6,@TestDataColour7," +
            "@TestDataColour8,@TestDataColour9,@TestDataColour10,@TestDataColour11)";

            cmd.Parameters.Add("TestTime", DbType.String).Value = tTestTime;//
            cmd.Parameters.Add("TestProjectTime", DbType.String).Value = tTestProjectTime;//
            cmd.Parameters.Add("TestName", DbType.String).Value = tTestName;// 
            cmd.Parameters.Add("TestResult", DbType.String).Value = tTestResult;//
            cmd.Parameters.Add("TestStatus", DbType.String).Value = tTestStatus;//

            if (tLstData != null)
            {
                for (int i = 0; i < tLstData.Count; i++)
                {
                    cmd.Parameters.Add("TestData" + i.ToString(), DbType.String).Value = tLstData[i];//   
                }
                for (int i = tLstData.Count; i < 12; i++)
                {
                    cmd.Parameters.Add("TestData" + i.ToString(), DbType.String).Value = "";//   

                }
            }
            else
            {
                for (int i = 0; i < 12; i++)//
                {
                    cmd.Parameters.Add("TestData" + i.ToString(), DbType.String).Value = "";//   
                }

            }

            //////////////////////////////////////

            if (tLstColour != null)
            {
                for (int i = 0; i < tLstColour.Count; i++)
                {
                    cmd.Parameters.Add("TestDataColour" + i.ToString(), DbType.String).Value = tLstColour[i];//   
                }
                for (int i = tLstData.Count; i < 12; i++)
                {
                    cmd.Parameters.Add("TestDataColour" + i.ToString(), DbType.String).Value = true;//   

                }
            }
            else
            {
                for (int i = 0; i < 12; i++)//12
                {
                    cmd.Parameters.Add("TestDataColour" + i.ToString(), DbType.String).Value = true;//   
                }

            }
            



            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 执行一条返回结果集的OleDbCommand命令，通过专用的连接字符串
        /// 使用参数数组提供参数
        /// </summary>
        /// <remarks>
        /// 使用示例：  
        ///  OleDbDataReader r = ExecuteReader(connString, CommandType.StoredProcedure, "PublishOrders", new OleDbParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">一个有效的数据库连接字符串</param>
        /// <param name="cmdType">OleDbCommand命令类型(存储过程<StoredProcedure>，表的名称<TableDirect>， T-SQL 文本命令语句<Text>(默认)， 等等。)</param>
        /// <param name="cmdText">存储过程的名字或者表的名字或者 T-SQL 文本命令语句</param>
        /// <param name="commandParameters">以数组形式提供OleDbCommand命令中用到的参数列表</param>
        /// <returns>返回一个包含结果的OleDbDataReader</returns>
        public static SQLiteDataReader ExecuteReader(string connectionString, CommandType cmdType, string cmdText, params SQLiteParameter[] commandParameters)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            SQLiteConnection conn = new SQLiteConnection(connectionString);
            SQLiteDataReader rdr = null;

            /**********
            在这里使用try/catch处理是因为如果方法出现异常，则OleDbDataReader就不存在，
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
            /*
            catch (OleDbException dbEx)
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
        /// 为执行命令准备参数
        /// </summary>
        /// <param name="cmd">OleDbCommand命令</param>
        /// <param name="conn">已经存在的数据库连接</param>
        /// <param name="trans">数据库事物处理</param>
        /// <param name="cmdType">OleDbCommand命令类型 (存储过程<StoredProcedure>，表的名称<TableDirect>， T-SQL 文本命令语句<Text>(默认)， 等等。)</param>
        /// <param name="cmdText">存储过程的名字或者表的名字或者 T-SQL 文本命令语句</param>
        /// <param name="cmdParms">以数组形式提供OleDbCommand命令中用到的参数列表</param>
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
