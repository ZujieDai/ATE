using SaiTer.ATE.DataModel.DataBaseModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.Struct;
using SaiTer.ATE.IDAL.SQLiteIDAL;
using SaiTer.ATE.InterFace;
using SaiTer.ATE.Manage;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace SaiTer.ATE.UI.Assitand.PrjUI
{
    public partial class FrmSchemeEdit : UIForm
    {
        private bool isAutoTest;
        private BusinessManage BCM;
        private static FrmSchemeEdit Instance = null;

        public static FrmSchemeEdit GetInstance()
        {
            if (Instance == null || Instance.IsDisposed)
            {
                Instance = new FrmSchemeEdit();
            }
            Instance.Activate();
            return Instance;
        }
        /// <summary>
        /// 左侧表格的试验项列表
        /// </summary>
        private List<StTrialItem> lstTrialItemsInfoAll = new List<StTrialItem>();

        /// <summary>
        /// 右侧表格的试验项列表
        /// </summary>
        private List<StTrialItem> lstItemsSelect = new List<StTrialItem>();


        /// <summary>
        /// 已有方案名称列表
        /// </summary>
        private List<StSchemeInfo> lstHadSchemeInfo = new List<StSchemeInfo>();

        private FrmSchemeEdit()
        {
            InitializeComponent();
            BCM = BusinessManage.GetInstance();
        }

        private void FrmSchemeEdit_Load(object sender, EventArgs e)
        {
            try
            {
                //自动化产线属性
                string strAutoTest = ConfigurationManager.AppSettings["isAutoTest"];
                if (strAutoTest != null)
                {
                    isAutoTest = bool.Parse(strAutoTest);
                }
                if (isAutoTest)
                {
                    var ChargerType = BCM._xmlInfoAssembly._systemXmlInfo.ChargerType;
                    cmbType.Items.Clear();
                    foreach (var item in ChargerType)
                    {
                        cmbType.Items.Add(item);
                    }

                    lblVolt.Visible = true;
                    lblCurrent.Visible = true;
                    lblType.Visible = true;
                    txtMaxVoltage.Visible = true;
                    txtRateCurrent.Visible = true;
                    cmbType.Visible = true;
                }

                List<StSchemeInfo> lstSchemeInfo = SchemeInfoManage.GetStandardScheme();
                if (lstSchemeInfo == null || lstSchemeInfo.Count == 0) { return; }
                cmbStandard.Items.Clear();
                foreach (var item in lstSchemeInfo)
                {
                    cmbStandard.Items.Add(item.SchemeName);
                }
                cmbStandard.SelectedIndex = 0;

                lstHadSchemeInfo.Clear();
                SchemeInfoManage.GetSchemeInfo(ref lstHadSchemeInfo);
                cmbScheme.Items.Clear();
                foreach (var item in lstHadSchemeInfo)
                {
                    cmbScheme.Items.Add(item.SchemeName);
                }

                BCM = BusinessManage.GetInstance();

                SetCustomerLogo();
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        private void SetCustomerLogo()
        {
            string Customer = ConfigurationManager.AppSettings["Customer"];
            if (!string.IsNullOrEmpty(Customer) && Customer.ToString().Trim().Equals("TPK"))
            {
                //pictureBox1.Size = new Size(400, 60);
                //pictureBox1.Dock = DockStyle.Fill;
                this.Icon = Properties.Resources.TPK_Icon;
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (lbAll.SelectedItem == null)
                {
                    return;
                }
                string trialName = lbAll.SelectedItem.ToString();
                if (lstItemsSelect.FindIndex(s => s.ItemName == trialName) >= 0)
                {
                    ShowErrorTip("已添加过该项！！！");
                    return;
                }

                StTrialItem stTrialItem = lstTrialItemsInfoAll.Find(s => s.ItemName == trialName);
                if (stTrialItem == null)
                {
                    return;
                }
                lstItemsSelect.Add(stTrialItem);
                lbSelect.Items.Add(trialName);
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        private void btnDel_Click(object sender, EventArgs e)
        {
            try
            {
                if (lbSelect.SelectedItem == null)
                {
                    return;
                }
                string trialName = lbSelect.SelectedItem.ToString();
                StTrialItem stTrialItem = lstItemsSelect.Find(s => s.ItemName == trialName);
                if (stTrialItem == null)
                {
                    return;
                }
                lstItemsSelect.Remove(stTrialItem);
                lbSelect.Items.Remove(trialName);
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        private void btnAll_Click(object sender, EventArgs e)
        {
            try
            {
                lbSelect.Items.Clear();
                lstItemsSelect.Clear();
                foreach (var item in lstTrialItemsInfoAll)
                {
                    lbSelect.Items.Add(item.ItemName);
                    lstItemsSelect.Add(item);
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            try
            {
                lbSelect.Items.Clear();
                lstItemsSelect.Clear();
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            try
            {
                this.lbSelect.MoveSelectedItems(true, () =>
                {

                });
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            try
            {
                this.lbSelect.MoveSelectedItems(false, () =>
                {

                });
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (lstItemsSelect.Count == 0)
                {
                    ShowErrorTip("未选择测试项！！");
                    return;
                }
                if (string.IsNullOrEmpty(txtName.Text))
                {
                    ShowErrorTip("方案名不能为空！！");
                    return;
                }
                if (txtName.Text.Contains("全部测试项"))
                {
                    ShowErrorTip("方案名不能包含关键字【全部测试项】！！");
                    return;
                }
                if (lstHadSchemeInfo.FindIndex(s => s.SchemeName == txtName.Text.Trim()) >= 0)
                {
                    ShowErrorTip("已有此方案名，方案名称不能重复！！");
                    return;
                }
                if (isAutoTest)
                {
                    if (string.IsNullOrEmpty(txtMaxVoltage.Text))
                    {
                        ShowErrorTip("额定电压不能为空！！");
                        return;
                    }
                    if (string.IsNullOrEmpty(txtRateCurrent.Text))
                    {
                        ShowErrorTip("额定电流不能为空！！");
                        return;
                    }
                    if (cmbType.SelectedIndex < 0)
                    {
                        ShowErrorTip("产品类型未选择！！");
                        return;
                    }
                }
                btnSave.Enabled = false;

                StSchemeInfo info = new StSchemeInfo();
                Random rand = new Random();
                info.SchemeID = rand.Next(100, 9999);
                info.CreatTime = DateTime.Now.ToString();
                info.SchemeName = txtName.Text.Trim();
                if (isAutoTest)
                {
                    info.RES1 = txtMaxVoltage.Text.Trim();
                    info.RES2 = txtRateCurrent.Text.Trim();
                    info.RES3 = cmbType.SelectedText;
                }
                if (!SchemeInfoManage.InsertScheme(info))
                {
                    btnSave.Enabled = true;
                    ShowErrorTip("出错了，保存失败");
                    return;
                }
                lstHadSchemeInfo.Add(info);
                lstItemsSelect.ForEach(item => { item.SchemeID = info.SchemeID; item.SchemeName = info.SchemeName; });
                Thread.Sleep(100);
                foreach (var item in lstItemsSelect)
                {
                    for (int i = 0; i < lbSelect.Items.Count; i++)
                    {
                        if (item.ItemName.Equals(lbSelect.Items[i]))
                        {
                            item.TrialOrder = i + 1;
                        }
                    }
                }
                if (!TrialItemsManage.InsertTrialItems(lstItemsSelect))
                {
                    ShowErrorTip("出错了，保存失败");
                    btnSave.Enabled = true;
                    SchemeInfoManage.DeleteScheme(txtName.Text.Trim());
                    return;
                }

                // 如果数据库存在该数据则认为有SE7441设备，新增测试方案也要添加配置
                // （设备调用前会初始化，读取配置新建安规的档案，所以此处不操作设备）
                if (BCM._xmlInfoAssembly._EquipMentControl.Safety != null && 
                    BCM._xmlInfoAssembly._EquipMentControl.Safety.DitEquipMentBase.Values.FirstOrDefault(equip => equip.GetType().Name.Equals("emtSafety_SE7441")) != null)
                {
                    SystemEvent.MessageInfo(true, "安规仪基本设置配置中，请稍等");
                    Application.DoEvents();
                    //新建方案对应安规仪的方案编号
                    string safetyOrder = "1", safetyName = "ST_001";
                    var lstEquipmentConfig = EquipmentConfigManage.GetEquipConfigs()?.FindAll(s => s.EquipmentName.Equals("Safety_SE7441") && s.ConfigType.Equals("Safety_Scheme"));
                    if (lstEquipmentConfig != null && lstEquipmentConfig.Count > 0)
                    {
                        safetyOrder = (lstEquipmentConfig.Count + 1).ToString();
                        safetyName = "ST_" + (lstEquipmentConfig.Count + 1).ToString().PadLeft(3, '0');
                        //先删除再增加
                        EquipmentConfigModel equipmentConfigManage = new EquipmentConfigModel()
                        {
                            ChargerType = info.SchemeID,
                            ConfigType = "Safety_Scheme",
                            EquipmentName = "Safety_SE7441",
                            Params1 = safetyOrder,
                            Params2 = safetyName,
                        };
                        EquipmentConfigManage.InsertEquipConfigs(equipmentConfigManage);
                    }
                    else
                    {
                        //一个方案都没
                        EquipmentConfigModel equipmentConfigManage = new EquipmentConfigModel()
                        {
                            ChargerType = info.SchemeID,
                            ConfigType = "Safety_Scheme",
                            EquipmentName = "Safety_SE7441",
                            Params1 = "1",
                            Params2 = "ST_" + (1).ToString().PadLeft(3, '0'),
                        };
                        EquipmentConfigManage.InsertEquipConfigs(equipmentConfigManage);
                    }
                    //安规仪的数据库配置增加一个默认值
                    EquipmentConfigModel safety_Params = new EquipmentConfigModel()
                    {
                        ChargerType = info.SchemeID,
                        ConfigType = "Safety_Params",
                        EquipmentName = "Safety_SE7441",
                        Params1 = "IR电压值(V)=500|HISET电阻值(MΩ)=0|LOSET电阻值(MΩ)=10|测试时间(S)=5|缓升时间(S)=3|缓降时间(S)=0|延迟时间(S)=0.5;IR电压值(V)=500|HISET电阻值(MΩ)=0|LOSET电阻值(MΩ)=10|测试时间(S)=5|缓升时间(S)=3|缓降时间(S)=0|延迟时间(S)=0;IR电压值(V)=500|HISET电阻值(MΩ)=9999|LOSET电阻值(MΩ)=1|测试时间(S)=5|缓升时间(S)=3|缓降时间(S)=0|延迟时间(S)=0.5",
                        Params2 = "交流耐压值(V)=2200|HISET电流值(mA)=10|LOSET电流值(mA)=0|测试时间(S)=5|缓升时间(S)=3|缓降时间(S)=0.5|ARC电弧灵敏度=2|输出频率(Hz)=0;交流耐压值(V)=2200|HISET电流值(mA)=10|LOSET电流值(mA)=0|测试时间(S)=5|缓升时间(S)=3|缓降时间(S)=0.5|ARC电弧灵敏度=2|输出频率(Hz)=0;交流耐压值(V)=2200|HISET电流值(mA)=10|LOSET电流值(mA)=0|测试时间(S)=5|缓升时间(S)=3|缓降时间(S)=0.5|ARC电弧灵敏度=2|输出频率(Hz)=0",
                        Params3 = "直流耐压值(V)=2800|HISET电流值(uA)=10|LOSET电流值(uA)=0|测试时间(S)=60|缓升时间(S)=0.5|缓降时间(S)=1|ARC电弧灵敏度=1;直流耐压值(V)=2800|HISET电流值(uA)=10|LOSET电流值(uA)=0|测试时间(S)=60|缓升时间(S)=0.5|缓降时间(S)=1|ARC电弧灵敏度=1;直流耐压值(V)=2800|HISET电流值(uA)=10|LOSET电流值(uA)=0|测试时间(S)=60|缓升时间(S)=0.5|缓降时间(S)=1|ARC电弧灵敏度=1",
                        Remark = "接地电压值(V)=3|接地电流值(A)=25|HISET电阻值(MΩ)=150|LOSET电阻值(MΩ)=0|测试时间(S)=60|输出频率(Hz)=0;接地电压值(V)=3|接地电流值(A)=25|HISET电阻值(MΩ)=150|LOSET电阻值(MΩ)=0|测试时间(S)=60|输出频率(Hz)=0;接地电压值(V)=3|接地电流值(A)=25|HISET电阻值(MΩ)=150|LOSET电阻值(MΩ)=0|测试时间(S)=60|输出频率(Hz)=0;接地电压值(V)=3|接地电流值(A)=25|HISET电阻值(MΩ)=150|LOSET电阻值(MΩ)=0|测试时间(S)=60|输出频率(Hz)=0",
                    };
                    EquipmentConfigManage.InsertEquipConfigs(safety_Params);
                    //根据方案编号和方案名保存
                    List<int> lstID = new List<int> { 1 };
                    BCM._xmlInfoAssembly._EquipMentControl.Safety.SafetyInit(lstID, safetyOrder, safetyName, true);
                    SystemEvent.MessageInfo(false, "");
                }

                cmbScheme.Items.Add(txtName.Text.Trim());
                UIMessageTip.ShowOk("保存成功");
                btnSave.Enabled = true;
            }
            catch (Exception ex)
            {
                btnSave.Enabled = true;
                Log.Log.LogException(ex);
                ShowErrorTip("出错了，保存失败");
            }
        }

        private void btnDelScheme_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(cmbScheme.Text))
                {
                    return;
                }
                SchemeInfoManage.DeleteScheme(cmbScheme.Text);
                TrialItemsManage.DeleteTrialItems(cmbScheme.Text);
                cmbScheme.Items.Remove(cmbScheme.Text);
                UIMessageTip.ShowOk("删除成功");
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                ShowErrorTip("出错了，删除失败");
            }
        }

        private void cmbStandard_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                lstTrialItemsInfoAll.Clear();
                TrialItemsManage.GetTrialSchemeFromSchemeName(cmbStandard.Text, ref lstTrialItemsInfoAll);

                if (lstTrialItemsInfoAll.Count == 0)
                {
                    return;
                }
                lbAll.Items.Clear();

                foreach (var trialItem in lstTrialItemsInfoAll)
                {
                    lbAll.Items.Add(trialItem.ItemName);
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        private void lbAll_ItemDoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (lbAll.SelectedItem == null)
                {
                    return;
                }
                string trialName = lbAll.SelectedItem.ToString();
                if (lstItemsSelect.FindIndex(s => s.ItemName == trialName) >= 0)
                {
                    ShowErrorTip("已添加过该项！！！");
                    return;
                }

                StTrialItem stTrialItem = lstTrialItemsInfoAll.Find(s => s.ItemName == trialName);
                if (stTrialItem == null)
                {
                    return;
                }
                lstItemsSelect.Add(stTrialItem);
                lbSelect.Items.Add(trialName);
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        private void lbSelect_ItemDoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (lbSelect.SelectedItem == null)
                {
                    return;
                }
                string trialName = lbSelect.SelectedItem.ToString();
                StTrialItem stTrialItem = lstItemsSelect.Find(s => s.ItemName == trialName);
                if (stTrialItem == null)
                {
                    return;
                }
                lstItemsSelect.Remove(stTrialItem);
                lbSelect.Items.Remove(trialName);
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }
    }



    public static class ListBoxExtension
    {
        public static bool MoveSelectedItems(this UIListBox listBox, bool isUp, Action noSelectAction)
        {
            if (listBox.SelectedItems.Count > 0)
            {
                return listBox.MoveSelectedItems(isUp);
            }
            else
            {
                noSelectAction();
                return false;
            }
        }

        public static bool MoveSelectedItems(this UIListBox listBox, bool isUp)
        {
            bool result = true;
            ListBox.SelectedIndexCollection indices = listBox.SelectedIndices;
            if (isUp)
            {
                if (listBox.SelectedItems.Count > 0 && indices[0] != 0)
                {
                    foreach (int i in indices)
                    {
                        result &= MoveSelectedItem(listBox, i, true);
                    }
                }
            }
            else
            {
                if (listBox.SelectedItems.Count > 0 && indices[indices.Count - 1] != listBox.Items.Count - 1)
                {
                    for (int i = indices.Count - 1; i >= 0; i--)
                    {
                        result &= MoveSelectedItem(listBox, indices[i], false);
                    }
                }
            }
            return result;
        }

        public static bool MoveSelectedItem(this UIListBox listBox, bool isUp, Action noSelectAction)
        {
            if (listBox.SelectedItems.Count > 0)
            {
                return MoveSelectedItem(listBox, listBox.SelectedIndex, isUp);
            }
            else
            {
                noSelectAction();
                return false;
            }
        }

        public static bool MoveSelectedItem(this UIListBox listBox, bool isUp)
        {
            return MoveSelectedItem(listBox, listBox.SelectedIndex, isUp);
        }

        private static bool MoveSelectedItem(this UIListBox listBox, int selectedIndex, bool isUp)
        {
            if (selectedIndex != (isUp ? 0 : listBox.Items.Count - 1))
            {
                object current = listBox.Items[selectedIndex];
                int insertAt = selectedIndex + (isUp ? -1 : 1);

                listBox.Items.RemoveAt(selectedIndex);
                listBox.Items.Insert(insertAt, current);
                listBox.SelectedIndex = insertAt;
                return true;
            }
            return false;
        }
    }
}
