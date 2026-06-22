using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.CAN;
using SaiTer.ATE.DataModel.WaveRecoder;
using SaiTer.ATE.EquipMent.WaveRecorder;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 设备-赛特录波板3.0(包含数字通道)
    /// </summary>
    public class emtWaveRecoderBoard30 : EquipMentBase
    {
        public static object SynLock = new object();
        WaveRecoderBoard30_Protocol protocol = new WaveRecoderBoard30_Protocol();
        FrmWaveRecoder FrmWaveRecoder;
        List<double> ChannelRatio = new List<double>();//通道互感器比例
        /// <summary>
        /// 波形数据（保存在内存中，读取过就有，开始录波之前会清空数据）
        /// </summary>
        List<WaveData> WaveDatas_T = new List<WaveData>();
        /// <summary>
        /// 起始点的时间，和数字通道做对比
        /// </summary>
        ulong iStartPoint = 0;
        /// <summary>
        /// 波形数据数量
        /// </summary>
        long iPointCount = 0;
        bool IsDebug = false;

        //private string sIsDebug = ConfigurationManager.AppSettings["IsDebug"].ToString().ToUpper();
        public emtWaveRecoderBoard30(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("录波板");
            if (Application.OpenForms["FrmMain"].InvokeRequired)
            {
                Application.OpenForms["FrmMain"].Invoke(new Action(() =>
                {
                    FrmWaveRecoder = new FrmWaveRecoder();
                }));
            }
            else
            {
                FrmWaveRecoder = new FrmWaveRecoder();
            }

            SetImagePath();
            if (ConfigurationManager.AppSettings["IsDebug"] != null)
            {
                string sIsDebug = ConfigurationManager.AppSettings["IsDebug"].ToString().ToUpper();
                if (sIsDebug == "TRUE")
                {
                    IsDebug = true;
                }
                else
                {
                    IsDebug = false;
                }
            }

            //增加通道变比可配置
            string sRatio = "";
            string[] Ratios = null;
            sRatio = ConfigurationManager.AppSettings["ChannelRatio"]?.ToString().Trim();
            sRatio = sRatio == null ? "" : sRatio;
            Ratios = sRatio.Split('|');
            for (int i = 0; i < Ratios.Length; i++)
            {
                if (Ratios[i].Trim() == "")
                {
                    Ratios[i] = "1";
                }
            }
            for (int i = 0; i < 16; i++)//设16通道
            {

                if (i < Ratios.Length)
                {
                    ChannelRatio.Add(Convert.ToDouble(Ratios[i]));
                }
                else
                {
                    if (i == 1)
                    {
                        ChannelRatio.Add(500);//2通道电流比例默认为500
                    }
                    else
                    {
                        ChannelRatio.Add(1);//默认为1
                    }
                }
            }
            //FrmWaveRecoder.Show();

        }

        /// <summary>
        /// 添加波形数据
        /// </summary>
        /// <param name="data"></param>
        private void AddWaveData(WaveData data)
        {
            for(int i=0;i<WaveDatas_T.Count;i++)
            {
                if(WaveDatas_T[i].ChannelType == data.ChannelType
                    && WaveDatas_T[i].Channel == data.Channel
                    && WaveDatas_T[i].SubChannel == data.SubChannel)
                {
                    WaveDatas_T[i] = data;
                    return;
                }
            }

            WaveDatas_T.Add(data);
        }


        public override void ReadWaveRecoderBoard_StateData()
        {
            SystemEvent.SendConnectState(false, this);

            while (true)
            {

                if (AutoReadData)
                {

                    byte[] RevMsgData = null;
                    byte[] WriteBuffer = protocol.ReadCanMsg();
                    if (EquipMentPort != null)
                    {
                        for (int i = 0; i < ReConnNum; i++)
                        {
                            DataBuf.Clear();
                            //string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                            //SendMsgToFile(EquipMentName+"发送数据：" + strTemp);
                            EquipMentPort.SendData(WriteBuffer);
                            RevMsgData = RevEquipMentData();

                            if (RevMsgData != null)
                            {
                                SystemEvent.SendConnectState(true, this);
                                return;
                            }
                            else
                            {
                                Thread.Sleep(100);
                                EquipMentPort.SendData(WriteBuffer);
                                RevMsgData = RevEquipMentData();
                                if (RevMsgData != null)
                                {
                                    SystemEvent.SendConnectState(true, this);
                                    return;
                                }
                                //SendMsgToFile(EquipMentName+"读状态数据失败，设备没返回数据！");
                                //总是为空导致出现红叉，先屏蔽
                                //SystemEvent.SendConnectState(false, this);
                                continue;
                            }

                        }
                    }
                    else
                    {
                        SendMsgToFile(EquipMentName + "通道对象不存在，请检查");
                        SystemEvent.SendConnectState(false, this);
                    }
                    Thread.Sleep(300);
                }
            }
        }


        /// <summary>
        /// 将数据反转（以字节对换位置）
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private string ReversalData2Len(string data)
        {
            string tmp = "";
            tmp = data.Substring(2, 2) + data.Substring(0, 2);
            return tmp;
        }

        private bool SendData(byte[] WriteBuffer)
        {
            lock (SynLock)
            {
                AutoReadData = false;
                Thread.Sleep(500);
                if (EquipMentPort != null)
                {
                    for (int i = 0; i < ReConnNum; i++)
                    {
                        DataBuf.Clear();
                        string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                        // SendMsgToFile("交流源发送数据：" + strTemp);
                        EquipMentPort.SendData(WriteBuffer);
                    }
                    AutoReadData = true;
                    return true;
                }
                else
                {
                    SendMsgToFile("录波板通道对象不存在，请检查录波板通道");
                    AutoReadData = true;
                    return false;
                }
            }
        }

        public override void WaveRecoder_SetSamplingRate(double data)
        {
            int itmp = (int)data;
            if (itmp != 1 && itmp != 10 && itmp != 100)
            {
                itmp = 1;
            }
            byte[] WriteBuffer = protocol.SetCyl(itmp);
            SendData(WriteBuffer);
        }

        public override void WaveRecoder_Start()
        {
            byte[] WriteBuffer = protocol.StartWave();
            SendData(WriteBuffer);
            iStartPoint = 0;//开始录波之前先把起始点归零
            iPointCount = 0;//开始录波之前先把长度归零
            Cursor1 = 0;
            Cursor2 = 0;
            Time_Cursor = 0;
            IsCursor = false;//此时未设置光标
            WaveDatas_T.Clear();//清空原来的波形
        }

        public override void WaveRecoder_Stop()
        {
            byte[] WriteBuffer = protocol.StopWave();
            SendData(WriteBuffer);
        }

        public override void WaveRecoder_ReadChannelData(int channnel, ref WaveData data)
        {
            try
            {
                if (channnel > 8) return;
                int iFrameLen = 0;//报文帧长度
                bool isMsgOver = false;//报文帧是否接收完毕
                byte[] RevMsgData = null;//接收数据
                SendData(protocol.ReadChannelData(channnel));
                RevMsgData = RevEquipMentData();


                StringBuilder sbtmp = new StringBuilder(100);

                DateTime LastMsgTime = DateTime.Now;

                if(RevMsgData==null)
                {
                    return;
                }

                if (RevMsgData[0] == 0x5A && RevMsgData[1] == 0xA5 && RevMsgData[6] == 0x80 && RevMsgData[7] == 0x00 && RevMsgData[8] == 0x03)
                {
                    StringBuilder sbLen = new StringBuilder();
                    //在固定位置取长度
                    sbLen.Append(Convert.ToString(RevMsgData[2], 16).PadLeft(2, '0'));
                    sbLen.Append(Convert.ToString(RevMsgData[3], 16).PadLeft(2, '0'));
                    sbLen.Append(Convert.ToString(RevMsgData[4], 16).PadLeft(2, '0'));
                    sbLen.Append(Convert.ToString(RevMsgData[5], 16).PadLeft(2, '0'));
                    iFrameLen = (int)Convert.ToUInt32(sbLen.ToString(), 16);
                    //判断是否接收完毕
                    if (RevMsgData.Length == iFrameLen)
                    {
                        isMsgOver = true;
                    }
                }

                //未接收完毕，直接退出
                if (!isMsgOver)
                {
                    return;
                }

                StringBuilder multMsg=new StringBuilder();
                multMsg.Append(BitConverter.ToString(RevMsgData).Replace("-", ""));
                string sdtmp = multMsg.ToString().Replace(" ", "");//去掉空格
                string sv = "";
                double dtmp = 0;
                long wavePoint = 0;//波形图中的有效点数

                if (sdtmp.Length < 20) return;

                iStartPoint = Convert.ToUInt32(sdtmp.Substring(24, 8), 16);//得出起始时间点
                wavePoint = (int)(iFrameLen - 12 - 4) / 2;//每个数据点有2个字节，分辨率0.1
                if (sdtmp.Length < 32 + (wavePoint * 4)) return;//长度不匹配
                iPointCount = wavePoint;
                List<double> dataY = new List<double>();

                for (int i = 0; i < wavePoint; i++)//4个字符(2个字节)为一个数据，
                {
                    sv = sdtmp.Substring(i * 4 + 32, 4);
                    sv = ReversalData2Len(sv);
                    dtmp = Convert.ToInt16(sv, 16);
                    //这里还差一步，需要增加变比算法
                    dtmp = dtmp * GetZoomX(channnel);
                    //dataY[i] = dtmp;
                    dataY.Add(dtmp);
                }

                data.Channel = channnel;
                data.SubChannel = 0;
                data.ChannelType = 0;
                data.LinePoints_Y = dataY;

                AddWaveData(data);

            }
            catch (Exception ex)
            {
                SendMsgToFile(ex.Message);
            }
        }

        public override void WaveRecoder_ReadDigitalChannelData(int channnel, int subchannel, ref WaveData data)
        {
            try
            {
                if (channnel > 160 || channnel <= 0) return;
                int iFrameLen = 0;//报文帧长度
                bool isMsgOver = false;//报文帧是否接收完毕
                byte[] RevMsgData = null;//接收数据
                if (iStartPoint == 0)//如果起始点为0，则通过读取1通道的数据来获取起始点
                {
                    WaveRecoder_ReadChannelData(1, ref data);
                }
                if (iStartPoint == 0)
                {
                    SendMsgToFile("数字通道起始点定位失败！！！");
                }
                if (iPointCount == 0)
                {
                    SendMsgToFile("数字通道长度获取失败！！！");
                }
                SendData(protocol.ReadDigitalChannelData(channnel));
                RevMsgData = RevEquipMentData();
                StringBuilder sbtmp = new StringBuilder(100);


                DateTime LastMsgTime = DateTime.Now;


                if (RevMsgData[0] == 0x5A && RevMsgData[1] == 0xA5 && RevMsgData[6] == 0x80 && RevMsgData[7] == 0x00 && RevMsgData[8] == 0x14)
                {
                    StringBuilder sbLen = new StringBuilder();
                    //在固定位置取长度
                    sbLen.Append(Convert.ToString(RevMsgData[2], 16).PadLeft(2, '0'));
                    sbLen.Append(Convert.ToString(RevMsgData[3], 16).PadLeft(2, '0'));
                    sbLen.Append(Convert.ToString(RevMsgData[4], 16).PadLeft(2, '0'));
                    sbLen.Append(Convert.ToString(RevMsgData[5], 16).PadLeft(2, '0'));
                    iFrameLen = (int)Convert.ToUInt32(sbLen.ToString(), 16);
                    //判断是否接收完毕
                    if (RevMsgData.Length == iFrameLen)
                    {
                        isMsgOver = true;
                    }
                }

                //未接收完毕，直接退出
                if (!isMsgOver)
                {
                    return;
                }

                int iDataLen_Single = 0;//单个数据长度
                int iDataCount = 0;//数据数量
                iDataLen_Single = (int)Convert.ToUInt32(Convert.ToString(RevMsgData[11], 16).PadLeft(2, '0'), 16);
                iDataCount = (iFrameLen - 12) / (iDataLen_Single + 4);

                List<double> lst_t = new List<double>();
                List<double> lst_v = new List<double>();
                StringBuilder multMsg = new StringBuilder();
                multMsg.Append(BitConverter.ToString(RevMsgData).Replace("-", ""));
                string sdtmp = multMsg.ToString().Replace(" ", "");//去掉空格
                string sv = "";//值
                string st = "";//时间
                double dtmp = 0;
                double dttmp = 0;
                long wavePoint = 0;//波形图中的有效点数

                if (sdtmp.Length < 19) return;



                wavePoint = iDataCount;//每个数据点有N+4个字节，4个字节的时间戳，N个字节的数据
                if (sdtmp.Length < 12 * 2 + (wavePoint * (iDataLen_Single + 4) * 2))
                {
                    SendMsgToFile("数字通道长度不匹配！！！");
                    return;//长度不匹配
                }

                for (int i = 0; i < wavePoint; i++)//4个字符(2个字节)为一个数据，最后8个字符(4个字节)为时间坐标
                {
                    st = sdtmp.Substring(i * ((iDataLen_Single + 4) * 2) + 24, 4 * 2);//4个字节的时间
                    sv = sdtmp.Substring(i * ((iDataLen_Single + 4) * 2) + 24 + 8, iDataLen_Single * 2);
                    dttmp = Convert.ToUInt64(st, 16);//时间
                    dttmp = dttmp - iStartPoint;//需要得出相对时间
                    if (channnel == 4 || channnel == 5 || channnel == 10 || channnel == 11 || channnel == 52)//这里有些数据需要正数
                    {
                        dtmp = Convert.ToUInt32(sv, 16);//值
                    }
                    else
                    {
                        dtmp = Convert.ToInt32(sv, 16);//值
                    }

                    if (dttmp >= 0)//只取录波开始后的数据
                    {
                        lst_t.Add(dttmp);
                        lst_v.Add(dtmp);
                    }
                }

                data.Channel = channnel;
                data.SubChannel = subchannel;
                data.ChannelType = 1;
                data.LinePoints_Y = LoadDigitalChannelData(channnel, lst_t, lst_v, subchannel);

                AddWaveData(data);
            }
            catch (Exception ex)
            {
                SendMsgToFile(ex.Message);
            }
        }

        private List<double> LoadDigitalChannelData(int iChnelNum, List<double> lstt, List<double> lstv, int SubChnelNum = 0)
        {
            double XZoom = 1;//缩放倍数
            double dOffSet = 0;//偏移量
            switch (iChnelNum)
            {
                case 1:
                    XZoom = 1;
                    dOffSet = 0;
                    break;
                case 2:
                case 8:
                case 9:
                case 19:
                case 20:
                case 22:
                case 23:
                case 25:
                case 26:
                case 45:
                case 51:
                case 129:
                case 132:
                case 137:
                    XZoom = 0.1;
                    dOffSet = 0;
                    break;
                case 17:
                case 46:
                case 47:
                    XZoom = 0.01;
                    dOffSet = 0;
                    break;
                case 18:
                case 27:
                case 28:
                case 130:
                case 133:
                case 138:
                    XZoom = 0.1;
                    dOffSet = -400;
                    break;
                case 21:
                case 48:
                case 49:
                case 142:
                case 144:
                    XZoom = 1;
                    dOffSet = -50;
                    break;
                default:
                    XZoom = 1;
                    dOffSet = 0;
                    break;
            }

            List<double> lsttmp = new List<double>();
            long tmpStart = (long)iStartPoint;
            long tmpCount = iPointCount;

            if (tmpStart == 0)//起点为0
            {
                return null;
            }
            if (tmpCount == 0)//长度为0
            {
                return null;
            }
            if (tmpCount == 0)
            {
                tmpCount = 1;
            }

            //把值进行换算
            for (int i = 0; i < lstv.Count; i++)
            {
                if (iChnelNum == 18 || iChnelNum == 27 || iChnelNum == 28 || iChnelNum == 130 || iChnelNum == 133 || iChnelNum == 138)
                {
                    lstv[i] =Math.Abs( (lstv[i] * XZoom) + dOffSet);//这里的电流解析都用正数表示，方便使用
                }
                else
                {
                    lstv[i] = (lstv[i] * XZoom) + dOffSet;
                    if (SubChnelNum != 0)//如果是子通道数据，需要再继续解析换算
                    {
                        lstv[i] = GetSubChannelData(iChnelNum, SubChnelNum, lstv[i]);
                    }
                }
            }

            //按照波形的长度，生成新的波形数据
            int iindex = 0;
            for (int i = 0; i < tmpCount; i++)
            {
                if (iindex < lstt.Count)
                {
                    if (i == lstt[iindex])
                    {
                        lsttmp.Add(lstv[iindex]);
                        iindex++;
                    }
                    else
                    {
                        if (i == 0)
                        {
                            lsttmp.Add(0);
                        }
                        else
                        {
                            lsttmp.Add(lsttmp[i - 1]);
                        }
                    }
                }
                else
                {
                    if (i == 0)
                    {
                        lsttmp.Add(0);
                    }
                    else
                    {
                        lsttmp.Add(lsttmp[i - 1]);
                    }
                }
            }


            return lsttmp;

        }

        private double GetSubChannelData(int iChannel, int iSubChannel, double dOriginalData)
        {
            double dValue = 0;
            if (iSubChannel == 0) return dOriginalData;//子通道为0就是原始数据
            string sbin = "";
            int itmp = (int)dOriginalData;
            if (iChannel == 31 || iChannel == 32 || iChannel == 33 || iChannel == 34
                || iChannel == 35 || iChannel == 36 || iChannel == 37 || iChannel == 38
                || iChannel == 39 || iChannel == 41 || iChannel == 42 || iChannel == 44
                || iChannel == 146 || iChannel == 147)
            {
                if (iSubChannel > 4) return dOriginalData;
                sbin = Convert.ToString(itmp, 2).PadLeft(8, '0');//转换为2进制的字符串
                sbin = sbin.Substring(8 - (iSubChannel * 2), 2);
                dValue = Convert.ToUInt32(sbin);
            }
            else if (iChannel == 40 || iChannel == 43)
            {
                if (iSubChannel > 8) return dOriginalData;
                sbin = Convert.ToString(itmp, 2).PadLeft(16, '0');//转换为2进制的字符串
                sbin = sbin.Substring(16 - (iSubChannel * 2), 2);
                dValue = Convert.ToUInt32(sbin);
            }
            else
            {
                return dOriginalData;
            }


            return dValue;
        }

        public override void WaveRecoderSaveScreen(ref string path)
        {
            string imageName = ImagePathFile + System.DateTime.Now.ToString("yyyy-MM-dd HH：mm：ss：ffff") + ".bmp";
            //Thread.Sleep(1000);
            FrmWaveRecoder.InitWaveData(WaveDatas_T);//加载波形数据
            WaveRecoder_SetCursor();//设置光标
            //Thread.Sleep(2000);//画完图后等2秒
            if (IsDebug)
            {
                FrmWaveRecoder.ShowDialog();//调试模式可以打开直接看波形图
            }
            FrmWaveRecoder.SaveImage(System.IO.Path.Combine(ImagePathFile, imageName));//保存图片到指定位置
            path = imageName.Substring(BaseDirectoryPath.Length);//去掉基目录路径后
            Thread.Sleep(100);
        }

        public override void WaveRecoder_InitWaveData(List<WaveData> waveDatas)
        {
            FrmWaveRecoder.InitWaveData(waveDatas);
        }

        public void WaveRecoder_SetCursor()
        {
            if (!IsCursor) return;

            FrmWaveRecoder.SetCursor(Cursor1, 1, 0);//目前只设置了X轴
            FrmWaveRecoder.SetCursor(Cursor2, 2, 0);//目前只设置了X轴

            FrmWaveRecoder.ShowCursorInfo(Cursor1, Cursor2);
        }

        public double GetZoomX(int iChannel)
        {
            double Xs = GetXs(iChannel);
            double ZoomX = (Xs * 10) / 32767;
            //if(iChannel==2)
            //{
            //    ZoomX = ZoomX * 500;//目前电流互感器是5000：1的（需要做成可设置）
            //}
            int iRatioIndex = iChannel - 1 < 0 ? 0 : iChannel - 1;
            ZoomX = ZoomX * ChannelRatio[iRatioIndex];
            return ZoomX;
        }

        public double GetXs(int iChannel)
        {
            //这个是目前录波板用的系数
            double Xs = 1;
            switch(iChannel)
            {
                case 1:
                case 6:
                    Xs = 100;
                    break;
                case 2:
                    Xs = 1;
                    break;
                case 3:
                case 7:
                case 8:
                    Xs = 10;
                    break;
                case 4:
                case 5:
                    Xs = 5;
                    break;
            }

            return Xs;
        }



    }


}
