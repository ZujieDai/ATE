using SaiTer.ATE.UI.Assitand.WaveRecoder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZedGraph;
using SaiTer.ATE.DataModel.WaveRecoder;
using NPOI.SS.Formula.Functions;

namespace SaiTer.ATE.UI.UserControls.EquipOperate
{
    public partial class UcWaveRecoder : UcEquipOperateBase
    {
        private DrawGraphicsDL _drawGraphicsDL;
        public UcWaveRecoder()
        {
            InitializeComponent();
            InitForm();
        }

        private void InitForm()
        {
            _drawGraphicsDL = new DrawGraphicsDL(zgc_bx);
            _drawGraphicsDL.Init();

        }

        private void UcWaveRecoder_Load(object sender, EventArgs e)
        {
            GetChargerID();
        }

        private void btn_ShowData_Click(object sender, EventArgs e)
        {
            if (chk_AllChnel.CheckedItems.Count > 4)
            {
                MessageBox.Show("同时展示的通道数量最大不能超过【4】个通道，请确认通道数量！！！");
                return;
            }

            _drawGraphicsDL.InitScaleAuto();
            _drawGraphicsDL.ZedGClear();

            double RDMax = 0;
            int iChnel = 0;
            int iSubChnel = 0;
            double tmpData = 0;
            List<PointPairList> listPPL = new List<PointPairList>();
            List<WaveData> WaveDatas = new List<WaveData>();
            int icolor = 0;
            for (int i = 0; i < chk_AllChnel.Items.Count; i++)
            {
                if (chk_AllChnel.GetItemChecked(i) == true)
                {
                    icolor++;//从1开始
                    iChnel = i + 1;
                    WaveData lptmp = new WaveData();
                    lptmp.Channel = iChnel;
                    lptmp.LineName = "通道" + (iChnel).ToString();
                    lptmp.LineColor = GetLineColorChnel(icolor);
                    lptmp.ZoomX = 1;
                    WaveDatas.Add(lptmp);
                    //读取通道数据
                    EquipMentControl.WaveRecoderCtrl.WaveRecoder_ReadChannelData(lstChargerID, iChnel, ref lptmp);

                    //判断数据的合法性
                    if (lptmp == null || lptmp.LinePoints_Y.Count < 1)
                    {
                        MessageBox.Show("通道" + (iChnel).ToString() + "关闭，无数据，请确认设备通道状态！！！");
                        continue;
                    }

                    //格式化通道参数
                    double ZoomX = 1;
                    double BcValue = 0;
                    //处理接收数据
                    PointPairList pplist = new PointPairList();//点的集合
                    for (int j = 0; j < lptmp.LinePoints_Y.Count; j++)
                    {

                        pplist.Add((j) * 1, lptmp.LinePoints_Y[j]);//时基从0开始

                        if (RDMax < lptmp.LinePoints_Y[j])
                        {
                            RDMax = lptmp.LinePoints_Y[j];
                        }
                    }
                    listPPL.Add(pplist);
                }
            }

            //这里处理数字通道
            string[] sDigitalChannels = rtb_DCs.Text.Split(',');
            List<int> iDigitalChannels = new List<int>();
            List<int> iDigitalSubChannels = new List<int>();
            int ic = 0;//通道
            int isc = 0;//子通道
            for (int j = 0; j < sDigitalChannels.Length; j++)
            {
                string[] ssc = sDigitalChannels[j].Split('_');
                if (!int.TryParse(ssc[0], out ic))
                {
                    ic = 0;
                }
                if (ssc.Length > 1)
                {
                    if (!int.TryParse(ssc[1], out isc))
                    {
                        isc = 0;
                    }
                }

                if (int.TryParse(ssc[0], out ic))
                {
                    iDigitalChannels.Add(ic);
                    iDigitalSubChannels.Add(isc);
                }
            }

            if (iDigitalChannels.Count > 0)
            {
                for (int i = 0; i < iDigitalChannels.Count; i++)
                {
                    icolor++;
                    iChnel = iDigitalChannels[i];
                    iSubChnel = iDigitalSubChannels[i];
                    WaveData lptmp = new WaveData();
                    lptmp.Channel = iChnel;
                    lptmp.LineName = "数字通道" + (iChnel).ToString();
                    if (iSubChnel != 0)
                    {
                        lptmp.LineName = lptmp.LineName + "_" + iSubChnel.ToString();
                    }
                    lptmp.LineColor = GetLineColorChnel(icolor);// GetRandomColor();
                    lptmp.ZoomX = 1;
                    WaveDatas.Add(lptmp);
                    //读取通道数据
                    EquipMentControl.WaveRecoderCtrl.WaveRecoder_ReadDigitalChannelData(lstChargerID, iChnel, iSubChnel, ref lptmp);

                    //判断数据的合法性
                    if (lptmp.LinePoints_Y == null || lptmp.LinePoints_Y.Count < 1)
                    {
                        MessageBox.Show("数字通道" + (iChnel).ToString() + "关闭，无数据，请确认设备通道状态！！！");
                        continue;
                    }

                    //格式化通道参数
                    double ZoomX = 1;// double.Parse(sChnelPara[i].Split(',')[1]);
                    double BcValue = 0;// double.Parse(sChnelPara[i].Split(',')[2]);
                    //处理接收数据
                    PointPairList pplist = new PointPairList();//点的集合
                    for (int j = 0; j < lptmp.LinePoints_Y.Count; j++)
                    {

                        pplist.Add((j) * 1, lptmp.LinePoints_Y[j]);//时基从0开始

                        if (RDMax < lptmp.LinePoints_Y[j])
                        {
                            RDMax = lptmp.LinePoints_Y[j];
                        }
                    }
                    listPPL.Add(pplist);
                }
            }


            //画波形图
            //GetYScale(RDMax);
            //_drawGraphicsDL.DrawPointList(pplist);
            _drawGraphicsDL.DrawPointListsNew(listPPL, WaveDatas);
            zgc_bx.Invalidate();
        }

        /// <summary>
        /// 获取随机颜色
        /// </summary>
        /// <returns></returns>
        private Color GetRandomColor()
        {
            Random random = new Random();
            byte[] rgbs = new byte[3];
            random.NextBytes(rgbs);
            Color cl = Color.FromArgb(rgbs[0], rgbs[1], rgbs[2]);
            return cl;
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

        private void btn_Start_Click(object sender, EventArgs e)
        {
            EquipMentControl.WaveRecoderCtrl.WaveRecoder_Start(lstChargerID);
        }

        private void btn_Stop_Click(object sender, EventArgs e)
        {
            EquipMentControl.WaveRecoderCtrl.WaveRecoder_Stop(lstChargerID);
        }
    }
}
