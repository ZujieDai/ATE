using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 设备基类 - 示波器相关虚函数
    /// </summary>
    public partial class EquipMentBase
    {
        #region -----------------------示波器-----------------------
        public string BaseDirectoryPath = System.AppDomain.CurrentDomain.BaseDirectory;
        public string ImagePathFile;
        public int DeviceId = 999;//横河设备ID
        /// <summary>
        /// 设置截屏路径
        /// </summary>
        public void SetImagePath()
        {

            string GetNowYear = System.DateTime.Now.Year.ToString();//获取年
            string GetNowMonth = System.DateTime.Now.Month.ToString();//获取月
            string GetNowLongData = System.DateTime.Now.ToString("yyyy-MM-dd");

            string subImagePath = BaseDirectoryPath + "报表(勿删)\\Image";
            if (false == System.IO.Directory.Exists(subImagePath))
            {
                //创建pic文件夹
                System.IO.Directory.CreateDirectory(subImagePath);
            }
            //检查是否有文件夹,没有则创建一个
            string subImagePath4 = BaseDirectoryPath + "报表(勿删)\\Image" + "\\" + GetNowLongData;
            if (false == System.IO.Directory.Exists(subImagePath4))
            {
                //创建pic文件夹
                System.IO.Directory.CreateDirectory(subImagePath4);
            }

            ImagePathFile = BaseDirectoryPath + "报表(勿删)\\Image" + "\\" + GetNowLongData + "\\";
        }

        public bool Query(string Command, ref string value)
        {
            bool bRet = false;
            EquipMentPort.VisaNS.Write(Command);
            System.Threading.Thread.Sleep(50);
            value = EquipMentPort.VisaNS.ReadString().Split(' ')[0];
            bRet = true;
            return bRet;
        }

        public bool QueryMultiple(List<string> listCommands, ref string[] rtbStrings)
        {
            rtbStrings = new string[listCommands.Count];
            bool bRet = false;
            for (int i = 0; i < listCommands.Count; i++)
            {
                string Command = listCommands[i];
                EquipMentPort.VisaNS.Write(Command);
                System.Threading.Thread.Sleep(50);
                string value = EquipMentPort.VisaNS.ReadString().Split(' ')[0];
                rtbStrings[i] = value;
            }
            bRet = true;
            return bRet;
        }

        public bool Read(int Length, ref byte[] value)
        {
            bool bRet = false;
            value = EquipMentPort.VisaNS.ReadByteArray(Length);
            bRet = true;
            return bRet;
        }

        public bool ReadString(ref string value)
        {
            bool bRet = false;
            value = EquipMentPort.VisaNS.ReadString().Split(' ')[0];
            bRet = true;
            return bRet;
        }

        public bool WriteString(string Command)
        {
            bool bRet = false;
            EquipMentPort.VisaNS.Write(Command);
            Thread.Sleep(50);
            bRet = true;
            return bRet;
        }

        public bool WriteMultipleString(List<string> listCommands)
        {
            bool bRet = false;
            foreach (string command in listCommands)
            {
                EquipMentPort.VisaNS.Write(command);
                Thread.Sleep(50);
            }
            bRet = true;
            return bRet;
        }

        /// <summary>
        /// 读示波器实时状态数据
        /// </summary>
        public virtual void Read_OscilloscopeState() { }

        /// <summary>
        /// 示波器初始化
        /// </summary>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void OscilloscopeIDefalut() { }

        /// <summary>
        /// 设置采样模式
        /// </summary>
        /// <param name="mode">0=SAMple|1=AVErage</param>
        /// <param name="num">用于平均值的计算点数</param>
        public virtual void Oscilloscope_SetACQuireMode(int mode, int num = 0) { }

        /// <summary>
        /// 示波器通道设置
        /// </summary>
        /// <param name="channel">通道(比如1、2、3)</param>
        /// <param name="gear">纵坐标档位  例子:10,20,50,100,200，现在单位为V，要根据探头比来相应设置)</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscilloscope_Channel_SetGear(int channel, string gear) { }

        /// <summary>
        /// 示波器通道设置
        /// </summary>
        /// <param name="channel">通道(比如1、2、3)</param>
        /// <param name="isOpen">是否开启当前通道</param>
        /// <param name="coupling">通道耦合(比如AC、DC)，测试项
        /// 纹波测试项AC,其它测试项基本为DC</param>
        /// <param name="tapewidth"> 带宽(比如20M、200M、FULL)</param>
        /// <param name="probe"> 探头比(比如100、200、0.01、0.02,100代表放大，0.01代表缩小)</param>
        /// <param name="tag">标签(纯文本英文)</param>
        /// <param name="impedance">阻抗(默认1M、50欧)</param>
        /// <param name="unit">单位(V或者A，代表电压或者电流)</param>
        /// <param name="isOpen_A">通道反相是否开启</param>
        /// <param name="gear">纵坐标档位(例子:10,20,50,100,200，现在单位为V，要根据探头比来相应设置)</param>
        /// <param name="position">纵坐标位置(根据实际纵坐标格子来算，一般是（-（格子/2），格子/2）),实际会自动乘以换算挡位</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscilloscope_Channel_Set(int channel, bool isOpen, string coupling, string tapewidth, string probe, string tag, string impedance, string unit, bool isOpen_A, string gear, string position) { }

        /// <summary>
        /// 初始化测量设置，清除所有测量项
        /// </summary>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscilloscope_Measure_Initialize() { }
        //
        /// <summary>
        /// 添加测量项
        /// </summary>
        /// <param name="MeasurementType">类型(看添加测量项参数说明)</param>
        /// <param name="channel"></param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscilloscope_AddMeasure(string MeasurementType, int channel) { }
        /// <summary>
        /// 读取第几个测量值    
        /// </summary>
        /// <param name="MeasureNumber">第几个</param>
        /// <param name="isIMMed">是否为即时读取</param>
        /// <returns></returns>
        public virtual void Oscilloscope_ReadMeasure(int MeasureNumber, ref string value) { }

        /// <summary>
        /// 按类型读取测量值
        /// </summary>
        /// <param name="MeasureType">测量值类型</param>
        /// <param name="channel">通道号(1,2)</param>
        /// <returns>返回为try catch 的错误，没有则返回为空<</returns>
        public virtual void Oscilloscope_ReadMeasure(string MeasureType, int channel, ref string value, bool isIMMed = false) { }
        /// <summary>
        /// 示波器时基设置
        /// </summary>
        /// <param name="isroll">是否滚动</param>
        /// <param name="timebase">时基（200、400，暂定ms为单位）</param>
        /// <param name="delay"> (0、1，暂定s为单位，默认0)</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscilloscope_TimeBase(bool isroll, string timebase, string delay) { }
        /// <summary>
        /// 示波器触发
        /// </summary>
        /// <param name="type_first">主类型(边沿或者超时,0为边沿，1为超时，2欠幅或矮脉冲，以后再加要用到的定义)</param>
        /// <param name="type_second">次类型(上升沿或者下降沿,RISE,FALL,Alternating)</param>
        /// <param name="coupling">触发耦合(默认直流,AC,DC)</param>
        /// <param name="timeout_type">超时类型(一般默认边沿,EDGE)</param>
        /// <param name="timeout">超时时间(ms为单位，配合超时触发用)</param>
        /// <param name="channel">通道(1、2)</param>
        /// <param name="triggerLevel">触发电平(0mV,1V,2V)</param>
        /// <param name="triggerType">触发类型(Auto、Normal、Single)(自动、普通、单次)</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscilloscope_Trigger(int type_first, string type_second, string coupling, string typeout_type, string timeout, int channel, string triggerLevel, string triggerType) { }

        /// <summary>
        /// 示波器触发类型设置
        /// </summary>
        /// <param name="TriggerType">触发类型(Auto、Normal、Single)(自动、普通、单次)</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscilloscope_TriggerTypeSet(string TriggerType) { }

        /// <summary>
        /// 读是否已经触发
        /// </summary>
        /// <param name="isTrigger"></param>
        public virtual void ReadTriggerState(ref bool isTrigger) { }
        
        /// <summary>
        /// 触发状态回读
        /// </summary>
        /// <returns>返回触发状态,0为stop,1为等待触发,-1为错误,2为其他状态</returns>
        public virtual int Oscilloscope_ReadTrigger() { return -1; }
        /// <summary>
        /// 波形数据回读
        /// </summary>
        /// <param name="channnel"> 通道(1、2)</param>
        /// <returns>返回为波形数据</returns>
        public virtual void OscilloscopeCursorData(int channnel, ref double[] data, int RecondLengZero = 10000 * 20) { data = null; }

        /// <summary>
        /// 截取示波器屏幕
        /// </summary>
        /// <returns>主要返回示波器截屏存储到电脑上的存储路径，一般是存储在程序运行
        /// 目录下的报表(勿删)\Image\年\月\年-月-日\生成的截屏名称.jpg
        /// 否则返回为try catch 的错误，没有则返回为空</returns>
        public virtual void OscilloscopeSaveScreen(ref string path) { path = ""; }

        /// <summary>
        /// 示波器启停控制
        /// </summary>
        /// <param name="isRun">是否运行</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscilloscope_IsRun(bool isRun) { isRun = false; }
        /// <summary>
        /// 启停状态回读
        /// </summary>
        /// <returns>true运行，false没有运行</returns>
        public virtual void Oscilloscope_ReadRun(ref bool isrun) { isrun = false; }


        /// <summary>
        /// 示波器光标类型设置
        /// </summary>
        /// <param name="type">参数为X、Y、XY</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscilloscope_CursorsSet(string type) { }
        /// <summary>
        /// 示波器横坐标卡点
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="value1">左边点1(0到100的比例，屏幕占比)</param>
        /// <param name="value2">右边点2(0到100的比例，屏幕占比)</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscilloscope_CursorPosition_X(int channel, double value1, double value2) { }        /// <summary>


        /// <summary>
        /// 示波器卡点单一
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="value1">(0到100的比例，屏幕占比)</param>
        /// <param name="isleft">是否为左边</param>
        /// <returns></returns>
        public virtual void Oscilloscope_CursorPosition_X_Single(int channel, double value, bool isleft) { }

        /// <summary>
        /// 示波器卡点按时间单一
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="time">时间为单位为ms</param>
        /// <param name="isleft">是否为左边</param>
        /// <returns></returns>
        public virtual void Oscilloscope_CursorPosition_X_Time(int channel, double time, bool isleft) { }
        /// <summary>
        /// 示波器纵坐标卡点
        /// </summary>
        /// <param name="channel">通道(1、2、3、4等)</param>
        /// <param name="value1">下边点1(0到100的比例，屏幕占比)</param>
        /// <param name="value2">上边点2(0到100的比例，屏幕占比)</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscilloscope_CursorPosition_Y(int channel, double value1, double value2) { }
        /// <summary>
        /// 示波器纵坐标卡点
        /// </summary>
        /// <param name="channel">通道(1、2、3、4等)</param>
        /// <param name="value">数值</param>
        /// <param name="cursorIndex">1=Y1;2=Y2</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscilloscope_CursorPosition_Y(int channel, double value, int cursorIndex) { }

        /// <summary>
        /// 回读光标参数
        /// </summary>
        /// <param name="channel">通道(1、2、3、4等)</param>
        /// <returns>返回double[5]。第一个为时间差，第二个为1/时间差=频率单位为Hz，第三个为横坐标左边值，第四个为横坐标右边值，第五个为幅度差。时间单位为ms</returns>
        public virtual void Oscilloscope_ReadCursors(int channel, ref double[] Cursors) { Cursors = null; }
        /// <summary>
        /// 控制示波器是否能触屏
        /// </summary>
        /// <param name="isTouch">能否触屏</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscilloscope_Touch(bool isTouch) { }

        /// <summary>
        /// 设置示波器最大存储深度
        /// </summary>
        /// <param name="storagedepth">存储深度，目前已有参数10k、50k、250k、1.25M、2.5M、12.5M、25M、125M、250M</param>
        /// <returns></returns>
        public virtual void Oscilloscope_StorageDepth(string storagedepth) { }
        /// <summary>
        /// 示波器时基设置
        /// </summary>
        /// <param name="isroll">是否滚动</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscilloscope_IsRoll(bool isroll) { }
        /// <summary>
        /// 计算卡点值
        /// </summary>
        /// <param name="data">回读的示波器的点</param>
        /// <param name="upvalue">上升或下降的高值</param>
        /// <param name="downvalue"> 上升或下降的低值</param>
        /// <param name="uptype">0为上升，1为下降</param>
        /// <param name="isAC">是不是交流</param>
        /// <returns>返回double[2]，返回占屏幕的比例值,跟示波器没有关系，自己根据示波器回读的点来进行计算</returns>
        public virtual void Oscilloscope_Points(double[] data, double upvalue, double downvalue, int uptype, bool isAC, ref double[] position) { position = null; }




        /// <summary>
        /// 计算卡点值单一
        /// </summary>
        /// <param name="data">回读的示波器的点</param>
        /// <param name="value">上升或下降的比较值</param>
        /// <param name="uptype">0为上升，1为下降</param>
        /// <param name="isright"> 卡的点是否为右边</param>
        /// <param name="isAC">是否为交流</param>
        /// <returns>返回double，返回占屏幕的比例值，跟示波器没有关系，自己根据示波器回读的点来进行计算</returns>
        public virtual void Oscilloscope_Points_Single(double[] data, double value, int uptype, bool isright, bool isAC, ref double position) { position = -1; }

        /// <summary>
        /// 计算卡点值单一
        /// </summary>
        /// <param name="data">回读的示波器的点</param>
        /// <param name="value">上升或下降的比较值</param>
        /// <param name="uptype">0为上升，1为下降</param>
        /// <param name="isright"> 卡的点是否为右边</param>
        /// <param name="isAC">是否为交流</param>
        /// <param name="count">连续多少个点</param>
        /// <returns>返回double，返回占屏幕的比例值，跟示波器没有关系，自己根据示波器回读的点来进行计算</returns>
        public virtual void Oscilloscope_Points_Single_AC(double[] data, double value, int uptype, bool isright, bool isAC, int count, ref double position) { position = -1; }

        /// <summary>
        /// 设置通道自动调整
        /// </summary>
        /// <param name="channel">通道(1、2、3、4等)</param>
        public virtual void Oscilloscope_AutoZero(int channel) { }
        #endregion

    }
}
