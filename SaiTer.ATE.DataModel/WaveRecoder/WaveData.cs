using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.WaveRecoder
{
    public class WaveData
    {
        /// <summary>
        /// 通道编号
        /// </summary>
        public int Channel { get; set; }
        /// <summary>
        /// 子通道
        /// </summary>
        public int SubChannel { get; set; }
        /// <summary>
        /// 通道类型（0：常规通道，1：数字通道）
        /// </summary>
        public int ChannelType { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string LineName { get; set; }
        /// <summary>
        /// 颜色
        /// </summary>
        public Color LineColor { get; set; }
        /// <summary>
        /// 缩放倍数
        /// </summary>
        public double ZoomX;
        /// <summary>
        /// X坐标集合
        /// </summary>
        public List<double> LinePoints_X { get; set; }
        /// <summary>
        /// Y坐标集合
        /// </summary>
        public List<double> LinePoints_Y { get; set; }

        public WaveData()
        {
            Channel = 0;
            SubChannel = 0;
            ChannelType = 0;
            LineName = "";
            LineColor = Color.White;
            ZoomX = 1;
            LinePoints_X = new List<double>();
            LinePoints_Y = new List<double>();
        }
    }
}
