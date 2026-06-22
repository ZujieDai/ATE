using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 录波板协议
    /// </summary>
    public static  class WaveRecorderProtocol
    {

        /*
         *协议格式   0x5A 0xA5  长度*2  发送方地址*1  命令标识*2  数据标识*2   
         * 上位机地址  01    录波板地址  80
         * TCP通讯  通信端口号  固定10001
         * 
         */
    }


    class WaveRecoderBoard30_Protocol
    {
        List<byte> bStart = new List<byte>() { 0x5A, 0xA5 };//帧起始符
        int iLen = 0;//长度
        byte bAdd = 0x01;//地址：上位机01，下位机80
        byte[] bCMDID = new byte[2] { 0x00, 0x01 };
        byte[] bDataID = new byte[2] { 0x00, 0x01 };
        //List<byte> bCMDID = new List<byte>();//命令标识
        //List<byte> bDataID = new List<byte>();//数据标识
        List<byte> bDatas = new List<byte>();//数据内容

        /// <summary>
        /// 组帧
        /// </summary>
        /// <returns></returns>
        public byte[] GetFrameMsg()
        {
            List<byte> bMsg = new List<byte>();
            iLen = 11 + bDatas.Count;
            bMsg.AddRange(bStart);
            bMsg.Add((byte)(iLen >> 24));
            bMsg.Add((byte)(iLen >> 16));
            bMsg.Add((byte)(iLen >> 8));
            bMsg.Add((byte)(iLen));
            bMsg.Add(bAdd);
            bMsg.AddRange(bCMDID);
            bMsg.AddRange(bDataID);
            bMsg.AddRange(bDatas);

            return bMsg.ToArray();
        }

        /// <summary>
        /// 开始录波
        /// </summary>
        /// <returns></returns>
        public byte[] StartWave()
        {
            bDatas.Clear();
            bCMDID[0] = 0x00;
            bCMDID[1] = 0x01;
            bDataID[0] = 0x00;
            bDataID[1] = 0x00;
            return GetFrameMsg();
        }

        /// <summary>
        /// 停止录波
        /// </summary>
        /// <returns></returns>
        public byte[] StopWave()
        {
            bDatas.Clear();
            bCMDID[0] = 0x00;
            bCMDID[1] = 0x02;
            bDataID[0] = 0x00;
            bDataID[1] = 0x00;
            return GetFrameMsg();
        }

        /// <summary>
        /// 读取通道数据
        /// </summary>
        /// <param name="ChnelNum"></param>
        /// <returns></returns>
        public byte[] ReadChannelData(int ChnelNum)
        {
            if (ChnelNum < 1 || ChnelNum > 8)
            {
                ChnelNum = 1;
            }
            bDatas.Clear();
            bCMDID[0] = 0x00;
            bCMDID[1] = 0x03;
            bDataID[0] = (byte)(ChnelNum >> 8);
            bDataID[1] = (byte)(ChnelNum);
            return GetFrameMsg();
        }
        /// <summary>
        /// 设置采样率
        /// </summary>
        /// <param name="iCyl">值：1，10，100   单位：k</param>
        /// <returns></returns>
        public byte[] SetCyl(int iCyl)
        {
            if (iCyl != 1 && iCyl != 10 && iCyl != 100)
            {
                iCyl = 1;
            }
            bDatas.Clear();
            bCMDID[0] = 0x00;
            bCMDID[1] = 0x05;
            bDataID[0] = (byte)(iCyl >> 8);
            bDataID[1] = (byte)(iCyl);
            return GetFrameMsg();
        }
        /// <summary>
        /// 读取CAN报文数据
        /// </summary>
        /// <returns></returns>
        public byte[] ReadCanMsg()
        {
            bDatas.Clear();
            bCMDID[0] = 0x00;
            bCMDID[1] = 0x0A;
            bDataID[0] = 0x00;
            bDataID[1] = 0x00;
            return GetFrameMsg();
        }
        /// <summary>
        /// 启动所有数字通道
        /// </summary>
        /// <returns></returns>
        public byte[] StartDigitalChannel()
        {
            bDatas.Clear();
            bCMDID[0] = 0x00;
            bCMDID[1] = 0x10;
            bDataID[0] = 0x00;
            bDataID[1] = 0x00;
            return GetFrameMsg();
        }
        /// <summary>
        /// 停止所有数字通道
        /// </summary>
        /// <returns></returns>
        public byte[] StopDigitalChannel()
        {
            bDatas.Clear();
            bCMDID[0] = 0x00;
            bCMDID[1] = 0x11;
            bDataID[0] = 0x00;
            bDataID[1] = 0x00;
            return GetFrameMsg();
        }
        /// <summary>
        /// 启动指定的数字通道录波
        /// </summary>
        /// <param name="ChnelNum"></param>
        /// <returns></returns>
        public byte[] StartDigitalChannel(int ChnelNum)
        {

            bDatas.Clear();
            bCMDID[0] = 0x00;
            bCMDID[1] = 0x12;
            bDataID[0] = (byte)(ChnelNum >> 8);
            bDataID[1] = (byte)(ChnelNum);
            return GetFrameMsg();
        }
        /// <summary>
        /// 停止指定的数字通道录波
        /// </summary>
        /// <param name="ChnelNum"></param>
        /// <returns></returns>
        public byte[] StopDigitalChannel(int ChnelNum)
        {

            bDatas.Clear();
            bCMDID[0] = 0x00;
            bCMDID[1] = 0x13;
            bDataID[0] = (byte)(ChnelNum >> 8);
            bDataID[1] = (byte)(ChnelNum);
            return GetFrameMsg();
        }
        /// <summary>
        /// 读取数字通道数据
        /// </summary>
        /// <param name="ChnelNum">数字通道编号</param>
        /// <returns></returns>
        public byte[] ReadDigitalChannelData(int ChnelNum)
        {
            bDatas.Clear();
            bCMDID[0] = 0x00;
            bCMDID[1] = 0x14;
            bDataID[0] = (byte)(ChnelNum >> 8);
            bDataID[1] = (byte)(ChnelNum);
            return GetFrameMsg();
        }


    }
}
