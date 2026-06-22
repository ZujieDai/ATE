using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.DataBaseModel;
using SaiTer.ATE.IDAL.SQLiteIDAL;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaiTer.ATE.UI.UserControls.EquipOperate
{
    public partial class UcLoopFeedbackLoad : UcEquipOperateBase
    {
        System.Timers.Timer Timer1 = new System.Timers.Timer();
        bool isAuto = true;
        List<bool> lstIsRelayClose = new List<bool>(); // 假设有一个布尔值来记录继电器的状态
        EquipmentConfigModel EquipmentConfig = new EquipmentConfigModel();
        public UcLoopFeedbackLoad()
        {
            InitializeComponent();
            for (int i = 0; i < 16; i++)
            {
                lstIsRelayClose.Add(false);
            }
            SetChannelState();
        }
        private void UcLoopFeedbackLoad_Load(object sender, EventArgs e)
        {
            GetChargerID();

            Timer1.Interval = 100;
            Timer1.Elapsed += Timer1_Elapsed;
            Timer1.Start();

            this.Disposed += UcLoopFeedbackLoad_Disposed;
        }

        private void UcLoopFeedbackLoad_Disposed(object sender, EventArgs e)
        {
            Timer1.Stop();
            Timer1.Dispose();
            Timer1 = null;
        }



        private void SetChannelState()
        {
            try
            {
                EquipmentConfig = EquipmentConfigManage.GetEquipConfigs()?.Find(s => s.EquipmentName.Equals("LoopFeedbackLoad"));
                string[] strGroups = EquipmentConfig.Params1.Split('|');
                #region ---------------------设置通道功率容量和是否启用--------------

                foreach (string item in strGroups)
                {
                    int group = int.Parse(item.Split('-')[0].TrimStart('G'));
                    string power = item.Split('-')[1];
                    foreach (var temp in this.Controls)
                    {
                        int moduleIndex = -1;
                        if (temp.GetType() == typeof(UILabel))
                        {
                            UILabel lbl = temp as UILabel;
                            if (lbl.Name.Contains("Module"))
                            {
                                moduleIndex = Convert.ToInt32(lbl.Name.Split('_')[1]);
                            }
                            if (moduleIndex == group)
                            {
                                lbl.BackColor = Color.FromArgb(0, 92, 140);
                                lbl.Text = item;
                                lbl.Tag = true;
                            }
                        }
                    }
                }
                string[] strChannels = EquipmentConfig.Params2.Split('|');
                foreach (var item in this.Controls)
                {
                    if (item.GetType() == typeof(UILabel))
                    {
                        UILabel lbl = item as UILabel;
                        if (!lbl.Name.Contains("Module"))
                        {
                            break;
                        }
                        if (lbl.Tag == null || !(bool)lbl.Tag)
                        {
                            int moduleIndex = Convert.ToInt32(lbl.Name.Split('_')[1]);
                            foreach (var tmp in this.Controls)
                            {
                                PictureBox pic = tmp as PictureBox;
                                if (pic != null && pic.Name.Contains("Relay"))
                                {
                                    int relayIndex = Convert.ToInt32(pic.Name.Split("_")[1]);
                                    if (relayIndex == moduleIndex)
                                    {
                                        pic.Enabled = false;
                                        if (relayIndex <= 7)
                                        {
                                            pic.Image = Properties.Resources.RelayClose1_7;
                                        }
                                        else if (relayIndex == 8 || relayIndex == 16)
                                        {
                                            pic.Image = Properties.Resources.RelayClose8;
                                        }
                                        else
                                        {
                                            pic.Image = Properties.Resources.RelayClose9_15;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    else if (item.GetType() == typeof(UIPanel))
                    {
                        UIPanel pnl = item as UIPanel;
                        foreach (var temp in strChannels)
                        {
                            string strCH = temp.Split('=')[0].Substring(2);//CH1=1

                            if (strCH.Equals(pnl.Name.Split('_')[1]))
                            {
                                bool isVisible = temp.Split('=')[1] == "1";
                                pnl.Visible = isVisible;
                            }
                        }
                    }
                }
                #endregion
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }
        private void picRelay_Click(object sender, EventArgs e)
        {
            try
            {
                if (isAuto)
                {
                    PictureBox pic = sender as PictureBox;

                    int index = Convert.ToInt32(pic.Name.Split('_')[1]) - 1;
                    //根据状态设置图片
                    if (lstIsRelayClose[index])
                    {
                        if (index <= 6)
                        {
                            pic.Image = Properties.Resources.RelayOpen1_7;
                        }
                        else if (index == 7 || index == 15)
                        {
                            pic.Image = Properties.Resources.RelayOpen8;
                        }
                        else
                        {
                            pic.Image = Properties.Resources.RelayOpen9_15;
                        }
                        lstIsRelayClose[index] = false; // 改变状态
                        SetParallel(index + 1, false);

                    }
                    else
                    {
                        if (index <= 6)
                        {
                            pic.Image = global::SaiTer.ATE.UI.Properties.Resources.RelayClose1_7;
                        }
                        else if (index == 7 || index == 15)
                        {
                            pic.Image = Properties.Resources.RelayClose8;
                        }
                        else
                        {
                            pic.Image = Properties.Resources.RelayClose9_15;
                        }

                        lstIsRelayClose[index] = true; // 改变状态
                        SetParallel(index + 1, true);
                    }
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        private void SetParallel(int channel, bool isParallel)
        {
            try
            {
                if (isParallel)
                {
                    EquipMentControl.LoopFeedbackLoad.LoopFeedbackLoad_Parallel(lstChargerID, channel);
                }
                else
                {
                    EquipMentControl.LoopFeedbackLoad.LoopFeedbackLoad_NoParallel(lstChargerID, channel);
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }
        private void picRelay_MouseEnter(object sender, EventArgs e)
        {
            try
            {
                foreach (var item in this.Controls)
                {
                    if (item.GetType() == typeof(PictureBox))
                    {
                        PictureBox pic = sender as PictureBox;
                        if (pic.Name == ((PictureBox)item).Name)
                        {
                            pic.Cursor = Cursors.Hand;
                        }
                    }
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        private void picRelay_MouseLeave(object sender, EventArgs e)
        {
            try
            {
                foreach (var item in this.Controls)
                {
                    if (item.GetType() == typeof(PictureBox))
                    {
                        PictureBox pic = sender as PictureBox;
                        if (pic.Name == ((PictureBox)item).Name)
                        {
                            pic.BorderStyle = BorderStyle.None;
                            pic.Cursor = Cursors.Default;
                        }
                    }
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            try
            {
                UIButton btn = sender as UIButton;
                int channel = Convert.ToInt32(btn.Name.Split('_')[1]);
                double volt = FindAndGetValue("txtV_" + channel);
                double current = FindAndGetValue("txtA_" + channel);
                EquipMentControl.LoopFeedbackLoad.SetLoopFeedbackLoadParams(lstChargerID, channel, volt, current);
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }


        private double FindAndGetValue(string controlName)
        {
            try
            {
                // 在Controls集合中查找指定名字的控件
                Control[] foundControls = this.Controls.Find(controlName, true);

                // 检查是否找到了匹配的控件
                if (foundControls.Length > 0 && foundControls[0] is UITextBox textBox)
                {
                    // 找到了 TextBox 控件，可以获取其文本内容
                    string textContent = textBox.Text;
                    return Convert.ToDouble(textContent);
                }
                else
                {
                    // 没有找到匹配的控件
                    return 0;
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); return 0; }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                UIButton btn = sender as UIButton;
                int channel = Convert.ToInt32(btn.Name.Split('_')[1]);
                if (btn.Text.Equals("启动"))
                {
                    EquipMentControl.LoopFeedbackLoad.LoopFeedbackLoad_ON(lstChargerID, channel);
                    btn.FillColor = Color.Red;
                    btn.Text = "停止";
                }
                else
                {
                    EquipMentControl.LoopFeedbackLoad.LoopFeedbackLoad_OFF(lstChargerID, channel);
                    btn.FillColor = Color.FromArgb(80, 160, 255);
                    btn.Text = "启动";
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        private void Timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

            if (Timer1 == null)
            {
                return;
            }
            try
            {
                UITextBox[] textBoxes_C = new UITextBox[16] { txtA_1, txtA_2, txtA_3, txtA_4, txtA_5, txtA_6,
                txtA_7,txtA_8,txtA_9,txtA_10,txtA_11,txtA_12,txtA_13,txtA_14,txtA_15,txtA_16};

                UITextBox[] textBoxes_V = new UITextBox[16] { txtV_1, txtV_2, txtV_3, txtV_4, txtV_5, txtV_6,
                txtV_7,txtV_8,txtV_9,txtV_10,txtV_11,txtV_12,txtV_13,txtV_14,txtV_15,txtV_16};

                UIButton[] buttons = new UIButton[16] { btnStart_1, btnStart_2, btnStart_3, btnStart_4, btnStart_5, btnStart_6,
                  btnStart_7,btnStart_8,btnStart_9,btnStart_10,btnStart_11,btnStart_12,btnStart_13,btnStart_14,btnStart_15,btnStart_16};

                PictureBox[] pictureBoxes = {  picRelay_1, picRelay_2, picRelay_3, picRelay_4, picRelay_5, picRelay_6,
                  picRelay_7,picRelay_8,picRelay_9,picRelay_10,picRelay_11,picRelay_12,picRelay_13,picRelay_14,picRelay_15,picRelay_16};


                for (int i = 1; i <= 16; i++)
                {
                    if (Timer1 == null)
                    {
                        return;
                    }
                    PropertyInfo propertyInfo = AllEquipStateData.DicLoopFeedbackLoad_StateData[1].GetType().GetProperty("Current_" + i);
                    object value = propertyInfo.GetValue(AllEquipStateData.DicLoopFeedbackLoad_StateData[1], null);
                    //// 将值分配给对应的文本框控件               
                    //textBoxes_C[i - 1].Invoke((MethodInvoker)delegate
                    //{
                    //    textBoxes_C[i - 1].Text = value.ToString();
                    //});

                    //propertyInfo = AllEquipStateData.DicLoopFeedbackLoad_StateData[1].GetType().GetProperty("Voltage_" + i);
                    //value = propertyInfo.GetValue(AllEquipStateData.DicLoopFeedbackLoad_StateData[1], null);

                    //textBoxes_V[i - 1].Invoke((MethodInvoker)delegate
                    //{
                    //    textBoxes_V[i - +1].Text = value.ToString();
                    //});


                    //propertyInfo = AllEquipStateData.DicLoopFeedbackLoad_StateData[1].GetType().GetProperty("RunState_" + i);
                    //value = propertyInfo.GetValue(AllEquipStateData.DicLoopFeedbackLoad_StateData[1], null);



                    //if (value != null && value.ToString().Equals("启动"))
                    //{

                    //    buttons[i - 1].Invoke((MethodInvoker)delegate
                    //    {
                    //        buttons[i - 1].FillColor = Color.Red;
                    //        buttons[i - 1].Text = "停止";
                    //    });
                    //}
                    //else
                    //{
                    //    buttons[i - 1].Invoke((MethodInvoker)delegate
                    //    {
                    //        buttons[i - 1].FillColor = Color.FromArgb(80, 160, 255);
                    //        buttons[i - 1].Text = "启动";
                    //    });
                    //}


                    propertyInfo = AllEquipStateData.DicLoopFeedbackLoad_StateData[1].GetType().GetProperty("Parallet_" + i);
                    value = propertyInfo.GetValue(AllEquipStateData.DicLoopFeedbackLoad_StateData[1], null);
                    if (value == null)
                    {
                        return;
                    }
                    isAuto = false;



                    if (pictureBoxes[i - 1].Enabled == false)
                    {
                        isAuto = true;
                        continue;
                    }
                    //根据状态设置图片
                    if (Convert.ToInt32(value) == 0)
                    {
                        if (i <= 7)
                        {
                            pictureBoxes[i - 1].Image = Properties.Resources.RelayOpen1_7;
                        }
                        else if (i == 8 || i == 16)
                        {
                            pictureBoxes[i - 1].Image = Properties.Resources.RelayOpen8;
                        }
                        else
                        {
                            pictureBoxes[i - 1].Image = Properties.Resources.RelayOpen9_15;
                        }
                        lstIsRelayClose[i - 1] = false; // 改变状态


                    }
                    else
                    {
                        if (i <= 7)
                        {
                            pictureBoxes[i - 1].Image = global::SaiTer.ATE.UI.Properties.Resources.RelayClose1_7;
                        }
                        else if (i == 8 || i == 116)
                        {
                            pictureBoxes[i - 1].Image = Properties.Resources.RelayClose8;
                        }
                        else
                        {
                            pictureBoxes[i - 1].Image = Properties.Resources.RelayClose9_15;
                        }

                        lstIsRelayClose[i - 1] = true; // 改变状态

                    }
                    isAuto = true;
                }

            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }
    }
}
