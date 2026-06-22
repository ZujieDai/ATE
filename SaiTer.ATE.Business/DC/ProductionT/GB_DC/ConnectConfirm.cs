using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 连接确认测试   互操作D0.1001
    /// </summary>
    public class ConnectConfirm : BusinessBase
    {
        #region 测试流程
        ////测试编号        D0.1001
        ////测试目的        检查充电机是否能通过测量检测点1的电压值判断车辆插头与车辆插座的连接状态，并进入对应的充电状态；
        ///                 通过测量检测点2的电压值，检查车辆插头内等效电阻R3是否正确。
        ////测试方法及步骤  a）状态0：车辆插头未插入车辆插座时，检查检测点1的电压值和充电状态；
        ////                b）状态1 / 状态2：将车辆插头插入车辆插座中，检查检测点1的电压值和充电状态；
        ////                c）状态3：车辆插头与车辆插座完全连接后，检查检测点1的电压值、检测点2的电压值、充电状态；
        ////                d）检查该阶段车辆接口锁止状态。
        ////测试要求    ——状态0：检测点1电压6±0.8V、检测点2电压12±0.8V、开关S闭合、充电接口断开、电子锁断开、不可充电；
        ////            ——状态1：检测点1电压12±0.8V、检测点2电压12±0.8V、开关S断开、充电接口断开、电子锁断开、不可充电；
        ////            ——状态2：检测点1电压6±0.8V、检测点2电压6±0.8V、开关S断开、充电接口连接中、电子锁断开、不可充电；
        ////            ——状态3：检测点1电压4±0.8V、检测点2电压6±0.8V、开关S闭合、充电接口完全连接、电子锁闭合、可充电。
        #endregion

        public ConnectConfirm(int type)
        {
            TrialType = type;
        }
        public override void ExecuteMethod()
        {
            
        }

        public override void InitEquiMent()
        {
            
        }

        public override void InitializeParams()
        {
            
        }

        public override void ProcessData()
        {
            
        }
    }
}
