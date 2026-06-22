using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.Manage;
using Sunny.UI.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaiTer.ATE.UI.UserControls.EquipOperate
{
    public partial class UcACSource : UcEquipOperateBase
    {
        private BusinessManage BCM = BusinessManage.GetInstance();
        /// <summary>
        /// 交流源
        /// </summary>
        public UcACSource()
        {
            InitializeComponent();
        }
        private void UcACSource_Load(object sender, EventArgs e)
        {
            GetChargerID();
            try
            {
                if (BCM.lstChargerInfo.Count == 0)
                {
                    txtVoltage.Text = "130";
                    lblVoltage.Text = (Convert.ToDouble(txtVoltage.Text) * 1.732).ToString();
                    //EquipMentControl.ACSource.ACSource_SetVolt(lstChargerID, 130);
                }
                else
                {
                    //交流项目设置成桩的额定电压， 直流设置成220V
                    double voltage = BCM.lstChargerInfo[0].NominalVoltage;
                    if (BCM.lstChargerInfo[0].NominalVoltage >= 220)
                    {
                        voltage = 220;
                    }
                    txtVoltage.Text = voltage.ToString();
                    lblVoltage.Text = (Convert.ToDouble(txtVoltage.Text) * 1.7320508075).ToString("F1");
                    //EquipMentControl.ACSource.ACSource_SetVolt(lstChargerID, Convert.ToDouble(voltage));
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }
        private void btnSetVoltage_Click(object sender, EventArgs e)
        {
            EquipMentControl.ACSource.ACSource_SetVolt(lstChargerID, Convert.ToDouble(txtVoltage.Text));
        }

        private void bntStart_Click(object sender, EventArgs e)
        {
            string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
            if (Customer.Equals("LJ") && EquipMentControl.ControlBoard?.DitEquipMentBase.Where(em => em.Value.EquipMentClassName.Equals("emtDIORelay")).ToArray().Length > 0)
            {
                //深圳LJ单开K5：ST-PTAC-GB导引模块带载接触器闭合(K5与中盛的DIO模块的输出Y3或者Y4互锁
                //SystemEvent.SendCountDownTimer("请确认当前负载已经关闭，带载控制程控板有损坏设备的风险", 999, 0);
                EquipMentControl.ResistanceLoad.ResistanceLoad_OFF(lstChargerID);
                EquipMentControl.BMS.BMS_OFF(lstChargerID);
                Thread.Sleep(200);
                var list = EquipMentControl.ControlBoard.ControlBoardReadState(lstChargerID);
                list[4] = false;
                EquipMentControl.ControlBoard.ControlResistanceSetRelay(list);
                Thread.Sleep(500);

                //深圳LJ待机功耗测试需要操作DIO继电器
                //1.Y1控制KM3、KM6接触器闭合(待机功耗测试)，X1是KM3、KM6闭合反馈
                //2.Y2控制KM1~2、KM4~5接触器闭合(除待机功耗测试外),X2是KM1~2、KM4~5闭合反馈
                //if (lstTrialScheme[i].TrialType == EmTrialType.待机功耗测试)
                //{
                //    EquipMentControl.ControlBoard.SetRelaySwitch(1, false);
                //    EquipMentControl.ControlBoard.SetRelaySwitch(0, true);
                //}
                //else
                //{
                EquipMentControl.ControlBoard.SetRelaySwitch(0, false);
                Thread.Sleep(300);
                EquipMentControl.ControlBoard.SetRelaySwitch(1, true);
                //}
            }

            EquipMentControl.ACSource.ACSource_ON(lstChargerID);
        }

        private void bntClose_Click(object sender, EventArgs e)
        {
            EquipMentControl.ACSource.ACSource_OFF(lstChargerID);
        }

        private void txtVoltage_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtVoltage.Text))
                {
                    lblVoltage.Text = "0";
                }
                else
                {
                    lblVoltage.Text = (Convert.ToDouble(txtVoltage.Text) * 1.732).ToString();
                }
            }
            catch { }
        }

        private void btnSetFreq_Click(object sender, EventArgs e)
        {
            try
            {
                EquipMentControl.ACSource.ACSource_SetFreq(lstChargerID, Convert.ToDouble(txtFreq.Text));
            }
            catch { }
        }
    }
}
