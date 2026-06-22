using SaiTer.ATE.DataModel.EquipStateData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 漏电测试仪协议
    /// </summary>
    public class LeakageCurrent_Protocol
    {
        public byte[] GetBuffer(int address, int param, int cmd = -1)
        {
            try
            {
                List<byte> ReturnbyteSource = new List<byte>();
                byte[] PrefixCode = new byte[] { 0x01, 0x06 };//前缀
                if (cmd==0x03)//读
                {
                    PrefixCode[1] = 0x03;
                }
                ReturnbyteSource.AddRange(PrefixCode);//前缀压入list

                ReturnbyteSource.Add((byte)(address >> 8));//寄存器地址高位
                ReturnbyteSource.Add((byte)address);//寄存器地址低位

                ReturnbyteSource.Add((byte)(param >> 8));//写入的数据 高位
                ReturnbyteSource.Add((byte)param);//写入的数据 低位

                byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
                byte[] CheckSumByte = CheckOut.ModBus_RTU_CRC16(writeBuff);//CRC校验函数。
                ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
                byte[] WriteBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
                return WriteBuffer;
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                return null;
            }
        }


        public byte[] QCGetBuffer(int address, int param)
        {
            try
            {
                List<byte> ReturnbyteSource = new List<byte>();
                byte[] PrefixCode = new byte[] { 0x03, 0x06 };//前缀

                ReturnbyteSource.AddRange(PrefixCode);//前缀压入list

                ReturnbyteSource.Add((byte)(address >> 8));//寄存器地址高位
                ReturnbyteSource.Add((byte)address);//寄存器地址低位

                ReturnbyteSource.Add((byte)(param >> 8));//写入的数据 高位
                ReturnbyteSource.Add((byte)param);//写入的数据 低位

                byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
                byte[] CheckSumByte = CheckOut.ModBus_RTU_CRC16(writeBuff);//CRC校验函数。
                ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
                byte[] WriteBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
                return WriteBuffer;
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                return null;
            }
        }


        public byte[] QCSendBuffer(int address, byte[] param)  //0x10  设置多个参数
        {
            try
            {
                List<byte> ReturnbyteSource = new List<byte>();
                byte[] PrefixCode = new byte[] { 0x03, 0x10 };//前缀

                ReturnbyteSource.AddRange(PrefixCode);//前缀压入list

                ReturnbyteSource.Add((byte)(address >> 8));//寄存器地址高位
                ReturnbyteSource.Add((byte)address);//寄存器地址低位


                int StartAddr = param.Length / 2;

                ReturnbyteSource.Add((byte)(StartAddr >> 8));//写入 起始地址
                ReturnbyteSource.Add((byte)StartAddr);      //      起始地址

                ReturnbyteSource.Add((byte)param.Length);//写入的数据个数 长度

                ReturnbyteSource.AddRange(param);



                byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
                byte[] CheckSumByte = CheckOut.ModBus_RTU_CRC16(writeBuff);//CRC校验函数。
                ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
                byte[] WriteBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
                return WriteBuffer;
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                return null;
            }
        }

        /// <summary>
        /// 解析中佳漏电流测试仪实时数据
        /// </summary>
        /// <param name="buffer">报文</param>
        /// <param name="chargerID">枪编号</param>
        /// <returns></returns>
        public ZJLeakageCurrent_StateData GetZJLeakageCurrent_StateData(byte[] buffer, int chargerID)
        {
            ZJLeakageCurrent_StateData state = new ZJLeakageCurrent_StateData();
            state.ChargerID = chargerID;
            try
            {
                if (CheckOut.CheckModbusCrc16_High_Right(buffer))
                {
                    int data = Convert.ToInt32(buffer[13].ToString("x2") + buffer[14].ToString("x2"), 16);
                    state.NowVoltage = Convert.ToSingle(data) / 10;

                    data = Convert.ToInt32(buffer[15].ToString("x2") + buffer[16].ToString("x2"), 16);
                    state.NowCurrent = Convert.ToSingle(data) / 10;


                    data = Convert.ToInt32(buffer[17].ToString("x2") + buffer[18].ToString("x2"), 16);
                    state.TestCurrent = Convert.ToSingle(data) / 10;


                    data = Convert.ToInt32(buffer[5].ToString("x2") + buffer[6].ToString("x2"), 16);
                    state.TestTime = Convert.ToSingle(data) / 10;

                    uint AlarmInfo = buffer[3];
                    AlarmInfo = (uint)AlarmInfo << 8;
                    AlarmInfo = Convert.ToUInt16(AlarmInfo + buffer[4]);

                    //报警显示
                    if (AlarmInfo == 0x0000)
                    {
                        state.AlarmInfo = "复位OK";
                    }
                    else if (AlarmInfo == 0x0064)
                    {
                        state.AlarmInfo = "正常";
                    }
                    else
                    {
                        state.AlarmInfo = "异常";
                    }

                    uint Phase = buffer[19];
                    Phase = (uint)Phase << 8;
                    Phase = Convert.ToUInt16(Phase + buffer[20]);
                    // RST(ABC)三相切换
                    if (Phase == 0x0000)
                    {
                        state.Phase = "A相";
                    }
                    else if (Phase == 0x0001)
                    {
                        state.Phase = "B相";
                    }
                    else if (Phase == 0x0002)
                    {
                        state.Phase = "C相";
                    }
                    uint S3_State = buffer[7];
                    S3_State = (uint)S3_State << 8;
                    S3_State = Convert.ToUInt16(S3_State + buffer[8]);

                    // S3开关状态
                    if (S3_State == 0x0001)
                    {
                        state.S3_State = "闭合";
                    }
                    else if (S3_State == 0x0000)
                    {
                        state.S3_State = "断开";
                    }
                    uint S2_State = buffer[29];
                    S2_State = (uint)S2_State << 8;
                    S2_State = Convert.ToUInt16(S2_State + buffer[30]);

                    // S2开关状态
                    if (S2_State == 0x0001)
                    {
                        state.S2_State = "闭合";
                    }
                    else if (S2_State == 0x0000)
                    {
                        state.S2_State = "断开";
                    }

                    uint S1_State = buffer[31];
                    S1_State = (uint)S1_State << 8;
                    S1_State = Convert.ToUInt16(S1_State + buffer[32]);

                    // S1开关状态
                    if (S1_State == 0x0001)
                    {
                        state.S1_State = "闭合";
                    }
                    else if (S1_State == 0x0000)
                    {
                        state.S1_State = "断开";
                    }
                    uint PresetCurrent = buffer[21];
                    PresetCurrent = (uint)PresetCurrent << 8;
                    PresetCurrent = Convert.ToUInt16(PresetCurrent + buffer[22]);

                    //预先调整动作电流
                    if (PresetCurrent == 0x0000)
                    {
                        state.PresetCurrent = "完成";
                    }
                    else if (PresetCurrent == 0x0002)
                    {
                        state.PresetCurrent = "调整中";
                    }

                }

            }
            catch (Exception ex)
            {

            }

            return state;
        }
		
        /// <summary>
        /// 解析齐充QC漏电流测试仪实时数据
        /// </summary>
        /// <param name="buffer">报文</param>
        /// <param name="chargerID">枪编号</param>
        /// <returns></returns>
        public QCLeakageCurrent_StateData GetQCLeakageCurrent_StateData(byte[] buffer, int chargerID)
        {
            QCLeakageCurrent_StateData state = new QCLeakageCurrent_StateData();
            state.ChargerID = chargerID;
            try
            {
                if (CheckOut.CheckModbusCrc16_High_Right(buffer))
                {
                    int _index = 3;
                    state.TestMode = (emTestMode)((buffer[_index] << 8) | buffer[_index + 1]);
                    _index += 2;
                    //_currentLevel = (buffer[_index] << 8) | buffer[_index + 1];
                    _index += 2;
                    //_interruptMode = (buffer[_index] << 8) | buffer[_index + 1];
                    _index += 2;
                    state.TripCurrent = (double)((buffer[_index] << 8) | buffer[_index + 1]) / 10.0;
                    _index += 2;
                    state.TripTime = (float)((buffer[_index] << 8) | buffer[_index + 1]);
                    _index += 2;

                    _index += 2;
                    int _baseTime = (buffer[_index] << 8) | buffer[_index + 1];
                    if (_baseTime == 0)
                    {
                        state.TripTime /= 10.0f;
                    }
                    _index += 2;
                    state.TestResult = (buffer[_index] << 8) | buffer[_index + 1];
                }
            }
            catch(Exception ex)
            {
                Log.Log.LogException(ex);
            }
            return state;
        }

        /// <summary>
        /// 解析齐充QC漏电流测试仪实时数据
        /// </summary>
        /// <param name="buffer">报文</param>
        /// <param name="chargerID">枪编号</param>
        /// <returns></returns>
        public QCLeakageCurrent_StateData GetQCLeakageCurrent_RunStateData(byte[] buffer, int chargerID, QCLeakageCurrent_StateData state)
        {
            state.ChargerID = chargerID;
            try
            {
                if (CheckOut.CheckModbusCrc16_High_Right(buffer))
                {
                    int _index = 3;
                    state.EnableVoltage = (buffer[_index] << 8) | buffer[_index + 1];
                    _index += 2;
                    state.EnableCurrent = (buffer[_index] << 8) | buffer[_index + 1];
                    _index += 2;
                    state.TestSW = (buffer[_index] << 8) | buffer[_index + 1];
                    _index += 2;
                    state.RunTime = (buffer[_index] << 8) | buffer[_index + 1];
                    _index += 2;
                    state.RemoteStatus = (buffer[_index] << 8) | buffer[_index + 1];
                    _index += 2;
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
            return state;
        }
    }
}
