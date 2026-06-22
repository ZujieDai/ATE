using SaiTer.ATE.EquipMent;
using SaiTer.ATE.IDAL.SQLiteIDAL;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    public partial class BusinessBase
    {
        #region 示波器
        /// <summary>
        /// 读示波器是否触发
        /// </summary>
        /// <param name="timeout"></param>
        public void ReadTriggerType(List<int> lstIDs, int timeout)
        {
            while (timeout-- >= 0)
            {
                SendNoticeToUIAndTxtFile("判断是否触发倒计时:" + timeout);
                bool isSDSOscilloscope = false;
                foreach (var item in ControlEquipMent.Oscilloscope.DitEquipMentBase)
                {
                    if (item.Value.GetType() == typeof(DLMOscilloscope) || item.Value.GetType() == typeof(emtTekOscilloscope))
                    {
                        isSDSOscilloscope = false;
                    }
                    else if (item.Value.GetType() == typeof(SDSOscilloscope))
                    {
                        isSDSOscilloscope = true;
                    }
                }
                if (!isSDSOscilloscope)
                {
                    Dictionary<int, bool> dicTemp = new Dictionary<int, bool>();
                    dicTemp = ControlEquipMent.Oscilloscope.ReadTriggerState(lstIDs);
                    if (dicTemp.FirstOrDefault().Value)
                    {
                        break;
                    }
                    if (timeout <= 0)
                    {
                        break;
                    }
                }
                else
                {
                    var dicTemp = new Dictionary<int, int>();
                    dicTemp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadTrigger(testWorkParam.lstIDs);
                    if (dicTemp[testWorkParam.lstIDs[0]] == 0)
                    {
                        break;
                    }
                    if (timeout <= 0)
                    {
                        break;
                    }
                }
                System.Threading.Thread.Sleep(1000);
            }
        }
        #endregion

        #region 录波仪设置函数

        public void SetChannel(bool[] channelopen, bool[] canchannelopen)
        {
            for (int i = 0; i < channelopen.Length; i++)
            {
                ControlEquipMent.Oscillograph?.Oscillograph_Channel_Open(testWorkParam.lstIDs, i + 1, channelopen[i]);
            }

            int count = canchannelopen.ToList().FindAll(c => c == true).Count;
            if (count > 0)
            {
                for (int i = 0; i < canchannelopen.Length; i++)
                {
                    ControlEquipMent.Oscillograph?.Oscillograph_SetCanChild_Open(15, i + 1, canchannelopen[i]);
                }
            }
            else
            {
                ControlEquipMent.Oscillograph?.Oscillograph_Channel_Open(testWorkParam.lstIDs, 15, false);
            }
            int count1 = canchannelopen.ToList().FindAll(c => c == true).Count;
            int count2 = channelopen.ToList().FindAll(c => c == true).Count;
            int count3 = count1 + count2;
            int count4 = 1;
            if (count3 < 8)
            {
                count4 = 1;
            }
            if (count3 >= 8 && count3 <= 15)
            {
                count4 = 8;
            }
            if (count3 > 15)
            {
                count4 = 16;
            }
            ControlEquipMent.Oscillograph?.Oscillograph_SetGroup3(1, count4, false);

        }
        /// <summary>
        /// 读录波仪触发状态
        /// </summary>
        /// <param name="timeout">循环等待时间</param>
        public bool ReadTriggerTypeOscillograph(int timeout)
        {
            while (timeout-- >= 0)
            {
                SendNoticeToUIAndTxtFile("判断是否触发倒计时:" + timeout);
                Dictionary<int, int> dicTemp = new Dictionary<int, int>();
                dicTemp = ControlEquipMent.Oscillograph?.Oscillograph_ReadTrigger();
                if (dicTemp.FirstOrDefault().Value== 0)
                {
                    return true;
                }
                if (timeout <= 0)
                {
                    break;
                }
                System.Threading.Thread.Sleep(1000);
            }
            return false;
        }







        /// <summary>
        /// 设置栅格多少个和是否开启精品栅格
        /// </summary>
        /// <param name="group"></param>
        /// <param name="format"></param>
        /// <param name="FGRid"></param>
        public void OscillographInstrument_SetGroup3(int group, int format, bool FGRid)
        {
            try
            {
                ControlEquipMent.Oscillograph?.Oscillograph_SetGroup3(group, format, FGRid);

            }
            catch (Exception ex)
            {
                SendException(ex);
            }

        }









        public void OscillographInstrument_CursorPosition_SetChannel(int channel, bool isCAN, int schannel)
        {

            try
            {
                ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_SetChannel(channel, isCAN, schannel);

            }
            catch (Exception ex)
            {
                SendException(ex);
            }




        }







        public void ClearMeasures()
        {



            try
            {
                for (int i = 1; i <= 8; i++)
                {
                    ControlEquipMent.Oscillograph?.Oscillograph_Measure_Initialize(i);
                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }




        }


        public double OscillographInstrumentReadMeasure(string MeasureType, int channel, bool isCAN, int Schannel)
        {




            try
            {
                Dictionary<int, string> value = ControlEquipMent.Oscillograph?.Oscillograph_ReadMeasure(MeasureType, channel, isCAN, Schannel);

                return Convert.ToDouble(value[LstChargerInfo[0].ChargerId]);
            }
            catch (Exception ex)
            {
                SendException(ex);
                return 0;
            }




        }






        /// <summary>
        /// 读取通道值
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="isCAN">是否为CAN</param>
        /// <param name="schaneel">子通道</param>
        /// <returns></returns>
        public string OscillographInstrumentReadValue(int channel, bool isCAN, int schaneel)
        {




            try
            {
                Dictionary<int, string> value = ControlEquipMent.Oscillograph?.GetChannelValue(channel, isCAN, schaneel);//测试取值用的

                return value[LstChargerInfo[0].ChargerId];

            }
            catch (Exception ex)
            {
                SendException(ex);
                return null;
            }




        }
















        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="isCAN"></param>
        /// <param name="schannel"></param>
        /// <param name="upvalue"></param>
        /// <param name="downvalue"></param>
        /// <param name="uptype">>0为上升，1为下降</param>
        /// <param name="isAC"></param>
        /// <param name="errorrate">误差</param>
        /// <returns></returns>
        public double GetTriggerTime(int channel, bool isCAN, int schannel, double upvalue, double downvalue, int uptype, bool isAC, double errorrate)
        {


            double time = 0;
            try
            {
                Dictionary<int, double[]> ReturnCursorData = ControlEquipMent.Oscillograph?.OscillographCursorData(channel, isCAN, schannel);
                double[] data = ReturnCursorData[LstChargerInfo[0].ChargerId];
                Dictionary<int, double[]> ReturnPoints = ControlEquipMent.Oscillograph?.Oscillograph_Points(data, upvalue, downvalue, uptype, isAC, errorrate);

                double[] value = ReturnPoints[LstChargerInfo[0].ChargerId];

                if (value[0] == -999 || value[1] == -999)
                {
                    ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X(channel, isCAN, schannel, -5, 5);
                    time = 9999;

                }
                else
                {
                    ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X(channel, isCAN, schannel, value[0], value[1]);
                    System.Threading.Thread.Sleep(2000);
                    Dictionary<int, double> ReturnCurcor = ControlEquipMent.Oscillograph?.Oscillograph_ReadCurcor();
                    time = ReturnCurcor[LstChargerInfo[0].ChargerId];
                }

            }
            catch (Exception ex)
            {
                SendException(ex);
            }



            return time;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="isCAN"></param>
        /// <param name="schannel"></param>
        /// <param name="upvalue"></param>
        /// <param name="downvalue"></param>
        /// <param name="uptype">>0为上升，1为下降</param>
        /// <param name="isAC"></param>
        /// <param name="errorrate">误差</param>
        /// <param name="YValue">出来的值</param>
        /// <returns></returns>
        public double GetTriggerTime(int channel, bool isCAN, int schannel, double upvalue, double downvalue, int uptype, bool isAC, double errorrate, out double YValue)
        {
            YValue = 0;



            double time = 0;
            try
            {

                Dictionary<int, double[]> ReturnCursorData = ControlEquipMent.Oscillograph?.OscillographCursorData(channel, isCAN, schannel);

                double[] data = ReturnCursorData[LstChargerInfo[0].ChargerId];
                Dictionary<int, double[]> ReturnOscillographPoints = ControlEquipMent.Oscillograph?.Oscillograph_Points(data, upvalue, downvalue, uptype, isAC, errorrate);
                double[] value = ReturnOscillographPoints[LstChargerInfo[0].ChargerId];
                if (data != null)
                {
                    if (data.Length > 10)
                    {
                        int index = data.Length / 10 * 3;
                        int index2 = data.Length / 10 * 4;
                        double y1value = 0;
                        for (int i = index - 1; i < index2 - 2; i++)
                        {
                            y1value += data[i];
                        }
                        YValue = y1value / data.Length * 10;
                        YValue = RetainDecimals<double>(YValue);
                    }
                }
                if (value[0] == -999 || value[1] == -999)
                {
                    ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X(channel, isCAN, schannel, -5, 5);
                    time = 9999;

                }
                else
                {
                    ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X(channel, isCAN, schannel, value[0], value[1]);
                    System.Threading.Thread.Sleep(2000);
                    Dictionary<int, double> ReadCurcor = ControlEquipMent.Oscillograph?.Oscillograph_ReadCurcor();
                    time = ReadCurcor[LstChargerInfo[0].ChargerId];
                }

            }
            catch (Exception ex)
            {
                SendException(ex);
            }



            return time;
        }

        /// <summary>
        /// 根据实际两个点来卡值
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="isCAN"></param>
        /// <param name="schannel"></param>
        /// <param name="upvalue"></param>
        /// <param name="downvalue"></param>
        /// <param name="uptype"></param>
        /// <param name="isAC"></param>
        /// <param name="errorrate"></param>
        /// <param name="YValue"></param>
        /// <returns></returns>
        public double GetTriggerTime_Position(int channel, bool isCAN, int schannel, double value1, double value2)
        {
            double time = 0;
            try
            {
                ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X(channel, isCAN, schannel, value1, value2);
                System.Threading.Thread.Sleep(2000);
                Dictionary<int, double> ReadCurcor = ControlEquipMent.Oscillograph?.Oscillograph_ReadCurcor();
                time = ReadCurcor[LstChargerInfo[0].ChargerId];

            }
            catch (Exception ex)
            {
                SendException(ex);
            }



            return time;
        }












        /// <summary>
        /// 取通道的某个位置值
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="isCAN"></param>
        /// <param name="schannel"></param>
        /// <param name="Percentage">百分比</param>
        public double GetData_Value(int channel, bool isCAN, int schannel, double Percentage)
        {



            double value = 0;
            try
            {

                Dictionary<int, double[]> CursorData = ControlEquipMent.Oscillograph?.OscillographCursorData(channel, isCAN, schannel);

                double[] data = CursorData[LstChargerInfo[0].ChargerId];
                if (data != null && data.Length > 1000)
                {
                    int Length = data.Length;
                    int RealLength = (int)(Length * Percentage);

                    value = data[RealLength - 10];


                }

            }
            catch (Exception ex)
            {
                SendException(ex);
            }



            return value;
        }






        /// <summary>
        /// 取触发的卡点时间
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="isCAN"></param>
        /// <param name="schannel"></param>
        /// <param name="value"></param>
        /// <param name="uptype">>0为上升，1为下降</param>
        /// <param name="isAC"></param>
        /// <param name="isLeft">是否为左边</param>
        /// <param name="errorrate">误差</param>
        /// <param name="isReverse">是否需要倒序</param>
        /// <returns></returns>
        public double GetTriggerTime_Single(int channel, bool isCAN, int schannel, double value, int uptype, bool isAC, bool isLeft, double errorrate, bool isReverse = false)
        {
            double time = 0;
            try
            {

                Dictionary<int, double[]> CursorData = ControlEquipMent.Oscillograph?.OscillographCursorData(channel, isCAN, schannel);

                double[] data = CursorData[LstChargerInfo[0].ChargerId];
                if (isReverse)
                {
                    data = data.Reverse().ToArray();
                }

                Dictionary<int, double> Points = ControlEquipMent.Oscillograph?.Oscillograph_Points_Single(data, value, uptype, !isLeft, isAC, errorrate);
                double returnvalue = Points[LstChargerInfo[0].ChargerId];
                if (returnvalue == -999)
                {
                    ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X_Single(channel, isCAN, schannel, 5, isLeft);
                    time = 9999;

                }
                else
                {
                    if(isReverse) { returnvalue = -returnvalue; }
                    ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X_Single(channel, isCAN, schannel, returnvalue, isLeft);
                    System.Threading.Thread.Sleep(2000);
                    Dictionary<int, double> ReadCurcor = ControlEquipMent.Oscillograph?.Oscillograph_ReadCurcor();

                    time = System.Math.Abs(ReadCurcor[LstChargerInfo[0].ChargerId]);
                }


            }
            catch (Exception ex)
            {
                SendException(ex);
            }



            return time;
        }





        public double GetTriggerTime_SingleWidthCount(int channel, bool isCAN, int schannel, double value, int uptype, bool isAC, bool isLeft, double errorrate, double CheckCount)
        {
            double time = 0;
            try
            {

                Dictionary<int, double[]> CursorData = ControlEquipMent.Oscillograph?.OscillographCursorData(channel, isCAN, schannel);

                double[] data = CursorData[LstChargerInfo[0].ChargerId];

                Dictionary<int, double> Points = ControlEquipMent.Oscillograph?.Oscillograph_Points_Single(data, value, uptype, !isLeft, isAC, errorrate, CheckCount);

                double returnvalue = Points[LstChargerInfo[0].ChargerId];
                if (returnvalue == -999)
                {
                    ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X_Single(channel, isCAN, schannel, 5, isLeft);
                    time = 9999;

                }
                else
                {
                    ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X_Single(channel, isCAN, schannel, returnvalue, isLeft);
                    System.Threading.Thread.Sleep(2000);
                    Dictionary<int, double> ReadCurcor = ControlEquipMent.Oscillograph?.Oscillograph_ReadCurcor();

                    time = System.Math.Abs(ReadCurcor[LstChargerInfo[0].ChargerId]);
                }


            }
            catch (Exception ex)
            {
                SendException(ex);
            }



            return time;
        }


















        /// <summary>
        /// 取触发的卡点时间
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="isCAN"></param>
        /// <param name="schannel"></param>
        /// <param name="value"></param>
        /// <param name="uptype">>0为上升，1为下降</param>
        /// <param name="isAC"></param>
        /// <param name="isLeft">是否为左边</param>
        /// <param name="errorrate">误差</param>
        /// <returns></returns>
        public double GetTriggerTime_Single_Position(int channel, bool isCAN, int schannel, double value, bool isLeft)
        {



            double time = 0;
            try
            {


                ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X_Single(channel, isCAN, schannel, value, isLeft);
                System.Threading.Thread.Sleep(2000);
                Dictionary<int, double> ReadCurcor = ControlEquipMent.Oscillograph?.Oscillograph_ReadCurcor();

                time = System.Math.Abs(ReadCurcor[LstChargerInfo[0].ChargerId]);

            }
            catch (Exception ex)
            {
                SendException(ex);
            }



            return time;
        }
        /// <summary>
        /// 取触发的卡点时间
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="isCAN"></param>
        /// <param name="schannel"></param>
        /// <param name="value"></param>
        /// <param name="uptype">>0为上升，1为下降</param>
        /// <param name="isAC"></param>
        /// <param name="isLeft">是否为左边</param>
        /// <param name="errorrate">误差</param>
        /// <returns></returns>
        public double GetTriggerTime_Single_VoltageExceeds(int channel, bool isCAN, int schannel, double value, int uptype, bool isAC, bool isLeft, double errorrate)
        {



            double returnvalue = 0;
            try
            {
                Dictionary<int, double[]> CursorData = ControlEquipMent.Oscillograph?.OscillographCursorData(channel, isCAN, schannel);
                double[] data = CursorData[LstChargerInfo[0].ChargerId];
                Dictionary<int, double> Points = ControlEquipMent.Oscillograph?.Oscillograph_Points_Single(data, value, uptype, !isLeft, isAC, errorrate);
                returnvalue = Points[LstChargerInfo[0].ChargerId];
                return returnvalue;


            }
            catch (Exception ex)
            {
                SendException(ex);
            }



            return returnvalue;
        }

        /// <summary>
        /// 取触发的卡点时间
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="isCAN"></param>
        /// <param name="schannel"></param>
        /// <param name="value"></param>
        /// <param name="uptype">>0为上升，1为下降</param>
        /// <param name="isAC"></param>
        /// <param name="isLeft">是否为左边</param>
        /// <param name="errorrate">误差</param>
        /// <returns></returns>
        public double GetTriggerTime_Single_VoltageExceeds_Max(int channel, bool isCAN, int schannel, double value, int uptype, bool isAC, bool isLeft, double errorrate)
        {



            double returnvalue = 0;
            try
            {

                Dictionary<int, double[]> CursorData = ControlEquipMent.Oscillograph?.OscillographCursorData(channel, isCAN, schannel);
                double[] data = CursorData[LstChargerInfo[0].ChargerId];
                double MAX = data.Max();
                value = MAX - 2;
                Dictionary<int, double> Points = ControlEquipMent.Oscillograph?.Oscillograph_Points_Single(data, value, uptype, !isLeft, isAC, errorrate);
                returnvalue = Points[LstChargerInfo[0].ChargerId];
                return returnvalue;


            }
            catch (Exception ex)
            {
                SendException(ex);
            }



            return returnvalue;
        }


        /// <summary>
        /// 取首个上升沿或者下降沿的时间
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="isCAN"></param>
        /// <param name="schannel"></param>
        /// <param name="value"></param>
        /// <param name="uptype">>0为上升，1为下降</param>
        /// <param name="isAC"></param>
        /// <param name="isLeft">是否为左边</param>
        /// <param name="errorrate">误差</param>
        /// <returns></returns>
        public double GetTriggerTime_Single2_NoCursor(int channel, bool isCAN, int schannel, double value, int uptype, bool isAC, bool isLeft, double errorrate, bool isReverse = false, int? CheckCount = null)
        {
            double dvalue = 0;
            try
            {

                Dictionary<int, double[]> CursorData = ControlEquipMent.Oscillograph?.OscillographCursorData(channel, isCAN, schannel);
                double[] data = CursorData[LstChargerInfo[0].ChargerId];
                if (isReverse)
                {
                    data = data.Reverse().ToArray();
                    uptype = uptype == 0 ? 1 : 0;
                }
                Dictionary<int, double> Points = ControlEquipMent.Oscillograph?.Oscillograph_Points_Single2(data, value, uptype, !isLeft, isAC, errorrate, CheckCount);
                double returnvalue = Points[LstChargerInfo[0].ChargerId];
                if (returnvalue == -999)
                {
                    dvalue = 10;

                }
                else
                {
                    dvalue = returnvalue + 5;
                    if (isReverse) { dvalue = 10 - dvalue; }
                }


            }
            catch (Exception ex)
            {
                SendException(ex);
            }



            dvalue = RetainDecimals<double>(dvalue);
            return dvalue;
        }

        /// <summary>
        /// 取首个上升沿或者下降沿的时间
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="isCAN"></param>
        /// <param name="schannel"></param>
        /// <param name="value"></param>
        /// <param name="uptype">>0为上升，1为下降</param>
        /// <param name="isAC"></param>
        /// <param name="isLeft">是否为左边</param>
        /// <param name="errorrate">误差</param>
        /// <returns></returns>
        public double GetTriggerTime_Single2(int channel, bool isCAN, int schannel, double value, int uptype, bool isAC, bool isLeft, double errorrate, int? CheckCount = null)
        {



            double time = 0;
            try
            {
                Dictionary<int, double[]> CursorData = ControlEquipMent.Oscillograph?.OscillographCursorData(channel, isCAN, schannel);
                double[] data = CursorData[LstChargerInfo[0].ChargerId];
                Dictionary<int, double> Points = ControlEquipMent.Oscillograph?.Oscillograph_Points_Single2(data, value, uptype, !isLeft, isAC, errorrate, CheckCount);
                double returnvalue = Points[LstChargerInfo[0].ChargerId];
                if (returnvalue == -999)
                {
                    ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X_Single(channel, isCAN, schannel, 5, isLeft);
                    time = 9999;

                }
                else
                {
                    ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X_Single(channel, isCAN, schannel, returnvalue, isLeft);
                    System.Threading.Thread.Sleep(2000);
                    Dictionary<int, double> ReadCurcor = ControlEquipMent.Oscillograph?.Oscillograph_ReadCurcor();

                    time = System.Math.Abs(ReadCurcor[LstChargerInfo[0].ChargerId]);
                }


            }
            catch (Exception ex)
            {
                SendException(ex);
            }



            return time;
        }


        /// <summary>
        /// 取首个上升沿或者下降沿的时间
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="isCAN"></param>
        /// <param name="schannel"></param>
        /// <param name="value"></param>
        /// <param name="isAC"></param>

        /// <returns></returns>
        public double GetTriggerTime_Single3(int channel, bool isCAN, int schannel, double value, bool isAC)
        {



            double dvalue = 0;
            try
            {

                Dictionary<int, double[]> CursorData = ControlEquipMent.Oscillograph?.OscillographCursorData(channel, isCAN, schannel);
                double[] data = CursorData[LstChargerInfo[0].ChargerId];
                Dictionary<int, double> Points = ControlEquipMent.Oscillograph?.Oscillograph_Points_Single3(data, value, isAC);
                double returnvalue = Points[LstChargerInfo[0].ChargerId];
                if (returnvalue == 10)
                {
                    dvalue = 10;

                }
                else
                {

                    System.Threading.Thread.Sleep(2000);
                    dvalue = returnvalue;
                }


            }
            catch (Exception ex)
            {
                SendException(ex);
            }



            dvalue = RetainDecimals<double>(dvalue);
            return dvalue;
        }



        /// <summary>
        /// 取第几个个上升沿或者下降沿的时间
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="isCAN"></param>
        /// <param name="schannel"></param>
        /// <param name="value"></param>
        /// <param name="uptype">>0为上升，1为下降</param>
        /// <param name="isAC"></param>
        /// <param name="isLeft">是否为左边</param>
        /// <param name="errorrate">误差</param>
        /// <param name="Count">取第几个</param>
        /// <returns></returns>
        public double OscillographInstrument_Points_Single_Multiple(int channel, bool isCAN, int schannel, double value, int uptype, bool isAC, bool isLeft, double errorrate, int Count)
        {



            double time = 0;
            try
            {
                Dictionary<int, double[]> CursorData = ControlEquipMent.Oscillograph?.OscillographCursorData(channel, isCAN, schannel);
                double[] data = CursorData[LstChargerInfo[0].ChargerId];

                Dictionary<int, double> Points = ControlEquipMent.Oscillograph?.Oscillograph_Points_Single_Multiple(data, value, uptype, !isLeft, isAC, errorrate, 0, Count);
                double returnvalue = Points[LstChargerInfo[0].ChargerId];
                if (returnvalue == 10)
                {
                    time = 9999;

                }
                else
                {

                    System.Threading.Thread.Sleep(2000);
                    Dictionary<int, double> ReadCurcor = ControlEquipMent.Oscillograph?.Oscillograph_ReadCurcor();

                    time = System.Math.Abs(ReadCurcor[LstChargerInfo[0].ChargerId]);
                }


            }
            catch (Exception ex)
            {
                SendException(ex);
            }



            return time;
        }

        /// <summary>
        /// 取第几个百分比的值
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="isCAN"></param>
        /// <param name="schannel"></param>
        /// <param name="value"></param>
        /// <param name="uptype">>0为上升，1为下降</param>
        /// <param name="isAC"></param>
        /// <param name="isLeft">是否为左边</param>
        /// <param name="errorrate">误差</param>
        /// <param name="Count">取第几个</param>
        /// <returns></returns>
        public double OscillographInstrument_Points_Single_Multiple2(int channel, bool isCAN, int schannel, double value, int uptype, bool isAC, bool isLeft, double errorrate, int Count, int? CheckCount = null)
        {
            double dvalue = 0;
            try
            {
                Dictionary<int, double[]> CursorData = ControlEquipMent.Oscillograph?.OscillographCursorData(channel, isCAN, schannel);
                double[] data = CursorData[LstChargerInfo[0].ChargerId];

                Dictionary<int, double> Points = ControlEquipMent.Oscillograph?.Oscillograph_Points_Single_Multiple(data, value, uptype, !isLeft, isAC, errorrate, 0, Count, CheckCount);
                double returnvalue = Points[LstChargerInfo[0].ChargerId];

                if (returnvalue == 10)
                {
                    dvalue = 10;

                }
                else
                {

                    dvalue = returnvalue + 5;
                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
            dvalue = RetainDecimals<double>(dvalue);
            return dvalue;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="isCAN"></param>
        /// <param name="schannel"></param>
        /// <param name="value"></param>
        /// <param name="uptype">>0为上升，1为下降</param>
        /// <param name="isAC"></param>
        /// <param name="isLeft">是否为左边</param>
        /// <param name="errorrate">误差</param>
        /// <param name="YValue">出来的值</param>
        /// <param name="WhichGrid">第几格</param>
        /// <returns></returns>
        public double GetTriggerTime_Single(int channel, bool isCAN, int schannel, double value, int uptype, bool isAC, bool isLeft, double errorrate, out double YValue, int WhichGrid)
        {
            YValue = 0;


            double time = 0;
            try
            {



                Dictionary<int, double[]> CursorData = ControlEquipMent.Oscillograph?.OscillographCursorData(channel, isCAN, schannel);
                double[] data = CursorData[LstChargerInfo[0].ChargerId];

                Dictionary<int, double> Points = ControlEquipMent.Oscillograph?.Oscillograph_Points_Single(data, value, uptype, !isLeft, isAC, errorrate);
                double returnvalue = Points[LstChargerInfo[0].ChargerId];


                if (data != null)
                {
                    if (data.Length > 10)
                    {
                        int index = data.Length / 10 * (WhichGrid - 1);
                        int index2 = data.Length / 10 * WhichGrid;
                        double y1value = 0;
                        for (int i = index - 1; i < index2 - 2; i++)
                        {
                            y1value += data[i];
                        }
                        YValue = y1value / data.Length * 10;
                        YValue = RetainDecimals<double>(YValue);
                    }
                }
                if (returnvalue == -999)
                {
                    ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X_Single(channel, isCAN, schannel, 5, isLeft);
                    time = 9999;

                }
                else
                {
                    ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X_Single(channel, isCAN, schannel, returnvalue, isLeft);
                    System.Threading.Thread.Sleep(2000);
                    Dictionary<int, double> ReadCurcor = ControlEquipMent.Oscillograph?.Oscillograph_ReadCurcor();

                    time = System.Math.Abs(ReadCurcor[LstChargerInfo[0].ChargerId]);
                }


            }
            catch (Exception ex)
            {
                SendException(ex);
            }



            return time;
        }



        /// <summary>
        /// 得到录波仪格子的值
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="isCAN"></param>
        /// <param name="schannel"></param>
        /// <param name="YValue">出来的值</param>
        /// <param name="GridCount">总格数</param>
        /// <param name="WhichGrid">第几格</param>
        public void GETInsertValue(int channel, bool isCAN, int schannel, out double YValue, double GridCount, double WhichGrid)
        {


            YValue = 0;

            try
            {

                Dictionary<int, double[]> CursorData = ControlEquipMent.Oscillograph?.OscillographCursorData(channel, isCAN, schannel);
                double[] data = CursorData[LstChargerInfo[0].ChargerId];
                if (data != null)
                {
                    if (data.Length > GridCount)
                    {
                        //int index = data.Length / GridCount * (WhichGrid - 1);
                        //int index2 = data.Length / GridCount * WhichGrid;
                        int index = Convert.ToInt32((double)data.Length * ((WhichGrid - 1) / GridCount));
                        int index2 = Convert.ToInt32((double)data.Length * (WhichGrid / GridCount));
                        double y1value = 0;
                        double count = 0;
                        for (int i = index - 1; i < index2 - 1; i++)
                        {
                            y1value += data[i];
                            count++;
                        }
                        YValue = y1value / count;
                        YValue = RetainDecimals<double>(YValue);
                    }
                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }


        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="isCAN"></param>
        /// <param name="schannel"></param>
        /// <param name="channel2"></param>
        /// <param name="isCAN2"></param>
        /// <param name="schannel2"></param>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <param name="uptype">>0为上升，1为下降</param>
        /// <param name="uptype2">>0为上升，1为下降</param>
        /// <param name="isAC"></param>
        /// <param name="errorrate">误差</param>
        /// <param name="isleft1">是否为左边</param>
        /// <param name="isleft2">是否为左边</param>
        /// <returns></returns>
        public double GetTriggerTime_X1X2(int channel, bool isCAN, int schannel, int channel2, bool isCAN2, int schannel2, double value1, double value2, int uptype, int uptype2, bool isAC, double errorrate, bool isleft1, bool isleft2)
        {


            double time = 0;
            try
            {

                Dictionary<int, double[]> CursorData = ControlEquipMent.Oscillograph?.OscillographCursorData(channel, isCAN, schannel);
                double[] data = CursorData[LstChargerInfo[0].ChargerId];
                Dictionary<int, double> Points = ControlEquipMent.Oscillograph?.Oscillograph_Points_Single(data, value1, uptype, !isleft1, isAC, errorrate, 300);
                double returnvalue1 = Points[LstChargerInfo[0].ChargerId];
                Dictionary<int, double[]> CursorData2 = ControlEquipMent.Oscillograph?.OscillographCursorData(channel2, isCAN2, schannel2);
                double[] data2 = CursorData2[LstChargerInfo[0].ChargerId];
                Dictionary<int, double> Points2 = ControlEquipMent.Oscillograph?.Oscillograph_Points_Single(data2, value2, uptype2, !isleft2, isAC, errorrate, 300);
                double returnvalue2 = Points2[LstChargerInfo[0].ChargerId];
                if (returnvalue1 == -999 || returnvalue2 == -999)
                {
                    ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X_Single(channel, isCAN, schannel, -5, true);
                    ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X_Single(channel, isCAN, schannel, 5, false);
                    time = 9999;

                }
                else
                {
                    ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X_Single(channel, isCAN, schannel, returnvalue1, true);
                    ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_X_Single(channel, isCAN, schannel, returnvalue2, false);
                    System.Threading.Thread.Sleep(2000);
                    Dictionary<int, double> ReadCurcor = ControlEquipMent.Oscillograph?.Oscillograph_ReadCurcor();

                    time = System.Math.Abs(ReadCurcor[LstChargerInfo[0].ChargerId]);
                }

            }
            catch (Exception ex)
            {
                SendException(ex);
            }



            return time;
        }





        ///// <summary>
        ///// 设置触发
        ///// </summary>
        ///// <param name="Level"></param>
        ///// <param name="chanenel"></param>
        ///// <param name="schannel"></param>
        ///// <param name="updown"></param>
        ///// <param name="isCAN"></param>
        ///// <param name="position"></param>
        public void OscillographInstrument_SetTrigger(double Level, int chanenel, int schannel, string updown, bool isCAN, double position, string TriggleType = "Single")
        {
            try
            {
                //通道1输出电流的变比可能是300或者录波仪不支持的档位，需要计算（现在只用A换算倍数）
                if (chanenel == 1)
                {
                    var oscillographCH1Config = EquipmentConfigManage.GetConfigParams(1, "Oscillograph_STRain_LSCale", "Oscillograph", "");
                    if (oscillographCH1Config != null && oscillographCH1Config.Length >= 3)
                    {
                        SystemEvent.SendLogMessage("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 开始设置录波仪通道1线性标尺");
                        if (double.TryParse(oscillographCH1Config[0], out double AVALue))
                        {
                            Level /= AVALue;
                        }
                    }
                }
                ControlEquipMent.Oscillograph?.Oscillograph_Trigger(updown, chanenel, isCAN, schannel, Level.ToString(), TriggleType, position);
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
        }

        public void OscillographInstrument_CursorPosition_Y_Value(int chanenel, double value1, double value2)
        {
            try
            {
                //通道1输出电流的变比可能是300或者录波仪不支持的档位，需要计算（现在只用A换算倍数）
                if (chanenel == 1)
                {
                    var oscillographCH1Config = EquipmentConfigManage.GetConfigParams(1, "Oscillograph_STRain_LSCale", "Oscillograph", "");
                    if (oscillographCH1Config != null && oscillographCH1Config.Length >= 3)
                    {
                        SystemEvent.SendLogMessage("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 开始设置录波仪通道1线性标尺");
                        if (double.TryParse(oscillographCH1Config[0], out double AVALue))
                        {
                            value1 /= AVALue;
                            value2 /= AVALue;
                        }
                    }
                }
                ControlEquipMent.Oscillograph?.Oscillograph_CursorPosition_Y_Value(chanenel, value1, value2);
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
        }


        /// <summary>
        /// 测量K3K4辅源电压下降时间，CSD和BSD统计报文时间
        /// </summary>
        /// <param name="Ta"></param>
        /// <param name="Tb"></param>
        /// <param name="Tc"></param>
        public void GetThreeTime(out double Ta, out double Tb, out double Tc)
        {
            Ta = Tb = Tc = 0;

            System.Threading.Thread.Sleep(500);

            try
            {
                //Dictionary<int, double[]> CursorData = ControlEquipMent.Oscillograph?.OscillographCursorData(2, false, 0);
                //double[] data = CursorData[LstChargerInfo[0].ChargerId];
                //Dictionary<int, double> Points = ControlEquipMent.Oscillograph?.Oscillograph_Points_Single(data, 6, 1, true, false, 0.05);
                //double returnvalue=Points[LstChargerInfo[0].ChargerId];
                //if (returnvalue == -999)
                //{

                //    Ta = 0;

                //}
                //else
                //{
                //    Ta = returnvalue * 0.1;
                //}
                Ta = GetTriggerTime_Single2_NoCursor(2, false, 0, 9, 1, false, false, 0.05) * 10;

                //CSD
                //Dictionary<int, double[]> CursorData2 = ControlEquipMent.Oscillograph?.OscillographCursorData(15, true, 8);
                //double[] data2 = CursorData2[LstChargerInfo[0].ChargerId];
                //Dictionary<int, double> Points2 = ControlEquipMent.Oscillograph?.Oscillograph_Points_Single(data2, 0.1, 0, true, false, 0.05);
                //double returnvalue2 = Points2[LstChargerInfo[0].ChargerId];
                //if (returnvalue2 == -999)
                //{

                //    Tb = 1;

                //}
                //else
                //{
                //    Tb = returnvalue2 * 0.1;
                //}
                Tb = GetTriggerTime_Single2_NoCursor(15, true, 8, 0.1, 0, false, false, 0.05) * 10;

                //BSD
                //Dictionary<int, double[]> CursorData3 = ControlEquipMent.Oscillograph?.OscillographCursorData(15, true, 7);
                //double[] data3 = CursorData3[LstChargerInfo[0].ChargerId];
                //Dictionary<int, double> Points3 = ControlEquipMent.Oscillograph?.Oscillograph_Points_Single(data3, 10, 0, true, false, 0.05);
                //double returnvalue3 = Points3[LstChargerInfo[0].ChargerId];
                //if (returnvalue3 == -999)
                //{

                //    Tc = 1;

                //}
                //else
                //{
                //    Tc = returnvalue3 * 0.1;
                //}
                Tc = GetTriggerTime_Single2_NoCursor(15, true, 7, 1, 0, false, false, 0.05) * 10;

            }
            catch (Exception ex)
            {
                SendException(ex);
            }


        }
        #endregion
    }
}
