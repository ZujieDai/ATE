using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 输入频率异常检测（包含输入过频、输入欠频）
    /// </summary>
    public class InputFreqError : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据


        double NomarlFreq = 50;//正常频率
        double SetFreq = 50;//设定异常频率

      
        public InputFreqError(int type)
        {
            TrialType = type;
        }

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            NomarlFreq = Convert.ToDouble(strParams[0].Split('=')[1]);
            SetFreq = Convert.ToDouble(strParams[1].Split('=')[1]);           
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
                SetCPReresh();
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                SaveTrialResult();
                SendMessageEndThisTrial();
            }
        }

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
                                //测试时间|输入电压|是否停机|测试结果     
                                LstTrialData[i].ExtentData = DateTime.Now.ToString() + "|" + SetFreq + "|" + NomarlFreq + "|未停机";

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }

                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                }

                //开始检测流程
                if (testWorkParam.lstIDs.Count > 0)
                {

                   

                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }



                    Thread.Sleep(5000);

                    ControlEquipMent.ACSource.ACSource_SetFreq(testWorkParam.lstIDs, SetFreq);
                    

                    SendNoticeToUIAndTxtFile("已发送交流源频率异常值：" + SetFreq + "Hz，等待交流源输出稳定。");
                    //Thread.Sleep(1000 * 16);

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

                    //采集数据
                    Data_Tmp = new Dictionary<int, string>();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        double voltage = 0;
                        if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_GB_DC ||
                                LstChargerInfo[0].ChargerType == EmChargerType.Charger_USA_DC ||
                                LstChargerInfo[0].ChargerType == EmChargerType.Charger_EUR_DC)
                        {
                            voltage = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage;
                            for (int j = 0; j < 20; j++)
                            {
                                if (voltage > 20)
                                {
                                    voltage = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage;

                                    Thread.Sleep(1000);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        else if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_GB_AC ||
                            LstChargerInfo[0].ChargerType == EmChargerType.Charger_USA_AC ||
                            LstChargerInfo[0].ChargerType == EmChargerType.Charger_EUR_AC)
                        {
                            voltage = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage;
                            for (int j = 0; j < 20; j++)
                            {
                                if (voltage > 20)
                                {
                                    voltage = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage;
                                    Thread.Sleep(1000);
                                }
                                else
                                {
                                    break;
                                }
                            }
                            //voltage = 0;
                        }
                        Data_Tmp.Add(testWorkParam.lstIDs[i], voltage.ToString("F2"));
                    }
                    ProcessDataTmp(Data_Tmp, "输入频率异常", "保护后桩输出电压(V)", "0", "30");

                    SendNoticeToUIAndTxtFile("恢复正常频率：" + NomarlFreq + "Hz，等待交流源输出稳定。");
                    ControlEquipMent.ACSource.ACSource_SetFreq(lstIDs, NomarlFreq);
                    //ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);



                    Thread.Sleep(2000);
                }
            }
        }


        public override void ProcessData()
        {
           
        }
    }
}
