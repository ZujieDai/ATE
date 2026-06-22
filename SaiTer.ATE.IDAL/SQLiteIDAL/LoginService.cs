using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.IDAL.SQLiteIDAL
{
    public class LoginService
    {
        private static string LoginDName = "SQLiteSaiTerNew.db";
        private static UserInfoManage userInfoManage = new UserInfoManage();
        public static bool Login(string uname, string upassword, ref string UserType)
        {
            string[] UserInfo = ConfigurationManager.AppSettings["UserInfo"].Split('|');
            ///
            if (uname == UserInfo[0] && upassword == UserInfo[1])
            {
                UserType = "超级管理员";
                return true;
            }

            UserInfoModel ui = new UserInfoModel();
            ui.UserName = uname;
            ui.Password = upassword;
            bool ret = userInfoManage.Login(LoginDName, ref ui);
            UserType = ui.UserType;
            return ret;
        }

        public static DataTable GetAllUser()
        {
            DataTable dtuser = null;
            dtuser = userInfoManage.QueryUserInfo(LoginDName);

            return dtuser;
        }

        /// <summary>
        /// 检查重复性（不重复返回true）
        /// </summary>
        /// <param name="user"></param>
        /// <param name="sErr"></param>
        /// <returns></returns>
        public static bool QueryRepeat(UserInfoModel user, ref string sErr)
        {
            bool ret = true;
            int num = 0;
            num = userInfoManage.QueryUserRecord(LoginDName, "UserID", user.UserID);
            if (num > 0)
            {
                sErr += "编号重复\r\n";
                ret &= false;
            }
            num = userInfoManage.QueryUserRecord(LoginDName, "UserName", user.UserName);
            if (num > 0)
            {
                sErr += "账号重复\r\n";
                ret &= false;
            }

            return ret;
        }

        public static bool InsertUser(UserInfoModel user)
        {
            bool ret = true;
            ret = userInfoManage.InsertUser(LoginDName, user);

            return ret;
        }

        public static int GetUserPKID()
        {
            int iPKID = 10000;
            try
            {
                iPKID = int.Parse(userInfoManage.GetMaxPKID(LoginDName));
            }
            catch
            {
                iPKID = 10000;
            }
            iPKID++;

            return iPKID;
        }

        public static bool DelUser(UserInfoModel user)
        {
            bool ret = userInfoManage.DelUser(LoginDName, user);

            return ret;
        }

        public static bool UpdateUser(UserInfoModel user)
        {
            bool ret = userInfoManage.UpdateUser(LoginDName, user);
            return ret;
        }
    }
}
