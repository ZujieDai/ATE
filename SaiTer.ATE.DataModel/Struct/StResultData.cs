using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.Struct
{
    /// <summary>
    /// 设备返回数据
    /// </summary>
    [Serializable]
    public struct StResultData
    {

        /// <summary>
        /// 控制的枪位编号 
        /// </summary>
        public int ChargeId;
        /// <summary>
        /// 结果数据列表
        /// </summary>
        public List<object> LstData;

        /// <summary>
        /// 试验项目
        /// </summary>
        public EmTrialType TestItem;

        ///// <summary>
        ///// 构造函数
        ///// </summary>
        //public StResultData()
        //{
        //    LstData = new List<object>();
        //    TestItem = EmTrialType.Null;
        //    ChargeId = 0;
        //}
       
    }
}
