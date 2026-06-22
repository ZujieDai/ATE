using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 国标研测交流：输入功能试验
    /// </summary>
    public class GB_RT_AC_InputFunctional : BusinessBase
    {
        private string ItemFlow = "";
        private string VoltageMinValue, VoltageMaxValue;
        private double InputVoltage;
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据

        public GB_RT_AC_InputFunctional(int trialType) { TrialType = trialType; }
        public override void InitializeParams()
        {
            Init();
            Data_Tmp = new Dictionary<int, string>();//临时测试数据
            string[] strParams = TrialItem.ResultParams.Split('|');
            InputVoltage = double.Parse(strParams[0].Split('=')[1]);
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
                //设置测试条件
                SetConditionValues();


                CountDownTimeInfo("请确认桩具备手工输入功能\r\n注：勾选上为具备", 300, 2);
                foreach (var item in testWorkParam.lstIDs)
                {
                    if (!DicManualVerifyResult[item])
                    {
                        ProcessDataResult(testWorkParam.lstIDs, "不具备", "是否具备手工输入功能", null, "输入功能试验");
                        return;
                    }
                }

                //ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                //System.Threading.Thread.Sleep(500);

                // 手动闭合
                List<bool> Ks = GetKStatus16_Charging();
                ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);

                CountDownTimeInfo($"请手动设置充电机需求电压{InputVoltage}V并启动充电", 300, 0);
                Thread.Sleep(3000);

                SetResisLoadVolCurrAndStart(testWorkParam.lstIDs, InputVoltage, RatedCurrent);
                WaitACCurrent(testWorkParam.lstIDs, RatedCurrent);
                System.Threading.Thread.Sleep(5 * 1000);


                ItemFlow = "人工启动";
                VoltageMinValue = (InputVoltage * 0.9).ToString();
                VoltageMaxValue = (InputVoltage * 1.1).ToString();
                d1 = new Dictionary<int, string>();
                foreach (var item in testWorkParam.lstIDs)
                {
                    double voltage = AllEquipStateData.DicBMS_AC_StateData[item].PhaseA_Voltage;
                    d1.Add(item, voltage.ToString("F2"));
                }
                ProcessDataTmp(d1, ItemFlow, "输出电压(V)", VoltageMinValue, VoltageMaxValue);

                //断线后的电压
                SetLoadDCOFF(testWorkParam.lstIDs);
                System.Threading.Thread.Sleep(5 * 1000);

                CountDownTimeInfo("请手动停止充电", 300, 0);
                SendNoticeToUIAndTxtFile("等待充电结束...");
                System.Threading.Thread.Sleep(5 * 1000);

                ItemFlow = "人工停止";
                VoltageMinValue = "0";
                VoltageMaxValue = "20";
                d1 = new Dictionary<int, string>();
                foreach (var item in testWorkParam.lstIDs)
                {
                    double voltage = AllEquipStateData.DicBMS_AC_StateData[item].PhaseA_Voltage;
                    d1.Add(item, voltage.ToString("F2"));
                }
                ProcessDataTmp(d1, ItemFlow, "输出电压(V)", VoltageMinValue, VoltageMaxValue);

                Ks = GetKStatus16_Charging();
                Ks[0] = false;
                ControlEquipMent.BMS.BMS_SetKState(lstIDs, Ks);
            }
        }

        public override void ProcessData()
        {
        }
    }
}
