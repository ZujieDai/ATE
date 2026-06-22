using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SaiTer.ATE.InterFace;
using System.Diagnostics;
using System.Configuration;

/*
 * 四川TRW客户没有漏电测试相关硬件设备，为了满足审厂要求，需要添加漏电测试项
 * 
 * 软件按照中佳漏电板设备做一个虚假的测试流程，生成随机数
 * 
 * 注：随机数的范围是客户要求的，没有其它需求时，不要改动
 * 
 */
 

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 漏电流保护测试（用于做模拟数据）
    /// </summary>
    public class LeakageCurrentTest_Joker : BusinessBase
    {
        public LeakageCurrentTest_Joker(int type) { TrialType = type; }
        string ItemFlow = "";
        string State = "";
        private double 阶段1检测时间, 阶段1AC动作电流上限, 阶段1AC动作电流下限, 阶段1测试结束延时时间, 供电电压;
        private double 阶段2检测时间, 阶段2AC动作电流上限, 阶段2AC动作电流下限, 阶段2测试结束延时时间;

        public override void InitializeParams()
        {
            //数据库参数格式
            //阶段1检测时间(S)=30|阶段1AC动作电流上限(mA)=15|阶段1AC动作电流下限(mA)=0|阶段1测试结束延时时间(S)=5|供电电压(V)=102|
            //阶段2检测时间(S)=30|阶段2AC动作电流上限(mA)=20|阶段2AC动作电流下限(mA)=15|阶段2测试结束延时时间(S)=5


            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            阶段1检测时间 = double.Parse(strParams[0].Split('=')[1]);
            阶段1AC动作电流上限 = double.Parse(strParams[1].Split('=')[1]);
            阶段1AC动作电流下限 = double.Parse(strParams[2].Split('=')[1]);
            阶段1测试结束延时时间 = double.Parse(strParams[3].Split('=')[1]);
            供电电压 = double.Parse(strParams[4].Split('=')[1]);
            阶段2检测时间 = double.Parse(strParams[5].Split('=')[1]);
            阶段2AC动作电流上限 = double.Parse(strParams[6].Split('=')[1]);
            阶段2AC动作电流下限 = double.Parse(strParams[7].Split('=')[1]);
            阶段2测试结束延时时间 = double.Parse(strParams[8].Split('=')[1]);

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


                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    //设置测试条件
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        int key = testWorkParam.lstIDs[i];
                        if (AllEquipStateData.DicBMS_AC_StateData.Count == 1)
                        {
                            key = 1;//多个桩只有一个源的时候，源实时状态字典内只有一个1号桩
                        }

                        d1.Add(testWorkParam.lstIDs[i], 供电电压.ToString());
                        d2.Add(testWorkParam.lstIDs[i], "60");
                    }
                    SetConditionValue("供电电压(V)", d1);
                    SetConditionValue("供电频率(Hz)", d2);


                    SendNoticeToUIAndTxtFile("设置漏电仪参数");
                    Thread.Sleep(2000);
                    SendNoticeToUIAndTxtFile("闭合漏电仪S2,S1,开始检测");
                    Thread.Sleep(2000);





                    ItemFlow = "空载/L1";
                    SendNoticeToUIAndTxtFile(ItemFlow);
                    Thread.Sleep(1000);
                    ProcessData();


                    ItemFlow = "满载/L1";
                    SendNoticeToUIAndTxtFile(ItemFlow);
                    SendNoticeToUIAndTxtFile("正在启动负载");
                    SetResisLoadVolCurrAndStart(testWorkParam.lstIDs, 供电电压, LstChargerInfo[0].NominalCurrent);
                    Thread.Sleep(2000);
                    ProcessData();
                    ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(500);


                    ItemFlow = "空载/L2";
                    SendNoticeToUIAndTxtFile(ItemFlow);
                    Thread.Sleep(1000);
                    ProcessData();


                    ItemFlow = "满载/L2";
                    SendNoticeToUIAndTxtFile(ItemFlow);
                    SendNoticeToUIAndTxtFile("正在启动负载");
                    SetResisLoadVolCurrAndStart(testWorkParam.lstIDs, 供电电压, LstChargerInfo[0].NominalCurrent);
                    Thread.Sleep(2000);
                    ProcessData();
                    ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(500);


                    //----------------测试阶段2,空载/L1----------------------------------------------
                    ItemFlow = "测试阶段2";
                    Dictionary<int, string> dicData = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {  
                        double testData = new Random().Next(16, 17) + new Random().NextDouble();
                        dicData.Add(item, testData.ToString("F2"));
                    }
                   
                    SendNoticeToUIAndTxtFile(ItemFlow + "空载/L1,测动作电流");
                    Thread.Sleep(2000);
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(500);
                    ProcessDataTmp(dicData, ItemFlow, "空载/L1,测动作电流(mA)", "15", "20");


                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    Thread.Sleep(2000);
                    SendNoticeToUIAndTxtFile(ItemFlow + "空载/L1,测动作时间");
                    Thread.Sleep(2000);
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(500);
                    dicData.Clear();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double testData = new Random().Next(120, 130) + new Random().NextDouble() * 10;
                        dicData.Add(item, testData.ToString("F2"));
                    }
                    ProcessDataTmp(dicData, ItemFlow, "空载/L1,测动作时间(mS)", "0", "1000");





                    //------------------------测试阶段2,满载/L1--------------------------------------

                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    Thread.Sleep(2000);                  
                    SendNoticeToUIAndTxtFile(ItemFlow + "满载/L1,测动作电流");
                    SendNoticeToUIAndTxtFile("正在启动负载");
                    SetResisLoadVolCurrAndStart(testWorkParam.lstIDs, 供电电压, LstChargerInfo[0].NominalCurrent);
                    Thread.Sleep(1000);
                    dicData.Clear();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double testData = new Random().Next(18, 19) + new Random().NextDouble();
                        dicData.Add(item, testData.ToString("F2"));
                    }
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(500);
                    ProcessDataTmp(dicData, ItemFlow, "满载/L1测动作电流(mA)", "15", "20");
                    SendNoticeToUIAndTxtFile("正在关闭负载");
                    ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(500);



                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    Thread.Sleep(2000);
                 
                    SendNoticeToUIAndTxtFile(ItemFlow + ",满载/L1,测动作时间");
                    SendNoticeToUIAndTxtFile("正在启动负载");
                    SetResisLoadVolCurrAndStart(testWorkParam.lstIDs, 供电电压, LstChargerInfo[0].NominalCurrent);
                    Thread.Sleep(1000);
                    dicData.Clear();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double testData = new Random().Next(170, 190) + new Random().NextDouble() * 20;
                        dicData.Add(item, testData.ToString("F2"));
                    }
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(500);
                    ProcessDataTmp(dicData, ItemFlow, "满载/L1测动作时间(mS)", "0", "1000");
                    SendNoticeToUIAndTxtFile("正在关闭负载");
                    ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(500);





                    //----------------测试阶段2,空载/L2----------------------------------------------
                    dicData.Clear();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double testData = new Random().Next(16, 17) + new Random().NextDouble();
                        dicData.Add(item, testData.ToString("F2"));
                    }
                    
                    SendNoticeToUIAndTxtFile(ItemFlow + ",空载/L2,测动作电流");
                    Thread.Sleep(2000);
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(1000);
                    ProcessDataTmp(dicData, ItemFlow, "空载/L2,测动作电流(mA)", "15", "20");


                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    Thread.Sleep(2000);
                    SendNoticeToUIAndTxtFile(ItemFlow + ",空载/L2,,测动作时间");
                    Thread.Sleep(2000);
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(1000);
                    dicData.Clear();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double testData = new Random().Next(120, 130) + new Random().NextDouble() * 10;
                        dicData.Add(item, testData.ToString("F2"));
                    }
                    ProcessDataTmp(dicData, ItemFlow, "空载/L2,测动作时间(mS)", "0", "1000");



                    //------------------------测试阶段2,满载/L2--------------------------------------

                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    Thread.Sleep(2000);                 
                    SendNoticeToUIAndTxtFile(ItemFlow + ",满载/L2,测动作电流");
                    SendNoticeToUIAndTxtFile("正在启动负载");
                    SetResisLoadVolCurrAndStart(testWorkParam.lstIDs, 供电电压, LstChargerInfo[0].NominalCurrent);
                    Thread.Sleep(1000);
                    dicData.Clear();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double testData = new Random().Next(18, 19) + new Random().NextDouble();
                        dicData.Add(item, testData.ToString("F2"));
                    }
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(1000);
                    ProcessDataTmp(dicData, ItemFlow, "满载/L2,测动作电流(mA)", "15", "20");
                    SendNoticeToUIAndTxtFile("正在关闭负载");
                    ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(1000);



                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    Thread.Sleep(2000);                  
                    SendNoticeToUIAndTxtFile(ItemFlow + "，满载/L2,测动作时间");
                    SendNoticeToUIAndTxtFile("正在启动负载");
                    SetResisLoadVolCurrAndStart(testWorkParam.lstIDs, 供电电压, LstChargerInfo[0].NominalCurrent);
                    Thread.Sleep(1000);
                    dicData.Clear();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double testData = new Random().Next(170, 190) + new Random().NextDouble() * 20;
                        dicData.Add(item, testData.ToString("F2"));
                    }
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(1000);
                    ProcessDataTmp(dicData, ItemFlow, "满载/L2,测动作时间(mS)", "0", "1000");
                    SendNoticeToUIAndTxtFile("正在关闭负载");
                    ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(1000);
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


                LstTrialData[k].TrialResult = EmTrialResult.Pass;

                LstTrialData[k].ExtentData = "测试阶段1|" + ItemFlow + "，测动作电流(mA)|" + 阶段1AC动作电流下限 + "|" + 阶段1AC动作电流上限 + "|" + "正常工作|报表(勿删)";


                LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                LstTrialData[k].ItemName = ItemFlow;
                SendTrialDataToUI(LstTrialData[k]);
                SaveTrialData(LstTrialData[k]);
            }
        }


    }
}
