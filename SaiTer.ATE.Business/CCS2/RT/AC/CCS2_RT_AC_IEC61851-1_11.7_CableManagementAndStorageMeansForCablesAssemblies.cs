using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 欧标研测交流：电缆组件的电缆管理和存储方式
    /// </summary>
    public class CCS2_RT_AC_CableManagementAndStorageMeansForCablesAssemblies : BusinessBase
    {
        string itemFlow = "";
        int CheckTime = 10;//人工检测时间（秒）
        string TipContent = "";

        public CCS2_RT_AC_CableManagementAndStorageMeansForCablesAssemblies(int type) { TrialType = type; }

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

                    itemFlow = "电缆组件的电缆管理和存储装置";
                    string info = $"请确认电动汽车充电设备，不使用时应为车辆插头提供一个存储装置。\r\n请确认是否合格(勾选上为合格)";
                    CountDownTimeInfo(info, CheckTime, 2);
                    ProcessData();

                    itemFlow = "电缆组件的电缆管理和存储装置";
                    info = $"请确认存放时车辆插头的最低点应位于距离地面 0.5 m 至 1.5 m 的高度。\r\n请确认是否合格(勾选上为合格)";
                    CountDownTimeInfo(info, CheckTime, 2);
                    ProcessData();

                    itemFlow = "电缆组件的电缆管理和存储装置";
                    info = $"请确认 电缆长度超过 7.5 m 的 EV 充电机，应提充电缆管理系统。不使用时，电缆的自由长度不得超过 7.5 m\r\n请确认是否合格(勾选上为合格)";
                    CountDownTimeInfo(info, CheckTime, 2);
                    ProcessData();

                    itemFlow = "电缆组件的电缆管理和存储装置";
                    info = $"请确保防止在存放或部分存放位置使用的电缆或电缆组件过热。\r\n请确认是否合格(勾选上为合格)";
                    CountDownTimeInfo(info, CheckTime, 2);
                    ProcessData();

                    itemFlow = "电缆组件的电缆管理和存储装置";
                    info = $"请确认根据 IEC 61316：1999 的第 22 条，检查是否符合电缆卷筒的存储要求。\r\n请确认是否合格(勾选上为合格)";
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
                LstTrialData[k].ExtentData = $"{itemFlow}|-|-|-|-";
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
