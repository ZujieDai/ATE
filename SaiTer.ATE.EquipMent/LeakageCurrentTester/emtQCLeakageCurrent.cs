using NationalInstruments.VisaNS;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.EquipStateData;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 设备-齐充QC漏电流测试仪
    /// </summary>
    public class emtQCLeakageCurrent : EquipMentBase
    {
        public readonly object SynLock = new object();
        public static bool isWriting;
        public readonly object lockWirte = new object();
        public QCLeakageCurrent_StateData stateData = new QCLeakageCurrent_StateData();
        private LeakageCurrent_Protocol protocol = new LeakageCurrent_Protocol();
        public emtQCLeakageCurrent(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = "QC剩余电流保护测试仪";// "QC" + " " + LanguageManager.GetByKey("剩余电流保护测试仪");
        }

        private int viAddr = 0x03;
        private enum FUNCTION_E
        {
            R_Coil = 0x01,              // 读线圈寄存器 位   取得一组逻辑线圈的当前状态（ON/OFF )
            R_DiscreteInput = 0x02,     // 读离散输入寄存器 位   取得一组开关输入的当前状态（ON/OFF )
            R_Hold = 0x03,              // 读保持寄存器 整型、浮点型、字符型 在一个或多个保持寄存器中取得当前的二进制值
            R_Input = 0x04,             // 读输入寄存器 整型、浮点型 在一个或多个输入寄存器中取得当前的二进制值
            W_SingleCoil = 0x05,        // 写单个线圈寄存器 位   强置一个逻辑线圈的通断状态
            W_SingleHold = 0x06,        // 写单个保持寄存器 整型、浮点型、字符型 把具体二进值装入一个保持寄存器
            W_MultipleCoil = 0x0F,      // 写多个线圈寄存器 位   强置一串连续逻辑线圈的通断
            W_MultipleHold = 0x10,      // 写多个保持寄存器 整型、浮点型、字符型 把具体的二进制值装入一串连续的保持寄存器
        }

        /// <summary>
        /// 数据组包
        /// </summary>
        /// <param name="vRegister">标识符</param>
        /// <param name="vbtData">数据内容</param>
        /// <returns>数据帧数组数据</returns>
        private List<byte> Pack(byte vAddr, FUNCTION_E vFunction, int vRegister, int vRegisterNum, List<byte> vbtData)
        {
            List<byte> wrDataBuffer = new List<byte>
            {
                vAddr, // Address
                (byte)vFunction, // Function
                (byte)(vRegister >> 8), // Register high
                (byte)vRegister, // Register low
                (byte)(vRegisterNum >> 8), // RegisterNum high / Write Single Data High
                (byte)(vRegisterNum), // RegisterNum low / Write Single Data Low
            };

            if (vbtData != null && (vFunction == FUNCTION_E.W_MultipleCoil || vFunction == FUNCTION_E.W_MultipleHold))
            {
                // 写多个寄存器的时候，需要带长度和数据
                wrDataBuffer.Add(((byte)vbtData.Count));
                wrDataBuffer.AddRange(vbtData);
            }

            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(wrDataBuffer.ToArray());//CRC校验函数。
            wrDataBuffer.AddRange(CheckSumByte);//把校验压入list

            return wrDataBuffer;
        }

        /// <summary>
        /// 设置漏电测试参数
        /// </summary>
        /// <param name="_TestType">测试类型：0，无效；1，漏电脱口电流；2，漏电突现时间；3，漏电闭合时间</param>
        /// <param name="_WaveType">电流波形：0=AC;1=10mA;2=30mA;3=50mA;4=100mA;5=300mA;6=500mA;7=1A;8=3A;9=5A;10=最大电流档位</param>
        /// <param name="_CurrentFreq">电流频率(Hz)：0=50;1=60;2=150;3=400;4=700;5=1K;6=2K;7=3K;</param>
        /// <param name="_InteruptType">触发模式：0=外部触发；1=内部触发</param>
        /// <param name="_LoadLine">剩余电流加载相线： 0=N； 1=L1；2=L2；3=L3</param>
        /// <param name="_OutCurrent">直接输出电流值：0~30000</param>
        /// <param name="_DCAddMode">直流叠加模式：0=关闭；1=正向叠加；2=负向叠加；</param>
        /// <param name="_DCAddCurrent">直流叠加电流值：0~15000</param>
        /// <param name="_CurrentEnableTime">电流使能时间：10~60000 (ms)</param>
        /// <param name="_StartCurrent"></param>
        /// <param name="_EndCurrent"></param>
        /// <param name="_TestTime"></param>
        /// <param name="_CurrentNP"></param>
        /// <returns></returns>
        public override void Leakage_SetParameters(int _TestType,
            int _WaveType, int _CurrentFreq, int _InteruptType, int _LoadLine,
            int _OutCurrent, int _DCAddMode, int _DCAddCurrent, int _CurrentEnableTime,
            int _StartCurrent, int _EndCurrent, int _TestTime, int _CurrentNP)
        {
            bool bRet = true;
            try
            {
                byte _testMode = (byte)((_TestType == 1) ? 1 : 0);
                int _currentLevel = 0;
                int[] _ranges = new int[10] { 0, 10, 30, 50, 100, 300, 500, 1000, 3000, 5000 };

                if (_testMode == 1)
                {
                    for (int i = 9; i >= 0; i--)
                    {
                        // 电流爬升模式
                        if (_StartCurrent > _ranges[i] || _EndCurrent > _ranges[i])
                        {
                            _currentLevel = i + 1;
                            break;
                        }
                    }
                }
                else
                {
                    for (int i = 9; i >= 0; i--)
                    {
                        if (_OutCurrent > _ranges[i])
                        {
                            _currentLevel = i + 1;
                            break;
                        }
                    }
                }

                int _currentFactor = (_currentLevel > 4) ? 1 : 10;
                UInt16 __OutCurrent = Convert.ToUInt16(_OutCurrent * _currentFactor);
                UInt16 __DCAddMode = Convert.ToUInt16(_DCAddMode * _currentFactor);
                UInt16 __StartCurrent = Convert.ToUInt16(_StartCurrent * _currentFactor); 
                UInt16 __EndCurrent = Convert.ToUInt16(_EndCurrent * _currentFactor);
                List<byte> bytes = new List<byte>()
                {
                    //上位机端控制指令
                    //40050 设备模式选择 0 = 手动模式 1 = 自动模式 0
                    0x00, 0x01,
                    //40051 电压使能 0 = 电压断开 1 = 电压使能 0
                    0x00, 0x00,
                    //40052 电流使能 0 = 电流断开 1 = 电流使能 0
                    0x00, 0x00,
                    //40053 直接输出电流值 0~30000 (具体设置规则见规则页) 0
                    (byte)(__OutCurrent >> 8), (byte)(__OutCurrent),
                    //40054 (暂不启用) - -
                    0x00, 0x00,
                    //40055 爬升模式起始电流 0~30000 (具体设置规则见规则页) 0
                    (byte)(__StartCurrent >> 8), (byte)(__StartCurrent),
                    //40056 爬升模式结束电流 0~30000 (具体设置规则见规则页) 0
                    (byte)(__EndCurrent >> 8), (byte)(__EndCurrent),
                    //40057 爬升模式爬升时间 100~60000 (ms) 30000
                    (byte)(_TestTime >> 8), (byte)(_TestTime),
                    //40058 (暂不启用) - -
                    0x00, 0x00,
                    //40059 电流测试模式 0 = 直接输出模式 1 = 电流爬升模式 0
                    (byte)(_testMode >> 8), (byte)(_testMode),
                    //40060 电流档位 1
                    //0 = (暂不启用)
                    //1 = 10mA
                    //2 = 30mA
                    //3 = 50mA
                    //4 = 100mA
                    //5 = 300mA
                    //6 = 500mA
                    //7 = 1A
                    //8 = 3A
                    //9 = 5A
                    //10 = 10A/20A/30A(由型号决定)
                    (byte)(_currentLevel >> 8), (byte)_currentLevel,
                    //40061 电流波形 0
                    //0 = AC
                    //1 = A0°
                    //2 = A90°
                    //3 = A135°
                    //4 = 2PDC(两相整流)
                    //5 = 3PDC(三相整流)
                    //6 = S-DC(平滑直流)
                    //7 = F-B(复合波10Hz/50Hz/1kH)
                    //8 = F-I(复合波50Hz/1kHz)
                    (byte)(_WaveType >> 8), (byte)(_WaveType),
                    //40062 AC电流频率 0
                    //0 = 50Hz
                    //1 = 60Hz
                    //2 = 150Hz
                    //3 = 400Hz
                    //4 = 700Hz
                    //5 = 1kHz
                    //6 = 2kHz
                    //7 = 3kHz
                    (byte)(_CurrentFreq >> 8), (byte)(_CurrentFreq),
                    //40063 电流极性 0 = 正 1 = 负 1
                    (byte)(_CurrentNP >> 8), (byte)(_CurrentNP),
                    //40064 触发模式 0 = 外部触发 1 = 内部触发 0
                    (byte)(_InteruptType >> 8), (byte)(_InteruptType),
                    //40065 直流叠加模式 0
                    //0 = 关闭
                    //1 = 正向叠加
                    //2 = 负向叠加
                    (byte)(_DCAddMode >> 8), (byte)(__DCAddMode),
                    //40066 直流叠加电流值 0~15000 (具体设置规则见规则页) 0
                    (byte)(_DCAddCurrent >> 8), (byte)(_DCAddCurrent),
                    //40067 剩余电流加载相线 0 = N / 1 = L1 / 2 = L2 / 3 = L3 1
                    (byte)(_LoadLine >> 8), (byte)(_LoadLine),
                    //40068 N线电压使能 0 = 不使能 / 1 = 使能 1
                    0x00, 0x01,
                    //40069 L1线电压使能 0 = 不使能 / 1 = 使能 1
                    0x00, 0x01,
                    //40070 L2线电压使能 0 = 不使能 / 1 = 使能 1
                    0x00, 0x01,
                    //40071 L3线电压使能 0 = 不使能 / 1 = 使能 1
                    0x00, 0x01,
                    //40072 电流使能时间 10~60000 (ms) (具体设置规则见规则页) 60000
                    (byte)(_CurrentEnableTime >> 8), (byte)(_CurrentEnableTime),
                };
                byte[] _wBytes = Pack((byte)viAddr, FUNCTION_E.W_MultipleHold, 50, 23, bytes).ToArray();
                byte[] ackBytes = null;
                bRet = SendData(_wBytes, out ackBytes);
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                bRet = false;
            }
        }

        public override void Leakage_EnableVolatge(int _enable)
        {
            bool bRet = true;
            try
            {
                List<byte> bytes = new List<byte>()
                {
                    //上位机端控制指令
                    //40051 电压使能 0 = 电压断开 1 = 电压使能 0
                    (byte)(_enable >> 8), (byte)(_enable)
                };
                byte[] _wBytes = Pack((byte)viAddr, FUNCTION_E.W_SingleHold, 51, 1, bytes).ToArray();
                byte[] ackBytes = null;
                bRet = SendData(_wBytes, out ackBytes);
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                bRet = false;
            }
        }

        public override void Leakage_EnableCurrent(int _enable)
        {
            bool bRet = true;
            try
            {
                List<byte> bytes = new List<byte>()
                {
                    //40052 电流使能 0 = 电流断开 1 = 电流使能 0
                    (byte)(_enable >> 8), (byte)(_enable),
                };
                byte[] _wBytes = Pack((byte)viAddr, FUNCTION_E.W_SingleHold, 52, 1, bytes).ToArray();
                byte[] ackBytes = null;
                bRet = SendData(_wBytes, out ackBytes);
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                bRet = false;
            }
        }

        public override void Leakage_StartTest(int _TestType, int _SnapTime)
        {
            bool bRet = true;
            try
            {
                if (_TestType == 3)
                {
                    Leakage_EnableCurrent(1);
                    Thread.Sleep(_SnapTime);
                    Leakage_EnableVolatge(1);
                }
                else
                {
                    Leakage_EnableVolatge(1);
                    Thread.Sleep(_SnapTime);
                    Leakage_EnableCurrent(1);
                }
                //1、测量电流爬升时【测试类型：漏电脱扣电流】
                //电流模式选择“电流爬升”
                //先点击“电压使能”
                //被测设备上电后
                //点击“电流使能”,
                //测试完成后，测试结果里会显示分断电流。 

                //2、测量突现时间时【测试类型：漏电突现时间】
                //电流模式选择“直接输出”
                //先点击“电压使能”
                //被测设备上电后
                //点击“电流使能”
                //测试完成后，测试结果里会显示分断时间。 

                //3、测量闭合时间时【测试类型：漏电闭合时间】
                //电流模式选择“直接输出”
                //先点击“电流使能”
                //然后点击“电压使能”
                //测试完成后，测试结果里会显示分断时间。
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                bRet = false;
            }
        }

        private bool SendData(byte[] WriteBuffer, out byte[] RevMsgData)
        {
            try
            {
                RevMsgData = new byte[] { };
                if (!isWriting)
                {
                    lock (lockWirte)
                    {
                        if (!isWriting)
                        {
                            isWriting = true;
                            AutoReadData = false;
                            Thread.Sleep(300);
                            if (EquipMentPort != null)
                            {

                                for (int i = 0; i < ReConnNum; i++)
                                {
                                    DataBuf.Clear();
                                    string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                                    SendMsgToFile(EquipMentName + "发送数据：" + strTemp);
                                    EquipMentPort.SendData(WriteBuffer);
                                    RevMsgData = RevEquipMentData();
                                    if (RevMsgData != null)
                                    {
                                        List<byte> nTemp = RevMsgData.ToList();

                                        if (CheckOut.CheckModbusCrc16_High_Right(RevMsgData) == false)//如果接收的数据CRC校验失败，则发送第二次
                                        {
                                            EquipMentPort.SendData(WriteBuffer);
                                        }
                                    }
                                    else
                                    {
                                        SendMsgToFile(EquipMentName + "发送数据：" + strTemp);
                                        SendMsgToFile(EquipMentName + "设置参数信息失败，设备没返回数据！");
                                        continue;
                                    }
                                }
                                AutoReadData = true;
                                return true;
                            }
                            else
                            {
                                SendMsgToFile(EquipMentName + "通道对象不存在，请检查");
                                AutoReadData = true;
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                RevMsgData = new byte[] { };
                return false;
            }
            finally
            {
                AutoReadData = true;
                isWriting = false;
            }
            return false;
        }

        public override void LeakageCurrent_ReadState()
        {
            //  SystemEvent.SendConnectState(false, this);
            if (!AllEquipStateData.DicQCLeakageCurrent_StateData.ContainsKey(ChargerID))
            {
                AllEquipStateData.DicQCLeakageCurrent_StateData.Add(ChargerID, stateData);
            }
            QCLeakageCurrent_StateData StateData = new QCLeakageCurrent_StateData();

            while (true)
            {
                lock (SynLock)
                {
                    Thread.Sleep(10);
                    if (AutoReadData)  //自动读数据
                    {
                        byte[] RevMsgData = null;
                        //刷新测试结果状态
                        byte[] WriteBuffer = { 0x03, 0x03, 0x00, 0xAB, 0x00, 0x09, 0xF5, 0xCE };//读取个9寄存器，包括CRC
                        if (EquipMentPort != null)
                        {
                            for (int i = 0; i < ReConnNum; i++)
                            {
                                DataBuf.Clear();
                                string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                                //SendMsgToFile(EquipMentName+"发送数据：" + strTemp);
                                if (isWriting)
                                    continue;
                                lock (lockWirte)
                                {
                                    if (isWriting)
                                        continue;
                                    EquipMentPort.SendData(WriteBuffer);
                                    Thread.Sleep(200);
                                    RevMsgData = RevEquipMentData();
                                }
                                if (RevMsgData != null)
                                {
                                    List<byte> nTemp = RevMsgData.ToList();
                                    if (CheckOut.CheckModbusCrc16_High_Right(RevMsgData) == false)//如果接收的数据CRC校验失败，则发送第二次
                                    {
                                        if (isWriting)
                                            continue;
                                        lock (lockWirte)
                                        {
                                            if (isWriting)
                                                continue;
                                            EquipMentPort.SendData(WriteBuffer);
                                            Thread.Sleep(200);
                                            RevMsgData = RevEquipMentData();
                                        }
                                    }
                                    StateData = protocol.GetQCLeakageCurrent_StateData(RevMsgData, this.ChargerID);
                                    //SystemEvent.SendMonitorMessage(StateData);
                                    //SystemEvent.SendConnectState(true, this);
                                }
                                else
                                {
                                    //SendMsgToFile(EquipMentName+"读状态数据失败，设备没返回数据！");
                                    StateData = new QCLeakageCurrent_StateData();
                                    StateData.ChargerID = this.ChargerID;
                                    //SystemEvent.SendMonitorMessage(StateData);
                                    //SystemEvent.SendConnectState(false, this);
                                    continue;
                                }

                            }
                        }
                        else
                        {
                            SendMsgToFile(EquipMentName + "通道对象不存在，请检查");
                            StateData = new QCLeakageCurrent_StateData();
                            StateData.ChargerID = this.ChargerID;
                            //SystemEvent.SendConnectState(false, this);
                            //SystemEvent.SendMonitorMessage(StateData);
                        }

                        //刷新运行状态
                        WriteBuffer = new byte[] { 0x03, 0x03, 0x00, 0x97, 0x00, 0x05, 0x35, 0xC7 };//读取个5寄存器，包括CRC
                        if (EquipMentPort != null)
                        {
                            for (int i = 0; i < ReConnNum; i++)
                            {
                                DataBuf.Clear();
                                string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                                //SendMsgToFile(EquipMentName+"发送数据：" + strTemp);
                                if (isWriting)
                                    continue;
                                lock (lockWirte)
                                {
                                    if (isWriting)
                                        continue;
                                    EquipMentPort.SendData(WriteBuffer);
                                    Thread.Sleep(200);
                                    RevMsgData = RevEquipMentData();
                                }

                                if (RevMsgData != null)
                                {
                                    List<byte> nTemp = RevMsgData.ToList();
                                    if (CheckOut.CheckModbusCrc16_High_Right(RevMsgData) == false)//如果接收的数据CRC校验失败，则发送第二次
                                    {
                                        if (isWriting)
                                            continue;
                                        lock (lockWirte)
                                        {
                                            if (isWriting)
                                                continue;
                                            EquipMentPort.SendData(WriteBuffer);
                                            Thread.Sleep(200);
                                            RevMsgData = RevEquipMentData();
                                        }
                                    }
                                    StateData = protocol.GetQCLeakageCurrent_RunStateData(RevMsgData, this.ChargerID, StateData);
                                    SystemEvent.SendMonitorMessage(StateData);
                                    SystemEvent.SendConnectState(true, this);
                                }
                                else
                                {
                                    //SendMsgToFile(EquipMentName+"读状态数据失败，设备没返回数据！");
                                    StateData = new QCLeakageCurrent_StateData();
                                    StateData.ChargerID = this.ChargerID;
                                    SystemEvent.SendMonitorMessage(StateData);
                                    SystemEvent.SendConnectState(false, this);
                                    continue;
                                }

                            }
                        }
                    }
                    Thread.Sleep(200);
                }
            }
        }

    }
}
