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

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 人工确认相关测试项（如一般检查、显示功能等）
    /// </summary>
    public class ManualInspection : BusinessBase
    {
        string itemFlow = "";
        public ManualInspection(int type) { TrialType = type; }

        int CheckTime = 10;//人工检测时间（秒）

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            CheckTime = Convert.ToInt32(double.Parse(strParams[0].Split('=')[1]));
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

                    //设置测试条件
                    SetConditionValues();

                    itemFlow = "外观检查";
                    string info = $"【{itemFlow}】充电机（含充电连接装置）的外壳应平整，无明显凹凸痕、划伤、变形等缺陷；表面涂镀层应均匀，无脱落；零部件（包括连接装置内触头）应紧固可靠，无锈蚀、毛刺、裂纹等缺陷和损伤\r\n注：勾选上为PASS，否则为FAIL";
                    CountDownTimeInfo(info, CheckTime, 2);
                    ProcessData();

                    itemFlow = "标志检查";
                    info = $"【{itemFlow}】充电机铭牌位置和内容的正确性与完整性，铭牌内容应符合NB/T 33001—2018中8.1.1的规定。充电机接线、接地及安全标志的正确性与完整性。通过观察并用一块浸透蒸馏水的脱脂棉在约15 s内擦拭15个来回，随后用一块浸透汽油的脱脂棉在约15 s内擦拭15个来回，试验期间应用约2N/cm2的压力将脱脂棉压在标志上，试验后，标志仍应易于辨认。\r\n注：勾选上为PASS，否则为FAIL";
                    CountDownTimeInfo(info, CheckTime, 2);
                    ProcessData();

                    itemFlow = "基本构成检查";
                    info = $"【{itemFlow}】打开充电机门，充电机的基本构成应包括动力电源输入、功率变换单元、输出开关单元、充电电缆和车辆插头、控制电源、充电控制单元、人机交互单元，宜包括有计量等功能单元。\r\n注：勾选上为PASS，否则为FAIL";
                    CountDownTimeInfo(info, CheckTime, 2);
                    ProcessData();

                    itemFlow = "机械开关检查：开关和隔离开关";
                    info = $"【{itemFlow}】充电机的交流/直流接触器应符合GB/T 18487.1—2015中10.2.2的规定或具备对应的证明材料。\r\n注：勾选上为PASS，否则为FAIL";
                    CountDownTimeInfo(info, CheckTime, 2);
                    ProcessData();

                    itemFlow = "机械开关检查：接触器";
                    info = $"【{itemFlow}】充电机的断路器应符合GB/T 18487.1—2015中10.2.3的规定或具备对应的证明材料。\r\n注：勾选上为PASS，否则为FAIL";
                    CountDownTimeInfo(info, CheckTime, 2);
                    ProcessData();

                    itemFlow = "机械开关检查：断路器";
                    info = $"【{itemFlow}】充电机的功率继电器应符合GB/T 18487.1—2015中10.2.4的规定或具备对应的证明材料。\r\n注：勾选上为PASS，否则为FAIL";
                    CountDownTimeInfo(info, CheckTime, 2);
                    ProcessData();

                    itemFlow = "机械开关检查：继电器";
                    info = $"【{itemFlow}】充电机应采取避雷防护措施，且符合GB/T 18487.1—2015中11.7的规定。\r\n注：勾选上为PASS，否则为FAIL";
                    CountDownTimeInfo(info, CheckTime, 2);
                    ProcessData();

                    itemFlow = "防雷措施检查";
                    info = $"【{itemFlow}】充电机应采取避雷防护措施，且符合GB/T 18487.1—2015中11.7的规定。\r\n注：勾选上为PASS，否则为FAIL";
                    CountDownTimeInfo(info, CheckTime, 2);
                    ProcessData();

                    itemFlow = "防盗措施检查";
                    info = $"【{itemFlow}】对于户外型充电机，检查其应具有防盗措施，如防盗锁和防盗螺钉等，且产品安装说明书中应有相关要求。\r\n注：勾选上为PASS，否则为FAIL";
                    CountDownTimeInfo(info, CheckTime, 2);
                    ProcessData();
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
                LstTrialData[k].ItemName = iIndex.ToString();
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
                LstTrialData[k].ExtentData = itemFlow + "|-|-|-|-";
                LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                SendTrialDataToUI(LstTrialData[k]);
                //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                SaveTrialData(LstTrialData[k]);
            }
            iIndex++;
        }
    }
}
