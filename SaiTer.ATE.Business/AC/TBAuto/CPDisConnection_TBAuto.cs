using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.Remoting.Channels;
using SaiTer.ATE.InterFace;
using SaiTer.ATE.MES;
using Newtonsoft.Json;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// CP断线测试
    /// </summary>
    public class CPDisConnection_TBAuto : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        bool IsCardCharg = false;   //是否为刷卡充电


        public CPDisConnection_TBAuto(int trialType) { TrialType = trialType; }

        public override void InitializeParams()
        {
            Init();

            string[] strParams = TrialItem.ResultParams.Split('|');
            if (strParams.Length > 0)
                IsCardCharg = double.Parse(strParams[0].Split('=')[1]) == 1;

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
                // TB欧标桩需要刷卡才能结束充电，并且等待CP波纹和充电电压为0
                //if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_EUR_AC && IsCardCharg)
                //{
                //    CountDownTimeInfo("请刷卡终止充电，并等待充电桩结算后点击确认", 999, 0);
                //    int i = 500;
                //    while (i-- > 0)
                //    {
                //        if (AllEquipStateData.DicBMS_AC_StateData[lstIDs[0]].CPDutyCycle == 0 && AllEquipStateData.DicBMS_AC_StateData[lstIDs[0]].PhaseA_Voltage < 20)
                //        {
                //            //双重判断
                //            Thread.Sleep(100);
                //            if (AllEquipStateData.DicBMS_AC_StateData[lstIDs[0]].CPDutyCycle == 0 && AllEquipStateData.DicBMS_AC_StateData[lstIDs[0]].PhaseA_Voltage < 20)
                //                break;
                //        }
                //        Thread.Sleep(100);
                //    }
                //}
                //SetCPReresh();
                //List<bool> Ks = GetKStatus16_Charging();
                //// Ks[5] = false;
                //Ks[0] = false;
                //ControlEquipMent.BMS.BMS_SetKState(lstIDs, Ks);
                //Thread.Sleep(1000);
                //ControlEquipMent.BMS.BMS_SetKState(lstIDs, Ks);

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

                //自动化产线提示刷卡
                //if (isAutoTest)
                //{
                //    string json = JsonConvert.SerializeObject(new
                //    {
                //        MasterMaterialBarCode = TBHttpMESAutomation.GetInstance().MasterMaterialBarCode,
                //        Data = new
                //        {
                //            ActionType = "3",
                //            Action = "1"
                //        }
                //    });
                //    string res = HttpBase.HttpPost(TBHttpMESAutomation.GetInstance().PostUrl + "autoline/action", json);
                //    //MessageBox.Show(res);
                //    SystemEvent.SendLogMessage("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 已发送开始刷卡动作");
                //}
                if (AllEquipStateData.DicBMS_AC_StateData[lstIDs[0]].CPDutyCycle == 0 && AllEquipStateData.DicBMS_AC_StateData[lstIDs[0]].PhaseA_Voltage < 20)
                {
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                }

                //自动化产线提示刷卡
                //if (isAutoTest)
                //{
                //    string json = JsonConvert.SerializeObject(new
                //    {
                //        MasterMaterialBarCode = TBHttpMESAutomation.GetInstance().MasterMaterialBarCode,
                //        Data = new
                //        {
                //            ActionType = "3",
                //            Action = "1"
                //        }
                //    });
                //    string res = HttpBase.HttpPost(TBHttpMESAutomation.GetInstance().PostUrl + "autoline/action", json);
                //    //MessageBox.Show(res);
                //    SystemEvent.SendLogMessage("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 已发送停止刷卡动作");
                //}

                SetConditionValues();
                SendNoticeToUIAndTxtFile("模拟导引CP断线");

                //模拟CP断线
                List<bool> Ks = GetKStatus16_Charging();
                Ks[3] = false;
                ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);

                //Thread.Sleep(3500);
                int timeOut = 12;
                while (timeOut > 0)
                {
                    if (AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[0]].CPDutyCycle == 0
                        && AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[0]].PhaseA_Voltage > 0
                        && AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[0]].PhaseA_Voltage < 20)
                    {
                        break;
                    }
                    Thread.Sleep(300);
                    timeOut--;
                }
                //断线后的电压
                Data_Tmp = new Dictionary<int, string>();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage.ToString());
                }
                ProcessDataTmp(Data_Tmp, "保护接地", "输出电压(V)", "0", "20");
            }
        }



        public override void ProcessData()
        {

        }


    }
}
