using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 冲击耐压试验（研测）
    /// </summary>
    public class ImpulseWithstandcsTest : BusinessBase
    {
        string ItemFlow;
        int trlTimeOut_S = 30;
        int TestTime = 120;
        int TipTime = 60;

        public ImpulseWithstandcsTest(int type)
        {
            TrialType = type;
        }

        public override void InitializeParams()
        {
            Init();

            // 检测时间(S)=120|提示时间(S)=60
            string[] strParams = TrialItem.ResultParams.Split('|');
            if (strParams.Length >= 2)
            {
                TestTime = int.Parse(strParams[0].Split('=')[1]);
                TipTime = int.Parse(strParams[1].Split('=')[1]);
            }
        }

        public override void InitEquiMent()
        {
            SendNoticeToUIAndTxtFile("设备初始化中...");

            // 模拟插拔枪
            SetCPReresh();
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
                //保存试验结果               
                SaveTrialResult();
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                //发送试验结束刷新UI
                SendMessageEndThisTrial();
            }
        }

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
                    if (_StopWatch.ElapsedMilliseconds / 1000 > trlTimeOut_S)
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
                                    //
                                    LstTrialData[i].ExtentData = "-|-|-|-|null";
                                    SendTrialDataToUI(LstTrialData[i]);
                                }
                            }
                        }
                        break;
                    }
                    if (testWorkParam.lstIDs.Count <= 0)
                    {
                        return;
                    }
                    //设置测试条件
                    SetConditionValues();

                    ImpulseTest("请断开充电桩输入线缆与测试设备的连接，确认与冲击耐压测试仪的连接, 符合【输入对地】的接线，打正极电压。", "输入对地正极");      // 输入对地正极

                    ImpulseTest("请断开充电桩输入线缆与测试设备的连接，确认与冲击耐压测试仪的连接, 符合【输入对地】的接线，打负极电压。", "输入对地负极");      // 输入对地负极

                    ImpulseTest("请断开充电桩输入线缆与测试设备的连接，确认与冲击耐压测试仪的连接, 符合【输出对地】的接线，打正极电压。", "输出对地正极");      // 输出对地正极

                    ImpulseTest("请断开充电桩输入线缆与测试设备的连接，确认与冲击耐压测试仪的连接, 符合【输出对地】的接线，打负极电压。", "输出对地负极");      // 输出对地负极

                    ImpulseTest("请断开充电桩输入线缆与测试设备的连接，确认与冲击耐压测试仪的连接, 符合【输入对输出】的接线，打正极电压。", "输入对输出正极");  // 输入对输出正极

                    ImpulseTest("请断开充电桩输入线缆与测试设备的连接，确认与冲击耐压测试仪的连接, 符合【输入对输出】的接线，打负极电压。", "输入对输出负极");  // 输入对输出负极

                    CountDownTimeInfo("如果不再测试安规测试,请连接好充电机电源输入电缆", TipTime, 1);
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        private void ImpulseTest(string tip1, string itemName)
        {
            SetLoadDCOFF(testWorkParam.lstIDs);
            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);
            System.Threading.Thread.Sleep(2000);

            CountDownTimeInfo(tip1, TipTime, 1);
            CountDownTimeInfo("请确认测试结果是否PASS！\r\n注：勾选上为PASS，否则为FAIL", TipTime, 2);
            ItemFlow = itemName;
            ProcessData();
        }

        public override void ProcessData()
        {
            foreach (var item in testWorkParam.lstIDs)
            {
                int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                if (k < 0)
                    return;
                LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;

                LstTrialData[k].ItemName = iIndex.ToString();
                LstTrialData[k].TrialName = TrialItem.ItemName;
                LstTrialData[k].SchemeName = TrialItem.SchemeName;
                LstTrialData[k].SchemeID = TrialItem.SchemeID;
                LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                if (DicManualVerifyResult[item])
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Pass;
                }
                else
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Fail;
                }
                LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                //界面展示的数据项格式
                //状态|测试结果     
                LstTrialData[k].ExtentData = TrialItem.ItemName + "|" + ItemFlow + "|-|-|-";
                LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                SendTrialDataToUI(LstTrialData[k]);
                //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                SaveTrialData(LstTrialData[k]);
            }
        }
    }
}
