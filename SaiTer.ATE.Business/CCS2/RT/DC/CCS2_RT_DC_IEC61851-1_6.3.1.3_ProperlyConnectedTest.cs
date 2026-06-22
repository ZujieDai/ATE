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
    /// 欧标研测直流：验证EV是否正确连接到EV充电设备
    /// </summary>
    public class CCS2_RT_DC_ProperlyConnectedTest : BusinessBase
    {
        public CCS2_RT_DC_ProperlyConnectedTest(int type)
        {
            TrialType = type;
        }
        public override void InitializeParams()
        {

        }
        public override void InitEquiMent()
        {

        }
        public override void ExecuteMethod()
        {
            try
            {
                Init();
                SetCPRersh_EUDCALL();
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
                SetLoadDCOFF(lstIDs);
                ControlEquipMent.BMS.BMS_OFF(lstIDs);

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

                                LstTrialData[i].ExtentData = "null|null|null|null|null";

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }
                Thread.Sleep(1000);
                //设置测试条件
                SetConditionValues();


                if (!CheckSwipingCard(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, LstChargerInfo[0].NominalCurrent))
                {
                    return;
                }
                //等待电压稳定
                Thread.Sleep(2000);

                SendNoticeToUIAndTxtFile("开启负载并机");
                ControlEquipMent.FeedbackLoad?.FeedbackLoad_Parallel(testWorkParam.lstIDs);

                double loadCurrent = LstChargerInfo[0].NominalCurrent;
                if(ControlEquipMent.FeedbackLoad != null || ControlEquipMent.LoopFeedbackLoad != null || ControlEquipMent.StarLoopFeedbackLoad != null)
                    loadCurrent = LstChargerInfo[0].NominalCurrent - 5;
                SetLoadPara(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage - 20, LstChargerInfo[0].NominalCurrent - 5, LstChargerInfo[0].NominalVoltage, LstChargerInfo[0].NominalCurrent);
                Thread.Sleep(300);
                SetLoadDCON(testWorkParam.lstIDs);
                SendNoticeToUIAndTxtFile("已发送负载正常电流值：" + loadCurrent + "A，等待负载稳定。");
                WaitDCCurrent(testWorkParam.lstIDs, loadCurrent);
                Thread.Sleep(3000);

                d1 = new Dictionary<int, string>();
                foreach (int item in testWorkParam.lstIDs)
                {
                    d1.Add(item, AllEquipStateData.DicBMS_EU_DC_StateData[item].ChargingVoltage.ToString());
                }
                ProcessDataTmp(d1, "验证充电连接", "充电电压(V)", (LstChargerInfo[0].NominalVoltage * 0.9).ToString(), (LstChargerInfo[0].NominalVoltage * 1.1).ToString());
                Thread.Sleep(200);
                d1 = new Dictionary<int, string>();
                foreach (int item in testWorkParam.lstIDs)
                {
                    d1.Add(item, AllEquipStateData.DicBMS_EU_DC_StateData[item].ChargingCurrent.ToString());
                }
                ProcessDataTmp(d1, "验证充电连接", "充电电流(A)", (loadCurrent * 0.9).ToString(), (loadCurrent * 1.1).ToString());
                Thread.Sleep(200);
                d1 = new Dictionary<int, string>();
                foreach (int item in testWorkParam.lstIDs)
                {
                    d1.Add(item, AllEquipStateData.DicBMS_EU_DC_StateData[item].CPVoltage.ToString());
                }
                ProcessDataTmp(d1, "验证充电连接", "CP电压(V)", "5.47", "6.53");

                SendNoticeToUIAndTxtFile("取消负载并机");
                ControlEquipMent.FeedbackLoad?.FeedbackLoad_NoParallel(testWorkParam.lstIDs);
                ControlEquipMent.ResistanceLoad?.ResistanceLoad_NoParallel(testWorkParam.lstIDs);

            }
        }
        public override void ProcessData()
        {

        }
    }
}
