using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.DataBaseModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.IDAL.DBUtility;
using SQLiteSugar;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.IDAL.SQLiteIDAL
{
    /// <summary>
    /// 用户信息表数据库管理类
    /// </summary>
    public class UserInfoManage
    {
        /// <summary>
        /// 查询表中记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> QueryModel<T>(string dbName) where T : new()
        {
            using (var db = DbContext.GetInstance(dbName))
            {
                List<T> lists = db.Queryable<T>().ToList();
                return lists;
            }
        }

        #region UserInfo表操作
        /// <summary>
        /// 查询所有用户数据
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        public static List<UserInfoModel> QueryAllUserinfo(string dbName)
        {
            List<UserInfoModel> ltmp;
            ltmp = QueryModel<UserInfoModel>(dbName);
            return ltmp;
        }

        /// <summary>
        /// 登录并返回用户信息
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        public bool Login(string dbName, ref UserInfoModel userInfo)
        {
            bool brslt = false;
            try
            {
                using (var db = DbContext.GetInstance(dbName))
                {
                    List<UserInfoModel> lists = db.SqlQuery<UserInfoModel>("select * from UserInfo where UserName=@UserName and Password=@Password",
                            new { UserName = userInfo.UserName, Password = userInfo.Password });
                    if (lists.Count > 0)
                    {
                        userInfo = lists[0];
                        brslt = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex, "数据库操作异常日志");
            }
            return brslt;
        }

        public bool AddUser(string dbName, UserInfoModel userInfo)
        {

            bool brslt = false;
            try
            {
                using (var db = DbContext.GetInstance(dbName))
                {
                    object sobj = db.Insert<UserInfoModel>(userInfo);
                    brslt = (bool)sobj;
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex, "数据库操作异常日志");

            }
            return brslt;
        }

        public bool DelUser(string dbName, UserInfoModel userInfo)
        {
            bool brslt = false;
            try
            {
                using (var db = DbContext.GetInstance(dbName))
                {
                    int i = db.ExecuteCommand("delete from UserInfo where UserName=@UserName;",
                            new { UserName = userInfo.UserName });
                    if (i > 0)
                    {
                        brslt = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex, "数据库操作异常日志");
            }
            return brslt;
        }

        public bool UpdateUser(string dbName, UserInfoModel userInfo)
        {
            try
            {
                using (var db = DbContext.GetInstance(dbName))
                {
                    bool ret = db.Update<UserInfoModel>(new
                    {
                        //PrimarykeyID = userInfo.PrimarykeyID,//这个不用更新
                        UserID = userInfo.UserID,
                        Password = userInfo.Password,
                        Level = userInfo.Level,
                        UserType = userInfo.UserType,
                        Remarks = userInfo.Remarks,
                        CreatTime = userInfo.CreatTime
                    }, it => it.UserName == userInfo.UserName);
                    return ret;
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex, "数据库操作异常日志");

                return false;
            }
        }

        public DataTable QueryUserInfo(string dbName)
        {
            try
            {
                DataTable dt = null;
                string sql = string.Format("Select UserID as {0}, UserName as {1}, Password as {2}, Level as {3}, Remarks as {4}, CreatTime as {5} from UserInfo;",
                    "编号",
                    "账号",
                    "密码",
                    "等级",
                    "备注",
                    "更新时间");
                using (var db = DbContext.GetInstance(dbName))
                {
                    dt = db.GetDataTable(sql);
                }
                return dt;
            }
            catch (Exception ex)
            {

                Log.Log.LogException(ex, "数据库操作异常日志");
                return null;
            }
        }

        public int QueryUserRecord(string dbName, string clname, string clvalue)
        {
            int num = 0;
            try
            {
                DataTable dt = null;
                string sql = string.Format("select count(*) from UserInfo where {1}='{2}';",
                    clname,
                    clname,
                    clvalue);
                using (var db = DbContext.GetInstance(dbName))
                {
                    dt = db.GetDataTable(sql);
                }
                num = int.Parse(dt.Rows[0][0].ToString());
                return num;
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex, "数据库操作异常日志");
                return num;
            }
        }

        public bool InsertUser(string dbName, UserInfoModel user)
        {
            bool ret = true;

            int num = 0;
            try
            {
                string sql = string.Format("insert into UserInfo "
                    //+ "( PrimarykeyID,UserID,UserName,Password,Level,UserType,Remarks,CreatTime ) "
                    + " values({0},'{1}','{2}','{3}',{4},'{5}','{6}','{7}') ;",
                    user.PrimarykeyID,
                    user.UserID,
                    user.UserName,
                    user.Password,
                    user.Level,
                    user.UserType,
                    user.Remarks,
                    user.CreatTime);
                using (var db = DbContext.GetInstance(dbName))
                {
                    num = db.ExecuteCommand(sql);
                }
                ret = num > 0 ? true : false;
                return ret;
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex, "数据库操作异常日志");
                return false;
            }
        }

        public string GetMaxPKID(string dbName)
        {
            string stmp = "10000";//如果无数据就返回10000
            try
            {
                DataTable dt = null;
                string sql = string.Format("select max({0}) from UserInfo;",
                    "PrimarykeyID");
                using (var db = DbContext.GetInstance(dbName))
                {
                    dt = db.GetDataTable(sql);
                }
                stmp = dt.Rows[0][0].ToString();
                return stmp;
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex, "数据库操作异常日志");
                return stmp;
            }
        }

        public static List<UserInfoModel> GetUserInfoModels()
        {
            List<UserInfoModel> lst = new List<UserInfoModel>();
            string strSQL = string.Format("SELECT UserID,UserName,Password,Level,UserType From UserInfo");

            DataTable dt = SQLiteHelper.ExecuteDataTable(SQLiteHelper.DbConnString, CommandType.Text, strSQL, "UserInfo", null);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    UserInfoModel model = new UserInfoModel();
                    model.UserID = DBConvert.ToString(dr["UserID"]);
                    model.UserName = DBConvert.ToString(dr["UserName"]);
                    model.Password = DBConvert.ToString(dr["Password"]);
                    model.Level = DBConvert.ToInt32(dr["Level"]);
                    model.UserType = DBConvert.ToString(dr["UserType"]);

                    //添加试验数据
                    lst.Add(model);
                }
            }

            return lst;
        }


        public static bool InsertUserInfo(List<UserInfoModel> lstUserInfos)
        {
            string strSQL = "Delete From UserInfo ";
            SQLiteHelper.ExecuteNonQuery(SQLiteHelper.DbConnString, CommandType.Text, strSQL, null);

            foreach (UserInfoModel model in lstUserInfos)
            {
                strSQL = string.Format("INSERT INTO UserInfo (UserName, Password , Level, UserType) VALUES ('{0}','{1}',{2},'{3}')", model.UserName, model.Password, model.Level, model.UserType);

                if (SQLiteHelper.ExecuteNonQuery(SQLiteHelper.DbConnString, CommandType.Text, strSQL, null) == 0)
                { return false; }
            }
            return true;
        }
        #endregion
    }
}
