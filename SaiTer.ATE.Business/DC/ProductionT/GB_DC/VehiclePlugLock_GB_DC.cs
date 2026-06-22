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
    /// 车辆插头锁止功能测试
    /// </summary>
    public class VehiclePlugLock_GB_DC : BusinessBase
    {
        string ItemFlow = "";
        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;
        float trlTimeOut_S = 100;//超时时间
        public VehiclePlugLock_GB_DC(int type) { TrialType = type; }

        public override void InitializeParams()
        {
            Init();
            //string[] strParams = TrialItem.ResultParams.Split('|');
            //BMSDemandVolt = Convert.ToDouble(strParams[0].Split('=')[1]);
            //LoadCurrent = Convert.ToDouble(strParams[1].Split('=')[1]);
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            ResiLoadCurrent = LstChargerInfo[0].NominalCurrent;
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

                //保存试验结果               
                SaveTrialResult();
                SetCPReresh();
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
                //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, LstChargerInfo[0].NominalVoltage, LoadCurrent + 10);

                //ControlEquipMent.BMS.BMS_ON(lstIDs);

                if (!CheckSwipingCard(testWorkParam.lstIDs))
                {
                    return;
                }
                d1.Clear();
                d2.Clear();
                //设置测试条件
                SetConditionValues();

                ItemFlow = "正常充电状态";
                CountDownTimeInfo("请确认车辆插头电子锁应可靠锁止\r\n勾选上代表正常锁止", 20, 2);
                ProcessData();

                d1 = new Dictionary<int, string>();
                foreach (var item in testWorkParam.lstIDs)
                {
                    d1.Add(item, AllEquipStateData.DicBMS_DC_StateData[item].CC1Voltage.ToString());
                }
                ProcessDataTmp(d1, ItemFlow, "CC1(V)", "3.65", "4.37");



                ItemFlow = "模拟电子锁故障";
                CountDownTimeInfo("请模拟充电机故障停机，然后点击确认按钮", 100, 0);
                CountDownTimeInfo("请确认车辆插头电子锁解锁\r\n勾选上代表正常解锁", 20, 2);
                ProcessData();

                d1 = new Dictionary<int, string>();
                foreach(var item in testWorkParam.lstIDs)
                {
                    d1.Add(item, AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage.ToString());
                }
                ProcessDataTmp(d1, ItemFlow, "解锁时车辆接口电压应降至60VDC以下", "-", "60");
                CountDownTimeInfo("请恢复充电机故障，然后点击确认按钮", 100, 0);


                ItemFlow = "检查电子锁装置应具备应急解锁功能";
                CountDownTimeInfo("请检查电子锁装置是否具备应急解锁功能\r\n勾选上代表具备", 20, 2);
                ProcessDataConnect(ItemFlow, "是否具备");
            }

        }


        public override void ProcessData()
        {
            foreach (var item in DicManualVerifyResult)
            {

                int k = LstTrialData.FindIndex(s => s.ChargerId == item.Key);
                int i = LstChargerInfo.FindIndex(s => s.ChargerId == item.Key);
                if (k < 0)
                    return;
                LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;
                LstTrialData[k].TrialName = TrialItem.ItemName;
                LstTrialData[k].SchemeName = TrialItem.SchemeName;
                LstTrialData[k].ItemName = ItemFlow;
                LstTrialData[k].SchemeID = TrialItem.SchemeID;
                LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                double volate = AllEquipStateData.DicBMS_DC_StateData[LstTrialData[k].ChargerId].ChargingVoltage;

                if (item.Value)
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Pass;
                    if (ItemFlow == "正常充电状态")
                    {
                        LstTrialData[k].ExtentData = ItemFlow + "|是否锁止和加外力检查有效性\r\n|-|-|是";
                    }
                    else
                    {
                        LstTrialData[k].ExtentData = ItemFlow + "|是否允许充电\r\n|-|-|否";
                    }

                }
                else
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Fail;
                    if (ItemFlow == "正常充电状态")
                    {
                        LstTrialData[k].ExtentData = ItemFlow + "|是否锁止和加外力检查有效性\r\n|-|-|否";
                    }
                    else
                    {
                        LstTrialData[k].ExtentData = ItemFlow + "|是否允许充电\r\n|-|-|是";
                    }
                }
                LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                //界面展示的数据项格式
                //状态|测试结果     

                LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                SendTrialDataToUI(LstTrialData[k]);
                //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                SaveTrialData(LstTrialData[k]);


            }

        }

    }
}
