using SaiTer.ATE.DataModel.EquipStateData;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices.ComTypes;
using System.Configuration;
using System.Diagnostics;
using System.Net.Sockets;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 程控板开、断继电器来控制电源输入
    /// </summary>

    public class emtACSource_CtrlBoard : EquipMentBase
    {
        public ACSource_StateData stateData = new ACSource_StateData();
        List<int> Switches;
        public emtACSource_CtrlBoard(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("交流源");
            //获取控制开关
            Switches = new List<int>();
            string[] ctrl_switch = ConfigurationManager.AppSettings["CtrlACSourceSwitch"]?.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            if (ctrl_switch != null && ctrl_switch.Length > 0)
            {
                foreach (string item in ctrl_switch)
                {
                    if (int.TryParse(item, out int result))
                        Switches.Add(result - 1);
                }
            }
            //else
            //{
            //    Switches.Add(9);//青岛S10控制1号枪市电
            //    Switches.Add(10);//S11控制2号枪    后面需要改到配置文件读取
            //}
        }

        public override List<bool> ControlBoardReadState()
        {
            byte[] buffer = { 0x01, 0x03, 0x00, 0x05, 0x00, 0x01, 0x94, 0x0B };
            List<bool> result = new List<bool>();
            for (int i = 0; i < 16; i++)
            {
                result.Add(false);
            }
            byte[] RevMsgData = null;
            lock (emtControlBoard.Locker)
            {
                EquipMentPort.SendData(buffer);
                RevMsgData = RevEquipMentData();
            }
            if (RevMsgData != null && RevMsgData.Length > 7)
            {
                RevMsgData = RevMsgData.Skip(RevMsgData.Length - 7).ToArray();
            }
            if (CheckOut.CheckModbusCrc16_High_Right(RevMsgData))
            {
                byte x1 = RevMsgData[3];//16-9
                byte x2 = RevMsgData[4];//8-1

                for (int i = 8; i < 16; i++)
                {
                    byte y = Convert.ToByte(Math.Pow(2, i - 8));
                    if ((x1 | y) == x1)
                    {
                        result[i] = true;
                    }
                }
                for (int i = 0; i < 8; i++)
                {
                    byte y = Convert.ToByte(Math.Pow(2, i));
                    if ((x2 | y) == x2)
                    {
                        result[i] = true;
                    }
                }
            }
            return result;
        }

        public override void ACSource_ON()
        {
            if(Switches == null || Switches.Count == 0)
            {
                SendMsgToFile("交流源控制开关未配置，请检查配置文件");
                return;
            }
            //List<bool> lstConditionState = new List<bool>();
            //for (int i = 0; i < 16; i++)
            //{
            //    lstConditionState.Add(false);
            //}
            //lstConditionState[0] = true;
            Thread.Sleep(1000);
            List<bool> lstConditionState = ControlBoardReadState();
            //lstConditionState[9] = true;//青岛S10控制1号枪市电
            //lstConditionState[10] = true;//S11控制2号枪    后面需要改到配置文件读取
            foreach (int item in Switches)
                lstConditionState[item] = true;
            byte[] WriteBuffer = GetBuffer(lstConditionState);
            SendData(WriteBuffer);
            //Thread.Sleep(300);
            //SendData(WriteBuffer);
        }

        public override void ACSource_OFF()
        {
            if (Switches == null || Switches.Count == 0)
            {
                SendMsgToFile("交流源控制开关未配置，请检查配置文件");
                return;
            }
            //List<bool> lstConditionState = new List<bool>();
            //for (int i = 0; i < 16; i++)
            //{
            //    lstConditionState.Add(false);
            //}
            //lstConditionState[0] = true;
            Thread.Sleep(1000);
            List<bool> lstConditionState = ControlBoardReadState();
            //lstConditionState[9] = false;//青岛S10控制1号枪市电
            //lstConditionState[10] = false;//S11控制2号枪    后面需要改到配置文件读取
            foreach (int item in Switches)
                lstConditionState[item] = false;
            byte[] WriteBuffer = GetBuffer(lstConditionState);
            SendData(WriteBuffer);
            //Thread.Sleep(300);
            //SendData(WriteBuffer);
        }




        private bool SendData(byte[] WriteBuffer)
        {
            if (EquipMentPort != null)
            {
                for (int i = 0; i < ReConnNum; i++)
                {
                    DataBuf.Clear();
                    string strTemp = BitConverter.ToString(WriteBuffer).Replace('-', ' ');
                    // SendMsgToFile("交流源发送数据：" + strTemp);
                    lock (emtControlBoard.Locker)
                    {
                        EquipMentPort.SendData(WriteBuffer);
                        var RevMsgData = RevEquipMentData();
                    }
                }
                return true;
            }
            else
            {
                SendMsgToFile("交流源通道对象不存在，请检查交流源通道");
                return false;
            }
        }
        private byte[] GetBuffer(List<bool> lstConditionState)
        {

            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x01, 0x06, 0x00, 0x05 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list
            Byte BYTE0 = 0;
            Byte BYTE1 = 0;
            for (int i = 0; i < 8; i++)
            {
                byte x = Convert.ToByte(Math.Pow(2, i));
                if (lstConditionState[i])
                {
                    BYTE0 = Convert.ToByte(x | BYTE0);
                }
            }
            for (int i = 8; i < 16; i++)
            {
                byte x = Convert.ToByte(Math.Pow(2, i - 8));
                if (lstConditionState[i])
                {
                    BYTE1 = Convert.ToByte(x | BYTE1);
                }
            }
            ReturnbyteSource.Add(BYTE1);//0000  0000
            ReturnbyteSource.Add(BYTE0);//0000  0000


            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。 
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;


        }

        public override void ACSource_ReadState()
        {
            if (!AllEquipStateData.DicACSource_StateData.ContainsKey(ChargerID))
            {
                AllEquipStateData.DicACSource_StateData.Add(ChargerID, stateData);
            }

            if (EquipMentPort != null)
            {
                stateData.ChargerID = this.ChargerID;
                stateData.Volt = 220;
                stateData.Freq = 50;
                SystemEvent.SendMonitorMessage(stateData);
            }
        }
    }
}
