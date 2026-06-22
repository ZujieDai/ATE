
using SaiTer.ATE.DataModel;
using SaiTer.ATE.EquipMent;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaiTer.ATE.UI.UserControls.EquipOperate
{
    public partial class UcElectronicLoad : UcEquipOperateBase
    {
        private string[] OneTextBlockS = new string[] { "定电流(mA)", "定电压(mV)", "定功率(mW)", "定电阻(mΩ)" };
        private byte[] OperateS = new byte[] { 0x00, 0x01, 0x02, 0x03 }; //负载模式（0 为CC, 1 为输出CV, 2 为CW, 3 为CR）
        private byte[] ComS = new byte[] { 0x2A, 0x2C, 0x2E, 0x30 };// 设置或读取负载的定电流值（2AH/2BH ）
                                                                    // 设置或读取负载的定电压值（2CH/2DH ） 设置或读取负载的定功率值（2EH/2FH ）  设置或读取负载的定电阻值（30H/31H ）
        private byte[] MultipleS = new byte[] { 10, 1, 1, 1 };//倍数 定电流值10  定电压值1   定功率值1 定电阻值1

        public UcElectronicLoad()
        {
            InitializeComponent();
        }
        private void UcElectronicLoad_Load(object sender, EventArgs e)
        {
            GetChargerID();

            var Customer = ConfigurationManager.AppSettings["Customer"];
            if (Customer != null && Customer.Equals("YKR"))
            {
                cmbFunction.Items.RemoveAt(1);
                OneTextBlockS = new string[] { "定电流(mA)", "定功率(mW)", "定电阻(mΩ)" };
                OperateS = new byte[] { 0x00, 0x02, 0x03 };
                ComS = new byte[] { 0x2A, 0x2E, 0x30 };
                MultipleS = new byte[] { 10, 1, 1 };
            }

            EquipMentControl.ElectronicLoad.SetElectronicLoadParams(lstChargerID, 0x20, 0x01);  //远程控制模式
        }
        private void btnSet_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < OneTextBlockS.Length; i++)
            {
                if (cmbFunction.Text == OneTextBlockS[i])
                {
                    EquipMentControl.ElectronicLoad.SetElectronicLoadParams(lstChargerID, ComS[i], Convert.ToUInt32(txtValue.Text) * MultipleS[i]);
                }
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            EquipMentControl.ElectronicLoad.ElectronicLoad_ON(lstChargerID);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            EquipMentControl.ElectronicLoad.ElectronicLoad_OFF(lstChargerID);
        }

        private void cmbFunction_SelectedIndexChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < OneTextBlockS.Length; i++)
            {
                if (cmbFunction.Text == OneTextBlockS[i])
                {
                    EquipMentControl.ElectronicLoad.SetElectronicLoadParams(lstChargerID, 0x28, OperateS[i]);
                }
            }
        }
    }
}
