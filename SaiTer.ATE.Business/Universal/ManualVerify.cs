using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SaiTer.ATE.InterFace;
using System.Diagnostics;
using System.Threading;
using System.Configuration;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 人工确认相关测试项（如一般检查、显示功能等）
    /// </summary>
    public class ManualVerify : BusinessBase
    {
        public ManualVerify(int type) { TrialType = type; }

        int CheckTime = 10;//人工检测时间（秒）
        string TipContent = "";

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            CheckTime = Convert.ToInt32(double.Parse(strParams[0].Split('=')[1]));

            string[] itemParams = TrialItem.ItemParams.Split('|');
            if(itemParams.Length > 0 && itemParams[0].Split('=').Length > 1)
            {
                TipContent = itemParams[0].Split('=')[1];
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

                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                SaveTrialResult();
                SendMessageEndThisTrial();
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


                    List<string> list = new List<string>();
                    //设置测试条件
                    SetConditionValues();
                    string sTmpName = "外观检查";
                    string info = "";
                    //初始化添加需要人工确认的项目
                    if (TrialType == (int)EmTrialType.一般检查)
                    {
                        list.Add("外观检查");
                        list.Add("标志检查");
                        list.Add("基本构成检查");
                        list.Add("机械开关设备检查");
                        //list.Add("防雷措施检查");
                        list.Add("防盗措施检查");
                        string Customer = ConfigurationManager.AppSettings["Customer"];
                        if (!string.IsNullOrEmpty(Customer) && Customer.Equals("NT"))
                        {
                            list.Add("有效连接检查");
                        }
                    }
                    else if (TrialType == (int)EmTrialType.输入功能测试)
                    {
                        list.Add("输入功能");
                        list.Add("启动功能");
                        list.Add("停止功能");
                    }
                    else if (TrialType == (int)EmTrialType.GB_PT_DC_GeneralInspection)//产测的一般检查
                    {
                        list.Add("外观检查");
                        list.Add("标志检查");
                        list.Add("基本构成检查");
                    }
                    else
                    {
                        list.Add(TrialItem.ItemName);
                    }

                    //提示人工确认项
                    for (int i = 0; i < list.Count; i++)
                    {
                        sTmpName = list[i];
                        info = string.IsNullOrEmpty(TipContent) ? "【" + sTmpName + "】为人工目测检查。请确认是否合格\r\n注：勾选上为合格" : TipContent;
                        CountDownTimeInfo(info, CheckTime, 2);
                        ProcessDataTmp1(sTmpName);
                    }
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
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
                double volate = 0;
                if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_GB_DC ||
                    LstChargerInfo[0].ChargerType == EmChargerType.Charger_USA_DC ||
                    LstChargerInfo[0].ChargerType == EmChargerType.Charger_EUR_DC)
                {
                    volate = AllEquipStateData.DicBMS_DC_StateData[LstTrialData[k].ChargerId].ChargingVoltage;
                }
                else
                {
                    volate = AllEquipStateData.DicBMS_AC_StateData[LstTrialData[k].ChargerId].PhaseA_Voltage;
                }

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
                LstTrialData[k].ExtentData = TrialItem.ItemName + "|-|-|-|-";
                LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                SendTrialDataToUI(LstTrialData[k]);
                //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                SaveTrialData(LstTrialData[k]);
            }

        }

        public  void ProcessDataTmp1(string sName)
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
                double volate = 0;
                if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_GB_DC ||
                    LstChargerInfo[0].ChargerType == EmChargerType.Charger_USA_DC ||
                    LstChargerInfo[0].ChargerType == EmChargerType.Charger_EUR_DC)
                {
                    volate = AllEquipStateData.DicBMS_DC_StateData[LstTrialData[k].ChargerId].ChargingVoltage;
                }
                else
                {
                    volate = AllEquipStateData.DicBMS_AC_StateData[LstTrialData[k].ChargerId].PhaseA_Voltage;
                }

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
                LstTrialData[k].ExtentData = sName + "|-|-|-|-";
                LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                LstTrialData[k].ItemName = sName;
                SendTrialDataToUI(LstTrialData[k]);
                //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                SaveTrialData(LstTrialData[k]);
            }

        }
    }
}
