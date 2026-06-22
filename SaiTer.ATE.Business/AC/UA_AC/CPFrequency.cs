using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/*---------测试说明---
 * 美标
 * 
 * 将充电桩与测试设备插枪连接，启动充电桩充电，检查充电桩CP信频率，间隔5秒记录3个数据点；

 * --------------相关标准及结果阈值
 * 充电桩CP信号输出频率应符合SAE J1772-2017 4.2.1中的相关规定，即频率在980Hz -1020Hz之间；
 * 
 * --------------测试流程
1.读源状态，如果关则开源，默认220V，关负载
2.示波器只开CH3通道，5V/格，时间500us/格，打开CH3通道测量值：频率，占空比，高值，低值
3.触发CH3，上升沿，3V电平
4.检测插枪，（CS电压:1.23-1.82V）（未插枪，提示客户插枪）
5.读导引是否充电中，否则导引开充电自动闭合S2（检测导引交流电压有无大于50V，未检测到提示刷卡，检测到后下一步）
6.读频率测量值，间隔5秒记录3个数据点，980Hz -1020Hz合格，记录到报告
7.不停充电
 
 * 
 */
namespace SaiTer.ATE.Business
{
    /// <summary>
    /// CP频率测试
    /// </summary>
    public class CPFrequency : BusinessBase
    {

        double FreqMax = 1020;
        double FreqMin = 980;
        Dictionary<int, string> DicFreqOne = new Dictionary<int, string>();//第一个频率检测点的数据结果  <枪位号，结果数据>
        Dictionary<int, string> DicFreqTwo = new Dictionary<int, string>();
        Dictionary<int, string> DicFreqThree = new Dictionary<int, string>();
        Dictionary<int, string> dicPath = new Dictionary<int, string>();//截图保存路径
        int TrialFlowNum = 1;
        public CPFrequency(int trialType) { TrialType = trialType; }

        public override void InitEquiMent()
        {
            //  AllEquipStateData.DicBMS_AC_StateData[]
        }

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            FreqMax = Convert.ToDouble(strParams[1].Split('=')[1]);
            FreqMin = Convert.ToDouble(strParams[0].Split('=')[1]);
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
                                LstTrialData[i].ExtentData = "-|-|-|-|null";

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    if (AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage < 50)
                    {
                        ControlEquipMent.ACSource?.ACSource_ON(testWorkParam.lstIDs);
                        ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    }
                }
                if (testWorkParam.lstIDs.Count > 0)
                {
                    SendNoticeToUIAndTxtFile("关闭示波器1、2、4号通道");
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, false, "DC", "20", Channel1, "CPFreq", "50", "V", false, "10", "0");
                    Thread.Sleep(100);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "CPFreq", "50", "V", false, "10", "0");
                    Thread.Sleep(100);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPFreq", "50", "V", false, "10", "0");
                    SendNoticeToUIAndTxtFile("开启示波器3号通道，并设置相应参数");
                    Thread.Sleep(100);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "20", Channel3, "CPFreq", "50", "V", false, "10", "0");

                    Thread.Sleep(100);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "100", 3, "3V", "Single");


                    SendNoticeToUIAndTxtFile("正在检测插枪状态");
                    if (!CheckChargerIn(testWorkParam.lstIDs))
                    {
                        return;
                    }
                   
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    { 
                        return; 
                    }
                   
                    SendNoticeToUIAndTxtFile("设置示波器3号通道测量项为【频率】");
                    ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);
                    Thread.Sleep(100);
                    ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "FREQ", 3);
                    Thread.Sleep(2000);

                    TrialFlowNum = 1;
                    dicPath = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("开始读示波器第1个频率点");
                    DicFreqOne = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FREQ", 3);
                    Thread.Sleep(100);
                    ProcessDataTmp(DicFreqOne, "测量点1", "CP频率(Hz)", FreqMin.ToString(), FreqMax.ToString(), dicPath);

                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);

                    SendNoticeToUIAndTxtFile("开始读示波器第2个频率点");
                    Thread.Sleep(4900);
                    TrialFlowNum = 2;
                    dicPath = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    DicFreqTwo = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FREQ", 3);
                    ProcessDataTmp(DicFreqTwo, "测量点2", "CP频率(Hz)", FreqMin.ToString(), FreqMax.ToString(), dicPath);

                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    SendNoticeToUIAndTxtFile("开始读示波器第3个频率点");
                    Thread.Sleep(5000);
                    TrialFlowNum = 3;
                    dicPath = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    DicFreqThree = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FREQ", 3);
                    ProcessDataTmp(DicFreqThree, "测量点3", "CP频率(Hz)", FreqMin.ToString(), FreqMax.ToString(), dicPath);
                }
            }
        }



        public override void ProcessData()
        {
            try
            {
                foreach (var item in testWorkParam.lstIDs)
                {
                    int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                    int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                    if (k < 0)
                        return;
                    LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;


                    LstTrialData[k].TrialName = TrialItem.ItemName;

                    switch (TrialFlowNum)
                    {
                        case 1:
                            LstTrialData[k].Data1 = DicFreqOne[LstChargerInfo[i].ChargerId];
                            LstTrialData[k].ItemName = "测量点" + TrialFlowNum;
                            LstTrialData[i].ExtentData = "测量点" + TrialFlowNum + "|" + DicFreqOne[LstChargerInfo[i].ChargerId] + "|" + FreqMin + "|" + FreqMax + "|" + dicPath[LstChargerInfo[i].ChargerId];
                            if (!string.IsNullOrEmpty(DicFreqOne[LstChargerInfo[i].ChargerId]) && DicFreqOne[LstChargerInfo[i].ChargerId] != "null")
                            {
                                if (Convert.ToDouble(DicFreqOne[LstChargerInfo[i].ChargerId]) <= FreqMax && Convert.ToDouble(DicFreqOne[LstChargerInfo[i].ChargerId]) >= FreqMin)
                                {
                                    LstTrialData[k].TrialResult = EmTrialResult.Pass;
                                }
                                else
                                {
                                    LstTrialData[k].TrialResult = EmTrialResult.Fail;
                                }
                            }
                            else
                            {
                                LstTrialData[k].TrialResult = EmTrialResult.Fail;
                            }
                            break;
                        case 2:
                            LstTrialData[k].Data1 = DicFreqTwo[LstChargerInfo[i].ChargerId];
                            LstTrialData[k].ItemName = "测量点" + TrialFlowNum;
                            LstTrialData[i].ExtentData = "测量点" + TrialFlowNum + "|" + DicFreqTwo[LstChargerInfo[i].ChargerId] + "|" + FreqMin + "|" + FreqMax + "|" + dicPath[LstChargerInfo[i].ChargerId];
                            if (!string.IsNullOrEmpty(DicFreqTwo[LstChargerInfo[i].ChargerId]) && DicFreqTwo[LstChargerInfo[i].ChargerId] != "null")
                            {
                                if (Convert.ToDouble(DicFreqTwo[LstChargerInfo[i].ChargerId]) <= FreqMax && Convert.ToDouble(DicFreqTwo[LstChargerInfo[i].ChargerId]) >= FreqMin)
                                {
                                    LstTrialData[k].TrialResult = EmTrialResult.Pass;
                                }
                                else
                                {
                                    LstTrialData[k].TrialResult = EmTrialResult.Fail;
                                }
                            }
                            else
                            {
                                LstTrialData[k].TrialResult = EmTrialResult.Fail;
                            }
                            break;
                        case 3:
                            LstTrialData[k].Data1 = DicFreqThree[LstChargerInfo[i].ChargerId];
                            LstTrialData[k].ItemName = "测量点" + TrialFlowNum;
                            LstTrialData[i].ExtentData = "测量点" + TrialFlowNum + "|" + DicFreqThree[LstChargerInfo[i].ChargerId] + "|" + FreqMin + "|" + FreqMax + "|" + dicPath[LstChargerInfo[i].ChargerId];

                            if (!string.IsNullOrEmpty(DicFreqThree[LstChargerInfo[i].ChargerId]) && DicFreqThree[LstChargerInfo[i].ChargerId] != "null")
                            {
                                if (Convert.ToDouble(DicFreqThree[LstChargerInfo[i].ChargerId]) <= FreqMax && Convert.ToDouble(DicFreqThree[LstChargerInfo[i].ChargerId]) >= FreqMin)
                                {
                                    LstTrialData[k].TrialResult = EmTrialResult.Pass;
                                }
                                else
                                {
                                    LstTrialData[k].TrialResult = EmTrialResult.Fail;
                                }
                            }
                            else
                            {
                                LstTrialData[k].TrialResult = EmTrialResult.Fail;
                            }
                            break;
                            //case 4:
                            //    //测量点序号|频率测量值(Hz)|频率下限|频率上限|测试结果|查看示波器截图 
                            //    LstTrialData[i].ExtentData = "测量点" + TrialFlowNum + "|" + DicFreqOne[LstChargerInfo[i].ChargerId] + "|" + DicFreqTwo[LstChargerInfo[i].ChargerId] + "|" + DicFreqThree[LstChargerInfo[i].ChargerId] + "|" + FreqMin + "|" + FreqMax + "|" + dicPath[LstChargerInfo[i].ChargerId];
                            //    break;
                    }
                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    LstTrialData[k].TrialCondition = "";
                    SendTrialDataToUI(LstTrialData[k]);

                    //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                    SaveTrialData(LstTrialData[k]);
                }


            }

            catch (Exception ex)
            {
                SendException(ex);
            }
        }
    }
}
