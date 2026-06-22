using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZedGraph;
using System.Drawing;
using SaiTer.ATE.DataModel.WaveRecoder;

namespace SaiTer.ATE.UI.Assitand.WaveRecoder
{
    /// <summary>
    /// 绘制录波仪的波形
    /// </summary>
    public class DrawGraphicsDL
    {
        private ZedGraphControl graph;
        private GraphPane graphPane;

        public DrawGraphicsDL(ZedGraphControl g)
        {
            this.graph = g;
            Init();
        }

        public void Init()
        {
            graphPane = graph.GraphPane;
            graphPane.Title.Text = "";
            //graphPane.XAxis.Title.Text = "时间（ms）";
            //graphPane.XAxis.Title.Text = "时间";
            //graphPane.YAxis.Title.Text = "刻度值（div）";
            graphPane.XAxis.Title.Text = "时间";
            graphPane.YAxis.Title.Text = "刻度值" + "（div）";
            //if (Prj.Prj.MainController.sLanguage=="CH")
            //{
            //    graphPane.XAxis.Title.Text = "时间";
            //    graphPane.YAxis.Title.Text = "刻度值（div）";
            //}
            //else
            //{
            //    graphPane.XAxis.Title.Text = "Time";
            //    graphPane.YAxis.Title.Text = "Scale（div）";
            //}
            //graphPane.XAxis.Scale.Format = "00:00:00:000";
            //graphPane.XAxis.Scale.Max = 100;
            graphPane.XAxis.Scale.Min = 0;
            //graphPane.XAxis.Scale.MajorStep = DL850GetData.IntervalTime;
            //graphPane.XAxis.Scale.MinorStep = 10;
            //graphPane.YAxis.Scale.Max = 20;
            //graphPane.YAxis.Scale.Min = 0;
            //graphPane.YAxis.Scale.MajorStep = 5;
            //graphPane.YAxis.Scale.MinorStep = 0;



            //坐标值显示是否允许重叠，如果False的话，控件会根据坐标值长度自动消除部分坐标值的显示状态
            graphPane.XAxis.Scale.IsPreventLabelOverlap = true;
            graphPane.YAxis.Scale.IsPreventLabelOverlap = true;

            ////显示网格虚线
            graphPane.YAxis.MajorGrid.IsVisible = false;
            graphPane.XAxis.MajorGrid.IsVisible = false;

            //设置背景透明
            graphPane.Chart.Fill = new Fill(System.Drawing.Color.Transparent, System.Drawing.Color.Transparent, 45F);



            //鼠标经过图表上的点时是否显示该点所对应的值 默认为false 
            graph.IsShowPointValues = true;

            //是否允许纵向缩放
            graph.IsEnableVZoom = false;
        }

        /// <summary>
        /// 设置Y轴刻度
        /// </summary>
        /// <param name="YmaxSca">最大刻度</param>
        /// <param name="YmajSca">每一小格最大刻度</param>
        public void InitScale(double YmaxSca, double YmajSca)
        {
            graphPane = graph.GraphPane;
            graphPane.YAxis.Scale.Max = YmaxSca;
            graphPane.YAxis.Scale.Min = (YmaxSca / -1);
            graphPane.YAxis.Scale.MajorStep = YmajSca;
            graphPane.YAxis.Scale.MinorStep = 0;
        }

        /// <summary>
        /// 设置Y轴刻度
        /// </summary>
        /// <param name="YmaxSca">最大刻度</param>
        /// <param name="YmajSca">每一小格最大刻度</param>
        public void InitScaleAC(double YmaxSca, double YmajSca)
        {
            graphPane = graph.GraphPane;
            graphPane.YAxis.Scale.Max = YmaxSca;
            graphPane.YAxis.Scale.Min = (YmaxSca / -1);
            graphPane.YAxis.Scale.MajorStep = YmajSca;
            graphPane.YAxis.Scale.MinorStep = 0;
        }

        public void DrawPointList(PointPairList PoList)
        {
            graphPane.CurveList.Clear();
            graphPane.GraphObjList.Clear();
            graphPane.Title.Text = "数据曲线图";

            LineItem _lineItem = graphPane.AddCurve("", PoList, Color.Red);
            _lineItem.Symbol.Fill = new Fill(Color.Red, Color.Red, 0.5F);
            _lineItem.Line.Width = 3F;
            _lineItem.Symbol.Size = 4F;

            graph.GraphPane.AxisChange();
            graph.AxisChange();
            graph.Refresh();

        }

        /// <summary>
        /// 画多条数据曲线
        /// </summary>
        /// <param name="pls"></param>
        /// <param name="lbyParas"></param>
        public void DrawPointLists(List<PointPairList> pls, List<WaveData> WaveDatas)
        {
            graphPane.CurveList.Clear();
            graphPane.GraphObjList.Clear();

            for (int i = 0; i < pls.Count; i++)
            {
                LineItem line = graphPane.AddCurve(WaveDatas[i].LineName, pls[i], WaveDatas[i].LineColor, SymbolType.None);
                line.Line.Width = 3F;
                line.Symbol.Size = 4F;
            }

            graph.GraphPane.AxisChange();
            graph.AxisChange();
            graph.Refresh();
        }

        /// <summary>
        /// 画多条数据曲线(凸显出各个点)
        /// </summary>
        /// <param name="pls"></param>
        /// <param name="lbyParas"></param>
        public void DrawPointListsNew(List<PointPairList> pls, List<WaveData> WaveDatas)
        {
            graphPane.CurveList.Clear();
            graphPane.GraphObjList.Clear();

            for (int i = 0; i < pls.Count; i++)
            {
                LineItem line = graphPane.AddCurve(WaveDatas[i].LineName, pls[i], WaveDatas[i].LineColor, SymbolType.Square);
                line.Line.Width = 3F;
                line.Symbol.Size = 4F;
            }

            graph.GraphPane.AxisChange();
            graph.AxisChange();
            graph.Refresh();
        }

        public void ZedGClear()
        {
            graphPane.CurveList.Clear();
            graphPane.GraphObjList.Clear();

            graph.GraphPane.AxisChange();
            graph.AxisChange();
            graph.Refresh();
        }

        /// <summary>
        /// 设置X,Y轴自动调整为默认大小
        /// </summary>
        public void InitScaleAuto()
        {
            graphPane.XAxis.Scale.MinAuto = false;
            graphPane.XAxis.Scale.MaxAuto = true;
            graphPane.XAxis.Scale.MajorStepAuto = true;
            graphPane.XAxis.Scale.MinorStepAuto = true;
            graphPane.XAxis.CrossAuto = true;
            graphPane.XAxis.Scale.MagAuto = true;
            graphPane.XAxis.Scale.FormatAuto = true;


            graphPane.YAxis.Scale.MinAuto = true;
            graphPane.YAxis.Scale.MaxAuto = true;
            graphPane.YAxis.Scale.MajorStepAuto = true;
            graphPane.YAxis.Scale.MinorStepAuto = true;
            graphPane.YAxis.CrossAuto = true;
            graphPane.YAxis.Scale.MagAuto = true;
            graphPane.YAxis.Scale.FormatAuto = true;


            graph.GraphPane.AxisChange();
            graph.AxisChange();
            graph.Refresh();
        }

    }
}
