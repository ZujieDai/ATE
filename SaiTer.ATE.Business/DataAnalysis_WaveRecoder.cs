using SaiTer.ATE.DataModel.WaveRecoder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 录波板波形分析
    /// </summary>
    public static class DataAnalysis_WaveRecoder
    {
        /// <summary>
        /// 获取预充电压（这个从前端电压来得出数据）
        /// </summary>
        /// <param name="wd">波形数据</param>
        /// <param name="dStandardValue">预充电压标准值</param>
        /// <param name="Volt">返回预充电压</param>
        public static void GetPreChargeVolt(WaveData wd,double dStandardValue,ref double dVolt)
        {
            dVolt = 0;
            List<double> Volts = new List<double>();
            Volts.AddRange(wd.LinePoints_Y);
            List<double> Volts2 = new List<double>();
            bool bisOK = false;
            for (int i = 0; i < Volts.Count; i++)//先用小范围的数据
            {
                if (Volts[i] - dStandardValue < 10 && Volts[i] - dStandardValue > 1)//锁定范围
                {
                    for(int j = 0; j < 300; j++)
                    {
                        if(Math.Abs(Volts[i+j] - dStandardValue) < 10)
                        {
                            Volts2.Add(Volts[i + j]);
                        }
                        else
                        {
                            Volts2.Clear();
                            break;
                        }
                    }

                    if(Volts2.Count>30)
                    {
                        bisOK = true;
                        break;
                    }
                }

                if(Math.Abs(Volts[i] - dStandardValue) < Math.Abs(dVolt - dStandardValue))//这里取一个离目标值最近的值
                {
                    dVolt = Volts[i];
                }

            }

            if(!bisOK)//没找到小范围的再用大范围的找
            {
                for (int i = 0; i < Volts.Count; i++)
                {
                    if (Volts[i] - dStandardValue < 20 && Volts[i] - dStandardValue > 1)//锁定范围
                    {
                        for (int j = 0; j < 300; j++)
                        {
                            if (Math.Abs(Volts[i + j] - dStandardValue) < 20)
                            {
                                Volts2.Add(Volts[i + j]);
                            }
                            else
                            {
                                Volts2.Clear();
                                break;
                            }
                        }

                        if (Volts2.Count > 30)
                        {
                            bisOK = true;
                            break;
                        }
                    }

                    if (Math.Abs(Volts[i] - dStandardValue) < Math.Abs(dVolt - dStandardValue))//这里取一个离目标值最近的值
                    {
                        dVolt = Volts[i];
                    }

                }
            }

            if (Volts2.Count > 50)//能达到要求就取平均值
            {
                Volts2.Remove(Volts2.Max());//去掉一个最大值
                Volts2.Remove(Volts2.Min());//去掉一个最小值
                dVolt = Volts2.Average();//得出数据，取平均值
            }
        }

        /// <summary>
        /// 获取BCP电池电压
        /// </summary>
        /// <param name="wd">波形数据</param>
        /// <param name="dVolt">返回BCP电压值</param>
        public static void GetBCPBatteryVolt(WaveData wd,ref double dVolt)
        {
            dVolt = 0;
            List<double> Volts = new List<double>();
            Volts.AddRange(wd.LinePoints_Y);
            List<double> Volts2 = new List<double>();
            for (int i = 0; i < Volts.Count; i++)
            {
                if (Volts[i] !=0)//第一个不是0的BCP电压就是电池电压
                {
                    dVolt = Volts[i];
                }
            }

        }

        /// <summary>
        /// 获取急停信号时刻
        /// </summary>
        /// <param name="wd">波形数据</param>
        /// <param name="isRise">是否上升信号</param>
        /// <param name="ComparisonValue">急停信号比较值</param>
        /// <param name="dTime">返回急停信号发生时刻</param>
        public static void GetEmergencyStopTime(WaveData wd,bool isRise,double ComparisonValue , ref double dTime)
        {
            dTime = 0;
            List<double> wds = new List<double>();
            wds.AddRange(wd.LinePoints_Y);

            if(isRise)
            {
                for(int i=0;i<wds.Count;i++)
                {
                    if(wds[i] > ComparisonValue)//当信号值大于阈值时，默认产生急停信号
                    {
                        dTime = i;
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < wds.Count; i++)
                {
                    if (wds[i] < ComparisonValue)//当信号值小于阈值时，默认产生急停信号
                    {
                        dTime = i;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 获取直流信号时刻
        /// </summary>
        /// <param name="wd">波形数据</param>
        /// <param name="isRise">是否上升信号</param>
        /// <param name="ComparisonValue">直流信号比较值</param>
        /// <param name="dTime">返回直流信号发生时刻</param>
        public static void GetDCSingleTime(WaveData wd, bool isRise, double ComparisonValue, ref double dTime)
        {
            dTime = 0;
            List<double> wds = new List<double>();
            wds.AddRange(wd.LinePoints_Y);

            if (isRise)
            {
                for (int i = 0; i < wds.Count; i++)
                {
                    if (wds[i] > ComparisonValue)//当信号值大于阈值时，默认产生急停信号
                    {
                        dTime = i;
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < wds.Count; i++)
                {
                    if (wds[i] < ComparisonValue)//当信号值小于阈值时，默认产生急停信号
                    {
                        dTime = i;
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// 获取直流信号时刻
        /// </summary>
        /// <param name="wd">波形数据</param>
        /// <param name="isRise">是否上升信号</param>
        /// <param name="StartValue">起始信号值</param>
        /// <param name="ComparisonValue">直流信号比较值</param>
        /// <param name="dTime">返回直流信号发生时刻</param>
        public static void GetDCSingleTime(WaveData wd, bool isRise, double StartValue , double ComparisonValue, ref double dTime)
        {
            dTime = 0;
            List<double> wds = new List<double>();
            wds.AddRange(wd.LinePoints_Y);

            if (isRise)
            {
                int icount = 0;
                for (int i = 0; i < wds.Count; i++)
                {
                    if (wds[i] < StartValue)
                    {
                        icount = i;
                        break;
                    }
                }
                for (int i = icount; i < wds.Count; i++)
                {
                    if (wds[i] > ComparisonValue)//当信号值大于阈值时，默认产生急停信号
                    {
                        dTime = i;
                        break;
                    }
                }
            }
            else
            {
                //如果是下降信号，先找到上升信号的时刻，再从上升信号的时刻开始找下降信号的时刻，这样可以避免波形中有多段相同的波形时，找错时刻的问题
                int icount = 0;
                for (int i = 0; i < wds.Count; i++)
                {
                    if (wds[i] > StartValue)
                    {
                        icount = i;
                        break;
                    }
                }

                for (int i = icount; i < wds.Count; i++)
                {
                    if (wds[i] < ComparisonValue)//当信号值小于阈值时，默认产生急停信号
                    {
                        dTime = i;
                        break;
                    }
                }

            }
        }
        /// <summary>
        /// 获取直流信号时刻(可能有多段相同的波形)
        /// </summary>
        /// <param name="wd">波形数据</param>
        /// <param name="isRise">是否上升信号</param>
        /// <param name="ComparisonValue">直流信号比较值（这个需要留余量）</param>
        /// <param name="dTime">返回直流信号发生时刻</param>
        /// <param name="dStart">起始X坐标（时间坐标）</param>
        /// <param name="isReverseOrder">是否倒序</param>
        public static void GetDCSingleTime_M(WaveData wd, bool isRise, double ComparisonValue, ref double dTime, double dStart = 0,bool isReverseOrder = false)
        {
            dTime = 0;
            List<double> wds = new List<double>();
            wds.AddRange(wd.LinePoints_Y);
            if (dStart >= wds.Count)
            {
                dStart = wds.Count - 1;
            }
            int iStart = (int)dStart;

            if (isRise)
            {
                if (!isReverseOrder)
                {
                    for (int i = iStart; i < wds.Count; i++)
                    {
                        if (wds[i] >= ComparisonValue)//当信号值大于阈值时，默认产生急停信号
                        {
                            dTime = i;
                            break;
                        }
                    }
                }
                else
                {
                    for (int i = iStart; i > 0; i--)
                    {
                        if (wds[i] >= ComparisonValue)//当信号值大于阈值时，默认产生急停信号
                        {
                            dTime = i;
                            break;
                        }
                    }
                }
            }
            else
            {

                if (!isReverseOrder)
                {
                    for (int i = iStart; i < wds.Count; i++)
                    {
                        if (wds[i] <= ComparisonValue)//当信号值小于阈值时，默认产生急停信号
                        {
                            dTime = i;
                            break;
                        }
                    }
                }
                else
                {
                    for (int i = iStart; i > 0; i--)
                    {
                        if (wds[i] <= ComparisonValue)//当信号值小于阈值时，默认产生急停信号
                        {
                            dTime = i;
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取AC信号上升时刻
        /// </summary>
        /// <param name="wd">波形数据</param>
        /// <param name="ComparisonValue">比较值</param>
        /// <param name="dTime">返回交流信号发生时刻</param>
        public static void GetACUPTime(WaveData wd,  double ComparisonValue, ref double dTime)
        {
            dTime = 0;
            List<double> wds = new List<double>();
            wds.AddRange(wd.LinePoints_Y);

            int icount = 0;
            for (int i = 0; i < wds.Count; i++)
            {
                if (wds[i] > ComparisonValue)//当信号值大于阈值时，默认产生急停信号
                {
                    icount++;
                }
                else
                {
                    icount = 0;
                }

                if(icount>10)
                {
                    dTime = i - 10;
                    break;
                }

            }
        }

        /// <summary>
        /// 获取AC信号下降时刻
        /// </summary>
        /// <param name="wd">波形数据</param>
        /// <param name="ComparisonValue">比较值</param>
        /// <param name="dTime">返回交流信号发生时刻</param>
        public static void GetACDownTime(WaveData wd, double ComparisonValue, ref double dTime)
        {
            dTime = 0;
            List<double> wds = new List<double>();
            wds.AddRange(wd.LinePoints_Y);

            int icount = 0;
            for (int i = wds.Count - 1; i >= 0; i--)
            {
                if (wds[i] > ComparisonValue)//当信号值大于阈值时，默认产生急停信号
                {
                    icount++;
                }
                else
                {
                    icount = 0;
                }

                if (icount > 10)
                {
                    dTime = i + 10;
                    break;
                }

            }
        }
        /// <summary>
        /// 获取CP下降时刻
        /// </summary>
        /// <param name="wd">波形数据</param>
        /// <param name="ComparisonValue">比较值</param>
        /// <param name="dTime">返回CP信号发生时刻</param>
        public static void GetCPPWMDownTime(WaveData wd, double ComparisonValue, ref double dTime)
        {
            dTime = 0;
            List<double> wds = new List<double>();
            wds.AddRange(wd.LinePoints_Y);

            int icount = 0;
            for (int i = wds.Count - 1; i >= 0; i--)
            {
                if (wds[i] > ComparisonValue)//当信号值大于阈值时，默认产生急停信号
                {
                    icount++;
                }
                else
                {
                    icount = 0;
                }

                if (icount > 5)
                {
                    dTime = i + 5;
                    break;
                }

            }
        }
        /// <summary>
        /// 获取CP上升时刻
        /// </summary>
        /// <param name="wd">波形数据</param>
        /// <param name="ComparisonValue">比较值</param>
        /// <param name="dTime">返回CP信号发生时刻</param>
        public static void GetCPPWMUPTime(WaveData wd, double ComparisonValue, ref double dTime)
        {
            dTime = 0;
            List<double> wds = new List<double>();
            wds.AddRange(wd.LinePoints_Y);

            int icount = 0;
            for (int i = 0; i < wds.Count; i++)
            {
                if (wds[i] > ComparisonValue)//当信号值大于阈值时，默认产生急停信号
                {
                    icount++;
                }
                else
                {
                    icount = 0;
                }

                if (icount > 5)
                {
                    dTime = i - 5;
                    break;
                }

            }
        }


        /// <summary>
        /// 获取CP的PWM信号切换为电平信号的时刻
        /// </summary>
        /// <param name="wd">波形数据</param>
        /// <param name="dTime">返回CP信号发生时刻</param>
        public static void GetCPPWMToLevelTime(WaveData wd, ref double dTime)
        {
            dTime = 0;
            List<double> wds = new List<double>();
            wds.AddRange(wd.LinePoints_Y);

            int icount = 0;
            for (int i = wds.Count - 1; i >= 0; i--)
            {
                if (wds[i] < -6)
                {
                    icount++;
                }
                else
                {
                    icount = 0;
                }

                if (icount > 5)
                {
                    dTime = i + 5;
                    break;
                }

            }
        }

        /// <summary>
        /// 获取CP电平信号切换为PWM信号的时刻
        /// </summary>
        /// <param name="wd">波形数据</param>
        /// <param name="dTime">返回CP信号发生时刻</param>
        public static void GetCPLevelToPWMTime(WaveData wd, ref double dTime)
        {
            dTime = 0;
            List<double> wds = new List<double>();
            wds.AddRange(wd.LinePoints_Y);

            int icount = 0;
            for (int i = 0; i < wds.Count; i++)
            {
                if (wds[i] <-6 )
                {
                    icount++;
                }
                else
                {
                    icount = 0;
                }

                if (icount > 5)
                {
                    dTime = i - 5;
                    break;
                }

            }
        }

        /// <summary>
        /// CAN报文值的分析时间
        /// </summary>
        /// <param name="wd">波形数据</param>
        /// <param name="ComparisonValue">判定阈值</param>
        /// <param name="isRise">是否上升</param>
        /// <param name="dTime">返回时间</param>
        public static void GetCANMsgTime(WaveData wd,double ComparisonValue,bool isRise, ref double dTime)
        {
            dTime = 0;
            List<double> wds = new List<double>();
            wds.AddRange(wd.LinePoints_Y);

            int icount = 0;
            if (isRise)//上升
            {
                for (int i = 0; i < wds.Count; i++)
                {
                    if (wds[i] >= ComparisonValue)
                    {
                        icount++;
                    }
                    else
                    {
                        icount = 0;
                    }

                    if (icount > 5)
                    {
                        dTime = i - 5;
                        break;
                    }
                }
            }
            else
            {
                for (int i = wds.Count-1; i >0 ; i--)
                {
                    if (wds[i] >= ComparisonValue)
                    {
                        icount++;
                    }
                    else
                    {
                        icount = 0;
                    }

                    if (icount > 5)
                    {
                        dTime = i + 5;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 根据时间坐标获取波形点的值
        /// </summary>
        /// <param name="wd"></param>
        /// <param name="WavePoint"></param>
        /// <param name="dValue"></param>
        public static double GetWavePointVave(WaveData wd, int WavePoint)
        {
            double dValue = 0;
            List<double> wds = new List<double>();
            wds.AddRange(wd.LinePoints_Y);

            if(WavePoint<wds.Count && WavePoint>0)
            {
                dValue = wds[WavePoint];
            }

            return dValue;
        }
        /// <summary>
        /// 获取最大值
        /// </summary>
        /// <param name="wd"></param>
        /// <param name="iStartIndex">起始位置</param>
        /// <returns></returns>
        public static double GetWavePointMaxVave(WaveData wd, int iStartIndex = 0)
        {
            double dValue = 0;
            List<double> wds = new List<double>();
            //wds.AddRange(wd.LinePoints_Y);
            wds.AddRange(wd.LinePoints_Y.GetRange(iStartIndex, wd.LinePoints_Y.Count - 1 - iStartIndex));

            dValue = wds.Max();

            return dValue;
        }

        /// <summary>
        /// 获取最小值
        /// </summary>
        /// <param name="wd"></param>
        /// <returns></returns>
        public static double GetWavePointMinVave(WaveData wd)
        {
            double dValue = 0;
            List<double> wds = new List<double>();
            wds.AddRange(wd.LinePoints_Y);

            dValue = wds.Min();

            return dValue;
        }

        /// <summary>
        /// 获取第一个非零值
        /// </summary>
        /// <param name="wd"></param>
        /// <param name="dVolt"></param>
        public static double GetNotZeroValue_first(WaveData wd)
        {
            double  dValue = 0;
            List<double> Volts = new List<double>();
            Volts.AddRange(wd.LinePoints_Y);
            List<double> Volts2 = new List<double>();
            for (int i = 0; i < Volts.Count; i++)
            {
                if (Volts[i] != 0)//第一个不是0的值
                {
                    dValue = Volts[i];
                }
            }
            return dValue;
        }

    }
}
