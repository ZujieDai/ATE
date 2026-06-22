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
    /// 国标交流 输入功能测试
    /// </summary>
    public class InputFunctional_GB_AC : BusinessBase
    {
        private string ItemFlow = "";
        private string VoltageMinValue, VoltageMaxValue;
        private double InputVoltage;
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据

        public InputFunctional_GB_AC(int trialType) { TrialType = trialType; }
        public override void InitializeParams()
        {
            Init();
            Data_Tmp = new Dictionary<int, string>();//临时测试数据
            string[] strParams = TrialItem.ResultParams.Split('|');
            //InputVoltage = double.Parse(strParams[0].Split('=')[1]);
        }

        public override void InitEquiMent()
        {
            //ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, MaxAllowChargeVoltage, 250, true, 390);
            //SetCPReresh();
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
                SetCPReresh();

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

                CountDownTimeInfo("请确认桩具备手工输入功能", 300, 2);
                foreach (var item in testWorkParam.lstIDs)
                {
                    if (!DicManualVerifyResult[item])
                    {
                        ProcessDataResult(testWorkParam.lstIDs, "不具备", "是否具备手工输入功能", false, "输入功能试验");
                        return;
                    }
                }

                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(500);
                //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                //Thread.Sleep(500);
                //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, InputVoltage, 0, true, InputVoltage);
                //Thread.Sleep(500);
                //断线后的电压
                //var list = GetKStatus16_Charging_DC();
                //list[27] = false;//开关S
                //ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, list.ToArray());
                CountDownTimeInfo($"请手动控制充电机启动充电", 300, 0);
                //WaitDCVoltage(testWorkParam.lstIDs, InputVoltage);
                Thread.Sleep(5 * 1000);

                //设置测试条件
                SetConditionValues();

                ItemFlow = "人工启动";
                Data_Tmp.Clear();
                foreach (var itmp in testWorkParam.lstIDs)
                {
                    double volt = AllEquipStateData.DicBMS_AC_StateData[itmp].PhaseA_Voltage;
                    volt = AllEquipStateData.DicBMS_AC_StateData[itmp].PhaseA_Voltage;
                    Data_Tmp.Add(itmp, volt.ToString());
                }
                ProcessDataTmp(Data_Tmp, "人工启动后", "输出电压(V)", "50", "-");

                //断线后的电压
                CountDownTimeInfo("请手动停止充电", 300, 0);
                Data_Tmp.Clear();
                foreach (var itmp in testWorkParam.lstIDs)
                {
                    double volt = AllEquipStateData.DicBMS_AC_StateData[itmp].PhaseA_Voltage;
                    volt = AllEquipStateData.DicBMS_AC_StateData[itmp].PhaseA_Voltage;
                    Data_Tmp.Add(itmp, volt.ToString());
                }
                ProcessDataTmp(Data_Tmp, "人工停止后", "输出电压(V)", "-", "20");
            }
        }

        public override void ProcessData()
        {
            foreach (var item in testWorkParam.lstIDs)
            {
                if (!DicManualVerifyResult[item])
                    continue;

                int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                if (k < 0)
                    return;
                LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;
                LstTrialData[k].ItemName = ItemFlow;
                LstTrialData[k].TrialName = TrialItem.ItemName;
                LstTrialData[k].SchemeName = TrialItem.SchemeName;
                LstTrialData[k].SchemeID = TrialItem.SchemeID;
                LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


                double voltage = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage;
                if (voltage >= Convert.ToDouble(VoltageMinValue))
                {
                    if (VoltageMaxValue != "-")
                    {
                        if (voltage <= Convert.ToDouble(VoltageMaxValue))
                            LstTrialData[k].TrialResult = EmTrialResult.Pass;
                        else
                            LstTrialData[k].TrialResult = EmTrialResult.Fail;
                    }
                    else
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;
                }
                else
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Fail;
                }
                LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                //界面展示的数据项格式
                //状态|测试结果     
                LstTrialData[k].ExtentData = ItemFlow + $"|输出电压(V)|{VoltageMinValue}|{VoltageMaxValue}|" + voltage;
                LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                SendTrialDataToUI(LstTrialData[k]);
                //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                SaveTrialData(LstTrialData[k]);
            }
        }
    }
}
