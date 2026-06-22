using SQLiteSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.IDAL
{
    public class DbContext
    {
        public static SqlSugarClient GetInstance(string DBName)
        {
            string reval = "DataSource="
                + System.AppDomain.CurrentDomain.BaseDirectory
                 + DBName;

            var db = new SqlSugarClient(reval);
            db.IsEnableLogEvent = true;//启用日志事件
            db.LogEventStarting = (sql, par) => { Console.WriteLine(sql + " " + par + "\r\n"); };
            return db;
        }
    }
}
