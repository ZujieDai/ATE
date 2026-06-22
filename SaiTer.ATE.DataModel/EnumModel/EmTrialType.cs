using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.EnumModel
{
    // 1. 定义自定义特性
    [AttributeUsage(AttributeTargets.Field)]
    public class APlusAppParamAttribute : Attribute
    {
        public string ProtocolConsistencyVersion { get; }
        public APlusAppParamAttribute(string paramValue)
        {
            ProtocolConsistencyVersion = paramValue;
        }
    }
    /// <summary>
    /// 测试项目编号枚举 （配置文件内要添加此编号且一一对应）
    /// </summary>
    public enum EmTrialType : uint
    {

        /// <summary>
        ///   空/无试验项目
        /// </summary>
        Null = 0,

        #region ====安规测试====

        绝缘电阻_输入对输出 = 1001,

        绝缘电阻_输入对地 = 1002,

        绝缘电阻_输出对地 = 1003,

        交流耐压_输入对输出 = 1004,

        交流耐压_输入对地 = 1005,

        交流耐压_输出对地 = 1006,

        直流耐压_输入对输出 = 1007,

        直流耐压_输入对地 = 1008,

        直流耐压_输出对地 = 1009,

        接地试验1 = 1010,

        接地试验2 = 1011,

        接地试验3 = 1012,

        接地试验_工装 = 1017,

        绝缘电阻 = 1013,

        交流耐压 = 1014,

        直流耐压 = 1015,

        接地试验 = 1016,

        #endregion========

        #region ====人工确认====

        一般检查 = 2001,

        充电连接装置检查 = 2002,

        锁止装置检查 = 2003,

        显示功能试验 = 2004,

        接触器粘连监测试验 = 2005,

        标志检查 = 2006,

        基本构成检查 = 2007,

        输入功能测试 = 2008,

        待机功耗测试 = 2009,

        选择档位 = 2010,//新乡KLQ独有测试项，业务类 SelectGear

        充电方式设定检查 = 2011,    //北京HK独有测试项

        R2R3电阻仿真等效电阻模拟功能 = 2012,    //北京HK独有测试项
        #endregion

        #region ====漏电保护====

        //3001、3002测试设备为中佳漏电板
        漏电保护试验_交流模式 = 3001,

        漏电保护试验_脉动直流模式 = 3002,

        #region 程控板漏电流测试（只判断状态，目前TB在用）

        漏电保护测试_程控板 = 3003,

        不动作漏电测试_程控板 = 3004,

        漏电测试1 = 3005,//虚假测试流程和数据，TRW客户需求

        漏电测试2 = 3006,

        漏电测试3 = 3007,
        #endregion 

        //3101-3499 测试设备为拓勉漏电仪
        漏电分断电流_AC_50Hz = 3101,

        漏电分断电流_A0 = 3102,

        漏电分断电流_A90 = 3103,

        漏电分断电流_A135 = 3104,

        漏电分断电流_A0_DC6mA = 3105,

        漏电分断电流_DC2P = 3106,

        漏电分断电流_DC3P = 3107,

        漏电分断电流_DCSM = 3108,

        漏电分断电流_AC_60Hz = 3109,

        漏电分断电流_AC_150Hz = 3110,

        漏电分断电流_AC_60Hz_DC6mA = 3111,

        漏电分断时间_1倍漏电 = 3201,

        漏电分断时间_2倍漏电 = 3202,

        漏电分断时间_5倍漏电 = 3203,

        漏电分断时间_A0_1点4倍漏电 = 3204,

        漏电分断时间_A0_2点8倍漏电 = 3205,

        漏电分断时间_A0_350mA = 3206,

        漏电分断时间_DCSM_DC6mA = 3207,

        漏电分断时间_DCSM_DC40mA = 3208,

        漏电分断时间_DCSM_DC120mA = 3209,

        漏电分断时间_DC2P_DC40mA = 3210,

        漏电分断时间_DC2P_DC120mA = 3211,

        漏电分断时间_DC3P_DC40mA = 3212,

        漏电分断时间_DC3P_DC120mA = 3213,

        漏电分断时间_AC_60Hz = 3214,

        漏电分断时间_AC_150Hz = 3215,

        漏电分断时间_DCSM = 3216,

        漏电分断时间_AC_60Hz_DC6mA = 3217,

        //以下漏电测试项为不固定参数测试
        漏电分断电流_1 = 3301,

        漏电分断电流_2 = 3302,

        漏电分断电流_3 = 3303,

        漏电分断电流_4 = 3304,

        漏电分断电流_5 = 3305,

        漏电分断电流_6 = 3306,

        漏电分断电流_7 = 3307,

        漏电分断电流_8 = 3308,

        漏电分断电流_9 = 3309,

        漏电分断电流_10 = 3310,

        漏电分断电流_11 = 3311,

        漏电分断电流_12 = 3312,

        漏电分断电流_13 = 3313,

        漏电分断电流_14 = 3314,

        漏电分断电流_15 = 3315,

        漏电分断电流_16 = 3316,

        漏电分断电流_17 = 3317,

        漏电分断电流_18 = 3318,

        漏电分断电流_19 = 3319,

        漏电分断电流_20 = 3320,

        漏电分断电流_21 = 3321,

        漏电分断电流_22 = 3322,

        漏电分断电流_23 = 3323,

        漏电分断电流_24 = 3324,

        漏电分断电流_25 = 3325,

        漏电分断电流_26 = 3326,

        漏电分断电流_27 = 3327,

        漏电分断电流_28 = 3328,

        漏电分断电流_29 = 3329,

        漏电分断电流_30 = 3330,

        漏电分断电流_31 = 3331,

        漏电分断电流_32 = 3332,

        漏电分断电流_33 = 3333,

        漏电分断电流_34 = 3334,

        漏电分断电流_35 = 3335,

        漏电分断电流_36 = 3336,

        漏电分断电流_37 = 3337,

        漏电分断电流_38 = 3338,

        漏电分断电流_39 = 3339,

        漏电分断电流_40 = 3340,

        漏电分断时间_1 = 3401,

        漏电分断时间_2 = 3402,

        漏电分断时间_3 = 3403,

        漏电分断时间_4 = 3404,

        漏电分断时间_5 = 3405,

        漏电分断时间_6 = 3406,

        漏电分断时间_7 = 3407,

        漏电分断时间_8 = 3408,

        漏电分断时间_9 = 3409,

        漏电分断时间_10 = 3410,

        漏电分断时间_11 = 3411,

        漏电分断时间_12 = 3412,

        漏电分断时间_13 = 3413,

        漏电分断时间_14 = 3414,

        漏电分断时间_15 = 3415,

        漏电分断时间_16 = 3416,

        漏电分断时间_17 = 3417,

        漏电分断时间_18 = 3418,

        漏电分断时间_19 = 3419,

        漏电分断时间_20 = 3420,

        漏电分断时间_21 = 3421,

        漏电分断时间_22 = 3422,

        漏电分断时间_23 = 3423,

        漏电分断时间_24 = 3424,

        漏电分断时间_25 = 3425,

        漏电分断时间_26 = 3426,

        漏电分断时间_27 = 3427,

        漏电分断时间_28 = 3428,

        漏电分断时间_29 = 3429,

        漏电分断时间_30 = 3430,

        漏电分断时间_31 = 3431,

        漏电分断时间_32 = 3432,

        漏电分断时间_33 = 3433,

        漏电分断时间_34 = 3434,

        漏电分断时间_35 = 3435,

        漏电分断时间_36 = 3436,

        漏电分断时间_37 = 3437,

        漏电分断时间_38 = 3438,

        漏电分断时间_39 = 3439,

        漏电分断时间_40 = 3440,

        #endregion

        #region ====通用测试项====
        /*
         * 三标/交、直流无差异的测试项
         */
        充电控制 = 4001,
        /// <summary>
        /// 青岛客户要求的测试项，只要能刷卡上电，就PASS
        /// </summary>
        刷卡充电功能测试 = 4002,

        #endregion

        CP频率检测 = 6001,

        CP占空比测试 = 6002,

        启动和充电阶段测试_美标 = 6003,

        启动和充电阶段测试_国标 = 7003,

        正常充电结束测试 = 6004,

        充电连接控制时序测试 = 6005,

        S2开关断开再闭合测试 = 6006,

        CP断线测试 = 6007,

        CP接地测试 = 6008,

        保护接地导体连续性丢失测试 = 6009,

        输出过流测试 = 6010,

        断开开关S2测试 = 6011,

        CP电压测试 = 6012,

        输入过压保护测试 = 6013,

        输入欠压保护测试 = 6014,

        动力电源输入失电测试_交流 = 6015,

        紧急停机保护测试 = 6016,

        开门保护测试 = 6017,

        CP回路电压限值测试 = 6018,

        枪头RC电阻测试_国标 = 6019,

        枪头RC电阻测试_欧标 = 6020,

        输入过压保护及恢复测试 = 6021,

        输入欠压保护及恢复测试 = 6022,

        LED状态检测 = 6023,

        枪头电阻测试_美标 = 6024,

        不动作电流测试 = 6025,

        CP信号测试_TB = 6026,

        LN反接测试 = 6027,

        输入过频测试 = 6028,

        输入欠频测试 = 6029,

        老化测试 = 6030,//交直流都用这个枚举， 但是业务类不同

        CP允许充电电压极限值测试 = 6031,

        带载分合电路试验 = 6032,//交流测试，北京HK提出

        充电连接控制时序_欧标交流 = 6033,

        PP断线_欧标交流 = 6034,

        CS信号_美标交流 = 6035,

        PE断线_交流 = 6036,

        二极管短路_交流 = 6037,

        CP电压测试_美标交流 = 6038,

        #region ====国标直流测试项====

        车辆插头锁止功能试验 = 8001,

        预充电功能试验 = 8002,

        输出过压保护试验 = 8003,

        启动急停装置试验 = 8004,

        // 接触器粘连试验 = 8005,          

        输出电流设定误差试验 = 8006,

        输出电压设定误差试验 = 8007,

        限压特性试验 = 8008,

        限流特性试验 = 8009,

        充电控制状态试验 = 8010,

        充电连接控制时序试验 = 8011,

        通信中断试验 = 8012,

        协议一致性测试 = 8013,

        保护接地连续性测试 = 8014,

        过温保护试验 = 8015,

        低压辅助电源试验 = 8016,

        稳流精度试验 = 8017,

        稳压精度试验 = 8018,

        电压纹波试验 = 8019,

        电流纹波试验 = 8020,

        效率试验 = 8021,

        绝缘检测功能试验 = 8022,

        最大恒功率输出试验 = 8023,

        功率控制试验 = 8024,

        控制导引电压限值试验 = 8025,

        输入电流过冲测试 = 8026,

        动力电源输入失电测试_直流 = 8027,

        连接检测信号断开 = 8028,

        蓄电池反接试验 = 8029,

        冲击耐压测试 = 8030,

        通信功能试验 = 8031,

        直流输出回路短路检测功能试验 = 8032,

        稳流精度研发 = 8033,

        稳压精度研发 = 8034,

        电压纹波因数研发 = 8035,

        限压特性试验研发 = 8036,

        限流特性试验研发 = 8037,

        最大恒功率试验 = 8038,

        蓄电池电压超过充电机范围试验 = 8039,

        充电需求大于蓄电池参数试验 = 8040,

        防逆流功能试验 = 8041,

        启动输出过冲 = 8042,

        输出电压控制误差测试 = 8043,

        输出电流控制误差测试 = 8044,

        蓄电池电压与通信报文不符 = 8055,

        输出短路保护试验=8056,

        冲击耐压试验研发 = 8057,

        输出电压测量误差研发 = 8058,

        输出电流测量误差研发 = 8059,

        功率因数试验 = 8060,
        #endregion


        #region====欧美标直流测试项====

        绝缘故障_欧标 = 8501,

        充电阶段测试_欧标 = 8502,

        输入过压测试_欧标 = 8503,

        输入欠压测试_欧标 = 8504,

        保护接地连续性丢失测试_欧标 = 8505,

        待机功耗试验_欧标 = 8506,

        连接确认测试_欧标 = 8507,

        充电准备就绪测试_欧标 = 8508,

        正常充电结束测试_欧标 = 8509,

        预充电试验_欧标 = 8510,

        车辆接口断开测试_欧标 = 8511,

        最大输出电流测试_欧标 = 8512,

        效率功率因数_欧标 = 8513,

        自检阶段测试_欧标 = 8514,

        CP断线测试_欧标 = 8515,

        输出电压超过车辆允许值_欧标 = 8516,

        绝缘故障测试_欧标 = 8517,

        PE断线测试_欧标 = 8518,

        交流源输入等级_欧标 = 8519,

        显示功能测试_欧标 = 8520,

        门禁故障测试_欧标 = 8521,

        突卸载测试_欧标 = 8522,

        过温保护测试_欧标 = 8523,

        最大额定输出功率测试_欧标 = 8524,

        直流电流调节精度测试_欧标 = 8525,

        直流电压调节精度测试_欧标 = 8526,

        输出电压测量误差_欧标 = 8527,

        输出电流测量误差_欧标 = 8528,

        电压纹波测试_欧标 = 8529,

        电流纹波测试_欧标 = 8530,

        急停输出电流停止速率_欧标 = 8531,

        通讯测试_欧标_SLAC = 8532,
        通讯测试_欧标_SDP = 8533,
        通讯测试_欧标_SessionSetup = 8534,
        通讯测试_欧标_SessionDiscovery = 8535,
        通讯测试_欧标_ServicePaymentSlecion = 8536,
        通讯测试_欧标_ContractAuthentication = 8537,
        通讯测试_欧标_ChargeParameterDiscovery = 8538,
        通讯测试_欧标_PowerDelivery = 8539,
        通讯测试_欧标_CableCheck = 8540,
        通讯测试_欧标_PreCharge = 8541,
        通讯测试_欧标_CurrentDemand = 8542,
        通讯测试_欧标_WeldingDetection = 8543,
        通讯测试_欧标_SessionStop = 8544,
        #endregion


        #region ====JJG1149 计量功能====

        工作误差 = 9001,

        示值误差 = 9002,

        付费金额误差 = 9003,

        时钟示值误差 = 9004,

        计量功能试验_人工 = 9005,
        #endregion


        #region ====协议一致性====

        #region 2015
        DP1001 = 11001,

        DP1002 = 11002,

        DP1003 = 11003,

        DP2001 = 12001,

        DP2002 = 12002,

        DP2003 = 12003,

        DP3001 = 13001,

        DP3002 = 13002,

        DP3003 = 13003,

        DP3004 = 13004,

        DP3005 = 13005,

        DP3006 = 13006,

        DP3007 = 13007,

        DP4001 = 14001,

        DP4002 = 14002,

        DN1001 = 21001,

        DN1002 = 21002,

        DN1003 = 21003,

        DN1004 = 21004,

        DN2001 = 22001,

        DN2002 = 22002,

        DN2003 = 22003,

        DN2004 = 22004,

        DN2005 = 22005,

        DN2006 = 22006,

        DN2007 = 22007,

        DN2008 = 22008,

        DN2009 = 22009,

        DN2010 = 22010,

        DN3001 = 23001,

        DN3002 = 23002,

        DN3003 = 23003,

        DN3004 = 23004,

        DN3005 = 23005,

        DN3006 = 23006,

        DN3007 = 23007,

        DN3008 = 23008,

        DN3009 = 23009,

        DN3010 = 23010,

        DN4001 = 24001,

        DN4002 = 24002,

        DN4003 = 24003,

        DN4004 = 24004,
        #endregion

        #region 2023_A类测试项

        //26年2月 19号 14:49:05
        //DP改成1420001
        //DN就用1421001开始
        [APlusAppParam("2025_A类")]
        DP1001_2025 = 1420001,
        [APlusAppParam("2025_A类")]
        DP1002_2025 = 1420002,
        [APlusAppParam("2025_A类")]
        DP1003_2025 = 1420003,
        [APlusAppParam("2025_A类")]
        DN1001_2025 = 1421001,
        [APlusAppParam("2025_A类")]
        DN1002_2025 = 1421002,
        [APlusAppParam("2025_A类")]
        DN1003_2025 = 1421003,
        [APlusAppParam("2025_A类")]
        DN1004_2025 = 1421004,
        [APlusAppParam("2025_A类")]
        DN1005_2025 = 1421005,
        [APlusAppParam("2025_A类")]
        DP2001_2025 = 1420004,
        [APlusAppParam("2025_A类")]
        DP2002_2025 = 1420005,
        [APlusAppParam("2025_A类")]
        DP2003_2025 = 1420006,
        [APlusAppParam("2025_A类")]
        DP2004_2025 = 1420007,
        [APlusAppParam("2025_A类")]
        DN2001_2025 = 1421006,
        [APlusAppParam("2025_A类")]
        DN2002_2025 = 1421007,
        [APlusAppParam("2025_A类")]
        DN2003_2025 = 1421008,
        [APlusAppParam("2025_A类")]
        DN2004_2025 = 1421009,
        [APlusAppParam("2025_A类")]
        DN2005_2025 = 1421010,
        [APlusAppParam("2025_A类")]
        DN2006_2025 = 1421011,
        [APlusAppParam("2025_A类")]
        DN2007_2025 = 1421012,
        [APlusAppParam("2025_A类")]
        DN2008_2025 = 1421013,
        [APlusAppParam("2025_A类")]
        DN2009_2025 = 1421014,
        [APlusAppParam("2025_A类")]
        DN2010_2025 = 1421015,
        [APlusAppParam("2025_A类")]
        DN2011_2025 = 1421016,
        [APlusAppParam("2025_A类")]
        DN2012_2025 = 1421017,
        [APlusAppParam("2025_A类")]
        DN2013_2025 = 1421018,
        [APlusAppParam("2025_A类")]
        DN2014_2025 = 1421019,
        [APlusAppParam("2025_A类")]
        DP3001_2025 = 1420008,
        [APlusAppParam("2025_A类")]
        DP3002_2025 = 1420009,
        [APlusAppParam("2025_A类")]
        DP3003_2025 = 1420010,
        [APlusAppParam("2025_A类")]
        DP3004_2025 = 1420011,
        [APlusAppParam("2025_A类")]
        DP3005_2025 = 1420012,
        [APlusAppParam("2025_A类")]
        DP3006_2025 = 1420013,
        [APlusAppParam("2025_A类")]
        DP3007_2025 = 1420014,
        [APlusAppParam("2025_A类")]
        DN3001_2025 = 1421020,
        [APlusAppParam("2025_A类")]
        DN3002_2025 = 1421021,
        [APlusAppParam("2025_A类")]
        DN3003_2025 = 1421022,
        [APlusAppParam("2025_A类")]
        DN3004_2025 = 1421023,
        [APlusAppParam("2025_A类")]
        DN3005_2025 = 1421024,
        [APlusAppParam("2025_A类")]
        DN3006_2025 = 1421025,
        [APlusAppParam("2025_A类")]
        DN3007_2025 = 1421026,
        [APlusAppParam("2025_A类")]
        DN3008_2025 = 1421027,
        [APlusAppParam("2025_A类")]
        DN3009_2025 = 1421028,
        [APlusAppParam("2025_A类")]
        DN3010_2025 = 1421029,
        [APlusAppParam("2025_A类")]
        DN3011_2025 = 1421030,
        [APlusAppParam("2025_A类")]
        DN3012_2025 = 1421031,
        [APlusAppParam("2025_A类")]
        DN3013_2025 = 1421032,
        [APlusAppParam("2025_A类")]
        DP4001_2025 = 1420015,
        [APlusAppParam("2025_A类")]
        DP4002_2025 = 1420016,
        [APlusAppParam("2025_A类")]
        DN4001_2025 = 1421033,
        [APlusAppParam("2025_A类")]
        DN4002_2025 = 1421034,
        [APlusAppParam("2025_A类")]
        DN4003_2025 = 1421035,
        [APlusAppParam("2025_A类")]
        DN4004_2025 = 1421036,

        #endregion

        #endregion


        #region ====国标研发录波仪测试项====
        输出电流响应时间研发 = 50001,

        输出电流停止速率研发 = 50002,

        测量值更新时间试验研发 = 50003,

        连接确认测试研发 = 50004,

        自检阶段测试开始前大于10V不正常电池电压时握手电压超上限 = 50005,

        自检阶段测试开始前大于10V不正常电池电压时握手电压超下限 = 50006,

        自检阶段测试开始前大于10V不正常电池电压时握手电压正常 = 50007,

        自检阶段测试小于10V握手电压低于充电机下限 = 50008,

        车辆最高允许充电总电压不匹配试验 = 50009,//车辆最高允许充电总电压不匹配试验和自检阶段测试小于10V握手电压低于充电机下限是同一个测试项

        自检阶段测试小于10V握手电压高于充电机上限 = 50010,

        自检阶段测试正常充电 = 50011,

        蓄电池二重保护功能试验 = 50012,

        输出电压超过车辆允许值测试 = 50013,

        连接检测信号断开研发 = 50014,

        车辆接口断开测试 = 50015,

        开关S断开测试 = 50016,

        预充电功能试验研发 = 50017,

        急停功能试验研发 = 50018,

        启动急停装置试验研发 = 50019,

        充电连接控制时序研发B5 = 50020,

        充电连接控制时序研发B6 = 50021,

        输出冲击电流研发 = 50022,

        充电准备就绪测试研发_GEPlus5 = 50023,

        充电准备就绪测试研发_GEMinus5 = 50024,

        充电准备就绪测试研发_LessPlus5 = 50025,

        充电准备就绪测试研发_LessMinus5 = 50026,

        充电阶段测试研发 = 50027,

        正常充电结束试验研发 = 50028,

        控制导引电压限值测试研发 = 50029,

        其他充电故障测试 = 50030,

        电压纹波试验研测 = 50031,

        电流纹波试验研测 = 50032,

        防逆流功能试验研测 = 50033,

        启动输出过冲试验研测 = 50034,

        保护接地连续性测试研测 = 50035,

        正常充电结束试验研发_主动停止充电 = 50036,

        正常充电结束试验研发_被动停止充电 = 50037,
        #endregion

        //编号规则：
        //第一位：1=GB 2=CCS1 3=CCS2 4=安规 5=CHAdeMO 8=企标 9=客户定制
        //第二位：产测=1 研测=2 群充=3 协议一致性=4
        //第三位：交流=1 直流=2
        //后续为四位数递增

        #region 国标-产测-交流
        GB_PT_AC_ReadyForCharging = 1110001,            //充电准备就绪
        GB_PT_AC_ConnectionConfirmation = 1110002,        //连接确认测试
        GB_PT_AC_CCDisConnection = 1110003,             //CC断线测试
        #endregion

        #region 国标-研测-交流
        GB_RT_AC_ReadyForCharging = 1210001,                    //充电准备就绪
        GB_RT_AC_ChargingConnectionControlTiming = 1210002,     //充电连接控制时序
        GB_RT_AC_S2ControlTesting = 1210003,                    //断开开关S2测试
        GB_RT_AC_CPOverVoltage = 1210004,                       //CP回路电压限值测试
        #endregion

        #region 国标-产测-直流
        GB_PT_DC_GeneralInspection = 1120001,           // 【5.2】  【一般检查】
        GB_PT_DC_ChargingControlFunction = 1120002,     // 【5.3.1】【充电控制功能试验】
        GB_PT_DC_EVPlugLockFunction = 1120003,          // 【5.3.5】【车辆插头锁止功能试验】
        GB_PT_DC_ShowFunctionTest = 1120004,            // 【5.3.7】【显示功能试验】
        GB_PT_DC_InputFunctional = 1120005,             // 【5.3.8】【输入功能试验】
        GB_PT_DC_EmergencyStopFunctional = 1120006,     // 【5.3.10】【急停功能试验】
        GB_PT_DC_InputOverVoltProtection = 1120007,     // 【5.4.1】【输入过压保护试验】
        GB_PT_DC_InputUnderVoltProtection = 1120008,    // 【5.4.2】【输入欠压保护试验】
        GB_PT_DC_OutputOverVoltProtection = 1120009,    // 【5.4.3】【输出过压保护试验】
        GB_PT_DC_OverTemperatureProtection = 1120010,   // 【5.4.5】【过温保护试验】
        GB_PT_DC_OpenDoorProtection = 1120011,          // 【5.4.6】【开门保护试验】
        GB_PT_DC_ActivateEmergencyStop = 1120012,       // 【5.4.7】【启动急停装置试验】
        GB_PT_DC_InputCurrentOvershoot = 1120013,       // 【5.4.8】【输入电流过冲试验】
        GB_PT_DC_BatteryReverseConnect = 1120014,       // 【5.4.9】【蓄电池反接试验】
        GB_PT_DC_ContactorAdhesion = 1120015,           // 【5.4.11】【接触器粘连试验】
        GB_PT_DC_PowerInputLost = 1120016,              // 【5.8.2】【动力电源失电试验】
        GB_PT_DC_InsulationResistance = 1120017,        // 【5.10.1】【绝缘电阻试验】
        GB_PT_DC_DielectricStrength = 1120018,          // 【5.10.2】【介电强度试验】（交流耐压、直流耐压）
        GB_PT_DC_EarthingConductor1 = 1120019,          // 【5.11】  【接地试验1】
        GB_PT_DC_AuxiliaryPower = 1120020,              // 【5.12.4】【低压辅助电源试验】
        GB_PT_DC_SteadyCurrentPrecision = 1120021,      // 【5.12.5】【稳流精度试验】
        GB_PT_DC_SteadyVoltagePrecision = 1120022,      // 【5.12.6】【稳压精度试验】
        GB_PT_DC_VoltageRippleFactor = 1120023,         // 【5.12.7】【电压纹波因数试验】
        GB_PT_DC_CurrentRippleFactor = 1120024,         // 【5.12.8】【电流纹波因数试验】
        GB_PT_DC_OutputCurrentSetError = 1120025,       // 【5.12.9】【输出电流设定误差试验】
        GB_PT_DC_OutputVoltSetError = 1120026,          // 【5.12.10】【输出电压设定误差试验】
        GB_PT_DC_LimitingVoltageCharacter = 1120027,    // 【5.12.11】【限压特性试验】
        GB_PT_DC_LimitingCurrentCharacter = 1120028,    // 【5.12.12】【限流特性试验】
        GB_PT_DC_EfficiencyTest = 1120029,              // 【5.12.19】【效率试验】
        GB_PT_DC_PowerFactor = 1120030,                 // 【5.12.20】【功率因数试验】
        GB_PT_DC_ChargingControlState = 1120031,        // 【5.15.1】【充电控制状态试验】
        GB_PT_DC_ChargingConnectionControlTiming = 1120032, // 【5.15.2】【充电连接控制时序试验】
        GB_PT_DC_CommunicationOutage = 1120033,         // 【5.15.4】【通信中断试验】
        GB_PT_DC_ProtectiveGroundingContinuance = 1120034,  // 【5.15.5】【保护接地连续性试验】
        GB_PT_DC_CC1DisConnection = 1120035,            // 【5.15.6】【连接检测信号断开试验】
        GB_PT_DC_PreCharging_WaveRecoder=1120036,       // 【5.3.6】【预充电功能测试（新录波板）】
        GB_PT_DC_ConnectionConfirm=1120037,             // 【GBT34657.1-2017 6.3.2.1】【连接确认测试】
        GB_PT_DC_ChargingReady=1120038,                 // 【GBT34657.1-2017 6.3.2.3】【充电准备就绪测试】
        GB_PT_DC_ChargingStageTest=1120039,             // 【GBT34657.1-2017 6.3.2.4】【充电阶段测试】
        GB_PT_DC_NormalChargingEndTest=1120040,         // 【GBT34657.1-2017 6.3.2.5】【正常充电结束测试】
        GB_PT_DC_SelfCheckPhaseTest = 1120041,          // 【GBT34657.1-2017 6.3.2.2】【自检阶段测试】
        GB_PT_DC_GeneralManual= 1120042,                // 【通用】【人工确认测试项】
        GB_PT_DC_ChargerModeAndConnectWay = 1120043,    // 【5.5】【充电模式和连接方式检查】
        GB_PT_DC_ChargerConnectorAndCable = 1120044,    // 【5.6】【充电连接装置及电缆检查】
        GB_PT_DC_ProtectionAgainstDirectContact = 1120045, // 【5.8.1】【直接接触防护试验】
        GB_PT_DC_ElectricalIsolation = 1120046,         // 【5.7】【电气隔离检查】
        GB_PT_DC_ElectricalAndCreepage = 1120047,       // 【5.9】【电气间隙和爬电距离试验】
        GB_PT_DC_InsulationResistance_Manual = 1120048, // 【5.10.1】【绝缘电阻试验_人工】
        GB_PT_DC_DielectricStrength_Manual = 1120049,   // 【5.10.2】【介电强度试验_人工】（交流耐压、直流耐压）
        GB_PT_DC_EarthingConductor2 = 1120050,          // 【5.11】  【接地试验2】
        GB_PT_DC_EarthingConductor3 = 1120051,          // 【5.11】  【接地试验3】
        GB_PT_DC_InsulationDetection = 1120052,         // 【5.3.3】【绝缘检测功能试验】
        GB_PT_DC_AgingTest = 1120053,                   // 【老化测试】
        #endregion

        #region 国标-研测-直流
        GB_RT_DC_ChargerDevSelfCheck = 1220001,     //【GBT18487.1-2023 B.4.3】 充电机自检
        GB_RT_DC_ChargingPause = 1220002,           //【GBT18487.1-2023 B.4.5】 充电暂停
        GB_RT_DC_CommunicationOutage = 1220003,     //【GBT18487.1-2023 B.4.7.4】 通讯中断
        GB_RT_DC_VehicleVoltExceedsMax = 1220004,   //【GBT18487.1-2023 B.4.7.6】 车辆接口电压超过最高允许输出总电压
        GB_RT_DC_ChargingLoopFault = 1220005,       //【GBT18487.1-2023 B.4.7.7】 充电回路故障
        GB_RT_DC_LockerAbnormal = 1220006,          //【GBT18487.1-2023 B.4.3】 电子锁异常
        GB_RT_DC_LoadDumpTest = 1220007,            //【GBT18487.1-2023 B.4.7.10】 甩负载试验
        GB_RT_DC_OutputVoltageOverProteciton = 1220008,//【GBT44263-2024 8.2.7】 过压保护试验
        GB_RT_DC_PreCharging = 1220009,             //【NBT33008.1-2018 5.3.6】 预充电功能试验
        GB_RT_DC_ActivateEmergencyStop = 1220010,   //【NBT33008.1-2018 5.4.7】【启动急停装置试验】
        GB_RT_DC_CC1DisConnection = 1220011,        //【NBT33008.1-2018 5.15.6】【连接检测信号断开试验】
        #endregion

        #region 国标-群充-直流
        GB_GC_DC_PowerDistributeDoubleCharger = 1320001,    //功率分配双枪充电测试
        GB_GC_DC_TakeTurnsChargingDobleCharger = 1320002,   //轮充双枪充电测试
        #endregion

        #region 美标交流研测
        CCS1_RT_AC_S2ControlTesting = 2210001,           //【S2断开再闭合】
        #endregion

        #region 欧标-研测-交流
        CCS2_RT_AC_GeneralRequirementsTest = 3210001,           //【4】【一般要求测试】
        CCS2_RT_AC_InputPowerCharacteristicsTest = 3210002,     //【5.1.1】 【电源输入的特性】
        CCS2_RT_AC_OutputPowerCharacteristicsTest = 3210003,    //【5.1.2】 【电源输出的特性】
        CCS2_RT_AC_NormalEnvironmentalConditionsTest = 3210004, //【5.2】 【正常环境条件】
        CCS2_RT_AC_SpecialEnvironmentalConditionsTest = 3210005,//【5.3】 【特殊环境条件】
        CCS2_RT_AC_AccessAccordingTest = 3210006,               //【5.4】 【通道分类】
        CCS2_RT_AC_MountingMethodTest = 3210007,                //【5.5】 【安装方式】
        CCS2_RT_AC_ElectricShockProtectionTest = 3210008,       //【5.6】 【防触电保护】
        CCS2_RT_AC_ChargingModesTest = 3210009,                 //【5.7】 【充电模式】
        CCS2_RT_AC_ChargingMode3Test = 3210010,                 //【6.2】 【充电模式3】
        CCS2_RT_AC_ProtectiveConductorContinuityTest = 3210011, //【6.3.1.2】 【保护导体的连续导通检查】
        CCS2_RT_AC_ProperlyConnectedTest = 3210012,             //【6.3.1.3】 【验证EV是否正确连接到EV充电设备（插枪检测）】
        CCS2_RT_AC_PowerSuoolyEnergizationTest = 3210013,       //【6.3.1.4】 【给电动汽车充电（测CP电压）】
        CCS2_RT_AC_De_energizationEVPowerSupplyTest = 3210014,  //【6.3.1.5】 【切断电动汽车的电源（CP断线）】
        CCS2_RT_AC_MaximumAllowableCurrentTest = 3210015,       //【6.3.1.6】 【最大允许充电电流】
        CCS2_RT_AC_ModesOptionalFunctionsGeneralTest = 3210016, //【6.3.2.1】 【模式可选功能概述】
        CCS2_RT_AC_EVPlugDisconnectionTest = 3210017,           //【6.3.2.3】 【故意和意外断开车辆插头和/或EV插头】
        CCS2_RT_AC_ProtectionAgainstDegressTest = 3210018,      //【8.1】 【接触危险带电部件的防护等级】
        CCS2_RT_AC_EVPlugConnectedDisconnectionTest = 3210019,  //【8.2.1】 【断开插接式EV充电设备的连接】
        CCS2_RT_AC_ConnectedEVLossVoltTest = 3210020,           //【8.2.2】 【永久连接的EV充电设备的电源电压损失】
        CCS2_RT_AC_FaultProtectionTest = 3210021,               //【8.3】 【故障保护】
        CCS2_RT_AC_ProtectiveConductorTest = 3210022,           //【8.4】 【保护导体】
        CCS2_RT_AC_CableManagementAndStorageMeansForCablesAssemblies = 3210023,  //【11.7】 【电缆组件的电缆管理和存储方式】
        CCS2_RT_AC_ClearancesAndCreepageDistancesTest = 3210024,//【12.3】 【电气间隙和爬电距离】
        CCS2_RT_AC_FrequencyAndGeneratorVoltageTest = 3210025,  //【A4.4】 【CP频率和电压测试】
        CCS2_RT_AC_DutyCycleTest = 3210026,                     //【A4.5】 【占空比测试】
        CCS2_RT_AC_ImpulseWaveformTest = 3210027,               //【A4.6】 【脉冲波形测试】
        CCS2_RT_AC_TypicalControlPilotCircuitTest = 3210028,    //【A4.7.2】 【典型控制导频电路】
        CCS2_RT_AC_EquipmentThatSupportGridTest = 3210029,      //【A4.7.4】 【支持电网的充电设备】
        CCS2_RT_AC_ProtectiveConductorInterruptionTest = 3210030,//【A4.8】 【中断保护导体的测试】
        CCS2_RT_AC_VoltageShortCircuitValuesTest = 3210031,     //【A4.9】 【试验电压的短路值】
        #endregion

        #region 欧标-产测-直流

        CCS2_PT_DC_ChargingConnectControlSeq =3120001,              //充电连接控制时序
        CCS2_PT_DC_ChargingStage = 3120002,                         //充电阶段测试
        CCS2_PT_DC_CommunicationOutage = 3120003,                   //通讯中断试验
        CCS2_PT_DC_ConnectionConfirmation = 3120004,                //连接确认测试
        CCS2_PT_DC_CPDisConnection = 3120005,                       //CP断线测试
        CCS2_PT_DC_CurrentMeasureError = 3120006,                   //输出电流测量误差欧标
        CCS2_PT_DC_CurrentRegulationDC = 3120007,                   //直流电流调节精度测试
        CCS2_PT_DC_CurrentRipple = 3120008,                         //电流纹波
        CCS2_PT_DC_DisplayFunction = 3120009,                       //显示功能测试
        CCS2_PT_DC_GeneralRequirementsTest = 3120010,               //一般要求
        CCS2_PT_DC_InsulationBeforeCharging = 3120011,              //充电前的绝缘检测
        CCS2_PT_DC_InsulationFault = 3120012,                       //绝缘检测
        CCS2_PT_DC_MaxRatedOPower = 3120013,                        //最大额定输出功率测试
        CCS2_PT_DC_NormalEndCharging = 3120014,                     //正常充电结束
        CCS2_PT_DC_OutputCurrentAdjustTime = 3120015,               //输出电流调整时间
        CCS2_PT_DC_OutputCurrentControlError = 3120016,             //输出电流控制误差
        CCS2_PT_DC_OutputCurrentStopRate = 3120017,                 //输出电流停止速率
        CCS2_PT_DC_OutputOvervoltage = 3120018,                     //输出电压超过车辆允许值
        CCS2_PT_DC_OutputVoltageControlError = 3120019,             //输出电压控制误差
        CCS2_PT_DC_OverTemperature = 3120020,                       //过温保护实验
        CCS2_PT_DC_PoPUnLoad = 3120021,                             //突卸载测试
        CCS2_PT_DC_ProtectiveGroundingContinuity = 3120022,         //保护接地连续性丢失测试
        CCS2_PT_DC_ReadyToCharge = 3120023,                         //充电准备就绪测试
        CCS2_PT_DC_SelfInspectionStage = 3120024,                   //自检阶段测试
        CCS2_PT_DC_ShortCircuitTest = 3120025,                      //短路测试
        CCS2_PT_DC_VoltageMeasureError = 3120026,                   //输出电压测量误差
        CCS2_PT_DC_VoltageRegulationDC = 3120027,                   //直流电压调节精度测试
        CCS2_PT_DC_VoltageRipple = 3120028,                         //电压纹波欧标
        CCS2_PT_DC_VehiclePlugLock = 3120029,                       //车辆插头锁止功能测试
        CCS2_PT_DC_ErrorIndication = 3120030,                       //计量功能：示值误差(计量模块)
        CCS2_PT_DC_ErrorWork = 3120031,                             //计量功能：工作误差(计量模块)
        CCS2_PT_DC_VoltageCheckInitialization = 3120032,            //初始化阶段电压检查
        CCS2_PT_DC_EmergencyStopFunctional = 3120033,               //紧急停机测试
        CCS2_RT_DC_EfficiencyTest = 3120034,                        //效率测试

        #endregion

        #region 欧标研测直流
        CCS2_RT_DC_ProtectiveConductorContinuityTest = 3220001,         //【IEC61851-1 6.3.1.2】【保护导体的连续导通检查】
        CCS2_RT_DC_ProperlyConnectedTest = 3220002,                     //【IEC61851-1 6.3.1.3】【验证EV是否正确连接到EV充电设备】
        CCS2_RT_DC_PowerSuoolyEnergizationTest = 3220003,               //【IEC61851-1 6.3.1.4】【给电动汽车充电】
        CCS2_RT_DC_De_energizationEVPowerSupplyTest = 3220004,          //【IEC61851-1 6.3.1.5】【切断电动汽车的电源】
        CCS2_RT_DC_MaximumAllowableCurrentTest = 3220005,               //【IEC61851-1 6.3.1.6】【最大允许电流】
        CCS2_RT_DC_EVPlugConnectedDisconnectionTest = 3220006,          //【IEC61851-1 8.2.1】【断开插接式EV充电设备的连接】
        CCS2_RT_DC_ConnectedEVLossVoltTest = 3220007,                   //【IEC61851-1 8.2.2】【永久连接的EV充电设备的电源电压损失】
        CCS2_RT_DC_FaultProtectionTest = 3220008,                       //【IEC61851-1 8.3】【故障保护】
        CCS2_RT_DC_CableAssemblyOverloadProtectionTest = 3220009,       //【IEC61851-1 13.2】【电缆组件的过载保护】
        CCS2_RT_DC_EVDCSupply = 3220010,                                //【IEC61851-23_6.4.3.101】【电动汽车直流电源】
        CCS2_RT_DC_MeasuringCurrentAndVoltage = 3220011,                //【IEC61851-23 6.4.3.102】【测量电流和电压】
        CCS2_RT_DC_CompatibilityAssessment = 3220012,                   //【IEC61851-23 6.4.3.105】【相容性评估】
        CCS2_RT_DC_InsulationBeforeCharging = 3220013,                  //【IEC61851-23 6.4.3.106】【充电前绝缘检测】
        CCS2_RT_DC_BatteryOvervoltageProtection = 3220014,              //【IEC61851-23 6.4.3.107】【电池端过电压保护】
        CCS2_RT_DC_ControlCircuitSupplyIntegrity = 3220015,             //【IEC61851-23 6.4.3.109】【控制电路供应完整性】
        CCS2_RT_DC_ShortCircuitBeforeCharging = 3220016,                //【IEC61851-23 6.4.3.110】【充电前的短路测试】
        CCS2_RT_DC_AgainstTemporaryOvervoltageProtection = 3220017,     //【IEC61851-23 6.4.3.113】【防止暂时过电压】
        CCS2_RT_DC_EmergencyShutdown = 3220018,                         //【IEC61851-23 6.4.3.114】【急停】
        CCS2_RT_DC_ProtectiveConductorContinuityChecking = 3220019,     //【IEC61851-23 6.4.3.2】【保护导体连续性检查】
        CCS2_RT_DC_SystemDe_energizationTest = 3220020,                 //【IEC61851-23 6.4.3.4】【系统断电】
        CCS2_RT_DC_RatedAndMaximumOutputPowerTest = 3220021,            //【IEC61851-23 101.2.1.1】【额定输出功率和最大输出功率】
        CCS2_RT_DC_OutputCurrentRegulationCCC = 3220022,                //【IEC61851-23 101.2.1.2.1】【输出电流调节】
        CCS2_RT_DC_OutputVoltageRegulationCVC = 3220023,                //【IEC61851-23 101.2.1.2.2】【输出电压调节】
        CCS2_RT_DC_ControlDelayChargingCurrentCCC = 3220024,            //【IEC61851-23 101.2.1.3】【电流调整速率（暂时无法测试，需要报文）】
        CCS2_RT_DC_ChargingCurrentDescendingRate = 3220025,             //【IEC61851-23 101.2.1.4】【电流停止速率（暂时无法测试，需要报文）】
        CCS2_RT_DC_CurrentRippleTest = 3220026,                         //【IEC61851-23 101.2.1.5】【电流纹波试验（没有录波仪采样，暂用示波器产测）】
        CCS2_RT_DC_VoltageRippleTest = 3220027,                         //【IEC61851-23 101.2.1.6】【电压纹波试验（没有录波仪采样，暂用示波器产测）】
        CCS2_RT_DC_LoadDumpTest = 3220028,                              //【IEC61851-23 101.2.1.7】【甩负荷试验（突卸载）】
        CCS2_RT_DC_NormalStartUp = 3220029,                             //【IEC61851-23 CC.3.2】【正常启动（t0->t10时序测试，缺少测试条件）】
        CCS2_RT_DC_NormalShutdown = 3220030,                            //【IEC61851-23 CC.3.3】【正常结束（t10->t17时序测试，缺少测试条件）】
        CCS2_RT_DC_DCSupplyInitiatedEmergencyShutdown = 3220031,        //【IEC61851-23 CC.3.4】【直流桩启动紧急停止】
        CCS2_RT_DC_EVInitiatedEmergencyShutdown = 3220032,              //【IEC61851-23 CC.3.5】【电动汽车启动紧急停止】
        CCS2_RT_DC_ITSystemRequirementsTest = 3220033,                  //【IEC61851-23 CC.4.1】【IT系统要求（充电中绝缘故障）】
        CCS2_RT_DC_InitializationVoltageCheck = 3220034,                //【IEC61851-23 CC.4.6】【初始化阶段电压检查】
        CCS2_RT_DC_PreChargingTest = 3220035,                           //【IEC61851-23 CC.5.1】【预充电】
        CCS2_RT_DC_WakeUpOfDCSupply = 3220036,                          //【IEC61851-23 CC.5.2】【电动汽车的直流电源唤醒】
        CCS2_RT_DC_TurnOnInrushCurrent = 3220037,                       //【IEC61851-23 CC.6.1】【输出冲击电流】
        CCS2_RT_DC_OutputCurrentRegulation = 3220038,                   //【IEC61851-23 CC.6.4】【直流输出电流调节】
        CCS2_RT_DC_FrequencyAndGeneratorVoltageTest = 3220039,          //【IEC61851-1 A4.4】 【CP频率和电压测试】
        CCS2_RT_DC_DutyCycleTest = 3220040,                             //【IEC61851-1 A4.5】 【占空比测试】
        CCS2_RT_DC_ImpulseWaveformTest = 3220041,                       //【IEC61851-1 A4.6】 【脉冲波形测试】
        CCS2_RT_DC_TurnOnInrushCurrentDL = 3220042,                     //【IEC61851-23 CC.6.1】【输出冲击电流（录波仪）】

        #endregion

        #region 客户定制测试项目
        CZ_SXHY_AgingTest_DCEA = 9120001,                           //欧标直流老化（深圳HY定制）
        CZ_SXHY_AgingTest_DCGB = 9120002,                           //国标直流老化（深圳HY定制）
        CZ_SXHY_AgingTest_DCUA = 9120003,                           //美标直流老化（深圳HY定制）


        CZ_NTGX_CN_ChargingAndDischarging = 9110001,                //充放电检测（南通GX  储能）
        CZ_NTGX_CN_OnOffGrid = 9110002,                             //并离网切换检测（南通GX  储能）
        CZ_NTGX_CN_CurrentHarmonic = 9110003,                       //电流谐波检测（南通GX  储能）
        CZ_NTGX_CN_VoltageHarmonic = 9110004,                       //电压谐波检测（南通GX  储能）
        CZ_NTGX_CN_DCComponent = 9110005,                           //直流分量检测（南通GX  储能）
        CZ_NTGX_CN_VoltageDeviation = 9110006,                      //输出电压偏差检测（南通GX  储能）
        CZ_NTGX_CN_FrequencyDeviation = 9110007,                    //输出频率偏差检测（南通GX  储能）
        CZ_NTGX_CN_ActivePowerControl = 9110008,                    //有功功率控制检测（南通GX  储能）
        CZ_NTGX_CN_PowerFactorCheck = 9110009,                      //功率因数检测（南通GX  储能）
        CZ_NTGX_CN_FrequencyAdaptabilityCheck = 9110010,            //频率适应性检测（南通GX  储能）
        CZ_NTGX_CN_VoltageAdaptabilityCheck = 9110011,              //电压适应性检测（南通GX  储能）
        CZ_NTGX_CN_ACFrequencyAnomalyProtection = 9110012,          //交流过欠频保护检测（南通GX  储能）
        CZ_NTGX_CN_ACVoltageAnomalyProtection = 9110013,            //交流过欠压保护检测（南通GX  储能）
        CZ_NTGX_CN_ACThreePhaseImbalanceProtection = 9110014,       //交流三相不平衡保护检测（南通GX  储能）
        CZ_NTGX_CN_ACPhaseSequenceProtection = 9110015,             //交流三相相序保护检测（南通GX  储能）

        //CZ_TPK_Outlook_ManualCheck = 9130001,   // 外观检查
        //CZ_TPK_NFC_GB_DC = 9130002,             // 国标直流 NFc
        //CZ_TPK_LED_GB_DC = 9130003,             // 国标直流 LED指示灯 
        //CZ_TPK_InsulationDetection = 9130004,   // 绝缘检测功能测试
        //CZ_TPK_EVPlugLockFunction = 9130005,    // EV充电锁功能测试
        //CZ_TPK_CC1DisConnection = 9130006,      //CC1断开检测   连接异常测试  
        //CZ_TPK_AuxiliaryPower = 9130007,        //辅助电源输出
        //CZ_TPK_DoubleCharging = 9130017,        //轮充双枪充电测试




        // 一般检查 = 2001,
        CZ_TPK_Outlook_ManualCheck1 = 9130001,                        // 外观检查
        CZ_TPK_Outlook_ManualCheck2 = 9130002,                        // 防水检查
        CZ_TPK_Outlook_ManualCheck3 = 9130003,
        CZ_TPK_Outlook_ManualCheck4 = 9130004,
        CZ_TPK_Outlook_ManualCheck5 = 9130005,
        CZ_TPK_Outlook_ManualCheck6 = 9130006,
        CZ_TPK_Outlook_ManualCheck7 = 9130007,
        CZ_TPK_Outlook_ManualCheck8 = 9130008,
        CZ_TPK_Outlook_ManualCheck9 = 9130009,
        CZ_TPK_Outlook_ManualCheck10 = 9130010,
        CZ_TPK_Outlook_ManualCheck11 = 9130011,
        CZ_TPK_Outlook_ManualCheck12 = 9130012,                    //急停功能测试 急停功能测试
        CZ_TPK_NFC_GB_DC = 9130013,                                // 国标直流 NFc    13
        CZ_TPK_LED_GB_DC = 9130014,                                // 国标直流 LED指示灯   14
        CZ_TPK_Outlook_ManualCheck15 = 9130015,                     //风扇  15
        CZ_TPK_Power_allocation = 9130016,                         //Power Allocation Dual-Gun Charging Test  16 功率分配双枪充电测试
        CZ_TPK_dual_gun_charging = 9130017,                       //Wheel charging dual gun charging test 17 轮充双枪充电测试
        CZ_TPK_EVPlugLockFunction = 9130018,                       // EV充电锁功能测试   18 
        CZ_TPK_gun_temperature = 9130019,                        //19 Charging gun temperature  充电枪温度
        CZ_TPK_fault_test = 9130020,                         //20  故障测试
        CZ_TPK_Error_testing = 9130021,                      //21 误差测试
        CZ_TPK_CC1DisConnection = 9130022,                         //CC1断开检测   连接异常测试    22
        CZ_TPK_AuxiliaryPower = 9130023,                           //辅助电源输出       23  
        CZ_TPK_ErrorPayment = 9130024,                             //错误支付功能测试     24
        CZ_TPK_Data_erasure = 9130025,                             //数据清除测试     25
        CZ_TPK_InsulationDetection = 9130026,                      //绝缘检测功能测试     26
        CZ_TPK_lightning_protection = 9130027,                     //雷击保护功能测试     27
        CZ_TPK_waterproof = 9130028,                               //防水功能测试     28
        CZ_TPK_aging = 9130029,                                    //老化测试     29


        CZ_JS_InsulationResistance_Manual = 9121001,    // 【5.10.1】【绝缘电阻试验_人工】
        CZ_JS_DielectricStrength_Manual = 9121002,      // 【5.10.2】【介电强度试验_人工】（交流耐压、直流耐压）
        CZ_JS_EarthingConductor_Manual = 9121003,       // 【5.11】【接地试验_人工】
        CZ_JS_LeakageCurrent_Manual = 9121004,          // 【漏电流试验_人工】

        CZ_TB_GeneralInspection = 9122001,              // 一般检查
        CZ_TB_OutputVoltageError = 9122002,             // 输出电压误差
        CZ_TB_OutputCurrentError = 9122003,             // 输出电流误差
        CZ_TB_FailureCheck = 9122004,                   // 故障检查
        CZ_TB_PrecisionSteadyVoltage = 9122005,         // 稳压精度
        CZ_TB_PrecisionSteadyCurrent = 9122006,         // 稳流精度
        CZ_TB_InputVoltageTest = 9122007,               // 输入电压
        CZ_TB_VoltageLimitCharacteristic = 9122008,     // 限压特性
        CZ_TB_CurrentLimitCharacteristic = 9122009,     // 限流特性
        CZ_TB_LampSignal = 9122010,                     // 灯语测试
        CZ_TB_InputOverVoltageError = 9122011,          // 输入电压过压
        CZ_TB_InputUnderVoltageError = 9122012,         // 输入电压欠压
        CZ_TB_InsulationFaultCheckEU = 9122013,         // 绝缘检测
        #endregion

        #region 惠州TB定制
        CZ_ComprehensiveTest_TBAuto = 9110201,
        CustomTest_TB = 9110202,
        ShowTipTest_TB = 9110203,
        DontChageTest_TB = 9110204,
        #endregion

        #region XJ客户提供企标 QCSG1211013-2016
        QB_RT_DC_GeneralInspection = 8210001,       //一般检查
        QB_RT_DC_ChargerSetWay = 8210002,           //充电设定方式检查
        QB_RT_DC_CommunicationFunction = 8210003,   //通信功能试验
        QB_RT_DC_ShowFunction = 8210004,            //显示功能试验
        QB_RT_DC_InputFunction = 8210005,           //输入功能试验
        QB_RT_DC_MeasureFunction = 8210006,         //计量功能试验
        QB_RT_DC_PayFunction = 8210007,             //付费交易功能试验
        QB_RT_DC_OutputVoltError = 8210008,         //输出电压误差试验
        QB_RT_DC_OutputCurrentError = 8210009,      //输出电流误差试验
        QB_RT_DC_SteadyVoltAccuracy = 8210010,      //稳压精度试验
        QB_RT_DC_SteadyCurrentAccuracy = 8210011,   //稳流精度试验
        QB_RT_DC_RippleFactor = 8210012,            //纹波系数试验(电压)
        QB_RT_DC_VoltLimitCharacteristic = 8210013, //限压特性试验
        QB_RT_DC_CurrentLimitCharacteristic = 8210014,//限流特性试验
        QB_RT_DC_Efficiency = 8210015,              //效率试验
        QB_RT_DC_PowerFactor = 8210016,             //功率因数试验
        QB_RT_DC_StandbyPowerConsumption = 8210017, //待机功耗试验
        QB_RT_DC_HarmonicsCurrent = 8210018,        //谐波电流试验
        QB_RT_DC_AuxiliaryPower = 8210019,          //低压辅助电源试验
        QB_RT_DC_InputOverVolt = 8210020,           //输入过压保护试验
        QB_RT_DC_InputUnderVolt = 8210021,          //输入欠压保护试验
        QB_RT_DC_InputOpenPhase = 8210022,          //输入缺相保护试验
        QB_RT_DC_OutputOverVolt = 8210023,          //输出过压保护试验
        QB_RT_DC_OutputOverCurrent = 8210024,       //输出过流保护试验
        QB_RT_DC_EmergencyStopFunction = 8210025,   //急停功能试验
        QB_RT_DC_InputImpulseCurrent = 8210026,     //输入冲击电流试验
        QB_RT_DC_OutputImpulseCurrent = 8210027,    //输出冲击电流试验
        QB_RT_DC_SlowStart = 8210028,               //缓启动试验
        QB_RT_DC_BatteryReverseConnect = 8210029,   //蓄电池反接试验
        QB_RT_DC_BatteryVoltCheck = 8210030,        //蓄电池电压检测试验
        QB_RT_DC_LockingFunction = 8210031,         //锁止功能试验
        QB_RT_DC_GroundingContinuity = 8210032,     //接地连续性试验
        QB_RT_DC_AntiElectricShockMeasures = 8210033,           //防触电措施试验
        QB_RT_DC_ChargerConnectorCompatibility = 8210034,       //充电接口兼容性试验
        QB_RT_DC_ChargerControlCompatibility = 8210035,         //充电控制兼容性试验
        QB_RT_DC_ChargerCommunicationCompatibility = 8210036,   //充电通信兼容性试验
        QB_RT_DC_ConnectionDevice = 8210037,        //连接装置试验
        #endregion

        #region 日标-产测-直流
        CHAdeMO_PT_DC_ChargingModeAndConnectionMethod = 4120001,
        CHAdeMO_PT_DC_ConnectionConfirm = 4120002,
        CHAdeMO_PT_DC_SelfCheckPhase = 4120003,
        CHAdeMO_PT_DC_ChargingPhase = 4120004,
        CHAdeMO_PT_DC_ChargingEndTest = 4120005,
        CHAdeMO_PT_DC_ChargingTiming = 4120006,
        CHAdeMO_PT_DC_OutputOverVoltage = 4120007,
        CHAdeMO_PT_DC_BatteryCompatibility = 4120008,
        CHAdeMO_PT_DC_OtherChargingFaults = 4120009,
        CHAdeMO_PT_DC_CommunicationOutage = 4120010,
        CHAdeMO_PT_DC_VehicleInterfaceDisconnected = 4120011,

        #region ====日标协议一致性====

        STCA1001 = 4121001,
        STCA1002 = 4121002,
        STCA1003 = 4121003,
        STCA1004 = 4121004,
        STCA1005 = 4121005,
        STCA1006 = 4121006,
        STCA1007 = 4121007,
        STCA1008 = 4121008,
        STCA1009 = 4121009,
        STCA1010 = 4121010,
        STCA1011 = 4121011,
        STCA1012 = 4121012,
        STCA1013 = 4121013,
        STCA1014 = 4121014,
        STCA1015 = 4121015,
        STCA1016 = 4121016,
        STCA1017 = 4121017,
        STCA1018 = 4121018,
        STCA1019 = 4121019,
        STCA1020 = 4121020,
        STCA1021 = 4121021,
        STCA1022 = 4121022,
        STCA1023 = 4121023,
        STCA1024 = 4121024,
        STCA1025 = 4121025,
        STCA1026 = 4121026,

        #endregion


        #endregion
    }
}
