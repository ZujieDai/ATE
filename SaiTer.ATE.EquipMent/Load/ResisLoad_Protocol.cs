using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.EquipStateData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.EquipMent.Load
{
    /// <summary>
    /// 电阻负载协议
    /// </summary>
    public class ResisLoad_Protocol
    {
        /////////////////////////赛特交直流负载/////////////////////////////
        //交流
        public byte[] LoadSetVoltage(Byte tFunc, Byte tOperator, Double tData)
        {
            List<byte> ReturnbyteSource = new List<byte>
            {
                0x68,//起始符
                0x00,//帧长度低
                0x00,//帧长度高
                0x68,//起始符
                0x80,//地址
                tFunc,//功能
                tOperator//操作
            };

            UInt32 tempdata = Convert.ToUInt32(tData * 100);//数据
            ReturnbyteSource.Add((Byte)(tempdata >> 24));
            ReturnbyteSource.Add((Byte)(tempdata >> 16));
            ReturnbyteSource.Add((Byte)(tempdata >> 8));
            ReturnbyteSource.Add((Byte)(tempdata));
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte CheckSumByte = CheckOut.LoadCrc(writeBuff);//CRC校验函数。 
            ReturnbyteSource.Add(CheckSumByte);//把校验压入list
            ReturnbyteSource.Add(0x16);//尾部


            ReturnbyteSource[1] = (Byte)ReturnbyteSource.Count;///帧长度低字节，低位在前
            ReturnbyteSource[2] = (Byte)(ReturnbyteSource.Count >> 8);///帧长度高字节，高位在后

            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;

        }

        public byte[] LoadSetCurrent(Byte tFunc, Byte tOperator, Double tData, int Rate = 100)
        {
            try
            {
                List<byte> ReturnbyteSource = new List<byte>
            {
                0x68,//起始符
                0x00,//帧长度低
                0x00,//帧长度高
                0x68,//起始符
                0x80,//地址
                tFunc,//功能
                tOperator//操作
            };


                UInt32 tempdata = Convert.ToUInt32(tData * Rate);//数据
                tempdata = tempdata * 3;///3
                ReturnbyteSource.Add((Byte)(tempdata >> 24));
                ReturnbyteSource.Add((Byte)(tempdata >> 16));
                ReturnbyteSource.Add((Byte)(tempdata >> 8));
                ReturnbyteSource.Add((Byte)(tempdata));
                byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
                byte CheckSumByte = CheckOut.LoadCrc(writeBuff);//CRC校验函数。 
                ReturnbyteSource.Add(CheckSumByte);//把校验压入list
                ReturnbyteSource.Add(0x16);//尾部


                ReturnbyteSource[1] = (Byte)ReturnbyteSource.Count;///帧长度低字节，低位在前
                ReturnbyteSource[2] = (Byte)(ReturnbyteSource.Count >> 8);///帧长度高字节，高位在后

                byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
                return writeBuffer;
            }
            catch (Exception ex) { Log.Log.LogException(ex); return null; }

        }

        public byte[] LoadSetPower(Byte tFunc, Byte tOperator, Double tData)
        {
            List<byte> ReturnbyteSource = new List<byte>
            {
                0x68,//起始符
                0x00,//帧长度低
                0x00,//帧长度高
                0x68,//起始符
                0x80,//地址
                tFunc,//功能
                tOperator//操作
            };

            try
            {
                UInt32 tempdata = Convert.ToUInt32(tData * 100);//数据   精度0.01
                tempdata = tempdata * 3;
                ReturnbyteSource.Add((Byte)(tempdata >> 24));
                ReturnbyteSource.Add((Byte)(tempdata >> 16));
                ReturnbyteSource.Add((Byte)(tempdata >> 8));
                ReturnbyteSource.Add((Byte)(tempdata));
                byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
                byte CheckSumByte = CheckOut.LoadCrc(writeBuff);//CRC校验函数。 
                ReturnbyteSource.Add(CheckSumByte);//把校验压入list
                ReturnbyteSource.Add(0x16);//尾部


                ReturnbyteSource[1] = (Byte)ReturnbyteSource.Count;///帧长度低字节，低位在前
                ReturnbyteSource[2] = (Byte)(ReturnbyteSource.Count >> 8);///帧长度高字节，高位在后

                byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
                return writeBuffer;
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                return null;
            }
        }



        //直流
        public byte[] DCLoadSetVoltageCurrent(Double tVoltage, Double tCurrent)
        {
            List<byte> ReturnbyteSource = new List<byte>
            {
                0x68,//起始符
                0x00,//帧长度低
                0x00,//帧长度高
                0x68,//起始符
                0x80,//地址
                0x04,//功能
                0x01//操作
            };
            UInt32 tempdata = Convert.ToUInt32(tVoltage * 100);//数据
            ReturnbyteSource.Add((Byte)(tempdata >> 24));
            ReturnbyteSource.Add((Byte)(tempdata >> 16));
            ReturnbyteSource.Add((Byte)(tempdata >> 8));
            ReturnbyteSource.Add((Byte)(tempdata));


            ReturnbyteSource.Add(0x02);//操作
            tempdata = Convert.ToUInt32(tCurrent * 100);//数据
            ReturnbyteSource.Add((Byte)(tempdata >> 24));
            ReturnbyteSource.Add((Byte)(tempdata >> 16));
            ReturnbyteSource.Add((Byte)(tempdata >> 8));
            ReturnbyteSource.Add((Byte)(tempdata));

            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte CheckSumByte = CheckOut.LoadCrc(writeBuff);//CRC校验函数。 
            ReturnbyteSource.Add(CheckSumByte);//把校验压入list
            ReturnbyteSource.Add(0x16);//尾部


            ReturnbyteSource[1] = (Byte)ReturnbyteSource.Count;///帧长度低字节，低位在前
            ReturnbyteSource[2] = (Byte)(ReturnbyteSource.Count >> 8);///帧长度高字节，高位在后

            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;

        }
        public byte[] DCLoadSetVoltageResistance(Double tVoltage, Double tResistance)
        {
            List<byte> ReturnbyteSource = new List<byte>
            {
                0x68,//起始符
                0x00,//帧长度低
                0x00,//帧长度高
                0x68,//起始符
                0x80,//地址
                0x04,//功能
                0x01//操作
            };
            UInt32 tempdata = Convert.ToUInt32(tVoltage * 100);//数据
            ReturnbyteSource.Add((Byte)(tempdata >> 24));
            ReturnbyteSource.Add((Byte)(tempdata >> 16));
            ReturnbyteSource.Add((Byte)(tempdata >> 8));
            ReturnbyteSource.Add((Byte)(tempdata));


            ReturnbyteSource.Add(0x03);//操作
            tempdata = Convert.ToUInt32(tResistance * 100);//数据
            ReturnbyteSource.Add((Byte)(tempdata >> 24));
            ReturnbyteSource.Add((Byte)(tempdata >> 16));
            ReturnbyteSource.Add((Byte)(tempdata >> 8));
            ReturnbyteSource.Add((Byte)(tempdata));

            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte CheckSumByte = CheckOut.LoadCrc(writeBuff);//CRC校验函数。 
            ReturnbyteSource.Add(CheckSumByte);//把校验压入list
            ReturnbyteSource.Add(0x16);//尾部


            ReturnbyteSource[1] = (Byte)ReturnbyteSource.Count;///帧长度低字节，低位在前
            ReturnbyteSource[2] = (Byte)(ReturnbyteSource.Count >> 8);///帧长度高字节，高位在后

            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;

        }
        public byte[] DCLoadSetVoltagePower(Double tVoltage, Double tPower)
        {

            List<byte> ReturnbyteSource = new List<byte>
            {
                0x68,//起始符
                0x00,//帧长度低
                0x00,//帧长度高
                0x68,//起始符
                0x80,//地址
                0x04,//功能
                0x01//操作
            };
            UInt32 tempdata = Convert.ToUInt32(tVoltage * 100);//数据
            ReturnbyteSource.Add((Byte)(tempdata >> 24));
            ReturnbyteSource.Add((Byte)(tempdata >> 16));
            ReturnbyteSource.Add((Byte)(tempdata >> 8));
            ReturnbyteSource.Add((Byte)(tempdata));


            ReturnbyteSource.Add(0x04);//操作
            tempdata = Convert.ToUInt32(tPower * 100);//数据
            ReturnbyteSource.Add((Byte)(tempdata >> 24));
            ReturnbyteSource.Add((Byte)(tempdata >> 16));
            ReturnbyteSource.Add((Byte)(tempdata >> 8));
            ReturnbyteSource.Add((Byte)(tempdata));

            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte CheckSumByte = CheckOut.LoadCrc(writeBuff);//CRC校验函数。 
            ReturnbyteSource.Add(CheckSumByte);//把校验压入list
            ReturnbyteSource.Add(0x16);//尾部


            ReturnbyteSource[1] = (Byte)ReturnbyteSource.Count;///帧长度低字节，低位在前
            ReturnbyteSource[2] = (Byte)(ReturnbyteSource.Count >> 8);///帧长度高字节，高位在后

            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;

        }

        //交直流
        public byte[] LoadSet(Byte tFunc, Byte tOperator)//交直流
        {
            List<byte> ReturnbyteSource = new List<byte>
            {
                0x68,//起始符
                0x00,//帧长度低
                0x00,//帧长度高
                0x68,//起始符
                0x80,//地址
                tFunc,//功能
                tOperator//操作
            };

            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte CheckSumByte = CheckOut.LoadCrc(writeBuff);//CRC校验函数。 
            ReturnbyteSource.Add(CheckSumByte);//把校验压入list
            ReturnbyteSource.Add(0x16);//尾部


            ReturnbyteSource[1] = (Byte)ReturnbyteSource.Count;///帧长度低字节，低位在前
            ReturnbyteSource[2] = (Byte)(ReturnbyteSource.Count >> 8);///帧长度高字节，高位在后

            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;

        }
        /// <summary>
        /// 解析交流电阻负载实时数据
        /// </summary>
        /// <param name="buffer">报文</param>
        /// <param name="chargerID">枪编号</param>
        /// <returns></returns>
        public ResisLoad_StateData GetResisLoad_AC_StateData(byte[] buffer, int chargerID)
        {

            ResisLoad_StateData StateData = new ResisLoad_StateData();
            StateData.ChargerID = chargerID;
            StateData.emType = EmChargerType.Charger_GB_AC;
            StateData.EquipName = "交流电阻负载";
            if (CheckOut.CheckLoadCrc(buffer) && buffer.Length > 58)
            {
                int temp = Convert.ToInt32(buffer[7].ToString("x2") + buffer[8].ToString("x2") + buffer[9].ToString("x2") + buffer[10].ToString("x2"), 16);
                int num = 0;
                for (int K = 0; K < 32; K++)
                {
                    if ((temp & 0x0001) == 0x0001)
                    {
                        num++;
                    }
                    temp = temp >> 1;
                }
                StateData.OnlineEquip = num;
                StateData.DemandResis = Convert.ToSingle(Convert.ToInt32(buffer[11].ToString("x2") + buffer[12].ToString("x2") + buffer[13].ToString("x2") + buffer[14].ToString("x2"), 16)) / 100;
                StateData.DemandCurrent = Convert.ToSingle(Convert.ToInt32(buffer[15].ToString("x2") + buffer[16].ToString("x2") + buffer[17].ToString("x2") + buffer[18].ToString("x2"), 16)) / 100;
                StateData.DemandPower = Convert.ToSingle(Convert.ToInt32(buffer[19].ToString("x2") + buffer[20].ToString("x2") + buffer[21].ToString("x2") + buffer[22].ToString("x2"), 16)) / 100;
                StateData.ActualPower = Convert.ToSingle(Convert.ToInt32(buffer[23].ToString("x2") + buffer[24].ToString("x2") + buffer[25].ToString("x2") + buffer[26].ToString("x2"), 16)) / 100;
                StateData.ActualResis = Convert.ToSingle(Convert.ToInt32(buffer[27].ToString("x2") + buffer[28].ToString("x2") + buffer[29].ToString("x2") + buffer[30].ToString("x2"), 16)) / 100;
                StateData.ActualVolt_A = Convert.ToSingle(Convert.ToInt32(buffer[31].ToString("x2") + buffer[32].ToString("x2") + buffer[33].ToString("x2") + buffer[34].ToString("x2"), 16)) / 100;
                StateData.ActualVolt_B = Convert.ToSingle(Convert.ToInt32(buffer[35].ToString("x2") + buffer[36].ToString("x2") + buffer[37].ToString("x2") + buffer[38].ToString("x2"), 16)) / 100;
                StateData.ActualVolt_C = Convert.ToSingle(Convert.ToInt32(buffer[39].ToString("x2") + buffer[40].ToString("x2") + buffer[41].ToString("x2") + buffer[42].ToString("x2"), 16)) / 100;
                StateData.ActualCurrent_A = Convert.ToSingle(Convert.ToInt32(buffer[43].ToString("x2") + buffer[44].ToString("x2") + buffer[45].ToString("x2") + buffer[46].ToString("x2"), 16)) / 100;
                StateData.ActualCurrent_B = Convert.ToSingle(Convert.ToInt32(buffer[47].ToString("x2") + buffer[48].ToString("x2") + buffer[49].ToString("x2") + buffer[50].ToString("x2"), 16)) / 100;
                StateData.ActualCurrent_C = Convert.ToSingle(Convert.ToInt32(buffer[51].ToString("x2") + buffer[52].ToString("x2") + buffer[53].ToString("x2") + buffer[54].ToString("x2"), 16)) / 100;
                //StateData.ActualCurrent_C =  Convert.ToSingle(Convert.ToInt32(buffer[55].ToString("x2") + buffer[56].ToString("x2") + buffer[57].ToString("x2") + buffer[58].ToString("x2"), 16)) / 100;

            }
            return StateData;
        }

        /// <summary>
        /// 解析直流电阻负载实时数据
        /// </summary>
        /// <param name="buffer">报文</param>
        /// <param name="chargerID">枪编号</param>
        /// <returns></returns>
        public ResisLoad_StateData GetResisLoad_DC_StateData(byte[] buffer, int chargerID)
        {

            try
            {
                ResisLoad_StateData StateData = new ResisLoad_StateData();
                StateData.ChargerID = chargerID;
                StateData.emType = EmChargerType.Charger_GB_DC;
                StateData.EquipName = "直流电阻负载";
                if (CheckOut.CheckLoadCrc(buffer))
                {
                    int temp = Convert.ToInt32(buffer[7].ToString("x2") + buffer[8].ToString("x2") + buffer[9].ToString("x2") + buffer[10].ToString("x2"), 16);
                    int num = 0;
                    for (int K = 0; K < 32; K++)
                    {
                        if ((temp & 0x0001) == 0x0001)
                        {
                            num++;
                        }
                        temp >>= 1;
                    }
                    StateData.OnlineEquip = num;
                    StateData.DemandVolt = Convert.ToSingle(Convert.ToInt32(buffer[11].ToString("x2") + buffer[12].ToString("x2") + buffer[13].ToString("x2") + buffer[14].ToString("x2"), 16)) / 100;
                    StateData.DemandResis = Convert.ToSingle(Convert.ToInt32(buffer[15].ToString("x2") + buffer[16].ToString("x2") + buffer[17].ToString("x2") + buffer[18].ToString("x2"), 16)) / 100;
                    StateData.DemandCurrent = Convert.ToSingle(Convert.ToInt32(buffer[19].ToString("x2") + buffer[20].ToString("x2") + buffer[21].ToString("x2") + buffer[22].ToString("x2"), 16)) / 100;
                    StateData.DemandPower = Convert.ToSingle(Convert.ToInt32(buffer[23].ToString("x2") + buffer[24].ToString("x2") + buffer[25].ToString("x2") + buffer[26].ToString("x2"), 16)) / 100;
                    StateData.ActualPower = Convert.ToSingle(Convert.ToInt32(buffer[27].ToString("x2") + buffer[28].ToString("x2") + buffer[29].ToString("x2") + buffer[30].ToString("x2"), 16)) / 100;
                    StateData.ActualResis = Convert.ToSingle(Convert.ToInt32(buffer[31].ToString("x2") + buffer[32].ToString("x2") + buffer[33].ToString("x2") + buffer[34].ToString("x2"), 16)) / 100;
                    StateData.ActualVolt_A = Convert.ToSingle(Convert.ToInt32(buffer[35].ToString("x2") + buffer[36].ToString("x2") + buffer[37].ToString("x2") + buffer[38].ToString("x2"), 16)) / 100;
                    StateData.ActualCurrent_A = Convert.ToSingle(Convert.ToInt32(buffer[39].ToString("x2") + buffer[40].ToString("x2") + buffer[41].ToString("x2") + buffer[42].ToString("x2"), 16)) / 100;
                }
                return StateData;
            }
           catch
            {
                return null;
            }
        }
    }
}
