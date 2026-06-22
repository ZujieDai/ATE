using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.CAN;
using SaiTer.ATE.DataModel.DataBaseModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.InterFace
{
    public static class SystemEvent
    {
        /// <summary>
        /// 设备返回数据集事件
        /// </summary>
        public static event ResultDataEventHandler EquipMentResultEvent;
        /// <summary>
        /// 设备数据事件发送方法
        /// </summary>
        /// <param name="Result">结果集</param>
        public static void SendEquipMentResult(StResultData Result)
        {
            if (EquipMentResultEvent != null)
            {
                EquipMentResultEvent(Result);
            }
        }
        /// <summary>
        /// 当前检定ID事件
        /// </summary>
        public static event OnSendCheckItemID SendCheckItemIDEvent;
        /// <summary>
        /// 发送检定系统当前检定项目
        /// </summary>
        public static event OnSendCheckItems SendCheckItemsEvent;
        /// <summary>
        /// 发送检定系统当前检定项目
        /// </summary>
        /// <param name="isTestItems">true-由勾选测试项触发   false-由选择方案名称触发</param>
        public static void SendDataGridViewItems(List<StTrialItem> lstItems, bool isTestItems, string strSchemeName = null)
        {
            if (SendCheckItemsEvent != null)
            {
                SendCheckItemsEvent(lstItems, isTestItems, strSchemeName);
            }
        }
        /// <summary>
        /// 发送设备连接状态
        /// </summary>
        public static event SendConnectStateHandler SentConnectStateEvent;
        /// <summary>
        /// 发送设备连接状态到窗体
        /// </summary>
        /// <param name="isConnect">连接状态</param>
        /// <param name="obj">设备对象</param>
        public static void SendConnectState(bool isConnect, object obj)
        {
            if (SentConnectStateEvent != null)
            {
                SentConnectStateEvent(isConnect, obj);
            }
        }

        /// <summary>
        /// 发送设备监视数据
        /// </summary>
        public static event SendMonitorMessageHandler SendMonitorMessageEvent;

        /// <summary>
        /// 外发检定消息[监视器数据]
        /// </summary>
        /// <param name="monitorData">监视器数据结构</param>
        public static void SendMonitorMessage(object monitorData)
        {
            if (SendMonitorMessageEvent != null)
            {
                SendMonitorMessageEvent(monitorData);
            }
        }
        /// <summary>
        /// 弹窗提示信息
        /// </summary>
        public static event SendCountDownTimerHandler SendCountDownTimerEvent;
        /// <summary>
        /// 倒计时窗口
        /// </summary>
        /// <param name="info">提示信息</param>
        /// <param name="time">倒计时时间(秒)</param>
        /// <param name="type">提示类型 0-纯倒计时提示信息。 1-倒计时等待选择</param>
        /// <param name="tag">输入数据的默认值</param>
        public static void SendCountDownTimer(string info, int time, int type, string tag = "")
        {
            if (SendCountDownTimerEvent != null)
            {
                SendCountDownTimerEvent(info, time, type, tag);
            }
        }


        public static event SendCountDownTimerResultHandler SendCountDownTimerResultEvent;

        public static void SendCountDownTimerResult(bool result)
        {
            if (SendCountDownTimerResultEvent != null)
            {
                SendCountDownTimerResultEvent(result);
            }
        }

        public static event SendManualVerifyResultHandler SendManualVerifyResultEvent;
        /// <summary>
        ///  /// <summary>
        /// 人工确认结果(枪位号，合格结论)
        /// </summary>
        /// </summary>
        /// <param name="dicResult"></param>
        public static void SendManualVerifyResult(Dictionary<int, bool> dicResult)
        {
            if (SendManualVerifyResultEvent != null) { SendManualVerifyResultEvent(dicResult); }
        }


        public static event SendInputDataHandler SendInputDataEvent;
        /// <summary>
        /// UI输入的数据
        /// </summary>
        /// <param name="value"></param>
        public static void SendInputData(string value)
        {
            if (SendInputDataEvent != null)
            {
                SendInputDataEvent(value);
            }

        }
        public static event SendLogMessageHandler SendLogMessageEvent;

        /// <summary>
        /// 发送日志到UI显示
        /// </summary>
        /// <param name="logMsg">日志内容</param>
        public static void SendLogMessage(string logMsg)
        {
            if (SendLogMessageEvent != null)
            {
                SendLogMessageEvent(logMsg);
            }
        }
        /// <summary>
        /// 发送检定项结论数据
        /// </summary>
        public static event SendTestResultToUIHandler SendTestResultToUIEvent;

        /// <summary>
        /// 发送检定项结论数据到UI
        /// </summary>
        /// <param name="Data">结果集</param>
        public static void SendTrialResultToUI(TrialDataModel Data, int chargerID = 1, bool isClear = false, int TrialIndex = -1)
        {
            if (SendTestResultToUIEvent != null)
            {
                SendTestResultToUIEvent(Data, chargerID, isClear, TrialIndex);
            }
        }
        /// <summary>
        /// 发送详细数据到UI
        /// </summary>
        public static event SendDataMessageToUIHandler SendDataMessageToUIEvent;

        /// <summary>
        /// 试验详细数据发送到UI
        /// </summary>   
        public static void SendDataMessageToUI(TrialDataModel TrialData, bool isClear = false)
        {
            if (SendDataMessageToUIEvent != null)
            {
                SendDataMessageToUIEvent(TrialData, isClear);
            }
        }
        /// <summary>
        /// 弹窗等待刷卡
        /// </summary>
        public static event SendWaitSwipingCardHandler SendWaitSwipingCardEvent;

        /// <summary>
        /// 弹窗等待刷卡
        /// </summary>
        /// <param name="lstIDs">需要刷卡的充电桩编号集合</param>
        /// <param name="tBMSDemandVoltage">BMS需求电压</param>
        /// <param name="BMSType">BMS类型</param>
        /// <param name="type">t弹窗类型 0：等待刷卡   1：插枪检测</param>
        public static void SendWaitSwipingCard(List<int> lstIDs, double tBMSDemandVoltage, EmChargerType BMSType, int type)
        {
            if (SendWaitSwipingCardEvent != null)
            {
                SendWaitSwipingCardEvent(lstIDs, tBMSDemandVoltage, BMSType, type);
            }
        }

        /// <summary>
        /// 设置主窗体按钮状态
        /// </summary>
        public static event SetUIButtonHandler SetUIButtonEvent;
        /// <summary>
        /// 设置主窗体按钮可用状态
        /// </summary>
        /// <param name="Enable">菜单栏按钮是否可用</param>
        /// <param name="btnStopEnalbe">停止检测按钮是否可用（字符串"false"不可用，"true"可用）</param>
        public static void SetUIButton(bool Enable, string btnStopEnalbe = null)
        {
            if (SetUIButtonEvent != null)
            {
                SetUIButtonEvent(Enable, btnStopEnalbe);
            }
        }
        /// <summary>
        /// 设置检测项是否全部选中
        /// </summary>
        public static event SetAllTestItemsCheckHandler SetAllTestItemsCheckEvent;
        /// <summary>
        /// 设置检测项是否全选
        /// </summary>
        /// <param name="isAllCheck"></param>
        public static void SetAllTestItemsCheck(bool isAllCheck)
        {
            if (SetAllTestItemsCheckEvent != null)
            {
                SetAllTestItemsCheckEvent(isAllCheck);
            }
        }

        /// <summary>
        /// 发送坐标宽度和高度
        /// </summary>
        public static event SendLocation EventLocationEvent;
        /// <summary>
        /// 发送坐标
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <param name="h">高度</param>
        /// <param name="w">宽度</param>
        public static void SendLocationAndWH(int x, int y, int h, int w, int id, string ComName)
        {
            if (EventLocationEvent != null)
            {
                EventLocationEvent(x, y, h, w, id, ComName);
            }
        }
        /// <summary>
        /// 界面发送端口名
        /// </summary>
        public static event SendComNameHandler ComNameEvent;

        /// <summary>
        /// 界面发送端口名
        /// </summary>
        public static void SendComText(int id, string strComName)
        {
            if (ComNameEvent != null)
            {
                ComNameEvent(id, strComName);
            }
        }
        /// <summary>
        /// 发送试验数据集(加载试验数据到界面显示)
        /// </summary>
        /// <param name="LstTrialData"></param>
        public static event SendTrialDataToUI EventSendTrialDataToUI;


        /// <summary>
        /// 发送试验数据集(加载试验数据到界面显示)
        /// </summary>
        /// <param name="LstTrialData"></param>
        public static void SendTrialDataToUIEvent(List<TrialDataModel> LstTrialData)
        {
            if (EventSendTrialDataToUI != null)
            {
                EventSendTrialDataToUI(LstTrialData);
            }
        }
        /// <summary>
        /// 接收检定项切换后跳转事件
        /// </summary>
        public static event SwitchCheckItemIndexHandler SwitchCheckItemIndexEvent;
        /// <summary>
        /// 接收检定项切换后跳转事件
        /// </summary>
        public static void SwitchCheckItemIndex(int index)
        {
            if (SwitchCheckItemIndexEvent != null)
            {
                SwitchCheckItemIndexEvent(index);
            }
        }

        public static event SaveTrialDataHandler SaveTrialDataEvent;

        public static void SaveTrialData()
        {
            if (SaveTrialDataEvent != null)
            {
                SaveTrialDataEvent();
            }
        }

        public static event SendTrialResultToUIHandler SendTrialResultToUIEvent;
        /// <summary>
        /// 发送检测最终结论到UI
        /// </summary>
        public static void SendTrialResult(EmTrialResult emTrialResult)
        {
            SendTrialResultToUIEvent?.Invoke(emTrialResult);
        }
        /// <summary>
        /// 切换语言事件
        /// </summary>
        public static event SendChangeLanguageHandler SendChangeLanguageEvent;
        /// <summary>
        /// 切换语言事件
        /// </summary>
        public static void SendChangeLanguage()
        {
            SendChangeLanguageEvent?.Invoke();
        }

        /// <summary>
        /// Can协议版本切换事件
        /// </summary>
        public static Action<ESGBDC_Ver> CanProtocolVersionSW;
        /// <summary>
        /// 发送测试状态
        /// </summary>
        /// <param name="emTrialState">测试状态类型</param>
        public static event SendTrialStateHandler SendTrialStateEvent;
        /// <summary>
        /// 发送测试状态
        /// </summary>
        /// <param name="emTrialState">测试状态类型</param>
        public static void SendTrialState(EmTrialState emTrialState)
        {
            SendTrialStateEvent?.Invoke(emTrialState);
        }

        public static event RevCANPacketHandler RevCANPacketEvent;
        public static void RevCANPacket(Dictionary<int, CanMsgRich> dicPacket)
        {
            RevCANPacketEvent?.Invoke(dicPacket);
        }

        public static event RevEUMsgHandler RevEUMsgEvent;
        public static void RevEUMsg(Dictionary<int, string> EU_Msg)
        {
            RevEUMsgEvent?.Invoke(EU_Msg);
        }

        public static event CreateCANExcelNameHandler CreateCANExcelNameEvent;

        public static void CreateCANExcelName(string TestItemName)
        {
            CreateCANExcelNameEvent?.Invoke(TestItemName);
        }

        public static event CreateCANExcelandler CreateCANExcelEvent;

        public static void CreateCANExcel()
        {
            CreateCANExcelEvent?.Invoke();
        }

        public static event MessageInfoHandler MessageInfoEvent;

        public static void MessageInfo(bool IsShow,string text,bool Confrim=false)
        {
            MessageInfoEvent?.Invoke(IsShow,text,Confrim);
        }

        public static event GetMessageInfoHandler GetMessageInfoEvent;


        public static bool GetMessageInfo()
        {               
           return GetMessageInfoEvent.Invoke();
        }


        public static event SetBMSHandler SetBMSEvent;
        public static void SetBMS(bool isShow = true)
        {
            SetBMSEvent?.Invoke(isShow);
        }

        public static event SetDtHandler SetDtEvent;

        public static void SetDt(int TestID)
        {
            SetDtEvent?.Invoke(TestID);
        }

        public static event SetChargerInfoHandler SetChargerInfoEvent;

        public static void SetChargerInfo(string Barcode, int SchemeIndex)
        {
            SetChargerInfoEvent?.Invoke(Barcode, SchemeIndex);
        }

        public static event SetSchemeHandler SetSchemeEvent;

        public static void SetScheme(int SchemeIndex)
        {
            SetSchemeEvent?.Invoke(SchemeIndex);
        }

        public static event StartTestHandler StartTestEvent;

        public static void StartTest()
        {
            StartTestEvent?.Invoke();
        }

        public static event StopTestHandler StopTestEvent;

        public static void StopTest()
        {
            StopTestEvent?.Invoke();
        }

    }
}
