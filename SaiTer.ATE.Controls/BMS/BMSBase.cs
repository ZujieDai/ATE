using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.BMS;
using SaiTer.ATE.DataModel.CAN;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.EquipMent;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using static SaiTer.ATE.EquipMent.BMS_Protocol;

namespace SaiTer.ATE.Controls
{
    public abstract class BMSBase : ControlsBase
    {
        /// <summary>
        /// 关闭导引
        /// </summary>
        /// <param name="lstIDs">充电枪id信息集合</param>
        public virtual void BMS_OFF(List<int> lstIDs, string[] classNames = null) { }

        /// <summary>
        /// 启动导引
        /// </summary>
        /// <param name="lstIDs">充电枪id信息集合</param>
        public virtual void BMS_ON(List<int> lstIDs, string[] classNames = null) { }

        /// <summary>
        /// 交流设置R2,R3电阻
        /// </summary>
        /// <param name="lstIDs">充电枪id信息集合</param>
        /// <param name="tR2">R2阻值</param>
        /// <param name="tR3">R3阻值</param>
        public virtual void BMS_GetResistance(List<int> lstIDs, ref UInt16 tR2, ref UInt16 tR3, string[] classNames = null) { }   /////BMS S2开关断开, CC CP PE闭合
        /// <summary>
        /// 交流设置R2,R3电阻
        /// </summary>
        /// <param name="lstIDs">充电枪id信息集合</param>
        /// <param name="tR2">R2阻值</param>
        /// <param name="tR3">R3阻值</param>
        public virtual void BMS_SetResistance(List<int> lstIDs, UInt16 tR2, UInt16 tR3, string[] classNames = null) { }   /////BMS S2开关断开, CC CP PE闭合
        /// <summary>
        /// 设置S2状态
        /// </summary>
        /// <param name="lstIDs">充电枪id信息集合</param>
        /// <param name="bs">FALSE断开或TRUE闭合</param>
        public virtual void BMS_SetS2State(List<int> lstIDs, bool bs, string[] classNames = null) { }   /////BMS S2开关闭合, CC CP PE闭合   电子锁控制解锁
        /// <summary>
        /// 三标导引切换
        /// </summary>
        /// <param name="lstIDs">充电枪id信息集合</param>
        /// <param name="type">充电桩类型</param>
        public virtual void BMSSetHCAC(List<int> lstIDs, EmChargerType type, string[] classNames = null) { }
        /// <summary>
        /// </summary>
        /// 交流设置互操作开关状态
        /// <param name="lstIDs">充电枪id信息集合</param>
        /// <param name="bs">开关状态集合（16个）</param>
        public virtual void BMS_SetKState(List<int> lstIDs, List<bool> bs, string[] classNames = null) { }

        /// <summary>
        /// 设置导引电能表常数和工作误差校验圈数
        /// </summary>
        /// <param name="lstIDs">充电枪id信息集合</param>
        /// <param name="ElecConstant">常数</param>
        /// <param name="InspecError">圈数</param>
        public virtual void BMSSetConstAndInspectionError(List<int> lstIDs, double ElecConstant, double InspecError, string[] classNames = null) { }

        /// <summary>
        /// BMS清除电量
        /// </summary>
        /// <param name="lstIDs"></param>
        public virtual void BMSClearEnergy(List<int> lstIDs, string[] classNames = null) { }

        /// <summary>
        /// BMS读取电量
        /// </summary>
        /// <param name="lstIDs"></param>
        public virtual double BMSGetEnergy(List<int> lstIDs, EmChargerType chargerType = EmChargerType.Charger_GB_DC) { return 0; }

        /// <summary>
        /// 设置/读取电表误差
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="ElectricConstant16">常数</param>
        /// <param name="InspectionError16">圈数</param>
        /// <returns></returns>
        public virtual Dictionary<int, double[]> BMSGetError(List<int> lstIDs, string ElectricConstant16, string InspectionError16, string[] classNames = null) { return null; }
        /// <summary>
        /// 误差清零
        /// </summary>
        /// <param name="lstIDs"></param>
        public virtual void BMSClearError(List<int> lstIDs, string[] classNames = null) { }


        /// <summary>
        /// 读取互操作开关状态
        /// </summary>
        /// <param name="lstIDs">充电枪id信息集合</param>
        public virtual Dictionary<int, List<bool>> BMS_GetKState(List<int> lstIDs, string[] classNames = null) { return null; }
        /// <summary>
        /// 读取直流互操作开关状态
        /// </summary>
        /// <param name="lstIDs">充电枪id信息集合</param>
        public virtual Dictionary<int, List<bool>> BMSGetKState_DC(List<int> lstIDs, out double resistance, out double batteryVoltage, string[] classNames = null) { resistance = 0; batteryVoltage = 0; return null; }
        /// <summary>
        /// 读取欧标直流互操作开关状态
        /// </summary>
        /// <param name="lstIDs">充电枪id信息集合</param>
        public virtual Dictionary<int, List<int>> BMSGetKState_EU_DC(List<int> lstIDs, out double batteryVoltage, string[] classNames = null) { batteryVoltage = 0; return null; }
        /// <summary>
        /// 直流BMS读取参数
        /// </summary>
        /// <param name="lstIDs">充电枪id信息集合</param>
        /// <param name="tComm">组合指令</param>
        /// <returns>参数集合</returns>
        public virtual Dictionary<int, List<double>> BMSGetParameter_EU_DC(List<int> lstIDs, byte tComm, string[] classNames = null) { return null; }


        /// <summary>
        /// 直流BMS设置充电需求阶段参数
        /// </summary>
        /// <param name="lstIDs">充电枪id信息集合</param>
        /// <param name="bmsVolt">BMS需求电压设置(V)</param>
        /// <param name="bmsCurrent">bmsCurrent BMS需求电流设置(A)</param>
        /// <param name="type">true - 恒压  false - 恒流</param>
        /// <param name="measureVolt">充电电压测量值</param>
        public virtual void SetParameter(List<int> lstIDs, Double bmsVolt, Double bmsCurrent, bool type, double measureVolt, bool canCharge = true, string[] classNames = null, InsulationState insulationState = InsulationState.正常) { }
        /// <summary>
        /// 直流BMS设置参数配置阶段参数(设置BCP报文最高允许电压
        /// </summary>
        /// <param name="lstIDs">充电枪id信息集合</param>
        /// <param name="bmsVolt">动力蓄电池当前电池电压(V)</param>
        /// <param name="maxVolt">最高允许充电电压</param>
        /// <param name="maxCurrent">最高允许充电电流</param>
        public virtual void SetParameter(List<int> lstIDs, Double bmsVolt, Double maxVolt, double maxCurrent, string[] classNames = null) { }
        /// <summary>
        /// 直流BMS设置握手阶段参数(设置BHM报文最高允许电压
        /// </summary>
        /// <param name="lstIDs">充电枪id信息集合</param>
        /// <param name="bmsVolt">BMS需求电压设置(V)</param>
        public virtual void SetParameter(List<int> lstIDs, Double bmsVolt, double BatteryTotalVolt = 410, string[] classNames = null) { }

        /// <summary>
        /// 直流BMS设置需求电压电流
        /// </summary>
        /// <param name="lstIDs">枪信息</param>
        /// <param name="bmsDemandVolt">BMS需求电压</param>
        /// <param name="bmsDemandCurrent">BMS需求电流</param>
        /// <param name="type">true 恒压   ，  false  恒流</param>
        public virtual void SetParams(List<int> lstIDs, double bmsDemandVolt, double bmsDemandCurrent, bool type, string[] classNames = null) { }
        /// <summary>
        /// 欧标充电数据设置1
        /// </summary>
        /// <param name="lstIDs">枪信息</param>
        /// <param name="Para1">数据内容1-3</param>
        /// <param name="RESS_SoC">RESS SoC 默认值15</param>
        /// <param name="MaxCurrent">BMS最大电流值</param>
        /// <param name="MaxVoltage">BMS最大电压值</param>
        public virtual void BMSSetPara1_EU_DC(List<int> lstIDs, List<int> Para1, double RESS_SoC, double MaxCurrent, double MaxVoltage, string[] classNames = null) { }
        /// <summary>
        /// 欧标充电数据设置1
        /// </summary>
        /// <param name="lstIDs">枪信息</param>
        /// <param name="RESS_SoC">RESS SoC 默认值15</param>
        /// <param name="MaxCurrent">BMS最大电流值</param>
        /// <param name="MaxVoltage">BMS最大电压值</param>
        public virtual void BMSSetPara1_EU_DC(List<int> lstIDs, double RESS_SoC, double MaxCurrent, double MaxVoltage, string[] classNames = null) { }
        /// <summary>
        /// 欧标充电数据设置2
        /// </summary>
        /// <param name="lstIDs">枪信息</param>
        /// <param name="FullSOC">Full SoC值默认值100</param>
        /// <param name="BulkSOC">Bulk SoC值默认值85</param>
        /// <param name="TargetVolt">EV目标需求电压</param>
        /// <param name="TargetCurrent">EV目标需求电流</param>
        /// <param name="ReadyVolt">预充充电电压默认值400</param>
        public virtual void BMSSetPara2_EU_DC(List<int> lstIDs, double FullSOC, double BulkSOC, double TargetVolt, double TargetCurrent, double ReadyVolt, string[] classNames = null) { }
        /// <summary>
        /// 欧标充电数据设置3
        /// </summary>
        /// <param name="lstIDs">枪信息</param>
        /// <param name="FullSOCRemainTime">剩余时间到满SoC</param>
        /// <param name="BulkSOCRemainTime">剩余时间到Bulk SoC</param>
        public virtual void BMSSetPara3_EU_DC(List<int> lstIDs, double FullSOCRemainTime, double BulkSOCRemainTime, string[] classNames = null) { }
        /// <summary>
        /// 直流BMS设置互操电阻值
        /// </summary>
        /// <param name="lstIDs">充电枪id信息集合</param>
        /// <param name="resistance">（桩模拟器R3/车模拟器R4）</param>
        /// <returns></returns>
        public virtual void BMSSetResistance(List<int> lstIDs, double resistance, string[] classNames = null) { }

        /// <summary>
        /// BMS设置互操电池电压
        /// </summary>
        /// <param name="lstIDs">充电枪id信息集合</param>
        /// <param name="batteryVoltage">电池电压</param>
        /// <returns></returns>
        public virtual void BMSSetBatteryVoltage(List<int> lstIDs, double batteryVoltage, string[] classNames = null) { }

        /// <summary>
        /// 直流BMS设置互操作开关状态
        /// </summary>
        /// <param name="lstIDs">充电枪id信息集合</param>
        /// <param name="resistance">（桩模拟器R3/车模拟器R4）</param>
        /// <param name="batteryVoltage">电池电压</param>
        /// <param name="bs">开关状态集合</param>
        public virtual void BMSSetKState_DC(List<int> lstIDs, double resistance, double batteryVoltage, bool[] bs, string[] classNames = null) { }
        /// <summary>
        /// 直流BMS双枪并充开关读取
        /// </summary>
        /// <param name="lstIDs">充电枪id信息集合</param>
        /// <param name="isON">开关状态</param>
        /// <returns></returns>
        public virtual void BMSReadCombine_DC(List<int> lstIDs, out bool isON, string[] classNames = null) { isON = false; }
        /// <summary>
        /// 直流BMS双枪并充开关设置
        /// </summary>
        /// <param name="lstIDs">充电枪id信息集合</param>
        /// <param name="isON">开关状态</param>
        /// <returns></returns>
        public virtual void BMSSetCombine_DC(List<int> lstIDs, bool isON, string[] classNames = null) { }
        /// <summary>
        /// 直流BMS通断开关控制
        /// </summary>
        /// <param name="lstIDs">充电枪id信息集合</param>
        /// <param name="bs">开关状态集合</param>
        public virtual void BMSSetKState_DC(List<int> lstIDs, byte tComm, byte[] bs, string[] classNames = null) { }
        /// <summary>
        /// 直流BMS读取DC+/DC-对PE漏电电阻值
        /// </summary>
        /// <param name="lstIDs">充电枪id信息集合</param>
        /// <param name="DCUpON">DC+对PE的漏电开关 0=断开；1=接通</param>
        /// <param name="DCDownON">DC-对PE的漏电开关 0=断开；1=接通</param>
        /// <param name="DCUpResistance">DC+对PE的漏电电阻值</param>
        /// <param name="DCDownResistance">DC-对PE的漏电电阻值</param>
        public virtual void BMSReadLeakageResistance_DC(List<int> lstIDs, out int DCUpON, out int DCDownON, out double DCUpResistance, out double DCDownResistance, string[] classNames = null) 
        { DCUpON = 0; DCDownON = 0; DCUpResistance = 0; DCDownResistance = 0; }
        /// <summary>
        /// 直流BMS设置DC+/DC-对PE漏电电阻值
        /// </summary>
        /// <param name="lstIDs">充电枪id信息集合</param>
        /// <param name="DCUpON">DC+对PE的漏电开关 0=断开；1=接通</param>
        /// <param name="DCDownON">DC-对PE的漏电开关 0=断开；1=接通</param>
        /// <param name="DCUpResistance">DC+对PE的漏电电阻值</param>
        /// <param name="DCDownResistance">DC-对PE的漏电电阻值</param>
        public virtual void BMSSetLeakageResistance_DC(List<int> lstIDs, int DCUpON, int DCDownON, double DCUpResistance, double DCDownResistance, string[] classNames = null) { }
        /// <summary>
        /// 直流BMS设置互操作开关状态
        /// </summary>
        /// <param name="lstIDs">充电枪id信息集合</param>
        /// <param name="batteryVoltage">电池电压</param>
        /// <param name="bs">数据内容第123字节的二进制开关（24位）</param>
        /// <param name="DCPlus">DC+绝缘阻值档位</param>
        /// <param name="DCMinus">DC-绝缘阻值档位</param>
        /// <param name="reserved">数据内容第5字节预留</param>
        public virtual void BMSSetKState_EU_DC(List<int> lstIDs, double batteryVoltage, bool[] bs, int DCPlus, int DCMinus, string reserved = "", string[] classNames = null) { }
        /// <summary>
        /// 赛特BMS直流通断控制
        /// </summary>
        /// <param name="tComm">组合指令</param>
        /// <param name="tisTrue">上位机CAN报文数据读取: 01:启动BMS报文数据读取 00:关闭BMS报文数据读取</param>
        public virtual void BMS_DC_SetControl(List<int> lstIDs, byte tComm, bool tisTrue, string[] classNames = null) { }

        /// <summary>
        /// BMS 协议一致性测试设置
        /// </summary>
        /// <param name="lstIDs">充电枪id信息集合</param>
        /// <param name="byte0"></param>
        /// <param name="byte1"></param>
        /// <param name="byte2"></param>
        /// <param name="byte3"></param>
        /// <param name="byte4"></param>
        /// <param name="byte5"></param>
        /// <param name="byte6"></param>
        /// <param name="byte7"></param>
        public virtual void BMSProtocolConsistency(List<int> lstIDs, byte byte0, byte byte1, byte byte2, byte byte3, byte byte4, byte byte5, byte byte6, byte byte7, string[] classNames = null) { }

        /// <summary>
        /// 设置系统时间
        /// </summary>
        public virtual void SetProtocolTime(List<int> lstIDs, string[] classNames = null) { }

        /// <summary>
        /// 获取BMS报文解析
        /// </summary>
        /// <returns></returns>
        public virtual Dictionary<int, CanMsgRich> RevCANPacket(List<int> lstIDs, string[] classNames = null) { return null; }

        /// <summary>
        /// 获取下位机版本
        /// </summary>
        /// <param name="SoftwareVersion">软件版本</param>
        /// <param name="FlowNumber">流水号</param>
        public virtual void BMSGetVersion(List<int> lstIDs, out string SoftwareVersion, out string FlowNumber, string[] classNames = null) { SoftwareVersion = ""; FlowNumber = ""; }

        /// <summary>
        /// 获取CAN报文是否停止接收
        /// </summary>
        /// <returns></returns>
        public virtual Dictionary<int, bool> GetIsCANStop(List<int> lstIDs) { return null; }

        /// <summary>
        /// 获取CAN报文解析后数据（用于判断测试项是否合格）
        /// </summary>
        /// <returns></returns>
        public virtual Dictionary<int, List<DataModel.CAN.CanMsgRich>> GetCANDATA(List<int> lstIDs) { return null; }


        /// <summary>
        /// 国标直流所有充电需求参数都重新设置发送
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="settingCharging">充电所有的需求参数</param>
        public virtual void BMSDC_SetAllParameter(List<int> lstIDs, SettingCharging settingCharging, string[] classNames = null) { }

        /// <summary>
        /// 获得DN2009测试项辅源停止时间，目前只有公牛下位机适配了这个功能
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <returns></returns>
        public virtual Dictionary<int, int> GetK3K4StopTime(List<int> lstIDs, string[] classNames = null) { return null; }

        /// <summary>
        /// 日标读取充电报文数据
        /// </summary>
        /// <param name="ErrorSign"></param>
        /// <param name="StateSign"></param>
        /// <returns></returns>
        public virtual Dictionary<int, List<int>> BMSGetData_JP_DC(List<int> lstIDs, out int[] ErrorSign, out int[] StateSign, string[] classNames = null) { ErrorSign = new int[8]; StateSign = new int[8]; return null; }

        /// <summary>
        /// 日标充电报文数据设置
        /// 1	0x00	2	预留
        /// 2	最小电池电压	2	
        /// 3	最大电池电压	2	1V 范围0~600V
        /// 4	充电率参考常数	1	1%
        /// 5	0x00	2	预留
        /// 6	最大充电时间	1	10秒/位，0 ~ 2540秒
        /// 7	最大充电时间	1	1分钟/位，0~255分钟
        /// 8	预计充电时间(分钟)  1	偏移量1,0 ~ 254分钟
        /// 9	0x00	4	预留
        /// 10	CHAdeMO控制协议号	1	
        /// 11	目标电池电压	2	1V 范围0~600V
        /// 12	充电电流请求	1	1A   0 ~ 255A
        /// 13	故障标志：
        /// 电池过压(bit0)：
        /// 0:正常，1:故障
        /// 电池欠压(bit1)：
        /// 0:正常，1:故障
        /// 电池电流偏差误差(bit2)：
        /// 0:正常，1:故障
        /// 电池温度高(bit3)：
        /// 0:正常，1:故障
        /// 电池电压偏差误差(bit4)：
        /// 0:正常，1:故障	1	
        /// 14	状态标志：
        /// 开启车辆充电(bit0)：
        /// 0:关闭，1:开启
        /// 车辆换挡位置(bit1)：
        /// 0:“停车”位置，1:其他位置
        /// 充电系统故障(bit2)：
        /// 0:正常，1:故障
        /// 车辆状态(bit3)：
        /// 0:EV接触器闭合或在焊接检测中
        /// 1: EV接触器开路或焊接终止检测
        /// 充电前正常停止请求(bit4)：
        /// 0:无请求，1:停止请求	1	
        /// 15	充电率	1	1%    0 -100%
        /// 16	0x00	1	预留
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <returns></returns>
        public virtual void BMSSetData_JP_DC(List<int> lstIDs, string MinBatteryVolt, string MaxBatteryVolt, string ChargingRateConst, string MaxChargingTime_S, string MaxChargingTime_M,
            string ChargingET, string CHAdeMONumber, string TargetBatteryVolt, string ChargingCurrent, int[] ErrorSign, int[] StateSign, string ChargingRate, string[] classNames = null) { }

        /// <summary>
        /// 日标直流BMS设置参数
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="bmsVolt">BMS需求电压设置(V)</param>
        /// <param name="bmsCurrent">bmsCurrent BMS需求电流设置(A)</param>
        /// <param name="maxVolt">最大电压值</param>
        public virtual void BMSSetParameter_JP_DC(List<int> lstIDs, Double bmsVolt, Double bmsCurrent, double maxVolt, string[] classNames = null) { }

        public virtual Dictionary<int, List<bool>> BMSGetKState_JP_DC(List<int> lstIDs, out int DCPRState, out int DCMRState, out double BatteryVolt, string[] classNames = null) { DCPRState = 0; DCMRState = 0; BatteryVolt = 0; return null; }

        /// <summary>
        /// 读取BMS交直流计量温湿度数据
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="Temp">温度</param>
        /// <param name="RH">湿度</param>
        public virtual void BMSGetTempRH(List<int> lstIDs, out double Temp, out double RH, string[] classNames = null) { Temp = 0; RH = 0; }

        public virtual void BMSSendEUMsg(List<int> lstIDs, string EUMsg, string[] classNames = null) { }
    }
}