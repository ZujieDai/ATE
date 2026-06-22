using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 录波仪的基类
    /// </summary>
    public partial class EquipMentBase
    {

        /// <summary>
        /// 录波仪初始化
        /// </summary>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void OscillographIDefalut() { }

        /// <summary>
        /// 录波仪初始化显示
        /// </summary>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void OscillographIDefalut_Show() { }
        /// <summary>
        /// 录波仪通道设置开关
        /// </summary>
        /// <param name="channel">通道(比如1、2、3)</param>
        /// <param name="isOpen">是否开启当前通道</param>

        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscillograph_Channel_Open(int channel, bool isOpen) { }

        /// <summary>
        /// 录波仪通道设置
        /// </summary>
        /// <param name="channel">通道(比如1、2、3)</param>
        /// <param name="TRACE">显示不同的轨道</param>
        /// <param name="isOpen">是否开启当前通道</param>
        /// <param name="coupling">通道耦合(比如AC、DC)，测试项
        /// 纹波测试项AC,其它测试项基本为DC</param>
        /// <param name="tapewidth"> 带宽(比如20M、200M、FULL)</param>
        /// <param name="SRATE"> 采样率 单位为1S/s</param>
        /// <param name="probe"> 探头比(比如100、200、0.01、0.02,100代表放大，0.01代表缩小)</param>
        /// <param name="tag">标签(纯文本英文)</param>
        /// <param name="unit">单位(V或者A，代表电压或者电流)</param>
        /// <param name="isOpen_A">通道反相是否开启</param>
        /// <param name="gear">纵坐标档位(例子:10,20,50,100,200，现在单位为V，要根据探头比来相应设置)</param>
        /// <param name="position">纵坐标位置(根据实际纵坐标格子来算，一般是（-（格子/2），格子/2）),实际会自动乘以换算挡位</param>
        /// <param name="filteringtype">0为LPF,1为数字滤波</param>
        /// <param name="digitfilteringtype">数字滤波方式 目前只采用GAUSs、IIR、SHARp 0、1、2</param>
        /// <param name="digittapetype">数字滤波带宽形式 ，默认低通，高通，带通,0、1、2</param>
        /// <param name="digittypewidth">带通滤波通带宽度 ？默认200Hz</param>
        /// <param name="Color">设置颜色</param>
        /// <param name="SGR1">显示在组1</param>
        /// <param name="SGR2">显示在组2</param>
        /// <param name="SGR3">显示在组3</param>
        /// <param name="SGR4">显示在组4</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscillograph_Channel_Set(int channel, int TRACE, bool isOpen, string coupling, string tapewidth, string SRATE, string probe, string tag, string unit, bool isOpen_A, string gear, string position, int filteringtype, int digitfilteringtype, int digittapetype, string digittypewidth, string Color, bool SGR1, bool SGR2, bool SGR3, bool SGR4) { }

        /// <summary>
        /// 录波仪通道设置带宽和滤波
        /// </summary>
        /// <param name="channel">通道(比如1、2、3)</param>
        /// <param name="tapewidth"> 带宽(比如20M、200M、FULL)</param>
        /// <param name="filteringtype">0为LPF,1为数字滤波</param>
        /// <param name="digitfilteringtype">数字滤波方式 目前只采用GAUSs、IIR、SHARp 0、1、2</param>
        /// <param name="digittapetype">数字滤波带宽形式 ，默认低通，高通，带通,{BPASs|HPASs|LPASs}0、1、2</param>
        /// <param name="digittypewidth">带通滤波通带宽度 ？默认200Hz</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscillograph_Channel_SetFiltering(int channel, string tapewidth, int filteringtype, int digitfilteringtype, int digittapetype, string digittypewidth) { }


        /// <summary>
        /// 录波仪通道设置挡位
        /// </summary>
        /// <param name="channel">通道(比如1、2、3)</param>
        /// <param name="gear">纵坐标档位(例子:10,20,50,100,200，现在单位为V，要根据探头比来相应设置)</param>
        /// <param name="position">纵坐标位置，为空则不操作</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>

        public virtual void Oscillograph_Channel_SetGear(int channel, string gear, string position) { }


        /// <summary>
        /// 录波仪通道设置挡位CAN
        /// </summary>
        /// <param name="channel">通道(比如1、2、3)</param>
        /// <param name="schannel">子通道(比如1、2、3)</param>
        /// <param name="UpLimit">显示上限</param>
        /// <param name="DownLimit">显示下限</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscillograph_Channel_SetGear_CAN(int channel, int schannel, string UpLimit, string DownLimit) { }
        /// <summary>
        /// 录波仪通道设置只滤波
        /// </summary>
        /// <param name="channel">通道(比如1、2、3)</param>
        /// <param name="coupling">通道耦合(比如AC、DC)，测试项
        /// 纹波测试项AC,其它测试项基本为DC</param>
        /// <param name="tapewidth"> 带宽(比如20M、200M、FULL)</param>

        /// <param name="filteringtype">0为LPF,1为数字滤波</param>
        /// <param name="digitfilteringtype">数字滤波方式 目前只采用GAUSs、IIR、SHARp 0、1、2</param>
        /// <param name="digittapetype">数字滤波带宽形式 ，默认低通，高通，带通,{BPASs|HPASs|LPASs}0、1、2</param>
        /// <param name="digittypewidth">带通滤波通带宽度 ？默认200Hz</param>

        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscillograph_Filtering(int channel, string coupling, string tapewidth, int filteringtype, int digitfilteringtype, int digittapetype, string digittypewidth) { }
        /// <summary>
        /// 初始化测量设置，清除所有测量项
        /// </summary>
        /// <param name="channel"></param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscillograph_Measure_Initialize(int channel) { }
        /// <summary>
        /// 添加测量项
        /// </summary>
        /// <param name="MeasurementType">类型(看添加测量项参数说明)</param>
        /// <param name="channel"></param>
        /// <param name="isCAN">是否为CAN</param>
        /// <param name="Schannel">子通道</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscillograph_AddMeasure(string MeasurementType, int channel, bool isCAN, int Schannel) { }
        /// <summary>
        /// 读取第几个测量值    
        /// </summary>
        /// <param name="MeasureNumber">第几个</param>
        /// <param name="returnvalue">返回测量的值</param>
        /// <returns></returns>
        public virtual void Oscillograph_ReadMeasure(int MeasureNumber, ref string returnvalue) { }

        /// <summary>
        /// 按类型读取测量值
        /// </summary>
        /// <param name="MeasureType">测量值类型</param>
        /// <param name="channel">通道号(1,2)</param>
        /// <param name="isCAN">是否为CAN</param>
        /// <param name="Schannel">子通道</param>
        /// <param name="returnvalue">返回测量的值</param>
        /// <returns></returns>
        public virtual void Oscillograph_ReadMeasure(string MeasureType, int channel, bool isCAN, int Schannel, ref string returnvalue) { }
        /// <summary>
        /// 录波仪时基设置
        /// </summary>
        /// <param name="timebase">时基（200、400，暂定ms为单位）</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscillograph_TimeBase(string timebase) { }
        /// <summary>
        /// 设置触发
        /// </summary>
        /// <param name="type_second">触发次类型，上升沿或者下降沿，交替沿，不确定是上还是下时，使用单次交替沿触发,比如急停  {RISE|FALL|BISLope} </param>
        /// <param name="channel">通道号</param>
        /// <param name="isCAN">是否为CAN</param>
        /// <param name="schannel">子通道</param>
        /// <param name="triggerLevel">触发电平</param>
        /// <param name="triggerType">触发类型(Auto、Normal、Single)(默认自动、自动电平，普通=普通、单次，N次，start)</param>
        /// <param name="position">0-100% 触发位置</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscillograph_Trigger(string type_second, int channel, bool isCAN, int schannel, string triggerLevel, string triggerType, double position) { }

        /// <summary>
        /// 录波仪触发类型设置
        /// </summary>
        /// <param name="TriggerType">触发类型(Auto、Normal、Single)(自动、普通、单次)</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscillograph_TriggerTypeSet(string TriggerType) { }

        /// <summary>
        /// 触发状态回读
        /// </summary>
        /// <returns>返回触发状态,0为stop,1为等待触发,-1为错误,2为其他状态</returns>
        public virtual int Oscillograph_ReadTrigger() { return 2; }
        /// <summary>
        /// 波形数据回读
        /// </summary>
        /// <param name="channel"> 通道(1、2)</param>
        /// <param name="isCAN"> 是否是CAN</param>
        /// <param name="schannel"> 子通道(1、2)</param>
        /// <returns>返回为波形数据</returns>
        public virtual double[] OscillographCursorData(int channel, bool isCAN, int schannel) { return null; }

        /// <summary>
        /// 截取录波仪屏幕
        /// </summary>
        /// <returns>主要返回录波仪截屏存储到电脑上的存储路径，一般是存储在程序运行
        /// 目录下的报表(勿删)\Image\年\月\年-月-日\生成的截屏名称.jpg
        /// 否则返回为try catch 的错误，没有则返回为空</returns>
        public virtual void OscillographSaveScreen(ref string path) { }

        /// <summary>
        /// 录波仪启停控制
        /// </summary>
        /// <param name="isRun">是否运行</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscillograph_IsRun(bool isRun) { }
        /// <summary>
        /// 启停状态回读
        /// </summary>
        /// <returns>true运行，false没有运行</returns>
        public virtual bool Oscillograph_ReadRun() { return false; }


        /// <summary>
        /// 录波仪光标类型设置
        /// </summary>
        /// <param name="type">参数为X、Y、XY</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        /// <summary>
        /// 录波仪光标类型开启
        /// </summary>
        /// <param name="Open">是否开启</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscillograph_CursorsOpen(bool Open) { }



        /// <summary>
        /// 录波仪测量类型开启（关闭会不显示测量值）
        /// </summary>
        /// <param name="Open">是否开启</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscillograph_MEASureOpen(bool Open) { }
        public virtual void Oscillograph_CursorsSet(string type) { }
        /// <summary>
        /// 录波仪横坐标卡点
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="isCAN">是否为CAN</param>
        /// <param name="schannel">子通道</param>
        /// <param name="value1">-5到5</param>
        /// <param name="value2">-5到5</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscillograph_CursorPosition_X(int channel, bool isCAN, int schannel, double value1, double value2) { }


        /// <summary>
        /// 录波仪卡点单一
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="isCAN">是否为CAN</param>
        /// <param name="schannel">子通道</param>
        /// <param name="value">-5到5</param>
        /// <param name="isleft">是否为左边</param>
        /// <returns></returns>
        public virtual void Oscillograph_CursorPosition_X_Single(int channel, bool isCAN, int schannel, double value, bool isleft) { }


        /// <summary>
        /// 设置XY的通道指定
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="isCAN"></param>
        /// <param name="schannel"></param>
        /// <returns></returns>
        public virtual void Oscillograph_CursorPosition_XY(int channel, bool isCAN, int schannel) { }


        /// <summary>
        /// 录波仪卡点单一
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="isCAN">是否为CAN通道</param>
        /// <param name="schannel">子通道</param>
        /// <returns></returns>
        public virtual void Oscillograph_CursorPosition_SetChannel(int channel, bool isCAN, int schannel) { }
        /// <summary>
        /// 录波仪卡点按时间单一
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="time">时间为单位为ms</param>
        /// <param name="isleft">是否为左边</param>
        /// <returns></returns>
        public virtual void Oscillograph_CursorPosition_X_Time(int channel, double time, bool isleft) { }
        /// <summary>
        /// 录波仪纵坐标卡点
        /// </summary>
        /// <param name="trace">轨道</param>
        /// <param name="value1">-5到5</param>
        /// <param name="value2">-5到5</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscillograph_CursorPosition_Y(int trace, double value1, double value2) { }
        /// <summary>
        /// 录波仪纵坐标卡点
        /// </summary>
        /// <param name="trace">轨道</param>
        /// <param name="value1">实际的值</param>
        /// <param name="value2">实际的值</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscillograph_CursorPosition_Y_Value(int trace, double value1, double value2) { }

        /// <summary>
        /// 回读光标参数
        /// </summary>
        /// <param name="channel">通道(1、2、3、4等)</param>
        /// <returns>返回double[5]。第一个为时间差，第二个为1/时间差=频率单位为Hz，第三个为横坐标左边值，第四个为横坐标右边值，第五个为幅度差。时间单位为ms</returns>
        public virtual double[] Oscillograph_ReadCursors(int channel) { return null; }

        /// <summary>
        /// 通道时间横向测量光标参数读取
        /// </summary>
        /// <returns>返回double 时间差 时间单位为ms</returns>
        public virtual double Oscillograph_ReadCurcor() { return 0; }
        /// <summary>
        /// 控制录波仪是否能触屏
        /// </summary>
        /// <param name="isTouch">能否触屏</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscillograph_Touch(bool isTouch) { }

        /// <summary>
        /// 设置录波仪最大存储深度
        /// </summary>
        /// <param name="storagedepth">存储深度，目前默认1M为1000000</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscillograph_StorageDepth(string storagedepth) { }



        /// <summary>
        /// 设置录波仪的模式，默认为示波器模式
        /// </summary>
        /// <param name="type">0为示波器模式，1为记录仪模式</param>
        /// <returns></returns>
        public virtual void Oscillograph_SetType(int type) { }


        /// <summary>
        /// 设置录波仪的组设置
        /// </summary>
        /// <param name="group">显示第几个组</param>
        /// <param name="format">格式</param>
        /// <param name="GRATicule">Grid为0,CROSshair为1,FRAMe为2</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscillograph_SetGroup(int group, int format, int GRATicule) { }





            /// <summary>
            /// 设置录波仪的组设置
            /// <param name="group">显示第几个组</param>
            /// <param name="format">格式</param>
            /// <param name="GRATicule">Grid为0,CROSshair为1,FRAMe为2</param>
            /// <param name="FGRid">精品栅格是否开启</param>
            /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscillograph_SetGroup2(int group, int format, int GRATicule, bool FGRid) { }

            /// <summary>
            /// 设置录波仪的精品栅格
            /// <param name="group">显示第几个组</param>
            /// <param name="format">格式</param>
            /// <param name="FGRid">精品栅格是否开启</param>
            /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscillograph_SetGroup3(int group, int format, bool FGRid) { }

            /// <summary>
            /// 设置录波仪的组设置
            /// </summary>
            /// <param name="group">显示第几个组</param>
            /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscillograph_Group_Show(int group) { }

            /// <summary>
            /// 设置录波仪的组通道设置
            /// </summary>
            /// <param name="trace">编号</param>
            /// <param name="mapping">映射</param>
            /// <param name="group">显示第几个组</param>
            /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscillograph_SetGroup_Channel(int trace, int mapping, int group) { }



            /// <summary>
            /// CAN通道设置
            /// </summary>
            /// <param name="channel">通道号</param>
            /// <param name="BaudRate">比特率=波特率，默认250kbps</param>
            /// <param name="IsListenOnly">只听开关，默认开</param>
            /// <param name="Terminator">终端电阻开关，默认关</param>
            /// <returns></returns>
        public virtual void Oscillograph_SetCan_Channel(int channel, string BaudRate, bool IsListenOnly, bool Terminator) { }




            /// <summary>
            /// CAN子通道设置
            /// </summary>
            /// <param name="channel">通道1</param>
            /// <param name="schannel">子通道</param>
            /// <param name="isOpen">是否开启</param>
            /// <returns></returns>
        public virtual void Oscillograph_SetCanChild_Open(int channel, int schannel, bool isOpen) { }


            /// <summary>
            /// CAN子通道设置
            /// </summary>
            /// <param name="channel">通道1</param>
            /// <param name="schannel">子通道</param>
            /// <param name="TRACE">显示不同的轨道</param>
            /// <param name="isOpen">是否开启</param>
            /// <param name="SRATE"> 采样率 单位为1S/s</param>
            /// <param name="Label">标签</param>
            /// <param name="MFORmat">标准帧/扩展帧，默认扩展帧，0和1，0为扩展帧</param>
            /// <param name="Mid">帧ID</param>
            /// <param name="ByteLength">字节长度，默认自动or长度？Auto为自动把</param>
            /// <param name="Start">起始位编号 </param>
            /// <param name="ByteCnt">位数</param>
            /// <param name="ByteOrder">字节顺序，默认little,big，0为低字节，1为高字节</param>
            /// <param name="ValueType">值定义：默认无符号整型，有符号整型，浮点，逻辑</param>
            /// <param name="Factor">值系数：如0.1</param>
            /// <param name="Offect">偏置，如-400</param>
            /// <param name="Unit">单位：字符</param>
            /// <param name="UpLimit">显示上限</param>
            /// <param name="DownLimit">显示下限</param>
            /// <param name="Color">设置颜色</param>
            /// <param name="SGr1">显示在组1？</param>
            /// <param name="SGr2">显示在组2？</param>
            /// <param name="SGr3">显示在组3？</param>
            /// <param name="SGr4">显示在组4？</param>
            /// <returns></returns>
        public virtual void Oscillograph_SetCanChild_Channel(int channel, int schannel, int TRACE, bool isOpen, string SRATE, string Label, int MFORmat, string Mid, string ByteLength, int Start, int ByteCnt, int ByteOrder, int ValueType, string Factor, string Offect, string Unit, string UpLimit, string DownLimit, string Color, bool SGr1, bool SGr2, bool SGr3, bool SGr4) { }






            /// <summary>
            /// 刷新CAN子通道设置
            /// </summary>
            /// <param name="channel">通道1</param>
            /// <param name="schannel">子通道</param>
            /// <param name="Mid">帧ID</param>
            /// <returns></returns>
        public virtual void Oscillograph_SetCanChild_Channel_Renovate(int channel, int schannel, string Mid) { }

            /// <summary>
            /// 删除所有的Channel显示
            /// <param name="group">显示第几个组</param>
            /// </summary>
            /// <returns>返回try catch的错误，否则为空</returns>
        public virtual void Remove_All_Channel(int group) { }


            /// <summary>
            /// 取得实时的值
            /// </summary>
            /// <param name="channel">通道号</param>
            /// <param name="isCan">是否为CAN</param>
            /// <param name="schannel">子通道</param>
            /// <returns>返回值</returns>
        public virtual double GetChannelValue(int channel, bool isCan, int schannel) { return 0; }


            /// <summary>
            /// 计算卡点值
            /// </summary>
            /// <param name="data">回读的录波仪的点</param>
            /// <param name="upvalue">上升或下降的高值</param>
            /// <param name="downvalue"> 上升或下降的低值</param>
            /// <param name="uptype">0为上升，1为下降</param>
            /// <param name="isAC">是不是交流</param>
            /// <param name="errorrate">误差</param>
            /// <returns>返回double[2]，返回占屏幕的比例值,跟录波仪没有关系，自己根据录波仪回读的点来进行计算</returns>
        public virtual double[] Oscillograph_Points(double[] data, double upvalue, double downvalue, int uptype, bool isAC, double errorrate) { return null; }




            /// <summary>
            /// 计算卡点值单一
            /// </summary>
            /// <param name="data">回读的录波仪的点</param>
            /// <param name="value">上升或下降的比较值</param>
            /// <param name="uptype">0为上升，1为下降</param>
            /// <param name="isright"> 卡的点是否为右边</param>
            /// <param name="isAC">是否为交流</param>
            /// <param name="errorrate">误差</param>
            /// <returns>返回double，返回占屏幕的比例值，跟录波仪没有关系，自己根据录波仪回读的点来进行计算</returns>
        public virtual double Oscillograph_Points_Single(double[] data, double value, int uptype, bool isright, bool isAC, double errorrate) { return 0; }


        /// <summary>
        /// 计算卡点值单一
        /// </summary>
        /// <param name="data">回读的示波器的点</param>
        /// <param name="value">上升或下降的比较值</param>
        /// <param name="uptype">0为上升，1为下降</param>
        /// <param name="isright"> 卡的点是否为右边</param>
        /// <param name="isAC">是否为交流</param>
        /// <param name="errorrate">误差</param>
        /// <returns>返回double，返回占屏幕的比例值，跟示波器没有关系，自己根据示波器回读的点来进行计算</returns>
        public virtual double Oscillograph_Points_Single(double[] data, double value, int uptype, bool isright, bool isAC, double errorrate, double CheckCount) { return 0; }

            /// <summary>
            /// 计算卡点值单一取首个上升沿或者下降沿
            /// </summary>
            /// <param name="data">回读的录波仪的点</param>
            /// <param name="value">上升或下降的比较值</param>
            /// <param name="uptype">0为上升，1为下降</param>
            /// <param name="isright"> 卡的点是否为右边</param>
            /// <param name="isAC">是否为交流</param>
            /// <param name="errorrate">误差</param>
            /// <returns>返回double，返回占屏幕的比例值，跟录波仪没有关系，自己根据录波仪回读的点来进行计算</returns>
        public virtual double Oscillograph_Points_Single2(double[] data, double value, int uptype, bool isright, bool isAC, double errorrate, int? CheckCount) { return 0; }


            /// <summary>
            /// 计算卡点值单一取第一个等于某个值的点
            /// </summary>
            /// <param name="data">回读的示波器的点</param>
            /// <param name="value">值</param>
            /// <param name="isAC">是否为交流</param>
            /// <returns>返回double，返回占屏幕的比例值，跟示波器没有关系，自己根据示波器回读的点来进行计算,0-10</returns>
        public virtual double Oscillograph_Points_Single3(double[] data, double value, bool isAC) { return 0; }


            /// <summary>
            /// 计算卡点值单一多个
            /// </summary>
            /// <param name="data">回读的示波器的点</param>
            /// <param name="value">上升或下降的比较值</param>
            /// <param name="uptype">0为上升，1为下降</param>
            /// <param name="isright"> 卡的点是否为右边</param>
            /// <param name="isAC">是否为交流</param>
            /// <param name="errorrate">误差</param>
            /// <param name="Index">到多少个值了</param>
            /// <param name="Count">取第几个值</param>
            /// <returns>返回double，返回占屏幕的比例值，跟示波器没有关系，自己根据示波器回读的点来进行计算</returns>
        public virtual double Oscillograph_Points_Single_Multiple(double[] OldData, double value, int uptype, bool isright, bool isAC, double errorrate, ref int Index, int Count, int? CheckCount) { return 0; }


        /// <summary>
        /// 录波仪连接状态
        /// </summary>
        public virtual void Read_OscillographState() { }

        /// <summary>
        /// 设置通道的线性标尺
        /// </summary>
        /// <param name="channel">通道号</param>
        /// <param name="mode">模式AXB|OFF|P12|SHUNt</param>
        /// <param name="AVALue">A值</param>
        /// <param name="BVALue">B值</param>
        /// <param name="Unit">单位</param>
        /// <param name="DISPlaytype">显示模式EXPonent|FLOating</param>
        public virtual void Oscillograph_STRain_LSCale(int channel, string mode, double AVALue, double BVALue, string Unit, string DISPlaytype) { }
    }
}
