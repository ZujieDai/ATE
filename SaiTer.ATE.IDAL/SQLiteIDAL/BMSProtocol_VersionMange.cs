using SaiTer.ATE.DataModel.DataBaseModel;
using SaiTer.ATE.DataModel.EnumModel;
using System;


namespace SaiTer.ATE.IDAL.SQLiteIDAL
{
    public class BMSProtocol_VersionMange
    {
        /// <summary>
        /// 修改版本号
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public bool InsertVersion(ESGBDC_Ver version)
        {
            try
            {
                //先删除再增加
                EquipmentConfigModel equipmentConfigManage = new EquipmentConfigModel()
                {
                    ChargerType = 1,
                    ConfigType = "GB_BMS",
                    EquipmentName = "Protocol_Version",
                    Params1 = version.ToString(),
                    Remark = "选定CAN报文版本"
                };
                return EquipmentConfigManage.InsertEquipConfigs(equipmentConfigManage);
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                return false;
                //throw ex;
            }
            
        }

        public ESGBDC_Ver SelectVersion()
        {
            //先删除再增加
            EquipmentConfigModel equipmentConfigManage = new EquipmentConfigModel()
            {
                ChargerType = 1,
                ConfigType = "GB_BMS",
                EquipmentName = "Protocol_Version",
                Remark = "选定CAN报文版本"
            };
           var version= EquipmentConfigManage.GetEquipConfigs()?.Find(x=>x.EquipmentName == equipmentConfigManage.EquipmentName);
            if (version != null)
            {
                try
                {
                    return  (ESGBDC_Ver)Enum.Parse(typeof(ESGBDC_Ver), version.Params1);
                }
                catch (ArgumentException ex)
                {
                 
                    throw ex;
                }
              
            }
            else
            {
                InsertVersion(ESGBDC_Ver.GBDC_2023);
                return ESGBDC_Ver.GBDC_2023;
            }
        }
    }
}
