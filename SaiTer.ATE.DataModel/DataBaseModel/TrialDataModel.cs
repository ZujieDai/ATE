using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.DataBaseModel
{
    /// <summary>
    /// 检测试验结果数据
    /// </summary>
    public class TrialDataModel
    {
        /// <summary>
        /// 该枪是否要检测
        /// </summary>
        public bool IsCheck = true;

        private string barcode = "";

        private int chargerId = 0;

        private string trialValue = "";

        private string itemName = "";

        private string trialName = "";

        private int schemeID = 0;

        private string schemeName = "";

        private string data1 = "";

        private string data2 = "";

        private string data3 = "";


        private EmTrialResult trialResult = EmTrialResult.Wait;

        private EmTrialType trialType = EmTrialType.Null;

        private string extentData = "";

        private string saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        private int reCheckCount = 1;


        /// <summary>
        /// 充电枪位条码号
        /// </summary>
        public string BarCode
        {
            get
            {
                return barcode;
            }
            set
            {
                barcode = value;
            }
        }


        /// <summary>
        /// 充电枪位编号
        /// </summary>
        public int ChargerId
        {
            get
            {
                return chargerId;
            }
            set
            {
                chargerId = value;
            }
        }

        /// <summary>
        /// 试验名称
        /// (如：稳压精度测试)
        /// </summary>
        public string TrialName
        {
            get { return trialName; }
            set { trialName = value; }
        }

        /// <summary>
        /// 试验项里的检测点名称
        /// (如：稳压精度(电压：500V ，电流20A))
        /// </summary>
        public string ItemName
        {
            get { return itemName; }
            set { itemName = value; }
        }
        /// <summary>
        /// 方案ID
        /// </summary>
        public int SchemeID
        {
            get { return schemeID; }
            set { schemeID = value; }
        }
        /// <summary>
        /// 方案名称
        /// </summary>
        public string SchemeName
        {
            get { return schemeName; }
            set { schemeName = value; }
        }

        /// <summary>
        /// 检测数据1
        /// </summary>
        public string Data1
        {
            get { return data1; }
            set { data1 = value; }
        }
        /// <summary>
        /// 检测数据2
        /// </summary>
        public string Data2
        {
            get { return data2; }
            set { data2 = value; }
        }
        /// <summary>
        /// 检测数据3
        /// </summary>
        public string Data3
        {
            get { return data3; }
            set { data3 = value; }
        }
        /// <summary>
        /// 试验数据
        /// </summary>
        public string TrialValue
        {
            get
            {
                return trialValue;
            }
            set
            {
                trialValue = value;
            }
        }


        /// <summary>
        /// 试验项分项结论
        /// </summary>
        public EmTrialResult TrialResult
        {
            get
            {
                return trialResult;
            }
            set
            {
                trialResult = value;
            }
        }

        /// <summary>
        /// 检测试验项目类型
        /// </summary>
        public EmTrialType TrialType
        {
            get
            {
                return trialType;
            }
            set
            {
                trialType = value;
            }
        }

        /// <summary>
        /// 扩展试验结果数据（详细数据，如需要）
        /// </summary>
        public string ExtentData
        {
            get
            {
                return extentData;
            }
            set
            {
                extentData = value;
            }
        }


        /// <summary>
        /// 检测数据保存日期(格式:yyyy-MM-dd HH:mm:ss)
        /// </summary>
        public string SaveTime
        {
            get
            {
                return saveTime;
            }
            set
            {
                saveTime = value;
            }
        }


        /// <summary>
        /// 复检次数
        /// </summary>
        public int ReCheckCount
        {
            get { return reCheckCount; }
            set { reCheckCount = value; }
        }

        /// <summary>
        /// 方案项参数
        /// </summary>
        public string SchemeItemParam { get; set; }

        /// <summary>
        /// 试验项总结论
        /// </summary>
        public EmTrialResult TrialFinalResult { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        /// 用户设置参数
        /// </summary>
        public string UserSetParams { get; set; }

        /// <summary>
        /// 试验条件
        /// </summary>
        public string TrialCondition { get; set; }
        /// <summary>
        /// 桩的唯一ID
        /// </summary>
        public long PKID { get; set; }

        /// <summary>
        /// 备用1
        /// </summary>
        public string RES1 { get; set; }
        /// <summary>
        /// 备用2
        /// </summary>
        public string RES2 { get; set; }
        /// <summary>
        /// 备用3
        /// </summary>
        public string RES3 { get; set; }
    }
}
