using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Configuration;


namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 接触器粘连测试 继电器控制零、火线黏连
    /// </summary>
    public class ContactorAdhesionTest_LN : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        private float trlTimeOut_S = 100;//超时时间

        private string ItemFlow = "";//流程步骤

        int index = 0;//需要控制的继电器索引号
        int StartChargerTime = 3;

        private List<bool> listK = new List<bool>() { true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };//程控板继电器
        public ContactorAdhesionTest_LN(int type)
        {
            TrialType = type;
        }

        public override void InitializeParams()
        {
            Init();

            string[] strParams = TrialItem.ItemParams.Split('|');
            if (strParams[0].Split('=').Count() != 1)
            {
                index = Convert.ToInt32(strParams[0].Split('=')[1].Trim('\r')) - 1;
            }
            string[] resParams = TrialItem.ResultParams.Split('|');
            //粘连后启动充电等待时间(s)=3
            if (resParams[0].Split('=').Count() != 1)
            {
                StartChargerTime = Convert.ToInt32(double.Parse(resParams[0].Split('=')[1].Trim('\r')));
            }
        }
        public override void InitEquiMent()
        {
            ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);
            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(testWorkParam.lstIDs);
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
                ControlEquipMent.ACSource.ACSource_OFF(lstIDs);
                Thread.Sleep(1500);
                SendNoticeToUIAndTxtFile("接触器L、N粘连恢复");
                listK[index] = false;
                ControlEquipMent.ControlBoard.ControlResistanceSetRelay(listK);
                Thread.Sleep(500);

                List<bool> Ks = GetKStatus16_Charging();
                // Ks[5] = false;
                Ks[0] = false;
                ControlEquipMent.BMS.BMS_SetKState(lstIDs, Ks);
                ControlEquipMent.ACSource.ACSource_ON(lstIDs);
                Thread.Sleep(1500);

                SetCPReresh();
                //保存试验结果               
                SaveTrialResult();
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

                SendNoticeToUIAndTxtFile("启动交流源，并等待输出稳定");

                ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(2000);
                //if (!CheckChargerIn(testWorkParam.lstIDs))
                //{
                //    return;
                //}
                //设置测试条件
                SetConditionValues();

                string info = "继电器L、N粘连测试";
                ItemFlow = "接触器L、N粘连";
                SendNoticeToUIAndTxtFile(info);

                listK = ControlEquipMent.ControlBoard.ControlBoardReadState();
                listK[index] = true;
                ControlEquipMent.ControlBoard.ControlResistanceSetRelay(listK);
                Thread.Sleep(2000);

                SendNoticeToUIAndTxtFile("启动交流源");
                ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);

                SendNoticeToUIAndTxtFile($"等待充电桩自检{StartChargerTime}秒后启动充电");
                Thread.Sleep(StartChargerTime * 1000);

                //提示刷卡
                var Ks = GetKStatus16_Charging();
                Ks[0] = true;
                ControlEquipMent.BMS.BMS_SetKState(lstIDs, Ks);
                WaitSwipingCard(testWorkParam.lstIDs, 2);

                //string systemState = AllEquipStateData.DicBMS_AC_StateData.FirstOrDefault().Value.SystemState;
                //if (systemState.Contains("充电中"))
                //    ProcessDataResult(testWorkParam.lstIDs, systemState, "系统状态", false, "接触器L、N粘连");
                //else
                //    ProcessDataResult(testWorkParam.lstIDs, systemState, "系统状态", true, "接触器L、N粘连");

                string Customer = ConfigurationManager.AppSettings["Customer"];
                if (Customer != null && Customer.Equals("HYQCP"))
                {
                    Data_Tmp.Clear();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        double volt = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage;
                        if (Data_Tmp.ContainsKey(testWorkParam.lstIDs[i]))
                            Data_Tmp[testWorkParam.lstIDs[i]] = volt.ToString();
                        else
                            Data_Tmp.Add(testWorkParam.lstIDs[i], volt.ToString());
                    }
                    ProcessDataTmp(Data_Tmp, "接触器L、N粘连", "充电电压(V)", "-", "-");
                    Data_Tmp.Clear();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        double volt = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPVoltage;
                        if (Data_Tmp.ContainsKey(testWorkParam.lstIDs[i]))
                            Data_Tmp[testWorkParam.lstIDs[i]] = volt.ToString();
                        else
                            Data_Tmp.Add(testWorkParam.lstIDs[i], volt.ToString());
                    }
                    ProcessDataTmp(Data_Tmp, "接触器L、N粘连", "CP电压(V)", "-", "-");
                    Data_Tmp.Clear();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        double duty = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPDutyCycle;
                        if (Data_Tmp.ContainsKey(testWorkParam.lstIDs[i]))
                            Data_Tmp[testWorkParam.lstIDs[i]] = duty.ToString();
                        else
                            Data_Tmp.Add(testWorkParam.lstIDs[i], duty.ToString());
                    }
                    ProcessDataTmp(Data_Tmp, "接触器L、N粘连", "CP占空比(%)", "-", "-");
                    Data_Tmp.Clear();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        double freq = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPFrequency;
                        if (Data_Tmp.ContainsKey(testWorkParam.lstIDs[i]))
                            Data_Tmp[testWorkParam.lstIDs[i]] = freq.ToString();
                        else
                            Data_Tmp.Add(testWorkParam.lstIDs[i], freq.ToString());
                    }
                    ProcessDataTmp(Data_Tmp, "接触器L、N粘连", "CP频率(Hz)", "-", "-");
                }

                CountDownTimeInfo(info + "请人工判断结果(有火线、零线粘连或者粘连故障则PASS)", 100, 2);
                ProcessData();

                if (!string.IsNullOrEmpty(Customer) && Customer.Equals("NT"))
                {
                    Thread.Sleep(1500);
                    ControlEquipMent.ACSource.ACSource_OFF(lstIDs);
                    Thread.Sleep(1500);
                    SendNoticeToUIAndTxtFile("接触器L、N粘连恢复");
                    listK[index] = false;
                    ControlEquipMent.ControlBoard.ControlResistanceSetRelay(listK);
                    Thread.Sleep(500);

                    SetCPReresh();
                    Thread.Sleep(1500);

                    info = "继电器L粘连测试";
                    ItemFlow = "接触器L粘连";

                    ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(1500);
                    CountDownTimeInfo(info + "请人工模拟有火线粘连。", 999, 0);
                    ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);
                    Thread.Sleep(1500);

                    Ks = GetKStatus16_Charging();
                    Ks[0] = true;
                    ControlEquipMent.BMS.BMS_SetKState(lstIDs, Ks);
                    WaitSwipingCard(testWorkParam.lstIDs, 2);
                    SendNoticeToUIAndTxtFile(info);
                    CountDownTimeInfo(info + "请人工判断结果(有火线粘连或者粘连故障则PASS)", 100, 2);
                    if (DicManualVerifyResult.First().Value)
                        ProcessDataResult(testWorkParam.lstIDs, "-", ItemFlow, true);
                    else
                        ProcessDataResult(testWorkParam.lstIDs, "-", ItemFlow, false);

                    SetCPReresh();
                    Thread.Sleep(1500);

                    info = "继电器N粘连测试";
                    ItemFlow = "接触器N粘连";

                    ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(1500);
                    CountDownTimeInfo(info + "请人工模拟有零线粘连。", 999, 0);
                    ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);
                    Thread.Sleep(1500);

                    Ks = GetKStatus16_Charging();
                    Ks[0] = true;
                    ControlEquipMent.BMS.BMS_SetKState(lstIDs, Ks);
                    WaitSwipingCard(testWorkParam.lstIDs, 2);
                    SendNoticeToUIAndTxtFile(info);
                    CountDownTimeInfo(info + "请人工判断结果(有零线粘连或者粘连故障则PASS)", 100, 2);
                    if (DicManualVerifyResult.First().Value)
                        ProcessDataResult(testWorkParam.lstIDs, "-", ItemFlow, true);
                    else
                        ProcessDataResult(testWorkParam.lstIDs, "-", ItemFlow, false);
                }

                //ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);
                //Thread.Sleep(1000);
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

                LstTrialData[k].ItemName = iIndex.ToString();
                LstTrialData[k].TrialName = TrialItem.ItemName;
                LstTrialData[k].SchemeName = TrialItem.SchemeName;
                LstTrialData[k].SchemeID = TrialItem.SchemeID;
                LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                double volate = AllEquipStateData.DicBMS_AC_StateData[LstTrialData[k].ChargerId].PhaseA_Voltage;
                if (DicManualVerifyResult[item])
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Pass;
                }
                else
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Fail;
                }
                LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                //界面展示的数据项格式
                //状态|测试结果     
                LstTrialData[k].ExtentData = TrialItem.ItemName + "|" + ItemFlow + "|-|-|-";
                LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                SendTrialDataToUI(LstTrialData[k]);
                //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                SaveTrialData(LstTrialData[k]);
                iIndex++;
            }

        }
    }
}
