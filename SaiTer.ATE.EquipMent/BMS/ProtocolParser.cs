using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.EquipMent
{
    public class ProtocolDataEventArgs : EventArgs
    {
        public byte[] ProtocolData { get; }

        public ProtocolDataEventArgs(byte[] data)
        {
            ProtocolData = data;
        }
    }

    public class ProtocolParser
    {
        private List<byte> buffer = new List<byte>();
        private void AddBuffer(byte[] bytes)
        {
            lock (objLock)
            {
                buffer.AddRange(bytes);
            }
        }
        private void RemoveBuffer(int index, int count)
        {
            lock (objLock)
            {
                buffer.RemoveRange(index, count);
            }
        }
        public List<byte> GetBuffer()
        {
            lock (objLock)
            {
                byte[] copyBuffer = (byte[])buffer.ToArray().Clone();
                return copyBuffer.ToList();
            }
        }
        public void ClearBuffer()
        {
            lock (objLock)
            {
                buffer.Clear();
            }
        }

        private readonly byte[] Head = { 0x7E, 0x00, 0xFF }; // 协议起始标志
        private readonly byte[] End = { 0x0D }; // 协议结束标志
        private const int CRC16Length = 2;
        private object objLock = new object();

        public bool IsPause { get; set; }

        public ProtocolParser()
        {

            //ThreadStart WorkStart = delegate { ProcessBuffer(); };
            //Thread thread = new Thread(WorkStart);
            //thread.Start();
            Thread backgroundThread = new Thread(ProcessBuffer);
            backgroundThread.IsBackground = true;
            backgroundThread.Start();


        }

        // 定义三个队列，分别用于存储不同协议类型的数据帧
        //private object stateDataLock = new object();
        //private Queue<byte[]> stateDataQueue = new Queue<byte[]>();
        /// <summary>
        /// 状态数据队列
        /// </summary>
        public Queue<byte[]> StateDataQueue = new Queue<byte[]>();
        /// <summary>
        /// 其它数据队列
        /// </summary>
        public Queue<byte[]> OtherDataQueue = new Queue<byte[]>();
        /// <summary>
        /// CAN报文数据队列
        /// </summary>
        public Queue<byte[]> CAN_Queue = new Queue<byte[]>();



        // 添加事件，分别用于通知三种数据帧到达
        public event EventHandler<ProtocolDataEventArgs> StateDataReceived;
        public event EventHandler<ProtocolDataEventArgs> OtherDataReceived;
        public event EventHandler<ProtocolDataEventArgs> CAN_DataReceived;

        public void AddBytesToBuffer(byte[] data)
        {
            AddBuffer(data);
        }
        int first0x0D = 0;//第一个0x0d结束符的位置（因为报文本身有可能出现0x0d，当搜索到第一个0x0d，校验不通过时，继续往下搜索）
        private void ProcessBuffer()
        {
            while (true)
            {
                int startIndex = 0;
                while (GetBuffer().Count > startIndex)
                {
                    if (IsPause)
                        continue;
                    List<byte> buffer = GetBuffer();
                    try
                    {
                        int HeadIndex = buffer.IndexOf(Head[0], startIndex);

                        if (HeadIndex != -1 && HeadIndex + Head.Length < buffer.Count)
                        {
                            if (buffer.Count - HeadIndex > (Head.Length + CRC16Length))
                            {
                                bool isHead = Head.SequenceEqual(buffer.Skip(HeadIndex).Take(Head.Length));
                                if (isHead)
                                {
                                    int EndIndex = 0;
                                    if (first0x0D == 0)
                                    {
                                        EndIndex = buffer.IndexOf(End[0], HeadIndex + Head.Length);
                                    }
                                    else
                                    {
                                        EndIndex = buffer.IndexOf(End[0], first0x0D + 1);
                                    }
                                    if (EndIndex == -1)
                                    {
                                        break;
                                    }
                                    if (EndIndex != -1 && EndIndex + CRC16Length < buffer.Count)
                                    {
                                        byte[] possibleMessage = buffer.GetRange(HeadIndex, EndIndex - HeadIndex + 1 + CRC16Length).ToArray();

                                        if (CheckOut.CheckModbusCrc16_High_Right(possibleMessage))
                                        {
                                            if (possibleMessage[4] == 0x01)//状态数据
                                            {
                                                StateDataQueue.Enqueue(possibleMessage);
                                                OnStateDataReceived(possibleMessage);
                                            }
                                            else if (possibleMessage[4] == 0x59)//CAN报文
                                            {

                                                CAN_Queue.Enqueue(possibleMessage);
                                                OnCAN_DataReceived(buffer.ToArray());
                                                Console.WriteLine("CAN_Queue Count:" + CAN_Queue.Count());
                                            }
                                            else//其它数据
                                            {
                                                OtherDataQueue.Enqueue(possibleMessage);
                                                OnOtherDataReceived(possibleMessage);
                                            }

                                            // 更新缓冲区，剔除已处理的字节
                                            RemoveBuffer(0, EndIndex + 1 + CRC16Length);
                                            startIndex = 0;
                                            first0x0D = 0;
                                            break;  // 跳出查找0D的循环
                                        }
                                        else
                                        {
                                            // 校验失败，继续查找下一个0D
                                            //startIndex = EndIndex + 1;
                                            //EndIndex = buffer.IndexOf(End[0], startIndex);
                                            first0x0D = EndIndex;
                                            if (EndIndex > 1000)
                                            {
                                                first0x0D = 0;
                                                RemoveBuffer(0, EndIndex);
                                                break;  // 跳出查找0D的循环
                                            }
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    // 如果协议起始标志不匹配，继续查找下一个可能的协议起始标志
                                    startIndex = HeadIndex + 1;
                                }
                            }
                            else
                            {
                                // 如果剩余字节不足以解析协议，等待更多的字节
                                break;
                            }
                        }
                        else
                        {
                            // 如果没有找到协议起始标志或者剩余字节不够，清空缓冲区
                            //buffer.Clear();
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        buffer.Clear();
                        first0x0D = 0;
                        startIndex = 0;
                        Log.Log.LogException(e);
                    }
                }
                Thread.Sleep(5);
            }
        }


        public void SendMsgToFile(string Msg)
        {
            Log.Log.LogMessage("[" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "]" + Msg, "" + 1 + "号枪BMS调试日志");
        }

        private void OnStateDataReceived(byte[] data)
        {
            StateDataReceived?.Invoke(this, new ProtocolDataEventArgs(data));
        }

        private void OnCAN_DataReceived(byte[] data)
        {
            CAN_DataReceived?.Invoke(this, new ProtocolDataEventArgs(data));
        }

        private void OnOtherDataReceived(byte[] data)
        {
            OtherDataReceived?.Invoke(this, new ProtocolDataEventArgs(data));
        }

    }
}
