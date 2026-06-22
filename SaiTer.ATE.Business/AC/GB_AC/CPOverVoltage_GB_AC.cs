using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.Remoting.Channels;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// CP回路电压超限值测试_国标交流
    /// </summary>
    public class CPOverVoltage_GB_AC : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储
        /// <summary>
        /// CC断开后的延时时间
        /// </summary>
        int sleepTime = 2000;

        UInt16 AboveR2 = 1300;//上偏超限R2电阻(Ω)
        UInt16 AboveR3 = 2740;

        UInt16 BelowR2 = 1300;//下偏超限R2电阻(Ω)
        UInt16 BelowR3 = 2740;//下偏超限R3电阻(Ω)

        UInt16 BelowR2_2 = 1300;//下偏超限R2电阻2(Ω)
        UInt16 BelowR3_2 = 2740;//下偏超限R3电阻2(Ω)

        private string maxValue = "300";
        public CPOverVoltage_GB_AC(int trialType) { TrialType = trialType; }

        public override void InitializeParams()
        {
            Init();
            //这里要从数据库里来解析
            string[] TrialParams = TrialItem.ResultParams.Split('|');
            if (TrialParams.Length >= 6)
            {
                AboveR2 = Convert.ToUInt16(double.Parse(TrialParams[0].Split('=')[1]));
                AboveR3 = Convert.ToUInt16(double.Parse(TrialParams[1].Split('=')[1]));
                BelowR2 = Convert.ToUInt16(double.Parse(TrialParams[2].Split('=')[1]));
                BelowR3 = Convert.ToUInt16(double.Parse(TrialParams[3].Split('=')[1]));
                BelowR2_2 = Convert.ToUInt16(double.Parse(TrialParams[4].Split('=')[1]));
                BelowR3_2 = Convert.ToUInt16(double.Parse(TrialParams[5].Split('=')[1]));
            }
            //string[] TrialParams = TrialItem.ResultParams.Split('|')[0].Split('=');

            //if (TrialParams.Length == 2)
            //{
            //    maxValue = TrialParams[1];
            //}
        }

        public override void InitEquiMent()
        {

        }
        public override void ExecuteMethod()
        {
            try
            {
                InitializeParams();
                InitEquiMent();
                StartItemFlow();
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
            finally
            {
                List<bool> Ks = GetKStatus16_Charging();
                // Ks[5] = false;
                Ks[0] = false;
                ControlEquipMent.BMS.BMS_SetKState(lstIDs, Ks);
                SetCPReresh();

                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                SaveTrialResult();
                SendMessageEndThisTrial();
            }
        }




        /// <summary>
        /// 测试流程
        /// </summary>
        public void StartItemFlow()
        {
            SendNoticeToUIAndTxtFile("开始" + TrialItem.ItemName + "--------------------------->");
            _StopWatch.Reset();
            _StopWatch.Start();
            while (true)
            {
                testWorkParam.lstIDs.Clear();
                for (int i = 0; i < LstTrialData.Count; i++)
                {
                    if (LstTrialData[i].IsCheck)
                    {
                        if (LstTrialData[i].TrialResult == EmTrialResult.Wait)
                        {
                            if (!testWorkParam.lstIDs.Contains(LstTrialData[i].ChargerId))
                            {
                                testWorkParam.lstIDs.Add(LstTrialData[i].ChargerId);
                            }
                        }
                    }
                }
                //是否全部有结论
                if (testWorkParam.lstIDs.Count <= 0) break;
                //是否超时
                if (_StopWatch.ElapsedMilliseconds / 1000 > 10)
                {
                    for (int i = 0; i < LstTrialData.Count; i++)
                    {
                        if (LstTrialData[i].IsCheck)
                        {
                            if (LstTrialData[i].TrialResult == EmTrialResult.Wait)
                            {
                                LstTrialData[i].TrialResult = EmTrialResult.Fail;
                                LstTrialData[i].TrialValue = ((int)(_StopWatch.ElapsedMilliseconds / 1000)).ToString();
                                int k = LstChargerInfo.FindIndex(s => s.ChargerId == LstTrialData[i].ChargerId);
                                LstTrialData[i].PKID = LstChargerInfo[k].PKID;
                                //界面展示的数据项格式
                                //CP占空比测量值1(%)|CP占空比测量值2(%)|CP占空比测量值3(%)|CP占空比下限|CP占空比上限|测试结果|查看示波器截图
                                //LstTrialData[i].ExtentData = "null|null|null|" + PwmMin + "|" + PwmMax;
                                LstTrialData[i].ExtentData = "null|null|null|null|null";

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }

                if (testWorkParam.lstIDs.Count == 0)
                {
                    return;
                }
                if (!CheckChargerIn(testWorkParam.lstIDs))
                {
                    return;
                }
                ControlEquipMent.BMS.BMS_SetResistance(testWorkParam.lstIDs, 1300, 2740);//控制导引设置正常电阻
                int waitTime = 50;
                //初始化示波器
                SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
                ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, false, "DC", "20", Channel1, "Output_Voltage", "50", "V", false, "250", "-2.5");
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, true, "DC", "20", Channel2, "AC_Output_Current", "50", "V", false, "40", "0");
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "0.25", Channel3, "CP_Voltage", "50", "V", false, "5", "0");
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");
                //设置时基ms
                ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "0.2", "0");//时基   延时
                Thread.Sleep(waitTime);
                SendNoticeToUIAndTxtFile("添加示波器测量项");
                //添加测量值
                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比
                Thread.Sleep(sleepTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "TOP", 3);//高值
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "BASE", 3);//低值

                //步骤5、设置触发
                ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "3", "Auto");//上升边沿触发  3通道  3V电平 自动
                Thread.Sleep(waitTime);
                //启动示波器
                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                ControlEquipMent.BMS.BMS_SetResistance(testWorkParam.lstIDs, AboveR2, AboveR3);//控制导引设置CP上偏超限电压10V

                Thread.Sleep(1000);
                Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "TOP", 3);
                ProcessDataTmp(Data_Tmp, "状态2’", "CP上偏超限电压(V)", "9.8", "12.8");

                Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FREQ", 3);
                ProcessData(Data_Tmp, "状态2’", "CP频率(Hz)", "970", "1030");

                ControlEquipMent.ACSource?.ACSource_OFF(testWorkParam.lstIDs);
                ControlEquipMent.BMS.BMS_SetResistance(testWorkParam.lstIDs, 1300, 2740);
                Thread.Sleep(3000);
                ControlEquipMent.ACSource?.ACSource_ON(testWorkParam.lstIDs);

                if (!CheckChargerIn(testWorkParam.lstIDs))
                {
                    return;
                }

                //检测是否刷卡（有PWM波即可）
                WaitSwipingCard(testWorkParam.lstIDs, 2);

                //步骤13、设置触发
                ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "3", "Auto");//上升边沿触发  3通道  3V电平 自动
                Thread.Sleep(waitTime);
                //启动示波器
                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);

                ControlEquipMent.BMS.BMS_SetResistance(testWorkParam.lstIDs, BelowR2, BelowR3);//控制导引设置CP下偏超限电压7V

                Thread.Sleep(1000);
                Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "TOP", 3);
                ProcessDataTmp(Data_Tmp, "状态2’", "CP下偏超限电压(V)", "0", "8.2");

                Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FREQ", 3);
                ProcessData(Data_Tmp, "状态2’", "CP频率(Hz)", "970", "1030");

                ControlEquipMent.ACSource?.ACSource_OFF(testWorkParam.lstIDs);
                ControlEquipMent.BMS.BMS_SetResistance(testWorkParam.lstIDs, 1300, 2740);//控制导引设置正常电阻
                Thread.Sleep(3000);
                ControlEquipMent.ACSource?.ACSource_ON(testWorkParam.lstIDs);

                //步骤17
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "Output_Voltage", "50", "V", false, "250", "-2.5");
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "0.25", Channel3, "CP_Voltage", "50", "V", false, "5", "2.5");
                Thread.Sleep(waitTime);
                //设置时基ms
                ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "400", "0");//时基   延时

                //添加测量值
                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "TOP", 1);
                Thread.Sleep(waitTime);
                //添加测量值
                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "Base", 1);
                Thread.Sleep(waitTime);
                if (!CheckChargerIn(testWorkParam.lstIDs))
                {
                    return;
                }
                //检测是否刷卡（有PWM波即可）
                WaitSwipingCard(testWorkParam.lstIDs, 2);
                //闭合开关S2，启动充电
                ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                SetResisLoadVolCurrAndStart(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, LstChargerInfo[0].NominalCurrent);
                //ControlEquipMent.ResistanceLoad.SetResisLoadVolCurr(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, LstChargerInfo[0].NominalCurrent);
                //Thread.Sleep(100);
                //ControlEquipMent.ResistanceLoad.ResistanceLoad_ON(testWorkParam.lstIDs);
                //步骤22、设置触发
                ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "3", "Auto");//上升边沿触发  3通道  3V电平 自动
                Thread.Sleep(waitTime);
                //启动示波器
                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                ControlEquipMent.BMS.BMS_SetResistance(testWorkParam.lstIDs, AboveR2, AboveR3);//控制导引设置CP上偏超限电压10V
                Thread.Sleep(1000);
                Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "TOP", 3);
                ProcessDataTmp(Data_Tmp, "状态3", "CP上偏超限电压(V)", "6.8", "12.8");

                Data_Tmp.Clear();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage.ToString());
                }
                ProcessDataTmp(Data_Tmp, "状态3", "输出电压", "0", "50");


                //步骤26
                ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(testWorkParam.lstIDs);
                ControlEquipMent.ACSource?.ACSource_OFF(testWorkParam.lstIDs);
                ControlEquipMent.BMS.BMS_SetResistance(testWorkParam.lstIDs, 1300, 2740);//控制导引设置正常电阻
                Thread.Sleep(3000);
                ControlEquipMent.ACSource?.ACSource_ON(testWorkParam.lstIDs);
                if (!CheckChargerIn(testWorkParam.lstIDs))
                {
                    return;
                }
                //检测是否刷卡（有PWM波即可）
                WaitSwipingCard(testWorkParam.lstIDs, 2);
                //闭合开关S2，启动充电
                ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                SetResisLoadVolCurrAndStart(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, LstChargerInfo[0].NominalCurrent);
                //ControlEquipMent.ResistanceLoad.SetResisLoadVolCurr(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, LstChargerInfo[0].NominalCurrent);
                //Thread.Sleep(100);
                //ControlEquipMent.ResistanceLoad.ResistanceLoad_ON(testWorkParam.lstIDs);

                //步骤30、设置触发
                ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "10", 3, "3", "Auto");//上升边沿触发  3通道  3V电平 自动
                Thread.Sleep(waitTime);
                //启动示波器
                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                ControlEquipMent.BMS.BMS_SetResistance(testWorkParam.lstIDs, BelowR2_2, BelowR3_2);//控制导引设置CP下偏超限电压4V
                Thread.Sleep(1000);
                Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "TOP", 3);
                ProcessDataTmp(Data_Tmp, "状态3’", "CP上偏超限电压(V)", "0", "5.2");

                Data_Tmp.Clear();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage.ToString());
                }
                ProcessDataTmp(Data_Tmp, "状态3’", "输出电压", "0", "50");

                //设置测试条件
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    int key = testWorkParam.lstIDs[i];
                    if (AllEquipStateData.DicACSource_StateData.Count == 1)
                    {
                        key = 1;//多个桩只有一个源的时候，源实时状态字典内只有一个1号桩
                    }
                    d1.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicACSource_StateData[key].Volt.ToString());
                    d2.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicACSource_StateData[key].Freq.ToString());
                }
                SetConditionValue("供电电压(V)", d1);
                SetConditionValue("供电频率(Hz)", d2);
                ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(testWorkParam.lstIDs);
                ControlEquipMent.BMS.BMS_SetResistance(testWorkParam.lstIDs, 1300, 2740);//控制导引设置正常电阻

            }
        }



        public override void ProcessData()
        {

        }

        /// <summary>
        /// 保存测试数据（数据在上下限之间为FAIL）
        /// </summary>
        /// <param name="datas">数据</param>
        /// <param name="sState">状态</param>
        /// <param name="sName">名称</param>
        /// <param name="minValue">下限</param>
        /// <param name="maxValue">上限</param>
        /// <param name="dImages">截图</param>
        public void ProcessData(Dictionary<int, string> datas, string sState, string sName, string minValue, string maxValue, Dictionary<int, string> dImages = null)
        {
            try
            {
                foreach (var item in testWorkParam.lstIDs)
                {
                    StringBuilder sbtmp = new StringBuilder();
                    int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                    int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                    if (k < 0)
                        return;
                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    LstTrialData[k].ItemName = iIndex.ToString();
                    bool bSx = true;//是否合格


                    double dData = Convert.ToDouble(datas[LstChargerInfo[i].ChargerId]);//数据
                    if (dImages != null)
                    {
                        sbtmp.Append(dImages[LstChargerInfo[i].ChargerId]);
                    }
                    else
                    {
                        sbtmp.Append("报表(勿删)");
                    }
                    double dSx = 0;//上限    
                    double dXx = 0;//下限
                    double.TryParse(maxValue, out dSx);
                    double.TryParse(minValue, out dXx);

                    if (dData <= dSx && dData >= dXx)
                    {
                        bSx = false;
                    }

                    else if (maxValue.Trim() == "*" || maxValue.Trim() == "-")
                    {
                        bSx = true;
                    }


                    if (bSx)
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                    }
                    //界面展示的数据项格式
                    //状态|数据名称|测量值|上限|下限|结果
                    LstTrialData[i].ExtentData = sState
                        + "|" + sName
                        + "|" + minValue
                        + "|" + maxValue
                        + "|" + datas[LstChargerInfo[i].ChargerId].ToString()
                        + "|" + sbtmp.ToString();

                    //LstTrialData[i].Data1 = sState
                    //  + "|" + sName
                    //  + "|" + minValue
                    //  + "|" + maxValue
                    //  + "|" + datas[LstChargerInfo[i].ChargerId].ToString();


                    //+ "|" + LstTrialData[k].TrialResult.ToString();
                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                    SendTrialDataToUI(LstTrialData[k]);
                    SaveTrialData(LstTrialData[k]);
                }


            }

            catch (Exception ex)
            {
                SendException(ex);
            }

            iIndex++;

        }

    }
}
