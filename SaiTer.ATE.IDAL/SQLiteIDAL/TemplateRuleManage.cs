using SaiTer.ATE.DataModel.DataBaseModel;
using SaiTer.ATE.IDAL.DBUtility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.IDAL.SQLiteIDAL
{
    public class TemplateRuleManage
    {
        /// <summary>
        /// 查询全部TemplateRule配置规则
        /// </summary>
        public static List<TemplateRuleModel> GetAllTemplateRule()
        {
            List<TemplateRuleModel> list = new List<TemplateRuleModel>();
            string sql = @"SELECT TrialType, TrialName, ItamName, DataContent, DataType, Bookmark, other FROM TemplateRule";
            DataTable dt = SQLiteHelper.ExecuteDataTable(SQLiteHelper.DbConnString, CommandType.Text, sql, "TemplateRule", null);
            if (dt == null || dt.Rows.Count == 0)
                return list;

            foreach (DataRow dr in dt.Rows)
            {
                list.Add(new TemplateRuleModel
                {
                    TrialType = DBConvert.ToString(dr["TrialType"]),
                    TrialName = DBConvert.ToString(dr["TrialName"]),
                    ItamName = DBConvert.ToString(dr["ItamName"]),
                    DataContent = DBConvert.ToString(dr["DataContent"]),
                    DataType = DBConvert.ToString(dr["DataType"]).ToLower(),
                    Bookmark = DBConvert.ToString(dr["Bookmark"]),
                    Other = DBConvert.ToString(dr["Other"])
                });
            }
            return list;
        }

    }
}
