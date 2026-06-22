using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    public class ManualCheck_General : BusinessBase
    {
        public ManualCheck_General(int type) { TrialType = type; }

        int CheckTime = 10;//人工检测时间（秒）
        string TipContent = "";

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            CheckTime = Convert.ToInt32(double.Parse(strParams[0].Split('=')[1]));

            string[] itemParams = TrialItem.ItemParams.Split('|');
            if (itemParams.Length > 0 && itemParams[0].Split('=').Length > 1)
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
                    string sTmpName = "";
                    string info = "";
                    //初始化添加需要人工确认的项目
                    list.Add(TrialItem.ItemName);

                    //提示人工确认项
                    for (int i = 0; i < list.Count; i++)
                    {
                        sTmpName = list[i];
                        info = "【" + sTmpName + "】为人工目测检查。请确认是否合格\r\n注：勾选上为合格。\r\n检测标准："+ TipContent;
                        CountDownTimeInfo(info, CheckTime, 2);
                        ProcessDataResults(testWorkParam.lstIDs, "-", "-", DicManualVerifyResult, sTmpName);
                    }
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        public override void ProcessData()
        {
        }
    }
}
