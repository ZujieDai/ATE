using Saiter;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaiTer.ATE.UI.Assitand.PrjUI
{


    public partial class FrmRegister : UIForm
    {
        private string _Warning;
        public FrmRegister(string warning)
        {
            InitializeComponent();
            _Warning = warning;
        }

        private void FrmLogin_Load(object sender, EventArgs e)
        {
#if ST_9980AP_DC
            lblLoginTitle.Text = KeyConst.WinLabel.ST9980AP;
            //lblLoginTitle.Text = KeyConst.WinLabel.ST910DC;
            //lblLoginTitle.Text = KeyConst.WinLabel.AST9000DC;
#elif ST_9980A_DC
            lblLoginTitle.Text = KeyConst.WinLabel.ST9980A;
#elif ST_9980BP
            lblLoginTitle.Text = KeyConst.WinLabel.ST9980BP;
            //lblLoginTitle.Text = KeyConst.WinLabel.ST910AC;
            //lblLoginTitle.Text = KeyConst.WinLabel.AST9000AC;
#elif ST_9980AP_AC
            lblLoginTitle.Text = KeyConst.WinLabel.ST9980AP_AC;
#elif ST_990_DC
            lblLoginTitle.Text = KeyConst.WinLabel.ST990;
#elif ST_990_DCAL
            lblLoginTitle.Text = KeyConst.WinLabel.ST990AL;
#elif ST_9980UA_AC
            lblLoginTitle.Text = KeyConst.WinLabel.ST9980UAAC;
#elif ST_9980EA_AC
            lblLoginTitle.Text = KeyConst.WinLabel.ST9980EAAC;
#elif ST_9980CA_DC
            lblLoginTitle.Text = KeyConst.WinLabel.ST9980CADC;
#endif

            tbMachineCode.Text = Encrypt.DeviceHelper.GetMachineCode();
            if (_Warning != "" && _Warning != "软件已注册！")
            {
                MessageBox.Show(_Warning);
            }
            else if (_Warning == "软件已注册！")
            {
                BtnResgister_Click(null, null);
                return;
            }
            lblRegState.Visible = true;
            lblRegState.Text = _Warning;
            SetCustomerLogo();
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

        private void BtnResgister_Click(object sender, EventArgs e)
        {
            try
            {
                //string machineCode = GetMachineCode();
                //lblRegState.Text = GetRegisterDate(machineCode);
                //ClsDongleATEFrame clsDongleATEFrame = new ClsDongleATEFrame();
                //string strRetReg = string.Empty;
                //bool bRet = clsDongleATEFrame.RegisterDongleReg(txtRegCode.Text, ref strRetReg);
                ////lblRegState.Text = GetRegisterDate(machineCode);
                UtilLicense m_objLicense = new UtilLicense(DeviceType.ST_ATEFrame);
                bool bRet = m_objLicense.CheckLicense(true, false); // 默认中文，默认启动校验
                if (bRet)
                {
                   
                    this.Hide();
                    FrmLogin frm = new FrmLogin();
                    frm.Show();
                }
                
            }
            catch
            {
                lblRegState.Text = "出现错误,请联系管理员！";
            }
            lblRegState.Visible = true;
        }
        private string GetMachineCode()
        {
            string serialNo = Encrypt.TimeClass.ReadSetting("", "SerialNumber", "-1");
            string machineCode = tbMachineCode.Text;
            return machineCode;
        }
        private string GetRegisterDate(string machineCode)
        {
            string text = "";
            if (txtRegCode.Text.Length < 64)
            {
                text = "注册码与本机不一致,请联系管理员！";
                return text;
            }
            try
            {
                string regCode = txtRegCode.Text.Substring(0, 64);
                Encrypt.Encryption encryption = new Encrypt.Encryption();
                string stdRegCode = Encrypt.Encryption.Encrypt(machineCode, Encrypt.Encryption.CRYPTO_KEY);

                if (regCode == stdRegCode)
                {
                    string time = txtRegCode.Text.Substring(64);

                    text = "该软件已经成功注册。" + System.Environment.NewLine;

                    string decryptTime = Encrypt.Encryption.Decrypt(time, Encrypt.Encryption.CRYPTO_KEY);
                    if (decryptTime == "99991231")
                    {
                        text += "取得永久使用权限。" + System.Environment.NewLine;
                    }
                    else
                    {
                        decryptTime = decryptTime.Substring(0, 4) + "/" + decryptTime.Substring(4, 2) + "/" + decryptTime.Substring(6, 2);
                        text += "软件试用期到" + decryptTime + System.Environment.NewLine;
                    }
                    //写入注册表
                    Encrypt.TimeClass.WriteSetting("", "SerialNumber", regCode + time);
                }
                else
                    text = "注册码与本机不一致,请联系管理员！";
            }
            catch (Exception ex)
            {
                Log.Log.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", ex);
            }
            return text;
        }
    }
}
