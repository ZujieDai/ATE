using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{ 
    public class ConformanceTest : BusinessBase
    {
        int trlTimeOut_S = 10;
        string ItemName = "";   //测试编号，如DP1001
        public ConformanceTest(int trialType)
        {
            TrialType = trialType;
        }

        public override void InitializeParams()
        {
            Init();
        }

        public override void InitEquiMent()
        {
            InitializationProtocol();
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
            try
            {
                SendNoticeToUIAndTxtFile("开始" + TrialItem.ItemName + "--------------------------->");
                _StopWatch.Reset();
                _StopWatch.Start();
                while (true)
                {
                    #region 无需修改部分
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
                        // 检测一遍即可
                        //for (int i = 0; i < LstTrialData.Count; i++)
                        //{
                        //    if (LstTrialData[i].IsCheck)
                        //    {
                        //        if (LstTrialData[i].TrialResult == EmTrialResult.Wait)
                        //        {
                        //            LstTrialData[i].TrialResult = EmTrialResult.Fail;
                        //            LstTrialData[i].TrialValue = ((int)(_StopWatch.ElapsedMilliseconds / 1000)).ToString();
                        //            int k = LstChargerInfo.FindIndex(s => s.ChargerId == LstTrialData[i].ChargerId);
                        //            LstTrialData[i].PKID = LstChargerInfo[k].PKID;
                        //            //界面展示的数据项格式
                        //            //
                        //            LstTrialData[i].ExtentData = "-|-|-|-|null";
                        //            SendTrialDataToUI(LstTrialData[i]);
                        //        }
                        //    }
                        //}
                        break;
                    }
                    if (testWorkParam.lstIDs.Count <= 0)
                    {
                        return;
                    }
                    #endregion

                    SetConditionValues();

                    // 创建测试流程并执行
                    CreatTestContent();
                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
            finally
            {
                RecoverProtocolConsist();
            }
        }

        public override void ProcessData()
        {

        }

    }
}
