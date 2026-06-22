using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 待机功耗(源自南网企标，目前河南产测使用)
    /// </summary>
    public class GB_PT_DC_StandbyPowerConsumption_XJ_QB : BusinessBase
    {
        public GB_PT_DC_StandbyPowerConsumption_XJ_QB(int trialType) { TrialType = trialType; }
        private double PowerConsumptionMax;
        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            PowerConsumptionMax = Convert.ToDouble(strParams[0].Split('=')[1]);

        }
        public override void InitEquiMent()
        {
        }
        public override void ExecuteMethod()
        {
            string Customer = ConfigurationManager.AppSettings["Customer"];
            try
            {
                if (Customer != null && Customer.ToString().ToUpper().Equals("HYQCP"))
                {
                    ControlEquipMent.PowerAnalyzer.SetScalingState(lstIDs, 0);
                }
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
                // SetCPReresh();
                if (Customer != null && Customer.ToString().ToUpper().Equals("HYQCP"))
                {
                    ControlEquipMent.PowerAnalyzer.SetScalingState(lstIDs, 1);
                }
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

                try
                {

                    if (testWorkParam.lstIDs.Count == 0)
                    {
                        return;
                    }
                    // 待机功耗不应该刷卡
                    //if (!CheckSwipingCard(testWorkParam.lstIDs))
                    //{
                    //    return;
                    //}

                    string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();

                    if (Customer != null && Customer.Contains("HK"))
                    {
                        //切换程控板电压
                        var listState = ControlEquipMent.ControlBoard.ControlBoardReadState();
                        listState[8] = false;
                        ControlEquipMent.ControlBoard.ControlResistanceSetRelay(listState);
                        Thread.Sleep(500);

                        //设置变比
                        ControlEquipMent.PowerAnalyzer.SetChannelRatio(testWorkParam.lstIDs, 1, false, 20);//1通道
                        Thread.Sleep(200);
                        ControlEquipMent.PowerAnalyzer.SetChannelRatio(testWorkParam.lstIDs, 2, false, 20);//2通道
                        Thread.Sleep(200);
                        ControlEquipMent.PowerAnalyzer.SetChannelRatio(testWorkParam.lstIDs, 3, false, 20);//3通道
                        Thread.Sleep(200);
                    }
                    else if (Customer != null && (Customer.Contains("YTZL_ACDC") || Customer.Equals("TS") || Customer.Equals("WR")))
                    {

                        //设置变比
                        ControlEquipMent.PowerAnalyzer.SetChannelRatio(testWorkParam.lstIDs, 1, false, 1);//1通道
                        Thread.Sleep(200);
                        ControlEquipMent.PowerAnalyzer.SetChannelRatio(testWorkParam.lstIDs, 2, false, 1);//2通道
                        Thread.Sleep(200);
                        ControlEquipMent.PowerAnalyzer.SetChannelRatio(testWorkParam.lstIDs, 3, false, 1);//3通道
                        Thread.Sleep(200);
                    }

                    Thread.Sleep(2000);

                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(500);
                    ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);
                    Thread.Sleep(3000);
                    //设置测试条件
                    SetConditionValues();
                    Thread.Sleep(7000);


                    Dictionary<int, string> dd = new Dictionary<int, string>();
                    foreach (var itmp in testWorkParam.lstIDs)
                    {
                        string strPowerConsumptionMax = (AllEquipStateData.DicPowerAnalyzer_StateData[itmp].TotalPower * 1000).ToString("F2");
                        for (int i = 0; i < 5; i++)
                        {
                            if (Convert.ToDouble(strPowerConsumptionMax) < 0.1)
                            {
                                strPowerConsumptionMax = (AllEquipStateData.DicPowerAnalyzer_StateData[itmp].TotalPower * 1000).ToString("F2");
                            }
                            else
                            {
                                break;
                            }
                        }
                        dd.Add(itmp, strPowerConsumptionMax);
                    }

                    //System.Windows.Forms.MessageBox.Show("等待数据确认");

                    ProcessDataTmp(dd, "待机功耗", "实测功耗值(W)", "0", PowerConsumptionMax.ToString());

                    if (Customer != null && (Customer.Contains("HK") || Customer.Contains("YTZL")))
                    {
                        //恢复程控板设置
                        var listState = ControlEquipMent.ControlBoard.ControlBoardReadState();
                        listState[8] = true;
                        ControlEquipMent.ControlBoard.ControlResistanceSetRelay(listState);
                        Thread.Sleep(500);
                    }
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
                    if (Customer != null && (Customer.Contains("HK") || Customer.Contains("YTZL_ACDC") || Customer.Equals("TS") || Customer.Equals("WR")))
                    {
                        if (Customer.Contains("HK"))
                        {
                            //切换程控板电压
                            var listState = ControlEquipMent.ControlBoard.ControlBoardReadState();
                            listState[8] = true;
                            ControlEquipMent.ControlBoard.ControlResistanceSetRelay(listState);
                            Thread.Sleep(500);
                        }

                        //设置变比
                        ControlEquipMent.PowerAnalyzer.SetChannelRatio(testWorkParam.lstIDs, 1, false, 2000);//1通道
                        Thread.Sleep(200);
                        ControlEquipMent.PowerAnalyzer.SetChannelRatio(testWorkParam.lstIDs, 2, false, 2000);//2通道
                        Thread.Sleep(200);
                        ControlEquipMent.PowerAnalyzer.SetChannelRatio(testWorkParam.lstIDs, 3, false, 2000);//3通道
                        Thread.Sleep(200);
                    }
                }
            }
        }
        public override void ProcessData()
        {

        }
    }
}
