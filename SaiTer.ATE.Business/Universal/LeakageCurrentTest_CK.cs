using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 漏电流保护测试（程控板控制电阻，判断状态即可，目前TB在用）
    /// </summary>
    public class LeakageCurrentTest_CK : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        public LeakageCurrentTest_CK(int type) { TrialType = type; }
        int index1 = 0;//需要控制的继电器索引号
        int index2 = 0;//需要控制的继电器索引号
        public override void InitializeParams()
        {
            //数据库参数格式
            //需要控制的继电器=7,8

            Init();
            try
            {
                string[] strParams = TrialItem.ItemParams.Split('|');
                string[] relayIndex = strParams[0].Split('=')[1].Split(',');
                index1 = Convert.ToInt32(double.Parse(relayIndex[0])) - 1;
                if (relayIndex.Length == 2)
                {
                    index2 = Convert.ToInt32(double.Parse(relayIndex[1])) - 1;
                }
                else
                {
                    index2 = index1;
                }
            }
            catch (Exception ex)
            {
                SendNoticeToUIAndTxtFile("请正确配置继电器参数");
                SendException(ex);
            }
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

        /// <summary>
        /// 测试流程
        /// </summary>
        public void StartItemFlow()
        {
            try
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
                                    LstTrialData[i].ExtentData = "null|null|null|null|null";

                                    SendTrialDataToUI(LstTrialData[i]);
                                }
                            }
                        }
                        break;
                    }


                    if (!CheckChargerIn(testWorkParam.lstIDs))
                    {
                        return;
                    }



                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);


                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }

                    SendNoticeToUIAndTxtFile("模拟漏电流输出");
                    List<bool> lstRelay = new List<bool>();
                    for (int i = 0; i < 16; i++)
                    {
                        lstRelay.Add(false);
                    }
                    lstRelay[0] = true;
                    lstRelay[index1] = true;
                    lstRelay[index2] = true;
                    SendNoticeToUIAndTxtFile("控制K" + (index1 + 1).ToString() + "、" + (index2 + 1).ToString() + "闭合");

                    ControlEquipMent.ControlBoard.ControlResistanceSetRelay(lstRelay);


                    //设置测试条件
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        int key = testWorkParam.lstIDs[i];
                        if (AllEquipStateData.DicBMS_AC_StateData.Count == 1)
                        {
                            key = 1;//多个桩只有一个源的时候，源实时状态字典内只有一个1号桩
                        }
                        d1.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicACSource_StateData[key].Volt.ToString());
                        d2.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicACSource_StateData[key].Freq.ToString());
                    }
                    SetConditionValue("供电电压(V)", d1);
                    SetConditionValue("供电频率(Hz)", d2);

                    Thread.Sleep(1000 * 3);

                    //恢复状态
                    SendNoticeToUIAndTxtFile("恢复正常状态");
                    lstRelay.Clear();
                    for (int i = 0; i < 16; i++)
                    {
                        lstRelay.Add(false);
                    }
                    lstRelay[0] = true;
                    ControlEquipMent.ControlBoard.ControlResistanceSetRelay(lstRelay);
                    Thread.Sleep(5000);
                    //断线后的电压
                    Data_Tmp = new Dictionary<int, string>();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage.ToString("F2"));
                    }
                    

                    if (TrialType == (int)EmTrialType.漏电保护测试_程控板)
                    {
                        ProcessDataTmp(Data_Tmp, "漏电保护测试", "漏电后输出电压(V)", "0", "30");
                    }
                    else if (TrialType == (int)EmTrialType.不动作漏电测试_程控板)
                    {
                        ProcessDataTmp(Data_Tmp, "不动作漏电测试", "漏电后输出电压(V)", "80", "-");
                    }

                   
                    //重新上电清除故障
                    ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);
                   
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }



        public override void ProcessData()
        {

        }



    }
}
