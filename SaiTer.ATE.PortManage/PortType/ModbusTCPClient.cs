using Modbus.Device;
using System;
using System.Net;
using System.Net.Sockets;

namespace SaiTer.ATE.PortManage.PortType
{
    public class ModbusTCPClient : PortBase
    {
        TcpClient client;
        ModbusIpMaster modbusMaster;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="type"></param>
        public ModbusTCPClient(int type)
        {

        }

        public override bool Close()
        {
            try
            {
                client.Close();
                modbusMaster.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                SendErrMsg(ex);
                return false;
            }
        }

        public override bool Open()
        {
            try
            {
                // 创建并初始化Modbus TCP客户端
                client = new TcpClient(Ipaddress, RemotePort);
                modbusMaster = ModbusIpMaster.CreateIp(client);

                // 设置超时时间（可选）
                modbusMaster.Transport.Retries = 3;
                modbusMaster.Transport.ReadTimeout = 1000;
                modbusMaster.Transport.WriteTimeout = 1000;

                return true;
            }
            catch (Exception ex)
            {
                SendErrMsg(ex);
                return false;
            }
        }

        public override void SendData(byte[] SendBuff)
        {
        }

        public void WriteData(ushort regNo, ushort value)
        {
            try
            {
                modbusMaster?.WriteSingleRegister(1, regNo, value);
            }
            catch (Exception ex)
            {
                SendErrMsg(ex);
            }
        }

        public ushort[] ReadData(ushort regNo, ushort value)
        {
            try
            {
                return modbusMaster?.ReadInputRegisters(1, regNo, value);
            }
            catch (Exception ex)
            {
                SendErrMsg(ex);
                return null;
            }
        }
    }
}
