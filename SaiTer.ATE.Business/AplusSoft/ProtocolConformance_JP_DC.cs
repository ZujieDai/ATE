using SaiTer.ATE.DataModel.DataBaseModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.IDAL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    public class ProtocolConformance_JP_DC : BusinessBase
    {
        int trlTimeOut_S = 1600;
        public ProtocolConformance_JP_DC(int type)
        {
            TrialType = type;
        }

        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            Init();
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
                SetCPRersh_JPDC();
                //保存试验结果               
                SaveTrialResult();
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                //发送试验结束刷新UI
                SendMessageEndThisTrial();
            }
        }

        public void APlusApp(string tCommand)
        {
            var bmsPort = LstPortInfo.Find(p => p.EquiqMentClassName.IndexOf("emtBMS_JP_DC") > -1);   //交流BMS
            var wrbPort = LstPortInfo.Find(p => p.EquiqMentClassName == "emtWaveRecoderBoard"); //录波板
            string strWrb = Convert.ToString(ConfigurationManager.AppSettings["WaveRecordIP"]);
            bmsPort.Close();    //解决占用
            System.Threading.Thread.Sleep(200);    //等待端口断开
            try
            {
                bool openRes = false;
                //COM连接的BMS
                if (bmsPort.PortName.Contains("COM"))
                {
                    openRes = JP_DC_Soft.OpenAPlusAppInitialize(bmsPort.PortName, Convert.ToInt32(bmsPort.PortParams), strWrb);
                }
                //TCP连接的BMS
                if (bmsPort.PortParams.Contains("."))
                {
                    openRes = JP_DC_Soft.OpenAPlusAppInitialize(bmsPort.PortParams, Convert.ToInt32(bmsPort.PortName), strWrb);
                }
                //if (!AplusSoft.OpenAPlusAppInitialize(bmsPort.PortName, Convert.ToInt32(bmsPort.PortParams), $"{wrbPort?.PortParams}:{wrbPort?.PortName}"))
                if (!openRes)
                {
                    LstTrialData[0].TrialResult = EmTrialResult.Fail;
                    LstTrialData[0].ExtentData = "通讯连接失败";
                    SendTrialDataToUI(LstTrialData[0]);
                    SaveTrialData(LstTrialData[0]);
                    return;
                }
                System.Threading.Thread.Sleep(2000);

                if (tCommand.Contains("STCA2"))
                {
                    JP_DC_Soft.APlusClient.Send("INTEROPCADC,START," + tCommand.Replace(".", ""));
                }
                else
                {
                    JP_DC_Soft.APlusClient.Send("CONSISTCADC,START," + tCommand.Replace(".", ""));
                }

                int num = 0;
                while (true)
                {
                    string recCommand = ASCIIEncoding.UTF8.GetString(JP_DC_Soft.APlusClient.LstRcvBytes.ToArray());
                    if (tCommand.Contains("STCA2"))
                    {
                        if (recCommand == "INTEROPCADC,FINISH," + tCommand.Replace(".", ""))
                        {
                            recCommand = "";
                            break;
                        }
                    }
                    else
                    {
                        if (recCommand == "CONSISTCADC,FINISH," + tCommand.Replace(".", ""))
                        {
                            recCommand = "";
                            break;
                        }
                    }



                    if (recCommand.Contains("SETLOAD,ON"))
                    {
                        // System.Threading.Thread.Sleep(10000); //等待充电电压达到需求值

                        string[] stringS = recCommand.Split(',');
                        if (stringS.Length >= 5)
                        {
                            if (ControlEquipMent.ResistanceLoad != null)
                            {
                                ControlEquipMent.ResistanceLoad.SetResisLoadVolCurr(testWorkParam.lstIDs, Convert.ToDouble(stringS[3]), Convert.ToDouble(stringS[4]));
                                ControlEquipMent.ResistanceLoad.ResistanceLoad_ON(testWorkParam.lstIDs);
                            }
                            else
                            {
                                ControlEquipMent.FeedbackLoad.SetFeedbackLoadParams(testWorkParam.lstIDs, Convert.ToDouble(stringS[3]), Convert.ToDouble(stringS[4]));
                                SetLoadDCON(testWorkParam.lstIDs);
                                SetLoadDCON(testWorkParam.lstIDs);
                            }
                            //SetDCLoadVoltageCurrentON(Convert.ToDouble(stringS[3]), Convert.ToDouble(stringS[4]));
                        }
                        recCommand = "";

                    }

                    if (recCommand.Contains("SETLOAD,OFF"))
                    {

                        if (ControlEquipMent.ResistanceLoad != null)
                            ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(testWorkParam.lstIDs);
                        else
                            SetLoadDCOFF(testWorkParam.lstIDs);
                        //DCLoadOFF();
                        recCommand = "";
                    }


                    System.Threading.Thread.Sleep(1000); //()
                    if (++num > 1500)//
                    {
                        //超时25分钟
                        LstTrialData[0].TrialResult = EmTrialResult.Fail;
                        LstTrialData[0].ExtentData = "超时,测试失败";

                        return;
                    }
                }
                System.Threading.Thread.Sleep(5000); //()

                List<TrialDataModel> LstItemData = new List<TrialDataModel>();
                int i = LstChargerInfo.FindIndex(s => s.ChargerId == testWorkParam.lstIDs[0]);
                if (tCommand.Contains("STCA2"))
                {
                    LstTrialData[0].SchemeID = TrialItem.SchemeID;
                    LstTrialData[0].SchemeName = TrialItem.SchemeName;
                    LstTrialData[0].ItemName = TrialItem.ItemName;
                    APlusSQLiteHelper.CheckDataToTable_TestInterop(tCommand, LstTrialData);
                    LstItemData = APlusSQLiteHelper.CheckDataToTable_InteropItems(tCommand, LstTrialData, LstChargerInfo[i].BarCode, LstChargerInfo[i].PKID);
                }
                else
                {
                    LstItemData = APlusSQLiteHelper.CheckDataToTable_TestItemsReportCADC(tCommand.Replace(".", ""), TrialItem, LstChargerInfo[i].BarCode, LstChargerInfo[i].PKID);

                }

                //LstTrialData[0].TrialResult = EmTrialResult.Pass;
                foreach (var item in LstItemData)
                {
                    //if(item.TrialResult == EmTrialResult.Fail)
                    //    LstTrialData[0].TrialResult = EmTrialResult.Fail;
                    item.IsCheck = LstTrialData[0].IsCheck;
                    item.ChargerId = LstTrialData[0].ChargerId;
                    item.TrialCondition = LstTrialData[0].TrialCondition;
                    item.Data3 = TrialItem.TrialOrder.ToString();
                    SendTrialDataToUI(item);
                    SaveTrialData(item);
                }
                LstTrialData[0] = LstItemData[0];
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }


        public void StartItemFlow()
        {
            try
            {
                SendNoticeToUIAndTxtFile("开始" + TrialItem.ItemName + "--------------------------->");
                _StopWatch.Reset();
                _StopWatch.Start();
                while (true)
                {
                    #region 无需修改部分
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
                    #endregion


                    SetConditionValues();

                    switch (TrialType)
                    {
                        case (int)EmTrialType.STCA1001:
                            APlusApp("STCA1001");
                            break;
                        case (int)EmTrialType.STCA1002:
                            APlusApp("STCA1002");
                            break;
                        case (int)EmTrialType.STCA1003:
                            APlusApp("STCA1003");
                            break;
                        case (int)EmTrialType.STCA1004:
                            APlusApp("STCA1004");
                            break;
                        case (int)EmTrialType.STCA1005:
                            APlusApp("STCA1005");
                            break;
                        case (int)EmTrialType.STCA1006:
                            APlusApp("STCA1006");
                            break;
                        case (int)EmTrialType.STCA1007:
                            APlusApp("STCA1007");
                            break;
                        case (int)EmTrialType.STCA1008:
                            APlusApp("STCA1008");
                            break;
                        case (int)EmTrialType.STCA1009:
                            APlusApp("STCA1009");
                            break;
                        case (int)EmTrialType.STCA1010:
                            APlusApp("STCA1010");
                            break;
                        case (int)EmTrialType.STCA1011:
                            APlusApp("STCA1011");
                            break;
                        case (int)EmTrialType.STCA1012:
                            APlusApp("STCA1012");
                            break;
                        case (int)EmTrialType.STCA1013:
                            APlusApp("STCA1013");
                            break;
                        case (int)EmTrialType.STCA1014:
                            APlusApp("STCA1014");
                            break;
                        case (int)EmTrialType.STCA1015:
                            APlusApp("STCA1015");
                            break;
                        case (int)EmTrialType.STCA1016:
                            APlusApp("STCA1016");
                            break;
                        case (int)EmTrialType.STCA1017:
                            APlusApp("STCA1017");
                            break;
                        case (int)EmTrialType.STCA1018:
                            APlusApp("STCA1018");
                            break;
                        case (int)EmTrialType.STCA1019:
                            APlusApp("STCA1019");
                            break;
                        case (int)EmTrialType.STCA1020:
                            APlusApp("STCA1020");
                            break;
                        case (int)EmTrialType.STCA1021:
                            APlusApp("STCA1021");
                            break;
                        case (int)EmTrialType.STCA1022:
                            APlusApp("STCA1022");
                            break;
                        case (int)EmTrialType.STCA1023:
                            APlusApp("STCA1023");
                            break;
                        case (int)EmTrialType.STCA1024:
                            APlusApp("STCA1024");
                            break;
                        case (int)EmTrialType.STCA1025:
                            APlusApp("STCA1025");
                            break;
                        case (int)EmTrialType.STCA1026:
                            APlusApp("STCA1026");
                            break;

                    }
                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
            finally
            {
                JP_DC_Soft.CloseAPlusApp();
                ControlEquipMent.BMS.BMSProtocolConsistency(lstIDs, 0, 0, 0, 0, 0, 0, 0, 0);
                //关闭CAN报文发送(下位机会主动发送CAN报文，所以要关闭。上位机只做请求访问)
                ControlEquipMent.BMS.BMS_DC_SetControl(lstIDs, 0x50, false);
                var bmsPort = LstPortInfo.Find(p => p.EquiqMentClassName.IndexOf("emtBMS_JP_DC") > -1);
                System.Threading.Thread.Sleep(1000);    //等待A+程序关闭和端口释放
                bmsPort.Open();     //恢复连接
                System.Threading.Thread.Sleep(500);    //等待端口连接正常
            }
        }

        public override void ProcessData()
        {

        }
    }
}
