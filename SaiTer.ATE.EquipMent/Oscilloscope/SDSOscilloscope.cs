using NationalInstruments.VisaNS;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.InterFace;
using SaiTer.ATE.PortManage.PortType;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SaiTer.ATE.EquipMent
{
    public class SDSOscilloscope : EquipMentBase
    {
        private static object SynLock = new object();
        int DataLen = 100;
        /// <summary>
        /// 命令延时时间ms
        /// </summary>

        Filtering filtering = new Filtering();
        int sleeptime = 200;
        public SDSOscilloscope(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = "SDS" + " " + LanguageManager.GetByKey("示波器");

            SetImagePath();
        }

        
        
        /// <summary>
        /// 示波器初始化
        /// </summary>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void OscilloscopeIDefalut()
        {
            //lock (SynLock)
            //{
            //    try
            //    {
            //        this.AutoReadData = false;
            //        EquipMentPort.VisaNS.Write("*RST");
            //        System.Threading.Thread.Sleep(sleeptime);
            //    }
            //    catch (Exception ex)
            //    {
            //        SendExMsg(ex);
            //    }
            //    finally { this.AutoReadData = true; }
            //}
        }

        /// <summary>
        /// 示波器通道设置
        /// </summary>
        /// <param name="channel">通道(比如1、2、3)</param>
        /// <param name="gear"> 纵坐标档位(例子:10,20,50,100,200，现在单位为V，要根据探头比来相应设置)</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscilloscope_Channel_SetGear(int channel, string gear)
        {
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;
                    if (gear != "")
                    {
                        EquipMentPort.VisaNS.Write(":C" + channel + ":VDIV " + gear + "V");//设置通道挡位
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
        /// <param name="channel">通道(比如1、2、3)</param>
        /// <param name="isOpen">是否开启当前通道</param>
        /// <param name="coupling">通道耦合(比如AC、DC)，测试项
        /// 纹波测试项AC,其它测试项基本为DC</param>
        /// <param name="tapewidth"> 带宽(比如20M、200M、FULL)</param>
        /// <param name="probe"> 探头比(比如100、200、0.01、0.02,100代表放大，0.01代表缩小)</param>
        /// <param name="tag">标签(纯文本英文)</param>
        /// <param name="impedance">阻抗(默认1M、50)</param>
        /// <param name="unit">单位(V或者A，代表电压或者电流)</param>
        /// <param name="isOpen_A">通道反相是否开启</param>
        /// <param name="gear"> 纵坐标档位(例子:10,20,50,100,200，现在单位为V，要根据探头比来相应设置)</param>
        /// <param name="position">纵坐标位置(-4、4),实际会自动乘以换算挡位</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscilloscope_Channel_Set(int channel, bool isOpen, string coupling, string tapewidth, string probe, string tag, string impedance, string unit, bool isOpen_A, string gear, string position)
        {
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;
                    if (isOpen)
                    {

                        EquipMentPort.VisaNS.Write("C" + channel + ":TRA ON");//设置是否开启
                        System.Threading.Thread.Sleep(sleeptime);
                        if (coupling == "AC")
                        {
                            EquipMentPort.VisaNS.Write(":CHANnel" + channel + ":COUPling AC");//设置耦合
                        }
                        if (coupling == "DC")
                        {
                            EquipMentPort.VisaNS.Write(":CHANnel" + channel + ":COUPling DC");//设置耦合
                        }
                        System.Threading.Thread.Sleep(sleeptime);
                        if (tapewidth != "")
                        {
                            if (tapewidth.ToUpper() == "FULL")
                            {
                                EquipMentPort.VisaNS.Write(":CHANnel" + channel + ":BWLimit" + " " + tapewidth);//设置全带宽
                                System.Threading.Thread.Sleep(sleeptime);
                            }
                            else
                            {
                                tapewidth = "20";//鼎阳2000系列示波器只支持全带宽和20M带宽

                                EquipMentPort.VisaNS.Write(":CHANnel" + channel + ":BWLimit" + " " + tapewidth + "M");//设置带宽
                                System.Threading.Thread.Sleep(sleeptime);
                            }
                        }

                        if (probe != "")
                        {
                            EquipMentPort.VisaNS.Write(":CHANnel" + channel + ":PROBe VALue," + " " + probe);//设置探头比
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (tag != "")
                        {
                            EquipMentPort.VisaNS.Write(":CHANnel" + channel + ":LABel ON");//设置标签开启
                            System.Threading.Thread.Sleep(sleeptime);
                            EquipMentPort.VisaNS.Write(":CHANnel" + channel + ":LABel:TEXT \"" + tag + "\"");//设置标签
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (impedance == "1M" || impedance == "1")
                        {
                            EquipMentPort.VisaNS.Write(":CHANnel" + channel + ":CPL A1M");//设置阻抗
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (impedance == "50")
                        {

                            EquipMentPort.VisaNS.Write(":CHANnel" + channel + ":CPL A50");//设置阻抗
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (unit != "")
                        {
                            EquipMentPort.VisaNS.Write(":C" + channel + ":UNIT " + unit);//设置单位
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (isOpen_A)
                        {
                            EquipMentPort.VisaNS.Write(":C" + channel + ":INVS ON");//设置通道反相关是否开启
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        else
                        {
                            EquipMentPort.VisaNS.Write(":C" + channel + ":INVS OFF");//设置通道反相关是否开启
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (gear != "")
                        {
                            EquipMentPort.VisaNS.Write(":C" + channel + ":VDIV " + gear + "V");//设置通道挡位
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (position != "")
                        {
                            double realposition = Convert.ToDouble(gear) * Convert.ToDouble(position);
                            EquipMentPort.VisaNS.Write(":C" + channel + ":OFST " + realposition + "V");//设置通道位置
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (isOpen)
                        {
                            EquipMentPort.VisaNS.Write("C" + channel + ":TRA ON");//设置是否开启
                        }
                        else
                        {
                            EquipMentPort.VisaNS.Write("C" + channel + ":TRA OFF");//设置是否开启
                        }
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    else
                    {
                        EquipMentPort.VisaNS.Write("C" + channel + ":TRA OFF");//设置是否开启
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
                    EquipMentPort.VisaNS.Write("MEACL");//清除所有测量项
                    System.Threading.Thread.Sleep(sleeptime);
                    EquipMentPort.VisaNS.Write(":MEAS ON");//测量开
                    System.Threading.Thread.Sleep(sleeptime);
                    EquipMentPort.VisaNS.Write("MEAS:MODE ADVanced");//高级测量，
                    System.Threading.Thread.Sleep(sleeptime);
                    EquipMentPort.VisaNS.Write("MEAS:ADV:STYL M2");//高级测量的显示模式
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
                    if (MeasurementType == "PKPK")
                    {
                        //MeasurementType = "PTOPeak";
                    }
                    if (MeasurementType == "MAX")
                    {
                        //MeasurementType = "MAXimum";
                    }
                    if (MeasurementType == "MIN")
                    {
                        //MeasurementType = "MINimum";
                    }
                    if (MeasurementType == "TOP")
                    {
                        //MeasurementType = "HIGH";
                    }
                    if (MeasurementType == "BASE")
                    {
                        //MeasurementType = "LOW";
                    }
                    if (MeasurementType == "MEAN")
                    {
                        //MeasurementType = "AVERage";
                    }
                    if (MeasurementType == "RMS")
                    {

                    }
                    if (MeasurementType == "PER")
                    {
                        MeasurementType = "PERiod";
                    }
                    if (MeasurementType == "FREQ")
                    {
                        MeasurementType = "FREQuency";
                    }
                    if (MeasurementType == "DUTY")
                    {

                    }
                    if (MeasurementType == "RISE")
                    {

                    }
                    if (MeasurementType == "FALL")
                    {

                    }
                    this.AutoReadData = false;
                    EquipMentPort.VisaNS.Write("PACU " + MeasurementType + ",C" + channel);//添加测量项
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
                    //鼎阳目前暂未找到相应的功能
                }
                catch (Exception ex)
                {
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
                try
                {
                    this.AutoReadData = false;
                    EquipMentPort.VisaNS.Write("C" + channel + ":PAVA? " + MeasureType);//发送读取测量项
                    System.Threading.Thread.Sleep(sleeptime);
                    string tString = EquipMentPort.VisaNS.ReadString().Trim(new char[] { '\n', '\r' });//回读测量项
                    string[] tStringS = tString.Split(',');
                    if (tStringS.Length >= 2)
                    {
                        value = CommonOscilloscope.ContentEDataChangeNum_D(tStringS[1]).ToString();

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
                    if (isroll)
                    {
                        EquipMentPort.VisaNS.Write("ROLL ON");//开启滚动
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    else
                    {
                        EquipMentPort.VisaNS.Write("ROLL OFF");//关闭滚动
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    EquipMentPort.VisaNS.Write("TDIV " + timebase + "MS"); //时基
                    System.Threading.Thread.Sleep(sleeptime);
                    EquipMentPort.VisaNS.Write(":TIMebase:DELay " + delay + "S"); //触发延时(时间测量光标定位的参考线，即X1轴与此线的距离，X2轴与此线的距离)
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
        /// <param name="type_first">主类型(边沿或者超时,0为边沿，1为超时，2欠幅或矮脉冲，以后再加要用到的定义)</param>
        /// <param name="type_second">次类型(上升沿或者下降沿,RISE,FALL,Alternating)</param>
        /// <param name="coupling">触发耦合(默认直流,AC,DC)</param>
        /// <param name="timeout_type">超时类型(一般默认边沿,EDGE、STATe为状态)</param>
        /// <param name="timeout">超时时间(ms为单位，配合超时触发用)</param>
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
                        EquipMentPort.VisaNS.Write(":TRIGger:TYPE EDGE");//设置触发类型为边沿
                        System.Threading.Thread.Sleep(sleeptime);
                        if (type_second == "RISE")
                        {
                            EquipMentPort.VisaNS.Write(":TRIGger:EDGE:SLOPe RISing");//设置边沿触发斜率
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (type_second == "FALL")
                        {
                            EquipMentPort.VisaNS.Write(":TRIGger:EDGE:SLOPe FALLing");//设置边沿触发斜率
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (type_second == "Alternating")
                        {
                            EquipMentPort.VisaNS.Write(":TRIGger:EDGE:SLOPe ALTernate");//设置边沿触发斜率
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        EquipMentPort.VisaNS.Write(":TRIGger:EDGE:COUPling " + coupling);//设置边沿触发耦合
                        System.Threading.Thread.Sleep(sleeptime);
                        EquipMentPort.VisaNS.Write(":TRIGger:EDGE:SOURce C" + channel);//设置边沿触发信号源
                        System.Threading.Thread.Sleep(sleeptime);
                        EquipMentPort.VisaNS.Write("C" + channel + ":TRLV " + triggerLevel);//设置触发电平
                        System.Threading.Thread.Sleep(sleeptime);

                    }
                    else if (type_first == 1)
                    {
                        EquipMentPort.VisaNS.Write(":TRIGger:TYPE DROPout");//设置触发类型为超时
                        System.Threading.Thread.Sleep(sleeptime);
                        if (type_second == "RISE")
                        {
                            EquipMentPort.VisaNS.Write(":TRIGger:DROPout:SLOPe RISing");//设置超时触发斜率
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (type_second == "FALL")
                        {
                            EquipMentPort.VisaNS.Write(":TRIGger:DROPout:SLOPe FALLing");//设置超时触发斜率
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (type_second == "Alternating")
                        {
                            EquipMentPort.VisaNS.Write(":TRIGger:DROPout:SLOPe ALTernate");//设置超时触发斜率
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        EquipMentPort.VisaNS.Write(":TRIGger:DROPout:COUPling " + coupling);//设置超时触发耦合
                        System.Threading.Thread.Sleep(sleeptime);
                        EquipMentPort.VisaNS.Write(":TRIGger:DROPout SOURce C" + channel);//设置超时触发信号源
                        System.Threading.Thread.Sleep(sleeptime);
                        EquipMentPort.VisaNS.Write(":TRIGger:DROPout:TYPE " + timeout_type);//设置超时触发类型
                        System.Threading.Thread.Sleep(sleeptime);
                        EquipMentPort.VisaNS.Write(":TRIGger:DROPout:TIME " + timeout + "ms");//设置超时触发时间
                        System.Threading.Thread.Sleep(sleeptime);
                        EquipMentPort.VisaNS.Write("C" + channel + ":TRLV " + triggerLevel);//设置触发电平
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    else if (type_first == 2)
                    {
                        EquipMentPort.VisaNS.Write(":TRIGger:TYPE PULSE");//设置触发类型为欠幅
                        System.Threading.Thread.Sleep(sleeptime);

                        if (type_second == "RISE")
                        {
                            EquipMentPort.VisaNS.Write(":TRIGger:PULSe:LIMit GREATerthan");//设置欠幅上限超过范围
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (type_second == "FALL")
                        {
                            EquipMentPort.VisaNS.Write(":TRIGger:PULSE:SLOPe LESSthan");//设置欠幅下限超过范围
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (type_second == "Alternating")
                        {
                            EquipMentPort.VisaNS.Write(":TRIGger:PULSe:LIMit INNer");//设置欠幅上下限均范围内
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        EquipMentPort.VisaNS.Write(":TRIGger:PULSe:COUPling " + coupling);//设置超时触发耦合
                        System.Threading.Thread.Sleep(sleeptime);
                        EquipMentPort.VisaNS.Write(":TRIGger:PULSe:SOURce C" + channel);//设置超时触发信号源
                        System.Threading.Thread.Sleep(sleeptime);
                        EquipMentPort.VisaNS.Write(":TRIGger:PULSe:TLOWer " + triggerLevel);//设置脉冲电平的下限
                        System.Threading.Thread.Sleep(sleeptime);
                        EquipMentPort.VisaNS.Write(":TRIGger:PULSe:TUPPer " + timeout);//设置脉冲电平的上限
                        System.Threading.Thread.Sleep(sleeptime);
                    }

                    //EquipMentPort.VisaNS.Write("INR?");
                    //System.Threading.Thread.Sleep(sleeptime);
                    //string TriggerType = EquipMentPort.VisaNS.ReadString().Trim(new char[] { '\n', '\r' });
                    //if (TriggerType == "8193" || TriggerType == "8192")
                    //{
                    //    return;
                    //}
                    EquipMentPort.VisaNS.Write("TRMD " + "AUTO");//先设置一次AUTO，再重新设置需要的触发类型，防止示波器本身处于
                                                                 //单次触发模式时，再次下发单次触发会停止触发
                    Thread.Sleep(sleeptime);
                    EquipMentPort.VisaNS.Write("TRMD " + triggerType);//设置触发类型
                    System.Threading.Thread.Sleep(sleeptime);

                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
                finally { this.AutoReadData = true; }
            }
        }

        public override void ReadTriggerState(ref bool isTrigger)
        {
            isTrigger = false;
            lock (SynLock)
            {
                try
                {
                    EquipMentPort.VisaNS.Write("INR?");
                    System.Threading.Thread.Sleep(sleeptime);
                    string TriggerType = EquipMentPort.VisaNS.ReadString().Trim(new char[] { '\n', '\r' });
                    if (TriggerType == "8193" || TriggerType == "8192")
                    {
                        isTrigger = true;
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
                    EquipMentPort.VisaNS.Write("TRMD " + TriggerType);
                    System.Threading.Thread.Sleep(sleeptime);
                }
                catch (Exception ex) { SendExMsg(ex); }
                finally { this.AutoReadData = true; }
            }
        }

        /// <summary>
        /// 触发状态回读
        /// </summary>
        /// <returns>返回触发状态,0为stop,1为等待触发,-1为错误,2为其他状态</returns>
        public override int Oscilloscope_ReadTrigger()
        {
            int TriggleType = -1;
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;
                    EquipMentPort.VisaNS.Write(":TRIGger:STATus?");//读取触发状态
                    System.Threading.Thread.Sleep(sleeptime);
                    string tString = EquipMentPort.VisaNS.ReadString().Trim(new char[] { '\n', '\r' });//回读测量项
                                                                                                       //string[] tStringS = tString.Split(',');
                                                                                                       //if (tStringS.Length >= 1)
                                                                                                       //{
                                                                                                       //    string value = tStringS[0].ToString();
                    if (tString.Contains("Ready"))
                    {
                        TriggleType = 1;
                    }
                    if (tString.Contains("Stop"))
                    {
                        TriggleType = 0;
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
        /// 应读到的点数量和实际读到的点数量差值导致的ΔX，这个值会影响光标定位
        /// </summary>
        private double deltaX = 0;
        /// <summary>
        /// 回读波形点数量设置值
        /// </summary>
        private int SetLength = 0;
        /// <summary>
        /// 回读波形点数量实际值
        /// </summary>
        private int dataLength = 0;
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
                    EquipMentPort.VisaNS.Write("STOP"); //停止运行
                    System.Threading.Thread.Sleep(sleeptime);
                    EquipMentPort.VisaNS.Write("chdr off");//示波器返回的数据只保留值数据，其他数据删掉
                    System.Threading.Thread.Sleep(sleeptime);



                    EquipMentPort.VisaNS.Write("C" + channnel.ToString() + ":OFST?");//如果通道测量的是电压，单位为V,测量的是电流，单位为A,
                    System.Threading.Thread.Sleep(sleeptime);
                    string Position = EquipMentPort.VisaNS.ReadString().Trim(new char[] { '\n', '\r' });
                    double RealPosition = Convert.ToDouble(CommonOscilloscope.ContentEDataChangeNum_D(Position));

                    // EquipMentPort.VisaNS.Write(":CHANnel" + channnel.ToString() + ":SCALe?");//如果通道测量的是电压，单位为V,测量的是电流，单位为A,
                    EquipMentPort.VisaNS.Write("C" + channnel + ":VDIV?");//如果通道测量的是电压，单位为V,测量的是电流，单位为A,
                    System.Threading.Thread.Sleep(sleeptime);
                    string tVDIV = EquipMentPort.VisaNS.ReadString().Trim(new char[] { '\n', '\r' });
                    double VDIV0 = Convert.ToDouble(CommonOscilloscope.ContentEDataChangeNum_D(tVDIV));
                    //double VDIV = Convert.ToDouble(VDIV0 / 20.00);
                    EquipMentPort.VisaNS.Write("C" + channnel.ToString() + ":WF? DAT2");//读取通道的点
                    System.Threading.Thread.Sleep(sleeptime);
                    string tString = EquipMentPort.VisaNS.ReadString();
                    // tString = tString.Substring(13,9);//C1:WF DAT2,#9002000000+其他字符串，如果没有发送chdr off，则返回此字符串
                    tString = tString.Substring(7, 9);//返回数据 DAT2,#9002000000+其他字符串， DAT2,#9之后的字符串是读取的数据长度，9个有效字符串，如数据长度为2000000
                    int tLength = (int)CommonOscilloscope.BackTryDecimalParse(tString);
                    DataLen = tLength;
                    byte[] ReceivedData2204 = EquipMentPort.VisaNS.ReadByteArray(tLength).ToArray();//此函数的读取的数据全为有效值(除最后两字节为0X0A,OX0A无效值外)
                    List<byte> LstByte = new List<byte>();
                    LstByte.AddRange(ReceivedData2204);
                    int Datalen = ReceivedData2204.Length;
                    double[] Data2204 = new double[Datalen];
                    SetLength = tLength;
                    dataLength = Data2204.Length;
                    int x = tLength - Datalen;//应读到的点数量和实际读到的点数量差值，这个值会影响光标定位
                    deltaX = Convert.ToDouble(x) / Convert.ToDouble(tLength);
                    int index = 0;
                    double rate = 118.4;
                    foreach (byte tByte in ReceivedData2204)
                    {
                        double value1 = Convert.ToDouble(tByte);

                        if (tByte <= 127)
                        {
                            Data2204[index] = (value1 / rate) * 4 * VDIV0 - RealPosition;
                        }
                        else
                        {
                            Data2204[index] = (((value1 - 255) / rate) * VDIV0 * 4) - RealPosition; //
                        }
                        index++;
                    }
                    data = Data2204;

                }
                catch (Exception ex)
                {
                    data = null;
                    SendExMsg(ex);
                }
                finally { this.AutoReadData = true; }
            }
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
                    EquipMentPort.VisaNS.Write("STOP"); //停止运行
                    System.Threading.Thread.Sleep(500);
                    EquipMentPort.VisaNS.Write("SCDP");
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
                    string restring = imageName.Substring(BaseDirectoryPath.Length);
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
                        EquipMentPort.VisaNS.Write(":TRIGger:RUN");
                    }
                    else
                    {
                        EquipMentPort.VisaNS.Write(":TRIGger:STOP");
                    }
                }
                catch (Exception ex) { SendExMsg(ex); }
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
                    EquipMentPort.VisaNS.Write(":TRIGger:STATus?");//读取触发状态
                    System.Threading.Thread.Sleep(sleeptime);
                    string tString = EquipMentPort.VisaNS.ReadString().Trim(new char[] { '\n', '\r' });//回读测量项
                    string[] tStringS = tString.Split(',');
                    if (tStringS.Length >= 2)
                    {
                        string value = System.Math.Abs(CommonOscilloscope.ContentEDataChangeNum_D(tStringS[1])).ToString();

                        if (value == "Stop")
                        {
                            isRun = false;
                        }
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
                    this.AutoReadData = false;
                    EquipMentPort.VisaNS.Write(":CURSor:MODE MANual," + type);
                    System.Threading.Thread.Sleep(sleeptime);
                }
                catch (Exception ex) { SendExMsg(ex); }
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
                    EquipMentPort.VisaNS.Write("tdiv?");
                    System.Threading.Thread.Sleep(sleeptime);
                    string tTDIV = EquipMentPort.VisaNS.ReadString().Trim(new char[] { '\n', '\r' });
                    Double TDIV = (Double)CommonOscilloscope.ContentEDataChangeNum_D(tTDIV);
                    System.Threading.Thread.Sleep(sleeptime);
                    double temp1 = value1 * 10 - 5;
                    temp1 = Convert.ToDouble(TDIV * temp1);
                    temp1 = Convert.ToDouble(temp1.ToString("f6"));
                    string tCursor = "C" + channel + ":CRST TREF," + temp1.ToString("#0.000000") + "s";
                    EquipMentPort.VisaNS.Write(tCursor);
                    System.Threading.Thread.Sleep(sleeptime);
                    double temp2 = value2 * 10 - 5;
                    temp2 = Convert.ToDouble(TDIV * temp2);
                    temp2 = Convert.ToDouble(temp2.ToString("f6"));
                    tCursor = "C" + channel + ":CRST TDIF," + temp2.ToString("#0.000000") + "s";
                    EquipMentPort.VisaNS.Write(tCursor);
                    System.Threading.Thread.Sleep(sleeptime);
                }
                catch (Exception ex) { SendExMsg(ex); }
                finally { this.AutoReadData = true; }
            }
        }

        /// <summary>
        /// 示波器卡点单一
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="value">(0到100的比例，屏幕占比)</param>
        /// <param name="isleft">是否为左边</param>
        /// <returns></returns>
        public override void Oscilloscope_CursorPosition_X_Single(int channel, double value, bool isleft)
        {
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;
                    EquipMentPort.VisaNS.Write("tdiv?");
                    System.Threading.Thread.Sleep(sleeptime);
                    string tTDIV = EquipMentPort.VisaNS.ReadString().Trim(new char[] { '\n', '\r' });
                    Double TDIV = (Double)CommonOscilloscope.ContentEDataChangeNum_D(tTDIV);
                    System.Threading.Thread.Sleep(sleeptime);
                    EquipMentPort.VisaNS.Write(":TIMebase:DELay?");
                    System.Threading.Thread.Sleep(sleeptime);
                    string delay1 = EquipMentPort.VisaNS.ReadString().Trim(new char[] { '\n', '\r' });
                    Double delay = (Double)CommonOscilloscope.ContentEDataChangeNum_D(delay1);
                    EquipMentPort.VisaNS.Write("CRTY X");//设置光标类型(同时打开光标显示)
                    int index = Convert.ToInt32(value * dataLength);
                    value = Convert.ToDouble(SetLength - dataLength + index) / Convert.ToDouble(SetLength);
                    if (isleft)
                    {
                        double temp1 = value * 10 - 5;
                        temp1 = Convert.ToDouble(TDIV * temp1) + delay;
                        temp1 = Convert.ToDouble(temp1.ToString("f6"));
                        // string tCursor = ":CURSor:X1 " + temp1.ToString("#0.000000") + "s";
                        string tCursor = "C" + channel + ":CRST TREF," + temp1.ToString("#0.000000") + "s"; //通道时间测量光标X1位置设置

                        EquipMentPort.VisaNS.Write(tCursor);
                        System.Threading.Thread.Sleep(sleeptime);


                    }
                    else
                    {
                        double temp2 = value * 10 - 5;
                        temp2 = Convert.ToDouble(temp2.ToString("f6"));
                        temp2 = Convert.ToDouble(TDIV * temp2) + delay;
                        // string tCursor = ":CURSor:X2 " + temp2.ToString("#0.000000") + "s";
                        string tCursor = "C" + channel + ":CRST TDIF," + temp2.ToString("#0.000000") + "s"; //通道时间测量光标X2位置设置
                        EquipMentPort.VisaNS.Write(tCursor);
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
                    if (isleft)
                    {
                        string tCursor = "C" + channel + ":CRST TREF," + time.ToString("#0.000000") + "ms";
                        EquipMentPort.VisaNS.Write(tCursor);
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    else
                    {
                        string tCursor = "C" + channel + ":CRST TDIF," + time.ToString("#0.000000") + "s";
                        EquipMentPort.VisaNS.Write(tCursor);
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
                    //设置通道源
                    EquipMentPort.VisaNS.Write(":CURSor:SOURce1 C" + channel);
                    System.Threading.Thread.Sleep(sleeptime);
                    EquipMentPort.VisaNS.Write(":CURSor:SOURce2 C" + channel);
                    System.Threading.Thread.Sleep(sleeptime);
                    EquipMentPort.VisaNS.Write(":CHANnel" + channel + ":SCALe?");
                    System.Threading.Thread.Sleep(sleeptime);
                    string tVDIV = EquipMentPort.VisaNS.ReadString().Trim(new char[] { '\n', '\r' });
                    double VDIV0 = Convert.ToDouble(CommonOscilloscope.ContentEDataChangeNum_D(tVDIV));
                    System.Threading.Thread.Sleep(sleeptime);
                    double temp1 = value1 * 8 - 4;
                    temp1 = Convert.ToDouble(VDIV0 * temp1);
                    string tCursor = ":CURSor:Y1 " + temp1;
                    EquipMentPort.VisaNS.Write(tCursor);
                    System.Threading.Thread.Sleep(sleeptime);
                    double temp2 = value2 * 8 - 4;
                    temp2 = Convert.ToDouble(VDIV0 * temp2);
                    tCursor = ":CURSor:Y2 " + temp2;
                    EquipMentPort.VisaNS.Write(tCursor);
                    System.Threading.Thread.Sleep(sleeptime);
                }
                catch (Exception ex) { SendExMsg(ex); }
                finally { this.AutoReadData = true; }
            }
        }
        /// <summary>
        /// 示波器纵坐标卡点
        /// </summary>
        /// <param name="channel">通道(1、2、3、4等)</param>
        /// <param name="value">数值</param>
        /// <param name="cursorIndex">1=Y1;2=Y2</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscilloscope_CursorPosition_Y(int channel, double value, int cursorIndex)
        {
            lock (SynLock)
            {
                try
                {
                    if (cursorIndex < 1 || cursorIndex > 2)
                        return;
                    //设置通道源
                    this.AutoReadData = false;
                    EquipMentPort.VisaNS.Write($":CURSor:SOURce{cursorIndex} C{channel}");
                    System.Threading.Thread.Sleep(sleeptime);
                    EquipMentPort.VisaNS.Write($":CURSor:Y{cursorIndex} " + value);
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

                    EquipMentPort.VisaNS.Write("C" + channel.ToString() + ":CRVA? HREL");//读取横坐标时间时间差和两个时间
                    System.Threading.Thread.Sleep(sleeptime);
                    string tDT = EquipMentPort.VisaNS.ReadString().Trim(new char[] { '\n', '\r' });
                    string[] tDTS = tDT.Split(',');// tDTS[1]时间差单位为S
                    decimal D_ValidTimeS = 100;//横坐标时间差
                    decimal D_ValidTimeHz = 100;//Hz
                    decimal D_ValidTimeLT = 100;//横坐标左边值
                    decimal D_ValidTimeRT = 100;//横坐标右边值
                    decimal D_ValidDV = 100;//纵坐标差值
                    if (tDTS.Length >= 5)
                    {
                        D_ValidTimeS = CommonOscilloscope.ContentEDataChangeDec(tDTS[1].Trim(new char[] { 'S', 's' }));
                        D_ValidTimeHz = CommonOscilloscope.ContentEDataChangeDec(tDTS[2].Trim(new char[] { 'H', 'z' }));
                        D_ValidTimeRT = CommonOscilloscope.ContentEDataChangeDec(tDTS[3].Trim(new char[] { 'S', 's' }));
                        D_ValidTimeLT = CommonOscilloscope.ContentEDataChangeDec(tDTS[4].Trim(new char[] { 'S', 's' }));
                        returnvalue[0] = Convert.ToDouble((D_ValidTimeS * 1000).ToString("#0.000"));//S转MS
                        returnvalue[1] = Convert.ToDouble(D_ValidTimeHz);
                        returnvalue[2] = Convert.ToDouble((D_ValidTimeLT * 1000).ToString("#0.000"));//S转MS
                        returnvalue[3] = Convert.ToDouble((D_ValidTimeRT * 1000).ToString("#0.000"));//S转MS
                    }
                    EquipMentPort.VisaNS.Write(":CURSor:YDELta?");//读取纵坐标差值
                    System.Threading.Thread.Sleep(sleeptime);
                    string tV = EquipMentPort.VisaNS.ReadString().Trim(new char[] { '\n', '\r' });
                    D_ValidDV = CommonOscilloscope.ContentEDataChangeDec(tV);
                    returnvalue[4] = Convert.ToDouble(D_ValidDV);
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
                        EquipMentPort.VisaNS.Write(":SYSTem:TOUCh ON");
                        System.Threading.Thread.Sleep(sleeptime);
                        //:SYSTem:TOUCh ON
                    }
                    else
                    {
                        EquipMentPort.VisaNS.Write(":SYSTem:TOUCh OFF");
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
                        EquipMentPort.VisaNS.Write("ROLL ON");//开启滚动
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    else
                    {
                        EquipMentPort.VisaNS.Write("ROLL OFF");//关闭滚动
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
        /// 设置示波器最大存储深度
        /// </summary>
        /// <param name="storagedepth">存储深度，2000系列示波器目前已有参数20k、200k、2M、20M、200M，</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscilloscope_StorageDepth(string storagedepth)
        {
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;
                    double value = 0;
                    string strValue = "200K";
                    if (storagedepth.ToUpper().Contains("K")) //带K传下来
                    {
                        value = Convert.ToDouble(storagedepth.Substring(0, storagedepth.Length - 1));

                        if (value < 100)
                        {
                            strValue = "20K";
                        }
                        else
                        {
                            strValue = "200K";
                        }
                    }
                    else //不带字母K，默认为M
                    {
                        string str = Regex.Replace(storagedepth, @"[^\d.\d]", "");
                        if (Regex.IsMatch(str, @"^[+-]?\d*[.]?\d*$"))
                        {
                            value = double.Parse(str);
                        }
                        else
                        {
                            value = double.Parse(storagedepth);
                        }

                        if (value < 1)
                        {
                            strValue = "200K";
                        }
                        else if (value >= 1 && value < 10)
                        {
                            strValue = "2M";
                        }
                        else if (value >= 10 && value < 80)
                        {
                            strValue = "20M";
                        }
                        else if (value >= 80)
                        {
                            strValue = "200M";
                        }
                    }


                    EquipMentPort.VisaNS.Write(":ACQuire:MDEPth " + strValue);//设置存储深度
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

    }
}
