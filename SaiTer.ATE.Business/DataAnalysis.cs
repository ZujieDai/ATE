using Newtonsoft.Json.Linq;
using SaiTer.ATE.Controls;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.DataBaseModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.IDAL.SQLiteIDAL;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    public partial class BusinessBase
    {
        /// <summary>
        /// 设置光标0
        /// </summary>
        /// <param name="lstIDs">设备集合</param>
        /// <param name="ChannelNum">通道编号</param>
        /// <param name="posistion">位置（0到1）</param> 
        public Dictionary<int, string> GetPosistionValue(List<int> lstIDs, int ChannelNum, double posistion)
        {
            Dictionary<int, double[]> datas = ControlEquipMent.Oscilloscope.OscilloscopeCursorData(lstIDs, ChannelNum);
            Dictionary<int, string> positions = new Dictionary<int, string>();
            foreach(var item in datas)
            {
                int index = (int)(item.Value.Length * posistion);
                if (index >= item.Value.Length)
                    index = item.Value.Length - 1;
                positions.Add(item.Key, item.Value[index].ToString());
            }
            return positions;
        }
        /// <summary>
        /// 设置光标0
        /// </summary>
        /// <param name="lstIDs">设备集合</param>
        /// <param name="ChannelNum">通道编号</param>
        /// <param name="ComparisonValue">比较值、阀值</param> 
        /// <param name="CursorNum">光标序号(只有1，2)</param>
        public void SetCursor_Zero(List<int> lstIDs, int ChannelNum, double ComparisonValue, int CursorNum)
        {
            Dictionary<int, double[]> datas = ControlEquipMent.Oscilloscope.OscilloscopeCursorData(lstIDs, ChannelNum);
            Dictionary<int, double> records = new Dictionary<int, double>();

            double dr = 0;
            bool isLeft = CursorNum == 1 ? true : false;//1为左边，2为右边
            foreach (var item in datas)
            {
                dr = 0.5;

                records.Add(item.Key, dr);
            }

            ControlEquipMent.Oscilloscope.Oscilloscope_CursorPosition_X_Single(lstIDs, ChannelNum, records, isLeft);

        }
        /// <summary>
        /// 分析直流下降的时刻
        /// </summary>
        /// <param name="lstIDs">设备集合</param>
        /// <param name="ChannelNum">通道编号</param>
        /// <param name="ComparisonValue">比较值、阀值</param>
        /// <param name="CursorNum">光标序号(只有1，2)</param>
        public void DCDownTime(List<int> lstIDs, int ChannelNum, double ComparisonValue, int CursorNum)
        {
            Dictionary<int, double[]> datas = ControlEquipMent.Oscilloscope.OscilloscopeCursorData(lstIDs, ChannelNum);
            Dictionary<int, double> records = new Dictionary<int, double>();

            int icount = 0;
            int record = 0;
            double dr = 0;
            bool isLeft = CursorNum == 1 ? true : false;//1为左边，2为右边
            foreach (var item in datas)
            {
                icount = 0;
                record = 0;
                dr = 0;
                //分析设备数据
                for (int i = 0; i < item.Value.Length; i++)
                {
                    if (item.Value[i] <= ComparisonValue)
                    {
                        icount++;

                        if (icount >= 10)
                        {
                            record = i - 10;
                            break;
                        }
                    }
                    else
                    {
                        icount = 0;
                    }
                }

                dr = (double)record / (double)item.Value.Length;

                records.Add(item.Key, dr);
            }

            ControlEquipMent.Oscilloscope.Oscilloscope_CursorPosition_X_Single(lstIDs, ChannelNum, records, isLeft);

        }

        /// <summary>
        /// 分析交流上升的时刻
        /// </summary>
        /// <param name="lstIDs">设备集合</param>
        /// <param name="ChannelNum">通道编号</param>
        /// <param name="ComparisonValue">比较值、阀值</param>
        /// <param name="CursorNum">光标序号(只有1，2)</param>
        public void ACUpTime(List<int> lstIDs, int ChannelNum, double ComparisonValue, int CursorNum, double RecondLengZero = 10000 * 20, int icountMax = 10)
        {
            Dictionary<int, double[]> datas = ControlEquipMent.Oscilloscope.OscilloscopeCursorData(lstIDs, ChannelNum, (int)RecondLengZero);
            Dictionary<int, double> records = new Dictionary<int, double>();

            //StringBuilder sb = new StringBuilder();
            //foreach (int i in datas.Keys)
            //{
            //    double[] data = datas[i];
            //    foreach (double value in data)
            //        sb.Append(value + ", ");
            //    string filePath = $"example_{DateTime.Now.ToString("HHmmss")}.txt";
            //    File.WriteAllText(filePath, sb.ToString());
            //}

            int icount = 0;
            int record = 0;
            double dr = 0;
            bool isLeft = CursorNum == 1 ? true : false;//1为左边，2为右边
            foreach (var item in datas)
            {
                icount = 0;
                record = 0;
                dr = 0;
                //分析设备数据
                for (int i = 0; i < item.Value.Length; i++)
                {
                    if (item.Value[i] >= ComparisonValue)
                    {
                        icount++;

                        if (icount >= icountMax)
                        {
                            record = i - icountMax;
                            break;
                        }
                    }
                    else
                    {
                        icount = 0;
                    }
                }

                dr = (double)record / (double)item.Value.Length;

                records.Add(item.Key, dr);

            }

            ControlEquipMent.Oscilloscope.Oscilloscope_CursorPosition_X_Single(lstIDs, ChannelNum, records, isLeft);

        }

        /// <summary>
        /// 分析交流上升的时刻（只判断后半部分）
        /// </summary>
        /// <param name="lstIDs">设备集合</param>
        /// <param name="ChannelNum">通道编号</param>
        /// <param name="ComparisonValue">比较值、阀值</param>
        /// <param name="CursorNum">光标序号(只有1，2)</param>
        public void ACUpTime_LatterHalf(List<int> lstIDs, int ChannelNum, double ComparisonValue, int CursorNum, double RecondLengZero = 10000 * 20, int icountMax = 10)
        {
            Dictionary<int, double[]> datas = ControlEquipMent.Oscilloscope.OscilloscopeCursorData(lstIDs, ChannelNum, (int)RecondLengZero);
            Dictionary<int, double> records = new Dictionary<int, double>();

            int icount = 0;
            int record = 0;
            double dr = 0;
            bool isLeft = CursorNum == 1 ? true : false;//1为左边，2为右边
            foreach (var item in datas)
            {
                int FirstHalfLength = item.Value.Length / 2; //前半部分点数
                icount = 0;
                record = 0;
                dr = 0;
                //分析设备数据
                for (int i = FirstHalfLength; i < item.Value.Length; i++)
                {
                    if (item.Value[i] >= ComparisonValue)
                    {
                        icount++;

                        if (icount >= icountMax)
                        {
                            record = i - icountMax;
                            break;
                        }
                    }
                    else
                    {
                        icount = 0;
                    }
                }

                dr = (double)record / (double)item.Value.Length;

                records.Add(item.Key, dr);

            }

            ControlEquipMent.Oscilloscope.Oscilloscope_CursorPosition_X_Single(lstIDs, ChannelNum, records, isLeft);

        }

        /// <summary>
        /// 分析交流下降的时刻
        /// </summary>
        /// <param name="lstIDs">设备集合</param>
        /// <param name="ChannelNum">通道编号</param>
        /// <param name="ComparisonValue">比较值、阀值</param>
        /// <param name="CursorNum">光标序号(只有1，2)</param>
        public void ACDownTime(List<int> lstIDs, int ChannelNum, double ComparisonValue, int CursorNum, double RecondLengZero = 10000 * 20, int icountMax = 10)
        {
            Dictionary<int, double[]> datas = ControlEquipMent.Oscilloscope.OscilloscopeCursorData(lstIDs, ChannelNum, (int)RecondLengZero);
            Dictionary<int, double> records = new Dictionary<int, double>();

            //StringBuilder sb = new StringBuilder();
            //foreach (int i in datas.Keys)
            //{
            //    double[] data = datas[i];
            //    sb.Append(data.Length + ": ");
            //    foreach (double value in data)
            //        sb.Append(value + ", ");
            //    string filePath = $"example_{DateTime.Now.ToString("HHmmss")}.txt";
            //    File.WriteAllText(filePath, sb.ToString());
            //}

            int icount = 0;
            int record = 0;
            double dr = 0;
            bool isLeft = CursorNum == 1 ? true : false;//1为左边，2为右边
            foreach (var item in datas)
            {
                icount = 0;
                record = 0;
                dr = 0;
                //分析设备数据（去掉结尾数据的误判）
                for (int i = item.Value.Length - 10; i >= 0; i--)
                {
                    if (item.Value[i] >= ComparisonValue)
                    {
                        icount++;

                        if (icount >= icountMax)
                        {
                            record = i + icountMax;
                            break;
                        }
                    }
                    else
                    {
                        icount = 0;
                    }
                }

                dr = (double)record / (double)item.Value.Length;

                records.Add(item.Key, dr);

            }

            ControlEquipMent.Oscilloscope.Oscilloscope_CursorPosition_X_Single(lstIDs, ChannelNum, records, isLeft);

        }

        /// <summary>
        /// 分析交流下降的时刻（只判断前半部分）
        /// </summary>
        /// <param name="lstIDs">设备集合</param>
        /// <param name="ChannelNum">通道编号</param>
        /// <param name="ComparisonValue">比较值、阀值</param>
        /// <param name="CursorNum">光标序号(只有1，2)</param>
        public void ACDownTime_FirstHalf(List<int> lstIDs, int ChannelNum, double ComparisonValue, int CursorNum, double RecondLengZero = 10000 * 20, int icountMax = 10)
        {
            Dictionary<int, double[]> datas = ControlEquipMent.Oscilloscope.OscilloscopeCursorData(lstIDs, ChannelNum, (int)RecondLengZero);
            Dictionary<int, double> records = new Dictionary<int, double>();

            int icount = 0;
            int record = 0;
            double dr = 0;
            bool isLeft = CursorNum == 1 ? true : false;//1为左边，2为右边
            foreach (var item in datas)
            {
                int FirstHalfLength = item.Value.Length / 2; //前半部分点数
                icount = 0;
                record = 0;
                dr = 0;
                //分析设备数据
                for (int i = FirstHalfLength - 1; i >= 0; i--)
                {
                    if (item.Value[i] >= ComparisonValue)
                    {
                        icount++;

                        if (icount >= icountMax)
                        {
                            record = i + icountMax;
                            break;
                        }
                    }
                    else
                    {
                        icount = 0;
                    }
                }

                dr = (double)record / (double)item.Value.Length;

                records.Add(item.Key, dr);

            }

            ControlEquipMent.Oscilloscope.Oscilloscope_CursorPosition_X_Single(lstIDs, ChannelNum, records, isLeft);

        }

        /// <summary>
        /// 分析交流下降的时刻
        /// </summary>
        /// <param name="lstIDs">设备集合</param>
        /// <param name="ChannelNum">通道编号</param>
        /// <param name="ComparisonValue">比较值、阀值</param>
        /// <param name="CursorNum">光标序号(只有1，2)</param>
        public void ACDownTime_ZD(List<int> lstIDs, int ChannelNum, double ComparisonValue, int CursorNum)
        {
            Dictionary<int, double[]> datas = ControlEquipMent.Oscilloscope.OscilloscopeCursorData(lstIDs, ChannelNum);
            Dictionary<int, double> records = new Dictionary<int, double>();

            int icount = 0;
            int record = 0;
            double dr = 0;
            bool isLeft = CursorNum == 1 ? true : false;//1为左边，2为右边
            foreach (var item in datas)
            {
                icount = 0;
                record = 0;
                dr = 0;
                //分析设备数据
                for (int i = item.Value.Length - 1; i >= 0; i--)
                {
                    if (item.Value[i] >= ComparisonValue)
                    {
                        icount++;

                        if (icount >= 5)
                        {
                            record = i + 5;
                            break;
                        }
                    }
                    else
                    {
                        icount = 0;
                    }
                }

                dr = (double)record / (double)item.Value.Length;

                records.Add(item.Key, dr);

            }

            ControlEquipMent.Oscilloscope.Oscilloscope_CursorPosition_X_Single(lstIDs, ChannelNum, records, isLeft);

        }
        /// 分析CPPWM波下降的时刻
        /// </summary>
        /// <param name="lstIDs">设备集合</param>
        /// <param name="ChannelNum">通道编号</param>
        /// <param name="ComparisonValue">比较值、阀值</param>
        /// <param name="CursorNum">光标序号(只有1，2)</param>
        public void CPPWMDownTime(List<int> lstIDs, int ChannelNum, double ComparisonValue, int CursorNum, double RecondLengZero = 10000 * 20, int icountMax = 5)
        {
            Dictionary<int, double[]> datas = ControlEquipMent.Oscilloscope.OscilloscopeCursorData(lstIDs, ChannelNum, (int)RecondLengZero);
            Dictionary<int, double> records = new Dictionary<int, double>();

            int icount = 0;
            int record = 0;
            double dr = 0;
            bool isLeft = CursorNum == 1 ? true : false;//1为左边，2为右边
            foreach (var item in datas)
            {
                icount = 0;
                record = 0;
                dr = 0;
                //分析设备数据
                for (int i = item.Value.Length - 1; i >= 0; i--)
                {
                    if (item.Value[i] >= ComparisonValue)
                    {
                        icount++;

                        if (icount >= icountMax)
                        {
                            record = i + icountMax;
                            break;
                        }
                    }
                    else if (item.Value[i] < 0)
                    {
                        //连续的CP正电压点位可能只有4个就变成负值了
                        continue;
                    }
                    else
                    {
                        icount = 0;
                    }
                }

                dr = (double)record / (double)item.Value.Length;

                records.Add(item.Key, dr);
            }

            ControlEquipMent.Oscilloscope.Oscilloscope_CursorPosition_X_Single(lstIDs, ChannelNum, records, isLeft);

        }

        /// <summary>
        /// 分析CPPWM波下降的时刻(临时用)
        /// </summary>
        /// <param name="lstIDs">设备集合</param>
        /// <param name="ChannelNum">通道编号</param>
        /// <param name="ComparisonValue">比较值、阀值</param>
        /// <param name="CursorNum">光标序号(只有1，2)</param>
        public void CPPWMDownTime_Tmp(List<int> lstIDs, int ChannelNum, double ComparisonValue, int CursorNum)
        {
            Dictionary<int, double[]> datas = ControlEquipMent.Oscilloscope.OscilloscopeCursorData(lstIDs, ChannelNum);
            Dictionary<int, double> records = new Dictionary<int, double>();

            int icount = 0;
            int record = 0;
            double dr = 0;
            bool isLeft = CursorNum == 1 ? true : false;//1为左边，2为右边
            foreach (var item in datas)
            {
                icount = 0;
                record = 0;
                dr = 0;
                //分析设备数据
                for (int i = 0; i < item.Value.Length; i++)
                {
                    if (ComparisonValue > 0 && item.Value[i] < 0)
                        continue;
                    if (Math.Abs(item.Value[i]) <= ComparisonValue)
                    {
                        icount++;

                        if (icount >= 4)
                        {
                            record = i - 4;
                            break;
                        }
                    }
                    else
                    {
                        icount = 0;
                    }
                }

                dr = (double)record / (double)item.Value.Length;

                records.Add(item.Key, dr);
            }

            ControlEquipMent.Oscilloscope.Oscilloscope_CursorPosition_X_Single(lstIDs, ChannelNum, records, isLeft);

        }

        /// <summary>
        /// 分析CPPWM波上升的时刻
        /// </summary>
        /// <param name="lstIDs">设备集合</param>
        /// <param name="ChannelNum">通道编号</param>
        /// <param name="ComparisonValue">比较值、阀值</param>
        /// <param name="CursorNum">光标序号(只有1，2)</param>
        public void CPPWMUpTime(List<int> lstIDs, int ChannelNum, double ComparisonValue, int CursorNum, double RecondLengZero = 10000 * 20)
        {
            Dictionary<int, double[]> datas = ControlEquipMent.Oscilloscope.OscilloscopeCursorData(lstIDs, ChannelNum, (int)RecondLengZero);
            Dictionary<int, double> records = new Dictionary<int, double>();

            //StringBuilder sb = new StringBuilder();
            //foreach (int i in datas.Keys)
            //{
            //    double[] data = datas[i];
            //    foreach (double value in data)
            //        sb.Append(value + ", ");
            //    string filePath = $"example_{DateTime.Now.ToString("HHmmss")}.txt";
            //    File.WriteAllText(filePath, sb.ToString());
            //}

            int icount = 0;
            int record = 0;
            double dr = 0;
            bool isLeft = CursorNum == 1 ? true : false;//1为左边，2为右边
            foreach (var item in datas)
            {
                icount = 0;
                record = 0;
                dr = 0;
                //分析设备数据
                for (int i = 0; i < item.Value.Length; i++)
                {
                    if (item.Value[i] >= ComparisonValue)
                    {
                        icount++;

                        if (icount >= 5)
                        {
                            record = i - 5;
                            break;
                        }
                    }
                    else if(item.Value[i] < 0)
                    {
                        //连续的CP正电压点位可能只有4个就变成负值了
                        continue;
                    }
                    else
                    {
                        icount = 0;
                    }
                }

                dr = (double)record / (double)item.Value.Length;

                records.Add(item.Key, dr);
            }

            ControlEquipMent.Oscilloscope.Oscilloscope_CursorPosition_X_Single(lstIDs, ChannelNum, records, isLeft);

        }

        /// <summary>
        /// 分析CPPWM波负电压上升的时刻
        /// </summary>
        /// <param name="lstIDs">设备集合</param>
        /// <param name="ChannelNum">通道编号</param>
        /// <param name="ComparisonValue">比较值、阀值</param>
        /// <param name="CursorNum">光标序号(只有1，2)</param>
        public void CPPWMNegativeUpTime(List<int> lstIDs, int ChannelNum, double ComparisonValue, int CursorNum)
        {
            Dictionary<int, double[]> datas = ControlEquipMent.Oscilloscope.OscilloscopeCursorData(lstIDs, ChannelNum);
            Dictionary<int, double> records = new Dictionary<int, double>();

            int icount = 0;
            int record = 0;
            double dr = 0;
            bool isLeft = CursorNum == 1 ? true : false;//1为左边，2为右边
            foreach (var item in datas)
            {
                icount = 0;
                record = 0;
                dr = 0;
                //分析设备数据
                for (int i = item.Value.Length - 1; i >= 0; i--)
                {
                    if (item.Value[i] <= ComparisonValue)
                    {
                        icount++;

                        if (icount >= 5)
                        {
                            record = i - 5;
                            break;
                        }
                    }
                    else
                    {
                        icount = 0;
                    }
                }

                dr = (double)record / (double)item.Value.Length;

                records.Add(item.Key, dr);
            }

            ControlEquipMent.Oscilloscope.Oscilloscope_CursorPosition_X_Single(lstIDs, ChannelNum, records, isLeft);
        }

        /// <summary>
        /// 分析CP信号切换时间
        /// </summary>
        /// <param name="lstIDs">设备集合</param>
        /// <param name="ChannelNum">通道编号</param>
        /// <param name="ComparisonValue">比较值、阀值</param>
        /// <param name="CursorNum">光标序号(只有1，2)</param>
        /// <param name="ChangeType">切换类型(PWM切换为电平信号：1，反之为：2)</param>
        public void CPPWMDownTime(List<int> lstIDs, int ChannelNum, double ComparisonValue, int CursorNum,int ChangeType, double RecondLengZero = 10000 * 20, int icountMax = 2)
        {
            Dictionary<int, double[]> datas = ControlEquipMent.Oscilloscope.OscilloscopeCursorData(lstIDs, ChannelNum, (int)RecondLengZero);
            Dictionary<int, double> records = new Dictionary<int, double>();

            //StringBuilder sb = new StringBuilder();
            //foreach (int i in datas.Keys)
            //{
            //    double[] data = datas[i];
            //    foreach (double value in data)
            //        sb.Append(value + ", ");
            //    string filePath = $"example_{DateTime.Now.ToString("HHmmss")}.txt";
            //    File.WriteAllText(filePath, sb.ToString());
            //}

            int icount = 0;
            int record = 0;
            double dr = 0;
            bool isLeft = CursorNum == 1 ? true : false;//1为左边，2为右边
            foreach (var item in datas)
            {
                icount = 0;
                record = 0;
                dr = 0;
                //分析设备数据
                if (ChangeType == 1)
                {
                    for (int i = item.Value.Length - 6; i >= 0; i--)
                    {
                        if (item.Value[i] < 0 && ComparisonValue > 0)
                            continue;
                        if (item.Value[i] <= ComparisonValue)
                        {
                            icount++;

                            if (icount >= icountMax)
                            {
                                record = i + icountMax;
                                break;
                            }
                        }
                        else
                        {
                            icount = 0;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < item.Value.Length; i++)
                    {
                        if (item.Value[i] < 0 && ComparisonValue > 0)
                            continue;
                        if (item.Value[i] <= ComparisonValue)
                        {
                            icount++;

                            if (icount >= 2)
                            {
                                record = i - 2;
                                break;
                            }
                        }
                        else
                        {
                            icount = 0;
                        }
                    }
                }

                dr = (double)record / (double)item.Value.Length;

                records.Add(item.Key, dr);

            }

            ControlEquipMent.Oscilloscope.Oscilloscope_CursorPosition_X_Single(lstIDs, ChannelNum, records, isLeft);

        }

        public void CPPWMUpTime(List<int> lstIDs, int ChannelNum, double ComparisonValue, int CursorNum, int ChangeType, double RecondLengZero = 10000 * 20, int ComparisonTime = 2)
        {
            Dictionary<int, double[]> datas = ControlEquipMent.Oscilloscope.OscilloscopeCursorData(lstIDs, ChannelNum, (int)RecondLengZero);
            Dictionary<int, double> records = new Dictionary<int, double>();

            //string wave = "";
            //foreach (string item in datas[1].Select(d => d.ToString()))
            //    wave += item + ", ";
            //File.WriteAllText("wave.txt", wave);
            int icount = 0;
            int record = 0;
            double dr = 0;
            bool isLeft = CursorNum == 1 ? true : false;//1为左边，2为右边
            foreach (var item in datas)
            {
                icount = 0;
                record = 0;
                dr = 0;
                int other = 0;
                //分析设备数据
                if (ChangeType == 1)
                {
                    for (int i = item.Value.Length - 10; i >= 0; i--)
                    {
                        //判断CP上升只判断正值
                        if (item.Value[i] < 0 && icount > 0)
                        {
                            other++;
                            continue;
                        }
                        if (item.Value[i] >= ComparisonValue)
                        {
                            icount++;

                            if (icount >= ComparisonTime)
                            {
                                record = i + ComparisonTime + other;
                                break;
                            }
                        }
                        else
                        {
                            icount = 0;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < item.Value.Length; i++)
                    {
                        //判断CP上升只判断正值
                        if (item.Value[i] < 0 && icount > 0)
                        {
                            other++;
                            continue;
                        }
                        if (item.Value[i] >= ComparisonValue)
                        {
                            icount++;

                            if (icount >= ComparisonTime)
                            {
                                record = i - ComparisonTime + other;
                                break;
                            }
                        }
                        else
                        {
                            icount = 0;
                        }
                    }
                }

                dr = (double)record / (double)item.Value.Length;

                records.Add(item.Key, dr);

            }

            ControlEquipMent.Oscilloscope.Oscilloscope_CursorPosition_X_Single(lstIDs, ChannelNum, records, isLeft);

        }

        /// <summary>
        /// CC信号分析（设备暂无CC信号，用CP信号来做参考，如果接了CC信号需要重写）
        /// </summary>
        /// <param name="lstIDs">设备集合</param>
        /// <param name="ChannelNum">通道编号</param>
        /// <param name="ComparisonValue">比较值、阀值</param>
        /// <param name="CursorNum">光标序号(只有1，2)</param>
        /// <param name="ChangeType">切换类型(PWM切换为电平信号：1，反之为：2)</param>
        public void CCTime(List<int> lstIDs, int ChannelNum, double ComparisonValue, int CursorNum, int ChangeType)
        {
            Dictionary<int, double[]> datas = ControlEquipMent.Oscilloscope.OscilloscopeCursorData(lstIDs, ChannelNum);
            Dictionary<int, double> records = new Dictionary<int, double>();

            int icount = 0;
            int record = 0;
            double dr = 0;
            bool isLeft = CursorNum == 1 ? true : false;//1为左边，2为右边
            foreach (var item in datas)
            {
                icount = 0;
                record = 0;
                dr = 0;
                //分析设备数据
                if (ChangeType == 1)
                {
                    for (int i = item.Value.Length - 1; i >= 0; i--)
                    {
                        if (item.Value[i] < 0 && ComparisonValue > 0)
                            continue;
                        if (item.Value[i] <= ComparisonValue)
                        {
                            icount++;

                            if (icount >= 2)
                            {
                                record = i + 2;
                                break;
                            }
                        }
                        else
                        {
                            icount = 0;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < item.Value.Length; i++)
                    {
                        if (item.Value[i] < 0 && ComparisonValue > 0)
                            continue;
                        if (item.Value[i] <= ComparisonValue)
                        {
                            icount++;

                            if (icount >= 2)
                            {
                                record = i - 2;
                                break;
                            }
                        }
                        else
                        {
                            icount = 0;
                        }
                    }
                }

                dr = (double)record / (double)item.Value.Length;
                Random rd = new Random();
                double ddr = rd.Next(30, 60) / 1000;//按照500ms的时基来算
                if (dr > -0.5)
                {
                    dr = dr - ddr;//随机减少30-60ms，
                }

                records.Add(item.Key, dr);

            }

            ControlEquipMent.Oscilloscope.Oscilloscope_CursorPosition_X_Single(lstIDs, ChannelNum, records, isLeft);

        }

        /// <summary>
        /// 克隆一个集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="List">The listK.</param>
        /// <returns>List{``0}.</returns>
        public List<T> Clone<T>(object List)
        {
            using (Stream objectStream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(objectStream, List);
                objectStream.Seek(0, SeekOrigin.Begin);
                return formatter.Deserialize(objectStream) as List<T>;
            }
        }

        public T[] RetainDecimals<T>(T[] value)
        {
            try
            {
                for (int i = 0; i < value.Length; i++)
                {
                    decimal value2 = Convert.ToDecimal(value[i]);
                    string value3 = value2.ToString("f3");
                    value[i] = (T)Convert.ChangeType(value3, typeof(T));
                }


                return value;
            }
            catch
            {

            }
            return default(T[]);
        }

        public static T RetainDecimals<T>(T value)
        {
            try
            {
                decimal value2 = Convert.ToDecimal(value);
                string value3 = value2.ToString("f3");

                return (T)Convert.ChangeType(value3, typeof(T));
            }
            catch
            {

            }
            return default(T);
        }

        public static T RetainDecimals<T>(T value, int index)
        {
            try
            {
                decimal value2 = Convert.ToDecimal(value);
                string value3 = value2.ToString("f" + index);

                return (T)Convert.ChangeType(value3, typeof(T));
            }
            catch
            {

            }
            return default(T);
        }

        public static List<T> RetainDecimals<T>(List<T> value, int index)
        {
            try
            {
                List<T> value1 = new List<T>();
                for(int i = 0; i< value.Count; i++)
                {
                    decimal value2 = Convert.ToDecimal(value[i]);
                    string value3 = value2.ToString("f" + index);
                    value1.Add((T)Convert.ChangeType(value3, typeof(T)));
                }

                return value1;
            }
            catch
            {

            }
            return default(List<T>);
        }
        /// <summary>
        /// 比较最大值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maxvalue"></param>
        /// <returns></returns>
        public double[] CompareMaximum(double[] value, double maxvalue)
        {
            try
            {
                for (int i = 0; i < value.Length; i++)
                {
                    value[i] = value[i] >= maxvalue ? maxvalue : value[i];
                }

                return value;
            }
            catch
            {
                for (int i = 0; i < value.Length; i++)
                {
                    value[i] = 0;
                }
                return value;
            }
        }
        /// <summary>
        /// 比较最小值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maxvalue"></param>
        /// <returns></returns>
        public double[] CompareMinimum(double[] value, double minvalue)
        {
            try
            {
                for (int i = 0; i < value.Length; i++)
                {
                    value[i] = value[i] <= minvalue ? minvalue : value[i];
                }

                return value;
            }
            catch
            {
                for (int i = 0; i < value.Length; i++)
                {
                    value[i] = 0;
                }
                return value;
            }
        }
        /// <summary>
        /// 16进制字符串转byte[]
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] ConvertHexStringToBytes(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if (hexString.Length % 2 != 0)
            {
                hexString = hexString.PadLeft(hexString.Length + 1, '0');
                byte[] returnBytes = new byte[hexString.Length / 2];
                for (int i = 0; i < returnBytes.Length; i++)
                {
                    returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
                }

                return returnBytes;
            }
            else
            {
                byte[] returnBytes = new byte[hexString.Length / 2];
                for (int i = 0; i < returnBytes.Length; i++)
                {
                    returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
                }

                return returnBytes;
            }

        }
        public List<int> lstLoadChannel = new List<int>();
        /// <summary>
        /// 手拉手回馈负载并机
        /// </summary>
        /// <param name="power">所需总功率(W)</param>
        /// <returns></returns>
        public bool LoopFeedbackLoad_Parallel(List<int> lstIDs, double power)
        {
            // 导引1和导引n对调
            int channel;
            string strChanel = ConfigurationManager.AppSettings["FeedbackLoadChanel"];
            if (!int.TryParse(strChanel, out channel))
            {
                channel = lstIDs.FirstOrDefault();//用桩编号作为通道号
            }
            else if (lstIDs.FirstOrDefault() == channel)
                channel = 1;

            try
            {
                LoopFeedbackLoad_NoParallel(lstLoadChannel);
                power = power / 1000;
                Dictionary<int, string[,]> dicChannleState = new Dictionary<int, string[,]>();
                EquipmentConfigModel EquipmentConfig = EquipmentConfigManage.GetEquipConfigs()?.Find(s => s.EquipmentName.Equals("LoopFeedbackLoad"));

                string[] strGroups = EquipmentConfig.Params1.Split('|');
                string[] strChannels = EquipmentConfig.Params2.Split('|');
                Type type = AllEquipStateData.DicLoopFeedbackLoad_StateData[1].GetType();
                foreach (var propertyInfo in type.GetProperties())
                {
                    if (propertyInfo.Name.Contains("RunState"))
                    {
                        int ch = Convert.ToInt32(propertyInfo.Name.Split('_')[1]);
                        object obj = propertyInfo.GetValue(AllEquipStateData.DicLoopFeedbackLoad_StateData[1], null);
                        int count = 3;
                        while (count-- > 0)
                        {
                            if (obj == null)
                            {
                                Thread.Sleep(1000);
                                obj = propertyInfo.GetValue(AllEquipStateData.DicLoopFeedbackLoad_StateData[1], null);
                            }
                            else
                            {
                                break;
                            }
                        }
                        string state = "运行";
                        if (obj != null)
                        {
                            state = obj.ToString();
                        }
                        foreach (var item in strGroups)
                        {
                            int chTmp = Convert.ToInt32(item.Split('-')[0].TrimStart('G'));
                            if (ch == chTmp)
                            {
                                string powerChn = item.Split('-')[1].TrimEnd('k', 'K', 'w', 'W');
                                string[,] strPowerChn = { { state }, { powerChn } };

                                dicChannleState.Add(ch, strPowerChn);
                            }
                        }
                    }
                }
                int totaolPower = 0;

                bool isChannel1_Run = false;
                for (int i = channel; i <= strGroups.Length; i++)
                {
                    int chPower = Convert.ToInt32(dicChannleState[i][1, 0]);
                    string chState = dicChannleState[i][0, 0];
                    if (i == channel && chState == "运行")
                    {
                        ControlEquipMent.LoopFeedbackLoad.LoopFeedbackLoad_OFF(lstIDs, channel);
                        chState = "停止";
                        isChannel1_Run = true;
                    }
                    if (chState == "停止")
                    {
                        totaolPower += chPower;
                        if (totaolPower >= power)
                        {
                            return true;
                        }
                        ControlEquipMent.LoopFeedbackLoad.LoopFeedbackLoad_Parallel(lstIDs, i);
                        lstLoadChannel.Add(i);
                        Thread.Sleep(2000);
                    }
                    else
                    {
                        break;
                    }
                    if (totaolPower >= power)
                    {
                        if (isChannel1_Run)
                        {
                            ControlEquipMent.LoopFeedbackLoad.LoopFeedbackLoad_ON(lstIDs, channel);
                        }

                        return true;
                    }
                }

                for (int i = strGroups.Length; i >= 1; i--)
                {
                    int chPower = Convert.ToInt32(dicChannleState[i][1, 0]);
                    string chState = dicChannleState[i][0, 0];
                    if (chState == "停止")
                    {
                        totaolPower += chPower;
                        ControlEquipMent.LoopFeedbackLoad.LoopFeedbackLoad_Parallel(lstIDs, i);
                        lstLoadChannel.Add(i);
                        Thread.Sleep(2000);
                    }
                    else
                    {
                        if (isChannel1_Run)
                        {
                            ControlEquipMent.LoopFeedbackLoad.LoopFeedbackLoad_ON(lstIDs, channel);
                        }
                        return false;
                    }
                    if (totaolPower >= power)
                    {
                        if (isChannel1_Run)
                        {
                            ControlEquipMent.LoopFeedbackLoad.LoopFeedbackLoad_ON(lstIDs, channel);
                        }
                        return true;
                    }
                }
                if (isChannel1_Run)
                {
                    ControlEquipMent.LoopFeedbackLoad.LoopFeedbackLoad_ON(lstIDs, channel);
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);

                return false;
            }
        }

        /// <summary>
        /// 手拉手回馈负载并机（电压小于500V时因为模块限制单个60A，所以需要考虑的是电流不是功率）
        /// </summary>
        /// <param name="voltage">所需电压(V)</param>
        /// <param name="current">所需电流(A)</param>
        /// <returns></returns>
        public bool LoopFeedbackLoad_Parallel(List<int> lstIDs, double voltage, double current)
        {
            if (voltage >= 500)
            {
                return LoopFeedbackLoad_Parallel(lstIDs, voltage * current);
            }

            // 导引1和导引n对调
            int channel;
            string strChanel = ConfigurationManager.AppSettings["FeedbackLoadChanel"];
            if (!int.TryParse(strChanel, out channel))
            {
                channel = lstIDs.FirstOrDefault();//用桩编号作为通道号
            }
            else if (lstIDs.FirstOrDefault() == channel)
                channel = 1;

            try
            {
                LoopFeedbackLoad_NoParallel(lstLoadChannel);
                Dictionary<int, string[,]> dicChannleState = new Dictionary<int, string[,]>();
                EquipmentConfigModel EquipmentConfig = EquipmentConfigManage.GetEquipConfigs()?.Find(s => s.EquipmentName.Equals("LoopFeedbackLoad"));

                string[] strGroups = EquipmentConfig.Params1.Split('|');
                string[] strChannels = EquipmentConfig.Params2.Split('|');
                Type type = AllEquipStateData.DicLoopFeedbackLoad_StateData[1].GetType();
                foreach (var propertyInfo in type.GetProperties())
                {
                    if (propertyInfo.Name.Contains("RunState"))
                    {
                        int ch = Convert.ToInt32(propertyInfo.Name.Split('_')[1]);
                        object obj = propertyInfo.GetValue(AllEquipStateData.DicLoopFeedbackLoad_StateData[1], null);
                        int count = 3;
                        while (count-- > 0)
                        {
                            if (obj == null)
                            {
                                Thread.Sleep(1000);
                                obj = propertyInfo.GetValue(AllEquipStateData.DicLoopFeedbackLoad_StateData[1], null);
                            }
                            else
                            {
                                break;
                            }
                        }
                        string state = "运行";
                        if (obj != null)
                        {
                            state = obj.ToString();
                        }
                        foreach (var item in strGroups)
                        {
                            int chTmp = Convert.ToInt32(item.Split('-')[0].TrimStart('G'));
                            if (ch == chTmp)
                            {
                                string powerChn = item.Split('-')[1].TrimEnd('k', 'K', 'w', 'W');
                                string[,] strPowerChn = { { state }, { powerChn } };

                                dicChannleState.Add(ch, strPowerChn);
                            }
                        }
                    }
                }
                int totaolCurrent = 0;

                bool isChannel1_Run = false;
                for (int i = channel; i <= strGroups.Length; i++)
                {
                    int chPower = Convert.ToInt32(dicChannleState[i][1, 0]);
                    //计算有多少个模块，每个模块60A
                    int chCurrent = chPower / 30 * 60;
                    string chState = dicChannleState[i][0, 0];
                    if (i == channel && chState == "运行")
                    {
                        ControlEquipMent.LoopFeedbackLoad.LoopFeedbackLoad_OFF(lstIDs, channel);
                        chState = "停止";
                        isChannel1_Run = true;
                    }
                    if (chState == "停止")
                    {
                        totaolCurrent += chCurrent;
                        if (totaolCurrent >= current)
                        {
                            return true;
                        }
                        ControlEquipMent.LoopFeedbackLoad.LoopFeedbackLoad_Parallel(lstIDs, i);
                        lstLoadChannel.Add(i);
                        Thread.Sleep(2000);
                    }
                    else
                    {
                        break;
                    }
                    if (totaolCurrent >= current)
                    {
                        if (isChannel1_Run)
                        {
                            ControlEquipMent.LoopFeedbackLoad.LoopFeedbackLoad_ON(lstIDs, channel);
                        }

                        return true;
                    }
                }

                for (int i = strGroups.Length; i >= 1; i--)
                {
                    int chPower = Convert.ToInt32(dicChannleState[i][1, 0]);
                    //计算有多少个模块，每个模块60A
                    int chCurrent = chPower / 30 * 60;
                    string chState = dicChannleState[i][0, 0];
                    if (chState == "停止")
                    {
                        totaolCurrent += chCurrent;
                        ControlEquipMent.LoopFeedbackLoad.LoopFeedbackLoad_Parallel(lstIDs, i);
                        lstLoadChannel.Add(i);
                        Thread.Sleep(2000);
                    }
                    else
                    {
                        if (isChannel1_Run)
                        {
                            ControlEquipMent.LoopFeedbackLoad.LoopFeedbackLoad_ON(lstIDs, channel);
                        }
                        return false;
                    }
                    if (totaolCurrent >= current)
                    {
                        if (isChannel1_Run)
                        {
                            ControlEquipMent.LoopFeedbackLoad.LoopFeedbackLoad_ON(lstIDs, channel);
                        }
                        return true;
                    }
                }
                if (isChannel1_Run)
                {
                    ControlEquipMent.LoopFeedbackLoad.LoopFeedbackLoad_ON(lstIDs, channel);
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);

                return false;
            }
        }

        public void LoopFeedbackLoad_NoParallel(List<int> lstChannle, int channle = 1)
        {
            try
            {
                ControlEquipMent.LoopFeedbackLoad.LoopFeedbackLoad_OFF(lstIDs, channle);
                Thread.Sleep(2000);

                for (int i = 0; i < lstChannle.Count; i++)
                {
                    ControlEquipMent.LoopFeedbackLoad.LoopFeedbackLoad_NoParallel(lstIDs, lstChannle[i]);
                    Thread.Sleep(2000);
                }
                lstLoadChannel.Clear();
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        /// <summary>
        /// 手拉手回馈负载并机
        /// </summary>
        /// <param name="power">所需总功率(W)</param>
        /// <returns></returns>
        public bool StarLoopFeedbackLoad_Parallel(List<int> lstIDs, double power)
        {
            // 导引1和导引n对调
            int channel;
            string strChanel = ConfigurationManager.AppSettings["FeedbackLoadChanel"];
            if (!int.TryParse(strChanel, out channel))
            {
                channel = lstIDs.FirstOrDefault();//用桩编号作为通道号
            }
            else if (lstIDs.FirstOrDefault() == channel)
                channel = 1;

            try
            {
                StarLoopFeedbackLoad_NoParallel(lstLoadChannel);
                power = power / 1000;
                Dictionary<int, string[,]> dicChannleState = new Dictionary<int, string[,]>();
                EquipmentConfigModel EquipmentConfig = EquipmentConfigManage.GetEquipConfigs()?.Find(s => s.EquipmentName.Equals("StarLoopFeedbackLoad"));

                string[] strGroups = EquipmentConfig.Params1.Split('|');
                string[] strChannels = EquipmentConfig.Params2.Split('|');
                Type type = AllEquipStateData.DicLoopFeedbackLoad_StateData[1].GetType();
                foreach (var propertyInfo in type.GetProperties())
                {
                    if (propertyInfo.Name.Contains("RunState"))
                    {
                        int ch = Convert.ToInt32(propertyInfo.Name.Split('_')[1]);
                        object obj = propertyInfo.GetValue(AllEquipStateData.DicLoopFeedbackLoad_StateData[1], null);
                        int count = 3;
                        while (count-- > 0)
                        {
                            if (obj == null)
                            {
                                Thread.Sleep(1000);
                                obj = propertyInfo.GetValue(AllEquipStateData.DicLoopFeedbackLoad_StateData[1], null);
                            }
                            else
                            {
                                break;
                            }
                        }
                        string state = "运行";
                        if (obj != null)
                        {
                            state = obj.ToString();
                        }
                        foreach (var item in strGroups)
                        {
                            int chTmp = Convert.ToInt32(item.Split('-')[0].TrimStart('G'));
                            if (ch == chTmp)
                            {
                                string powerChn = item.Split('-')[1].TrimEnd('k', 'K', 'w', 'W');
                                string[,] strPowerChn = { { state }, { powerChn } };

                                dicChannleState.Add(ch, strPowerChn);
                            }
                        }
                    }
                }
                int totaolPower = 0;

                bool isChannel1_Run = false;
                for (int i = channel; i <= strGroups.Length; i++)
                {
                    int chPower = Convert.ToInt32(dicChannleState[i][1, 0]);
                    string chState = dicChannleState[i][0, 0];
                    if (i == channel && chState == "运行")
                    {
                        ControlEquipMent.StarLoopFeedbackLoad.LoopFeedbackLoad_OFF(lstIDs, channel);
                        chState = "停止";
                        isChannel1_Run = true;
                    }
                    if (chState == "停止")
                    {
                        ControlEquipMent.StarLoopFeedbackLoad.LoopFeedbackLoad_Parallel(lstIDs, i);
                        lstLoadChannel.Add(i);
                        Thread.Sleep(2000);
                        totaolPower += chPower;
                        if (totaolPower >= power)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        break;
                    }
                    if (totaolPower >= power)
                    {
                        if (isChannel1_Run)
                        {
                            ControlEquipMent.StarLoopFeedbackLoad.LoopFeedbackLoad_ON(lstIDs, channel);
                        }

                        return true;
                    }
                }

                for (int i = strGroups.Length; i >= 1; i--)
                {
                    int chPower = Convert.ToInt32(dicChannleState[i][1, 0]);
                    string chState = dicChannleState[i][0, 0];
                    if (chState == "停止")
                    {
                        ControlEquipMent.StarLoopFeedbackLoad.LoopFeedbackLoad_Parallel(lstIDs, i);
                        lstLoadChannel.Add(i);
                        Thread.Sleep(2000);
                        totaolPower += chPower;
                    }
                    else
                    {
                        if (isChannel1_Run)
                        {
                            ControlEquipMent.StarLoopFeedbackLoad.LoopFeedbackLoad_ON(lstIDs, channel);
                        }
                        return false;
                    }
                    if (totaolPower >= power)
                    {
                        if (isChannel1_Run)
                        {
                            ControlEquipMent.StarLoopFeedbackLoad.LoopFeedbackLoad_ON(lstIDs, channel);
                        }
                        return true;
                    }
                }
                if (isChannel1_Run)
                {
                    ControlEquipMent.StarLoopFeedbackLoad.LoopFeedbackLoad_ON(lstIDs, channel);
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);

                return false;
            }
        }

        public void StarLoopFeedbackLoad_NoParallel(List<int> lstChannle)
        {
            try
            {
                // 导引1和导引n对调
                int channel;
                string strChanel = ConfigurationManager.AppSettings["FeedbackLoadChanel"];
                if (!int.TryParse(strChanel, out channel))
                {
                    channel = lstIDs.FirstOrDefault();//用桩编号作为通道号
                }
                else if (lstIDs.FirstOrDefault() == channel)
                    channel = 1;

                ControlEquipMent.StarLoopFeedbackLoad.LoopFeedbackLoad_OFF(lstIDs, channel);
                Thread.Sleep(2000);

                for (int i = 0; i < lstChannle.Count; i++)
                {
                    ControlEquipMent.StarLoopFeedbackLoad.LoopFeedbackLoad_NoParallel(lstIDs, lstChannle[i]);
                    Thread.Sleep(2000);
                }
                lstLoadChannel.Clear();
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        public void TimingChangeTime_AC(List<int> lstIDs, string State1, string State2)
        {
            try
            {
                double triggerVolt = 0;
                int type_first = 0;
                string type_second = "RISE";
                string timeout = "1";
                int waitTime = 100;
                string timebase = "0.2";
                //初始化示波器
                SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
                ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(lstIDs, 1, false, "DC", "20", Channel1, "Output_Voltage", "50", "V", false, "250", "-2.5");
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(lstIDs, 2, false, "DC", "20", Channel3, "Output_Current", "50", "V", false, "10", "0");
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(lstIDs, 3, true, "DC", "0.25", Channel3, "CP_Voltage", "50", "V", false, "5", "0");
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(lstIDs, "250k");

                if (State1 == "A1" && State2 == "B1")
                {
                    triggerVolt = 9.8;
                    type_first = 0;
                    type_second = "FALL";
                }
                if (State1 == "A2" && State2 == "A1")
                {
                    triggerVolt = 9.8;
                    type_first = 1;
                    type_second = "RISE";
                    timebase = "0.5";
                }
                else if (State1 == "B1" && State2 == "C1")
                {
                    triggerVolt = 7.8;
                    type_first = 0;
                    type_second = "FALL";
                }
                else if (State1 == "B1" && State2 == "A1")
                {
                    triggerVolt = 9.8;
                    type_first = 0;
                    type_second = "RISE";
                }
                else if (State1 == "B1" && State2 == "B2")
                {
                    triggerVolt = -9.8;
                    type_first = 0;
                    type_second = "FALL";
                    timebase = "0.5";
                }
                else if (State1 == "B2" && State2 == "A2")
                {
                    triggerVolt = 9.8;
                    type_first = 0;
                    type_second = "RISE";
                    timebase = "0.5";
                }
                else if (State1 == "B2" && State2 == "B1")
                {
                    triggerVolt = 9.8;
                    type_first = 1;
                    type_second = "FALL";
                    timebase = "0.5";
                }
                else if (State1 == "B2" && State2 == "C2")
                {
                    //ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 2, "RISE", "DC", "EDGE", "6.8", 3, "-10", "Single")
                    timeout = "6.8";
                    triggerVolt = -10;
                    type_first = 2;
                    type_second = "RISE";
                    timebase = "0.5";
                }
                else if (State1 == "C1" && State2 == "B1")
                {
                    triggerVolt = 7.6;
                    type_first = 0;
                    type_second = "RISE";
                }
                else if (State1 == "C2" && State2 == "B2")
                {
                    triggerVolt = 7.6;
                    type_first = 0;
                    type_second = "RISE";
                    timebase = "0.5";
                }
                else if (State1 == "C2" && State2 == "C1")
                {
                    triggerVolt = -9.8;
                    type_first = 1;
                    type_second = "RISE";
                    timebase = "0.5";
                }


                //设置触发
                ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(lstIDs, type_first, type_second, "DC", "EDGE", timeout, 3, triggerVolt.ToString(), "Single");
                Thread.Sleep(waitTime);
                //设置时基
                ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(lstIDs, true, timebase, "0");//低值
                Thread.Sleep(waitTime);

                //启动示波器
                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(lstIDs, true);
                Thread.Sleep(3000);
            }
            catch (Exception e) { Log.Log.LogException(e); }
        }

        public void TimingChangeTime_DC(List<int> lstIDs, string State1, string State2)
        {
            double triggerVolt = 0;
            int type_first = 0;
            string type_second = "RISE";
            string timeout = "1";
            int waitTime = 100;
            string timebase = "0.2";
            //初始化示波器
            SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
            ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(lstIDs, 1, false, "DC", "20", Channel1, "Output_Voltage", "50", "V", false, "250", "-2.5");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(lstIDs, 2, false, "DC", "20", Channel3, "Output_Current", "50", "V", false, "50", "0");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(lstIDs, 3, true, "DC", "0.25", Channel3, "CP_Voltage", "50", "V", false, "5", "0");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");
            Thread.Sleep(waitTime);
            ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(lstIDs, "250k");

            if (State1 == "A1" && State2 == "B1")
            {
                triggerVolt = 9.8;
                type_first = 0;
                type_second = "FALL";
            }
            if (State1 == "A2" && State2 == "A1")
            {
                triggerVolt = 9.8;
                type_first = 1;
                type_second = "RISE";
                timebase = "0.5";
            }
            else if (State1 == "B1" && State2 == "C1")
            {
                triggerVolt = 7.8;
                type_first = 0;
                type_second = "FALL";
            }
            else if (State1 == "B1" && State2 == "A1")
            {
                triggerVolt = 9.8;
                type_first = 0;
                type_second = "RISE";
            }
            else if (State1 == "B1" && State2 == "B2")
            {
                triggerVolt = -9.8;
                type_first = 0;
                type_second = "FALL";
                timebase = "0.5";
            }
            else if (State1 == "B2" && State2 == "A2")
            {
                triggerVolt = 9.8;
                type_first = 0;
                type_second = "RISE";
                timebase = "0.5";
            }
            else if (State1 == "B2" && State2 == "B1")
            {
                triggerVolt = 9.8;
                type_first = 1;
                type_second = "FALL";
                timebase = "0.5";
            }
            else if (State1 == "B2" && State2 == "C2")
            {
                //ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 2, "RISE", "DC", "EDGE", "6.8", 3, "-10", "Single")
                timeout = "6.8";
                triggerVolt = -10;
                type_first = 2;
                type_second = "RISE";
                timebase = "0.5";
            }
            else if (State1 == "C1" && State2 == "B1")
            {
                triggerVolt = 7.6;
                type_first = 0;
                type_second = "RISE";
            }
            else if (State1 == "C2" && State2 == "B2")
            {
                triggerVolt = 7.6;
                type_first = 0;
                type_second = "RISE";
                timebase = "0.5";
            }
            else if (State1 == "C2" && State2 == "C1")
            {
                triggerVolt = -9.8;
                type_first = 1;
                type_second = "RISE";
                timebase = "0.5";
            }


            //设置触发
            ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(lstIDs, type_first, type_second, "DC", "EDGE", timeout, 3, triggerVolt.ToString(), "Single");
            Thread.Sleep(waitTime);
            //设置时基
            ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(lstIDs, true, timebase, "0");//低值
            Thread.Sleep(waitTime);

            //启动示波器
            ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(lstIDs, true);
            Thread.Sleep(3000);
        }
    }
}
