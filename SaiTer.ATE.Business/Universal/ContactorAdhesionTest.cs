using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.InterFace;
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
    /// 接触器粘连测试
    /// </summary>
    public class ContactorAdhesionTest : BusinessBase
    {
        float trlTimeOut_S = 100;//超时时间
        public ContactorAdhesionTest(int type) { TrialType = type; }

        public override void InitializeParams()
        {
            Init();
        }
        public override void InitEquiMent()
        {
            ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);
            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            ControlEquipMent.ResistanceLoad?.ResistanceLoad_OFF(testWorkParam.lstIDs);
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
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                string info = "请短接接触器主触点为常闭状态(无法断开)!";
                SendNoticeToUIAndTxtFile(info);

                CountDownTimeInfo(info, 999, 0);

                //操作太快交流源可能才关闭，来不及启动
                Thread.Sleep(2000);
                SendNoticeToUIAndTxtFile("启动交流源，并等待输出稳定");               
                ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);
                Thread.Sleep(5000);

                PullCharger(testWorkParam.lstIDs);
                Thread.Sleep(200);
                ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                Thread.Sleep(200);
                InsertCharger(testWorkParam.lstIDs);

                //if (!CheckSwipingCard(testWorkParam.lstIDs))
                //{
                //    return;
                //}
                double insulationVolt = 0;
                int timeout = 600;
                MessgaeInfo(true, "请刷卡充电!", true);
                while (timeout-- > 0)
                {
                    var bmsData = AllEquipStateData.DicBMS_DC_StateData.Where(c => testWorkParam.lstIDs.Contains(c.Value.ChargerID)).ToDictionary(bms => bms.Key, bms => bms.Value);
                    if (bmsData.Count() < 1 || bmsData.Values.FirstOrDefault() == null)
                        continue;

                    int bmsState = ChangeBMSChargeStatus(bmsData.First().Value.ChargingState);
                    //CRO-AA阶段前绝缘电压，取最大值
                    if (bmsState <= 5)
                    {
                        double newInsulationVolt = AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo.First().ChargerId].ChargingVoltage;
                        insulationVolt = newInsulationVolt > insulationVolt ? newInsulationVolt : insulationVolt;
                    }
                    bool ALLCanCharge = bmsData.All(c => ChangeBMSChargeStatus(c.Value.ChargingState) > 3 && ChangeBMSChargeStatus(c.Value.ChargingState) < 9);
                    if (ALLCanCharge)
                    {
                        break;
                    }

                    System.Threading.Thread.Sleep(100);
                }
                MessgaeInfo(false, "请刷卡充电!");
                d1 = new Dictionary<int, string>();
                foreach (var item in testWorkParam.lstIDs)
                {
                    d1.Add(item, insulationVolt.ToString());
                }
                ProcessDataTmp(d1, "接触器粘连", "绝缘电压(V)", "-", "-");
                Thread.Sleep(3000);

                //设置测试条件
                SetConditionValues();

                info = "请人工检查充电机应停止绝缘检测过程!\r\n注：枪号勾选上为PASS，否则为FAIL";
                SendNoticeToUIAndTxtFile(info);
                CountDownTimeInfo(info, 999, 2);

                //开始判断数据
                ProcessData();

                CountDownTimeInfo("请人工检查充电桩是否有故障报警!\r\n注：勾选上为有故障报警", 999, 2);
                ProcessDataConnect("应发出告警提示", "是否有告警提示");

                CountDownTimeInfo("请恢复接触器主触点", 999, 0);

                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            }

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
                    //状态|测试结果     
                    LstTrialData[k].ExtentData = "接触器粘连|充电机应停止绝缘检测过程|-|-|是";
                }
                else
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Fail;
                    //状态|测试结果     
                    LstTrialData[k].ExtentData = "接触器粘连|充电机应停止绝缘检测过程|-|-|否";
                }
                LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                //界面展示的数据项格式
                LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                SendTrialDataToUI(LstTrialData[k]);
                //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                SaveTrialData(LstTrialData[k]);
            }

        }
    }
}
