using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    public class CZ_TB_GeneralInspection : BusinessBase
    {
        string itemFlow = "";
        public CZ_TB_GeneralInspection(int type) { TrialType = type; }

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
                SetCPRersh_EUDC();
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


                    List<string> list = new List<string>()
                    {
                        "外壳检查",
                        "涂层和零部件检查",
                        "字迹检查",
                        "连接检查",
                        "急停检查"
                    };
                    //设置测试条件
                    SetConditionValues();
                    string sTmpName = "外观检查";
                    List<string> infos = new List<string>()
                    {
                        "检查充电机外壳应平整，无明显凹痕、划伤、变形等缺陷",
                        "表面涂镀层应均匀、不应脱落；零部件紧固可靠，无锈蚀、毛刺、裂纹等缺陷和损伤",
                        "所有铭牌、标志均安装端正牢固，字迹清晰",
                        "检查电源线和枪线是否锁紧，防止线缆轻易拉伸",
                        "检查急停按钮按压是否正常，按下能否正常回弹"
                    };


                    //提示人工确认项
                    for (int i = 0; i < list.Count; i++)
                    {
                        sTmpName = list[i];
                        string info = $"请确认是否符合【{infos[i]}】\r\n注：勾选上为合格";
                        CountDownTimeInfo(info, CheckTime, 2);
                        ProcessDataResult(testWorkParam.lstIDs, "-", "-", DicManualVerifyResult.First().Value, sTmpName);
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
