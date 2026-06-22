using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
/*
* 
未充电前，继电器L黏连测试
未充电前，继电器N黏连测试
充电后，继电器L黏连测试
充电后，继电器N黏连测试

程控板：
1.零线粘连：K10
2.火线粘连：K11
3.单相切换：K12
* 
* 
*/
namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 接触器粘连测试 继电器控制零、火线黏连
    /// </summary>
    public class ContactorAdhesionTest_K : BusinessBase
    {
        private float trlTimeOut_S = 100;//超时时间

        private string ItemFlow = "";//流程步骤

        private List<bool> listK = new List<bool>() { true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };//程控板继电器
        public ContactorAdhesionTest_K(int type)
        {
            TrialType = type;
        }

        public override void InitializeParams()
        {
            Init();
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
               
                ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(2000);
                if (!CheckChargerIn(testWorkParam.lstIDs))
                {
                    return;
                }
                //设置测试条件
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    int key = testWorkParam.lstIDs[i];
                    if (AllEquipStateData.DicACSource_StateData.Count == 1)
                    {
                        key = 1;//多个桩只有一个源的时候，源实时状态字典内只有一个1号桩
                    }
                    d1.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicACSource_StateData[key].Volt.ToString());
                    d2.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicACSource_StateData[key].Freq.ToString());
                }
                SetConditionValue("供电电压(V)", d1);
                SetConditionValue("供电频率(Hz)", d2);

                string info = "继电器L黏连测试";
                ItemFlow = "接触器L黏连";
                SendNoticeToUIAndTxtFile(info);
                listK[10] = false;
                listK[11] = true;
                ControlEquipMent.ControlBoard.ControlResistanceSetRelay(listK);
                Thread.Sleep(2000);
                CountDownTimeInfo(info + "请人工判断结果(有火线黏连或者黏连故障则PASS)", 100, 2);
                ProcessData();
                listK[10] = false;
                listK[11] = false;
                ControlEquipMent.ControlBoard.ControlResistanceSetRelay(listK);
                ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);
                Thread.Sleep(2000);


                info = "继电器N黏连测试";
                ItemFlow = "接触器N黏连";
                SendNoticeToUIAndTxtFile(info);

                ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);
                if (!CheckChargerIn(testWorkParam.lstIDs))
                {
                    return;
                }
                listK[10] = true;
                listK[11] = false;
                ControlEquipMent.ControlBoard.ControlResistanceSetRelay(listK);
                Thread.Sleep(2000);
                CountDownTimeInfo(info + "请人工判断结果(有零线黏连或者黏连故障则PASS)", 100, 2);
                ProcessData();

                //SendNoticeToUIAndTxtFile("启动充电");

                //if (!CheckSwipingCard(testWorkParam.lstIDs))
                //{
                //    return;
                //}

                //info = "继电器L黏连测试";
                //ItemFlow = "充电后火线黏连";
                //SendNoticeToUIAndTxtFile(info);
                //listK[10] = true;
                //listK[11] = false;
                //ControlEquipMent.ControlBoard.ControlResistanceSetRelay(listK);
                //CountDownTimeInfo(info + "请人工判断结果", 100, 2);
                //ProcessData();


                //info = "继电器N黏连测试";
                //ItemFlow = "充电后零线黏连";
                //SendNoticeToUIAndTxtFile(info);
                //listK[10] = false;
                //listK[11] = true;
                //ControlEquipMent.ControlBoard.ControlResistanceSetRelay(listK);

                //CountDownTimeInfo(info + "请人工判断结果", 100, 2);
                //ProcessData();

                listK[11] = false;
                listK[10] = false;
                ControlEquipMent.ControlBoard.ControlResistanceSetRelay(listK);

                List<bool> Ks = GetKStatus16_Charging();
                // Ks[5] = false;
                Ks[0] = false;
                ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);

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
            }

        }
    }
}
