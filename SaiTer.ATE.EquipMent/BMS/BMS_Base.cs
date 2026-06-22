using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.BMS;
using SaiTer.ATE.DataModel.CAN;
using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SaiTer.ATE.EquipMent.BMS_Protocol;

namespace SaiTer.ATE.EquipMent
{
    public enum InsulationState
    {
        正常 = 0,       // 正常 (00)
        异常 = 1,     // 异常 (01)
        不可信 = 2,    // 不可信 (10)
        预留 = 3      // 预留 (11)
    }

    /// <summary>
    /// 设备基类    BMS相关虚函数
    /// </summary>
    public partial class EquipMentBase
    {
        /// <summary>
        /// CAN报文接收缓存
        /// </summary>
        public Queue<byte[]> CANDataBuf = new Queue<byte[]>();
        /// <summary>
        /// CAN报文解析后数据（用于判断测试项是否合格）
        /// </summary>
        public List<CanMsgRich> CANDATA = new List<CanMsgRich>();
        public bool isCANStop = true;
        /// <summary>
        /// CAN报文是否停止接收（1.5秒内没接收到设为true）
        /// </summary>
        public bool IsCANStop {  get { return isCANStop; } }

        #region ----------BMS导引---------
        /// <summary>
        /// 读导引实时数据
        /// </summary>
        public virtual void ReadBMS_StateData() { }
        public virtual void ReadBMSChargingData(out double ChargingVolt, out double ChargingCurrent) { ChargingVolt = 0; ChargingCurrent = 0; }
        /// <summary>
        /// 关闭导引
        /// </summary>
        public virtual void BMS_OFF() { }
        /// <summary>
        /// 启动导引
        /// </summary>
        public virtual void BMS_ON() { }
        /// <summary>
        /// 读取导引开关状态
        /// </summary>
        /// <returns></returns>
        public virtual List<bool> BMS_GetKState() { return null; }
        /// <summary>
        /// 设置导引电能表常数和工作误差校验圈数
        /// </summary>
        /// <param name="ElecConstant">常数</param>
        /// <param name="InspecError">圈数</param>
        public virtual void BMSSetConstAndInspectionError(double ElecConstant, double InspecError) { }

        /// <summary>
        /// 读取/设置误差
        /// </summary>
        /// <param name="ElectricConstant16">电表常数</param>
        /// <param name="InspectionError16">校验圈数</param>
        /// <returns></returns>
        public virtual double[] BMSGetError(string ElectricConstant16, string InspectionError16) { return null; }

        /// <summary>
        /// 导引清除计量误差
        /// </summary>
        public virtual void BMSClearError() { }
        /// <summary>
        /// 读导引电量
        /// </summary>
        /// <returns></returns>
        public virtual double BMSGetEnergy() { return 0; }
        /// <summary>
        /// 导引电量清零
        /// </summary>
        public virtual void BMSClearEnergy() { }

        /// <summary>
        /// 三标一体BMS切换
        /// </summary>
        /// <param name="type">充电桩类型</param>
        public virtual void BMSSetHCAC(EmChargerType type) { }
        /// <summary>
        /// 读取R2,R3电阻
        /// </summary>
        public virtual void BMS_GetResistance(ref UInt16 tR2, ref UInt16 tR3) { }
        /// <summary>
        /// 设置R2,R3电阻
        /// </summary>
        /// <param name="tR2">R2阻值</param>
        /// <param name="tR3">R3阻值</param>
        public virtual void BMS_SetResistance(UInt16 tR2, UInt16 tR3) { }   /////BMS S2开关断开, CC CP PE闭合
        /// <summary>
        /// 设置S2状态
        /// </summary>
        /// <param name="bs">FALSE断开或TRUE闭合</param>
        public virtual void BMS_SetS2State(bool bs) { }   /////BMS S2开关闭合, CC CP PE闭合   电子锁控制解锁
        /// <summary>
        /// 设置互操作开关状态
        /// </summary>
        /// <param name="bs">开关状态集合（16个）</param>
        public virtual void BMS_SetKState(List<bool> bs) { }



        /// <summary>
        /// 直流BMS设置参数
        /// </summary>
        /// <param name="bmsVolt">BMS需求电压设置(V)</param>
        /// <param name="bmsCurrent">bmsCurrent BMS需求电流设置(A)</param>
        /// <param name="type">true - 恒压  false - 恒流</param>
        /// <param name="bmsCurrent">充电电压测量值</param>
        public virtual void BMSSetParameter(Double bmsVolt, Double bmsCurrent, bool type, double measureVolt, bool canCharge = true,InsulationState insulationState = InsulationState.正常)
        {

        }
        /// <summary>
        /// 获得DN2009测试项辅源停止时间，目前只有公牛下位机适配了这个功能
        /// </summary>
        /// <returns></returns>
        public virtual void GetK3K4StopTime(out int K3K4Time)
        {
            K3K4Time = -999;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="settingCharging">所有的参数设置</param>
        public virtual void BMSSetALLParameter(SettingCharging settingCharging)
        {

        }
        /// <summary>
        /// 直流BMS设置参数
        /// </summary>
        /// <param name="bmsVolt">动力蓄电池当前电池电压(V)</param>
        /// <param name="maxVolt">最高允许充电电压</param>
        /// <param name="maxCurrent">最高允许充电电流</param>
        public virtual void BMSSetParameter(Double bmsVolt, Double maxVolt, double maxCurrent)
        {

        }
        /// <summary>
        /// 欧标充电数据设置1
        /// </summary>
        /// <param name="Para1">数据内容1-3</param>
        /// <param name="RESS_SoC">RESS SoC值</param>
        /// <param name="MaxCurrent">BMS最大电流值</param>
        /// <param name="MaxVoltage">BMS最大电压值</param>
        public virtual void BMSSetPara1_EU_DC(List<int> para1, double RESS_SoC, double MaxCurrent, double MaxVoltage) { }
        /// <summary>
        /// 欧标充电数据设置1
        /// </summary>
        /// <param name="RESS_SoC">RESS SoC值</param>
        /// <param name="MaxCurrent">BMS最大电流值</param>
        /// <param name="MaxVoltage">BMS最大电压值</param>
        public virtual void BMSSetPara1_EU_DC(double RESS_SoC, double MaxCurrent, double MaxVoltage) { }
        /// <summary>
        /// 欧标充电数据设置2
        /// </summary>
        /// <param name="FullSOC">Full SoC值</param>
        /// <param name="BulkSOC">Bulk SoC值</param>
        /// <param name="TargetVolt">EV目标需求电压</param>
        /// <param name="TargetCurrent">EV目标需求电流</param>
        /// <param name="ReadyVolt">预充充电电压</param>
        public virtual void BMSSetPara2_EU_DC(double FullSOC, double BulkSOC, double TargetVolt, double TargetCurrent, double ReadyVolt) { }
        /// <summary>
        /// 欧标充电数据设置3
        /// </summary>
        /// <param name="FullSOCRemainTime">剩余时间到满SoC</param>
        /// <param name="BulkSOCRemainTime">剩余时间到Bulk SoC</param>
        public virtual void BMSSetPara3_EU_DC(double FullSOCRemainTime, double BulkSOCRemainTime) { }
        /// <summary>
        /// 直流BMS设置参数
        /// </summary>
        /// <param name="bmsVolt">BMS需求电压设置(V)</param>
        public virtual void BMSSetParameter(Double bmsVolt, double BatteryTotalVolt)
        {

        }
        /// <summary>
        /// 直流BMS设置互操电阻值
        /// </summary>
        /// <param name="resistance">（桩模拟器R3/车模拟器R4）</param>
        /// <returns></returns>
        public virtual void BMSSetResistance(double resistance) { }

        /// <summary>
        /// BMS设置互操电池电压
        /// </summary>
        /// <param name="batteryVoltage">电池电压</param>
        /// <returns></returns>
        public virtual void BMSSetBatteryVoltage(double batteryVoltage) { }

        /// <summary>
        /// 直流BMS双枪并充开关读取
        /// </summary>
        /// <param name="isON">开关状态</param>
        public virtual void BMSReadCombine_DC(out bool isON) { isON = false; }
        /// <summary>
        /// 直流BMS双枪并充开关控制
        /// </summary>
        /// <param name="isON">开关状态</param>
        public virtual void BMSSetCombine_DC(bool isON) { }
        /// <summary>
        /// 直流BMS设置互操作开关状态
        /// </summary>
        /// <param name="resistance">（桩模拟器R3/车模拟器R4）</param>
        /// <param name="batteryVoltage">电池电压</param>
        /// <param name="bs">开关状态集合</param>
        public virtual void BMSSetKState_DC(double resistance, double batteryVoltage, bool[] bs) { }
        /// <summary>
        /// 直流BMS通断开关控制
        /// </summary>
        /// <param name="bs">开关状态集合</param>
        public virtual void BMSSetKState_DC(byte tComm, byte[] bs) { }
        /// <summary>
        /// 直流BMS读取DC+/DC-对PE漏电电阻值
        /// </summary>
        /// <param name="DCUpON">DC+对PE的漏电开关 0=断开；1=接通</param>
        /// <param name="DCDownON">DC-对PE的漏电开关 0=断开；1=接通</param>
        /// <param name="DCUpResistance">DC+对PE的漏电电阻值</param>
        /// <param name="DCDownResistance">DC-对PE的漏电电阻值</param>
        public virtual void BMSReadLeakageResistance_DC(out int DCUpON, out int DCDownON, out double DCUpResistance, out double DCDownResistance)
        { DCUpON = 0; DCDownON = 0; DCUpResistance = 0; DCDownResistance = 0; }
        /// <summary>
        /// 直流BMS设置DC+/DC-对PE漏电电阻值
        /// </summary>
        /// <param name="DCUpON">DC+对PE的漏电开关 0=断开；1=接通</param>
        /// <param name="DCDownON">DC-对PE的漏电开关 0=断开；1=接通</param>
        /// <param name="DCUpResistance">DC+对PE的漏电电阻值</param>
        /// <param name="DCDownResistance">DC-对PE的漏电电阻值</param>
        public virtual void BMSSetLeakageResistance_DC(int DCUpON, int DCDownON, double DCUpResistance, double DCDownResistance) { }
        /// <summary>
        /// 直流BMS读取互操作开关状态
        /// </summary>
        /// <param name="batteryVoltage">电池电压</param>
        /// <returns>开关状态集合</returns>
        public virtual void BMSSetKState_EU_DC(double batteryVoltage, bool[] bs, int DCPlus, int DCMinus, string reserved) { }
        /// <summary>
        /// 直流BMS读取互操作开关状态
        /// </summary>
        /// <param name="resistance">（桩模拟器R3/车模拟器R4）</param>
        /// <param name="batteryVoltage">电池电压</param>
        /// <returns>开关状态集合</returns>
        public virtual List<bool> BMSGetKState_DC(out double resistance, out double batteryVoltage) { resistance = 0; batteryVoltage = 0; return null; }
        /// <summary>
        /// 直流BMS读取互操作开关状态
        /// </summary>
        /// <param name="resistance">（桩模拟器R3/车模拟器R4）</param>
        /// <param name="batteryVoltage">电池电压</param>
        /// <returns>开关状态集合</returns>
        public virtual List<int> BMSGetKState_EU_DC(out double batteryVoltage) { batteryVoltage = 0; return null; }
        /// <summary>
        /// 直流BMS读取参数
        /// </summary>
        /// <param name="tComm">组合指令</param>
        /// <returns>参数集合</returns>
        public virtual List<double> BMSGetParameter_EU_DC(byte tComm) { return null; }

        /// <summary>
        /// 赛特BMS直流通断控制
        /// </summary>
        /// <param name="tComm">组合指令</param>
        /// <param name="tisTrue"></param>
        public virtual void BMS_DC_SetControl(byte tComm, bool tisTrue) { }

        /// <summary>
        /// BMS 协议一致性测试设置
        /// </summary>
        /// <param name="byte0"></param>
        /// <param name="byte1"></param>
        /// <param name="byte2"></param>
        /// <param name="byte3"></param>
        /// <param name="byte4"></param>
        /// <param name="byte5"></param>
        /// <param name="byte6"></param>
        /// <param name="byte7"></param>
        public virtual void BMSProtocolConsistency(byte byte0, byte byte1, byte byte2, byte byte3, byte byte4, byte byte5, byte byte6, byte byte7) { }

        /// <summary>
        /// 设置系统时间
        /// </summary>
        public virtual void SetProtocolTime() { }

        /// <summary>
        /// 获取BMS报文解析
        /// </summary>
        /// <returns></returns>
        public virtual byte[] RevCANPacket() { return null; }

        /// <summary>
        /// 获取下位机版本
        /// </summary>
        /// <param name="SoftwareVersion">软件版本</param>
        /// <param name="FlowNumber">流水号</param>
        public virtual void BMSGetVersion(out string SoftwareVersion, out string FlowNumber) { SoftwareVersion = ""; FlowNumber = ""; }

        /// <summary>
        /// 日标读取充电报文数据
        /// </summary>
        /// <param name="ErrorSign"></param>
        /// <param name="StateSign"></param>
        /// <returns></returns>
        public virtual List<int> BMSGetData_JP_DC(out int[] ErrorSign, out int[] StateSign) { ErrorSign = new int[8]; StateSign = new int[8]; return null; }

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
        public virtual void BMSSetData_JP_DC(string MinBatteryVolt, string MaxBatteryVolt, string ChargingRateConst, string MaxChargingTime_S, string MaxChargingTime_M,
            string ChargingET, string CHAdeMONumber, string TargetBatteryVolt, string ChargingCurrent, int[] ErrorSign, int[] StateSign, string ChargingRate) { }



        /// <summary>
        /// 日标直流BMS设置参数
        /// </summary>
        /// <param name="bmsVolt">BMS需求电压设置(V)</param>
        /// <param name="bmsCurrent">bmsCurrent BMS需求电流设置(A)</param>
        /// <param name="maxVolt">最大电压值</param>
        public virtual void BMSSetParameter_JP_DC(Double bmsVolt, Double bmsCurrent,  double maxVolt)
        {

        }

        /// <summary>
        /// 日标读取开关状态
        /// </summary>
        /// <param name="DCPRState"></param>
        /// <param name="DCMRState"></param>
        /// <param name="BatteryVolt"></param>
        /// <returns></returns>
        public virtual List<bool> BMSGetKState_JP_DC(out int DCPRState, out int DCMRState, out double BatteryVolt) {  DCPRState = 0; DCMRState = 0; BatteryVolt = 0; return null; }

        /// <summary>
        /// 读取BMS交直流计量温湿度数据
        /// </summary>
        /// <param name="Temp">温度</param>
        /// <param name="RH">湿度</param>
        public virtual void BMSGetTempRH(out double Temp, out double RH) { Temp = 0; RH = 0; }
        /// <summary>
        /// 欧标报文发送指令
        /// </summary>
        /// <param name="EUMsg"></param>
        public virtual void SendEUMsg(string EUMsg) { }
        #endregion

    }
}
