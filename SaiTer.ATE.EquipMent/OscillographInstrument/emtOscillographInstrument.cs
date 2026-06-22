using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using NPOI.SS.Formula.Functions;
using SaiTer.ATE.InterFace;
using System.Threading;
using TmctlAPINet;
using SaiTer.ATE.PortManage.PortType;

namespace SaiTer.ATE.EquipMent
{
    public class emtOscillographInstrument : EquipMentBase
    {
        private static object SynLock = new object();
        int DataLenLength = 100;
        int tmpID = -999;
        /// <summary>
        /// 命令延时时间ms
        /// </summary>
        int sleeptime = 5;
        Filtering filtering = new Filtering();

        public emtOscillographInstrument(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = "录波仪";

            SetImagePath();
        }

        /// <summary>
        /// 录波仪初始化
        /// </summary>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void OscillographIDefalut()
        {
            lock (SynLock)
            {
                try
                {
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":INITialize:EXECute");
                    System.Threading.Thread.Sleep(sleeptime);
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
            }
        }

        public override void OscillographIDefalut_Show()
        {
            lock (SynLock)
            {
                try
                {
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":DISPLAY:CINFORMATION:MODE 1");
                    System.Threading.Thread.Sleep(sleeptime);
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":DISPlay:CINFormation:TYPE MONitor");
                    System.Threading.Thread.Sleep(sleeptime);
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":DISPlay:CINFormation:WIDTh NARRow");
                    System.Threading.Thread.Sleep(sleeptime);
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
            }
        }

        /// <summary>
        /// 录波仪通道设置开关
        /// </summary>
        /// <param name="channel">通道(比如1、2、3)</param>
        /// <param name="isOpen">是否开启当前通道</param>

        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscillograph_Channel_Open(int channel, bool isOpen)
        {
            lock (SynLock)
            {
                try
                {
                    if (isOpen)
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":DISPlay ON");
                        System.Threading.Thread.Sleep(sleeptime);

                    }
                    else
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":DISPlay OFF");
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
            }
        }



        /// <summary>
        /// 录波仪通道设置挡位
        /// </summary>
        /// <param name="channel">通道(比如1、2、3)</param>
        /// <param name="gear">纵坐标档位(例子:10,20,50,100,200，现在单位为V，要根据探头比来相应设置)</param>
        /// <param name="position">纵坐标位置，为空则不操作</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscillograph_Channel_SetGear(int channel, string gear, string position)
        {
            lock (SynLock)
            {
                try
                {
                    if (gear != "")
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":VDIV " + gear);
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    if (position != "")
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":POSition " + position);
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
            }
        }

        /// <summary>
        /// 录波仪通道设置挡位CAN
        /// </summary>
        /// <param name="channel">通道(比如1、2、3)</param>
        /// <param name="schannel">子通道(比如1、2、3)</param>
        /// <param name="UpLimit">显示上限</param>
        /// <param name="DownLimit">显示下限</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscillograph_Channel_SetGear_CAN(int channel, int schannel, string UpLimit, string DownLimit)
        {
            lock (SynLock)
            {
                try
                {
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANNEL" + channel + ":CAN:SCHANNEL" + schannel + ":SCALE " + UpLimit + "," + DownLimit);
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
            }
        }
        /// <summary>
        /// 录波仪通道设置
        /// </summary>
        /// <param name="channel">通道(比如1、2、3)</param>
        /// <param name="TRACE">显示不同的轨道</param>
        /// <param name="isOpen">是否开启当前通道</param>
        /// <param name="coupling">通道耦合(比如AC、DC)，测试项
        /// 纹波测试项AC,其它测试项基本为DC</param>
        /// <param name="tapewidth"> 带宽(比如20M、200M、FULL)</param>
        /// <param name="SRATE"> 采样率 单位为1S/s</param>
        /// <param name="probe"> 探头比(比如100、200、0.01、0.02,100代表放大，0.01代表缩小)</param>
        /// <param name="tag">标签(纯文本英文)</param>
        /// <param name="unit">单位(V或者A，代表电压或者电流)</param>
        /// <param name="isOpen_A">通道反相是否开启</param>
        /// <param name="gear">纵坐标档位(例子:10,20,50,100,200，现在单位为V，要根据探头比来相应设置)</param>
        /// <param name="position">纵坐标位置(根据实际纵坐标格子来算，一般是（-（格子/2），格子/2）),实际会自动乘以换算挡位</param>
        /// <param name="filteringtype">0为LPF,1为数字滤波</param>
        /// <param name="digitfilteringtype">数字滤波方式 目前只采用GAUSs、IIR、SHARp 0、1、2</param>
        /// <param name="digittapetype">数字滤波带宽形式 ，默认低通，高通，带通,{BPASs|HPASs|LPASs}0、1、2</param>
        /// <param name="digittypewidth">带通滤波通带宽度 ？默认200Hz</param>
        /// <param name="Color">设置颜色</param>
        /// <param name="SGR1">显示在组1</param>
        /// <param name="SGR2">显示在组2</param>
        /// <param name="SGR3">显示在组3</param>
        /// <param name="SGR4">显示在组4</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscillograph_Channel_Set(int channel, int TRACE, bool isOpen, string coupling, string tapewidth, string SRATE, string probe, string tag, string unit, bool isOpen_A, string gear, string position, int filteringtype, int digitfilteringtype, int digittapetype, string digittypewidth, string Color, bool SGR1, bool SGR2, bool SGR3, bool SGR4)
        {
            lock (SynLock)
            {
                try
                {
                    if (isOpen)
                    {

                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":DISPlay ON");
                        System.Threading.Thread.Sleep(sleeptime);
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":COUPling " + coupling);
                        System.Threading.Thread.Sleep(sleeptime);
                        //EquipMentPort.VisaNS.Write(":CHANnel" + channel + ":SRATE " + SRATE);
                        //System.Threading.Thread.Sleep(sleeptime);
                        if (unit == "V")
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":PROBe " + probe);
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (unit == "A")
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":PROBe " + "C" + probe);
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":LABEL " + "\"" + tag + "\"");
                        System.Threading.Thread.Sleep(sleeptime);
                        if (isOpen_A)
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":VOLTAGE:INVERT ON");
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        else
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":VOLTAGE:INVERT OFF");
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":VDIV " + gear);
                        System.Threading.Thread.Sleep(sleeptime);

                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":POSition " + position);
                        System.Threading.Thread.Sleep(sleeptime);
                        if (filteringtype == 0)
                        {

                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":RMATh:BWIDth:MODE LPF");
                            System.Threading.Thread.Sleep(sleeptime);
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":VOLTAGE:BWIDTH " + tapewidth);
                            System.Threading.Thread.Sleep(sleeptime);

                        }
                        else
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":RMATh:BWIDth:MODE DIGital");
                            System.Threading.Thread.Sleep(sleeptime);
                            if (digitfilteringtype == 0)
                            {
                                EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":RMATh:BWIDth:TYPE GAUSs");
                                System.Threading.Thread.Sleep(sleeptime);
                            }
                            if (digitfilteringtype == 1)
                            {
                                EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":RMATh:BWIDth:TYPE IIR");
                                System.Threading.Thread.Sleep(sleeptime);
                            }
                            if (digitfilteringtype == 2)
                            {
                                EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":RMATh:BWIDth:TYPE SHARp");
                                System.Threading.Thread.Sleep(sleeptime);
                            }
                            if (digittapetype == 0)
                            {
                                EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":RMATH:BWIDTH:BAND BPASs");
                                System.Threading.Thread.Sleep(sleeptime);
                            }
                            if (digittapetype == 1)
                            {
                                EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":RMATH:BWIDTH:BAND HPASs");
                                System.Threading.Thread.Sleep(sleeptime);
                            }
                            if (digittapetype == 2)
                            {
                                EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":RMATH:BWIDTH:BAND LPASs");
                                System.Threading.Thread.Sleep(sleeptime);
                            }


                            if (digittapetype == 2)
                            {
                                EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":RMATH:BWIDTH:CFREQUENCY " + tapewidth);
                                System.Threading.Thread.Sleep(sleeptime);
                                EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":RMATH:BWIDTH:CUToff " + digittypewidth);
                                System.Threading.Thread.Sleep(sleeptime);
                            }
                            else
                            {
                                EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":RMATH:BWIDTH:CFREQUENCY " + tapewidth);
                                System.Threading.Thread.Sleep(sleeptime);
                            }
                            if (SGR1)
                            {
                                EquipMentPort.DLMOscilloscope.Send(tmpID, "DISPLAY:GROUP1:TRACE" + TRACE + ":SOURCE " + channel);
                                System.Threading.Thread.Sleep(sleeptime);
                            }
                            else
                            {
                                EquipMentPort.DLMOscilloscope.Send(tmpID, "DISPLAY:GROUP1:TRACE" + TRACE + ":SOURCE OFF");
                                System.Threading.Thread.Sleep(sleeptime);
                            }
                            if (SGR2)
                            {
                                EquipMentPort.DLMOscilloscope.Send(tmpID, "DISPLAY:GROUP2:TRACE" + TRACE + ":SOURCE " + channel);
                                System.Threading.Thread.Sleep(sleeptime);
                            }
                            else
                            {
                                EquipMentPort.DLMOscilloscope.Send(tmpID, "DISPLAY:GROUP2:TRACE" + TRACE + ":SOURCE OFF");
                                System.Threading.Thread.Sleep(sleeptime);
                            }
                            if (SGR3)
                            {
                                EquipMentPort.DLMOscilloscope.Send(tmpID, "DISPLAY:GROUP3:TRACE" + TRACE + ":SOURCE " + channel);
                                System.Threading.Thread.Sleep(sleeptime);
                            }
                            else
                            {
                                EquipMentPort.DLMOscilloscope.Send(tmpID, "DISPLAY:GROUP3:TRACE" + TRACE + ":SOURCE OFF");
                                System.Threading.Thread.Sleep(sleeptime);
                            }
                            if (SGR4)
                            {
                                EquipMentPort.DLMOscilloscope.Send(tmpID, "DISPLAY:GROUP4:TRACE" + TRACE + ":SOURCE " + channel);
                                System.Threading.Thread.Sleep(sleeptime);
                            }
                            else
                            {
                                EquipMentPort.DLMOscilloscope.Send(tmpID, "DISPLAY:GROUP4:TRACE" + TRACE + ":SOURCE OFF");
                                System.Threading.Thread.Sleep(sleeptime);
                            }


                        }


                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":DISPLAY:CHANnel" + channel + ":COLOR " + Color);
                        System.Threading.Thread.Sleep(sleeptime);


                    }
                    else
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":DISPlay OFF");
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
            }
        }


        /// <summary>
        /// 录波仪通道设置带宽和滤波
        /// </summary>
        /// <param name="channel">通道(比如1、2、3)</param>
        /// <param name="tapewidth"> 带宽(比如20M、200M、FULL)</param>
        /// <param name="filteringtype">0为LPF,1为数字滤波</param>
        /// <param name="digitfilteringtype">数字滤波方式 目前只采用GAUSs、IIR、SHARp 0、1、2</param>
        /// <param name="digittapetype">数字滤波带宽形式 ，默认低通，高通，带通,{BPASs|HPASs|LPASs}0、1、2</param>
        /// <param name="digittypewidth">带通滤波通带宽度 ？默认200Hz</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscillograph_Channel_SetFiltering(int channel, string tapewidth, int filteringtype, int digitfilteringtype, int digittapetype, string digittypewidth)
        {
            lock (SynLock)
            {
                try
                {
                    if (filteringtype == 0)
                    {

                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":RMATh:BWIDth:MODE LPF");
                        System.Threading.Thread.Sleep(sleeptime);
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":VOLTAGE:BWIDTH " + tapewidth);
                        System.Threading.Thread.Sleep(sleeptime);

                    }
                    else
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":RMATh:BWIDth:MODE DIGital");
                        System.Threading.Thread.Sleep(sleeptime);
                        if (digitfilteringtype == 0)
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":RMATh:BWIDth:TYPE GAUSs");
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (digitfilteringtype == 1)
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":RMATh:BWIDth:TYPE IIR");
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (digitfilteringtype == 2)
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":RMATh:BWIDth:TYPE SHARp");
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (digittapetype == 0)
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":RMATH:BWIDTH:BAND BPASs");
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (digittapetype == 1)
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":RMATH:BWIDTH:BAND HPASs");
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (digittapetype == 2)
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":RMATH:BWIDTH:BAND LPASs");
                            System.Threading.Thread.Sleep(sleeptime);
                        }


                        if (digittapetype == 2)
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":RMATH:BWIDTH:CFREQUENCY " + tapewidth);
                            System.Threading.Thread.Sleep(sleeptime);
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":RMATH:BWIDTH:CUToff " + digittypewidth);
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        else
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":RMATH:BWIDTH:CFREQUENCY " + tapewidth);
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                    }
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
            }
        }


        /// <summary>
        /// 录波仪通道设置只滤波
        /// </summary>
        /// <param name="channel">通道(比如1、2、3)</param>
        /// <param name="coupling">通道耦合(比如AC、DC)，测试项
        /// 纹波测试项AC,其它测试项基本为DC</param>
        /// <param name="tapewidth"> 带宽(比如20M、200M、FULL)</param>

        /// <param name="filteringtype">0为LPF,1为数字滤波</param>
        /// <param name="digitfilteringtype">数字滤波方式 目前只采用GAUSs、IIR、SHARp 0、1、2</param>
        /// <param name="digittapetype">数字滤波带宽形式 ，默认低通，高通，带通,{BPASs|HPASs|LPASs}0、1、2</param>
        /// <param name="digittypewidth">带通滤波通带宽度 ？默认200Hz</param>

        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscillograph_Filtering(int channel, string coupling, string tapewidth, int filteringtype, int digitfilteringtype, int digittapetype, string digittypewidth)
        {
            lock (SynLock)
            {
                try
                {

                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":DISPlay ON");
                    System.Threading.Thread.Sleep(sleeptime);
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":COUPling " + coupling);
                    System.Threading.Thread.Sleep(sleeptime);


                    if (filteringtype == 0)
                    {

                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + "RMATh:BWIDth: MODE LPF");
                        System.Threading.Thread.Sleep(sleeptime);
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":VOLTAGE:BWIDTH " + tapewidth);
                        System.Threading.Thread.Sleep(sleeptime);

                    }
                    else
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":RMATh:BWIDth: MODE DIGital");
                        System.Threading.Thread.Sleep(sleeptime);
                        if (digitfilteringtype == 0)
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":RMATh:BWIDth:TYPE GAUSs");
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (digitfilteringtype == 1)
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":RMATh:BWIDth:TYPE IIR");
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (digitfilteringtype == 2)
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":RMATh:BWIDth:TYPE SHARp");
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (digittapetype == 0)
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":RMATH:BWIDTH:BAND BPASs");
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (digittapetype == 1)
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":RMATH:BWIDTH:BAND HPASs");
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (digittapetype == 2)
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":RMATH:BWIDTH:BAND LPASs");
                            System.Threading.Thread.Sleep(sleeptime);
                        }


                        if (digittapetype == 2)
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":RMATH:BWIDTH:CFREQUENCY " + tapewidth);
                            System.Threading.Thread.Sleep(sleeptime);
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":RMATH:BWIDTH:PBAND " + digittypewidth);
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        else
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":RMATH:BWIDTH:CFREQUENCY " + tapewidth);
                            System.Threading.Thread.Sleep(sleeptime);
                        }

                    }
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
            }
        }
        /// <summary>
        /// 初始化测量设置，清除所有测量项
        /// </summary>
        /// <param name="channel"></param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscillograph_Measure_Initialize(int channel)
        {
            lock (SynLock)
            {
                try
                {
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":MEASURE:CHANnel" + channel + ":ALL OFF");
                    System.Threading.Thread.Sleep(sleeptime);
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
            }
        }
        /// <summary>
        /// 添加测量项
        /// </summary>
        /// <param name="MeasurementType">类型(看添加测量项参数说明)</param>
        /// <param name="channel"></param>
        /// <param name="isCAN">是否为CAN</param>
        /// <param name="Schannel">子通道</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscillograph_AddMeasure(string MeasurementType, int channel, bool isCAN, int Schannel)
        {
            lock (SynLock)
            {
                try
                {
                    if (!isCAN)
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":MEASURE:CHANNEL" + channel + ":" + MeasurementType + ":STAT 1");
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    else
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":MEASURE:CHANNEL" + channel + ":" + MeasurementType + ":SCHannel" + Schannel + ":STAT 1");
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
            }
        }
        /// <summary>
        /// 读取第几个测量值    
        /// </summary>
        /// <param name="MeasureNumber">第几个</param>
        /// <param name="returnvalue">返回测量的值</param>
        /// <returns></returns>
        public override void Oscillograph_ReadMeasure(int MeasureNumber, ref string returnvalue)
        {
            lock (SynLock)
            {
                try
                {

                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
            }
        }

        /// <summary>
        /// 按类型读取测量值
        /// </summary>
        /// <param name="MeasureType">测量值类型</param>
        /// <param name="channel">通道号(1,2)</param>
        /// <param name="isCAN">是否为CAN</param>
        /// <param name="Schannel">子通道</param>
        /// <param name="returnvalue">返回测量的值</param>
        /// <returns></returns>
        public override void Oscillograph_ReadMeasure(string MeasureType, int channel, bool isCAN, int Schannel, ref string returnvalue)
        {
            lock (SynLock)
            {
                try
                {
                    if (!isCAN)
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":MEASURE:CHANNEL" + channel + ":" + MeasureType + ":Value?");
                        //System.Threading.Thread.Sleep(sleeptime);
                        string tString = ReciveDataString();
                        String[] tStringS = tString.Split(' ');
                        if (tStringS.Length >= 2)
                        {
                            decimal tDecimal = CommonOscilloscope.BackTryDecimalParse(tStringS[1]);
                            returnvalue = tDecimal.ToString();
                        }
                    }
                    else
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":MEASURE:CHANNEL" + channel + ":" + MeasureType + ":SCHannel" + Schannel + ":STAT 1");
                        System.Threading.Thread.Sleep(sleeptime);
                        string tString = ReciveDataString();
                        String[] tStringS = tString.Split(' ');
                        if (tStringS.Length >= 2)
                        {
                            decimal tDecimal = CommonOscilloscope.BackTryDecimalParse(tStringS[1]);
                            returnvalue = tDecimal.ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
            }
        }
        /// <summary>
        /// 录波仪时基设置
        /// </summary>
        /// <param name="timebase">时基（200、400，暂定s为单位）</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscillograph_TimeBase(string timebase)
        {
            lock (SynLock)
            {
                try
                {
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":STOP");
                    System.Threading.Thread.Sleep(sleeptime);
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":TIMebase:TDIV " + timebase);
                    System.Threading.Thread.Sleep(sleeptime);
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":STart");
                    System.Threading.Thread.Sleep(sleeptime);
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
            }
        }
        /// <summary>
        /// 设置触发
        /// </summary>
        /// <param name="type_second">触发次类型，上升沿或者下降沿，交替沿，不确定是上还是下时，使用单次交替沿触发,比如急停  {RISE|FALL|BISLope} </param>
        /// <param name="channel">通道号</param>
        /// <param name="isCAN">是否为CAN</param>
        /// <param name="schannel">子通道</param>
        /// <param name="triggerLevel">触发电平</param>
        /// <param name="triggerType">触发类型(Auto、Normal、Single)(默认自动、自动电平，普通=普通、单次，N次，start)</param>
        /// <param name="position">0-100% 触发位置</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscillograph_Trigger(string type_second, int channel, bool isCAN, int schannel, string triggerLevel, string triggerType, double position)
        {
            lock (SynLock)
            {
                try
                {

                    System.Threading.Thread.Sleep(sleeptime);
                    if (isCAN)
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":TRIGger:SIMPle:SOURce " + channel + "," + schannel);
                        System.Threading.Thread.Sleep(sleeptime);

                    }
                    else
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":TRIGger:SIMPle:SOURce " + channel);
                        System.Threading.Thread.Sleep(sleeptime);
                    }

                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":TRIGGER:SIMPLE:SLOPE " + type_second);

                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":TRIGGER:SIMPLE:LEVEL " + triggerLevel);
                    System.Threading.Thread.Sleep(sleeptime);
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":TRIGGER:MODE " + triggerType);
                    System.Threading.Thread.Sleep(sleeptime);
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":TRIGger:POSition " + position);
                    System.Threading.Thread.Sleep(sleeptime);
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
            }
        }

        /// <summary>
        /// 录波仪触发类型设置
        /// </summary>
        /// <param name="TriggerType">触发类型(Auto、Normal、Single)(自动、普通、单次)</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscillograph_TriggerTypeSet(string TriggerType)
        {
            lock (SynLock)
            {
                try
                {
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":TRIGGER:MODE " + TriggerType);
                    System.Threading.Thread.Sleep(sleeptime);
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
            }
        }

        /// <summary>
        /// 触发状态回读
        /// </summary>
        /// <returns>返回触发状态,0为stop,1为等待触发,-1为错误,2为其他状态</returns>
        public override int Oscillograph_ReadTrigger()
        {
            lock (SynLock)
            {
                try
                {
                    //EquipMentPort.VisaNS.Write(":WAVEFORM:LENGth?");//返回数据格式：:WAV:LENG 10000
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




                    EquipMentPort.DLMOscilloscope.Send(tmpID, "STATus:CONDition?");//返回数据格式：:WAV:LENG 10000
                                                                                   //System.Threading.Thread.Sleep(sleeptime);
                    string value = ReciveDataString();

                    value = value.Replace("[^0-9]", "");
                    value = CommonOscilloscope.RemoveNonNumericCharacters(value);
                    int value2 = Convert.ToInt32(value);

                    if (value2 % 2 == 0)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }

                    return 2;
                }
                catch (Exception ex)
                {
                    return -1;
                }
            }
        }
        /// <summary>
        /// 波形数据回读
        /// </summary>
        /// <param name="channel"> 通道(1、2)</param>
        /// <param name="isCAN"> 是否是CAN</param>
        /// <param name="schannel"> 子通道(1、2)</param>
        /// <returns>返回为波形数据</returns>
        public override double[] OscillographCursorData(int channel, bool isCAN, int schannel)
        {
            lock (SynLock)
            {
                try
                {
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":STOP");
                    System.Threading.Thread.Sleep(100);
                    StringBuilder sbTmp = new StringBuilder(10000);

                    //设置波形数据输出格式
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":WAVEFORM:FORMAT ASCII"); //   SendCmd(":WAVEFORM:FORMAT BYTE");// 
                    System.Threading.Thread.Sleep(sleeptime);
                    //设置需要读取的通道
                    if (isCAN)
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":WAVEFORM:TRACE " + channel.ToString() + "," + schannel);
                        System.Threading.Thread.Sleep(50);
                    }
                    else
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":WAVEFORM:TRACE " + channel.ToString());
                        System.Threading.Thread.Sleep(sleeptime);
                    }

                    //首先读取出数据的长度
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":WAVEFORM:LENGth?");//返回数据格式：:WAV:LENG 10000
                                                                                   //System.Threading.Thread.Sleep(sleeptime);
                    string length = ReciveDataString().Replace("\n", "");
                    string[] SplitLength = length.Split(' ');
                    SplitLength[1] = CommonOscilloscope.RemoveNonNumericCharacters(SplitLength[1]);
                    int Datalen = int.Parse(SplitLength[1]);
                    DataLenLength = Datalen;
                    //  Length = Datalen;//返回数据长度
                    int istart = 0;
                    int iend = 0;
                    //得出循环次数，每次取10000条数据
                    int iTimes = (int)Math.Ceiling((double)(Convert.ToDouble(Datalen) / Convert.ToDouble(10000)));

                    for (int i = 0; i < iTimes; i++)
                    {
                        //读取数据开始的位置
                        istart = i * 8000;
                        //读取数据结束的位置
                        if (i != iTimes - 1)
                        {
                            iend = ((i + 1) * 8000) - 1;
                        }
                        else
                        {
                            iend = Datalen - 1;
                        }
                        //开始读取数据
                        //设置数据开始的位置
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":WAVEFORM:START " + istart.ToString());
                        System.Threading.Thread.Sleep(50);
                        //设置数据结束的位置
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":WAVEFORM:END " + iend.ToString());
                        System.Threading.Thread.Sleep(50);
                        //发送读取波形数据命令
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":WAVEFORM:SEND?");
                        //读取波形数据
                        if (i == 0)
                        {
                            sbTmp.Append(ReciveDataString());
                        }
                        else
                        {
                            sbTmp.Append("," + ReciveDataString());
                        }
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    string[] tempBuffur = sbTmp.ToString().Split(',');
                    Decimal[] DecOSCdlm3054 = new Decimal[Datalen];
                    double[] DecOSCdlm = new double[Datalen];
                    //正序排
                    int index = 0;
                    foreach (string tString in tempBuffur)
                    {
                        DecOSCdlm3054[index++] = CommonOscilloscope.ContentEDataChangeNum_D(tString);

                    }
                    index = 0;
                    foreach (decimal value in DecOSCdlm3054)
                    {
                        DecOSCdlm[index++] = Convert.ToDouble(value);

                    }



                    return DecOSCdlm;
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 截取录波仪屏幕
        /// </summary>
        /// <returns>主要返回录波仪截屏存储到电脑上的存储路径，一般是存储在程序运行
        /// 目录下的报表(勿删)\Image\年\月\年-月-日\生成的截屏名称.jpg
        /// 否则返回为try catch 的错误，没有则返回为空</returns>
        public override void OscillographSaveScreen(ref string path)
        {
            lock (SynLock)
            {
                try
                {
                    string restring = "null";                    
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":STOP");//示波器通道1要设置参数   :TIMEBASE:TDIV 100MS  :CHANnel1:VDIV 100V   才能采集到

                    System.Threading.Thread.Sleep(2000); //
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":IMAGE:SEND?"); //读取波形图片二进制块
                    
                    int DataLen = 0;
                    int ret = EquipMentPort.DLMOscilloscope.ReceiveBlockHeader(DeviceId, ref DataLen);
                    byte[] ImageData = new byte[DataLen];
                    ret = ReceiveBlock(DataLen, ref ImageData);
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
                    SendExMsg(ex);
                }
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
        /// <summary>
        /// 录波仪启停控制
        /// </summary>
        /// <param name="isRun">是否运行</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscillograph_IsRun(bool isRun)
        {
            lock (SynLock)
            {
                try
                {
                    if (isRun)
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":STart");
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    else
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":STOP");
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
            }
        }
        /// <summary>
        /// 启停状态回读
        /// </summary>
        /// <returns>true运行，false没有运行</returns>
        public override bool Oscillograph_ReadRun()
        {
            lock (SynLock)
            {
                try
                {
                    return false;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }


        /// <summary>
        /// 录波仪光标类型开启
        /// </summary>
        /// <param name="Open">是否开启</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscillograph_CursorsOpen(bool Open)
        {
            lock (SynLock)
            {
                try
                {
                    if (Open)
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, "CURSOR:TY:TYPE HAVertical");
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    else
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, "CURSOR:TY:TYPE OFF");
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
            }
        }



        /// <summary>
        /// 录波仪测量类型开启（关闭会不显示测量值）
        /// </summary>
        /// <param name="Open">是否开启</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscillograph_MEASureOpen(bool Open)
        {
            //MODE {OFF|ON|CYCLe|HISTory}
            lock (SynLock)
            {
                try
                {
                    if (Open)
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":MEASure:MODE ON");
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    else
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":MEASure:MODE OFF");
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
            }
        }

        /// <summary>
        /// 录波仪光标类型设置
        /// </summary>
        /// <param name="type">参数为X、Y、XY</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscillograph_CursorsSet(string type)
        {
            lock (SynLock)
            {
                try
                {
                    if (type == "")
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CURSOR:TY:TYPE OFF");
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    if (type == "X")
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CURSOR:TY:TYPE VERTical");
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    if (type == "Y")
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CURSOR:TY:TYPE HORizontal");
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    if (type == "XY")
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CURSOR:TY:TYPE HAVertical");
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
            }
        }
        /// <summary>
        /// 录波仪横坐标卡点
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="isCAN">是否为CAN</param>
        /// <param name="schannel">子通道</param>
        /// <param name="value1">-5到5</param>
        /// <param name="value2">-5到5</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscillograph_CursorPosition_X(int channel, bool isCAN, int schannel, double value1, double value2)
        {
            lock (SynLock)
            {
                try
                {
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":CURSOR:TY:TYPE?");//返回数据格式：:WAV:LENG 10000
                                                                                  //System.Threading.Thread.Sleep(sleeptime);
                    string type = ReciveDataString().Split(' ')[1];
                    if (type.Contains("VERT"))
                    {
                        if (isCAN)
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CURSOR:TY:VERTical:TRACE " + channel + "," + schannel);
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        else
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CURSOR:TY:VERTical:TRACE " + channel);
                            System.Threading.Thread.Sleep(sleeptime);
                        }

                    }
                    else
                    {
                        if (isCAN)
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CURSOR:TY:HORizontal:TRACE " + channel + "," + schannel);
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        else
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CURSOR:TY:HORizontal:TRACE " + channel);
                            System.Threading.Thread.Sleep(sleeptime);
                        }


                    }
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":CURSOR:TY:VERTICAL:POSITION1 " + value1);
                    System.Threading.Thread.Sleep(sleeptime);
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":CURSOR:TY:VERTICAL:POSITION2 " + value2);
                    System.Threading.Thread.Sleep(sleeptime);
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
            }
        }


        /// <summary>
        /// 录波仪卡点单一
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="isCAN">是否为CAN通道</param>
        /// <param name="schannel">子通道</param>
        /// <param name="value">-5到5</param>
        /// <param name="isleft">是否为左边</param>
        /// <returns></returns>
        public override void Oscillograph_CursorPosition_X_Single(int channel, bool isCAN, int schannel, double value, bool isleft)
        {
            lock (SynLock)
            {
                try
                {
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":CURSOR:TY:TYPE?");//返回数据格式：:WAV:LENG 10000
                                                                                  //System.Threading.Thread.Sleep(sleeptime);
                    string type = ReciveDataString().Split(' ')[1];
                    if (type.Contains("VERT"))
                    {
                        if (isCAN)
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CURSOR:TY:VERTical:TRACE " + channel + "," + schannel);
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        else
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CURSOR:TY:VERTical:TRACE " + channel);
                            System.Threading.Thread.Sleep(sleeptime);
                        }


                    }
                    else
                    {
                        if (isCAN)
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CURSOR:TY:HORizontal:TRACE " + channel + "," + schannel);
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        else
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CURSOR:TY:HORizontal:TRACE " + channel);
                            System.Threading.Thread.Sleep(sleeptime);
                        }

                    }


                    if (isleft)
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CURSOR:TY:VERTICAL:POSITION1 " + value);
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    else
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CURSOR:TY:VERTICAL:POSITION2 " + value);
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
            }
        }





        /// <summary>
        /// 录波仪卡点单一
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="isCAN">是否为CAN通道</param>
        /// <param name="schannel">子通道</param>
        /// <returns></returns>
        public override void Oscillograph_CursorPosition_SetChannel(int channel, bool isCAN, int schannel)
        {
            lock (SynLock)
            {
                try
                {
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":CURSOR:TY:TYPE?");//返回数据格式：:WAV:LENG 10000
                                                                                  //System.Threading.Thread.Sleep(sleeptime);
                    string type = ReciveDataString().Split(' ')[1];
                    if (type.Contains("VERT"))
                    {
                        if (isCAN)
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CURSOR:TY:VERTical:TRACE " + channel + "," + schannel);
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        else
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CURSOR:TY:VERTical:TRACE " + channel);
                            System.Threading.Thread.Sleep(sleeptime);
                        }


                    }
                    else
                    {
                        if (isCAN)
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CURSOR:TY:HORizontal:TRACE " + channel + "," + schannel);
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        else
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CURSOR:TY:HORizontal:TRACE " + channel);
                            System.Threading.Thread.Sleep(sleeptime);
                        }

                    }
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
            }
        }

        /// <summary>
        /// 录波仪卡点按时间单一
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="time">时间为单位为ms</param>
        /// <param name="isleft">是否为左边</param>
        /// <returns></returns>
        public override void Oscillograph_CursorPosition_X_Time(int channel, double time, bool isleft)
        {
            lock (SynLock)
            {
                try
                {

                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
            }
        }
        /// <summary>
        /// 录波仪纵坐标卡点
        /// </summary>
        /// <param name="trace">轨道</param>
        /// <param name="value1">-5到5</param>
        /// <param name="value2">-5到5</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscillograph_CursorPosition_Y(int trace, double value1, double value2)
        {
            lock (SynLock)
            {
                try
                {
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":CURSOR:TY:HORizontal:POSITION1 " + value1);
                    System.Threading.Thread.Sleep(sleeptime);
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":CURSOR:TY:HORizontal:POSITION2 " + value2);
                    System.Threading.Thread.Sleep(sleeptime);
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
            }
        }
        /// <summary>
        /// 录波仪纵坐标卡点
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="value1">实际的值</param>
        /// <param name="value2">实际的值</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscillograph_CursorPosition_Y_Value(int channel, double value1, double value2)
        {
            lock (SynLock)
            {
                try
                {
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":POSition?");
                    //System.Threading.Thread.Sleep(sleeptime);
                    double position = double.Parse(ReciveDataString().Split(' ')[1]);
                    System.Threading.Thread.Sleep(1000);

                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":VDIV?");
                    //System.Threading.Thread.Sleep(sleeptime);
                    double VDIV = double.Parse(ReciveDataString().Split(' ')[1]);

                    double value = value1 / VDIV;
                    value = value + position;

                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":CURSOR:TY:HORizontal:POSITION1 " + value);
                    System.Threading.Thread.Sleep(sleeptime);
                    value = value2 / VDIV;
                    value = value + position;
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":CURSOR:TY:HORizontal:POSITION2 " + value);
                    System.Threading.Thread.Sleep(sleeptime);
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
            }
        }
        /// <summary>
        /// 回读光标参数
        /// </summary>
        /// <param name="channel">通道(1、2、3、4等)</param>
        /// <returns>返回double[5]。第一个为时间差，第二个为1/时间差=频率单位为Hz，第三个为横坐标左边值，第四个为横坐标右边值，第五个为幅度差。时间单位为ms</returns>
        public override double[] Oscillograph_ReadCursors(int channel)
        {
            lock (SynLock)
            {
                try
                {

                    return null;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }


        /// <summary>
        /// 通道时间横向测量光标参数读取
        /// </summary>
        /// <returns>返回double 时间差 时间单位为s</returns>
        public override double Oscillograph_ReadCurcor()
        {
            lock (SynLock)
            {
                try
                {
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":CURSOR:TY:VERTICAL:DX:VALUE?");//返回数据格式：:WAV:LENG 10000
                    System.Threading.Thread.Sleep(sleeptime);
                    string value = ReciveDataString().Split(' ')[1];
                    if (value.Contains("N"))
                    {
                        return 0;
                    }
                    else
                    {
                        return Convert.ToDouble(value);
                    }

                }
                catch (Exception ex)
                {
                    return 0;
                }
            }
        }
        /// <summary>
        /// 控制录波仪是否能触屏
        /// </summary>
        /// <param name="isTouch">能否触屏</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscillograph_Touch(bool isTouch)
        {
            lock (SynLock)
            {
                try
                {
                    if (isTouch)
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":SYSTEM:TPANEL:MODE ON");//返回数据格式：:WAV:LENG 10000
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    else
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":SYSTEM:TPANEL:MODE OFF");//返回数据格式：:WAV:LENG 10000
                        System.Threading.Thread.Sleep(sleeptime);

                    }
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
            }
        }

        /// <summary>
        /// 设置录波仪最大存储深度
        /// </summary>
        /// <param name="storagedepth">存储深度，目前默认1M为1000000</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscillograph_StorageDepth(string storagedepth)
        {
            lock (SynLock)
            {
                try
                {
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":ACQuire:RLENgth " + storagedepth);//返回数据格式：:WAV:LENG 10000
                    System.Threading.Thread.Sleep(sleeptime);
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
            }
        }



        /// <summary>
        /// 设置录波仪的模式，默认为示波器模式
        /// </summary>
        /// <param name="type">0为示波器模式，1为记录仪模式</param>
        /// <returns></returns>
        public override void Oscillograph_SetType(int type)
        {
            lock (SynLock)
            {
                try
                {
                    if (type == 0)
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":ACQuire:SMODe SCOPe");//返回数据格式：:WAV:LENG 10000
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    if (type == 1)
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":ACQuire:SMODe RECorder");//返回数据格式：:WAV:LENG 10000
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
            }
        }


        /// <summary>
        /// 设置录波仪的组设置
        /// </summary>
        /// <param name="group">显示第几个组</param>
        /// <param name="format">格式</param>
        /// <param name="GRATicule">Grid为0,CROSshair为1,FRAMe为2</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscillograph_SetGroup(int group, int format, int GRATicule)
        {
            lock (SynLock)
            {
                try
                {
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":DISPLAY:SDGROUP " + group);
                    System.Threading.Thread.Sleep(sleeptime);
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":DISPLAY:GROUP" + group + ":FORMAT " + format);
                    System.Threading.Thread.Sleep(sleeptime);
                    if (GRATicule == 0)
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":DISPlay:GRATicule GRID");
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    if (GRATicule == 1)
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":DISPlay:GRATicule CROSshair");
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    if (GRATicule == 2)
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":DISPlay:GRATicule FRAMe");
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                }
                catch (Exception ex) { SendExMsg(ex); }
            }
        }




        /// <summary>
        /// 设置录波仪的组设置
        /// <param name="group">显示第几个组</param>
        /// <param name="format">格式</param>
        /// <param name="GRATicule">Grid为0,CROSshair为1,FRAMe为2</param>
        /// <param name="FGRid">精品栅格是否开启</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscillograph_SetGroup2(int group, int format, int GRATicule, bool FGRid)
        {
            lock (SynLock)
            {
                try
                {
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":DISPLAY:SDGROUP " + group);//返回数据格式：:WAV:LENG 10000
                    System.Threading.Thread.Sleep(sleeptime);
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":DISPLAY:GROUP" + group + ":FORMAT " + format);//返回数据格式：:WAV:LENG 10000
                    System.Threading.Thread.Sleep(sleeptime);
                    if (GRATicule == 0)
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":DISPlay:GRATicule GRID");//返回数据格式：:WAV:LENG 10000
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    if (GRATicule == 1)
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":DISPlay:GRATicule CROSshair");//返回数据格式：:WAV:LENG 10000
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    if (GRATicule == 2)
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":DISPlay:GRATicule FRAMe");//返回数据格式：:WAV:LENG 10000
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    if (FGRid)
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":DISPlay:FGRid ON");//返回数据格式：:WAV:LENG 10000
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    else
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":DISPlay:FGRid OFF");//返回数据格式：:WAV:LENG 10000
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
            }
        }




        /// <summary>
        /// 设置录波仪的精品栅格
        /// <param name="group">显示第几个组</param>
        /// <param name="format">格式</param>
        /// <param name="FGRid">精品栅格是否开启</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscillograph_SetGroup3(int group, int format, bool FGRid)
        {
            lock (SynLock)
            {
                try
                {
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":DISPLAY:SDGROUP " + group);//返回数据格式：:WAV:LENG 10000
                    System.Threading.Thread.Sleep(sleeptime);
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":DISPLAY:GROUP" + group + ":FORMAT " + format);//返回数据格式：:WAV:LENG 10000
                    System.Threading.Thread.Sleep(sleeptime);
                    if (FGRid)
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":DISPlay:FGRid ON");//返回数据格式：:WAV:LENG 10000
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    else
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":DISPlay:FGRid OFF");//返回数据格式：:WAV:LENG 10000
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
            }
        }



        /// <summary>
        /// 设置录波仪的组设置
        /// </summary>
        /// <param name="group">显示第几个组</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscillograph_Group_Show(int group)
        {
            lock (SynLock)
            {
                try
                {
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":DISPLAY:SDGROUP " + group);//返回数据格式：:WAV:LENG 10000
                    System.Threading.Thread.Sleep(sleeptime);
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
            }
        }

        /// <summary>
        /// 设置录波仪的组通道设置
        /// </summary>
        /// <param name="trace">编号</param>
        /// <param name="mapping">映射</param>
        /// <param name="group">显示第几个组</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscillograph_SetGroup_Channel(int trace, int mapping, int group)
        {
            lock (SynLock)
            {
                try
                {
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":DISPlay:MAPPing USERdefine");//返回数据格式：:WAV:LENG 10000
                    System.Threading.Thread.Sleep(sleeptime);


                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":DISPlay:GROup" + group + ":TRACe" + trace + ":ZNUMber " + mapping);//返回数据格式：:WAV:LENG 10000
                    System.Threading.Thread.Sleep(sleeptime);
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
            }
        }



        /// <summary>
        /// CAN通道设置
        /// </summary>
        /// <param name="channel">通道号</param>
        /// <param name="BaudRate">比特率=波特率，默认250kbps,250000</param>
        /// <param name="IsListenOnly">只听开关，默认开</param>
        /// <param name="Terminator">终端电阻开关，默认关</param>
        /// <returns></returns>
        public override void Oscillograph_SetCan_Channel(int channel, string BaudRate, bool IsListenOnly, bool Terminator)
        {
            lock (SynLock)
            {
                try
                {
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANNEL" + channel + ":CAN:PORT:BRATE " + BaudRate);//返回数据格式：:WAV:LENG 10000
                    System.Threading.Thread.Sleep(sleeptime);
                    if (IsListenOnly)
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANNEL" + channel + ":CAN:PORT:LONLy ON");//返回数据格式：:WAV:LENG 10000
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    else
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANNEL" + channel + ":CAN:PORT:LONLy OFF");//返回数据格式：:WAV:LENG 10000
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    if (Terminator)
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANNEL" + channel + ":CAN:PORT:TERMinato ON");//返回数据格式：:WAV:LENG 10000
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    else
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANNEL" + channel + ":CAN:PORT:TERMinato OFF");//返回数据格式：:WAV:LENG 10000
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
            }
        }

        /// <summary>
        /// CAN子通道设置
        /// </summary>
        /// <param name="channel">通道1</param>
        /// <param name="schannel">子通道</param>
        /// <param name="isOpen">是否开启</param>
        /// <returns></returns>
        public override void Oscillograph_SetCanChild_Open(int channel, int schannel, bool isOpen)
        {
            lock (SynLock)
            {
                try
                {
                    if (isOpen)
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":DISPlay ON");
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":CAN:SCHannel" + schannel + ":INPut ON");
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    else
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":CAN:SCHannel" + schannel + ":INPut OFF");
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
            }
        }

        /// <summary>
        /// CAN子通道设置
        /// </summary>
        /// <param name="channel">通道1</param>
        /// <param name="schannel">子通道</param>
        /// <param name="TRACE">第几个</param>
        /// <param name="isOpen">是否开启</param>
        /// <param name="SRATE"> 采样率 单位为1S/s</param>
        /// <param name="Label">标签</param>
        /// <param name="MFORmat">标准帧/扩展帧，默认扩展帧，0和1，0为扩展帧</param>
        /// <param name="Mid">帧ID</param>
        /// <param name="ByteLength">字节长度，默认自动or长度？Auto为自动把</param>
        /// <param name="Start">起始位编号 </param>
        /// <param name="ByteCnt">位数</param>
        /// <param name="ByteOrder">字节顺序，默认little,big，0为低字节，1为高字节</param>
        /// <param name="ValueType">值定义：默认无符号整型，有符号整型，浮点，逻辑 {UNSigned|SIGNed|FLOat|LOGic} 依次为0、1、2、3</param>
        /// <param name="Factor">值系数：如0.1</param>
        /// <param name="Offect">偏置，如-400</param>
        /// <param name="Unit">单位：字符</param>
        /// <param name="UpLimit">显示上限</param>
        /// <param name="DownLimit">显示下限</param>
        /// <param name="Color">设置颜色</param>
        /// <param name="SGr1">显示在组1？</param>
        /// <param name="SGr2">显示在组2？</param>
        /// <param name="SGr3">显示在组3？</param>
        /// <param name="SGr4">显示在组4？</param>
        public override void Oscillograph_SetCanChild_Channel(int channel, int schannel, int TRACE, bool isOpen, string SRATE, string Label, int MFORmat, string Mid, string ByteLength, int Start, int ByteCnt, int ByteOrder, int ValueType, string Factor, string Offect, string Unit, string UpLimit, string DownLimit, string Color, bool SGr1, bool SGr2, bool SGr3, bool SGr4)
        {
            lock (SynLock)
            {
                try
                {
                    if (isOpen)
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":DISPlay ON");
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":CAN:SCHannel" + schannel + ":INPut ON");
                        System.Threading.Thread.Sleep(sleeptime);
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANNEL" + channel + ":CAN:SCHANNEL" + schannel + ":LABEL \"" + Label + "\"");
                        System.Threading.Thread.Sleep(sleeptime);


                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":SRATE " + SRATE);
                        System.Threading.Thread.Sleep(sleeptime);
                        if (MFORmat == 0)
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANNEL" + channel + ":CAN:SCHANNEL" + schannel + ":MFORmat EXTended");
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        else
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANNEL" + channel + ":CAN:SCHANNEL" + schannel + ":MFORmat STANdard");
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANNEL" + channel + ":CAN:SCHANNEL" + schannel + ":MID \"" + Mid + "\"");
                        System.Threading.Thread.Sleep(sleeptime);
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANNEL" + channel + ":CAN:SCHANNEL" + schannel + ":BCOunt " + ByteLength);
                        System.Threading.Thread.Sleep(sleeptime);
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANNEL" + channel + ":CAN:SCHANNEL" + schannel + ":SBIT " + Start);
                        System.Threading.Thread.Sleep(sleeptime);
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANNEL" + channel + ":CAN:SCHANNEL" + schannel + ":BICount " + ByteCnt);
                        System.Threading.Thread.Sleep(sleeptime);
                        if (ByteOrder == 0)
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANNEL" + channel + ":CAN:SCHANNEL" + schannel + ":BORDer Little");
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        else
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANNEL" + channel + ":CAN:SCHANNEL" + schannel + ":BORDer Big");
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (ValueType == 0)
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANNEL" + channel + ":CAN:SCHANNEL" + schannel + ":VTYPE UNSigned");
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (ValueType == 1)
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANNEL" + channel + ":CAN:SCHANNEL" + schannel + ":VTYPE SIGNed");
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (ValueType == 2)
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANNEL" + channel + ":CAN:SCHANNEL" + schannel + ":VTYPE FLOat");
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (ValueType == 3)
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANNEL" + channel + ":CAN:SCHANNEL" + schannel + ":VTYPE LOGic");
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANNEL" + channel + ":CAN:SCHANNEL" + schannel + ":FACTor " + Factor);
                        System.Threading.Thread.Sleep(sleeptime);
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANNEL" + channel + ":CAN:SCHANNEL" + schannel + ":OFFSET " + Offect);
                        System.Threading.Thread.Sleep(sleeptime);
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANNEL" + channel + ":CAN:SCHANNEL" + schannel + ":UNIT " + "\"" + Unit + "\"");
                        System.Threading.Thread.Sleep(sleeptime);
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANNEL" + channel + ":CAN:SCHANNEL" + schannel + ":SCALE " + UpLimit + "," + DownLimit);
                        System.Threading.Thread.Sleep(sleeptime);
                        if (SGr1)
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, "DISPLAY:GROUP1:TRACE" + TRACE + ":SOURCE " + channel + "," + schannel);
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        else
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, "DISPLAY:GROUP1:TRACE" + TRACE + ":SOURCE OFF");
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (SGr2)
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, "DISPLAY:GROUP2:TRACE" + TRACE + ":SOURCE " + channel + "," + schannel);
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        else
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, "DISPLAY:GROUP2:TRACE" + TRACE + ":SOURCE OFF");
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (SGr3)
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, "DISPLAY:GROUP3:TRACE" + TRACE + ":SOURCE " + channel + "," + schannel);
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        else
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, "DISPLAY:GROUP3:TRACE" + TRACE + ":SOURCE OFF");
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        if (SGr4)
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, "DISPLAY:GROUP4:TRACE" + TRACE + ":SOURCE " + channel + "," + schannel);
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        else
                        {
                            EquipMentPort.DLMOscilloscope.Send(tmpID, "DISPLAY:GROUP4:TRACE" + TRACE + ":SOURCE OFF");
                            System.Threading.Thread.Sleep(sleeptime);
                        }
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":DISPLAY:CHANnel" + channel + ":SCHannel" + schannel + ":COLOR " + Color);
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                    else
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":CAN:SCHannel" + schannel + ":INPut OFF");
                        System.Threading.Thread.Sleep(sleeptime);
                    }
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
            }
        }






        /// <summary>
        /// 刷新CAN子通道设置
        /// </summary>
        /// <param name="channel">通道1</param>
        /// <param name="schannel">子通道</param>
        /// <param name="Mid">帧ID</param>
        /// <returns></returns>
        public override void Oscillograph_SetCanChild_Channel_Renovate(int channel, int schannel, string Mid)
        {
            lock (SynLock)
            {
                try
                {
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANNEL" + channel + ":CAN:SCHANNEL" + schannel + ":MFORmat STANdard");
                    System.Threading.Thread.Sleep(sleeptime);

                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANNEL" + channel + ":CAN:SCHANNEL" + schannel + ":MFORmat EXTended");
                    System.Threading.Thread.Sleep(sleeptime);

                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANNEL" + channel + ":CAN:SCHANNEL" + schannel + ":MID \"" + Mid + "\"");
                    System.Threading.Thread.Sleep(sleeptime);
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
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
        /// <param name="errorrate">误差</param>
        /// <returns>返回double[2]，返回占屏幕的比例值,跟示波器没有关系，自己根据示波器回读的点来进行计算</returns>
        public override double[] Oscillograph_Points(double[] data, double upvalue, double downvalue, int uptype, bool isAC, double errorrate)
        {
            int Length = data.Length;
            int CheckCount = Length / 50;


            double uperrorvalue = upvalue * errorrate;
            if (upvalue < 0)
            {
                uperrorvalue = -upvalue * errorrate;
            }


            double downerrorvalue = downvalue * errorrate;
            if (downvalue < 0)
            {
                downerrorvalue = downvalue * errorrate;
            }
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

                        if (value > downvalue - downerrorvalue)//最大值
                        {

                            if (++count1 > CheckCount)// 如果连续大于300
                            {
                                count1 = 0;
                                Record1 = index1 - CheckCount + 1;
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

                        if (value > upvalue - uperrorvalue)//最大值
                        {

                            if (++count2 > CheckCount)// 
                            {
                                count2 = 0;
                                Record2 = index2 - CheckCount + 1;
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

                        if (value > upvalue + uperrorvalue)//最大值
                        {



                        }
                        else
                        {
                            if (++count1 > CheckCount)// 
                            {
                                count1 = 0;
                                Record1 = index1 - CheckCount;
                                break;

                            }

                        }
                        index1++;
                    }
                    foreach (double value in data)
                    {

                        if (value > downvalue - downerrorvalue)//最大值
                        {



                        }
                        else
                        {
                            if (++count2 > CheckCount)// 
                            {
                                count2 = 0;
                                Record2 = index2 - CheckCount;
                                break;

                            }
                        }
                        index2++;
                    }
                }

                returnvalue[0] = Record1 / data.Length;//左边值

                //if (DataLenLength >= data.Length)
                //{
                //    int value = DataLenLength - data.Length;
                //    returnvalue[0] = (Record1 + value) / DataLenLength;//左边值
                //    returnvalue[1] = (Record2 + value) / DataLenLength;//右边值

                //}
                //else
                //{
                returnvalue[0] = Record1 / data.Length;//左边值
                returnvalue[1] = Record2 / data.Length;//右边值

                //}
                returnvalue[0] = returnvalue[0] * 10 - 5;
                returnvalue[1] = returnvalue[1] * 10 - 5;
                if (Record1 == 0)
                {
                    returnvalue[0] = -999;
                }
                if (Record2 == 0)
                {
                    returnvalue[1] = -999;
                }
                return returnvalue;
            }
            catch (Exception ex)
            {

            }
            return returnvalue;
        }








        /// <summary>
        /// 计算卡点值单一
        /// </summary>
        /// <param name="data">回读的示波器的点</param>
        /// <param name="value">上升或下降的比较值</param>
        /// <param name="uptype">0为上升，1为下降</param>
        /// <param name="isright"> 卡的点是否为右边</param>
        /// <param name="isAC">是否为交流</param>
        /// <param name="errorrate">误差</param>
        /// <returns>返回double，返回占屏幕的比例值，跟示波器没有关系，自己根据示波器回读的点来进行计算</returns>
        public override double Oscillograph_Points_Single(double[] data, double value, int uptype, bool isright, bool isAC, double errorrate)
        {
            lock (SynLock)
            {
                int Length = data.Length;
                int CheckCount = Length / 100;
                double errorvalue = 0;
                if (value == 0)
                {
                    errorvalue = -0.5;
                }
                if (value < 0)
                {
                    errorvalue = -(value * errorrate);
                }
                else
                {
                    errorvalue = value * errorrate;
                }



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

                    data = filtering.MedianFilter(data, 10);//中值滤波算法滤掉毛刺
                    double Record1 = 0;
                    double Record2 = 0;
                    int index1 = 0;
                    int count1 = 0;
                    int index2 = 0;
                    int count2 = 0;
                    if (uptype == 0)
                    {
                        foreach (double dvalue in data)
                        {

                            if (dvalue >= value - errorvalue)//最大值
                            {

                                if (++count1 > CheckCount)// 如果连续大于300
                                {
                                    count1 = 0;
                                    Record1 = index1 - CheckCount + 1;
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

                            if (dvalue <= value + errorvalue)//最大值
                            {
                                if (++count1 > CheckCount)// 
                                {
                                    count1 = 0;
                                    Record1 = index1 - CheckCount + 1;
                                    break;

                                }

                            }
                            index1++;
                        }
                    }
                    //TestData.Log.Write("Record1" + Record1);
                    //if (DataLenLength >= data.Length)
                    //{
                    //    int dvalue = DataLenLength - data.Length;
                    //    returnvalue = (Record1 + dvalue) / DataLenLength;//左边值

                    //}
                    //else
                    //{
                    returnvalue = Record1 / data.Length;//左边值


                    //}
                    if (Record1 == 0)
                    {

                        return -999;
                    }

                    returnvalue = returnvalue * 10 - 5;
                    return returnvalue;
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
                return returnvalue;
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
        /// <param name="errorrate">误差</param>
        /// <returns>返回double，返回占屏幕的比例值，跟示波器没有关系，自己根据示波器回读的点来进行计算</returns>
        public override double Oscillograph_Points_Single(double[] data, double value, int uptype, bool isright, bool isAC, double errorrate, double CheckCount)
        {
            lock (SynLock)
            {
                int Length = data.Length;

                double errorvalue = 0;
                if (value == 0)
                {
                    errorvalue = -0.5;
                }
                if (value < 0)
                {
                    errorvalue = -(value * errorrate);
                }
                else
                {
                    errorvalue = value * errorrate;
                }



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

                    data = filtering.MedianFilter(data, 10);//中值滤波算法滤掉毛刺
                    double Record1 = 0;
                    double Record2 = 0;
                    int index1 = 0;
                    int count1 = 0;
                    int index2 = 0;
                    int count2 = 0;
                    if (uptype == 0)
                    {
                        foreach (double dvalue in data)
                        {

                            if (dvalue >= value - errorvalue)//最大值
                            {

                                if (++count1 > CheckCount)// 如果连续大于300
                                {
                                    count1 = 0;
                                    Record1 = index1 - CheckCount + 1;
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

                            if (dvalue > value + errorvalue)//最大值
                            {



                            }
                            else
                            {
                                if (++count1 > CheckCount)// 
                                {
                                    count1 = 0;
                                    Record1 = index1 - CheckCount + 1;
                                    break;

                                }

                            }
                            index1++;
                        }
                    }
                    //TestData.Log.Write("Record1" + Record1);
                    //if (DataLenLength >= data.Length)
                    //{
                    //    int dvalue = DataLenLength - data.Length;
                    //    returnvalue = (Record1 + dvalue) / DataLenLength;//左边值

                    //}
                    //else
                    //{

                    double LENGTH = data.Length;
                    if (data.Length < DataLenLength)
                    {
                        LENGTH = DataLenLength;
                        double rvalue = DataLenLength - data.Length;
                        Record1 = Record1 + rvalue;
                    }


                    returnvalue = Record1 / data.Length;//左边值


                    //}
                    if (Record1 == 0)
                    {

                        return -999;
                    }

                    returnvalue = returnvalue * 10 - 5;
                    return returnvalue;
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
                return returnvalue;
            }
        }


        /// <summary>
        /// 计算卡点值单一取首个上升沿或者下降沿的时间
        /// </summary>
        /// <param name="data">回读的示波器的点</param>
        /// <param name="value">上升或下降的比较值</param>
        /// <param name="uptype">0为上升，1为下降</param>
        /// <param name="isright"> 卡的点是否为右边</param>
        /// <param name="isAC">是否为交流</param>
        /// <param name="errorrate">误差</param>
        /// <returns>返回double，返回占屏幕的比例值，跟示波器没有关系，自己根据示波器回读的点来进行计算</returns>
        public override double Oscillograph_Points_Single2(double[] data, double value, int uptype, bool isright, bool isAC, double errorrate, int? checkCount)
        {
            lock (SynLock)
            {
                int Length = data.Length;
                int CheckCount = checkCount == null ? Length / 50 : 100;
                double errorvalue = 0;
                if (value == 0)
                {
                    errorvalue = -0.5;
                }
                if (value < 0)
                {
                    errorvalue = -(value * errorrate);
                }
                else
                {
                    errorvalue = value * errorrate;
                }



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

                    data = filtering.MedianFilter(data, 10);//中值滤波算法滤掉毛刺
                    double Record1 = 0;
                    double Record2 = 0;
                    int index1 = 0;
                    int count1 = 0;
                    int index2 = 0;
                    int count2 = 0;
                    if (uptype == 0)
                    {
                        foreach (double dvalue in data)
                        {

                            if (dvalue > value + errorvalue)//最大值
                            {



                            }
                            else
                            {
                                if (++count1 > CheckCount)// 
                                {
                                    count1 = 0;
                                    Record1 = index1 - CheckCount + 1;
                                    break;

                                }

                            }
                            index1++;
                        }
                        double[] NewData = null;

                        int Count = (int)Record1;
                        if (Record1 < 100)
                        {
                            NewData = data;
                            Count = 0;
                        }
                        else
                        {

                            NewData = new double[(data.Length - Count)];

                            for (int i = Count; i < data.Length; i++)
                            {
                                if (i <= NewData.Length - 1)
                                {
                                    NewData[i - Count] = System.Math.Abs(data[i]);
                                }

                            }


                        }
                        Record1 = 0;
                        index1 = 0;
                        foreach (double dvalue in NewData)
                        {

                            if (dvalue > value - errorvalue)//最大值
                            {

                                if (++count1 > CheckCount)// 如果连续大于300
                                {
                                    count1 = 0;
                                    Record1 = index1 - CheckCount + 1;
                                    break;

                                }

                            }
                            else
                            {
                                count1 = 0;

                            }
                            index1++;
                        }
                        Record1 = Record1 + Count;

                    }
                    if (uptype == 1)
                    {

                        foreach (double dvalue in data)
                        {

                            if (dvalue > value - errorvalue)//最大值
                            {

                                if (++count1 > CheckCount)// 如果连续大于300
                                {
                                    count1 = 0;
                                    Record1 = index1 - CheckCount + 1;
                                    break;

                                }

                            }
                            else
                            {
                                count1 = 0;

                            }
                            index1++;
                        }

                        double[] NewData = null;

                        int Count = (int)Record1;
                        if (Record1 < 100)
                        {
                            NewData = data;
                            Count = 0;
                        }
                        else
                        {

                            NewData = new double[(data.Length - Count)];

                            for (int i = Count; i < data.Length; i++)
                            {
                                if (i <= NewData.Length - 1)
                                {
                                    NewData[i - Count] = System.Math.Abs(data[i]);
                                }

                            }


                        }


                        Record1 = 0;
                        index1 = 0;
                        foreach (double dvalue in NewData)
                        {

                            if (dvalue > value + errorvalue)//最大值
                            {



                            }
                            else
                            {
                                if (++count1 > CheckCount)// 
                                {
                                    count1 = 0;
                                    Record1 = index1 - CheckCount + 1;
                                    break;

                                }

                            }
                            index1++;
                        }
                        Record1 = Record1 + Count;
                    }
                    //TestData.Log.Write("Record1" + Record1);
                    //if (DataLenLength >= data.Length)
                    //{
                    //    int dvalue = DataLenLength - data.Length;
                    //    returnvalue = (Record1 + dvalue) / DataLenLength;//左边值

                    //}
                    //else
                    //{
                    returnvalue = Record1 / data.Length;//左边值


                    //}
                    if (Record1 == 0)
                    {

                        return -999;
                    }

                    returnvalue = returnvalue * 10 - 5;
                    return returnvalue;
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
                return returnvalue;
            }
        }



        /// <summary>
        /// 计算卡点值单一取第一个等于某个值的点
        /// </summary>
        /// <param name="data">回读的示波器的点</param>
        /// <param name="value">值</param>
        /// <param name="isAC">是否为交流</param>
        /// <returns>返回double，返回占屏幕的比例值，跟示波器没有关系，自己根据示波器回读的点来进行计算,0-10</returns>
        public override double Oscillograph_Points_Single3(double[] data, double value, bool isAC)
        {
            lock (SynLock)
            {
                //int Length = data.Length;
                //int CheckCount = Length / 50;

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

                    double Record1 = 0;
                    int index1 = 0;


                    foreach (double dvalue in data)
                    {

                        if (dvalue == value)//最大值
                        {
                            Record1 = index1;
                            break;

                        }
                        index1++;
                    }

                    ////TestData.Log.Write("Record1" + Record1);
                    //if (DataLenLength >= data.Length)
                    //{
                    //    int dvalue = DataLenLength - data.Length;
                    //    returnvalue = (Record1 + dvalue) / DataLenLength;//左边值

                    //}
                    //else
                    //{



                    //}
                    if (Record1 == 0)
                    {

                        return 10;
                    }
                    else
                    {
                        returnvalue = Record1 / data.Length;//左边值
                    }

                    returnvalue = returnvalue * 10;
                    return returnvalue;
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
                return returnvalue;
            }
        }



        /// <summary>
        /// 计算卡点值单一多个
        /// </summary>
        /// <param name="data">回读的示波器的点</param>
        /// <param name="value">上升或下降的比较值</param>
        /// <param name="uptype">0为上升，1为下降</param>
        /// <param name="isright"> 卡的点是否为右边</param>
        /// <param name="isAC">是否为交流</param>
        /// <param name="errorrate">误差</param>
        /// <param name="Index">到多少个值了</param>
        /// <param name="Count">取第几个值</param>
        /// <returns>返回double，返回占屏幕的比例值，跟示波器没有关系，自己根据示波器回读的点来进行计算</returns>
        public override double Oscillograph_Points_Single_Multiple(double[] OldData, double value, int uptype, bool isright, bool isAC, double errorrate, ref int Index, int Count, int? checkCount)
        {

            double[] data = new double[OldData.Length - Index];

            for (int i = Index; i < OldData.Length; i++)
            {
                if (i < data.Length - 1)
                {
                    data[i - Index] = OldData[i];
                }
            }



            int Length = data.Length;
            int CheckCount = checkCount == null ? Length / 50 : 100;
            double errorvalue = 0;

            if (value == 0)
            {
                errorvalue = -0.5;
            }
            if (value < 0)
            {
                errorvalue = -(value * errorrate);
            }
            else
            {
                errorvalue = value * errorrate;
            }



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

                data = filtering.MedianFilter(data, 10);//中值滤波算法滤掉毛刺
                int Record1 = 0;
                double Record2 = 0;
                int index1 = 0;
                int count1 = 0;
                int index2 = 0;
                int count2 = 0;
                if (Count == 0)
                {

                }
                else
                {
                    if (uptype == 0)
                    {
                        foreach (double dvalue in data)
                        {

                            if (dvalue > value + errorvalue)//最大值
                            {



                            }
                            else
                            {
                                if (++count1 > CheckCount)// 
                                {
                                    count1 = 0;
                                    Record1 = index1 - CheckCount + 1;
                                    break;

                                }

                            }
                            index1++;
                        }
                        double[] NewData = null;

                        int CountIndex = (int)Record1;
                        if (Record1 < 100)
                        {
                            NewData = data;
                            CountIndex = 0;
                        }
                        else
                        {

                            NewData = new double[(data.Length - CountIndex)];

                            for (int i = CountIndex; i < data.Length; i++)
                            {
                                if (i <= NewData.Length - 1)
                                {
                                    NewData[i - CountIndex] = System.Math.Abs(data[i]);
                                }

                            }


                        }
                        Record1 = 0;
                        index1 = 0;
                        foreach (double dvalue in NewData)
                        {

                            if (dvalue > value - errorvalue)//最大值
                            {

                                if (++count1 > CheckCount)// 如果连续大于300
                                {
                                    count1 = 0;
                                    Record1 = index1 - CheckCount + 1;
                                    break;

                                }

                            }
                            else
                            {
                                count1 = 0;

                            }
                            index1++;
                        }
                        Record1 = Record1 + CountIndex;

                    }
                    if (uptype == 1)
                    {

                        foreach (double dvalue in data)
                        {

                            if (dvalue > value - errorvalue)//最大值
                            {

                                if (++count1 > CheckCount)// 如果连续大于300
                                {
                                    count1 = 0;
                                    Record1 = index1 - CheckCount + 1;
                                    break;

                                }

                            }
                            else
                            {
                                count1 = 0;

                            }
                            index1++;
                        }

                        double[] NewData = null;

                        int CountIndex = (int)Record1;
                        if (Record1 < 100)
                        {
                            NewData = data;
                            CountIndex = 0;
                        }
                        else
                        {

                            NewData = new double[(data.Length - CountIndex)];

                            for (int i = CountIndex; i < data.Length; i++)
                            {
                                if (i <= NewData.Length - 1)
                                {
                                    NewData[i - CountIndex] = System.Math.Abs(data[i]);
                                }

                            }


                        }


                        Record1 = 0;
                        index1 = 0;
                        foreach (double dvalue in NewData)
                        {

                            if (dvalue > value + errorvalue)//最大值
                            {



                            }
                            else
                            {
                                if (++count1 > CheckCount)// 
                                {
                                    count1 = 0;
                                    Record1 = index1 - CheckCount + 1;
                                    break;

                                }

                            }
                            index1++;
                        }
                        Record1 = Record1 + CountIndex;
                    }
                    Record1 = Index + Record1;

                    Count--;
                    if (Count > 0)
                        Oscillograph_Points_Single_Multiple(OldData, value, uptype, isright, isAC, errorrate, ref Record1, Count, CheckCount);
                }

                Index = Record1;

                //if (DataLenLength >= OldData.Length)
                //{
                //    int dvalue = DataLenLength - OldData.Length;
                //    returnvalue = (Record1 + dvalue) / DataLenLength;//左边值

                //}
                //else
                //{
                returnvalue = (double)Record1 / data.Length;//左边值


                //}


                returnvalue = returnvalue * 10 - 5;
                return returnvalue;
            }
            catch (Exception ex)
            {
                SendExMsg(ex);
            }
            return returnvalue;


        }

        /// <summary>
        /// 删除所有的Channel显示
        /// <param name="group">显示第几个组</param>
        /// </summary>
        /// <returns>返回try catch的错误，否则为空</returns>
        public override void Remove_All_Channel(int group)
        {
            lock (SynLock)
            {
                try
                {

                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":DISPlay:GROup" + group + ":ACLear");
                    System.Threading.Thread.Sleep(sleeptime);
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
            }
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
        /// <summary>
        /// 取得实时的值
        /// </summary>
        /// <param name="channel">通道号</param>
        /// <param name="isCan">是否为CAN</param>
        /// <param name="schannel">子通道</param>
        /// <returns>返回值</returns>
        public override double GetChannelValue(int channel, bool isCan, int schannel)
        {
            lock (SynLock)
            {
                try
                {
                    if (isCan)
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":MONITOR:ASEND:CHAN" + channel + ":SCHannel" + schannel + "?");
                        //System.Threading.Thread.Sleep(sleeptime);
                    }
                    else
                    {
                        EquipMentPort.DLMOscilloscope.Send(tmpID, ":MONITOR:ASEND:CHAN" + channel + "?");
                        //System.Threading.Thread.Sleep(sleeptime);
                    }
                    string tString = ReciveDataString();

                    String[] tStringS = tString.Split(' ');

                    tStringS = tStringS.Where(x => x != " " && x != "" && x != null).ToArray();


                    if (tStringS.Length >= 2)
                    {

                        string value = Regex.Replace(tStringS[1], "[a - z]", "", RegexOptions.IgnoreCase);
                        decimal tDecimal = CommonOscilloscope.BackTryDecimalParse(value);
                        return (double)tDecimal;

                    }
                    return 0;
                }
                catch (Exception ex)
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// 读录波仪连接状态
        /// </summary>
        public override void Read_OscillographState()
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

                                    tcp.DLMOscilloscope.Send(tmpID, "*IDN?");
                                    int ret = 1;
                                    int rl = -1;
                                    ret = tcp.DLMOscilloscope.Receive(DeviceId, sbuff, sbuff.Capacity, ref rl);
                                    if (ret != 0)
                                    {
                                        string strIP = EquipMentPort.Ipaddress;
                                        tcp.DLMOscilloscope = new TMCTL();
                                        tcp.DLMOscilloscope.Initialize(TMCTL.TM_CTL_VXI11, strIP, ref tmpID);
                                        if (tmpID >= 0)
                                        {
                                            DeviceId = tmpID;
                                            SystemEvent.SendConnectState(true, this);
                                        }
                                        else
                                            SystemEvent.SendConnectState(false, this);
                                    }
                                    else
                                    {
                                        SystemEvent.SendConnectState(true, this);
                                    }

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
                                    string strIP = EquipMentPort.Ipaddress;
                                    tcp.DLMOscilloscope = new TMCTL();
                                    tcp.DLMOscilloscope.Initialize(TMCTL.TM_CTL_VXI11, strIP, ref tmpID);
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


        /// <summary>
        /// 设置通道的线性标尺
        /// </summary>
        /// <param name="channel">通道号</param>
        /// <param name="mode">模式AXB|OFF|P12|SHUNt</param>
        /// <param name="AVALue">A值</param>
        /// <param name="BVALue">B值</param>
        /// <param name="Unit">单位</param>
        /// <param name="DISPlaytype">显示模式EXPonent|FLOating</param>
        public override void Oscillograph_STRain_LSCale(int channel, string mode, double AVALue, double BVALue, string Unit, string DISPlaytype)
        {
            lock (SynLock)
            {
                try
                {
                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":LSCale:MODE " + mode);
                    System.Threading.Thread.Sleep(sleeptime);
                    Log.Log.LogMessage(":CHANnel" + channel + ":LSCale:MODE " + mode, EquipMentPort.EquiqMentClassName + "_日志");

                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":LSCale:AVALue " + AVALue);
                    System.Threading.Thread.Sleep(sleeptime);
                    Log.Log.LogMessage(":CHANnel" + channel + ":LSCale:AVALue " + AVALue, EquipMentPort.EquiqMentClassName + "_日志");

                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":LSCale:BVALue " + BVALue);
                    System.Threading.Thread.Sleep(sleeptime);
                    Log.Log.LogMessage(":CHANnel" + channel + ":LSCale:BVALue " + BVALue, EquipMentPort.EquiqMentClassName + "_日志");

                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":LSCale:UNIT \"" + Unit + "\"");
                    System.Threading.Thread.Sleep(sleeptime);
                    Log.Log.LogMessage(":CHANnel" + channel + ":LSCale:UNIT " + Unit, EquipMentPort.EquiqMentClassName + "_日志");

                    EquipMentPort.DLMOscilloscope.Send(tmpID, ":CHANnel" + channel + ":LSCale:DISPlaytype:MODE " + DISPlaytype);
                    System.Threading.Thread.Sleep(sleeptime);
                    Log.Log.LogMessage(":CHANnel" + channel + ":LSCale:DISPlaytype:MODE " + DISPlaytype, EquipMentPort.EquiqMentClassName + "_日志");
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
            }
        }
    }
}
