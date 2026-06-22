using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SaiTer.ATE.InterFace;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 蓄电池电压与通信报文不符试验  研发测试项
    /// </summary>
    public class BatteryVoltNotMatchMessage : BusinessBase
    {
        public BatteryVoltNotMatchMessage(int trialType) { TrialType = trialType; }

        private double cldlxdc车辆动力蓄电池当前电池电压 = 190;
        private double dcdyckz电池电压参考值 = 400;

        public override void InitializeParams()
        {
            Init();
            //车辆动力蓄电池当前电池电压(V)=190|电池电压参考值(V)=400
            string[] strParams = TrialItem.ResultParams.Split('|');
            if (strParams.Length >= 2)
            {
                cldlxdc车辆动力蓄电池当前电池电压 = Convert.ToDouble(strParams[0].Split('=')[1]);
                dcdyckz电池电压参考值 = Convert.ToDouble(strParams[1].Split('=')[1]);
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
                //流程结束,恢复BMS电压
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(100);
                ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, 390);
                Thread.Sleep(100);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, MaxAllowChargeVoltage);  //设置BHM报文最高允许电压
                Thread.Sleep(100);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 390, MaxAllowChargeVoltage, 250); //设置BCP报文最高允许电压
                Thread.Sleep(100);
                // SetCPReresh();
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
                #region  ------  此部分代码保留,作用可忽略---------------
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


                if (testWorkParam.lstIDs.Count == 0)
                {
                    return;
                }
                #endregion

                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                PullCharger(testWorkParam.lstIDs);

                SendNoticeToUIAndTxtFile($"设置BMS参数,电池电压设置为{cldlxdc车辆动力蓄电池当前电池电压}V,并启动充电");
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage);
                Thread.Sleep(100);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, cldlxdc车辆动力蓄电池当前电池电压, LstChargerInfo[0].NominalVoltage, 250);
                Thread.Sleep(100);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, 250, false, LstChargerInfo[0].NominalVoltage);
                Thread.Sleep(100);
                ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                Thread.Sleep(100);
                InsertCharger(testWorkParam.lstIDs);

                SystemEvent.MessageInfo(true, "请刷卡充电...");
                int timeout = 200;
                while (timeout-- > 0)
                {
                    int state = ChangeBMSChargeStatus(AllEquipStateData.DicBMS_DC_StateData[1].ChargingState);
                    if (state >= 2)
                    {
                        SendNoticeToUIAndTxtFile($"到达充电配置阶段,设置电池电压为{dcdyckz电池电压参考值}V");
                        ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, dcdyckz电池电压参考值);
                        Thread.Sleep(100);
                        break;
                    }
                    if (timeout < 0)
                    {
                        SendNoticeToUIAndTxtFile("刷卡失败,测试结束");
                    }
                    Thread.Sleep(1000);
                }
                SystemEvent.MessageInfo(false, "");
                SetConditionValues();

                CountDownTimeInfo("请判断充电桩是已经停充并告警。\r\n注：勾选上为已停充并告警", 50, 2);


                Dictionary<int, string> dd = new Dictionary<int, string>();
                foreach (var itmp in testWorkParam.lstIDs)
                {
                    string voltage = cldlxdc车辆动力蓄电池当前电池电压.ToString();
                    dd.Add(itmp, voltage);
                }
                ProcessDataTmp(dd, "蓄电池电压与通信报文不符", "通信报文蓄电池电压", "-", "-");


                dd.Clear();
                foreach (var itmp in testWorkParam.lstIDs)
                {
                    string voltage = dcdyckz电池电压参考值.ToString();
                    dd.Add(itmp, voltage);
                }
                ProcessDataTmp(dd, "蓄电池电压与通信报文不符", "电池电压参考值", "-", "-");

                ProcessData();
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
                LstTrialData[k].TrialName = TrialItem.ItemName;
                LstTrialData[k].SchemeName = TrialItem.SchemeName;
                LstTrialData[k].SchemeID = TrialItem.SchemeID;
                LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                LstTrialData[k].ItemName = iIndex.ToString();
                string resultInfo;
                if (DicManualVerifyResult[item])
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Pass;
                    resultInfo = "已停充告警";
                }
                else
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Fail;
                    resultInfo = "未停充告警";
                }
                LstTrialData[k].PKID = LstChargerInfo[i].PKID;

                LstTrialData[k].ExtentData = "蓄电池电压与通信报文不符" + "|应停充告警|-|-|" + resultInfo;
                LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                SendTrialDataToUI(LstTrialData[k]);
                SaveTrialData(LstTrialData[k]);
            }
            iIndex++;
        }
    }
}
