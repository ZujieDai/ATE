using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel
{
    /// <summary>
    /// 充电枪基本信息
    /// </summary>
    [Serializable]
    public class ChargerInfoModel
    {

        private bool _isCheck;
        /// <summary>
        /// 是否需要检测
        /// </summary>
        public bool IsCheck
        {
            get
            {
                return _isCheck;
            }
            set
            {
                _isCheck = value;
            }
        }

        private int _ChargerId = 0;
        /// <summary>
        /// 充电枪ID
        /// </summary>
        public int ChargerId
        {
            get
            {
                return _ChargerId;
            }
            set
            {
                _ChargerId = value;
            }
        }


        private string _BarCode = string.Empty;
        /// <summary>
        /// 充电枪条码
        /// </summary>
        public string BarCode
        {
            get
            {
                return _BarCode;
            }
            set
            {
                _BarCode = value;
            }
        }
        private EmTrialResult _CheckResult = EmTrialResult.Wait;
        /// <summary>
        /// 检测结果
        /// </summary>
        public EmTrialResult CheckResult
        {
            get
            {
                return _CheckResult;
            }
            set
            {
                _CheckResult = value;
            }
        }
        private EmChargerType _ChargerType = EmChargerType.Charger_GB_DC;
        /// <summary>
        /// 充电枪类型
        /// </summary>
        public EmChargerType ChargerType
        {
            get
            {
                return _ChargerType;
            }
            set { _ChargerType = value; }
        }

        /// <summary>
        /// 复检次数
        /// </summary>
        public int ReCheckCount { get; set; }
        /// <summary>
        /// 产品名称
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// 产品类型
        /// </summary>
        public string ProductModel { get; set; }
        /// <summary>
        /// 操作员
        /// </summary>
        public string Operater { get; set; }

        /// <summary>
        /// 审核员
        /// </summary>
        public string Auditor { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreateTime { get; set; }

        /// <summary>
        /// 所用的测试方案名称
        /// </summary>
        public string SchemeName { get; set; }

        /// <summary>
        /// 额定电压
        /// </summary>
        public double NominalVoltage { get; set; }

        /// <summary>
        /// 最小充电电压（直流桩用）
        /// </summary>
        public double MinAllowChargeVoltage { get; set; }

        /// <summary>
        /// 最大输出功率
        /// </summary>
        public double MaxOutputPower { get; set; }

        /// <summary>
        /// 最大允许充电电流
        /// </summary>
        public double MaxAllowChargeCurrent { get; set; }

        /// <summary>
        /// 额定电流
        /// </summary>
        public double NominalCurrent { get; set; }
    
        /// <summary>
        /// 频率
        /// </summary>
        public double Frequency { get; set; }

        /// <summary>
        /// 唯一主键ID
        /// </summary>
        public long PKID { get; set; }

        /// <summary>
        /// 备用字段1
        /// </summary>
        public string RES1 { get; set; }

        /// <summary>
        /// 备用字段2
        /// </summary>
        public string RES2 { get; set; }

        /// <summary>
        /// 备用字段3
        /// </summary>
        public string RES3 { get; set; }

        #region 恒功率段
        /// <summary>
        /// 恒功率高电压段下限
        /// </summary>
        public double CWHightVoltL { get; set; }

        /// <summary>
        /// 恒功率高电压段上限
        /// </summary>
        public double CWHightVoltH { get; set; }

        /// <summary>
        /// 恒功率低电压段下限
        /// </summary>
        public double CWLowerVoltL { get; set; }

        /// <summary>
        /// 恒功率低电压段上限
        /// </summary>
        public double CWLowerVoltH { get; set; }
        #endregion
    }
}
