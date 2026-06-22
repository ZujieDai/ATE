using SaiTer.ATE.DataModel.WaveRecoder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 录波板、录波仪相关虚函数
    /// </summary>
    public partial class EquipMentBase
    {
        /// <summary>
        /// 光标1
        /// </summary>
        public double Cursor1 = 0;
        /// <summary>
        /// 光标2
        /// </summary>
        public double Cursor2 = 0;
        /// <summary>
        /// 光标间的时间差
        /// </summary>
        public double Time_Cursor = 0;
        /// <summary>
        /// 是否有设置光标
        /// </summary>
        public bool IsCursor = false;

        public virtual void ReadWaveRecoderBoard_StateData() { }


        /// <summary>
        /// 设置采样率
        /// </summary>
        /// <param name="data">目前支持1，10，100   单位k</param>
        public virtual void WaveRecoder_SetSamplingRate(double data) { }
        /// <summary>
        /// 录波板启动录波
        /// </summary>
        public virtual void WaveRecoder_Start() { }
        /// <summary>
        /// 录波板停止录波
        /// </summary>
        public virtual void WaveRecoder_Stop() { }
        /// <summary>
        /// 通道波形数据读取
        /// </summary>
        /// <param name="channnel"> 通道(1、2)</param>
        /// <returns>返回为波形数据</returns>
        public virtual void WaveRecoder_ReadChannelData(int channnel, ref WaveData data) { data = null; }
        /// <summary>
        /// 数字通道波形数据读取
        /// </summary>
        /// <param name="channnel">通道</param>
        /// <param name="subchannel">子通道</param>
        /// <param name="data"></param>
        public virtual void WaveRecoder_ReadDigitalChannelData(int channnel, int subchannel, ref WaveData data) { data = null; }
        /// <summary>
        /// 设置光标位置
        /// </summary>
        /// <param name="iCursor">光标序号(1、左边，2、右边)</param>
        /// <param name="dPoint"></param>
        public void WaveRecoder_SetCursor(int iCursor, double dPoint) 
        {
            IsCursor = true;
            if (iCursor == 1)
            {
                Cursor1 = dPoint;
            }
            else
            {
                Cursor2 = dPoint;
            }
        }
        /// <summary>
        /// 获取光标有关数据（返回三个数据，光标1，光标2，光标之间的时间差）
        /// </summary>
        /// <returns></returns>
        public double WaveRecoder_GetCursorData(int iType)
        {

            //List<double> data = new List<double>();
            //data.Add(Cursor1);
            //data.Add(Cursor2);
            //data.Add(Math.Abs(Cursor1 - Cursor2));
            switch (iType)
            {
                case 0:
                    return Math.Abs(Cursor1 - Cursor2);
                case 1:
                    return Cursor1;
                case 2:
                    return Cursor2;
                default:
                    return Math.Abs(Cursor1 - Cursor2);
            }
        }

        /// <summary>
        /// 录波板截图
        /// </summary>
        /// <returns>主要返回示波器截屏存储到电脑上的存储路径，一般是存储在程序运行
        /// 目录下的报表(勿删)\Image\年\月\年-月-日\生成的截屏名称.jpg
        /// 否则返回为try catch 的错误，没有则返回为空</returns>
        public virtual void WaveRecoderSaveScreen(ref string path) { path = ""; }

        /// <summary>
        /// 加载波形数据
        /// </summary>
        /// <param name="waveDatas"></param>
        public virtual void WaveRecoder_InitWaveData(List<WaveData> waveDatas) { }

    }
}
