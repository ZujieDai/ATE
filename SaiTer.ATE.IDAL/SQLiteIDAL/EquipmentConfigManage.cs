using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.IDAL.DBUtility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SaiTer.ATE.DataModel.DataBaseModel;
using NPOI.SS.Util;
using System.Data.SQLite;

namespace SaiTer.ATE.IDAL.SQLiteIDAL
{
    /// <summary>
    /// 设备配置表
    /// </summary>
    public class EquipmentConfigManage
    {
        /// <summary>
        /// 获取设备配置参数信息
        /// </summary>       
        /// <returns></returns>
        public static List<EquipmentConfigModel> GetEquipConfigs()
        {
            bool tableExists = CheckIfTableExists(SQLiteHelper.DbConnString, "EquipmentConfig");
            if (!tableExists)
            {
                return null;
            }
            List<EquipmentConfigModel> lstConfig = new List<EquipmentConfigModel>();
            string strSQL = string.Format("SELECT CHARGERTYPE, CONFIGTYPE,EQUIPNAME,PARAMS1,PARAMS2,PARAMS3,REMARK FROM EquipmentConfig");

            DataTable dt = SQLiteHelper.ExecuteDataTable(SQLiteHelper.DbConnString, CommandType.Text, strSQL, "EquipmentConfig", null);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    EquipmentConfigModel ConfigModel = new EquipmentConfigModel();
                    ConfigModel.ChargerType = DBConvert.ToInt32(dr["CHARGERTYPE"]);
                    ConfigModel.ConfigType = DBConvert.ToString(dr["CONFIGTYPE"]);
                    ConfigModel.EquipmentName = DBConvert.ToString(dr["EQUIPNAME"]);
                    ConfigModel.Params1 = DBConvert.ToString(dr["PARAMS1"]);
                    ConfigModel.Params2 = DBConvert.ToString(dr["PARAMS2"]);
                    ConfigModel.Params3 = DBConvert.ToString(dr["PARAMS3"]);
                    ConfigModel.Remark = DBConvert.ToString(dr["REMARK"]);

                    lstConfig.Add(ConfigModel);
                }
                return lstConfig;
            }
            else
                return null;
        }
        public static bool InsertEquipConfigs(EquipmentConfigModel config)
        {

            //先删除历史数据再插入
            string strDeleteSQL = string.Format($"DELETE FROM EquipmentConfig WHERE CHARGERTYPE = {config.ChargerType} AND CONFIGTYPE = '{config.ConfigType}' AND EQUIPNAME = '{config.EquipmentName}'");
            SQLiteHelper.ExecuteNonQuery(SQLiteHelper.DbConnString, CommandType.Text, strDeleteSQL, null);

            string strSQL = string.Format("INSERT INTO  EquipmentConfig (ChargerType,ConfigType,EquipName,Params1,Params2,Params3,Remark)  " +
                "VALUES ({0},'{1}','{2}','{3}','{4}','{5}','{6}')",
                config.ChargerType, config.ConfigType, config.EquipmentName, config.Params1, config.Params2, config.Params3, config.Remark);

            if (SQLiteHelper.ExecuteNonQuery(SQLiteHelper.DbConnString, CommandType.Text, strSQL, null) == 0)
                return false;

            return true;

        }
        private static bool CheckIfTableExists(string databasePath, string tableName)
        {
            using (SQLiteConnection connection = new SQLiteConnection(databasePath))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand($"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}'", connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        return reader.HasRows;
                    }
                }
            }
        }

        public static string[] GetConfigParams(int chargerType, string configType, string equipmentName, string reMark)
        {
            try
            {
                List<EquipmentConfigModel> lstConfig = EquipmentConfigManage.GetEquipConfigs();
                lstConfig = lstConfig?.FindAll(s => s.ChargerType == chargerType &&
                    s.ConfigType.ToLower().Equals(configType.ToLower()) && s.EquipmentName.ToLower().Equals(equipmentName.ToLower()));
                if (lstConfig == null)
                {
                    return null;
                }
                var model = lstConfig.First(c => c.Params3.Contains(reMark)); 
                List<string> list = new List<string>()
                {
                    model.Params1,
                    model.Params2,
                    model.Params3
                };
                return list.ToArray();
                //foreach (var model in lstConfig)
                //{
                //    // 注意：如果CP关键字检索出现多余的触发配置，可以把条件改为reMark == "CP" || reMark == "CP_Voltage"
                //    // 通道3通过程控板开关控制是CP信号或者输出电流
                //    if (reMark.Contains("CP"))
                //    {
                //        // 如果是CP信号必须为CP的配置
                //        if (!model.Remark.Contains("CP"))
                //            continue;
                //    }
                //    else
                //    {
                //        // 否则不能是CP信号的配置
                //        if (model.Remark.Contains("CP"))
                //            continue;
                //    }
                //    List<string> list = new List<string>()
                //    {
                //        model.Params1,
                //        model.Params2,
                //        model.Params3
                //    };
                //    return list.ToArray();
                //}
                //return null;
            }
            catch
            {
                return null;
            }
        }

        public static List<string[]> GetConfigParams(int chargerType, string configType, string equipmentName)
        {
            try
            {
                List<string[]> res = new List<string[]>();
                List<EquipmentConfigModel> lstConfig = EquipmentConfigManage.GetEquipConfigs();
                lstConfig = lstConfig?.FindAll(s => s.ChargerType == chargerType &&
                    s.ConfigType.ToLower().Equals(configType.ToLower()) && s.EquipmentName.ToLower().Equals(equipmentName.ToLower()));
                if (lstConfig == null)
                {
                    return null;
                }
                foreach (var model in lstConfig)
                {
                    List<string> list = new List<string>()
                    {
                        model.Params1,
                        model.Params2,
                        model.Params3
                    };
                    res.Add(list.ToArray());
                }
                return res;
            }
            catch
            {
                return null;
            }
        }
    }
}
