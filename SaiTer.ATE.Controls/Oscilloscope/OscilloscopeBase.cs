using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Controls
{
     

    public abstract class OscilloscopeBase : ControlsBase
    {

        #region---------------示波器----------------- 
        /// <summary>
        /// 示波器初始化
        /// </summary>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void OscilloscopeIDefalut(List<int> lstIDs) { }
        /// <summary>
        /// 设置采样模式
        /// </summary>
        /// <param name="mode">0=SAMple|1=AVErage</param>
        /// <param name="num">用于平均值的计算点数</param>
        public virtual void Oscilloscope_SetACQuireMode(List<int> lstIDs, int mode, int num = 0) { }
        /// <summary>
        /// 示波器通道设置
        /// </summary>
        /// <param name="lstIDs">需要设置的枪位号</param>
        /// <param name="channel">通道(比如1、2、3)</param>
        /// <param name="gear">纵坐标档位  例子:10,20,50,100,200，现在单位为V，要根据探头比来相应设置)</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscilloscope_Channel_SetGear(List<int> lstIDs, int channel, string gear) { }
        /// <summary>
        /// 示波器通道设置
        /// </summary>
        /// <param name="lstIDs">需要设置的枪位号</param>
        /// <param name="channel">通道  比如1、2、3)</param>
        /// <param name="isOpen">是否开启当前通道</param>
        /// <param name="coupling">通道耦合  比如AC、DC)，测试项 纹波测试项AC,其它测试项基本为DC</param>
        /// <param name="tapewidth"> 带宽  比如20M、200M、FULL)</param>
        /// <param name="probe"> 探头比  比如100、200、0.01、0.02,100代表放大，0.01代表缩小)</param>
        /// <param name="tag">标签  纯文本英文)</param>
        /// <param name="impedance">阻抗  默认1M、50欧)</param>
        /// <param name="unit">单位  V或者A，代表电压或者电流)</param>
        /// <param name="isOpen_A">通道反相是否开启</param>
        /// <param name="gear">纵坐标档位  例子:10,20,50,100,200，现在单位为V，要根据探头比来相应设置)</param>
        /// <param name="position">纵坐标位置  根据实际纵坐标格子来算，一般是（-（格子/2），格子/2）),实际会自动乘以换算挡位</param>
        public virtual void Oscilloscope_Channel_Set(List<int> lstIDs, int channel, bool isOpen, string coupling, string tapewidth, string probe, string tag, string impedance, string unit, bool isOpen_A, string gear, string position) { }

        /// <summary>
        /// 初始化测量设置，清除所有测量项
        /// </summary>
        public virtual void Oscilloscope_Measure_Initialize(List<int> lstIDs) { }

        /// <summary>
        /// 添加测量项
        /// </summary>
        /// <param name="MeasurementType">类型 看添加测量项参数说明)</param>
        /// <param name="channel"></param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscilloscope_AddMeasure(List<int> lstIDs, string MeasurementType, int channel) { }
        /// <summary>
        /// 读取第几个测量值    
        /// </summary>
        /// <param name="MeasureNumber">第几个</param>
        /// <returns></returns>
        public virtual Dictionary<int, string> Oscilloscope_ReadMeasure(List<int> lstIDs, int MeasureNumber) { return null; }

        /// <summary>
        /// 按类型读取测量值
        /// </summary>
        /// <param name="MeasureType">测量值类型</param>
        /// <param name="channel">通道号  1,2)</param>
        /// <returns>返回为try catch 的错误，没有则返回为空<</returns>
        public virtual Dictionary<int, string> Oscilloscope_ReadMeasure(List<int> lstIDs, string MeasureType, int channel, bool isIMMed = false) { return null; }
        /// <summary>
        /// 示波器时基设置
        /// </summary> 
        /// <param name="isroll">是否滚动</param>
        /// <param name="timebase">时基（200、400，暂定ms为单位）</param>
        /// <param name="delay">   0、1，暂定s为单位，默认0)</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscilloscope_TimeBase(List<int> lstIDs, bool isroll, string timebase, string delay) { }
        /// <summary>
        /// 示波器触发
        /// </summary>
        /// <param name="type_first">主类型  边沿或者超时,0为边沿，1为超时，2欠幅或矮脉冲，以后再加要用到的定义)</param>
        /// <param name="type_second">次类型  上升沿或者下降沿,RISE,FALL,Alternating)</param>
        /// <param name="coupling">触发耦合  默认直流,AC,DC)</param>
        /// <param name="timeout_type">超时类型  一般默认边沿,EDGE)</param>
        /// <param name="timeout">超时时间  ms为单位，配合超时触发用)</param>
        /// <param name="channel">通道  1、2)</param>
        /// <param name="triggerLevel">触发电平  0mV,1V,2V)</param>
        /// <param name="triggerType">触发类型  Auto、Normal、Single)  自动、普通、单次)</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscilloscope_Trigger(List<int> lstIDs, int type_first, string type_second, string coupling, string typeout_type, string timeout, int channel, string triggerLevel, string triggerType) { }

        /// <summary>
        /// 示波器触发类型设置
        /// </summary>
        /// <param name="TriggerType">触发类型  Auto、Normal、Single)  自动、普通、单次)</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscilloscope_TriggerTypeSet(List<int> lstIDs, string TriggerType) { }
        /// <summary>
        /// 读是否触发
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="isTrigger"></param>
        public virtual Dictionary<int, bool> ReadTriggerState(List<int> lstIDs) { return null; }

        /// <summary>
        /// 触发状态回读
        /// </summary>
        /// <returns>返回触发状态,0为stop,1为等待触发,-1为错误,2为其他状态</returns>
        public virtual Dictionary<int, int> Oscilloscope_ReadTrigger(List<int> lstIDs) { return null; }
        /// <summary>
        /// 波形数据回读
        /// </summary>
        /// <param name="channnel"> 通道(  1、2)</param>
        /// <returns>返回为(枪位号-波形数据)的字典</returns>
        public virtual Dictionary<int, double[]> OscilloscopeCursorData(List<int> lstIDs, int channnel, int RecondLengZero = 10000 * 20) { return null; }

        /// <summary>
        /// 截取示波器屏幕
        /// </summary>
        /// <returns>主要返回示波器截屏存储到电脑上的存储路径，(枪位号，路径)字典
        /// 一般是存储在程序运行
        /// 目录下的报表( 勿删)\Image\年\月\年-月-日\生成的截屏名称.jpg
        /// 否则返回为try catch 的错误，没有则返回为空</returns>
        public virtual Dictionary<int, string> OscilloscopeSaveScreen(List<int> lstIDs) { return null; }

        /// <summary>
        /// 示波器启停控制
        /// </summary>
        /// <param name="isRun">是否运行</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscilloscope_IsRun(List<int> lstIDs, bool isRun) { }
        /// <summary>
        /// 启停状态回读
        /// </summary>
        /// <returns>true运行，false没有运行</returns>
        public virtual void Oscilloscope_ReadRun(List<int> lstIDs, ref bool isrun) { isrun = false; }


        /// <summary>
        /// 示波器光标类型设置
        /// </summary>
        /// <param name="type">参数为X、Y、XY</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscilloscope_CursorsSet(List<int> lstIDs, string type) { }
        /// <summary>
        /// 示波器横坐标卡点
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="value1">左边点1( 0到100的比例，屏幕占比)</param>
        /// <param name="value2">右边点2( 0到100的比例，屏幕占比)</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscilloscope_CursorPosition_X(List<int> lstIDs, int channel, double value1, double value2) { }        /// <summary>


        /// <summary>
        /// 示波器卡点单一
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="value1">( 0到100的比例，屏幕占比)</param>
        /// <param name="isleft">是否为左边</param>
        /// <returns></returns>
        public virtual void Oscilloscope_CursorPosition_X_Single(List<int> lstIDs, int channel, Dictionary<int, double> values, bool isleft) { }

        /// <summary>
        /// 示波器卡点按时间单一
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="time">时间为单位为ms</param>
        /// <param name="isleft">是否为左边</param>
        /// <returns></returns>
        public virtual void Oscilloscope_CursorPosition_X_Time(List<int> lstIDs, int channel, double time, bool isleft) { }
        /// <summary>
        /// 示波器纵坐标卡点
        /// </summary>
        /// <param name="channel">通道( 1、2、3、4等)</param>
        /// <param name="value1">下边点1( 0到100的比例，屏幕占比)</param>
        /// <param name="value2">上边点2( 0到100的比例，屏幕占比)</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscilloscope_CursorPosition_Y(List<int> lstIDs, int channel, double value1, double value2) { }
        /// <summary>
        /// 示波器纵坐标卡点
        /// </summary>
        /// <param name="channel">通道( 1、2、3、4等)</param>
        /// <param name="value">数值</param>
        /// <param name="cursorIndex">1=Y1;2=Y2</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscilloscope_CursorPosition_Y(List<int> lstIDs, int channel, double value, int cursorIndex) { }

        /// <summary>
        /// 回读光标参数
        /// </summary>
        /// <param name="channel">通道( 1、2、3、4等)</param>
        /// <returns>返回double[5]。第一个为时间差，第二个为1/时间差=频率单位为Hz，第三个为横坐标左边值，第四个为横坐标右边值，第五个为幅度差。时间单位为ms</returns>
        public virtual void Oscilloscope_ReadCursors(List<int> lstIDs, int channel, ref Dictionary<int, double[]> Cursors) { Cursors = null; }
        /// <summary>
        /// 控制示波器是否能触屏
        /// </summary>
        /// <param name="isTouch">能否触屏</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscilloscope_Touch(List<int> lstIDs, bool isTouch) { }

        /// <summary>
        /// 设置示波器最大存储深度
        /// </summary>
        /// <param name="storagedepth">存储深度，目前已有参数10k、50k、250k、1.25M、2.5M、12.5M、25M、125M、250M</param>
        /// <returns></returns>
        public virtual void Oscilloscope_StorageDepth(List<int> lstIDs, string storagedepth) { }
        /// <summary>
        /// 示波器时基设置
        /// </summary>
        /// <param name="isroll">是否滚动</param>
        /// <returns>返回为try catch 的错误，没有则返回为空</returns>
        public virtual void Oscilloscope_IsRoll(List<int> lstIDs, bool isroll) { }
        /// <summary>
        /// 计算卡点值
        /// </summary>
        /// <param name="data">回读的示波器的点</param>
        /// <param name="upvalue">上升或下降的高值</param>
        /// <param name="downvalue"> 上升或下降的低值</param>
        /// <param name="uptype">0为上升，1为下降</param>
        /// <param name="isAC">是不是交流</param>
        /// <returns>返回double[2]，返回占屏幕的比例值,跟示波器没有关系，自己根据示波器回读的点来进行计算</returns>
        public virtual void Oscilloscope_Points(List<int> lstIDs, double[] data, double upvalue, double downvalue, int uptype, bool isAC, ref double[] position) { position = null; }




        /// <summary>
        /// 计算卡点值单一
        /// </summary>
        /// <param name="data">回读的示波器的点</param>
        /// <param name="value">上升或下降的比较值</param>
        /// <param name="uptype">0为上升，1为下降</param>
        /// <param name="isright"> 卡的点是否为右边</param>
        /// <param name="isAC">是否为交流</param>
        /// <returns>返回double，返回占屏幕的比例值，跟示波器没有关系，自己根据示波器回读的点来进行计算</returns>
        public virtual void Oscilloscope_Points_Single(List<int> lstIDs, Dictionary<int, double[]> data, double value, int uptype, bool isright, bool isAC,ref Dictionary<int, double> Position) { }

        /// <summary>
        /// 计算卡点值单一
        /// </summary>
        /// <param name="data">回读的示波器的点</param>
        /// <param name="value">上升或下降的比较值</param>
        /// <param name="uptype">0为上升，1为下降</param>
        /// <param name="isright"> 卡的点是否为右边</param>
        /// <param name="isAC">是否为交流</param>
        /// <param name="count">连续多少个点</param>
        /// <returns>返回double，返回占屏幕的比例值，跟示波器没有关系，自己根据示波器回读的点来进行计算</returns>
        public virtual void Oscilloscope_Points_Single_AC(List<int> lstIDs, double[] data, double value, int uptype, bool isright, bool isAC, int count, ref double position) { position = -1; }

        /// <summary>
        /// 设置通道自动调整
        /// </summary>
        /// <param name="channel">通道(1、2、3、4等)</param>
        public virtual void Oscilloscope_AutoZero(List<int> lstIDs, int channel) { }

        #endregion
    }
}
