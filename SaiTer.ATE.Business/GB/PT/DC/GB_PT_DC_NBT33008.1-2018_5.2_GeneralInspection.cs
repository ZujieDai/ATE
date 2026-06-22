using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 国标产测直流：一般检查
    /// </summary>
    public class GB_PT_DC_GeneralInspection : BusinessBase
    {
        string itemFlow = "";
        public GB_PT_DC_GeneralInspection(int type) { TrialType = type; }

        int CheckTime = 10;//人工检测时间（秒）

        public override void InitializeParams()
        {
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
                SetCPReresh();
                Init();
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

                    ////设置测试条件
                    //SetConditionValues();

                    //itemFlow = "一般检查";
                    //string info = $"【{itemFlow}】请确认充电机符合标准\r\n(勾选上为合格)";
                    //CountDownTimeInfo(info, CheckTime, 2);



                    List<string> list = new List<string>();
                    //设置测试条件
                    SetConditionValues();
                    string sTmpName = "外观检查";
                    string info = ""; 

                    if (TrialType == (int)EmTrialType.GB_PT_DC_GeneralInspection)//产测的一般检查
                    {
                        list.Add("外观检查");
                        list.Add("标志检查");
                        list.Add("基本构成检查");
                        list.Add("机械开关检查");
                        list.Add("防雷措施检查");
                        list.Add("防盗措施检查");
                    }
                    else
                    {
                        list.Add(TrialItem.ItemName);
                    }
                    //提示人工确认项
                    for (int i = 0; i < list.Count; i++)
                    {
                        sTmpName = list[i];
                        info =  "【" + sTmpName + "】为人工目测检查。请确认是否合格\r\n注：勾选上为合格" ;
                        CountDownTimeInfo(info, CheckTime, 2);
                        //ProcessDataResult(testWorkParam.lstIDs, "-", sTmpName, DicManualVerifyResult.First().Value, "一般检查");
                        d1 = new Dictionary<int, string>();
                        var dicResult = new Dictionary<int, EmTrialResult>();
                        foreach (int item in testWorkParam.lstIDs)
                        {
                            d1.Add(item, "-");
                            dicResult.Add(item, DicManualVerifyResult[item] ? EmTrialResult.Pass : EmTrialResult.Fail);
                        }
                        ProcessDataResults(testWorkParam.lstIDs, d1, sTmpName, dicResult, "一般检查");
                    }

                    //ProcessData();
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        public override void ProcessData()
        {

        }

    }
}
