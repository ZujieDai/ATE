using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.EquipStateData;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media.Animation;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 设备 -(临时交流源) 响河设备
    /// </summary>
    public class emtACSource_TMP : EquipMentBase
    {
        /*
         * 功能码
         *  10H:		写设置参数、控制电源输出停止
	        03H:		读设置参数
	        04H:		读测量数据、状态
         * 
         * 
         */


        public ACSource_StateData stateData = new ACSource_StateData();
        private static object SynLock = new object();
        public emtACSource_TMP(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("临时交流源");
        }

        public override void ACSource_ON()
        {
            byte[] WriteBuffer = { 0x01, 0x10, 0xB3, 0xB0, 0x00, 0x01, 0x02, 0x00, 0x00, 0x3E, 0xAB };
            SendData(WriteBuffer);
        }

        public override void ACSource_OFF()
        {
            byte[] WriteBuffer = { 0x01, 0x10, 0xB3, 0xB1, 0x00, 0x01, 0x02, 0x00, 0x00, 0x3F, 0x7A };
            SendData(WriteBuffer);
        }


        public override void ACSource_SetVolt(double Volt)
        {
            // byte[] WriteBuffer = SetBuffer(41001, Convert.ToUInt16(Volt * 1000));
            /* 设定电压100V指令：01 10 A0 28 00 02 04 00 01 86 A0 3B CE//
                0186A0 = 100000 / 1000 = 100.000V
                返回：01 10 A0 28 00 02 E3 C0
            */
            Byte[] writeBuffer;
            try
            {
                UInt32 temp1 = 0;
                List<byte> ReturnbyteSource = new List<byte>();
                byte[] PrefixCode = new byte[] { 0x01, 0x10, 0xA0, 0x28, 0x00, 0x02, 0x04 };//前缀
                ReturnbyteSource.AddRange(PrefixCode);//前缀压入list

                temp1 = Convert.ToUInt32(Volt * 1000);
                ReturnbyteSource.Add((byte)(temp1 >> 24));//0000  0000
                ReturnbyteSource.Add((byte)(temp1 >> 16));//0000  0000  //// 
                ReturnbyteSource.Add((byte)(temp1 >> 8));//0000  0000  //// 
                ReturnbyteSource.Add((byte)temp1);//0000  0000  //// 

                byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
                byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
                ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
                writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节

            }
            catch
            {
                writeBuffer = new byte[] { 0x01, 0x10, 0xA0, 0x28, 0x00, 0x02, 0x04, 0x00, 0x03, 0x5B, 0x60, 0xC3, 0x0E };//前缀
            }

            SendData(writeBuffer);
        }

    




        private bool SendData(byte[] WriteBuffer)
        {
            AutoReadData = false;
            Thread.Sleep(500);
            lock (SynLock)
            {
                if (EquipMentPort != null)
                {
                    for (int i = 0; i < ReConnNum; i++)
                    {
                        DataBuf.Clear();
                        string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                        // SendMsgToFile("交流源发送数据：" + strTemp);
                        EquipMentPort.SendData(WriteBuffer);
                    }
                    AutoReadData = true;
                    return true;
                }
                else
                {
                    SendMsgToFile("交流源通道对象不存在，请检查交流源通道");
                    AutoReadData = true;
                    return false;
                }
            }
        }
    }
}
