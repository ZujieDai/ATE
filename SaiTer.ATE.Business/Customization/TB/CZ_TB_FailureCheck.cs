using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 故障检查
    /// </summary>
    public class CZ_TB_FailureCheck : BusinessBase
    {
        float trlTimeOut_S = 100;//超时时间

        public CZ_TB_FailureCheck(int type) { TrialType = type; }


        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
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
                //SetCPRersh_EUDC();
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

                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);//打开交流源
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);//启动BMS
                }

                SendNoticeToUIAndTxtFile("启动充电");

                int BMSInfo = ChangeBMSChargeStatus_EU_DC(AllEquipStateData.DicBMS_EU_DC_StateData[testWorkParam.lstIDs[0]].SystemState);
                if (BMSInfo < 20 || BMSInfo > 23)
                {
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                }

                d1 = new Dictionary<int, string>();
                d2 = new Dictionary<int, string>();
                foreach (int item in testWorkParam.lstIDs)
                {
                    d1.Add(item, AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt.ToString("F2"));
                }
                ProcessDataTmp(d1, "充电过程", "输出电压(V)", "-", "-");

                //设置测试条件
                SetConditionValues();

                string info1 = "", info2 = "", info3 = "", sState = "";
                info1 = "请按下急停按钮";
                info2 = "请按下充电桩急停按钮,然后点击确认或倒计时结束后自动判断";
                info3 = "请恢复急停按钮";
                sState = "急停功能检查";

                SendNoticeToUIAndTxtFile(info1);
                CountDownTimeInfo(info2, 60, 0);
                Thread.Sleep(5000);


                Dictionary<int, string> dicData = new Dictionary<int, string>();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    double voltage = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[i]].Channel4RMSVolt;
                    dicData.Add(testWorkParam.lstIDs[i], voltage.ToString("F2"));
                }
                ProcessDataTmp(dicData, sState, "直流输出电压值(V)", "0", "20");

                CountDownTimeInfo("请人工判断充电桩的灯语是否显示红灯闪烁。\r\n（勾选枪号为Pass）", 60, 2);
                if (DicManualVerifyResult.First().Value)
                    ProcessDataResult(testWorkParam.lstIDs, "合格", "急停灯语", true);
                else
                    ProcessDataResult(testWorkParam.lstIDs, "不合格", "急停灯语", false);

                CountDownTimeInfo(info3, 60, 0);

                //急停后需要重新启动交流源
                SendNoticeToUIAndTxtFile("开始重新启动交流源恢复故障");
                ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);
                //这个时间不能缩短
                Thread.Sleep(7000);
                ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);

                //SetCPRersh_EUDC();
                CheckSwipingCard(testWorkParam.lstIDs);

                dicData = new Dictionary<int, string>();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    double voltage = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[i]].Channel4RMSVolt;
                    dicData.Add(testWorkParam.lstIDs[i], voltage.ToString("F2"));
                }
                ProcessDataTmp(dicData, "急停恢复上电", "直流输出电压值(V)", "60", "-");
            }

        }

        public override void ProcessData()
        {
        }
    }
}
