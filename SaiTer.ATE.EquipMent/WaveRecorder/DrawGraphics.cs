using SaiTer.ATE.DataModel.WaveRecoder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using ZedGraph;
using NPOI.SS.Formula.Functions;
using System.Reflection;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 绘制录波仪的波形
    /// </summary>
    public class DrawGraphics
    {
        private ZedGraphControl graph;
        private GraphPane graphPane;
        private double Max_X = 0;//X轴最大值（用于设置光标）
        private double Max_Y = 0;//Y轴最大值（用于设置光标）
        private double Min_Y = 0;//Y轴最小值（用于设置光标）
        private int iCursorType = 0;//光标类型：0  X轴  1  Y轴
        private long Datacount = 0;
        

        public DrawGraphics(ZedGraphControl g)
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
            //graphPane.XAxis.Title.Text = "时间";
            //graphPane.YAxis.Title.Text = "刻度值" + "（div）";
            graphPane.XAxis.Title.Text = "Time";
            graphPane.YAxis.Title.Text = "Scale" + "(div)";
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
                double aa=pls[i].Max().X;
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
                //LineItem line = graphPane.AddCurve(WaveDatas[i].LineName, pls[i], WaveDatas[i].LineColor, SymbolType.Square);
                LineItem line = graphPane.AddCurve(WaveDatas[i].LineName, pls[i], WaveDatas[i].LineColor, SymbolType.None);
                line.Line.Width = 3F;
                line.Symbol.Size = 4F;
                Datacount = pls[i].Count;

                //if (Max_X < pls[i].Max().X)
                //{
                //    Max_X = pls[i].Max().X;
                //}
                //if (Max_Y < pls[i].Max().Y)
                //{
                //    Max_Y = pls[i].Max().Y;
                //}
            }

            graph.GraphPane.AxisChange();
            graph.AxisChange();
            graph.Refresh();
        }

        public void ZedGClear()
        {
            Max_X = 0;
            Max_Y = 0;
            graphPane.CurveList.Clear();
            graphPane.GraphObjList.Clear();

            graph.GraphPane.AxisChange();
            graph.AxisChange();
            graph.Refresh();
        }

        public void SetMaxX(double dmx)
        {
            Max_X = dmx;
        }

        public void SetMaxY(double dmy)
        {
            Max_Y = dmy;
        }

        public void SetMinY(double dmy)
        {
            Min_Y = dmy;
        }

        /// <summary>
        /// 设置光标
        /// </summary>
        /// <param name="dPosition">位置</param>
        /// <param name="iType">类型：0，X轴    1：Y轴</param>
        public void SetCursor(double dPosition,int iIndex, int iType = 0)
        {
            iCursorType = iType;
            StringBuilder sbtmp = new StringBuilder();
            PointPairList pps = new PointPairList();
            double dtmpValue = 0;
            if (iType == 1)//Y轴
            {
                dtmpValue = ((double)(Max_X / 10)) * 11;
                for (int i = 0; i < 12; i++)
                {
                    pps.Add(new PointPair((i * dtmpValue / 11), dPosition));
                }
                sbtmp.Append("Y" + iIndex.ToString() + ":" + dPosition);
            }
            else//X轴
            {
                dtmpValue = ((double)(Max_Y / 10)) * 11;
                pps.Add(new PointPair(dPosition, Min_Y - (dtmpValue / 11)));//这里加个最低点
                pps.Add(new PointPair(dPosition, Max_Y + (dtmpValue / 11)));//这里加个最低点
                //for (int i = 0; i < 12; i++)//用实线这里不用多加点
                //{
                //    pps.Add(new PointPair(dPosition, (i * dtmpValue / 11)));
                //}
                sbtmp.Append("X" + iIndex.ToString() + ":" + dPosition);
            }

            LineItem line = graphPane.AddCurve("", pps, Color.Gray, SymbolType.VDash);
            //line.Line.StepType = StepType.ForwardSegment;//虚线

            //这里增加标签
            TextObj text = new TextObj(sbtmp.ToString(), dPosition, Max_Y / 2 + (iIndex * (Max_Y / 10)), CoordType.AxisXYScale, AlignH.Left, AlignV.Center);//设置标签要显示的内容和位置
            text.FontSpec.Border.IsVisible = true;//添加标签的边框
            text.FontSpec.Fill.IsVisible = true;//对标签填充颜色
            text.FontSpec.Fill.Color = System.Drawing.Color.WhiteSmoke;
            text.FontSpec.Angle = 0;//横向显示
            graph.GraphPane.GraphObjList.Add(text);//标签添加到图上

            graph.GraphPane.AxisChange();
            graph.AxisChange();
            graph.Refresh();
        }

        public void ShowCursorInfo(double dc1,double dc2)
        {
            StringBuilder sbtmp = new StringBuilder();
            if (iCursorType == 1)
            {
                sbtmp.Append("ΔY:" + (Math.Abs(dc2 - dc1)).ToString());
            }
            else
            {
                sbtmp.Append("ΔX:" + (Math.Abs(dc2 - dc1)).ToString());
            }
            //这里增加标签
            TextObj text = new TextObj(sbtmp.ToString(), (dc2 + dc1) /2, 0, CoordType.AxisXYScale, AlignH.Left, AlignV.Center);//设置标签要显示的内容和位置
            text.FontSpec.Border.IsVisible = true;//添加标签的边框
            text.FontSpec.Fill.IsVisible = true;//对标签填充颜色
            text.FontSpec.Fill.Color = System.Drawing.Color.WhiteSmoke;
            text.FontSpec.Angle = 0;//横向显示
            graph.GraphPane.GraphObjList.Add(text);//标签添加到图上

            //这里增加缩放
            double dmin = 0;
            double dmax = 0;
            double dCz = Math.Abs(dc1 - dc2);
            if(Math.Abs(dc1-dc2)< Datacount * 0.1)
            {
                dCz = Datacount * 0.1;
            }

            if (dc1 < dc2)
            {
                dmin = dc1 - dCz;
                dmax = dc2 + dCz;
            }
            else
            {
                dmin = dc2 - dCz;
                dmax = dc1 + dCz;
            }

            //坐标处理
            dmin = dmin < 0 ? 0 : dmin;
            dmax = dmax < 0 ? 0 : dmax;
            dmin = dmin >Datacount  ? Datacount : dmin;
            dmax = dmax > Datacount ? Datacount : dmax;

            graph.GraphPane.XAxis.Scale.Min = dmin;
            graph.GraphPane.XAxis.Scale.Max = dmax;

            //刷新图形
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
