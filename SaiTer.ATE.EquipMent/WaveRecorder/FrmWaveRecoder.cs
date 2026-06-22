using SaiTer.ATE.DataModel.WaveRecoder;
using Sunny.UI;
using Sunny.UI.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZedGraph;

namespace SaiTer.ATE.EquipMent.WaveRecorder
{
    public partial class FrmWaveRecoder : UIForm
    {
        private DrawGraphics _drawGraphics;
        List<WaveData> wds = new List<WaveData>();
        List<PointPairList> listPPL = new List<PointPairList>();
        public FrmWaveRecoder()
        {
            InitializeComponent();
            InitForm();
        }

        private void InitForm()
        {
            _drawGraphics = new DrawGraphics(zgc_bx);
            _drawGraphics.Init();
        }

        private Color GetLineColorChnel(int lineIndex)
        {
            switch (lineIndex)
            {
                case 1: return Color.Yellow;
                case 2: return Color.Green;
                case 3: return Color.Red;
                case 4: return Color.DarkBlue;
                case 5: return Color.Aqua;
                case 6: return Color.MediumVioletRed;
                case 7: return Color.LawnGreen;
                case 8: return Color.LightBlue;
                case 9: return Color.Blue;
                case 10: return Color.Tomato;
                case 11: return Color.DarkOrange;
                case 12: return Color.Chocolate;
                case 13: return Color.Brown;
                case 14: return Color.DarkKhaki;
                case 15: return Color.DimGray;
                case 16: return Color.DarkTurquoise;
                default: return Color.GreenYellow;
            }
        }

        public void InitWaveData(List<WaveData> datas)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    _InitWaveData(datas);
                }));
            }
            else
            {
                _InitWaveData(datas);
            }
        }


        public void _InitWaveData(List<WaveData> datas)
        {
            wds = datas;
            _drawGraphics.InitScaleAuto();
            _drawGraphics.ZedGClear();

            double RDMax = 0;//数据最大值
            double RDMin = 0;//数据最小值
            double RDCount = 0;//数据数量
            int iChnel = 0;
            int iSubChnel = 0;
            double tmpData = 0;
            listPPL = new List<PointPairList>();
            //List<WaveData> WaveDatas = new List<WaveData>();
            int icolor = 0;
            for (int i = 0; i < datas.Count; i++)
            {
                if (datas[i].LinePoints_Y.Count > 0)
                {
                    icolor++;//从1开始
                    iChnel = i + 1;
                    datas[i].LineColor = GetLineColorChnel(icolor);
                    if (RDCount < datas[i].LinePoints_Y.Count)
                    {
                        RDCount = datas[i].LinePoints_Y.Count;
                    }
                    //WaveData lptmp = new WaveData();
                    //lptmp.Channel = iChnel;
                    //lptmp.LineName = "通道" + (iChnel).ToString();
                    //lptmp.LineColor = GetLineColorChnel(icolor);
                    //lptmp.ZoomX = 1;
                    //WaveDatas.Add(lptmp);

                    //处理接收数据
                    PointPairList pplist = new PointPairList();//点的集合
                    for (int j = 0; j < datas[i].LinePoints_Y.Count; j++)
                    {

                        pplist.Add((j) * 1, datas[i].LinePoints_Y[j]);//时基从0开始

                        if (RDMax < datas[i].LinePoints_Y[j])
                        {
                            RDMax = datas[i].LinePoints_Y[j];
                        }

                        if (RDMin > datas[i].LinePoints_Y[j])
                        {
                            RDMin = datas[i].LinePoints_Y[j];
                        }
                    }
                    listPPL.Add(pplist);
                }
            }

            //画波形图
            //GetYScale(RDMax);
            _drawGraphics.SetMaxX(RDCount);
            _drawGraphics.SetMaxY(RDMax);
            _drawGraphics.SetMinY(RDMin);
            _drawGraphics.DrawPointListsNew(listPPL, datas);
            zgc_bx.Invalidate();

        }

        public void SetCursor(double dPosition,int iIndex,int iType)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    _drawGraphics.SetCursor(dPosition, iIndex, iType);
                }));
            }
            else
            {
                _drawGraphics.SetCursor(dPosition, iIndex, iType);
            }
        }

        public void ShowCursorInfo(double dc1,double dc2)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    _drawGraphics.ShowCursorInfo(dc1, dc2);
                }));
            }
            else
            {
                _drawGraphics.ShowCursorInfo(dc1, dc2);
            }
        }

        public bool SaveImage(string sPath)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        Image img = zgc_bx.GetImage();
                        img.Save(sPath, System.Drawing.Imaging.ImageFormat.Bmp);
                    }));
                }
                else
                {
                    Image img = zgc_bx.GetImage();
                    img.Save(sPath, System.Drawing.Imaging.ImageFormat.Bmp);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.InitWaveData(wds);
        }
    }
}
