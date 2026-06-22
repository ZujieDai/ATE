using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.DataBaseModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.IDAL.DBUtility;

namespace SaiTer.ATE.IDAL.SQLiteIDAL
{
    public class EquipMentCMDManage
    {
        /// <summary>
        /// 查询所有的命令
        /// </summary>
        /// <param name="lstEquipMentCMD"></param>
        /// <returns></returns>
        public static bool SelectEquipMentCMD_ALL(out List<EquipMentCMD> lstEquipMentCMD)
        {
            lstEquipMentCMD = new List<EquipMentCMD>();
            string strSQL = string.Format("SELECT EquipMentCMDID, EquipMentCMDName,EquipMentType,EquipMentModel,CMDType,CMDID_Can,CMDContent,CMD_Description , CreatTime FROM EquipMentCMD");

            DataTable dt = SQLiteHelper.ExecuteDataTable(SQLiteHelper.DbConnString, CommandType.Text, strSQL, "EquipMentCMD", null);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    EquipMentCMD EMCMD = new EquipMentCMD();
                    EMCMD.EquipMentCMDID = DBConvert.ToInt32(dr["EquipMentCMDID"]);
                    EMCMD.EquipMentCMDName = DBConvert.ToString(dr["EquipMentCMDName"]);
                    EMCMD.EquipMentType = DBConvert.ToInt32(dr["EquipMentType"]);
                    EMCMD.EquipMentModel = DBConvert.ToString(dr["EquipMentModel"]);
                    EMCMD.CMDType = DBConvert.ToInt32(dr["CMDType"]);
                    EMCMD.CMDID_Can = DBConvert.ToString(dr["CMDID_Can"]);
                    EMCMD.CMDContent = DBConvert.ToString(dr["CMDContent"]);
                    EMCMD.CMD_Description = DBConvert.ToString(dr["CMD_Description"]);
                    EMCMD.CreatTime = DBConvert.ToString(dr["CreatTime"]);

                    lstEquipMentCMD.Add(EMCMD);
                }
                return true;
            }
            else
                return true;
        }

        /// <summary>
        /// 查询对应的设备类型和型号的命令
        /// </summary>
        /// <param name="iType">设备类型</param>
        /// <param name="sModel">设备型号</param>
        /// <param name="lstEquipMentCMD"></param>
        /// <returns></returns>
        public static bool SelectEquipMentCMD_EquipMentType_Model(int iType,string sModel,out List<EquipMentCMD> lstEquipMentCMD)
        {
            lstEquipMentCMD = new List<EquipMentCMD>();
            string strSQL = string.Format("SELECT EquipMentCMDID, EquipMentCMDName,EquipMentType,EquipMentModel,CMDType,CMDID_Can,CMDContent,CMD_Description , CreatTime FROM EquipMentCMD WHERE EquipMentType = {0:D} AND EquipMentModel = '{1:S}' ", iType, sModel);

            DataTable dt = SQLiteHelper.ExecuteDataTable(SQLiteHelper.DbConnString, CommandType.Text, strSQL, "EquipMentCMD", null);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    EquipMentCMD EMCMD = new EquipMentCMD();
                    EMCMD.EquipMentCMDID = DBConvert.ToInt32(dr["EquipMentCMDID"]);
                    EMCMD.EquipMentCMDName = DBConvert.ToString(dr["EquipMentCMDName"]);
                    EMCMD.EquipMentType = DBConvert.ToInt32(dr["EquipMentType"]);
                    EMCMD.EquipMentModel = DBConvert.ToString(dr["EquipMentModel"]);
                    EMCMD.CMDType = DBConvert.ToInt32(dr["CMDType"]);
                    EMCMD.CMDID_Can = DBConvert.ToString(dr["CMDID_Can"]);
                    EMCMD.CMDContent = DBConvert.ToString(dr["CMDContent"]);
                    EMCMD.CMD_Description = DBConvert.ToString(dr["CMD_Description"]);
                    EMCMD.CreatTime = DBConvert.ToString(dr["CreatTime"]);

                    lstEquipMentCMD.Add(EMCMD);
                }
                return true;
            }
            else
                return true;
        }

        /// <summary>
        /// 删除对应的命令
        /// </summary>
        /// <param name="ECMD"></param>
        /// <returns></returns>
        public static bool DeleteEquipMentCMD(EquipMentCMD ECMD)
        {
            string strDeleteSQL = string.Format("DELETE FROM EquipMentCMD WHERE EquipMentType = {0:D} AND EquipMentModel = '{1:S}' ",
                                     (int)ECMD.EquipMentType, ECMD.EquipMentModel);
            if (SQLiteHelper.ExecuteNonQuery(SQLiteHelper.DbConnString, CommandType.Text, strDeleteSQL, null) == 0)
                return false;

            return true;
        }

        /// <summary>
        /// 增加命令
        /// </summary>
        /// <param name="lstEquipMentCMD"></param>
        /// <param name="isDel">是否需要删除</param>
        /// <returns></returns>
        public static bool InsertEquipMentCMDs(List<EquipMentCMD> lstEquipMentCMD, bool isDel = true)
        {
            if (lstEquipMentCMD == null || lstEquipMentCMD.Count == 0)
                return false;
            foreach (EquipMentCMD ECMD in lstEquipMentCMD)
            {
                //先删除历史数据再插入
                if (isDel)
                {
                    DeleteEquipMentCMD(ECMD);
                }
                string strSQL = string.Format("INSERT INTO EquipMentCMD  VALUES ({0:D},'{1:S}',{2:D},'{3:S}',{4:D},'{5:S}','{6:S}','{7:D}' ,'{8:S}')",
                   ECMD.EquipMentCMDID, ECMD.EquipMentCMDName, ECMD.EquipMentType, ECMD.EquipMentModel, ECMD.CMDType, ECMD.CMDID_Can, ECMD.CMDContent, ECMD.CMD_Description, ECMD.CreatTime);

                if (SQLiteHelper.ExecuteNonQuery(SQLiteHelper.DbConnString, CommandType.Text, strSQL, null) == 0)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 获取所有型号
        /// </summary>
        /// <param name="Models"></param>
        /// <returns></returns>
        public static bool  GetEquipMentModel(out List<string> Models)
        {
            Models = new List<string>();
            string strSQL = string.Format("SELECT DISTINCT EquipMentModel FROM EquipMentCMD");

            DataTable dt = SQLiteHelper.ExecuteDataTable(SQLiteHelper.DbConnString, CommandType.Text, strSQL, "EquipMentCMD", null);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    Models.Add(DBConvert.ToString(dr["EquipMentModel"]));
                }
                return true;
            }
            else
                return true;
        }




    }
}
