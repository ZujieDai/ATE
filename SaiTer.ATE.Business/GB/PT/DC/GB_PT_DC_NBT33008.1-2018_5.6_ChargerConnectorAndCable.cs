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
    /// 国标产测直流：电缆管理及贮存检查（人工）
    /// </summary>
    public class GB_PT_DC_ChargerConnectorAndCable : BusinessBase
    {
        string itemFlow = "";
        public GB_PT_DC_ChargerConnectorAndCable(int type) { TrialType = type; }

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

                    itemFlow = "连接装置";
                    string info = $"【{itemFlow}】请检查充电机所配置的充电用连接装置应具备符合GB/T 20234.1—2015、GB/T 20234.3—2015规定的证明材料，或者按照GB/T 34657.1—2017中6.2规定的方法对车辆插头的结构尺寸、插头空间尺寸进行复核。\r\n(勾选上为合格)";
                    CountDownTimeInfo(info, CheckTime, 2);
                    ProcessDataResult(testWorkParam.lstIDs, "-", itemFlow, DicManualVerifyResult.First().Value, "充电连接装置检查");

                    itemFlow = "管理及贮存";
                    info = $"【{itemFlow}】请检查充电机电缆长度不应超过7.5m，并应随桩配置车辆插头贮存装置，或符合标准其它要求。\r\n(勾选上为合格)";
                    CountDownTimeInfo(info, CheckTime, 2);
                    ProcessDataResult(testWorkParam.lstIDs, "-", itemFlow, DicManualVerifyResult.First().Value, "电缆管理及贮存检查");
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        public override void ProcessData()
        {
        }
    }
}
