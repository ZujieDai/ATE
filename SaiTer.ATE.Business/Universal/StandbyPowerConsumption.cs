using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 待机功耗流程
    /// </summary>
    public class StandbyPowerConsumption : BusinessBase
    {
        public StandbyPowerConsumption(int trialType) { TrialType = trialType; }
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
                    else if (Customer != null && (Customer.Contains("YTZL_ACDC") || Customer.Equals("TS") || Customer.Equals("WR") || Customer.Equals("LJ")))
                    {

                        //设置变比
                        ControlEquipMent.PowerAnalyzer.SetChannelRatio(testWorkParam.lstIDs, 1, false, 1);//1通道
                        Thread.Sleep(200);
                        ControlEquipMent.PowerAnalyzer.SetChannelRatio(testWorkParam.lstIDs, 2, false, 1);//2通道
                        Thread.Sleep(200);
                        ControlEquipMent.PowerAnalyzer.SetChannelRatio(testWorkParam.lstIDs, 3, false, 1);//3通道
                        Thread.Sleep(200);
                    }
                    else if(Customer != null && Customer.Equals("LJ"))
                    {
                        Thread.Sleep(2000);
                        ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);
                        Thread.Sleep(2000);
                        ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);
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
                        double powerConsumptionMax = AllEquipStateData.DicPowerAnalyzer_StateData[itmp].TotalPower * 1000;
                        int timeout = 10;
                        while(timeout-- > 0)
                        {
                            if (powerConsumptionMax > 0 && powerConsumptionMax < PowerConsumptionMax)
                                break;
                            Thread.Sleep(1000); //功分仪刷新慢
                            powerConsumptionMax = AllEquipStateData.DicPowerAnalyzer_StateData[itmp].TotalPower * 1000;
                        }
                        dd.Add(itmp, powerConsumptionMax.ToString("F2"));
                    }

                    //System.Windows.Forms.MessageBox.Show("等待数据确认");

                    ProcessDataTmp(dd, "待机功耗", "实测功耗值(W)", "0", PowerConsumptionMax.ToString());

                    if (Customer != null && (Customer.Contains("HK")|| Customer.Contains("YTZL")))
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
                    if (Customer != null && (Customer.Contains("HK") || Customer.Contains("YTZL_ACDC") || Customer.Equals("TS") || Customer.Equals("WR") || Customer.Equals("LJ")))
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
