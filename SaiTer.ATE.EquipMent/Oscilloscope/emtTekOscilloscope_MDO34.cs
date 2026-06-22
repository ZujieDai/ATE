using NationalInstruments.VisaNS;
using SaiTer.ATE.InterFace;
using SaiTer.ATE.PortManage.PortType;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Drawing;
using System.Drawing.Imaging;
using static System.Windows.Forms.AxHost;
using System.Collections;
using System.Runtime.InteropServices;
using SaiTer.ATE.DataModel;
using System.Runtime.Remoting.Channels;
using System.Windows.Input;
using NPOI.POIFS.Crypt;
using System.Configuration;
using SaiTer.ATE.DataModel.EnumModel;
using NPOI.SS.Formula.Functions;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 设备-泰克示波器
    /// </summary>
    public class emtTekOscilloscope_MDO34 : EquipMentBase
    {
        private string Customer;
        private static object SynLock = new object();
        //List<(int, int, string)> Measures = new List<(int, int, string)>();
        List<Measure> Measures = new List<Measure>();
        public class Measure
        {
            public int index { get; set; }
            public int channnel { get; set; }
            public string MeasureType { get; set; }
            public Measure(int a, int b, string c)
            {
                index = a;
                channnel = b;
                MeasureType = c;
            }
        }

        int DataLen = 100;
        /// <summary>
        /// 命令延时时间ms
        /// </summary>
        Filtering filtering = new Filtering();
        int sleeptime = 100;
        public emtTekOscilloscope_MDO34(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = "TEK MDO34" + " " + LanguageManager.GetByKey("示波器");
            SetImagePath();
            Customer = ConfigurationManager.AppSettings["Customer"];
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
                    EquipMentPort.VisaNS.Write("ACQuire:MODe SAMple");//采集模式：采样
                    System.Threading.Thread.Sleep(sleeptime);
                    EquipMentPort.VisaNS.Write("*RST");
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
        /// 示波器通道设置
        /// </summary>
        /// <param name="channel">通道(比如1、2、3)</param>
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
        public override void Oscilloscope_Channel_Set(int channel, bool isOpen, string coupling, string tapewidth, string probe, string tag, string impedance, string unit, bool isOpen_A, string gear, string position)
        {
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;
                    if (EquipMentPort.VisaNS != null)
                    {
                        if (isOpen)
                        {

                            EquipMentPort.VisaNS.Write("SELect:CH" + channel + " ON");//打开通道  ✔
                            System.Threading.Thread.Sleep(sleeptime);
                            if (coupling == "AC")
                            {
                                EquipMentPort.VisaNS.Write("CH" + channel + ":COUPling AC");//设置耦合 ✔
                            }
                            if (coupling == "DC")
                            {
                                EquipMentPort.VisaNS.Write("CH" + channel + ":COUPling DC");//设置耦合✔
                            }
                            System.Threading.Thread.Sleep(sleeptime);
                            if (tapewidth != "")
                            {
                                //tek示波器设置范围{TWEnty|FULl|<NR3>}，<NR3>is 1 to 20MHz
                                if (tapewidth == "0.25")
                                {
                                    tapewidth = "20";
                                }
                                else if (tapewidth == "FULL")
                                {
                                    tapewidth = "FULl";
                                }
                                EquipMentPort.VisaNS.Write("CH" + channel + ":BANDWIDTH " + tapewidth);//设置带宽 CH1:BANDWIDTH TWENTY
                                System.Threading.Thread.Sleep(sleeptime);
                            }

                            if (probe != "")
                            {
                                EquipMentPort.VisaNS.Write("CH" + channel + ":PROBe:GAIN " + 1.000 / Convert.ToDouble(probe));//设置探头比 CH1:PRObe:GAIN 1
                                System.Threading.Thread.Sleep(sleeptime);
                            }
                            if (tag != "")
                            {
                                EquipMentPort.VisaNS.Write("CH" + channel + ":LABel \"" + tag + "\"");//设置标签
                                System.Threading.Thread.Sleep(sleeptime);
                            }
                            if (impedance == "1M")
                            {
                                //EquipMentPort.VisaNS.Write("C" + channel + ":CPL A1M");//设置阻抗  C1:CPL A1M
                                //System.Threading.Thread.Sleep(sleeptime);
                            }
                            if (impedance == "50")
                            {

                                //EquipMentPort.VisaNS.Write("C" + channel + ":CPL A50");//设置阻抗
                                //System.Threading.Thread.Sleep(sleeptime);
                            }
                            if (unit != "")
                            {
                                EquipMentPort.VisaNS.Write(":CH" + channel + ":YUNit \"" + unit + "\"");//设置单位
                                System.Threading.Thread.Sleep(sleeptime);
                            }
                            if (isOpen_A)
                            {
                                EquipMentPort.VisaNS.Write(":CH" + channel + "INVert ON");//设置通道反相关是否开启
                                System.Threading.Thread.Sleep(sleeptime);
                            }
                            else
                            {
                                EquipMentPort.VisaNS.Write(":CH" + channel + "INVert OFF");//设置通道反相关是否开启
                                System.Threading.Thread.Sleep(sleeptime);
                            }
                            if (gear != "")
                            {
                                EquipMentPort.VisaNS.Write(":CH" + channel + ":VOLts " + gear);//设置通道档位
                                System.Threading.Thread.Sleep(sleeptime);
                            }
                            if (position != "")
                            {

                                EquipMentPort.VisaNS.Write(":CH" + channel + ":POSITION " + position);//设置通道位置
                                System.Threading.Thread.Sleep(sleeptime);
                            }
                        }
                        else
                        {
                            EquipMentPort.VisaNS.Write("SELect:CH" + channel + " OFF");//关闭通道
                            System.Threading.Thread.Sleep(sleeptime);
                        }
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
                    if (EquipMentPort.VisaNS != null)
                    {
                        this.AutoReadData = false;
                        EquipMentPort.VisaNS.Write("MEASUrement:MEAS1:STATE OFF");
                        System.Threading.Thread.Sleep(sleeptime);
                        EquipMentPort.VisaNS.Write("MEASUrement:MEAS2:STATE OFF");
                        System.Threading.Thread.Sleep(sleeptime);
                        EquipMentPort.VisaNS.Write("MEASUrement:MEAS3:STATE OFF");
                        System.Threading.Thread.Sleep(sleeptime);
                        EquipMentPort.VisaNS.Write("MEASUrement:MEAS4:STATE OFF");
                        System.Threading.Thread.Sleep(sleeptime);
                        EquipMentPort.VisaNS.Write("MEASUrement:MEAS5:STATE OFF");
                        System.Threading.Thread.Sleep(sleeptime);
                        EquipMentPort.VisaNS.Write("MEASUrement:MEAS6:STATE OFF");
                        System.Threading.Thread.Sleep(sleeptime);

                        Measures.Clear();
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
                    if (MeasurementType == "PKPK")
                    {
                        MeasurementType = "PK2Pk";
                    }
                    if (MeasurementType == "MAX")
                    {
                        MeasurementType = "MAXimum";
                    }
                    if (MeasurementType == "MIN")
                    {
                        MeasurementType = "MINImum";
                    }
                    if (MeasurementType == "TOP")
                    {
                        MeasurementType = "HIGH";
                    }
                    if (MeasurementType == "BASE")
                    {
                        MeasurementType = "LOW";
                    }
                    if (MeasurementType == "MEAN")
                    {

                    }
                    if (MeasurementType == "RMS")
                    {

                    }
                    if (MeasurementType == "PER")
                    {
                        MeasurementType = "PERIod";
                    }
                    if (MeasurementType == "FREQ")
                    {
                        MeasurementType = "FREQuency";
                    }
                    if (MeasurementType == "DUTY")
                    {
                        MeasurementType = "PDUty";
                    }
                    if (MeasurementType == "RISE")
                    {

                    }
                    if (MeasurementType == "FALL")
                    {

                    }



                    //EquipMentPort.VisaNS.Write("MEASUrement:MEAS6:SOUrce1 CH"+channel);//指定通道
                    //System.Threading.Thread.Sleep(sleeptime);
                    int Count = Measures.Count;
                    if (Count < 6)
                    {
                        int key = Count + 1;
                        Measures.Add(new Measure(key, channel, MeasurementType));

                        EquipMentPort.VisaNS.Write("MEASUrement:MEAS" + key + ":SOUrce1 CH" + channel);//指定通道
                        System.Threading.Thread.Sleep(sleeptime + 50);
                        EquipMentPort.VisaNS.Write("MEASUrement:MEAS" + key + ":STATE ON");// 通道增加测量项 参数1
                        System.Threading.Thread.Sleep(sleeptime + 50);
                        EquipMentPort.VisaNS.Write("MEASUrement:MEAS" + key + ":TYPe " + MeasurementType);//添加测量项
                        System.Threading.Thread.Sleep(sleeptime + 50);
                        EquipMentPort.VisaNS.Write("MEASUrement:MEAS" + key + ":TYPe " + MeasurementType);//一次不成功，再来一次
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
                    EquipMentPort.VisaNS.Write("MEASUREMENT:MEAS" + MeasureNumber + ":VALUE?");
                    System.Threading.Thread.Sleep(sleeptime);
                    string ReturnString = EquipMentPort.VisaNS.ReadString().Split(' ')[0];
                    value = CommonOscilloscope.BackTryDecimalParse(ReturnString).ToString();

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
                if (MeasureType == "PKPK")
                {
                    MeasureType = "PK2Pk";
                }
                if (MeasureType == "MAX")
                {
                    MeasureType = "MAXimum";
                }
                if (MeasureType == "MIN")
                {
                    MeasureType = "MINImum";
                }
                if (MeasureType == "TOP")
                {
                    MeasureType = "HIGH";
                }
                if (MeasureType == "BASE")
                {
                    MeasureType = "LOW";
                }
                if (MeasureType == "MEAN")
                {

                }
                if (MeasureType == "RMS")
                {

                }
                if (MeasureType == "PER")
                {
                    MeasureType = "PERIod";
                }
                if (MeasureType == "FREQ")
                {
                    MeasureType = "FREQuency";
                }
                if (MeasureType == "DUTY")
                {
                    MeasureType = "PDUty";
                }

                int MeasureNumber = 0;
                int[] Value = Measures.Where(s => s.MeasureType == MeasureType && s.channnel == channel).Select(s => s.index).ToArray();
                if (Value != null && Value.Length > 0)
                {
                    MeasureNumber = Value[0];
                }

                try
                {
                    //滚动中需要读取瞬时值
                    if (isIMMed)
                    {
                        EquipMentPort.VisaNS.Write("MEASUrement:IMMed:SOUrce CH" + channel);//指定通道
                        System.Threading.Thread.Sleep(sleeptime + 50);
                        EquipMentPort.VisaNS.Write("MEASUrement:IMMed:Type " + MeasureType);
                        System.Threading.Thread.Sleep(sleeptime + 50);
                        EquipMentPort.VisaNS.Write("MEASUREMENT:IMMed:VALUE?");
                    }
                    else
                        EquipMentPort.VisaNS.Write("MEASUREMENT:MEAS" + MeasureNumber + ":VALUE?");//MEASUREMENT:MEAS0:VALUE?
                                                                                                   //"MEASUREMENT:MEAS"+tNum.ToString()+":VALUE?"
                                                                                                   // System.Threading.Thread.Sleep(sleeptime * 2);

                    Thread.Sleep(sleeptime + 50);
                    string ReturnString = EquipMentPort.VisaNS.ReadString();
                    ReturnString = ReturnString.Split(' ')[0];
                    value = CommonOscilloscope.BackTryDecimalParse(ReturnString).ToString();

                }
                catch (Exception x)
                {
                    if (x.Message.Contains("操作完成前发生超时"))
                    {
                        try
                        {
                            EquipMentPort.VisaNS.Write("MEASUREMENT:MEAS" + MeasureNumber + ":VALUE?");//MEASUREMENT:MEAS0:VALUE?
                                                                                                       //"MEASUREMENT:MEAS"+tNum.ToString()+":VALUE?"
                                                                                                       // System.Threading.Thread.Sleep(sleeptime * 2);

                            Thread.Sleep(50);
                            string ReturnString = EquipMentPort.VisaNS.ReadString();
                            ReturnString = ReturnString.Split(' ')[0];
                            value = CommonOscilloscope.BackTryDecimalParse(ReturnString).ToString();
                        }
                        catch (Exception ex)
                        {
                            value = "null";
                            SendExMsg(ex);
                        }
                    }

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
                    double timebase_new = Convert.ToDouble(timebase) / 1000;
                    timebase = timebase_new.ToString();
                    this.AutoReadData = false;
                    if (isroll)
                    {
                        EquipMentPort.VisaNS.Write("ACQuire:STATE RUN");//开启滚动
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    else
                    {
                        EquipMentPort.VisaNS.Write("ACQuire:STATE STOP");//关闭滚动
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    EquipMentPort.VisaNS.Write("HORizontal:MAIn:SECdiv " + timebase); //时基
                    System.Threading.Thread.Sleep(sleeptime);
                    EquipMentPort.VisaNS.Write("HORizontal:MAIn:SECdiv " + timebase); //时基
                    System.Threading.Thread.Sleep(sleeptime);
                    EquipMentPort.VisaNS.Write("HORizontal:DELay:MODe ON"); //延时开
                    System.Threading.Thread.Sleep(sleeptime);
                    EquipMentPort.VisaNS.Write("HORizontal:DELay:TIMe " + delay); //触发延时(时间测量光标定位的参考线，即X1轴与此线的距离，X2轴与此线的距离)
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
                    if (type_first == 0)
                    {
                        EquipMentPort.VisaNS.Write("TRIGger:A:TYPe EDGE");//设置触发类型为边沿
                        System.Threading.Thread.Sleep(sleeptime);
                        if (type_second == "RISE")
                        {
                            EquipMentPort.VisaNS.Write("TRIGger:A:EDGE:SLOpe RISe");//设置边沿触发斜率
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (type_second == "FALL")
                        {
                            EquipMentPort.VisaNS.Write("TRIGger:A:EDGE:SLOpe FALL");//设置边沿触发斜率
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (type_second == "Alternating")
                        {
                            EquipMentPort.VisaNS.Write("TRIGger:A:EDGE:SLOpe RISe");//设置边沿触发斜率
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        EquipMentPort.VisaNS.Write("TRIGger:A:EDGE:COUPling " + coupling);//设置边沿触发耦合
                        System.Threading.Thread.Sleep(sleeptime);
                        EquipMentPort.VisaNS.Write("TRIGger:A:EDGE:SOUrce CH" + channel);//设置边沿触发信号源
                        System.Threading.Thread.Sleep(sleeptime);
                        EquipMentPort.VisaNS.Write("TRIGGER:A:LEVel " + triggerLevel);//设置触发电平
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    else if (type_first == 1)
                    {
                        EquipMentPort.VisaNS.Write("TRIGger:A:TYPe PULSE");//设置触发类型为脉冲
                        System.Threading.Thread.Sleep(sleeptime);
                        EquipMentPort.VisaNS.Write("TRIGger:A:PULSE:CLASS TIMEOut");//设置触发类型为超时
                        System.Threading.Thread.Sleep(sleeptime);
                        if (type_second == "RISE")
                        {
                            EquipMentPort.VisaNS.Write("TRIGger:A:TIMEOut:POLarity STAYSHigh");//设置保持高电平
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (type_second == "FALL")
                        {
                            EquipMentPort.VisaNS.Write("TRIGger:A:TIMEOut:POLarity STAYSLow");//设置保持低电平
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (type_second == "Alternating")
                        {
                            EquipMentPort.VisaNS.Write("TRIGger:A:TIMEOut:POLarity EITher");//设置保持任意电平
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        EquipMentPort.VisaNS.Write($"TRIGger:A:TIMEOut:TIMe {Convert.ToDouble(timeout) / 1000.0}");//设置触发时限(s)
                        System.Threading.Thread.Sleep(sleeptime);
                        EquipMentPort.VisaNS.Write("TRIGger:A:TIMEOut:SOUrce CH" + channel);//设置触发信号源
                        System.Threading.Thread.Sleep(sleeptime);
                        EquipMentPort.VisaNS.Write("TRIGGER:A:LEVel " + triggerLevel);//设置触发电平
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    else if (type_first == 2)
                    {
                        EquipMentPort.VisaNS.Write("TRIGger:A:TYPe PULSE");//设置触发类型为脉冲
                        System.Threading.Thread.Sleep(sleeptime);
                        EquipMentPort.VisaNS.Write("TRIGger:A:PULSE:CLASS RUNT");//设置触发类型为欠幅
                        System.Threading.Thread.Sleep(sleeptime);
                        if (type_second == "RISE")
                        {
                            EquipMentPort.VisaNS.Write("TRIGger:A:RUNT:POLarity POSitive");//设置极性
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (type_second == "FALL")
                        {
                            EquipMentPort.VisaNS.Write("TRIGger:A:RUNT:POLarity NEGative");//设置极性
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (type_second == "Alternating")
                        {
                            EquipMentPort.VisaNS.Write("TRIGger:A:RUNT:POLarity EITher");//设置极性任一
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        EquipMentPort.VisaNS.Write("TRIGger:A:PULSEWidth:SOUrce CH" + channel);//设置边沿触发信号源
                        System.Threading.Thread.Sleep(sleeptime);

                        if (Convert.ToDouble(timeout) <= Convert.ToDouble(triggerLevel))
                        {
                            EquipMentPort.VisaNS.Write("TRIGger:A:LOWerthreshold:CH" + channel + " " + timeout);//设置触发电平
                            System.Threading.Thread.Sleep(sleeptime);

                            EquipMentPort.VisaNS.Write("TRIGGER:A:UPPERTHRESHOLD:CH" + channel + " " + triggerLevel);//设置触发电平
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        else
                        {
                            EquipMentPort.VisaNS.Write("TRIGger:A:LOWerthreshold:CH" + channel + " " + triggerLevel);//设置触发电平
                            System.Threading.Thread.Sleep(sleeptime);

                            EquipMentPort.VisaNS.Write("TRIGGER:A:UPPERTHRESHOLD:CH" + channel + " " + timeout);//设置触发电平
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                    }
                    if (triggerType.ToUpper() == "AUTO")
                    {
                        triggerType = "AUTO";
                        EquipMentPort.VisaNS.Write("TRIGger:A:MODe " + triggerType);//设置触发类型  
                    }
                    else if (triggerType.ToUpper() == "SINGLE")
                    {
                        EquipMentPort.VisaNS.Write("TRIGger:A:MODe NORMal");//设置触发类型  
                        //triggerType = "NORMAL";//单次触发
                        System.Threading.Thread.Sleep(sleeptime);
                        EquipMentPort.VisaNS.Write("ACQuire:STATE RUN");//滚动打开
                        System.Threading.Thread.Sleep(sleeptime);
                        //EquipMentPort.VisaNS.Write("FPAnel:PRESS SINGleseq");//按下面板上的SINGLE按钮 
                        EquipMentPort.VisaNS.Write("ACQUIRE:STOPAFTER SEQUENCE");//按下面板上的SINGLE按钮 

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
                    if (TriggerType == "Single")
                    {

                        EquipMentPort.VisaNS.Write("TRIGger:A:MODe NORMal");//设置触发类型  
                        //triggerType = "NORMAL";//单次触发
                        System.Threading.Thread.Sleep(sleeptime);
                        EquipMentPort.VisaNS.Write("ACQuire:STATE RUN");//滚动打开
                        System.Threading.Thread.Sleep(sleeptime);
                        //EquipMentPort.VisaNS.Write("FPAnel:PRESS SINGleseq");//按下面板上的SINGLE按钮 
                        EquipMentPort.VisaNS.Write("ACQUIRE:STOPAFTER SEQUENCE");//按下面板上的SINGLE按钮 
                    }
                    else
                    {
                        EquipMentPort.VisaNS.Write("TRIGger:A:MODe " + TriggerType);//设置触发类型
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
                    EquipMentPort.VisaNS.Write("TRIGger:STATE?");//读取触发状态
                    System.Threading.Thread.Sleep(sleeptime);
                    string tString = EquipMentPort.VisaNS.ReadString().Trim(new char[] { '\n', '\r' });//回读测量项
                                                                                                       //string[] tStringS = tString.Split(',');
                                                                                                       //if (tStringS.Length >= 1)
                                                                                                       //{
                                                                                                       //    string value = tStringS[0].ToString();
                    if (tString.Contains("TRIGGER") || tString.Contains("SAVE"))
                    {
                        TriggleType = 0;
                    }
                    if (tString.Contains("Stop"))
                    {
                        TriggleType = 1;
                    }
                    //    return 2;
                    //}
                    //:TRIGger: STATus ?
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
        public override void OscilloscopeCursorData(int channnel, ref double[] data, int RecondLengZero = 10000 * 20)
        {
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;
                    // int RecondLengZero = 10000;
                    int tLength = -1;
                    string tString = null;
                    try
                    {
                        tLength = Function1(channnel);
                    }
                    catch//遇到操作超时错误后重新执行一次
                    {
                        tLength = Function1(channnel);
                    }

                    try
                    {
                        EquipMentPort.VisaNS.Write("CH" + channnel + ":POSITION?");//
                                                                                   //System.Threading.Thread.Sleep(sleeptime);
                        tString = EquipMentPort.VisaNS.ReadString();
                    }
                    catch//遇到操作超时错误后重新执行一次
                    {
                        EquipMentPort.VisaNS.Write("CH" + channnel + ":POSITION?");
                        tString = EquipMentPort.VisaNS.ReadString();
                    }
                    double position = (double)CommonOscilloscope.BackTryDecimalParse(tString);

                    try
                    {
                        EquipMentPort.VisaNS.Write("CH" + channnel + ":VOLts?");//
                                                                                //System.Threading.Thread.Sleep(sleeptime);
                        tString = EquipMentPort.VisaNS.ReadString();
                    }
                    catch
                    {
                        EquipMentPort.VisaNS.Write("CH" + channnel + ":VOLts?");
                        tString = EquipMentPort.VisaNS.ReadString();
                    }
                    double VOLts = (double)CommonOscilloscope.BackTryDecimalParse(tString);
                    System.Threading.Thread.Sleep(50);
                    var list = new List<byte>();
                    try
                    {
                        EquipMentPort.VisaNS.Write("DATa:STARt 1");//设置开始点
                        System.Threading.Thread.Sleep(50);
                        EquipMentPort.VisaNS.Write("DATa:STOP " + tLength.ToString());////设置结束点
                        System.Threading.Thread.Sleep(50);
                        EquipMentPort.VisaNS.Write("CURVe?");

                        //System.Threading.Thread.Sleep(100);
                        byte[] ReceivedData2204 = EquipMentPort.VisaNS.ReadByteArray(RecondLengZero).ToArray();//此函数的读取的数据全为有效值(除最后两字节为0X0A,OX0A无效值外)
                        list.AddRange(ReceivedData2204);                                                                                               //没有验证ReceivedData2204数据的正确性，待调试
                    }
                    catch
                    {
                        EquipMentPort.VisaNS.Write("DATa:STARt 1");//设置开始点
                        System.Threading.Thread.Sleep(50);
                        EquipMentPort.VisaNS.Write("DATa:STOP " + tLength.ToString());////设置结束点
                        System.Threading.Thread.Sleep(50);
                        EquipMentPort.VisaNS.Write("CURVe?");

                        //System.Threading.Thread.Sleep(100);
                        byte[] ReceivedData2204 = EquipMentPort.VisaNS.ReadByteArray(RecondLengZero).ToArray();
                        list.AddRange(ReceivedData2204);
                    }


                    if (list.Count > 10)
                    {
                        list.RemoveRange(0, 8);
                    }
                    System.Threading.Thread.Sleep(50);
                    var list2 = new List<byte>();
                    try
                    {
                        EquipMentPort.VisaNS.Write("DATa:STARt " + RecondLengZero.ToString());//设置开始点
                        System.Threading.Thread.Sleep(50);
                        EquipMentPort.VisaNS.Write("DATa:STOP " + tLength.ToString());////设置结束点
                        System.Threading.Thread.Sleep(50);
                        EquipMentPort.VisaNS.Write("CURVe?");
                        byte[] ReceivedData2204 = EquipMentPort.VisaNS.ReadByteArray(RecondLengZero).ToArray();//此函数的读取的数据全为有效值(除最后两字节为0X0A,OX0A无效值外)
                        list2.AddRange(ReceivedData2204);
                    }
                    catch
                    {
                        EquipMentPort.VisaNS.Write("DATa:STARt " + RecondLengZero.ToString());//设置开始点
                        System.Threading.Thread.Sleep(50);
                        EquipMentPort.VisaNS.Write("DATa:STOP " + tLength.ToString());////设置结束点
                        System.Threading.Thread.Sleep(50);
                        EquipMentPort.VisaNS.Write("CURVe?");
                        byte[] ReceivedData2204 = EquipMentPort.VisaNS.ReadByteArray(RecondLengZero).ToArray();
                        list2.AddRange(ReceivedData2204);
                    }


                    if (list2.Count > 10)
                    {
                        list2.RemoveRange(0, 8);
                    }
                    list.AddRange(list2);

                    var lastbyte = new double[list.Count];
                    for (var i = 0; i < list.Count; i++)
                    {
                        double value = ComplementConvertToInt(list[i]);
                        double value2 = (value / 124) * VOLts * 5 - VOLts * position;
                        //double value2 = (value / 124) * VOLts * 5 - VOLts * System.Math.Abs(position);
                        //lastbyte[i] = System.Math.Abs(value2);
                        lastbyte[i] = value2;
                    }


                    data = lastbyte;

                }
                catch (Exception ex)
                {
                    data = null;
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
                string restring = "null";

                try
                {
                    this.AutoReadData = false;
                    EquipMentPort.VisaNS.Write("ACQuire:STATE STOP"); //停止运行
                    System.Threading.Thread.Sleep(50);
                    EquipMentPort.VisaNS.Write("HardCopy:FormatException BMP");
                    System.Threading.Thread.Sleep(50);
                    EquipMentPort.VisaNS.Write("Hardcopy:Layout Portrait");
                    System.Threading.Thread.Sleep(50);
                    EquipMentPort.VisaNS.Write("Hardcopy Start");
                    System.Threading.Thread.Sleep(1000);
                    int Length = 20 * 1024 * 1024;
                    byte[] ImageData = EquipMentPort.VisaNS.ReadByteArray(Length);
                    string m_ImagrRootDir = AppDomain.CurrentDomain.BaseDirectory;

                    //保存图片到本地文件夹
                    System.IO.MemoryStream ms = new System.IO.MemoryStream(ImageData);
                    System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
                    //保存到磁盘文件
                    string imageName = ImagePathFile + System.DateTime.Now.ToString("yyyy-MM-dd HH：mm：ss：ffff") + ".bmp";
                    img.Save(System.IO.Path.Combine(ImagePathFile, imageName), System.Drawing.Imaging.ImageFormat.Bmp);
                    ms.Dispose();
                    restring = imageName.Substring(BaseDirectoryPath.Length);//去掉基目录路径后
                    System.Threading.Thread.Sleep(sleeptime * 10);
                    path = restring;

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
                    if (isRun)
                    {
                        EquipMentPort.VisaNS.Write("ACQuire:STATE RUN");//滚动打开
                    }
                    else
                    {
                        EquipMentPort.VisaNS.Write("ACQuire:STATE STOP");//滚动打开
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
                    EquipMentPort.VisaNS.Write("TRIGger:STATE?");//读取触发状态
                    System.Threading.Thread.Sleep(sleeptime);
                    string tString = EquipMentPort.VisaNS.ReadString().Trim(new char[] { '\n', '\r' });//回读测量项
                                                                                                       //string[] tStringS = tString.Split(',');
                                                                                                       //if (tStringS.Length >= 1)
                                                                                                       //{
                                                                                                       //    string value = tStringS[0].ToString();
                    if (tString.Contains("TRIGGER") || tString.Contains("SAVE"))
                    {
                        isRun = false;
                    }
                    if (tString.Contains("Stop"))
                    {
                        isRun = true;
                    }
                    //:TRIGger: STATus ?
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
                    EquipMentPort.VisaNS.Write("CURSor:FUNCtion OFF");
                    this.AutoReadData = false;
                    if (type == "X")
                    {
                        EquipMentPort.VisaNS.Write("CURSor:FUNCtion WAVEform");
                    }
                    else if (type == "Y")
                    {
                        EquipMentPort.VisaNS.Write("CURSor:FUNCtion HBArs");
                    }
                    else if (type == "XY")
                    {
                        EquipMentPort.VisaNS.Write("CURSor:FUNCtion SCREEN");
                    }
                    else
                    {
                        EquipMentPort.VisaNS.Write("CURSor:FUNCtion OFF");
                    }
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
        /// 示波器横坐标卡点
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="value1">左边点1(0到100的比例，屏幕占比)</param>
        /// <param name="value2">右边点2(0到100的比例，屏幕占比)</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscilloscope_CursorPosition_X(int channel, double value1, double value2)
        {
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;

                    EquipMentPort.VisaNS.Write("HORizontal:MAIn:SECdiv?");//单位是S
                    System.Threading.Thread.Sleep(sleeptime);

                    string tString = EquipMentPort.VisaNS.ReadString();
                    decimal timebase = CommonOscilloscope.BackTryDecimalParse(tString);
                    double temp1 = value1 * 10 - 5;// ratio * 15 - 7.5+0.05;
                    temp1 = temp1 * Convert.ToDouble(timebase);
                    EquipMentPort.VisaNS.Write("CURSor:VBArs:POSITION1 " + temp1);// CURSor: VBArs: POSITION<x>
                    System.Threading.Thread.Sleep(sleeptime);
                    double temp2 = value1 * 10 - 5;// ratio * 15 - 7.5+0.05;
                    temp2 = temp2 * Convert.ToDouble(timebase);
                    EquipMentPort.VisaNS.Write("CURSor:VBArs:POSITION2 " + temp2);// CURSor: VBArs: POSITION<x>
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
        /// 示波器卡点单一
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="value1">(0到100的比例，屏幕占比)</param>
        /// <param name="isleft">是否为左边</param>
        /// <returns></returns>
        public override void Oscilloscope_CursorPosition_X_Single(int channel, double value, bool isleft)
        {
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;
                    EquipMentPort.VisaNS.Write("CURSor:FUNCtion TIME");
                    System.Threading.Thread.Sleep(sleeptime);
                    EquipMentPort.VisaNS.Write("HORizontal:MAIn:SECdiv?");
                    System.Threading.Thread.Sleep(sleeptime);
                    string tString = EquipMentPort.VisaNS.ReadString();
                    decimal timebase = CommonOscilloscope.BackTryDecimalParse(tString);
                    EquipMentPort.VisaNS.Write("HORizontal:DELay:TIMe?");
                    decimal sDelay = CommonOscilloscope.BackTryDecimalParse(EquipMentPort.VisaNS.ReadString());
                    double temp1 = value * 10 - 5;
                    temp1 = temp1 * Convert.ToDouble(timebase) + (double)sDelay;
                    double temp2 = value * 10 - 5;
                    temp2 = temp2 * Convert.ToDouble(timebase) + (double)sDelay;
                    if (isleft)
                    {
                        EquipMentPort.VisaNS.Write("CURSor:VBArs:POSITION1 " + temp1);
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    else
                    {
                        EquipMentPort.VisaNS.Write("CURSor:VBArs:POSITION2 " + temp2);
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
                    time = time / 1000;
                    if (isleft)
                    {
                        EquipMentPort.VisaNS.Write("CURSor:VBArs:POSITION1 " + time);
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    else
                    {
                        EquipMentPort.VisaNS.Write("CURSor:VBArs:POSITION2 " + time);
                        System.Threading.Thread.Sleep(sleeptime);
                    }
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
        public override void Oscilloscope_CursorPosition_Y(int channel, double value1, double value2)
        {
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;
                    double temp1 = value1 * 10 - 5;
                    EquipMentPort.VisaNS.Write("CURSor:HBArs:POSITION1" + temp1);
                    System.Threading.Thread.Sleep(sleeptime);
                    double temp2 = value2 * 10 - 5;
                    EquipMentPort.VisaNS.Write("CURSor:HBArs:POSITION2" + temp2);
                    System.Threading.Thread.Sleep(sleeptime);
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
                    ////读取SDS2204XPlus时间测量光标1与光标2的时间差值
                    //EquipMentPort.VisaNS.SendCmd("chdr off");//示波器返回的数据只保留值数据，其他数据删掉
                    System.Threading.Thread.Sleep(sleeptime);

                    EquipMentPort.VisaNS.Write("CURSor:VBArs:DELTa?");//读取横坐标时间时间差和两个时间
                    System.Threading.Thread.Sleep(sleeptime);
                    string tDT = EquipMentPort.VisaNS.ReadString().Trim(new char[] { '\n', '\r' });
                    returnvalue[0] = Convert.ToDouble(tDT) * 1000;
                    returnvalue[1] = 1 / returnvalue[0];
                    EquipMentPort.VisaNS.Write("CURSor:VBArs:POSITION1?");//读取横坐标时间时间差和两个时间
                    System.Threading.Thread.Sleep(sleeptime);
                    string tString = EquipMentPort.VisaNS.ReadString();
                    double Left = (double)CommonOscilloscope.BackTryDecimalParse(tString);
                    returnvalue[2] = Left;
                    EquipMentPort.VisaNS.Write("CURSor:VBArs:POSITION2?");//读取横坐标时间时间差和两个时间
                    System.Threading.Thread.Sleep(sleeptime);
                    tString = EquipMentPort.VisaNS.ReadString();
                    double Right = (double)CommonOscilloscope.BackTryDecimalParse(tString);
                    returnvalue[3] = Right;
                    returnvalue[4] = 0;
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
                    if (isTouch)
                    {
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(sleeptime);
                    }
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
                    if (isroll)
                    {
                        EquipMentPort.VisaNS.Write("ACQuire:STATE RUN");
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    else
                    {
                        EquipMentPort.VisaNS.Write("ACQuire:STATE STOP");
                        System.Threading.Thread.Sleep(sleeptime);
                    }
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
                    if (storagedepth.Contains("k"))
                    {
                        storagedepth = storagedepth.Replace("k", "");
                        storagedepth = storagedepth.Replace("[^0-9]", "");
                        double value = Convert.ToDouble(storagedepth) * 1000;
                        EquipMentPort.VisaNS.Write("HORizontal:RESOlution " + value.ToString());
                    }
                    if (storagedepth.Contains("M"))
                    {
                        storagedepth = storagedepth.Replace("M", "");
                        storagedepth = storagedepth.Replace("[^0-9]", "");
                        double value = Convert.ToDouble(storagedepth) * 1000000;
                        EquipMentPort.VisaNS.Write("HORizontal:RESOlution " + value.ToString());
                    }
                    System.Threading.Thread.Sleep(sleeptime);
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
                                //if (gv.isAuto)
                                {
                                    tcp.VisaNS.Write("*IDN?");
                                    //System.Threading.Thread.Sleep(50);
                                    tcp.VisaNS.ReadString();
                                    //string ReturnString = tcp.VisaNS.ReadString().Split(' ')[0];
                                    SystemEvent.SendConnectState(true, this);
                                }
                                //else
                                //{
                                //    SystemEvent.SendConnectState(false, this);
                                //}

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
                                //if (gv.isAuto)
                                {
                                    string tPort = "TCPIP::" + EquipMentPort.Ipaddress + "::INSTR";
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

                    Thread.Sleep(300);
                }
            }
        }
        public static double ComplementConvertToInt(byte bit)
        {
            try
            {

                sbyte bt2 = (sbyte)bit;
                int lastbytezero = (int)bt2;
                return lastbytezero;

                ////原来的
                //int lastbytezero = Convert.ToInt32(bit);
                //string stringzero = Convert.ToString(lastbytezero, 2);
                //stringzero = stringzero.PadLeft(8, '0');
                //if (stringzero.Substring(0, 1) == "1")
                //{
                //    stringzero = stringzero.Substring(1);
                //    return  System.Math.Abs(Convert.ToInt32(stringzero, 2));
                //}
                //else
                //{
                //    return System.Math.Abs(Convert.ToInt32(stringzero, 2));
                //}
            }
            catch
            {
                return 0;
            }
        }
        public override void ReadTriggerState(ref bool isTrigger)
        {
            isTrigger = false;
            lock (SynLock)
            {
                try
                {
                    EquipMentPort.VisaNS.Write("TRIGger:STATE?");
                    System.Threading.Thread.Sleep(sleeptime);
                    string TriggerType = EquipMentPort.VisaNS.ReadString().Trim(new char[] { '\n', '\r' });


                    if (TriggerType.ToUpper().Contains("TRIGGER") || TriggerType.ToUpper().Contains("SAVE"))
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
                    if (EquipMentPort.VisaNS != null)
                    {
                        this.AutoReadData = false;
                        EquipMentPort.VisaNS.Write("CH" + channel + ":PRObe:AUTOZero EXECute");
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                }
                catch (Exception ex) { SendExMsg(ex); }
                finally { this.AutoReadData = true; }
            }
        }

    }
}
