using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 欧标研测交流：最大允许充电电流
    /// </summary>
    public class CCS2_RT_AC_MaximumAllowableCurrentTest : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储
        double OutputCurrent = 40;
        double DemandCurrent = 30;
        public CCS2_RT_AC_MaximumAllowableCurrentTest(int type)
        {
            TrialType = type;
        }

        public override void InitializeParams()
        {
            Init();
            //输出过流值(A)=20.00|正常电流值(A)=16.00
            string[] strParams = TrialItem.ResultParams.Split('|');
            OutputCurrent = Convert.ToDouble(strParams[0].Split('=')[1]);
            DemandCurrent = Convert.ToDouble(strParams[1].Split('=')[1]);
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
                ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(lstIDs);
                SetCPReresh();
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
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
                                LstTrialData[i].ExtentData = DateTime.Now.ToString() + "|" + DemandCurrent + "|" + OutputCurrent + "|未停机";

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    if (AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage < 50)
                    {
                        ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);
                        ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    }
                }
                //开始检测流程
                if (testWorkParam.lstIDs.Count > 0)
                {

                    SendNoticeToUIAndTxtFile("检测充电桩充电状态。");
                    //闭合开关S2，启动充电
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }

                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        if (AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage < 50)
                        {
                            SendNoticeToUIAndTxtFile("未检测到" + testWorkParam.lstIDs[i] + "号枪处于充电状态，该枪停止检测");
                            testWorkParam.lstIDs.Remove(testWorkParam.lstIDs[i]);
                        }
                    }

                    //无充电桩在测试，直接退出 
                    if (testWorkParam.lstIDs.Count <= 0)
                    {
                        return;
                    }

                    Thread.Sleep(5000);

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

                    //ControlEquipMent.ResistanceLoad.SetResisLoadVolCurr(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage,DemandCurrent); 

                    SetResisLoadVolCurrAndStart(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, DemandCurrent);
                    SendNoticeToUIAndTxtFile("已发送负载正常电流值：" + DemandCurrent + "A，等待负载稳定。");
                    Thread.Sleep(5000);

                    SetResisLoadVolCurrAndStart(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, OutputCurrent);
                    //ControlEquipMent.ResistanceLoad.SetResisLoadVolCurr(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, OutputCurrent);
                    SendNoticeToUIAndTxtFile("已发送负载过流值：" + OutputCurrent + "A，等待负载稳定。");
                    Thread.Sleep(1000 * 18);

                    //采集数据
                    Data_Tmp = new Dictionary<int, string>();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage.ToString("F2"));
                    }

                    ProcessDataTmp(Data_Tmp, "最大允许电流测试", "保护后桩输出电压(V)", "0", "30");

                    SendNoticeToUIAndTxtFile("关闭负载。重启交流源恢复桩故障");

                    ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);

                    Thread.Sleep(3000);
                    ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);
                }
            }
        }


        public override void ProcessData()
        {
            try
            {
                foreach (var item in testWorkParam.lstIDs)
                {
                    int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                    int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                    if (k < 0)
                        return;
                    LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;


                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    double volate = AllEquipStateData.DicBMS_AC_StateData[LstTrialData[k].ChargerId].PhaseA_Voltage;
                    string strResult = "未停机";
                    if (volate < 50)
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;
                        strResult = "已停机";
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                    }
                    //界面展示的数据项格式
                    //测试时间|BMS需求电压|输入电压|是否停机|测试结果     
                    LstTrialData[i].ExtentData = LstTrialData[k].SaveTime + "|" + DemandCurrent + "|" + OutputCurrent + "|" + strResult;
                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    SendTrialDataToUI(LstTrialData[k]);
                    //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                    SaveTrialData(LstTrialData[k]);
                }


            }

            catch (Exception ex)
            {
                SendException(ex);
            }
        }
    }
}
