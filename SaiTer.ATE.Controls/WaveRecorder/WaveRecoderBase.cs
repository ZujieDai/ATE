using SaiTer.ATE.DataModel.WaveRecoder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Controls.WaveRecorder
{
    public class WaveRecoderBase : ControlsBase
    {
        public virtual void ReadWaveRecoderBoard_StateData(List<int> lstIDs) { }


        /// <summary>
        /// 设置采样率
        /// </summary>
        /// <param name="data">目前支持1，10，100   单位k</param>
        public virtual void WaveRecoder_SetSamplingRate(List<int> lstIDs, double data) { }
        /// <summary>
        /// 录波板启动录波
        /// </summary>
        public virtual void WaveRecoder_Start(List<int> lstIDs) { }
        /// <summary>
        /// 录波板停止录波
        /// </summary>
        public virtual void WaveRecoder_Stop(List<int> lstIDs) { }
        /// <summary>
        /// 通道波形数据读取
        /// </summary>
        /// <param name="channnel"> 通道(1、2)</param>
        /// <returns>返回为波形数据</returns>
        public virtual void WaveRecoder_ReadChannelData(List<int> lstIDs, int channnel, ref WaveData data, string linName="") { data = null; }
        /// <summary>
        /// 数字通道波形数据读取
        /// </summary>
        /// <param name="channnel">通道</param>
        /// <param name="subchannel">子通道</param>
        /// <param name="data"></param>
        public virtual void WaveRecoder_ReadDigitalChannelData(List<int> lstIDs, int channnel, int subchannel, ref WaveData data, string linName = "") { data = null; }

        /// <summary>
        /// 设置光标
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="iCursor"></param>
        /// <param name="dPoint"></param>
        public virtual void WaveRecoder_SetCursor(List<int> lstIDs, int iCursor, double dPoint) { }
        /// <summary>
        /// 获取光标位置及光标间的时间差
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="iType">数据类型，0：默认光标之间的时间差    1：光标1的时间     2：光标2的时间</param>
        /// <returns></returns>
        public virtual Dictionary<int, double> WaveRecoder_GetCursorData(List<int> lstIDs,int iType=0) { return null; }


        /// <summary>
        /// 录波板截图
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <returns>主要返回示波器截屏存储到电脑上的存储路径，一般是存储在程序运行
        /// 目录下的报表(勿删)\Image\年\月\年-月-日\生成的截屏名称.jpg
        /// 否则返回为try catch 的错误，没有则返回为空</returns>
        public virtual Dictionary<int, string> WaveRecoderSaveScreen(List<int> lstIDs) { return null; }
    }
}
