using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.CAN;
using SaiTer.ATE.DataModel.DataBaseModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.InterFace
{
    /// <summary>
    /// 设备返回结果数据集
    /// </summary>
    /// <param name="st">设备结果集</param>
    public delegate void ResultDataEventHandler(StResultData st);
    /// <summary>
    /// 发送检定系统当前检定项目ID号
    /// </summary>
    /// <param name="ID"></param>
    public delegate void OnSendCheckItemID(int ID);

    /// <summary>
    /// 发送检定系统当前检定项目
    /// </summary>
    /// <param name="isTestItems">true-由勾选测试项触发   false-由选择方案名称触发</param>
    public delegate void OnSendCheckItems(List<StTrialItem> lstTrialItems, bool isTestItems, string strSchemeName = null);
    /// <summary>
    /// 发送监视器
    /// </summary>
    /// <param name="monitorDada"></param>
    public delegate void SendMonitorMessageHandler(object monitorDada);

    /// <summary>
    /// 发送设备连接状态
    /// </summary>
    /// <param name="isConnect">连接状态</param>
    /// <param name="obj">设备对象</param>
    public delegate void SendConnectStateHandler(bool isConnect, object obj);
    /// <summary>
    /// 倒计时窗体
    /// </summary>
    /// <param name="info">提示信息</param>
    /// <param name="time">倒计时时间（秒）</param>
    /// <param name="type">提示类型 0-纯倒计时提示信息。 1-倒计时等待选择</param>
    /// <param name="tag">输入数据的默认值</param>
    public delegate void SendCountDownTimerHandler(string info, int time, int type, string tag = "");
    /// <summary>
    /// 接收倒计时结束结果
    /// </summary>
    public delegate void SendCountDownTimerResultHandler(bool result);
    /// <summary>
    /// 人工确认结果(枪位号，合格结论)
    /// </summary>
    public delegate void SendManualVerifyResultHandler(Dictionary<int, bool> dicReuslt);


    /// <summary>
    /// 发送UI输入的数据
    /// </summary>
    /// <param name="value"></param>
    public delegate void SendInputDataHandler(string value);

    /// <summary>
    /// 发送等待刷卡
    /// </summary>
    /// <param name="lstIDS"></param>
    /// <param name="tBMSDemandVoltage"></param>
    /// <param name="BMSType"></param>
    /// <param name="type">t弹窗类型 0：等待刷卡   1：插枪检测</param>
    public delegate void SendWaitSwipingCardHandler(List<int> lstIDS, double tBMSDemandVoltage, EmChargerType BMSType, int type);

    /// <summary>
    /// 发送日志到UI显示
    /// </summary>
    /// <param name="logMsg">日志内容</param>
    public delegate void SendLogMessageHandler(string logMsg);

    /// <summary>
    /// 检测结果发送到UI
    /// </summary>
    /// <param name="TrialData">结果集</param>  
    public delegate void SendTestResultToUIHandler(TrialDataModel TrialData, int chargerID = 1, bool isClear = false, int TrialIndex = -1);
    /// <summary>
    /// 检测详细数据发送到UI
    /// </summary>
    /// <param name="TrialData">结果集</param>
    public delegate void SendDataMessageToUIHandler(TrialDataModel TrialData, bool isClear = false);

    /// <summary>
    /// 设置主窗体按钮状态
    /// </summary>
    /// <param name="Enable"></param>
    public delegate void SetUIButtonHandler(bool Enable, string btnStopEnalbe = null);

    public delegate void SetAllTestItemsCheckHandler(bool isCheckAll);

    /// <summary>
    /// 发送坐标和高度宽度
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="h"></param>
    /// <param name="w"></param>
    public delegate void SendLocation(int x, int y, int h, int w, int id, string ComName);

    /// <summary>
    /// 发送端口名
    /// </summary>
    /// <param name="ComName"></param>
    public delegate void SendComNameHandler(int id, string ComName);
    /// <summary>
    /// 发送试验数据集(加载试验数据到界面显示)
    /// </summary>
    /// <param name="LstTrialData"></param>
    public delegate void SendTrialDataToUI(List<TrialDataModel> LstTrialData);

    /// <summary>
    /// 切换检测项的索引号
    /// </summary>
    public delegate void SwitchCheckItemIndexHandler(int index);

    /// <summary>
    /// 保存试验数据到正式表
    /// </summary>
    public delegate void SaveTrialDataHandler();

    /// <summary>
    /// 最终检定结论发送到UI
    /// </summary>
    public delegate void SendTrialResultToUIHandler(EmTrialResult emTrialResult);

    /// <summary>
    /// 切换语言
    /// </summary>
    public delegate void SendChangeLanguageHandler();

    /// <summary>
    /// 发送测试状态
    /// </summary>
    /// <param name="emTrialState">测试状态类型</param>
    public delegate void SendTrialStateHandler(EmTrialState emTrialState);

    /// <summary>
    /// 接收CAN报文
    /// </summary>
    /// <param name="emTrialState">CAN报文数据</param>
    public delegate void RevCANPacketHandler(Dictionary<int, CanMsgRich> dicPacket);

    /// <summary>
    /// 接收欧标报文
    /// </summary>
    /// <param name="EU_Msg">欧标报文数据</param>
    public delegate void RevEUMsgHandler(Dictionary<int, string> EU_Msg);

    /// <summary>
    /// 传递单项的测试名称
    /// </summary>
    /// <param name="TestItemName"></param>
    public delegate void CreateCANExcelNameHandler(string TestItemName);


    /// <summary>
    /// 传递单项的测试名称
    /// </summary>
    /// <param name="TestItemName"></param>
    public delegate void CreateCANExcelandler();



    /// <summary>
    /// 单纯提示消息
    /// </summary>
    /// <param name="IsShow">是否显示</param>
    /// <param name="Info">提示内容</param>
    ///  <param name="Confrim">是否显示确认按钮，默认不显示</param>
    public delegate void MessageInfoHandler(bool IsShow,string Info,bool Confrim=false);

    /// <summary>
    /// 判断是否显示
    /// </summary>
    public delegate bool GetMessageInfoHandler();

    /// <summary>
    /// 发送BMS全部参数
    /// </summary>
    public delegate void SetBMSHandler(bool isShow = true);


    /// <summary>
    /// 协议一致性对应的测试ID
    /// </summary>
    /// <param name="TestID"></param>
    public delegate void SetDtHandler(int TestID);

    /// <summary>
    /// 设置枪信息
    /// </summary>
    /// <param name="Barcode">测试SN码</param>
    public delegate void SetChargerInfoHandler(string Barcode, int SchemeIndex);

    /// <summary>
    /// 设置测试方案
    /// </summary>
    /// <param name="SchemeIndex">测试方案索引</param>
    public delegate void SetSchemeHandler(int SchemeIndex);

    /// <summary>
    /// 开始测试
    /// </summary>
    public delegate void StartTestHandler();

    /// <summary>
    /// 停止测试
    /// </summary>
    public delegate void StopTestHandler();
}
