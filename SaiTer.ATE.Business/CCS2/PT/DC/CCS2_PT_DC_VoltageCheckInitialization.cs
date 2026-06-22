using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 初始化阶段电压检查
    /// </summary>
    public class CCS2_PT_DC_VoltageCheckInitialization : BusinessBase
    {
        public CCS2_PT_DC_VoltageCheckInitialization(int type)
        {
            TrialType = type;
        }
        private int TestTime = 0;//测试时间
        double BMSDemandVolt = 0;//额定电压
        double ResiLoadCurrent = 0;//额定电流
        double ExceedBattery = 390;//超过的电压值
        int trlTimeOut_S = 0;


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
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                //发送试验结束刷新UI
                SendMessageEndThisTrial();
            }
        }

        private void StartItemFlow()
        {
            try
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
                    SetConditionValues();



                    SendNoticeToUIAndTxtFile("正在打开输出过压命令中...");

                    bool[] Ks = new bool[24];
                    Ks[0] = true;//DC+DC-控制
                    Ks[1] = true;//CC信号控制
                    Ks[2] = true;//CP信号控制
                    Ks[4] = true;//PE信号控制
                    Ks[14] = true;//输出过压
                    ControlEquipMent.BMS.BMSSetKState_EU_DC(lstIDs, ExceedBattery, Ks.ToArray(), 0, 0, "0");



                    WaitSwipingCard(testWorkParam.lstIDs, 3);


                    CountDownTimeInfo("等待到达充电中", 100, 0);


                    ProcessDataChargeState();


                    SendNoticeToUIAndTxtFile("恢复互操作中...");
                    SetCPRersh_EUDC();





                }
            }
            catch (Exception ex) { SendException(ex); }


        }


        public override void InitEquiMent()
        {
            SetCPRersh_EUDCALL();
        }

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            ResiLoadCurrent = LstChargerInfo[0].NominalCurrent;

            if (strParams.Length >= 1)
            {
                if (strParams[0].Split('=').Length > 1)
                {
                    ExceedBattery = Convert.ToDouble(strParams[0].Split('=')[1]);
                }

            }
        }

        public override void ProcessData()
        {

        }


        public void ProcessDataChargeState()
        {
            try
            {
                foreach (var item in testWorkParam.lstIDs)
                {
                    int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                    int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                    if (k < 0)
                        return;
                    LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;

                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    string ChargeState = AllEquipStateData.DicBMS_EU_DC_StateData[item].SystemState;
                    if (ChargeState == "CurrentDemandReq" || ChargeState == "CurrentDemandRes")
                    {

                        LstTrialData[k].TrialResult = EmTrialResult.Fail;

                        LstTrialData[i].ExtentData = "电池过压" + "|充电状态|-|-|" + ChargeState;
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;

                        LstTrialData[i].ExtentData = "电池过压" + "|充电状态|-|-|" + ChargeState;
                    }








                    //界面展示的数据项格式   

                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    SendTrialDataToUI(LstTrialData[k]);
                    //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                    SaveTrialData(LstTrialData[k]);
                }


            }

            catch (Exception ex)
            {
                SendException(ex);
            }
        }



        public double CompareMaximum(double value, double maxvalue)
        {
            try
            {
                value = value >= maxvalue ? maxvalue : value;
                return value;
            }
            catch
            {
                return 0;
            }
        }

    }
}
