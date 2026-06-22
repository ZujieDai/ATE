using SaiTer.ATE.Controls;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.DataBaseModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.Struct;
using SaiTer.ATE.IDAL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    public class ProtocolConformance : BusinessBase
    {
        int trlTimeOut_S = 1600;
        public ProtocolConformance(int type)
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
                SetCPReresh();
                //保存试验结果               
                SaveTrialResult();
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                //发送试验结束刷新UI
                SendMessageEndThisTrial();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tCommand"></param>
        /// <param name="trialType"></param>
        public void APlusApp(EmTrialType trialType)
        {
            var sendStr = "";
            // 反射获取枚举值的特性
            var fieldInfo = trialType.GetType().GetField(trialType.ToString());
            var attribute = fieldInfo?.GetCustomAttribute<APlusAppParamAttribute>();
            var tCommand = ((EmTrialType)TrialType).ToString().Replace(".", "");

            var bmsPort = LstPortInfo.Find(p => p.EquiqMentClassName.IndexOf("emtBMS") > -1);   //交流BMS
            var wrbPort = LstPortInfo.Find(p => p.EquiqMentClassName == "emtWaveRecoderBoard"); //录波板
            string strWrb = Convert.ToString(ConfigurationManager.AppSettings["WaveRecordIP"]);
            try
            {
                bmsPort.Close();    //解决占用
                bool openRes = false;
                //COM连接的BMS
                if(bmsPort.PortName.Contains("COM"))
                {
                    openRes = AplusSoft.OpenAPlusAppInitialize(bmsPort.PortName, Convert.ToInt32(bmsPort.PortParams), strWrb);
                }
                //TCP连接的BMS
                if (bmsPort.PortParams.Contains("."))
                {
                    openRes = AplusSoft.OpenAPlusAppInitialize(bmsPort.PortParams, Convert.ToInt32(bmsPort.PortName), strWrb);
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

                if (tCommand.Contains("D0"))
                {
                    AplusSoft.APlusClient.Send("INTEROP,START," + tCommand.Replace(".", ""));
                }
                else
                {
                    
                    //获取不到就是2015之前的老版本
                    if (attribute != null)
                    {
                        switch (attribute.ProtocolConsistencyVersion)
                        {
                            case "2025_A类":
                                tCommand = tCommand.Substring(0, tCommand.IndexOf('_'));
                                sendStr ="CONSIST2023,START," +tCommand;
                                break;
                        }
                    }
                    else
                        sendStr = "CONSIST,START," + tCommand;

                    AplusSoft.APlusClient.Send(sendStr);

                }
                int num = 0;
                string recCommand="";
                while (true)
                {
                    recCommand = Encoding.UTF8.GetString(AplusSoft.APlusClient.LstRcvBytes.ToArray());
                    Console.WriteLine(recCommand);
                    if (tCommand.Contains("D0"))
                    {
                        if (recCommand == "INTEROP,FINISH," + tCommand)
                        {
                            recCommand = "";
                            break;
                        }
                    }
                    else
                    {
                      
                        if (RecvTestEnd(attribute==null?null:attribute.ProtocolConsistencyVersion, recCommand, tCommand))
                            break;
                       
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
                if (tCommand.Contains("D0"))
                {
                    LstTrialData[0].SchemeID = TrialItem.SchemeID;
                    LstTrialData[0].SchemeName = TrialItem.SchemeName;
                    LstTrialData[0].ItemName = TrialItem.ItemName;
                    APlusSQLiteHelper.CheckDataToTable_TestInterop(tCommand, LstTrialData);
                    LstItemData = APlusSQLiteHelper.CheckDataToTable_InteropItems(tCommand, LstTrialData, LstChargerInfo[i].BarCode, LstChargerInfo[i].PKID);
                }
                else
                {
                    LstItemData = APlusSQLiteHelper.CheckDataToTable_TestItemsReport(tCommand.Replace(".", ""), TrialItem, LstChargerInfo[i].BarCode, LstChargerInfo[i].PKID);

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
        /// <summary>
        ///是否测试结束
        /// </summary>
        /// <param name="protocolConsistencyVersion"></param>
        /// <param name="recCommand"></param>
        /// <param name="tCommand"></param>
        /// <returns></returns>
        private bool RecvTestEnd(string protocolConsistencyVersion, string recCommand, string tCommand)
        {

            switch (protocolConsistencyVersion)
            {
                case "2025_A类":
                    tCommand= "CONSIST2023,FINISH," + tCommand;
                    break;
                default:
                        tCommand = "CONSIST,FINISH," + tCommand;
                    break;
            }

            if (recCommand == tCommand)
                return true;
            else
                return false;
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

                    //上电
                    //if (!CheckSwipingCard(testWorkParam.lstIDs))
                    //{
                    //    return;
                    //}
                    SetConditionValues();
                    APlusApp((EmTrialType)TrialType);
                  
              
                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
            finally
            {
                AplusSoft.CloseAPlusApp();
                ControlEquipMent.BMS.BMSProtocolConsistency(lstIDs, 0, 0, 0, 0, 0, 0, 0, 0);
                //关闭CAN报文发送(下位机会主动发送CAN报文，所以要关闭。上位机只做请求访问)
                ControlEquipMent.BMS.BMS_DC_SetControl(lstIDs, 0x50, false);
                var bmsPort = LstPortInfo.Find(p => p.EquiqMentClassName.IndexOf("emtBMS") > -1);
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
