
using NationalInstruments.VisaNS;
using SaiTer.ATE.EquipMent;
using SaiTer.ATE.InterFace;
using SaiTer.ATE.PortManage.PortType;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TmctlAPINet;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 设备-横河示波器
    /// </summary>
    public class DLMOscilloscope : EquipMentBase
    {
        private static object SynLock = new object();

        int DataLen = 100;
        int tmpID = -999;
        /// <summary>
        /// 命令延时时间ms
        /// </summary>

        Filtering filtering = new Filtering();
        int sleeptime = 100;
        public DLMOscilloscope(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = "DLM示波器";

            SetImagePath();
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
                    EquipMentPort.DLMOscilloscope.Send(DeviceId, "*RST");
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
        /// <param name="CHANNEL">通道(比如1、2、3)</param>
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
        public override void Oscilloscope_Channel_Set(int CHANNEL, bool isOpen, string coupling, string tapewidth, string probe, string tag, string impedance, string unit, bool isOpen_A, string gear, string position)
        {
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;
                    int result = -1;
                    if (isOpen)
                    {

                        result = EquipMentPort.DLMOscilloscope.Send(DeviceId, ":CHANNEL" + CHANNEL + ":DISPlay ON");//设置是否开启
                        System.Threading.Thread.Sleep(sleeptime);
                        if (coupling == "AC")
                        {
                            result = EquipMentPort.DLMOscilloscope.Send(DeviceId, "CHANNEL" + CHANNEL + ":COUPling AC");//设置耦合
                        }
                        if (coupling == "DC")
                        {
                            result = EquipMentPort.DLMOscilloscope.Send(DeviceId, ":CHANNEL" + CHANNEL + ":COUPling DC");//设置耦合
                        }
                        System.Threading.Thread.Sleep(sleeptime);
                        if (tapewidth != "")
                        {
                            //tapewidth = tapewidth.Replace("M", "");
                            if (tapewidth == "FULL")
                            {
                                result = EquipMentPort.DLMOscilloscope.Send(DeviceId, ":CHANNEL" + CHANNEL + ":BWIDTH" + " " + tapewidth + "HZ");//设置带宽         
                            }
                            else
                            {
                                result = EquipMentPort.DLMOscilloscope.Send(DeviceId, ":CHANNEL" + CHANNEL + ":BWIDTH" + " " + tapewidth + "MHZ");//设置带宽                  
                            }
                            System.Threading.Thread.Sleep(sleeptime);
                        }

                        if (probe != "")
                        {
                            if (unit == "A")
                            {
                                result = EquipMentPort.DLMOscilloscope.Send(DeviceId, ":CHANNEL" + CHANNEL + ":PROBe" + " C" + probe);//设置探头比，同时变为电流功能
                            }
                            else
                            {
                                result = EquipMentPort.DLMOscilloscope.Send(DeviceId, ":CHANNEL" + CHANNEL + ":PROBe " + probe);//设置探头比,同时变为电压功能
                            }

                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (tag != "")
                        {
                            result = EquipMentPort.DLMOscilloscope.Send(DeviceId, ":CHANNEL" + CHANNEL + ":LABEL:DISPLAY ON");//设置标签开启
                            System.Threading.Thread.Sleep(sleeptime);
                            result = EquipMentPort.DLMOscilloscope.Send(DeviceId, ":CHANNEL" + CHANNEL + ":LABEL:DEFINE \"" + tag + "\"");//设置标签
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        //if (impedance == "1M")
                        //{
                        //    EquipMentPort.DLMOscilloscope.Send(DeviceId,":CHANNEL" + CHANNEL + ":CPL A1M");//设置阻抗
                        //    System.Threading.Thread.Sleep(sleeptime);
                        //}
                        //if (impedance == "50")
                        //{

                        //    EquipMentPort.DLMOscilloscope.Send(DeviceId,":CHANNEL" + CHANNEL + ":CPL A50");//设置阻抗
                        //    System.Threading.Thread.Sleep(sleeptime);
                        //}
                        //if (unit != "")
                        //{
                        //    EquipMentPort.DLMOscilloscope.Send(DeviceId,":C" + CHANNEL + ":UNIT " + unit);//设置单位
                        //    System.Threading.Thread.Sleep(sleeptime);
                        //}
                        if (isOpen_A)
                        {
                            result = EquipMentPort.DLMOscilloscope.Send(DeviceId, ":CHANNEL" + CHANNEL + ":INVERT ON");//设置通道反相关是否开启
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        else
                        {
                            result = EquipMentPort.DLMOscilloscope.Send(DeviceId, ":CHANNEL" + CHANNEL + ":INVERT OFF");//设置通道反相关是否开启
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (gear != "")
                        {
                            result = EquipMentPort.DLMOscilloscope.Send(DeviceId, ":CHANNEL" + CHANNEL + ":VDIV " + gear + "V");//设置通道挡位
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (position != "")
                        {
                            //double realposition = Convert.ToDouble(gear) * Convert.ToDouble(position);
                            result = EquipMentPort.DLMOscilloscope.Send(DeviceId, ":CHANNEL" + CHANNEL + ":POSITION " + position);//设置通道位置
                            System.Threading.Thread.Sleep(sleeptime);
                        }

                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    else
                    {
                        result = EquipMentPort.DLMOscilloscope.Send(DeviceId, ":CHANnel" + CHANNEL + ":DISPlay OFF");//设置是否开启
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
                    //EquipMentPort.DLMOscilloscope.Send(DeviceId,":MEASURE:CHANNEL1:ALL OFF");//清除所有测量项
                    //System.Threading.Thread.Sleep(sleeptime);
                    //EquipMentPort.DLMOscilloscope.Send(DeviceId,":MEASURE:CHANNEL2:ALL OFF");//清除所有测量项
                    //System.Threading.Thread.Sleep(sleeptime);
                    //EquipMentPort.DLMOscilloscope.Send(DeviceId,":MEASURE:CHANNEL3:ALL OFF");//清除所有测量项
                    //System.Threading.Thread.Sleep(sleeptime);
                    //EquipMentPort.DLMOscilloscope.Send(DeviceId,":MEASURE:CHANNEL4:ALL OFF");//清除所有测量项
                    //System.Threading.Thread.Sleep(sleeptime);


                    EquipMentPort.DLMOscilloscope.Send(DeviceId, ":MEASURE:CHANNEL1:ALL OFF");//清除所有测量项
                    System.Threading.Thread.Sleep(sleeptime);

                    EquipMentPort.DLMOscilloscope.Send(DeviceId, ":MEASURE:CHANNEL2:ALL OFF");//清除所有测量项
                    System.Threading.Thread.Sleep(sleeptime);

                    EquipMentPort.DLMOscilloscope.Send(DeviceId, ":MEASURE:CHANNEL3:ALL OFF");//清除所有测量项
                    System.Threading.Thread.Sleep(sleeptime);

                    EquipMentPort.DLMOscilloscope.Send(DeviceId, ":MEASURE:CHANNEL4:ALL OFF");//清除所有测量项
                    System.Threading.Thread.Sleep(sleeptime);



                    EquipMentPort.DLMOscilloscope.Send(DeviceId, ":MEASURE:CHANNEL1:AREA2:ALL OFF");//清除所有测量项
                    System.Threading.Thread.Sleep(sleeptime);

                    EquipMentPort.DLMOscilloscope.Send(DeviceId, ":MEASURE:CHANNEL2:AREA2:ALL OFF");//清除所有测量项
                    System.Threading.Thread.Sleep(sleeptime);

                    EquipMentPort.DLMOscilloscope.Send(DeviceId, ":MEASURE:CHANNEL3:AREA2:ALL OFF");//清除所有测量项
                    System.Threading.Thread.Sleep(sleeptime);

                    EquipMentPort.DLMOscilloscope.Send(DeviceId, ":MEASURE:CHANNEL4:AREA2:ALL OFF");//清除所有测量项
                    System.Threading.Thread.Sleep(sleeptime);

                    EquipMentPort.DLMOscilloscope.Send(DeviceId, ":MEASURE:MODE ON");//测量开
                    System.Threading.Thread.Sleep(sleeptime);
                    //EquipMentPort.DLMOscilloscope.Send(DeviceId,"MEAS:MODE ADVanced");//高级测量，
                    //System.Threading.Thread.Sleep(sleeptime);
                    //EquipMentPort.DLMOscilloscope.Send(DeviceId,"MEAS:ADV:STYL M2");//高级测量的显示模式
                    //System.Threading.Thread.Sleep(sleeptime);
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
        /// <param name="CHANNEL"></param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscilloscope_AddMeasure(string MeasurementType, int CHANNEL)
        {
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;
                    if (MeasurementType == "PKPK")
                    {
                        MeasurementType = "PTOPeak";
                    }
                    if (MeasurementType == "MAX")
                    {
                        MeasurementType = "MAXimum";
                    }
                    if (MeasurementType == "MIN")
                    {
                        MeasurementType = "MINimum";
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
                        MeasurementType = "AVERage";
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
                    EquipMentPort.DLMOscilloscope.Send(DeviceId, ":MEASURE:CHANNEL" + CHANNEL + ":" + MeasurementType + ":STATE ON");//添加测量项
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
        /// <param name="CHANNEL">通道号(1,2)</param>
        /// <returns>返回测量的值<</returns>
        public override void Oscilloscope_ReadMeasure(string MeasureType, int CHANNEL, ref string returnvalue, bool isIMMed = false)
        {
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;
                    Thread.Sleep(300);
                    if (MeasureType == "PKPK")
                    {
                        MeasureType = "PTOPeak";
                    }
                    if (MeasureType == "MAX")
                    {
                        MeasureType = "MAXimum";
                    }
                    if (MeasureType == "MIN")
                    {
                        MeasureType = "MINimum";
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
                        MeasureType = "AVERage";
                    }
                    if (MeasureType == "RMS")
                    {

                    }
                    if (MeasureType == "PER")
                    {
                        MeasureType = "PERiod";
                    }
                    if (MeasureType == "FREQ")
                    {
                        MeasureType = "FREQuency";
                    }
                    if (MeasureType == "DUTY")
                    {

                    }
                    if (MeasureType == "RISE")
                    {

                    }
                    if (MeasureType == "FALL")
                    {

                    }
                    EquipMentPort.DLMOscilloscope.Send(DeviceId, ":MEASURE:CHANNEL" + CHANNEL + ":" + MeasureType + ":VALUE?");//发送读取测量项

                    StringBuilder sbuff = new StringBuilder(10000000);
                    string tString = ReciveDataString().Trim(new char[] { '\n', '\r' });//回读测量项
                    tString = tString.Trim(new char[] { '\n', '\r' });//回读测量项
                    string[] tStringS = tString.Split(':');

                    if (tStringS.Length >= 2)
                    {
                        string value = tStringS[4];
                        value = value.Trim(new char[] { '\n', '\r' });//回读测量项
                        value = value.Replace("\\n", " ").Trim().Replace("VAL", "");
                        string value2 = CommonOscilloscope.ContentEDataChangeNum_D(value).ToString();
                        returnvalue = value2;
                    }
                }
                catch (Exception ex)
                {
                    returnvalue = "null";
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
                    Thread.Sleep(300);
                    if (isroll)
                    {
                        EquipMentPort.DLMOscilloscope.Send(DeviceId, ":SEARCH:ASCROLL1:START LEFT");//开启滚动
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    else
                    {
                        EquipMentPort.DLMOscilloscope.Send(DeviceId, ":SEARCH:ASCROLL1:STOP");//关闭滚动
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    EquipMentPort.DLMOscilloscope.Send(DeviceId, ":TIMebase:TDIV " + timebase + "MS");
                    System.Threading.Thread.Sleep(sleeptime);
                    EquipMentPort.DLMOscilloscope.Send(DeviceId, ":TRIGGER:DELAY:TIME " + delay + "S"); //触发延时(时间测量光标定位的参考线，即X1轴与此线的距离，X2轴与此线的距离)
                    System.Threading.Thread.Sleep(sleeptime);

                    //只设置触发时间可能不管用（%）
                    //double position = Convert.ToDouble(delay) * 1000.0 / (Convert.ToDouble(timebase) * 10) * 100.0 + 50.0;
                    //EquipMentPort.DLMOscilloscope.Send(DeviceId, ":TRIGGER:POSITION " + position.ToString("F1"));
                    //System.Threading.Thread.Sleep(sleeptime);
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
        /// <param name="timeout">超时时间(ms为单位，配合超时触发用)，type_first为2时候为触发上限电平，单位为V</param>
        /// <param name="CHANNEL">通道(1、2)</param>
        /// <param name="triggerLevel">触发电平(0mV,1V,2V)</param>
        /// <param name="triggerType">触发类型(Auto、Normal、Single)(自动、普通、单次)</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscilloscope_Trigger(int type_first, string type_second, string coupling, string timeout_type, string timeout, int CHANNEL, string triggerLevel, string triggerType)
        {
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;
                    Thread.Sleep(300);
                    if (type_first == 0)
                    {
                        EquipMentPort.DLMOscilloscope.Send(DeviceId, ":TRIGGER:ATRIGGER:TYPE SIMP");
                    }
                    if (type_first == 1)
                    {
                        EquipMentPort.DLMOscilloscope.Send(DeviceId, ":TRIGGER:ATRIGGER:TYPE TIM");
                        System.Threading.Thread.Sleep(sleeptime);
                        EquipMentPort.DLMOscilloscope.Send(DeviceId, ":TRIGGER:TIME1 " + timeout + "ms");
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    if (type_first == 2)
                    {
                        EquipMentPort.DLMOscilloscope.Send(DeviceId, ":TRIGGER:ATRIGGER:TYPE RUNT");
                        System.Threading.Thread.Sleep(sleeptime);
                        EquipMentPort.DLMOscilloscope.Send(DeviceId, ":TRIGGER:TIME1 " + timeout + "ms");
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    if (type_first == 1)
                    {
                        System.Threading.Thread.Sleep(sleeptime);
                        if (type_second == "RISE")
                        {
                            EquipMentPort.DLMOscilloscope.Send(DeviceId, ":TRIGGER:ATRIGGER:TIMeout:POLarity POSitive");
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (type_second == "FALL")
                        {
                            EquipMentPort.DLMOscilloscope.Send(DeviceId, ":TRIGGER:ATRIGGER:TIMeout:POLarity NEGative");
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (type_second == "Alternating")
                        {
                            EquipMentPort.DLMOscilloscope.Send(DeviceId, ":TRIGGER:ATRIGGER:TIMeout:POLarity EITHer");
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(sleeptime);
                        if (type_second == "RISE")
                        {
                            EquipMentPort.DLMOscilloscope.Send(DeviceId, ":TRIGGER:ATRIGGER:SIMPLE:SLOPE RISE");
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (type_second == "FALL")
                        {
                            EquipMentPort.DLMOscilloscope.Send(DeviceId, ":TRIGGER:ATRIGGER:SIMPLE:SLOPE FALL");
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (type_second == "Alternating")
                        {
                            EquipMentPort.DLMOscilloscope.Send(DeviceId, ":TRIGGER:ATRIGGER:SIMPLE:SLOPE BOTH");
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                    }
                    EquipMentPort.DLMOscilloscope.Send(DeviceId, ":TRIGGER:ATRIGGER:SIMPLE:SOURCE " + CHANNEL);
                    System.Threading.Thread.Sleep(sleeptime);
                    if (type_first != 2)
                    {
                        EquipMentPort.DLMOscilloscope.Send(DeviceId, ":TRIGGER:SOURCE:CHANNEL" + CHANNEL + ":LEVEL " + triggerLevel);
                    }
                    else
                    {
                        EquipMentPort.DLMOscilloscope.Send(DeviceId, ":TRIGGER:SOURCE:CHANNEL" + CHANNEL + ":ULLEVEL " + timeout + "V" + "," + triggerLevel + "V");
                    }
                    System.Threading.Thread.Sleep(sleeptime);
                    EquipMentPort.DLMOscilloscope.Send(DeviceId, ":TRIGGER:MODE " + triggerType);
                    System.Threading.Thread.Sleep(sleeptime);
                }
                catch (Exception ex) { SendExMsg(ex); }
                finally { this.AutoReadData = true; }
            }
        }


        /// <summary>
        /// 示波器触发类型设置
        /// </summary>
        /// <param name="TriggerType">触发类型(AUTO|ALEVel|NORMal|NSINgle)(自动、普通、单次)</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscilloscope_TriggerTypeSet(string TriggerType)
        {
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;
                    Thread.Sleep(300);
                    EquipMentPort.DLMOscilloscope.Send(DeviceId, ":TRIGGER:MODE " + TriggerType);
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
            lock (SynLock)
            {
                int TriggleType = -1;
                try
                {
                    this.AutoReadData = false;
                    Thread.Sleep(300);

                    //EquipMentPort.DLMOscilloscope.Send(DeviceId,":WAVEFORM:LENGth?");//返回数据格式：:WAV:LENG 10000
                    // System.Threading.Thread.Sleep(sleeptime);
                    // int Datalen = int.Parse(ReciveDataString().Split(' ')[1]);
                    // if(Datalen>100)
                    // {
                    //     return 1;
                    // }
                    //if(Datalen==0)
                    // {
                    //     return 0;
                    // }
                    // return 2;




                    EquipMentPort.DLMOscilloscope.Send(DeviceId, "STATus:CONDition?");//返回数据格式：:WAV:LENG 10000

                    string value = ReciveDataString();

                    value = value.Replace("[^0-9]", "");
                    int value2 = Convert.ToInt32(value);

                    if (value2 % 2 == 0)
                    {
                        TriggleType = 0;
                    }
                    else
                    {
                        TriggleType = 1;
                    }
                    return TriggleType;

                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                    return -1;
                }
                finally
                {
                    this.AutoReadData = true;
                }
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

                    System.Threading.Thread.Sleep(sleeptime);
                    StringBuilder sbTmp = new StringBuilder(10000);

                    //设置波形数据输出格式
                    EquipMentPort.DLMOscilloscope.Send(DeviceId, ":WAVEFORM:FORMAT ASCII"); //   Send(":WAVEFORM:FORMAT BYTE");// 
                    System.Threading.Thread.Sleep(sleeptime);
                    //设置需要读取的通道
                    EquipMentPort.DLMOscilloscope.Send(DeviceId, ":WAVEFORM:TRACE " + channnel.ToString());
                    System.Threading.Thread.Sleep(sleeptime);
                    //首先读取出数据的长度
                    EquipMentPort.DLMOscilloscope.Send(DeviceId, ":WAVEFORM:LENGth?");//返回数据格式：:WAV:LENG 10000

                    int Datalen = int.Parse(ReciveDataString().Split(' ')[1]);
                    //  Length = Datalen;//返回数据长度
                    int istart = 0;
                    int iend = 0;
                    //得出循环次数，每次取10000条数据
                    int iTimes = (int)Math.Ceiling((double)(Convert.ToDouble(Datalen) / Convert.ToDouble(10000)));

                    for (int i = 0; i < iTimes; i++)
                    {
                        //读取数据开始的位置
                        istart = i * 10000;
                        //读取数据结束的位置
                        if (i != iTimes - 1)
                        {
                            iend = ((i + 1) * 10000) - 1;
                        }
                        else
                        {
                            iend = Datalen - 1;
                        }
                        //开始读取数据
                        //设置数据开始的位置
                        EquipMentPort.DLMOscilloscope.Send(DeviceId, ":WAVEFORM:START " + istart.ToString());
                        System.Threading.Thread.Sleep(sleeptime);
                        //设置数据结束的位置
                        EquipMentPort.DLMOscilloscope.Send(DeviceId, ":WAVEFORM:END " + iend.ToString());
                        System.Threading.Thread.Sleep(sleeptime);
                        //发送读取波形数据命令
                        EquipMentPort.DLMOscilloscope.Send(DeviceId, ":WAVEFORM:SEND?");
                        //读取波形数据
                        if (i == 0)
                        {
                            sbTmp.Append(ReciveDataString());
                        }
                        else
                        {
                            sbTmp.Append("," + ReciveDataString());
                        }
                    }
                    string[] tempBuffur = sbTmp.ToString().Split(',');
                    double[] DecOSCdlm3054 = new double[Datalen];
                    //int index = -1;
                    //foreach (string tString in tempBuffur)
                    //{
                    //    ///实际采集到的数据与此函数返回的数据顺序发生先后调换了（即采集的开始数据放到了返回数据的结尾）
                    //    DecOSCdlm3054[index++] = Convert.ToDouble(CommonOscilloscope.ContentEDataChangeDec(tString));//把开头的数据放到DecOSCdlm3054最后，从最后查询，易找到

                    //}
                    int index = -1;
                    foreach (string tString in tempBuffur)
                    {
                        int x = ++index;
                        if (x < Datalen)
                        {
                            DecOSCdlm3054[x] = Convert.ToDouble(CommonOscilloscope.ContentEDataChangeDec(tString));//把开头的数据放到DecOSCdlm3054最后，从最后查询，易找到
                        }
                        ///实际采集到的数据与此函数返回的数据顺序发生先后调换了（即采集的开始数据放到了返回数据的结尾）

                    }

                    data = DecOSCdlm3054;

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
                string restring = "null";
                try
                {
                    this.AutoReadData = false;
                    Thread.Sleep(300);
                    ////

                    EquipMentPort.DLMOscilloscope.Send(DeviceId, ":STOP");//示波器通道1要设置参数   :TIMEBASE:TDIV 100MS  :CHANnel1:VDIV 100V   才能采集到

                    System.Threading.Thread.Sleep(sleeptime); //
                    EquipMentPort.DLMOscilloscope.Send(DeviceId, ":IMAGE:SEND?"); //读取波形图片二进制块
                                                                                  //System.Threading.Thread.Sleep(sleeptime * 5); //

                    byte[] ImageData = ReciveDataByte();
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
        /// Convert Byte[] to Image
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
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
                    Thread.Sleep(300);
                    if (isRun)
                    {
                        EquipMentPort.DLMOscilloscope.Send(DeviceId, ":Start");
                    }
                    else
                    {
                        EquipMentPort.DLMOscilloscope.Send(DeviceId, ":STOP");
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
                    Thread.Sleep(300);

                    EquipMentPort.DLMOscilloscope.Send(DeviceId, "STATus:CONDition?");//返回数据格式：:WAV:LENG 10000

                    string value = ReciveDataString();

                    value = value.Replace("[^0-9]", "");
                    int value2 = Convert.ToInt32(value);

                    if (value2 % 2 == 0)
                    {
                        isRun = false;
                    }
                    else
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
                    this.AutoReadData = false;
                    Thread.Sleep(300);
                    if (type == "X")
                    {
                        EquipMentPort.DLMOscilloscope.Send(DeviceId, ":CURSOR:TY:TYPE VERT");
                    }
                    else if (type == "Y")
                    {
                        EquipMentPort.DLMOscilloscope.Send(DeviceId, ":CURSOR:TY:TYPE HOR");
                    }
                    else if (type == "XY")
                    {
                        EquipMentPort.DLMOscilloscope.Send(DeviceId, ":CURSOR:TY:TYPE HAV");
                    }
                    else
                    {
                        EquipMentPort.DLMOscilloscope.Send(DeviceId, ":CURSOR:TY:TYPE OFF");
                    }
                    System.Threading.Thread.Sleep(sleeptime);
                }
                catch (Exception ex) { SendExMsg(ex); }
                finally { this.AutoReadData = true; }
            }
        }
        /// <summary>
        /// 示波器横坐标卡点
        /// </summary>
        /// <param name="CHANNEL">通道</param>
        /// <param name="value1">左边点1(0到100的比例，屏幕占比)</param>
        /// <param name="value2">右边点2(0到100的比例，屏幕占比)</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscilloscope_CursorPosition_X(int CHANNEL, double value1, double value2)
        {
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;
                    Thread.Sleep(300);
                    EquipMentPort.DLMOscilloscope.Send(DeviceId, ":CURSOR:TY:DEGREE:TRACE " + CHANNEL);
                    System.Threading.Thread.Sleep(sleeptime);
                    value1 = value1 * 10 - 5;
                    value2 = value2 * 10 - 5;
                    EquipMentPort.DLMOscilloscope.Send(DeviceId, ":CURSOR:TY:DEGREE:POSITION1 " + value1);
                    System.Threading.Thread.Sleep(sleeptime);
                    EquipMentPort.DLMOscilloscope.Send(DeviceId, ":CURSOR:TY:DEGREE:POSITION2 " + value2);
                    System.Threading.Thread.Sleep(sleeptime);
                }
                catch (Exception ex) { SendExMsg(ex); }
                finally { this.AutoReadData = true; }
            }
        }

        /// <summary>
        /// 示波器卡点单一
        /// </summary>
        /// <param name="CHANNEL">通道</param>
        /// <param name="value1">(0到100的比例，屏幕占比)</param>
        /// <param name="isleft">是否为左边</param>
        /// <returns></returns>
        public override void Oscilloscope_CursorPosition_X_Single(int CHANNEL, double value, bool isleft)
        {
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;
                    EquipMentPort.DLMOscilloscope.Send(DeviceId, ":CURSOR:TY:DEGREE:TRACE " + CHANNEL);
                    System.Threading.Thread.Sleep(sleeptime);
                    EquipMentPort.DLMOscilloscope.Send(DeviceId, ":CURSOR:TY:TYPE VERTical;:CURSOR:TY:VERTICAL:ALL OFF;:CURSOR:TY:HORIZONTAL:ALL OFF");
                    System.Threading.Thread.Sleep(100);
                    EquipMentPort.DLMOscilloscope.Send(DeviceId, ":CURSOR:TY:VERTICAL:T1:STATE ON;:CURSOR:TY:VERTICAL:T2:STATE ON;:CURSOR:TY:VERTICAL:DT:STATE ON");
                    System.Threading.Thread.Sleep(sleeptime);
                    value = value * 10 - 5;
                    if (isleft)
                    {
                        EquipMentPort.DLMOscilloscope.Send(DeviceId, ":CURSOR:TY:DEGREE:POSITION1 " + value);
                    }
                    else
                    {
                        EquipMentPort.DLMOscilloscope.Send(DeviceId, ":CURSOR:TY:DEGREE:POSITION2 " + value);
                    }
                    System.Threading.Thread.Sleep(sleeptime);
                }
                catch (Exception ex) { SendExMsg(ex); }
                finally { this.AutoReadData = true; }
            }
        }

        /// <summary>
        /// 示波器卡点按时间单一
        /// </summary>
        /// <param name="CHANNEL">通道</param>
        /// <param name="time">ms</param>
        /// <param name="isleft">是否为左边</param>
        /// <returns></returns>
        public override void Oscilloscope_CursorPosition_X_Time(int CHANNEL, double time, bool isleft)
        {

            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;
                    EquipMentPort.DLMOscilloscope.Send(DeviceId, ":CURSOR:TY:DEGREE:TRACE " + CHANNEL);
                    System.Threading.Thread.Sleep(sleeptime);
                    EquipMentPort.DLMOscilloscope.Send(DeviceId, ":TIMebase:TDIV?");
                    double tdiv = double.Parse(CommonOscilloscope.ContentEDataChangeDec(ReciveDataString().Split(' ')[1]).ToString());
                    time = time / 1000 / tdiv;
                    time = time * 10 - 5;
                    if (isleft)
                    {
                        EquipMentPort.DLMOscilloscope.Send(DeviceId, ":CURSOR:TY:DEGREE:POSITION1 " + time);
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    else
                    {
                        EquipMentPort.DLMOscilloscope.Send(DeviceId, ":CURSOR:TY:DEGREE:POSITION2 " + time);
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
        /// <param name="CHANNEL">通道</param>
        /// <param name="value1">下边点1(0到100的比例，屏幕占比)</param>
        /// <param name="value2">上边点2(0到100的比例，屏幕占比)</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscilloscope_CursorPosition_Y(int CHANNEL, double value1, double value2)
        {
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;
                    EquipMentPort.DLMOscilloscope.Send(DeviceId, ":CURSOR:TY:DEGREE:TRACE " + CHANNEL);
                    System.Threading.Thread.Sleep(sleeptime);
                    value1 = value1 * 0.8;
                    value2 = value2 * 0.8;
                    EquipMentPort.DLMOscilloscope.Send(DeviceId, ":CURSOR:TY:HORIZONTAL:POSITION1 " + value1);
                    System.Threading.Thread.Sleep(sleeptime);
                    EquipMentPort.DLMOscilloscope.Send(DeviceId, ":CURSOR:TY:HORIZONTAL:POSITION2 " + value2);
                    System.Threading.Thread.Sleep(sleeptime);
                }
                catch (Exception ex) { SendExMsg(ex); }
                finally { this.AutoReadData = true; }
            }
        }
        /// <summary>
        /// 回读光标参数
        /// </summary>
        /// <param name="CHANNEL">通道(1、2、3、4等)</param>
        /// <returns>返回double[5]。第一个为时间差,，第二个为1/时间差=频率单位为Hz，第三个为横坐标左边值，第四个为横坐标右边值，第五个为幅度差。时间单位为ms</returns>
        public override void Oscilloscope_ReadCursors(int CHANNEL, ref double[] Cursors)
        {
            lock (SynLock)
            {
                double[] returnvalue = new double[5];
                try
                {
                    this.AutoReadData = false;
                    EquipMentPort.DLMOscilloscope.Send(DeviceId, ":CURSOR:TY:DEGREE:TRACE " + CHANNEL);
                    System.Threading.Thread.Sleep(sleeptime);


                    EquipMentPort.DLMOscilloscope.Send(DeviceId, ":CURSOR:TY:VERTICAL:DT:VALUE?");

                    string ReturnString = ReciveDataString().Split(' ')[1];
                    decimal D_ValidTimeS = CommonOscilloscope.ContentEDataChangeDec(ReturnString);
                    returnvalue[0] = Convert.ToDouble(D_ValidTimeS) * 1000;

                    returnvalue[1] = 1 / Convert.ToDouble(D_ValidTimeS);

                    EquipMentPort.DLMOscilloscope.Send(DeviceId, ":CURSOR:TY:VERTICAL:T1:VALUE?");

                    string ReturnString2 = ReciveDataString().Split(' ')[1];
                    decimal D_ValidTimeS2 = CommonOscilloscope.ContentEDataChangeDec(ReturnString2);

                    returnvalue[2] = Convert.ToDouble(D_ValidTimeS2);

                    EquipMentPort.DLMOscilloscope.Send(DeviceId, ":CURSOR:TY:VERTICAL:T2:VALUE?");

                    string ReturnString3 = ReciveDataString().Split(' ')[1];
                    decimal D_ValidTimeS3 = CommonOscilloscope.ContentEDataChangeDec(ReturnString3);

                    returnvalue[3] = Convert.ToDouble(D_ValidTimeS2);


                    //EquipMentPort.DLMOscilloscope.Send(DeviceId,":CURSOR:TY:VERTICAL:T2:VALUE?");
                    //System.Threading.Thread.Sleep(sleeptime);
                    //string ReturnString4 = ReciveDataString().Split(' ')[1];
                    //decimal D_ValidTimeS4 = CommonOscilloscope.ContentEDataChangeDec(ReturnString4);

                    //returnvalue[4] = Convert.ToDouble(D_ValidTimeS4);
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
                        EquipMentPort.DLMOscilloscope.Send(DeviceId, ":SYSTEM:TOUCH ON");
                        System.Threading.Thread.Sleep(sleeptime);
                        //:SYSTem:TOUCh ON
                    }
                    else
                    {
                        EquipMentPort.DLMOscilloscope.Send(DeviceId, ":SYSTEM:TOUCH OFF");
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
                        EquipMentPort.DLMOscilloscope.Send(DeviceId, ":SEARCH:ASCROLL1:START LEFT");//开启滚动
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    else
                    {
                        EquipMentPort.DLMOscilloscope.Send(DeviceId, ":SEARCH:ASCROLL1:STOP");//关闭滚动
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
                        EquipMentPort.DLMOscilloscope.Send(DeviceId, ":ACQuire:RLENgth " + value.ToString());//获取存储深度
                    }
                    if (storagedepth.Contains("M"))
                    {
                        storagedepth = storagedepth.Replace("k", "");
                        storagedepth = storagedepth.Replace("[^0-9]", "");
                        double value = Convert.ToDouble(storagedepth) * 1000000;
                        EquipMentPort.DLMOscilloscope.Send(DeviceId, ":ACQuire:RLENgth " + value.ToString());//获取存储深度
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

            }
            catch (Exception ex)
            {

                SendExMsg(ex);
            }
            finally { this.AutoReadData = true; }
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

                            if (++count1 > 1000)// 如果连续大于300
                            {
                                count1 = 0;
                                Record1 = index1 - 999;
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

                            if (++count1 > 50)// 如果连续大于300
                            {
                                count1 = 0;
                                Record1 = index1 - 49;
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
                            if (++count1 > 50)// 
                            {
                                count1 = 0;
                                Record1 = index1 - 49;
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


        public string ReciveDataString()
        {
            lock (SynLock)
            {
                StringBuilder sbuff = new StringBuilder(10000000);
                try
                {
                    int ret = 1;
                    int rl = 0;
                    ret = EquipMentPort.DLMOscilloscope.Receive(DeviceId, sbuff, sbuff.Capacity, ref rl);

                }
                catch
                {
                }


                return sbuff.ToString();
            }
        }
        public byte[] ReciveDataByte()
        {


            int DataLen = 0;
            int ret = ReceiveBlockHeader(ref DataLen);
            byte[] ImageData = new byte[DataLen];
            ret = ReceiveBlock(DataLen, ref ImageData);

            return ImageData;
        }
        /// <summary>
        /// 接收块数据头，包含块数据长度
        /// </summary>
        /// <param name="ReLen"></param>
        /// <returns></returns>
        public int ReceiveBlockHeader(ref int ReLen)
        {
            lock (SynLock)
            {
                int ret = 0;
                try
                {
                    ret = 1;
                    //接收块数据头，包含块数据长度
                    ret = EquipMentPort.DLMOscilloscope.ReceiveBlockHeader(DeviceId, ref ReLen);
                }
                catch
                {

                }
                return ret;
            }
        }

        /// <summary>
        /// 接收块数据（目前只接收二进制数据）
        /// </summary>
        /// <returns></returns>
        public int ReceiveBlock(int rlen, ref byte[] data)
        {
            lock (SynLock)
            {
                int ret = 0;
                try
                {
                    ret = 1;
                    int datasize = 0;
                    int totalsize = 0;
                    int end1 = 0;
                    rlen += 1;
                    //data = new sbyte[rlen];
                    while (end1 == 0) //继续接收数据，直到设置结束标志。
                    {
                        ret = EquipMentPort.DLMOscilloscope.ReceiveBlockData(DeviceId, ref data[totalsize], rlen, ref datasize, ref end1);
                        if (ret != 0) break;
                        totalsize += datasize;
                    }
                }
                catch
                {

                }
                return ret;
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
                        if (tmpID >= 0)
                        {
                            try
                            {
                                if (AutoReadData)
                                {
                                    StringBuilder sbuff = new StringBuilder(10000000);
                                    sbuff.ToString();
                                    tcp.DLMOscilloscope.Send(tmpID, ":MEASURE:CHANNEL1:AREA2:FREQ:VALUE?");
                                    int freq = 999;

                                    int returnInt = tcp.DLMOscilloscope.Receive(tmpID, sbuff, sbuff.Capacity, ref freq);

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
                                if (AutoReadData)
                                {
                                    string tPort = EquipMentPort.Ipaddress;
                                    tcp.DLMOscilloscope = new TMCTL();
                                    tcp.DLMOscilloscope.Initialize(TMCTL.TM_CTL_VXI11, tPort, ref tmpID);
                                    if (tmpID >= 0)
                                    {
                                        DeviceId = tmpID;
                                        SystemEvent.SendConnectState(true, this);
                                    }
                                    else
                                        SystemEvent.SendConnectState(false, this);
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
        public override void ReadTriggerState(ref bool isTrigger)
        {
            isTrigger = false;
            lock (SynLock)
            {
                try
                {
                    TCPClient tcp = EquipMentPort as TCPClient;
                    //EquipMentPort.VisaNS.Write("STATus:CONDition?");
                    tcp.DLMOscilloscope.Send(DeviceId, "STATus:CONDition?");
                    System.Threading.Thread.Sleep(sleeptime);
                    //string TriggerType = EquipMentPort.VisaNS.ReadString().Trim(new char[] { '\n', '\r' });
                    StringBuilder sbuff = new StringBuilder(10000000);
                    int freq = 999;
                    string TriggerType = tcp.DLMOscilloscope.Receive(tmpID, sbuff, sbuff.Capacity, ref freq).ToString();

                    string value =RemoveNonNumericCharacters(TriggerType);
                    int value2 = Convert.ToInt32(value);

                    if (value2 % 2 == 0)
                    {
                        isTrigger= true;
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

        public  string RemoveNonNumericCharacters(string inputString)
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

    }
}
