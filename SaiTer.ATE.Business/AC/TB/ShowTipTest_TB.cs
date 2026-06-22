using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 人工设置提示内容，判断结果
    /// </summary>
    public class ShowTipTest_TB : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据

        string TipText1 = "";
        string SNameText1 = "";

        public ShowTipTest_TB(int type)
        {
            TrialType = type;
        }

        public override void InitializeParams()
        {
            Init();

            //弹窗提示(文本)=|判断依据(文本)=
            string[] strParams = TrialItem.ResultParams.Split('|');
            TipText1 = strParams[0].Split('=')[1];
            SNameText1 = strParams[1].Split('=')[1];
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
                ControlEquipMent.ACSource.ACSource_ON(lstIDs);

                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                SaveTrialResult();
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
                                //测试时间|输入电压|是否停机|测试结果     
                                LstTrialData[i].ExtentData = "-|-|-|-|null";
                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }

                //开始检测流程
                if (testWorkParam.lstIDs.Count < 0)
                {

                    return;
                }

                ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);
                Thread.Sleep(1500);
                SetConditionValues();

                CountDownTimeInfo(TipText1 + "\r\n（勾选为合格，否则为不合格）", 999, 2);
                ProcessDataConnect(null, SNameText1);
            }
        }


        public override void ProcessData()
        {

        }
    }
}
