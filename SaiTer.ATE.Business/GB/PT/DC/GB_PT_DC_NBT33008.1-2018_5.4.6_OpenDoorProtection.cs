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
    /// 国标产测直流：开门保护试验
    /// </summary>
    public class GB_PT_DC_OpenDoorProtection : BusinessBase
    {
        float trlTimeOut_S = 100;//超时时间

        public GB_PT_DC_OpenDoorProtection(int type) { TrialType = type; }


        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
        }

        public override void ExecuteMethod()
        {
            try
            {
                SetCPReresh();
                Init();
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

                string info1 = "", info2 = "", info3 = "", sState = "";
                info1 = "请模拟开门";
                info2 = "请模拟开门操作,然后点击确认或倒计时结束后自动判断";
                info3 = "请关门";
                sState = "充电前打开充电机门";

                // 充电前开门
                SendNoticeToUIAndTxtFile(info1);
                CountDownTimeInfo(info2, 60, 0);
                Thread.Sleep(5000);

                CountDownTimeInfo("请检查充电桩是否无法启动充电!\r\n注：勾选上为无法启动充电", 999, 2);
                //ProcessDataConnect("充电前打开充电机门", "是否无法启动充电");
                d1 = new Dictionary<int, string>();
                var dicResult = new Dictionary<int, EmTrialResult>();
                foreach (int item in testWorkParam.lstIDs)
                {
                    d1.Add(item, DicManualVerifyResult[item] ? "是" : "否");
                    dicResult.Add(item, DicManualVerifyResult[item] ? EmTrialResult.Pass : EmTrialResult.Fail);
                }
                ProcessDataResults(testWorkParam.lstIDs, d1, "是否无法启动充电", dicResult, "充电前打开充电机门");

                CountDownTimeInfo(info3, 60, 0);

                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                SetCPReresh();


                //充电中开门
                sState = "充电中打开充电机门";
                SendNoticeToUIAndTxtFile("启动充电");
                if (!CheckSwipingCard(testWorkParam.lstIDs))
                {
                    return;
                }
                Thread.Sleep(2000);

                d1 = new Dictionary<int, string>();
                foreach (int item in testWorkParam.lstIDs)
                {
                    d1.Add(item, AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage.ToString("F2"));
                }
                ProcessDataTmp(d1, "开门前充电中状态", "直流输出电压(V)", "-", "-");

                SendNoticeToUIAndTxtFile(info1);
                CountDownTimeInfo(info2, 60, 0);
                Thread.Sleep(5000);


                var dicData = new Dictionary<int, string>();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    string voltage = "0";
                    voltage = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage.ToString();
                    if (dicData.ContainsKey(testWorkParam.lstIDs[i]))
                    {
                        dicData[testWorkParam.lstIDs[i]] = voltage;
                    }
                    else
                    {
                        dicData.Add(testWorkParam.lstIDs[i], voltage);
                    }
                }
                ProcessDataTmp(dicData, sState, "直流输出电压(V)", "0", "20");
                CountDownTimeInfo(info3, 60, 0);

                //CountDownTimeInfo("请检查充电机是否切断动力电源输入。\r\n(注:勾选上为已切断)", 20, 2);
                //ProcessDataConnect(sState, "是否切断动力电源输入");

                CountDownTimeInfo("请人工确认充电过程中一体式充电机切断动力电源输入和输出，或分体式切断相应部分输入或输出!\r\n注：勾选上为PASS，否则为FAIL", 999, 2);
                //ProcessDataConnect(sState, "是否切断动力电源输入和输出");
                d1 = new Dictionary<int, string>();
                dicResult = new Dictionary<int, EmTrialResult>();
                foreach (int item in testWorkParam.lstIDs)
                {
                    d1.Add(item, DicManualVerifyResult[item] ? "是" : "否");
                    dicResult.Add(item, DicManualVerifyResult[item] ? EmTrialResult.Pass : EmTrialResult.Fail);
                }
                ProcessDataResults(testWorkParam.lstIDs, d1, "是否切断动力电源输入和输出", dicResult, sState);

            }

        }
        public override void ProcessData()
        {

        }
    }
}
