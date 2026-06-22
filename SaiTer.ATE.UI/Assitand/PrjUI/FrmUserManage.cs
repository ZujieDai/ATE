using SaiTer.ATE.DataModel;
using SaiTer.ATE.IDAL.SQLiteIDAL;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaiTer.ATE.UI.Assitand.PrjUI
{
    public partial class FrmUserManage : UIForm
    {
        private static FrmUserManage Instance = null;

        public static FrmUserManage GetInstance()
        {
            if (Instance == null || Instance.IsDisposed)
            {
                Instance = new FrmUserManage();
            }
            Instance.Activate();
            return Instance;
        }

        private FrmUserManage()
        {
            InitializeComponent();
        }

        private void FrmUserManage_Load(object sender, EventArgs e)
        {
            try
            {
                dgvUserInfo.Rows.Clear();
                List<UserInfoModel> lstUserInfos = UserInfoManage.GetUserInfoModels();
                for (int i = 0; i < lstUserInfos.Count; i++)
                {
                    dgvUserInfo.Rows.Add(1);
                    dgvUserInfo.Rows[i].Cells["UserName"].Value = lstUserInfos[i].UserName;
                    dgvUserInfo.Rows[i].Cells["Pwd"].Value = lstUserInfos[i].Password;
                    dgvUserInfo.Rows[i].Cells["Level"].Value = lstUserInfos[i].UserType;
                }
                SetCustomerLogo();
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
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

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                List<UserInfoModel> lstUserInfos = new List<UserInfoModel>();
                List<string> lstUserName = new List<string>();
                for (int i = 0; i < dgvUserInfo.Rows.Count - 1; i++)
                {
                    UserInfoModel model = new UserInfoModel();
                    if (dgvUserInfo.Rows[i].Cells["UserName"].Value == null || string.IsNullOrEmpty(dgvUserInfo.Rows[i].Cells["UserName"].Value.ToString()))
                    {
                        ShowErrorTip("用户名不能为空");
                        return;
                    }
                    if (dgvUserInfo.Rows[i].Cells["Pwd"].Value == null || string.IsNullOrEmpty(dgvUserInfo.Rows[i].Cells["Pwd"].Value.ToString()))
                    {
                        ShowErrorTip("密码不能为空");
                        return;
                    }
                    if (dgvUserInfo.Rows[i].Cells["Level"].Value == null || string.IsNullOrEmpty(dgvUserInfo.Rows[i].Cells["Level"].Value.ToString()))
                    {
                        ShowErrorTip("请选择用户权限");
                        return;
                    }
                    if (lstUserName.Contains(dgvUserInfo.Rows[i].Cells["UserName"].Value.ToString()))
                    {
                        ShowErrorTip("用户名不能重复！");
                        return;
                    }
                    lstUserName.Add(dgvUserInfo.Rows[i].Cells["UserName"].Value.ToString());
                    model.UserName = dgvUserInfo.Rows[i].Cells["UserName"].Value.ToString();
                    model.Password = dgvUserInfo.Rows[i].Cells["Pwd"].Value.ToString();
                    model.UserType = dgvUserInfo.Rows[i].Cells["Level"].Value.ToString();
                    model.Level = dgvUserInfo.Rows[i].Cells["Level"].Value.ToString() == "管理员" ? 1 : 2;
                    lstUserInfos.Add(model);
                }
                bool resut = UserInfoManage.InsertUserInfo(lstUserInfos);
                if (resut)
                {
                    UIMessageTip.ShowOk("保存成功");
                }
                else
                {
                    ShowErrorTip("保存失败！！！");
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }


        private void btnDelete_Click(object sender, EventArgs e)
        {
            int index = dgvUserInfo.SelectedIndex;
            if (index == -1 || index == dgvUserInfo.Rows.Count - 1)
            {
                return;
            }
            dgvUserInfo.Rows.RemoveAt(index);
        }
    }
}
