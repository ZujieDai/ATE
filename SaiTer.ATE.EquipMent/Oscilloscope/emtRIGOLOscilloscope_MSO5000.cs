using NationalInstruments.VisaNS;
using Saiter;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.EquipMent;
using SaiTer.ATE.InterFace;
using SaiTer.ATE.PortManage.PortType;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// RIGOL-MSO5000系列示波器
    /// </summary>
    public class emtRIGOLOscilloscope_MSO5000 : EquipMentBase
    {
        const int CI_MEAURE_MAXCOUNTER = 8;
        /// <summary>
        ///  0=幅值AMPLitude；
        ///  1=平均值AVERage；
        ///  2=平均频率AVGFreq；
        ///  3=平均周期AVGPeriod；
        ///  4=带宽BWIDth；
        ///  5=延迟DELay；
        ///  6=DT
        ///  7=占空比DUTYcycle
        ///  8=负脉冲计数N-NUMber
        ///  9=下降时间FALL
        ///  10=频率FREQuency
        ///  11=顶端值HIGH/TOP
        ///  12=底端值LOW
        ///  13=最大值MAX
        ///  14=最小值MIN
        ///  15=下降过冲N-OverShoot
        ///  16=负脉宽N-Width
        ///  17=周期PERiod
        ///  18=正脉冲计数PNUMber
        ///  19=上升过冲P-OverShoot
        ///  20=峰峰值PKPK
        ///  21=正脉宽PWIDth
        ///  22=上升时间RISE
        ///  23=均方根RMS
        ///  24=偏差SDEViation
        ///  25=周期均方根CRMS
        ///  </summary>
        const string CS_MEASURE_TYPE = "VPP|VAVG|FREQuency|DUTYcycle|BWIDth|DELay|DT|PDUTy|PPULses|" +
            "FTIMe|FREQuency|VTOP|VBASe|VMAX|VMIN|OVERshoot|NPULses|PERiod|PPULses|OVERshoot|" +
            "VPP|PWIdth|RTIMe|VRMS|SDEViation|DUTYcycle";

        /// <summary>
        ///  0=自动 Auto
        ///  1=正常 Normal
        ///  2=单次 single
        ///  3=自动电平/F触发
        ///  4=多单次 NSingle
        /// </summary>
        const string CS_TRIGGER_MODE = "AUTO|NORMal|SINGle|NORMal|NORMal";

        /// <summary>
        ///  0=边沿 Edge
        ///  1=超时 Dropout
        ///  2=欠幅 Runt
        /// </summary>
        const string CS_TRIGGER_TYPE = "EDGE|TIMeout|RUNT";

        /// <summary>
        ///  0=上升 RISing
        ///  1=下降 FALLing
        ///  2=交替 ALTernate
        /// </summary>
        const string CS_TRIGGER_SLOPE = "POSitive|NEGative|RFALl";

        /// <summary>
        /// 触发状态:
        /// 0=停止/已触发Stop
        /// 1=等待触发Wait Trigger
        /// 2=自动触发Auto
        /// 3=触发中Trig'd
        /// 4=警告Arm
        /// 5=滚动Roll
        /// </summary>
        public const string CS_TRIGGER_STATE = "STOP|RUN|SINGle|STOP|RUN|RUN";

        const int CI_CURSSOR_HNUM = 10;
        const int CI_CURSSOR_VNUM = 8;

        private string Customer;
        private static object SynLock = new object();
        List<Measure> Measures = new List<Measure>();
        public class Measure
        {
            public int index { get; set; }
            public int channnel { get; set; }
            public string MeasureType { get; set; }
            public Measure(int _index, int _channnel, string _MeasureType)
            {
                index = _index;
                channnel = _channnel;
                MeasureType = _MeasureType;
            }
        }

        int DataLen = 100;
        /// <summary>
        /// 命令延时时间ms
        /// </summary>
        Filtering filtering = new Filtering();
        int sleeptime = 100;
        int rlngErrNum = 0;
        string rstrErrDescr = "";
        public emtRIGOLOscilloscope_MSO5000(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = "RIGOL " + LanguageManager.GetByKey("示波器");
            SetImagePath();
            Customer = ConfigurationManager.AppSettings["Customer"];
            //KS3000X.Dev_Config("KS3000X", true, 4, new bool[] { true, true, true, true },
            //    new string[] { "KS3000X", null, null, null }, ref rlngErrNum, ref rstrErrDescr);
            //bool btmp = KS3000X.Dev_Initialize("IVI", "TCPIP0::" + EquipMentPort.Ipaddress + "::inst0::INSTR", ref rlngErrNum, ref rstrErrDescr);

            //SystemEvent.SendConnectState(btmp, this);
        }


        /// <summary>
        /// 示波器初始化
        /// </summary>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void OscilloscopeIDefalut()
        {
            lock (SynLock)
            {
                try
                {

                    this.AutoReadData = false;
                    //EquipMentPort.VisaNS.Write("*RST");
                    //System.Threading.Thread.Sleep(sleeptime);
                    ////采集模式：采样
                    ////if (Customer != null && Customer.ToUpper().Equals("HR"))
                    ////    Oscilloscope_SetACQuireMode(1, 16);
                    ////else
                    //Oscilloscope_SetACQuireMode(0);
                    System.Threading.Thread.Sleep(sleeptime);

                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
                finally { this.AutoReadData = true; }
            }
        }

        /// <summary>
        /// 设置采样模式
        /// </summary>
        /// <param name="mode">0=SAMple|1=AVErage</param>
        /// <param name="num">用于平均值的计算点数</param>
        public override void Oscilloscope_SetACQuireMode(int mode, int num = 0)
        {
            lock (SynLock)
            {
                try
                {
                    string quireMode = new string[] { "SAMple", "AVErage" }[mode];
                    this.AutoReadData = false;
                    EquipMentPort.VisaNS.Write($":ACQuire:MODe {quireMode}");//采集模式：采样
                    System.Threading.Thread.Sleep(sleeptime);
                    if (mode == 1)
                    {
                        EquipMentPort.VisaNS.Write($":ACQuire:NUMAVg {num}");
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
                finally { this.AutoReadData = true; }
            }
        }

        /// <summary>
        /// 示波器通道设置
        /// </summary>
        /// <param name="viChannel">通道(比如1、2、3)</param>
        /// <param name="isOpen">是否开启当前通道</param>
        /// <param name="coupling">通道耦合(比如AC、DC)，测试项
        /// 纹波测试项AC,其它测试项基本为DC</param>
        /// <param name="tapewidth"> 带宽(比如20M、200M、FULL)</param>
        /// <param name="probe"> 探头比(比如100、200、0.01、0.02,100代表放大，0.01代表缩小)</param>
        /// <param name="tag">标签(纯文本英文)</param>
        /// <param name="impedance">阻抗(默认1M、50欧)</param>
        /// <param name="unit">单位(V或者A，代表电压或者电流)</param>
        /// <param name="isOpen_A">通道反相是否开启</param>
        /// <param name="gear">纵坐标档位(例子:10,20,50,100,200，现在单位为V，要根据探头比来相应设置)</param>
        /// <param name="position">纵坐标位置(根据实际纵坐标格子来算，一般是（-（格子/2），格子/2）),实际会自动乘以换算挡位</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscilloscope_Channel_Set(int viChannel, bool isOpen, string coupling, string tapewidth, string probe, string tag, string impedance, string unit, bool isOpen_A, string gear, string position)
        {
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;
                    if (isOpen)
                    {
                        int viCoupling, viBandWidth, viImpedance, viUnit, viInvert;
                        float vfProbe, vfGear, vfPosition;
                        string vstrTag = "";

                        if (coupling == "AC")
                        {
                            viCoupling = 0;
                        }
                        else if(coupling == "DC")
                        {
                            viCoupling = 1;
                        }
                        else if(coupling == "GND")
                        {
                            viCoupling = 2;
                        }
                        else
                        {
                            viCoupling = 1;
                        }

                        viBandWidth = 0;
                        if (tapewidth != "")
                        {
                            if (tapewidth == "0.25" || tapewidth == "20")
                            {
                                viBandWidth = 1;
                            }
                            else if (tapewidth == "FULL")
                            {
                                viBandWidth = 0;
                            }
                            else//这里是200M
                            {
                                viBandWidth = 2;
                            }
                        }

                        vfProbe = 1;
                        if (probe != "")
                        {
                            vfProbe = (float)Convert.ToDouble(probe);
                            //EquipMentPort.VisaNS.Write("CH" + channel + ":PROBe:GAIN " + 1.000 / Convert.ToDouble(probe));//设置探头比 CH1:PRObe:GAIN 1

                        }
                        vstrTag = tag;
                        if (impedance == "1M")
                        {
                            viImpedance = 0;
                        }
                        else
                        {
                            viImpedance = 1;
                        }
                        if (unit == "A")
                        {
                            viUnit = 1;
                        }
                        else
                        {
                            viUnit = 0;
                        }

                        viInvert = isOpen_A ? 1 : 0;
                        vfGear = 5;
                        if (gear != "")
                        {
                            vfGear = (float)Convert.ToDouble(gear);
                        }

                        vfPosition = 0;
                        if (position != "")
                        {

                            vfPosition = (float)Convert.ToDouble(position);
                        }
                        string[] stbBanswidths = { "OFF", "20M", "200M" }; // OFF是200M全带宽，ON是20M
                        string[] stbCouplings = { "AC", "DC", "GND" };
                        string[] stbImpedances = { "ONEMeg", "FIFTy" };
                        string[] stbUints = { "VOLT", "AMPere" };
                        string _label = string.IsNullOrEmpty(vstrTag) ? $"CH-{viChannel}" : vstrTag;
                        List<string> listCommands = new List<string>()
                        {
                            $":CHANnel{viChannel}:DISPlay ON",
                            $":CHANnel{viChannel}:COUPling {stbCouplings[viCoupling]}", // 设置耦合
                            $":CHANnel{viChannel}:BWLimit {stbBanswidths[viBandWidth]}", // 设置带宽
                            $":CHANnel{viChannel}:IMPedance {stbImpedances[viImpedance]}", // 设置阻抗
                            $":CHANnel{viChannel}:INVert {(viInvert == 0 ? "OFF" : "ON")}", // 设置通道反相
                            $":CHANnel{viChannel}:LABel {_label}", // 设置标签内容
                            $":CHANnel{viChannel}:PROBe {vfProbe}", // 设置变比
                            $":CHANnel{viChannel}:SCALe {vfGear}", // 设置通道挡位
                            $":CHANnel{viChannel}:UNITs {stbUints[viUnit]}", // 设置通道单位
                            $":CHANnel{viChannel}:OFFSet {(vfPosition * vfGear)}", // 设置通道位置
                        };

                        WriteMultipleString(listCommands);
                    }
                    else
                    {
                        EquipMentPort.VisaNS.Write($":CHANnel{viChannel}:DISPlay OFF");//关闭通道
                    }
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
                finally { this.AutoReadData = true; }
            }
        }

        /// <summary>
        /// 初始化测量设置，清除所有测量项
        /// </summary>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscilloscope_Measure_Initialize()
        {
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;
                    List<string> listCommands = new List<string>()
                    {
                        ":ACQuire:TYPE PEAK", // 平均值模式，测量值不会显示跳动
                        ":MEASure:CLEAr"
                    };
                    WriteMultipleString(listCommands);
                    Measures.Clear();
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
                finally { this.AutoReadData = true; }
            }
        }

        /// <summary>
        /// 添加测量项
        /// </summary>
        /// <param name="MeasurementType">类型(看添加测量项参数说明)</param>
        /// <param name="channel"></param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscilloscope_AddMeasure(string MeasurementType, int channel)
        {
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;
                    Thread.Sleep(300);
                    int viMeasureType = 0;
                    if (MeasurementType == "PKPK")
                    {
                        viMeasureType = 20;
                    }
                    if (MeasurementType == "MAX")
                    {
                        viMeasureType = 13;
                    }
                    if (MeasurementType == "MIN")
                    {
                        viMeasureType = 14;
                    }
                    if (MeasurementType == "TOP")
                    {
                        viMeasureType = 11;
                    }
                    if (MeasurementType == "BASE")
                    {
                        viMeasureType = 12;
                    }
                    if (MeasurementType == "MEAN")
                    {

                    }
                    if (MeasurementType == "RMS")
                    {
                        viMeasureType = 23;
                    }
                    if (MeasurementType == "PER")
                    {
                        viMeasureType = 17;
                    }
                    if (MeasurementType == "FREQ")
                    {
                        viMeasureType = 10;
                    }
                    if (MeasurementType == "DUTY")
                    {
                        viMeasureType = 7;
                    }
                    if (MeasurementType == "RISE")
                    {
                        viMeasureType = 22;
                    }
                    if (MeasurementType == "FALL")
                    {
                        viMeasureType = 9;
                    }


                    int Count = Measures.Count;
                    if (Count < 6)
                    {
                        string[] tbMeasureTypes = CS_MEASURE_TYPE.Split('|');
                        string strCurMeasureType = tbMeasureTypes[viMeasureType];
                        int iIndex = Measures.Select(m => m.MeasureType).ToList().IndexOf($"{channel}-{strCurMeasureType}") + 1;
                        if (iIndex == 0)
                        {
                            if (Measures.Count < CI_MEAURE_MAXCOUNTER)
                            {
                                int key = Count + 1;
                                Measures.Add(new Measure(key, channel, MeasurementType));
                            }
                            iIndex = Measures.Count;
                        }

                        WriteString($":MEASure:ITEM{tbMeasureTypes[viMeasureType]} CHANnel{channel}");
                    }
                    else
                    {
                        Measures[0].MeasureType = MeasurementType;
                    }
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
                finally { this.AutoReadData = true; }
            }
        }
        /// <summary>
        /// 读取第几个测量值    
        /// </summary>
        /// <param name="MeasureNumber">第几个</param>
        /// <returns></returns>
        public override void Oscilloscope_ReadMeasure(int MeasureNumber, ref string value)
        {
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;

                    int viMeasureType = 0;
                    int viChannel = 1;
                    string MeasurementType = Measures[MeasureNumber].MeasureType;
                    viChannel = Measures[MeasureNumber].channnel;
                    if (MeasurementType == "PKPK")
                    {
                        viMeasureType = 20;
                    }
                    if (MeasurementType == "MAX")
                    {
                        viMeasureType = 13;
                    }
                    if (MeasurementType == "MIN")
                    {
                        viMeasureType = 14;
                    }
                    if (MeasurementType == "TOP")
                    {
                        viMeasureType = 11;
                    }
                    if (MeasurementType == "BASE")
                    {
                        viMeasureType = 12;
                    }
                    if (MeasurementType == "MEAN")
                    {

                    }
                    if (MeasurementType == "RMS")
                    {
                        viMeasureType = 23;
                    }
                    if (MeasurementType == "PER")
                    {
                        viMeasureType = 17;
                    }
                    if (MeasurementType == "FREQ")
                    {
                        viMeasureType = 10;
                    }
                    if (MeasurementType == "DUTY")
                    {
                        viMeasureType = 7;
                    }
                    if (MeasurementType == "RISE")
                    {
                        viMeasureType = 22;
                    }
                    if (MeasurementType == "FALL")
                    {
                        viMeasureType = 9;
                    }
                    string[] tbMeasureTypes = CS_MEASURE_TYPE.Split('|');
                    string strCurMeasureType = tbMeasureTypes[viMeasureType];
                    if (!string.IsNullOrEmpty(strCurMeasureType))
                    {
                        string strTmp = string.Empty;
                        bool bRet = Query($":MEASure:ITEM{strCurMeasureType}? CHANnel{viChannel}", ref strTmp); // 查询测量项值
                        if (bRet && (!strTmp.Contains("*")))
                        {
                            try
                            {
                                decimal readVaule = decimal.Parse(strTmp, System.Globalization.NumberStyles.Float);
                                if (MeasurementType == "DUTY")
                                    readVaule *= 100.0m;
                                value = readVaule.ToString("F3");
                            }
                            catch
                            {
                                value = "-10000";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    value = "";
                    SendExMsg(ex);
                }
                finally { this.AutoReadData = true; }
            }
        }
        /// <summary>
        /// 按类型读取测量值
        /// </summary>
        /// <param name="MeasureType">测量值类型</param>
        /// <param name="channel">通道号(1,2)</param>
        /// <returns>返回测量的值<</returns>
        public override void Oscilloscope_ReadMeasure(string MeasureType, int channel, ref string value, bool isIMMed = false)
        {
            lock (SynLock)
            {
                this.AutoReadData = false;
                Thread.Sleep(500);
                int viMeasureType = 0;
                int viChannel = 1;
                string MeasurementType = MeasureType;
                viChannel = channel;
                if (MeasurementType == "PKPK")
                {
                    viMeasureType = 20;
                }
                if (MeasurementType == "MAX")
                {
                    viMeasureType = 13;
                }
                if (MeasurementType == "MIN")
                {
                    viMeasureType = 14;
                }
                if (MeasurementType == "TOP")
                {
                    viMeasureType = 11;
                }
                if (MeasurementType == "BASE")
                {
                    viMeasureType = 12;
                }
                if (MeasurementType == "MEAN")
                {

                }
                if (MeasurementType == "RMS")
                {
                    viMeasureType = 23;
                }
                if (MeasurementType == "PER")
                {
                    viMeasureType = 17;
                }
                if (MeasurementType == "FREQ")
                {
                    viMeasureType = 10;
                }
                if (MeasurementType == "DUTY")
                {
                    viMeasureType = 7;
                }
                if (MeasurementType == "RISE")
                {
                    viMeasureType = 22;
                }
                if (MeasurementType == "FALL")
                {
                    viMeasureType = 9;
                }

                try
                {
                    string[] tbMeasureTypes = CS_MEASURE_TYPE.Split('|');
                    string strCurMeasureType = tbMeasureTypes[viMeasureType];
                    if (!string.IsNullOrEmpty(strCurMeasureType))
                    {
                        string strTmp = string.Empty;
                        bool bRet = Query($":MEASure:{strCurMeasureType}? CHANnel{viChannel}", ref strTmp); // 查询测量项值
                        if (bRet && (!strTmp.Contains("*")))
                        {
                            try
                            {
                                decimal readVaule = decimal.Parse(strTmp, System.Globalization.NumberStyles.Float);
                                if (MeasurementType == "DUTY")
                                    readVaule *= 100.0m;
                                value = readVaule.ToString("F3");
                            }
                            catch
                            {
                                value = "-10000";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    value = "";
                    SendExMsg(ex);
                }
                finally { this.AutoReadData = true; }

            }
        }
        /// <summary>
        /// 示波器时基设置
        /// </summary>
        /// <param name="isroll">是否滚动</param>
        /// <param name="timebase">时基（200、400，暂定ms为单位）</param>
        /// <param name="delay"> (0、1，暂定s为单位，默认0)</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscilloscope_TimeBase(bool isroll, string timebase, string delay)
        {
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;
                    int viRollMode = isroll ? 1 : 0;
                    float vfTimebase = (float)Convert.ToDouble(timebase);
                    float vfDelay = (float)Convert.ToDouble(timebase);

                    this.AutoReadData = false;
                    List<string> listCommands = new List<string>()
                    {
                        ":ACQuire:POINts:AUTO ON", // 自动根据频域数据自动确定采样点数
                        ":TIMebase:MODE MAIN", // 平均值模式，测量值不会显示跳动
                        ":ACQuire:TYPE PEAK",
                        string.Format(":ACQuire:SRATe:AUTO {0}", viRollMode==1?"ON":"OFF"),
                        string.Format(":TIMebase:SCALe {0}", (vfTimebase/1000).ToString("e")),// 设置时基，传进来的是毫秒，之前的指令带了MS,最多到1us
                        string.Format(":TIMebase:DELay:OFFSet {0}", (vfDelay/1000).ToString("e")),
                    };
                    WriteMultipleString(listCommands);

                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
                finally { this.AutoReadData = true; }
            }
        }



        /// <summary>
        /// 示波器触发
        /// </summary>
        /// <param name="type_first">主类型(边沿或者超时,0为边沿，1为超时，2欠幅或矮脉冲，以后再加要用到的定义 )</param>
        /// <param name="type_second">次类型(上升沿或者下降沿,RISE,FALL,Alternating)</param>
        /// <param name="coupling">触发耦合(默认直流,AC,DC)</param>
        /// <param name="timeout_type">超时类型(一般默认边沿,EDGE、STATe为状态)</param>
        /// <param name="timeout">超时时间(ms为单位，配合超时触发用)，type_first为2时候为触发下限电平，单位为V</param>
        /// <param name="channel">通道(1、2)</param>
        /// <param name="triggerLevel">触发电平(0mV,1V,2V)</param>
        /// <param name="triggerType">触发类型(Auto、Normal、Single)(自动、普通、单次)</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscilloscope_Trigger(int type_first, string type_second, string coupling, string timeout_type, string timeout, int channel, string triggerLevel, string triggerType)
        {
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;
                    int viMode, viMainType, viSubType, viCoupling, viTimeoutType;
                    float vfTimeout, vfLower, vfLevelOrUpper;

                    viMainType = type_first;
                    viSubType = 0;
                    if (type_second == "FALL")
                    {
                        viSubType = 1;
                    }
                    else if (type_second == "Alternating")
                    {
                        viSubType = 2;
                    }
                    viMode = 0;
                    if (triggerType == "Normal")
                    {
                        viMode = 1;
                    }
                    else if (triggerType == "Single")
                    {
                        viMode = 2;
                    }
                    viCoupling = 0;
                    if (coupling == "DC")
                    {
                        viCoupling = 1;
                    }
                    viTimeoutType = 0;
                    if (timeout_type == "STATe")
                    {
                        viTimeoutType = 1;
                    }
                    (vfTimeout, vfLower) = (10, 10);
                    if (timeout != "")
                    {
                        vfTimeout = (float)Convert.ToDouble(timeout);
                    }
                    vfLevelOrUpper = 10;
                    if (triggerLevel != "")
                    {
                        vfLower = (float)Convert.ToDouble(triggerLevel.Replace("V", "").Replace("m", ""));
                    }

                    string[] stbModes = CS_TRIGGER_MODE.Split('|');
                    string[] stbTrigTypes = CS_TRIGGER_TYPE.Split('|');
                    string[] stbSlopes = CS_TRIGGER_SLOPE.Split('|');
                    string strTrigType = stbTrigTypes[viMainType];
                    List<string> listCommands = new List<string>()
                    {
                        ":STOP",
                        string.Format(":TRIGger:MODE {0}", strTrigType), // 设置触发模式
                        string.Format(":TRIGger:SWEep {0}", stbModes[viMode]), // 设置触发类型
                        string.Format("TRIGger:{0}:SOURce CHANnel{1}", strTrigType, channel), // 设置source
                        string.Format("TRIGger:{0}:COUPling {1}", strTrigType, viCoupling == 0 ? "AC" : "DC"), // 设置耦合
                        string.Format("TRIGger:{0}:SLOPe {1}", strTrigType, stbSlopes[viSubType]),
                        string.Format("TRIGger:{0}:LEVel {1}", strTrigType, vfLevelOrUpper)
                    };
                    
                    //超时模式
                    if (viMainType == 1)
                    {
                        listCommands.Add(string.Format("TRIGger:{0}:SLOPe {1}", strTrigType, stbSlopes));
                        listCommands.Add(string.Format("TRIGger:{0}:TIME {1}", strTrigType, vfTimeout));
                        listCommands.Add(string.Format("TRIGger:{0}:LEVel {1}", strTrigType, vfLower));
                    }
                    // 欠幅模式
                    if (viMainType == 2)
                    {
                        listCommands.Add(string.Format("TRIGger:{0}:POLarity {1}", strTrigType, stbSlopes));
                        if (Convert.ToDouble(timeout) <= Convert.ToDouble(triggerLevel))
                        {
                            listCommands.Add(string.Format("TRIGger:{0}:ALEVel {1}", strTrigType, vfLower));//电平上限
                            listCommands.Add(string.Format("TRIGger:{0}:BLEVel {1}", strTrigType, vfTimeout));//电平下限
                        }
                        else
                        {
                            listCommands.Add(string.Format("TRIGger:{0}:ALEVel {1}", strTrigType, vfTimeout));//电平上限
                            listCommands.Add(string.Format("TRIGger:{0}:BLEVel {1}", strTrigType, vfLower));//电平下限
                        }
                    }

                    if (viMode == 2)
                    {
                        listCommands.Add(":SINGle");
                    }

                    if (viMode == 0)
                    {
                        listCommands.Add(":RUN");
                    }
                    WriteMultipleString(listCommands);
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
                finally { this.AutoReadData = true; }
            }
        }

        /// <summary>
        /// 示波器触发类型设置
        /// </summary>
        /// <param name="TriggerType">触发类型(Auto、Normal、Single)(自动、普通、单次)</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscilloscope_TriggerTypeSet(string TriggerType)
        {
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;
                    int viMode = 0;
                    if (TriggerType == "Normal")
                    {
                        viMode = 1;
                    }
                    else if (TriggerType == "Single")
                    {
                        viMode = 2;
                    }
                    string[] stbModes = CS_TRIGGER_MODE.Split('|');
                    List<string> listCommands = new List<string>()
                    {
                        string.Format(":TRIGger:SWEep {0}", stbModes[viMode]), // 设置触发模式
                    };
                    if (viMode != 0)
                    {
                        listCommands.Add(":RUN");
                    }
                    WriteMultipleString(listCommands);
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
                finally { this.AutoReadData = true; }
            }
        }
        /// <summary>
        /// 触发状态回读
        /// </summary>
        /// <returns>返回触发状态,0为stop,1为等待触发,-1为错误,2为其他状态</returns>
        public override int Oscilloscope_ReadTrigger()
        {
            lock (SynLock)
            {
                int TriggleType = -1;
                try
                {
                    this.AutoReadData = false;
                    string rstrState = string.Empty;
                    if (Query(":TRIGger:STATus?", ref rstrState))
                    {
                        string[] strings = CS_TRIGGER_STATE.Split('|');
                        rstrState = rstrState.Trim().Replace("\n", "");
                        TriggleType = Array.IndexOf(strings, rstrState);
                    }
                    return TriggleType;
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                    return -1;
                }
                finally { this.AutoReadData = true; }
            }
        }
        /// <summary>
        /// 波形数据回读
        /// </summary>
        /// <param name="channnel"> 通道(1、2)</param>
        /// <returns>返回为波形数据</returns>
        public override void OscilloscopeCursorData(int channnel, ref double[] rtdBuffer, int RecondLengZero = 10000 * 20)
        {
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;
                    string strTmp = string.Empty;

                    //":DIGitize CHAN1"   ' 采集
                    //":WAVeform:SOURce CHAN1"
                    //":WAVeform:FORMAT BYTE"
                    //":WAVeform:DATA?"  ' 请求数据

                    List<string> listCommands = new List<string>()
                    {
                        ":STOP",
                        $":WAVeform:SOUR CHAN{channnel}",
                        $":WAVeform:MODE RAW",
                        ":WAVeform:FORMat ASCii",
                        ":WAVeform:DATA?",
                    };
                    WriteMultipleString(listCommands);

                    string btWaveDataTemp = string.Empty;
                    ReadString(ref btWaveDataTemp);

                    // 获取偏置
                    Query(":WAVeform:PREamble?", ref strTmp);
                    string[] listTmp = strTmp.Split(',');

                    //<format 16-bit NR1>,
                    //< type 16 - bit NR1 >,
                    //< points 32 - bit NR1 >,
                    //< count 32 - bit NR1 >,
                    //< xincrement 64 - bit floating point NR3 >,
                    //< xorigin 64 - bit floating point NR3 >,
                    //< xreference 32 - bit NR1 >,
                    //< yincrement 32 - bit floating point NR3 >,
                    //< yorigin 32 - bit floating point NR3 >,
                    //< yreference 32 - bit NR1 >
                    double YINCrement = Convert.ToDouble(ClsAlgorithm.ContentEDataChangeNum_D(listTmp[7].Trim().Replace("\n", "")));
                    double YORigin = Convert.ToDouble(ClsAlgorithm.ContentEDataChangeNum_D(listTmp[8].Trim().Replace("\n", "")));
                    double YREFerence = Convert.ToDouble(ClsAlgorithm.ContentEDataChangeNum_D(listTmp[9].Trim().Replace("\n", "")));
                    // 实际电压值 = (原始数据值 - YREFerence) × YINCrement + YORigin
                    // (lngDataValue - lngYReference) _* sngYIncrement + sngYOrigin)
                    string[] _WaveDataTbl = btWaveDataTemp.Trim().Substring(btWaveDataTemp.IndexOf('+')).Split(',');
                    rtdBuffer = new double[_WaveDataTbl.Length];
                    int index = 0;
                    foreach (var sDataTmp in _WaveDataTbl)
                    {
                        //double value1 = Convert.ToDouble(sDataTmp);
                        if (double.TryParse(sDataTmp, out double value1))
                            rtdBuffer[index++] = ((value1 - YREFerence) * YINCrement) + YORigin;
                        else
                        {

                        }
                    }
                }
                catch (Exception ex)
                {
                    rtdBuffer = null;
                    SendExMsg(ex);
                }
                finally { this.AutoReadData = true; }
            }

        }

        private int Function1(int channnel)
        {
            int tLength;
            EquipMentPort.VisaNS.Write("ACQuire:STATE STOP"); //停止运行
            System.Threading.Thread.Sleep(sleeptime);
            EquipMentPort.VisaNS.Write("CURVe Block");//设置回读读取格式
            System.Threading.Thread.Sleep(sleeptime);
            EquipMentPort.VisaNS.Write("DATa:SOUrce CH" + channnel);//设置回读波形长指定通道
            System.Threading.Thread.Sleep(sleeptime);
            // EquipMentPort.VisaNS.Write("CURVe WIDth1");//示波器返回的数据只保留值数据，其他数据删掉
            EquipMentPort.VisaNS.Write("DATa:WIDth1");//设置1个有效数据（1字节8位）
            System.Threading.Thread.Sleep(sleeptime);
            EquipMentPort.VisaNS.Write("WFMINPRE:NR_PT 200000");//设置记录长度
            System.Threading.Thread.Sleep(sleeptime);
            EquipMentPort.VisaNS.Write("WFMInpre:NR_Pt?");//
                                                          //System.Threading.Thread.Sleep(sleeptime);
            string tString = EquipMentPort.VisaNS.ReadString();
            tLength = (int)CommonOscilloscope.BackTryDecimalParse(tString);
            return tLength;
        }

        /// <summary>
        /// 截取示波器屏幕
        /// </summary>
        /// <returns>主要返回示波器截屏存储到电脑上的存储路径，一般是存储在程序运行
        /// 目录下的报表(勿删)\Image\年\月\年-月-日\生成的截屏名称.jpg
        /// 否则返回为try catch 的错误，没有则返回为空</returns>
        public override void OscilloscopeSaveScreen(ref string path)
        {
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;
                    List<string> listTmp = new List<string>()
                    {
                        ":STOP",
                        ":SAVE:IMAGe:TYPE BMP24",
                        ":SAVE:IMAGe:COLor COLor"
                    };
                    WriteMultipleString(listTmp);
                    WriteString(":SAVE:IMAGe:TYPE BMP24");

                    //// 4. 可选：查询当前设置（调试用）
                    //string strTmp = string.Empty;
                    //Query(":HARDcopy:DESTination?", ref rstrErrDescr);
                    //Console.WriteLine(strTmp);

                    // 5. 发送截图命令                    
                    WriteString(":DISPlay:DATA?");
                    System.Threading.Thread.Sleep(3000);
                    path = string.Format("{0}\\{1}.bmp", ImagePathFile, System.DateTime.Now.ToString("yyyy-MM-dd HH#mm#ss#ffff"));
                    if (!Directory.Exists(ImagePathFile))
                    {
                        Directory.CreateDirectory(ImagePathFile);
                    }

                    int Length = 20 * 1024 * 1024;
                    byte[] ImageData = EquipMentPort.VisaNS.ReadByteArray(Length);
                    string m_ImagrRootDir = AppDomain.CurrentDomain.BaseDirectory;
                    if (ImageData == null || ImageData.Length < 2)
                    {
                        path = "";
                        return;
                    }

                    // 在返回数据中查找 BMP 文件头（0x42 0x4D -> 'B' 'M'）
                    int startIndex = -1;
                    for (int i = 0; i < ImageData.Length - 1; i++)
                    {
                        if (ImageData[i] == 0x42 && ImageData[i + 1] == 0x4D)
                        {
                            startIndex = i;
                            break;
                        }
                    }

                    // 如果没找到 BMP 头，退回使用数组起始位置（保持兼容性）
                    if (startIndex < 0)
                    {
                        startIndex = 0;
                    }

                    int validLen = ImageData.Length - startIndex;
                    if (validLen <= 0)
                    {
                        path = "";
                        return;
                    }

                    // 从 startIndex 截取到末尾
                    byte[] bmpData = new byte[validLen];
                    Array.Copy(ImageData, startIndex, bmpData, 0, validLen);

                    // 使用截取后的数据构建图片并保存
                    using (var ms = new System.IO.MemoryStream(bmpData))
                    {
                        System.Drawing.Image img = System.Drawing.Image.FromStream(ms);

                        string imageName = ImagePathFile + System.DateTime.Now.ToString("yyyy-MM-dd HH：mm：ss：ffff") + ".bmp";
                        img.Save(System.IO.Path.Combine(ImagePathFile, imageName), System.Drawing.Imaging.ImageFormat.Bmp);
                        string restring = imageName.Substring(BaseDirectoryPath.Length);
                        System.Threading.Thread.Sleep(sleeptime * 10);
                        path = restring;
                    }
                }
                catch (Exception ex)
                {
                    path = "";
                    SendExMsg(ex);
                }
                finally { this.AutoReadData = true; }
            }
        }
        /// <summary>
        /// 示波器启停控制
        /// </summary>
        /// <param name="isRun">是否运行</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscilloscope_IsRun(bool isRun)
        {
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;
                    WriteString(!isRun ? ":STOP" : ":RUN");
                    //该示波器波形刷新慢，统一多等待4秒
                    if (isRun)
                        Thread.Sleep(4000);
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
                finally { this.AutoReadData = true; }
            }
        }
        /// <summary>
        /// 启停状态回读
        /// </summary>
        /// <returns>true运行，false没有运行</returns>
        public override void Oscilloscope_ReadRun(ref bool isRun)
        {
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;
                    int viState = 0;
                    string rstrState = string.Empty;
                    bool bRet = Query(":RSTate?", ref rstrState);
                    if (bRet)
                    {
                        rstrState = rstrState.Trim().Replace("\n", "").ToUpper();
                        if (rstrState == "STOP")
                        {
                            viState = 0;
                        }
                        else
                        {
                            viState = 1;
                        }
                    }
                    isRun = viState == 0 ? false : true;
                }
                catch (Exception ex)
                {
                    isRun = false;
                    SendExMsg(ex);
                }
                finally { this.AutoReadData = true; }
            }
        }
        /// <summary>
        /// 示波器光标类型设置
        /// </summary>
        /// <param name="type">参数为X、Y、XY</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscilloscope_CursorsSet(string type)
        {
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;
                    int viType = 0;
                    if (type == "Y")
                    {
                        viType = 1;
                    }
                    else if (type == "XY")
                    {
                        viType = 2;
                    }
                    List<string> listTmp = new List<string>()
                    {
                        ":MARKer:MODE MANual",
                        $":MARKer:X1:DISPlay {((viType == 0 || viType == 2) ? "ON" : "OFF")}",
                        $":MARKer:X2:DISPlay {((viType == 0 || viType == 2) ? "ON" : "OFF")}",
                        $":MARKer:Y1:DISPlay {((viType == 1 || viType == 2) ? "ON" : "OFF")}",
                        $":MARKer:Y2:DISPlay {((viType == 1 || viType == 2) ? "ON" : "OFF")}"
                    };

                    WriteMultipleString(listTmp);
                    System.Threading.Thread.Sleep(500);
                    if (viType == 0 || viType == 2)
                    {
                        Oscilloscope_CursorPosition_X(1, 0, 100);
                    }

                    if (viType == 1 || viType == 2)
                    {
                        Oscilloscope_CursorPosition_X(1, 0, 100);
                    }
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
                finally { this.AutoReadData = true; }
            }
        }
        /// <summary>
        /// 示波器横坐标卡点
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="value1">左边点1(0到100的比例，屏幕占比)</param>
        /// <param name="value2">右边点2(0到100的比例，屏幕占比)</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscilloscope_CursorPosition_X(int channel, double vfLeftPoint, double vfRightPoint)
        {
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;

                    string strTmp = string.Empty;
                    Query(string.Format(":TIMebase:SCALe?"), ref strTmp);
                    double Scale = (double)decimal.Parse(strTmp, System.Globalization.NumberStyles.Float);

                    Query(string.Format(":TIMebase:POSition?"), ref strTmp);
                    double Delay = (double)decimal.Parse(strTmp, System.Globalization.NumberStyles.Float);

                    double xCurrsorPos = (Scale * CI_CURSSOR_HNUM);
                    double xCurrsorPos1 = xCurrsorPos * ((vfLeftPoint / 100) - 0.5) + Delay;
                    double xCurrsorPos2 = xCurrsorPos * ((vfRightPoint / 100) - 0.5) + Delay;

                    List<string> listCommands = new List<string>()
                    {
                        $":MARKer:X1Y1source CHANnel{channel}",
                        $":MARKer:X2Y2source CHANnel{channel}",
                        $":MARKer:X1Position {xCurrsorPos1}",
                        $":MARKer:X2Position {xCurrsorPos2}",
                    };

                    WriteMultipleString(listCommands);
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
                finally { this.AutoReadData = true; }
            }
        }
        /// <summary>
        /// 示波器卡点单一
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="value">(0到1的比例，屏幕占比)</param>
        /// <param name="isleft">是否为左边</param>
        /// <returns></returns>
        public override void Oscilloscope_CursorPosition_X_Single(int channel, double value, bool isleft)
        {
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;
                    EquipMentPort.VisaNS.Write("CURSor:MODE MANual");       //手动模式
                    System.Threading.Thread.Sleep(sleeptime);
                    EquipMentPort.VisaNS.Write("CURSor:MANual:TYPE TIME");  //设置X光标
                    System.Threading.Thread.Sleep(sleeptime);
                    EquipMentPort.VisaNS.Write("CURSor:MANual:SOURce CHANnel" + channel);
                    System.Threading.Thread.Sleep(sleeptime);
                    EquipMentPort.VisaNS.Write("CURSor:MANual:TUNit SECond");//测量结果中的 AX、BX 和△X 以“秒”为单位，1/△X 以“赫兹”为单位
                    System.Threading.Thread.Sleep(sleeptime);
                    //水平方向的像素范围为0至999
                    int temp = Math.Min(999, Convert.ToInt32(value * 1000));
                    if (isleft)
                    {
                        EquipMentPort.VisaNS.Write("CURSor:MANual:CAX " + temp);
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    else
                    {
                        EquipMentPort.VisaNS.Write("CURSor:MANual:CBX " + temp);
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                }
                catch (Exception ex) { SendExMsg(ex); }
                finally { this.AutoReadData = true; }
            }
        }

        /// <summary>
        /// 示波器卡点按时间单一
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="time">(0到100的比例，屏幕占比)</param>
        /// <param name="isleft">是否为左边</param>
        /// <returns></returns>
        public override void Oscilloscope_CursorPosition_X_Time(int channel, double time, bool isleft)
        {

            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;
                    int viLeftOrRight = isleft ? 1 : 2;

                    List<string> listCommands = new List<string>()
                    {
                        $":MARKer:X1Y1source CHANnel{channel}",
                        $":MARKer:X{viLeftOrRight}Position {time}",
                    };

                    WriteMultipleString(listCommands);
                }
                catch (Exception ex) { SendExMsg(ex); }
                finally { this.AutoReadData = true; }
            }
        }
        /// <summary>
        /// 示波器纵坐标卡点
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="value1">下边点1(0到100的比例，屏幕占比)</param>
        /// <param name="value2">上边点2(0到100的比例，屏幕占比)</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscilloscope_CursorPosition_Y(int channel, double vfLeftPoint, double vfRightPoint)
        {
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;

                    string strTmp = string.Empty;
                    Query($":CHANnel{channel}:SCALe?", ref strTmp);
                    double eachVoltage = (double)decimal.Parse(strTmp, System.Globalization.NumberStyles.Float);

                    Query($":CHANnel{channel}:OFFSet?", ref strTmp);
                    double offset = (double)decimal.Parse(strTmp, System.Globalization.NumberStyles.Float);

                    double xCurrsorPos = (double)(eachVoltage * CI_CURSSOR_VNUM);
                    vfLeftPoint = (float)(xCurrsorPos * ((vfLeftPoint / 100) - 0.5) + offset);
                    vfRightPoint = (float)(xCurrsorPos * ((vfRightPoint / 100) - 0.5) + offset);

                    List<string> listCommands = new List<string>()
                    {
                        $":MARKer:X1Y1source CHANnel{channel}",
                        $":MARKer:X2Y2source CHANnel{channel}",
                        $":MARKer:Y1Position {vfLeftPoint}",
                        $":MARKer:Y2Position {vfRightPoint}",
                    };

                    WriteMultipleString(listCommands);
                }
                catch (Exception ex) { SendExMsg(ex); }
                finally { this.AutoReadData = true; }
            }
        }
        /// <summary>
        /// 回读光标参数
        /// </summary>
        /// <param name="channel">通道(1、2、3、4等)</param>
        /// <returns>返回double[5]。第一个为时间差,，第二个为1/时间差=频率单位为Hz，第三个为横坐标左边值，第四个为横坐标右边值，第五个为幅度差。时间单位为ms</returns>
        public override void Oscilloscope_ReadCursors(int channel, ref double[] Cursors)
        {
            double[] returnvalue = new double[5];
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;
                    float rfTime = 0, rfTimeFreq = 0, rfLeftPoint = 0, rfRightPoint = 0, rfDeltaLR = 0;

                    WriteString(string.Format("CURSOR:SELECT:SOURCE CH{0}", channel));
                    List<string> listCommands = new List<string>()
                    {
                        ":MARKer:XDELta?",
                        ":MARKer:X1Position?",
                        ":MARKer:X2Position?",
                        ":MARKer:Y1Position?",
                        ":MARKer:Y2Position?",
                    };
                    string[] rtbStrings = new string[5];
                    if (QueryMultiple(listCommands, ref rtbStrings))
                    {
                        rfTime = (float)Convert.ToDouble(rtbStrings[0]) * 1000;
                        if (rfTime != 0)
                        {
                            rfTimeFreq = (float)(1 / rfTime);
                        }
                        else
                        {
                            rfTimeFreq = 0;
                        }
                        rfLeftPoint = (float)ClsAlgorithm.ContentEDataChangeDec(rtbStrings[1]);
                        rfRightPoint = (float)ClsAlgorithm.ContentEDataChangeDec(rtbStrings[2]);
                        rfDeltaLR = Math.Abs((float)ClsAlgorithm.ContentEDataChangeDec(rtbStrings[3]) - (float)ClsAlgorithm.ContentEDataChangeDec(rtbStrings[4]));
                    }

                    returnvalue[0] = rfTime;
                    returnvalue[1] = rfTimeFreq;
                    returnvalue[2] = rfLeftPoint;
                    returnvalue[3] = rfRightPoint;
                    returnvalue[4] = rfDeltaLR;
                    Cursors = returnvalue;
                }
                catch (Exception ex)
                {
                    Cursors = null;
                    SendExMsg(ex);
                }
                finally { this.AutoReadData = true; }
            }
        }
        /// <summary>
        /// 控制示波器是否能触屏
        /// </summary>
        /// <param name="isTouch">能否触屏</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscilloscope_Touch(bool isTouch)
        {
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;
                    WriteString($":SYSTem:TOUCh {(isTouch ? "OFF" : "ON")}");
                }
                catch (Exception ex) { SendExMsg(ex); }
                finally { this.AutoReadData = true; }
            }
        }

        /// <summary>
        /// 示波器时基设置
        /// </summary>
        /// <param name="isroll">是否滚动</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscilloscope_IsRoll(bool isroll)
        {
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;
                    WriteString(isroll ? ":STOP" : ":RUN");
                }
                catch (Exception ex) { SendExMsg(ex); }
                finally { this.AutoReadData = true; }
            }
        }
        /// <summary>
        /// 设置示波器最大存储深度
        /// </summary>
        /// <param name="storagedepth">存储深度，目前已有参数10k、50k、250k、1.25M、2.5M、12.5M、25M、125M、250M</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscilloscope_StorageDepth(string storagedepth)
        {
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;
                    double value = 1000;
                    float vfDepth = 0;
                    if (storagedepth.Contains("k"))
                    {
                        storagedepth = storagedepth.Replace("k", "");
                        storagedepth = storagedepth.Replace("[^0-9]", "");
                        value = Convert.ToDouble(storagedepth) / 1000;
                    }
                    if (storagedepth.Contains("M"))
                    {
                        storagedepth = storagedepth.Replace("M", "");
                        storagedepth = storagedepth.Replace("[^0-9]", "");
                        value = Convert.ToDouble(storagedepth);
                    }
                    vfDepth = (float)value;
                    // 自动根据时基处理存储深度
                    WriteString(":ACQuire:POINts:AUTO ON");
                }
                catch (Exception ex) { SendExMsg(ex); }
                finally { this.AutoReadData = true; }
            }
        }



        /// <summary>
        /// 计算卡点值
        /// </summary>
        /// <param name="data">回读的示波器的点</param>
        /// <param name="upvalue">上升或下降的高值</param>
        /// <param name="downvalue"> 上升或下降的低值</param>
        /// <param name="uptype">0为上升，1为下降</param>
        /// <param name="isAC">是不是交流</param>
        /// <returns>返回double[2]，返回占屏幕的比例值,跟示波器没有关系，自己根据示波器回读的点来进行计算</returns>
        public override void Oscilloscope_Points(double[] data, double upvalue, double downvalue, int uptype, bool isAC, ref double[] position)
        {
            lock (SynLock)
            {
                if (isAC)
                {
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = System.Math.Abs(data[i]);
                    }
                }
                double[] returnvalue = new double[2];//返回的值，按屏幕的百分比来算
                try
                {
                    this.AutoReadData = false;

                    data = filtering.MedianFilter(data, 10);//中值滤波算法滤掉毛刺
                    double Record1 = 0;
                    double Record2 = 0;
                    int index1 = 0;
                    int count1 = 0;
                    int index2 = 0;
                    int count2 = 0;
                    if (uptype == 0)
                    {
                        foreach (double value in data)
                        {

                            if (value > downvalue + 1)//最大值
                            {

                                if (++count1 > 5000)// 如果连续大于300
                                {
                                    count1 = 0;
                                    Record1 = index1 - 4999;
                                    break;

                                }

                            }
                            else
                            {
                                count1 = 0;

                            }
                            index1++;
                        }
                        foreach (double value in data)
                        {

                            if (value > upvalue - 1)//最大值
                            {

                                if (++count2 > 5000)// 
                                {
                                    count2 = 0;
                                    Record2 = index2 - 4999;
                                    break;

                                }

                            }
                            else
                            {
                                count2 = 0;
                            }
                            index2++;
                        }



                    }
                    if (uptype == 1)
                    {
                        foreach (double value in data)
                        {

                            if (value > upvalue - 1)//最大值
                            {



                            }
                            else
                            {
                                if (++count1 > 10000)// 
                                {
                                    count1 = 0;
                                    Record1 = index1 - 4999;
                                    break;

                                }

                            }
                            index1++;
                        }
                        foreach (double value in data)
                        {

                            if (value > downvalue - 1)//最大值
                            {



                            }
                            else
                            {
                                if (++count2 > 10000)// 
                                {
                                    count2 = 0;
                                    Record2 = index2 - 9999;
                                    break;

                                }
                            }
                            index2++;
                        }
                    }
                    if (DataLen >= data.Length)
                    {
                        int value = DataLen - data.Length;
                        returnvalue[0] = (Record1 + value) / DataLen;//左边值
                        returnvalue[1] = (Record2 + value) / DataLen;//右边值
                        position = returnvalue;
                    }
                    else
                    {
                        returnvalue[0] = Record1 / data.Length;//左边值
                        returnvalue[1] = Record2 / data.Length;//右边值
                        position = returnvalue;
                    }
                    position = returnvalue;
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
                finally { this.AutoReadData = true; }

            }

        }






        /// <summary>
        /// 计算卡点值单一
        /// </summary>
        /// <param name="data">回读的示波器的点</param>
        /// <param name="value">上升或下降的比较值</param>
        /// <param name="uptype">0为上升，1为下降</param>
        /// <param name="isright"> 卡的点是否为右边</param>
        /// <param name="isAC">是否为交流</param>
        /// <returns>返回double，返回占屏幕的比例值，跟示波器没有关系，自己根据示波器回读的点来进行计算</returns>
        public override void Oscilloscope_Points_Single(double[] data, double value, int uptype, bool isright, bool isAC, ref double position)
        {
            lock (SynLock)
            {
                if (isAC)
                {
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = System.Math.Abs(data[i]);
                    }
                }

                try
                {
                    this.AutoReadData = false;

                    data = filtering.MedianFilter(data, 10);//中值滤波算法滤掉毛刺
                    double Record1 = 0;
                    int index1 = 0;
                    int count1 = 0;
                    if (uptype == 0)
                    {
                        foreach (double dvalue in data)
                        {

                            if (dvalue > value + 1)//最大值
                            {

                                if (++count1 > 5000)// 如果连续大于300
                                {
                                    count1 = 0;
                                    Record1 = index1 - 4999;
                                    break;
                                }

                            }
                            else
                            {
                                count1 = 0;

                            }
                            index1++;
                        }

                    }
                    if (uptype == 1)
                    {
                        foreach (double dvalue in data)
                        {

                            if (dvalue > value - 1)//最大值
                            {



                            }
                            else
                            {
                                if (++count1 > 1000)// 
                                {
                                    count1 = 0;
                                    Record1 = index1 - 999;
                                    break;

                                }

                            }
                            index1++;
                        }
                    }
                    double returnvalue;
                    //TestData.Log.Write("Record1" + Record1);
                    if (DataLen >= data.Length)
                    {
                        int dvalue = DataLen - data.Length;
                        returnvalue = (Record1 + dvalue) / DataLen;//左边值
                        position = returnvalue;
                    }
                    else
                    {
                        returnvalue = Record1 / data.Length;//左边值

                        position = returnvalue;
                    }
                    position = returnvalue;
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
                finally { this.AutoReadData = true; }

            }

        }

        /// <summary>
        /// 计算卡点值单一
        /// </summary>
        /// <param name="data">回读的示波器的点</param>
        /// <param name="value">上升或下降的比较值</param>
        /// <param name="uptype">0为上升，1为下降</param>
        /// <param name="isright"> 卡的点是否为右边</param>
        /// <param name="isAC">是否为交流</param>
        /// <param name="count">连续多少个点</param>
        /// <returns>返回double，返回占屏幕的比例值，跟示波器没有关系，自己根据示波器回读的点来进行计算</returns>
        public override void Oscilloscope_Points_Single_AC(double[] data, double value, int uptype, bool isright, bool isAC, int count, ref double position)
        {
            lock (SynLock)
            {
                if (isAC)
                {
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = System.Math.Abs(data[i]);
                    }
                }
                double returnvalue = 0;//返回的值，按屏幕的百分比来算
                try
                {
                    this.AutoReadData = false;

                    data = filtering.MedianFilter(data, 10);//中值滤波算法滤掉毛刺
                    double Record1 = 0;
                    int index1 = 0;
                    int count1 = 0;
                    if (uptype == 0)
                    {
                        foreach (double dvalue in data)
                        {

                            if (dvalue > value + 1)//最大值
                            {

                                if (++count1 > 500)// 如果连续大于300
                                {
                                    count1 = 0;
                                    Record1 = index1 - 499;
                                    break;

                                }

                            }
                            else
                            {
                                count1 = 0;

                            }
                            index1++;
                        }

                    }
                    if (uptype == 1)
                    {
                        foreach (double dvalue in data)
                        {

                            if (dvalue > value - 1)//最大值
                            {



                            }
                            else
                            {
                                if (++count1 > 500)// 
                                {
                                    count1 = 0;
                                    Record1 = index1 - 499;
                                    break;

                                }

                            }
                            index1++;
                        }
                    }
                    //TestData.Log.Write("Record1" + Record1);
                    if (DataLen >= data.Length)
                    {
                        int dvalue = DataLen - data.Length;
                        returnvalue = (Record1 + dvalue) / DataLen;//左边值
                        position = returnvalue;
                    }
                    else
                    {
                        returnvalue = Record1 / data.Length;//左边值

                        position = returnvalue;
                    }
                    position = returnvalue;
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
                finally { this.AutoReadData = true; }
            }
        }

        /// <summary>
        /// Convert Byte[] to Image
        /// </summary>
        /// <param name = "buffer" ></ param >
        /// < returns ></ returns >
        public static Image BytesToImage(byte[] buffer)
        {
            MemoryStream ms = new MemoryStream(buffer);
            Image image = System.Drawing.Image.FromStream(ms);
            return image;
        }
        public string CreateImageFromBytes(string fileName, byte[] buffer)
        {
            string file = fileName;
            System.Drawing.Image image = BytesToImage(buffer);
            ImageFormat format = image.RawFormat;
            if (format.Equals(ImageFormat.Jpeg))
            {
                file += ".jpeg";
            }
            else if (format.Equals(ImageFormat.Png))
            {
                file += ".png";
            }
            else if (format.Equals(ImageFormat.Bmp))
            {
                file += ".bmp";
            }
            else if (format.Equals(ImageFormat.Gif))
            {
                file += ".gif";
            }
            else if (format.Equals(ImageFormat.Icon))
            {
                file += ".icon";
            }
            System.IO.FileInfo info = new System.IO.FileInfo(file);
            System.IO.Directory.CreateDirectory(info.Directory.FullName);
            File.WriteAllBytes(file, buffer);
            return file;
        }




        public override void Read_OscilloscopeState()
        {
            SystemEvent.SendConnectState(false, this);
            TCPClient tcp = EquipMentPort as TCPClient;
            while (true)
            {
                lock (SynLock)
                {
                    if (EquipMentPort != null)
                    {
                        if (tcp.blConntOk)
                        {
                            try
                            {

                                if (this.AutoReadData)
                                {
                                    //这里读取ID
                                    tcp.VisaNS.Write("*IDN?");
                                    //System.Threading.Thread.Sleep(50);
                                    tcp.VisaNS.ReadString();
                                    //string ReturnString = tcp.VisaNS.ReadString().Split(' ')[0];
                                    SystemEvent.SendConnectState(true, this);
                                }

                            }
                            catch
                            {
                                tcp.blConntOk = false;
                                SystemEvent.SendConnectState(false, this);
                            }
                        }
                        else
                        {
                            try
                            {

                                if (this.AutoReadData)
                                {
                                    string tPort;
                                    if (EquipMentPort.Ipaddress.Contains("."))
                                        tPort = "TCPIP::" + EquipMentPort.Ipaddress + "::INSTR";
                                    else
                                        tPort = EquipMentPort.Ipaddress;
                                    tcp.VisaNS = (MessageBasedSession)ResourceManager.GetLocalManager().Open(tPort);
                                    tcp.blConntOk = true;
                                    SystemEvent.SendConnectState(true, this);
                                }

                            }
                            catch (Exception ex)
                            {
                                // Log.Log.LogException(ex); //网络断开或者IP不对才会报这个错，UI会显示断开，不用写日志
                                tcp.blConntOk = false;
                                SystemEvent.SendConnectState(false, this);
                            }
                        }


                    }
                }
                Thread.Sleep(300);
            }
        }

        public override void ReadTriggerState(ref bool isTrigger)
        {
            isTrigger = false;
            lock (SynLock)
            {
                try
                {
                    int TriggleType = -1;
                    string rstrState = string.Empty;
                    if (Query(":RSTate?", ref rstrState))
                    {
                        string[] strings = CS_TRIGGER_STATE.Split('|');
                        rstrState = rstrState.Trim().Replace("\n", "");
                        TriggleType = Array.IndexOf(strings, rstrState);
                    }

                    if (TriggleType == 0)
                    {
                        isTrigger = true;
                    }
                    else
                    {
                        isTrigger = false;
                    }

                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
                finally { this.AutoReadData = true; }
            }
        }

        public string RemoveNonNumericCharacters(string inputString)
        {
            string resultString = "";

            for (int i = 0; i < inputString.Length; i++)
            {
                if (Char.IsDigit(inputString[i]))
                {
                    resultString += inputString[i];
                }
            }

            return resultString;
        }

        /// <summary>
        /// 设置通道自动调整
        /// </summary>
        /// <param name="channel">通道</param>
        /// <returns></returns>
        public override void Oscilloscope_AutoZero(int channel)
        {
            lock (SynLock)
            {
                try
                {

                }
                catch (Exception ex) { SendExMsg(ex); }
                finally { this.AutoReadData = true; }
            }
        }

    }
}
