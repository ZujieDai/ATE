using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.EquipMent;
using SaiTer.ATE.IDAL.SQLiteIDAL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Controls
{
    public class OscilloscopeControl : OscilloscopeBase
    {
        private string[] ClassNames = new string[] { "SDSOscilloscope", "emtTekOscilloscope", "DLMOscilloscope", "emtTekOscilloscope_MDO34", "emtKSOscilloscope_3000X", "emtRIGOLOscilloscope_MSO5000" };

        public override void OscilloscopeIDefalut(List<int> lstIDs)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    //ThreadPool.QueueUserWorkItem(state =>
                    //{
                    //    item.Value.OscilloscopeIDefalut();
                    //});
                    item.Value.OscilloscopeIDefalut();

                }
            }
        }

        public override void Oscilloscope_SetACQuireMode(List<int> lstIDs, int mode, int num = 0)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.Oscilloscope_SetACQuireMode(mode, num);

                }
            }
        }

        /// <summary>
        /// 通过程控板控制示波器的通道切换
        /// 1通道--交流桩输出电压/直流输出电压
        /// 2通道--交流输出电流/直流输出电流
        /// 3通道--欧标交流导引CP/交直流桩输入交流电流/欧标直流CP信号
        /// 4通道--桩输入电压
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="configType"></param>
        /// <param name="reMark"></param>
        private void SwitchChannelByControlBoard(List<int> lstIDs, string configType, string reMark)
        {
            try
            {
                for (int i = 0; i < lstIDs.Count; i++)
                {
                    var cb = DitControlEquipMent("emtControlBoard");
                    if (cb.Values.First() is emtControlBoard controlBoard)
                    {
                        List<bool> lstRelay = controlBoard.ControlBoardReadState();
                        if (lstRelay == null || lstChargerInfo.Find(c => c.ChargerId == lstIDs[i]) == null)
                        {
                            return;
                        }

                        string[] configParams = EquipmentConfigManage.GetConfigParams((int)lstChargerInfo.Find(c => c.ChargerId == lstIDs[i])?.ChargerType, configType, "ControlBoard", reMark);
                        if (configParams == null)
                            continue;
                        if (configParams.Length > 0 && configParams[0] != null)
                        {
                            string[] closeRelay = configParams[0].Split('=')[1].Split('|');
                            foreach (string str in closeRelay)
                            {
                                lstRelay[Convert.ToInt32(str) - 1] = true;
                            }
                        }
                        if (configParams.Length > 1 && configParams[1] != null)
                        {
                            string[] openRelay = configParams[1].Split('=')[1].Split('|');
                            foreach (string str in openRelay)
                            {
                                lstRelay[Convert.ToInt32(str) - 1] = false;
                            }
                        }
                        controlBoard.ControlResistanceSetRelay(lstRelay);
                    }
                }
            }
            catch(Exception ex)
            {
                SendExMsg(ex);
            }


        }

        /// <summary>
        /// 示波器通道设置
        /// </summary>
        /// <param name="lstIDs">需要设置的枪位号</param>
        /// <param name="channel">通道(比如1、2、3)</param>
        /// <param name="gear">纵坐标档位  例子:10,20,50,100,200，现在单位为V，要根据探头比来相应设置)</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public override void Oscilloscope_Channel_SetGear(List<int> lstIDs, int channel, string gear) 
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.Oscilloscope_Channel_SetGear(channel, gear);
                }
            }
        }

        public override void Oscilloscope_Channel_Set(List<int> lstIDs, int channel, bool isOpen, string coupling, string tapewidth, string probe, string tag, string impedance, string unit, bool isOpen_A, string gear, string position)
        {
            if (isOpen)
            {
                // 通道2直流输出电流探头变比100:1，交流输出电流探头20:1
                if (channel == 2 && tag.IndexOf("AC") > -1)
                    probe = "20";
                // 通道3交直流输入电流探头变比100:1，CP探头1:1
                // 2024/11/22——CP探头比需要改为20比1，后续系统柜也是
                // 2025/5/26——LJ的CP探头比为50:1
                if (channel == 3 && tag.IndexOf("CP") > -1 && probe != "50")
                    probe =  "1";
                // 设置程控板切换通道
                SwitchChannelByControlBoard(lstIDs, "Oscilloscope_Channel_" + channel, tag);
            }
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                Channel_Set(lstIDs, channel, isOpen, coupling, tapewidth, probe, tag, impedance, unit, isOpen_A, gear, position, item);
            }

        }

        private static void Channel_Set(List<int> lstIDs, int channel, bool isOpen, string coupling, string tapewidth, string probe, string tag, string impedance, string unit, bool isOpen_A, string gear, string position, KeyValuePair<int, EquipMentBase> item)
        {
            int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
            if (id.Length > 0)
            {
                item.Value.Oscilloscope_Channel_Set(channel, isOpen, coupling, tapewidth, probe, tag, impedance, unit, isOpen_A, gear, position);
            }
        }

        public override void Oscilloscope_Measure_Initialize(List<int> lstIDs)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.Oscilloscope_Measure_Initialize();
                }
            }
        }

        public override void Oscilloscope_AddMeasure(List<int> lstIDs, string MeasurementType, int channel)
        {

            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.Oscilloscope_AddMeasure(MeasurementType, channel);
                }
            }
        }


        public override Dictionary<int, string> Oscilloscope_ReadMeasure(List<int> lstIDs, int MeasureNumber)
        {
            Dictionary<int, string> dicMeasure = new Dictionary<int, string>();
            string Measure = "";
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.Oscilloscope_ReadMeasure(MeasureNumber, ref Measure);
                    if (Measure == "")
                    {
                        item.Value.Oscilloscope_ReadMeasure(MeasureNumber, ref Measure);
                    }
                    dicMeasure.Add(id[0], Measure);
                }
            }

            return dicMeasure;
        }


        public override Dictionary<int, string> Oscilloscope_ReadMeasure(List<int> lstIDs, string MeasureType, int channel, bool isIMMed = false)
        {
            Dictionary<int, string> dicMeasure = new Dictionary<int, string>();
            string Measure = "";
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.Oscilloscope_ReadMeasure(MeasureType, channel, ref Measure, isIMMed);
                    if (Measure == "")
                    {
                        item.Value.Oscilloscope_ReadMeasure(MeasureType, channel, ref Measure, isIMMed);
                    }
                    dicMeasure.Add(id[0], Measure);
                }
            }

            return dicMeasure;
        }
        public override void Oscilloscope_TimeBase(List<int> lstIDs, bool isroll, string timebase, string delay)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.Oscilloscope_TimeBase(isroll, timebase, delay);
                }
            }
        }

        public override void Oscilloscope_Trigger(List<int> lstIDs, int type_first, string type_second, string coupling, string typeout_type, string timeout, int channel, string triggerLevel, string triggerType)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.Oscilloscope_Trigger(type_first, type_second, coupling, typeout_type, timeout, channel, triggerLevel, triggerType);

                }
            }
        }



        public override void Oscilloscope_TriggerTypeSet(List<int> lstIDs, string TriggerType)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.Oscilloscope_TriggerTypeSet(TriggerType);
                }
            }
        }
        public override Dictionary<int, bool> ReadTriggerState(List<int> lstIDs)
        {
            Dictionary<int, bool> dic = new Dictionary<int, bool>();
            bool isTriggle = false;
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                   item.Value.ReadTriggerState(ref isTriggle);
                    dic.Add(id[0], isTriggle);
                }
            }
            return dic;
        }

        public override Dictionary<int, int> Oscilloscope_ReadTrigger(List<int> lstIDs)
        {
            Dictionary<int, int> dic = new Dictionary<int, int>();
            int TriggleType = -1;
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    TriggleType = item.Value.Oscilloscope_ReadTrigger();
                    dic.Add(id[0], TriggleType);
                }
            }
            return dic;
        }

        public override Dictionary<int, double[]> OscilloscopeCursorData(List<int> lstIDs, int channnel, int RecondLengZero = 10000 * 20)
        {
            Dictionary<int, double[]> dic = new Dictionary<int, double[]>();
            double[] data = null;
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.OscilloscopeCursorData(channnel, ref data, RecondLengZero);
                    if (data == null)
                    {
                        item.Value.OscilloscopeCursorData(channnel, ref data, RecondLengZero);
                    }
                    dic.Add(id[0], data);
                }
            }

            return dic;
        }


        public override Dictionary<int, string> OscilloscopeSaveScreen(List<int> lstIDs)
        {
            Dictionary<int, string> dicPath = new Dictionary<int, string>();
            string path = "";
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    //ThreadPool.QueueUserWorkItem(state =>
                    //{
                    //    item.Value.OscilloscopeSaveScreen(ref path);
                    //});

                    item.Value.OscilloscopeSaveScreen(ref path);
                    if(string.IsNullOrEmpty(path))
                        item.Value.OscilloscopeSaveScreen(ref path);
                    dicPath.Add(id[0], path);
                }
            }
            return dicPath;

        }

        public override void Oscilloscope_IsRun(List<int> lstIDs, bool isRun)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    ThreadPool.QueueUserWorkItem(state =>
                    {
                        item.Value.Oscilloscope_IsRun(isRun);
                    });
                }
            }
        }

        public override void Oscilloscope_ReadRun(List<int> lstIDs, ref bool isrun)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.Oscilloscope_ReadRun(ref isrun);
                }
            }
        }



        public override void Oscilloscope_CursorsSet(List<int> lstIDs, string type)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.Oscilloscope_CursorsSet(type);
                }
            }
        }

        public override void Oscilloscope_CursorPosition_X(List<int> lstIDs, int channel, double value1, double value2) { }        /// <summary>



        public override void Oscilloscope_CursorPosition_X_Single(List<int> lstIDs, int channel, Dictionary<int, double> values, bool isleft)
        {

            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.Oscilloscope_CursorPosition_X_Single(channel, values[item.Value.ChargerID], isleft);
                }
            }
        }

        public override void Oscilloscope_CursorPosition_X_Time(List<int> lstIDs, int channel, double time, bool isleft) { }

        public override void Oscilloscope_CursorPosition_Y(List<int> lstIDs, int channel, double value1, double value2)
        {

            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.Oscilloscope_CursorPosition_Y(channel, value1, value2);
                }
            }
        }

        public override void Oscilloscope_CursorPosition_Y(List<int> lstIDs, int channel, double value, int cursorIndex)
        {

            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.Oscilloscope_CursorPosition_Y(channel, value, cursorIndex);
                }
            }
        }


        public override void Oscilloscope_ReadCursors(List<int> lstIDs, int channel, ref Dictionary<int, double[]> Cursors)
        {
            try
            {
                double[] dtmp = null;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {

                        item.Value.Oscilloscope_ReadCursors(channel, ref dtmp);

                        if (dtmp == null)
                        {
                            item.Value.Oscilloscope_ReadCursors(channel, ref dtmp);
                        }

                        if (Cursors.ContainsKey(item.Value.ChargerID))
                        {
                            Cursors[item.Value.ChargerID] = dtmp;
                        }
                        else
                        {
                            Cursors.Add(item.Value.ChargerID, dtmp);
                        }

                    }
                }
            }
            catch (Exception e) { Log.Log.LogException(e); }
            //Cursors = null; 
        }

        public override void Oscilloscope_Touch(List<int> lstIDs, bool isTouch) { }

        public override void Oscilloscope_StorageDepth(List<int> lstIDs, string storagedepth)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.Oscilloscope_StorageDepth(storagedepth);
                }
            }
        }

        public override void Oscilloscope_IsRoll(List<int> lstIDs, bool isroll) { }

        public override void Oscilloscope_Points(List<int> lstIDs, double[] data, double upvalue, double downvalue, int uptype, bool isAC, ref double[] position) { position = null; }




        public override void Oscilloscope_Points_Single(List<int> lstIDs, Dictionary<int, double[]> data, double value, int uptype, bool isright, bool isAC, ref Dictionary<int, double> Position)
        {
            double position = -999;
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    double dtmp = -999; 
                item.Value.Oscilloscope_Points_Single(data[item.Value.ChargerID], value, uptype, isright, isAC, ref dtmp);

                    if (Position.ContainsKey(item.Value.ChargerID))
                    {
                        Position[item.Value.ChargerID] = dtmp;
                    }
                    else
                    {
                        Position.Add(item.Value.ChargerID, dtmp);
                    }
                }


            }

        }


        public override void Oscilloscope_Points_Single_AC(List<int> lstIDs, double[] data, double value, int uptype, bool isright, bool isAC, int count, ref double position) { position = -1; }


        /// <summary>
        /// 设置通道自动调整
        /// </summary>
        /// <param name="channel">通道(1、2、3、4等)</param>
        public override void Oscilloscope_AutoZero(List<int> lstIDs, int channel)
        {
            try
            {
                Dictionary<int, List<bool>> res = new Dictionary<int, List<bool>>();
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {
                        item.Value.Oscilloscope_AutoZero(channel);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }
    }
}
