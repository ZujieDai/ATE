
using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.EquipStateData;
using SaiTer.ATE.EquipMent;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaiTer.ATE.UI.UserControls
{
    /// <summary> 
    /// 设备监控数据控件
    /// </summary>
    public partial class ucEquipMonitorData : UserControl
    {
        public AllEquipStateData EquipStateData = null;
        public int ChargerID;//当前控件枪位号
        public string EquipName = "";//当前控件设备名称
        string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();


        public ucEquipMonitorData(object MonitorData)
        {
            InitializeComponent();
            EquipStateData = AllEquipStateData.GetInstance();
            SystemEvent.SendMonitorMessageEvent += SystemEvent_SendMonitorMessageEvent;

            if (this.InvokeRequired)
            {
                this.Invoke(new SetControlValueEventHander(IniControl), MonitorData);
            }
            else
            {
                IniControl(MonitorData);
            }

        }

        private void SystemEvent_SendMonitorMessageEvent(object monitorDada)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new SetControlValueEventHander(RefreshControlValue), monitorDada);
            }
            else
            {
                RefreshControlValue(monitorDada);
            }
        }

        private delegate void SetControlValueEventHander(object Obj);
        /// <summary>
        /// 刷新控件数据
        /// </summary>
        /// <param name="monitorData"></param>
        private void RefreshControlValue(object monitorData)
        {
            try
            {
                StateDataBase data = (StateDataBase)monitorData;
                if (data != null && ChargerID == data.ChargerID && EquipName == data.EquipName)
                {
                    if (monitorData.GetType() == typeof(BMS_DC_StateData))
                    {
                        if (dgvMonitor.InvokeRequired)
                        {
                            Action SetData = delegate { SetBMS_DC_StateData(monitorData); };
                            dgvMonitor.Invoke(SetData);
                        }
                        else
                        {
                            SetBMS_DC_StateData(monitorData);
                        }

                    }
                    else if (monitorData.GetType() == typeof(BMS_EU_DC_StateData))
                    {
                        if (dgvMonitor.InvokeRequired)
                        {
                            Action SetData = delegate { SetBMS_EU_DC_StateData(monitorData); };
                            dgvMonitor.Invoke(SetData);
                        }
                        else
                        {
                            SetBMS_EU_DC_StateData(monitorData);
                        }

                    }
                    else if (monitorData.GetType() == typeof(BMS_USA_DC_StateData))
                    {
                        if (dgvMonitor.InvokeRequired)
                        {
                            Action SetData = delegate { SetBMS_USA_DC_StateData(monitorData); };
                            dgvMonitor.Invoke(SetData);
                        }
                        else
                        {
                            SetBMS_USA_DC_StateData(monitorData);
                        }

                    }
                    else if (monitorData.GetType() == typeof(BMS_JP_DC_StateData))
                    {
                        if (dgvMonitor.InvokeRequired)
                        {
                            Action SetData = delegate { SetBMS_JP_DC_StateData(monitorData); };
                            dgvMonitor.Invoke(SetData);
                        }
                        else
                        {
                            SetBMS_JP_DC_StateData(monitorData);
                        }

                    }
                    else if (monitorData.GetType() == typeof(BMS_AC_StateData))
                    {
                        if (dgvMonitor.InvokeRequired)
                        {
                            Action SetData = delegate { SetBMS_AC_StateData(monitorData); };
                            dgvMonitor.Invoke(SetData);
                        }
                        else
                        {
                            SetBMS_AC_StateData(monitorData);
                        }
                    }
                    //else if (monitorData.GetType() == typeof(SDSOscilloscope_StateData))
                    //{
                    //    if (dgvMonitor.InvokeRequired)
                    //    {
                    //        Action SetData = delegate { SetSDSOscilloscope_StateData(monitorData); };
                    //        dgvMonitor.Invoke(SetData);
                    //    }
                    //    else
                    //    {
                    //        SetSDSOscilloscope_StateData(monitorData);
                    //    }
                    //}
                    else if (monitorData.GetType() == typeof(ACSource_StateData))
                    {
                        if (dgvMonitor.InvokeRequired)
                        {
                            Action SetData = delegate { SetACSourceSPWY_StateData(monitorData); };
                            dgvMonitor.Invoke(SetData);
                        }
                        else
                        {
                            SetACSourceSPWY_StateData(monitorData);
                        }
                    }
                    else if (monitorData.GetType() == typeof(ResisLoad_StateData))
                    {
                        if (dgvMonitor.InvokeRequired)
                        {
                            Action SetData = delegate { SetResisLoad_StateData(monitorData); };
                            dgvMonitor.Invoke(SetData);
                        }
                        else
                        {
                            SetResisLoad_StateData(monitorData);
                        }
                    }
                    else if (monitorData.GetType() == typeof(ResisLoad_MultiChannel_AC_StateData))
                    {
                        if (dgvMonitor.InvokeRequired)
                        {
                            Action SetData = delegate { SetResisLoad_MultiChannel_AC_StateData(monitorData); };
                            dgvMonitor.Invoke(SetData);
                        }
                        else
                        {
                            SetResisLoad_MultiChannel_AC_StateData(monitorData);
                        }
                    }
                    else if (monitorData.GetType() == typeof(ResisLoad_MultiChannel_DC_StateData))
                    {
                        if (dgvMonitor.InvokeRequired)
                        {
                            Action SetData = delegate { SetResisLoad_MultiChannel_DC_StateData(monitorData); };
                            dgvMonitor.Invoke(SetData);
                        }
                        else
                        {
                            SetResisLoad_MultiChannel_DC_StateData(monitorData);
                        }
                    }
                    else if (monitorData.GetType() == typeof(QCLeakageCurrent_StateData))   //刷新数据 
                    {
                        if (dgvMonitor.InvokeRequired)
                        {
                            Action SetData = delegate { SetQCLeakageCurrent_StateData(monitorData); };
                            dgvMonitor.Invoke(SetData);
                        }
                        else
                        {
                            SetQCLeakageCurrent_StateData(monitorData);
                        }
                    }
                    else if (monitorData.GetType() == typeof(ZJLeakageCurrent_StateData))
                    {
                        if (dgvMonitor.InvokeRequired)
                        {
                            Action SetData = delegate { SetZJLeakageCurrent_StateData(monitorData); };
                            dgvMonitor.Invoke(SetData);
                        }
                        else
                        {
                            SetZJLeakageCurrent_StateData(monitorData);
                        }
                    }
                    else if (monitorData.GetType() == typeof(PowerAnalyzer_StateData))
                    {
                        if (dgvMonitor.InvokeRequired)
                        {
                            Action SetData = delegate { SetPowerAnalyzer_StateData(monitorData); };
                            dgvMonitor.Invoke(SetData);
                        }
                        else
                        {
                            SetPowerAnalyzer_StateData(monitorData);
                        }
                    }
                    else if (monitorData.GetType() == typeof(ElectronicLoad_StateData))
                    {
                        if (dgvMonitor.InvokeRequired)
                        {
                            Action SetData = delegate { SetElectronicLoad_StateData(monitorData); };
                            dgvMonitor.Invoke(SetData);
                        }
                        else
                        {
                            SetElectronicLoad_StateData(monitorData);
                        }
                    }
                    else if (monitorData.GetType() == typeof(FeedbackLoad_StateData))
                    {
                        if (dgvMonitor.InvokeRequired)
                        {
                            Action SetData = delegate { SetFeedbackLoad_StateData(monitorData); };
                            dgvMonitor.Invoke(SetData);
                        }
                        else
                        {
                            SetFeedbackLoad_StateData(monitorData);
                        }
                    }
                    else if (monitorData.GetType() == typeof(FeedbackLoadAC_StateData))
                    {
                        if (dgvMonitor.InvokeRequired)
                        {
                            Action SetData = delegate { SetFeedbackLoadAC_StateData(monitorData); };
                            dgvMonitor.Invoke(SetData);
                        }
                        else
                        {
                            SetFeedbackLoadAC_StateData(monitorData);
                        }
                    }
                    else if (monitorData.GetType() == typeof(LoopFeedbackLoad_StateData))
                    {
                        if (dgvMonitor.InvokeRequired)
                        {
                            Action SetData = delegate { SetLoopFeedbackLoad_StateData(monitorData); };
                            dgvMonitor.Invoke(SetData);
                        }
                        else
                        {
                            SetLoopFeedbackLoad_StateData(monitorData);
                        }
                    }
                    else if (monitorData.GetType() == typeof(MultiMeter_StateData))
                    {
                        if (dgvMonitor.InvokeRequired)
                        {
                            Action SetData = delegate { SetMultiMeter_StateData(monitorData); };
                            dgvMonitor.Invoke(SetData);
                        }
                        else
                        {
                            SetMultiMeter_StateData(monitorData);
                        }
                    }
                    else if (monitorData.GetType() == typeof(AuxiliaryLoadCtrl_StateData))
                    {
                        if (dgvMonitor.InvokeRequired)
                        {
                            Action SetData = delegate { SetAuxiliaryLoadCtrl_StateData(monitorData); };
                            dgvMonitor.Invoke(SetData);
                        }
                        else
                        {
                            SetAuxiliaryLoadCtrl_StateData(monitorData);
                        }
                    }
                    else if (monitorData.GetType() == typeof(Charger_NTGX_StateData))
                    {
                        if (dgvMonitor.InvokeRequired)
                        {
                            Action SetData = delegate { SetChargerNTGXCtrl_StateData(monitorData); };
                            dgvMonitor.Invoke(SetData);
                        }
                        else
                        {
                            SetChargerNTGXCtrl_StateData(monitorData);
                        }
                    }
                    //其它设备按照这个格式填代码即可
                    //else if (monitorData.GetType() == typeof(这里填入其它设备类型))
                    //{
                    //    if (dgvMonitor.InvokeRequired)
                    //    {
                    //        Action SetData = delegate { 设置其它设备监视控件的方法(monitorData); };
                    //        dgvMonitor.Invoke(SetData);
                    //    }
                    //    else
                    //    {
                    //        设置其它设备监视控件的方法(monitorData);
                    //    }
                    //}
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }
        /// <summary>
        /// 设置直流BMS状态数据
        /// </summary>
        /// <param name="monitorData"></param>
        private void SetBMS_DC_StateData(object monitorData)
        {
            BMS_DC_StateData bmsStateData = monitorData as BMS_DC_StateData;

            if (dgvMonitor.Rows.Count == 0)
            {
                dgvMonitor.Rows.Add(8);

                dgvMonitor.Rows[0].Cells[0].Value = LanguageManager.GetByKey("充电电压") + "(V)";
                dgvMonitor.Rows[1].Cells[0].Value = LanguageManager.GetByKey("充电电流") + "(A)";

                dgvMonitor.Rows[2].Cells[0].Value = LanguageManager.GetByKey("充电状态");
                dgvMonitor.Rows[3].Cells[0].Value = LanguageManager.GetByKey("CC1电压") + "(V)";
                dgvMonitor.Rows[4].Cells[0].Value = LanguageManager.GetByKey("CC2电压") + "(V)";
                dgvMonitor.Rows[5].Cells[0].Value = LanguageManager.GetByKey("辅助电源电压") + "(V)"; ;
                dgvMonitor.Rows[6].Cells[0].Value = LanguageManager.GetByKey("DC+温度") + "(℃)";
                dgvMonitor.Rows[7].Cells[0].Value = LanguageManager.GetByKey("DC-温度") + "(℃)";
                //dgvMonitor.Rows[8].Cells[0].Value = LanguageManager.GetByKey("枪座温度") + "(℃)";
                //dgvMonitor.Rows[9].Cells[0].Value = LanguageManager.GetByKey("环境温度") + "(℃)";
            }

            dgvMonitor.Rows[0].Cells[1].Value = bmsStateData.ChargingVoltage.ToString();
            dgvMonitor.Rows[1].Cells[1].Value = bmsStateData.ChargingCurrent.ToString();

            dgvMonitor.Rows[2].Cells[1].Value = bmsStateData.ChargingState;
            dgvMonitor.Rows[3].Cells[1].Value = bmsStateData.CC1Voltage;
            dgvMonitor.Rows[4].Cells[1].Value = bmsStateData.CC2Voltage;
            dgvMonitor.Rows[5].Cells[1].Value = bmsStateData.APSVoltage;
            dgvMonitor.Rows[6].Cells[1].Value = bmsStateData.DCPulsTemp;
            dgvMonitor.Rows[7].Cells[1].Value = bmsStateData.DCMinusTemp;
            //dgvMonitor.Rows[8].Cells[1].Value = bmsStateData.ChargerTemp;
            //dgvMonitor.Rows[9].Cells[1].Value = bmsStateData.EnvironmentTemp;
            if (!AllEquipStateData.DicBMS_DC_StateData.ContainsKey(bmsStateData.ChargerID))
            {
                AllEquipStateData.DicBMS_DC_StateData.Add(bmsStateData.ChargerID, bmsStateData);
            }
            else
            {
                AllEquipStateData.DicBMS_DC_StateData[bmsStateData.ChargerID] = bmsStateData;
            }
        }

        /// <summary>
        /// 设置欧标直流BMS状态数据
        /// </summary>
        /// <param name="monitorData"></param>
        private void SetBMS_EU_DC_StateData(object monitorData)
        {
            BMS_EU_DC_StateData bmsStateData = monitorData as BMS_EU_DC_StateData;

            if (Customer != null && Customer.Equals("YKR"))
            {
                if (dgvMonitor.Rows.Count == 0)
                {
                    dgvMonitor.Rows.Add(9);

                    dgvMonitor.Rows[0].Cells[0].Value = LanguageManager.GetByKey("系统状态");
                    dgvMonitor.Rows[1].Cells[0].Value = LanguageManager.GetByKey("充电电压") + "(V)";
                    dgvMonitor.Rows[2].Cells[0].Value = LanguageManager.GetByKey("充电电流") + "(A)";
                    dgvMonitor.Rows[3].Cells[0].Value = LanguageManager.GetByKey("充电功率") + "(kW)";
                    dgvMonitor.Rows[4].Cells[0].Value = LanguageManager.GetByKey("CP占空比") + "(%)";
                    dgvMonitor.Rows[5].Cells[0].Value = LanguageManager.GetByKey("CP电压") + "(V)";
                    dgvMonitor.Rows[6].Cells[0].Value = LanguageManager.GetByKey("CP频率") + "(Hz)";
                    dgvMonitor.Rows[7].Cells[0].Value = LanguageManager.GetByKey("枪座温度") + "(℃)";
                    dgvMonitor.Rows[8].Cells[0].Value = LanguageManager.GetByKey("错误信息");
                }

                dgvMonitor.Rows[0].Cells[1].Value = bmsStateData.SystemState;
                dgvMonitor.Rows[1].Cells[1].Value = bmsStateData.ChargingVoltage.ToString();
                dgvMonitor.Rows[2].Cells[1].Value = bmsStateData.ChargingCurrent.ToString();
                dgvMonitor.Rows[3].Cells[1].Value = bmsStateData.ChargingPower;
                dgvMonitor.Rows[4].Cells[1].Value = bmsStateData.CPDutyCycle;
                dgvMonitor.Rows[5].Cells[1].Value = bmsStateData.CPVoltage;
                dgvMonitor.Rows[6].Cells[1].Value = bmsStateData.CPFrequency;
                dgvMonitor.Rows[7].Cells[1].Value = bmsStateData.ChargerTemp;
                dgvMonitor.Rows[8].Cells[1].Value = bmsStateData.ErrorMessage;
            }
            else
            {
                if (dgvMonitor.Rows.Count == 0)
                {
                    dgvMonitor.Rows.Add(10);

                    dgvMonitor.Rows[0].Cells[0].Value = LanguageManager.GetByKey("系统状态");
                    dgvMonitor.Rows[1].Cells[0].Value = LanguageManager.GetByKey("充电电压") + "(V)";
                    dgvMonitor.Rows[2].Cells[0].Value = LanguageManager.GetByKey("充电电流") + "(A)";
                    dgvMonitor.Rows[3].Cells[0].Value = LanguageManager.GetByKey("充电功率") + "(kW)";
                    dgvMonitor.Rows[4].Cells[0].Value = LanguageManager.GetByKey("充电电量") + "(kWh)";
                    dgvMonitor.Rows[5].Cells[0].Value = LanguageManager.GetByKey("CP占空比") + "(%)";
                    dgvMonitor.Rows[6].Cells[0].Value = LanguageManager.GetByKey("CP电压") + "(V)";
                    dgvMonitor.Rows[7].Cells[0].Value = LanguageManager.GetByKey("CP频率") + "(Hz)";
                    dgvMonitor.Rows[8].Cells[0].Value = LanguageManager.GetByKey("枪座温度") + "(℃)";
                    dgvMonitor.Rows[9].Cells[0].Value = LanguageManager.GetByKey("错误信息");
                }

                dgvMonitor.Rows[0].Cells[1].Value = bmsStateData.SystemState;
                dgvMonitor.Rows[1].Cells[1].Value = bmsStateData.ChargingVoltage.ToString();
                dgvMonitor.Rows[2].Cells[1].Value = bmsStateData.ChargingCurrent.ToString();
                dgvMonitor.Rows[3].Cells[1].Value = bmsStateData.ChargingPower;
                dgvMonitor.Rows[4].Cells[1].Value = bmsStateData.ChargingQuantity;
                dgvMonitor.Rows[5].Cells[1].Value = bmsStateData.CPDutyCycle;
                dgvMonitor.Rows[6].Cells[1].Value = bmsStateData.CPVoltage;
                dgvMonitor.Rows[7].Cells[1].Value = bmsStateData.CPFrequency;
                dgvMonitor.Rows[8].Cells[1].Value = bmsStateData.ChargerTemp;
                dgvMonitor.Rows[9].Cells[1].Value = bmsStateData.ErrorMessage;
            }
            if (!AllEquipStateData.DicBMS_EU_DC_StateData.ContainsKey(bmsStateData.ChargerID))
            {
                AllEquipStateData.DicBMS_EU_DC_StateData.Add(bmsStateData.ChargerID, bmsStateData);
            }
            else
            {
                AllEquipStateData.DicBMS_EU_DC_StateData[bmsStateData.ChargerID] = bmsStateData;
            }
        }

        /// <summary>
        /// 设置美标直流BMS状态数据
        /// </summary>
        /// <param name="monitorData"></param>
        private void SetBMS_USA_DC_StateData(object monitorData)
        {
            BMS_USA_DC_StateData bmsStateData = monitorData as BMS_USA_DC_StateData;

            if (dgvMonitor.Rows.Count == 0)
            {
                dgvMonitor.Rows.Add(10);

                dgvMonitor.Rows[0].Cells[0].Value = LanguageManager.GetByKey("系统状态");
                dgvMonitor.Rows[1].Cells[0].Value = LanguageManager.GetByKey("充电电压") + "(V)";
                dgvMonitor.Rows[2].Cells[0].Value = LanguageManager.GetByKey("充电电流") + "(A)";
                dgvMonitor.Rows[3].Cells[0].Value = LanguageManager.GetByKey("充电功率") + "(kW)";
                dgvMonitor.Rows[4].Cells[0].Value = LanguageManager.GetByKey("充电电量") + "(kWh)";
                dgvMonitor.Rows[5].Cells[0].Value = LanguageManager.GetByKey("CP占空比") + "(%)";
                dgvMonitor.Rows[6].Cells[0].Value = LanguageManager.GetByKey("CP电压") + "(V)";
                dgvMonitor.Rows[7].Cells[0].Value = LanguageManager.GetByKey("CP频率") + "(Hz)";
                dgvMonitor.Rows[8].Cells[0].Value = LanguageManager.GetByKey("枪座温度") + "(℃)";
                dgvMonitor.Rows[9].Cells[0].Value = LanguageManager.GetByKey("错误信息");
            }

            dgvMonitor.Rows[0].Cells[1].Value = bmsStateData.SystemState;
            dgvMonitor.Rows[1].Cells[1].Value = bmsStateData.ChargingVoltage.ToString();
            dgvMonitor.Rows[2].Cells[1].Value = bmsStateData.ChargingCurrent.ToString();
            dgvMonitor.Rows[3].Cells[1].Value = bmsStateData.ChargingPower;
            dgvMonitor.Rows[4].Cells[1].Value = bmsStateData.ChargingQuantity;
            dgvMonitor.Rows[5].Cells[1].Value = bmsStateData.CPDutyCycle;
            dgvMonitor.Rows[6].Cells[1].Value = bmsStateData.CPVoltage;
            dgvMonitor.Rows[7].Cells[1].Value = bmsStateData.CPFrequency;
            dgvMonitor.Rows[8].Cells[1].Value = bmsStateData.ChargerTemp;
            dgvMonitor.Rows[9].Cells[1].Value = bmsStateData.ErrorMessage;
            if (!AllEquipStateData.DicBMS_USA_DC_StateData.ContainsKey(bmsStateData.ChargerID))
            {
                AllEquipStateData.DicBMS_USA_DC_StateData.Add(bmsStateData.ChargerID, bmsStateData);
            }
            else
            {
                AllEquipStateData.DicBMS_USA_DC_StateData[bmsStateData.ChargerID] = bmsStateData;
            }
        }

        /// <summary>
        /// 设置日标直流BMS状态数据
        /// </summary>
        /// <param name="monitorData"></param>
        private void SetBMS_JP_DC_StateData(object monitorData)
        {
            BMS_JP_DC_StateData bmsStateData = monitorData as BMS_JP_DC_StateData;

            if (dgvMonitor.Rows.Count == 0)
            {
                dgvMonitor.Rows.Add(9);

                dgvMonitor.Rows[0].Cells[0].Value = LanguageManager.GetByKey("系统状态");
                dgvMonitor.Rows[1].Cells[0].Value = LanguageManager.GetByKey("充电电压") + "(V)";
                dgvMonitor.Rows[2].Cells[0].Value = LanguageManager.GetByKey("充电电流") + "(A)";
                dgvMonitor.Rows[3].Cells[0].Value = LanguageManager.GetByKey("环境温度") + "(℃)";
                dgvMonitor.Rows[4].Cells[0].Value = LanguageManager.GetByKey("版本号");
                dgvMonitor.Rows[5].Cells[0].Value = LanguageManager.GetByKey("Signal d1");
                dgvMonitor.Rows[6].Cells[0].Value = LanguageManager.GetByKey("Signal d2");
                dgvMonitor.Rows[7].Cells[0].Value = LanguageManager.GetByKey("Signal k");
                dgvMonitor.Rows[8].Cells[0].Value = LanguageManager.GetByKey("ProxDetect");
            }

            dgvMonitor.Rows[0].Cells[1].Value = bmsStateData.SystemState;
            dgvMonitor.Rows[1].Cells[1].Value = bmsStateData.ChargingVoltage.ToString();
            dgvMonitor.Rows[2].Cells[1].Value = bmsStateData.ChargingCurrent.ToString();
            dgvMonitor.Rows[3].Cells[1].Value = bmsStateData.ChargerTemp;
            dgvMonitor.Rows[4].Cells[1].Value = bmsStateData.Version;
            dgvMonitor.Rows[5].Cells[1].Value = bmsStateData.Signal_d1;
            dgvMonitor.Rows[6].Cells[1].Value = bmsStateData.Signal_d2;
            dgvMonitor.Rows[7].Cells[1].Value = bmsStateData.Signal_k;
            dgvMonitor.Rows[8].Cells[1].Value = bmsStateData.ProxDetect;
            if (!AllEquipStateData.DicBMS_JP_DC_StateData.ContainsKey(bmsStateData.ChargerID))
            {
                AllEquipStateData.DicBMS_JP_DC_StateData.Add(bmsStateData.ChargerID, bmsStateData);
            }
            else
            {
                AllEquipStateData.DicBMS_JP_DC_StateData[bmsStateData.ChargerID] = bmsStateData;
            }
        }

        /// <summary>
        /// 设置交流BMS状态数据
        /// </summary>
        /// <param name="monitorData"></param>
        private void SetBMS_AC_StateData(object monitorData)
        {
            BMS_AC_StateData StateData = monitorData as BMS_AC_StateData;

            //主板的电能和计量模块的电能不一样，隐藏
            if (Customer != null && Customer.Equals("HYQCP"))
            {
                if (dgvMonitor.Rows.Count == 0)
                {
                    dgvMonitor.Rows.Add(15);

                    dgvMonitor.Rows[0].Cells[0].Value = "A_" + LanguageManager.GetByKey("电压") + "(V)";
                    dgvMonitor.Rows[1].Cells[0].Value = "B_" + LanguageManager.GetByKey("电压") + "(V)";
                    dgvMonitor.Rows[2].Cells[0].Value = "C_" + LanguageManager.GetByKey("电压") + "(V)";
                    dgvMonitor.Rows[3].Cells[0].Value = "A_" + LanguageManager.GetByKey("电流") + "(A)";
                    dgvMonitor.Rows[4].Cells[0].Value = "B_" + LanguageManager.GetByKey("电流") + "(A)";
                    dgvMonitor.Rows[5].Cells[0].Value = "C_" + LanguageManager.GetByKey("电流") + "(A)";
                    dgvMonitor.Rows[6].Cells[0].Value = LanguageManager.GetByKey("充电功率") + "(kW)";
                    dgvMonitor.Rows[7].Cells[0].Value = LanguageManager.GetByKey("充电电量") + "(kWh)";
                    dgvMonitor.Rows[8].Cells[0].Value = "CP " + LanguageManager.GetByKey("占空比") + "(%)";
                    dgvMonitor.Rows[9].Cells[0].Value = "CP " + LanguageManager.GetByKey("电压") + "(V)";
                    dgvMonitor.Rows[10].Cells[0].Value = "CP " + LanguageManager.GetByKey("频率") + "(Hz)";
                    dgvMonitor.Rows[11].Cells[0].Value = "CC " + LanguageManager.GetByKey("电阻") + "/CS" + LanguageManager.GetByKey("电压") + "(Ω/V)";
                    dgvMonitor.Rows[12].Cells[0].Value = LanguageManager.GetByKey("允许充电电流") + "(A)";
                    dgvMonitor.Rows[13].Cells[0].Value = LanguageManager.GetByKey("枪连接状态");
                    dgvMonitor.Rows[14].Cells[0].Value = LanguageManager.GetByKey("系统状态");

                }

                //刷新电能的指令
                if (StateData.SystemState == "-1")
                {
                    dgvMonitor.Rows[7].Cells[1].Value = StateData.ChargeKwh <= 0 ? 0 : StateData.ChargeKwh;
                }
                else
                {
                    dgvMonitor.Rows[0].Cells[1].Value = StateData.PhaseA_Voltage;
                    dgvMonitor.Rows[1].Cells[1].Value = StateData.PhaseB_Voltage;
                    dgvMonitor.Rows[2].Cells[1].Value = StateData.PhaseC_Voltage;
                    dgvMonitor.Rows[3].Cells[1].Value = StateData.PhaseA_Current;
                    dgvMonitor.Rows[4].Cells[1].Value = StateData.PhaseB_Current;
                    dgvMonitor.Rows[5].Cells[1].Value = StateData.PhaseC_Current;
                    dgvMonitor.Rows[6].Cells[1].Value = StateData.ChargePower;
                    if(string.IsNullOrEmpty(Convert.ToString(dgvMonitor.Rows[7].Cells[1].Value)))
                    {
                        dgvMonitor.Rows[7].Cells[1].Value = 0;
                    }
                    dgvMonitor.Rows[8].Cells[1].Value = StateData.CPDutyCycle;
                    dgvMonitor.Rows[9].Cells[1].Value = StateData.CPVoltage;
                    dgvMonitor.Rows[10].Cells[1].Value = StateData.CPFrequency;
                    dgvMonitor.Rows[11].Cells[1].Value = StateData.CCResistance;
                    dgvMonitor.Rows[12].Cells[1].Value = StateData.AllowChargingCurrent;
                    dgvMonitor.Rows[13].Cells[1].Value = StateData.ConnectState;
                    dgvMonitor.Rows[14].Cells[1].Value = StateData.SystemState;
                }
            }
            else
            {
                if (dgvMonitor.Rows.Count == 0)
                {
                    dgvMonitor.Rows.Add(15);

                    dgvMonitor.Rows[0].Cells[0].Value = "A_" + LanguageManager.GetByKey("电压") + "(V)";
                    dgvMonitor.Rows[1].Cells[0].Value = "B_" + LanguageManager.GetByKey("电压") + "(V)";
                    dgvMonitor.Rows[2].Cells[0].Value = "C_" + LanguageManager.GetByKey("电压") + "(V)";
                    dgvMonitor.Rows[3].Cells[0].Value = "A_" + LanguageManager.GetByKey("电流") + "(A)";
                    dgvMonitor.Rows[4].Cells[0].Value = "B_" + LanguageManager.GetByKey("电流") + "(A)";
                    dgvMonitor.Rows[5].Cells[0].Value = "C_" + LanguageManager.GetByKey("电流") + "(A)";
                    dgvMonitor.Rows[6].Cells[0].Value = LanguageManager.GetByKey("充电功率") + "(kW)";
                    dgvMonitor.Rows[7].Cells[0].Value = LanguageManager.GetByKey("充电电量") + "(kWh)";
                    dgvMonitor.Rows[8].Cells[0].Value = "CP " + LanguageManager.GetByKey("占空比") + "(%)";
                    dgvMonitor.Rows[9].Cells[0].Value = "CP " + LanguageManager.GetByKey("电压") + "(V)";
                    dgvMonitor.Rows[10].Cells[0].Value = "CP " + LanguageManager.GetByKey("频率") + "(Hz)";
                    dgvMonitor.Rows[11].Cells[0].Value = "CC " + LanguageManager.GetByKey("电阻") + "/CS" + LanguageManager.GetByKey("电压") + "(Ω/V)";
                    dgvMonitor.Rows[12].Cells[0].Value = LanguageManager.GetByKey("允许充电电流") + "(A)";
                    dgvMonitor.Rows[13].Cells[0].Value = LanguageManager.GetByKey("枪连接状态");
                    dgvMonitor.Rows[14].Cells[0].Value = LanguageManager.GetByKey("系统状态");

                }

                dgvMonitor.Rows[0].Cells[1].Value = StateData.PhaseA_Voltage;
                dgvMonitor.Rows[1].Cells[1].Value = StateData.PhaseB_Voltage;
                dgvMonitor.Rows[2].Cells[1].Value = StateData.PhaseC_Voltage;
                dgvMonitor.Rows[3].Cells[1].Value = StateData.PhaseA_Current;
                dgvMonitor.Rows[4].Cells[1].Value = StateData.PhaseB_Current;
                dgvMonitor.Rows[5].Cells[1].Value = StateData.PhaseC_Current;
                dgvMonitor.Rows[6].Cells[1].Value = StateData.ChargePower;
                dgvMonitor.Rows[7].Cells[1].Value = StateData.ChargeKwh;
                dgvMonitor.Rows[8].Cells[1].Value = StateData.CPDutyCycle;
                dgvMonitor.Rows[9].Cells[1].Value = StateData.CPVoltage;
                dgvMonitor.Rows[10].Cells[1].Value = StateData.CPFrequency;
                dgvMonitor.Rows[11].Cells[1].Value = StateData.CCResistance;
                dgvMonitor.Rows[12].Cells[1].Value = StateData.AllowChargingCurrent;
                dgvMonitor.Rows[13].Cells[1].Value = StateData.ConnectState;
                dgvMonitor.Rows[14].Cells[1].Value = StateData.SystemState;
            }


            if (!AllEquipStateData.DicBMS_AC_StateData.ContainsKey(StateData.ChargerID))
            {
                AllEquipStateData.DicBMS_AC_StateData.Add(StateData.ChargerID, StateData);
            }
            else
            {
                AllEquipStateData.DicBMS_AC_StateData[StateData.ChargerID] = StateData;
            }
        }

        /// <summary>
        /// 设置鼎阳示波器状态数据
        /// </summary>
        /// <param name="monitorData"></param>
        private void SetSDSOscilloscope_StateData(object monitorData)
        {
            /* 示波器状态不监控
            SDSOscilloscope_StateData StateData = monitorData as SDSOscilloscope_StateData;

            if (dgvMonitor.Rows.Count == 0)
            {
                dgvMonitor.Rows.Add(14);

                dgvMonitor.Rows[0].Cells[0].Value = LanguageManager.GetByKey("");
                dgvMonitor.Rows[0].Cells[2].Value = LanguageManager.GetByKey("");
                dgvMonitor.Rows[1].Cells[0].Value = LanguageManager.GetByKey("");
                dgvMonitor.Rows[1].Cells[2].Value = LanguageManager.GetByKey("");
                dgvMonitor.Rows[2].Cells[0].Value = LanguageManager.GetByKey("");
                dgvMonitor.Rows[2].Cells[2].Value = LanguageManager.GetByKey("");
                dgvMonitor.Rows[3].Cells[0].Value = LanguageManager.GetByKey("");
                dgvMonitor.Rows[3].Cells[2].Value = LanguageManager.GetByKey("");
                dgvMonitor.Rows[4].Cells[0].Value = LanguageManager.GetByKey("");
                dgvMonitor.Rows[4].Cells[2].Value = LanguageManager.GetByKey("");
                dgvMonitor.Rows[5].Cells[0].Value = LanguageManager.GetByKey("");
                dgvMonitor.Rows[5].Cells[2].Value = LanguageManager.GetByKey("");
                dgvMonitor.Rows[6].Cells[0].Value = LanguageManager.GetByKey("");
                dgvMonitor.Rows[6].Cells[2].Value = LanguageManager.GetByKey("");
                dgvMonitor.Rows[7].Cells[0].Value = LanguageManager.GetByKey("");
                dgvMonitor.Rows[7].Cells[2].Value = LanguageManager.GetByKey("");
                dgvMonitor.Rows[8].Cells[0].Value = LanguageManager.GetByKey("");
                dgvMonitor.Rows[8].Cells[2].Value = LanguageManager.GetByKey("");
                dgvMonitor.Rows[9].Cells[0].Value = LanguageManager.GetByKey("");
                dgvMonitor.Rows[9].Cells[2].Value = LanguageManager.GetByKey("");
                dgvMonitor.Rows[10].Cells[0].Value = LanguageManager.GetByKey("");
                dgvMonitor.Rows[10].Cells[2].Value = LanguageManager.GetByKey("");
                dgvMonitor.Rows[11].Cells[0].Value = LanguageManager.GetByKey("");
                dgvMonitor.Rows[11].Cells[2].Value = LanguageManager.GetByKey("");
                dgvMonitor.Rows[12].Cells[0].Value = LanguageManager.GetByKey("");
                dgvMonitor.Rows[12].Cells[2].Value = LanguageManager.GetByKey("");
                dgvMonitor.Rows[13].Cells[0].Value = LanguageManager.GetByKey("");


            }

            //dgvMonitor.Rows[0].Cells[1].Value = StateData.;
            //dgvMonitor.Rows[1].Cells[1].Value = StateData.;
            //dgvMonitor.Rows[2].Cells[1].Value = StateData.;
            //dgvMonitor.Rows[3].Cells[1].Value = StateData.;
            //dgvMonitor.Rows[4].Cells[1].Value = StateData.;
            //dgvMonitor.Rows[5].Cells[1].Value = StateData.;
            //dgvMonitor.Rows[6].Cells[1].Value = StateData.;
            //dgvMonitor.Rows[7].Cells[1].Value = StateData.;
            //dgvMonitor.Rows[8].Cells[1].Value = StateData.;
            //dgvMonitor.Rows[9].Cells[1].Value = StateData.;
            //dgvMonitor.Rows[10].Cells[1].Value = StateData.;
            //dgvMonitor.Rows[11].Cells[1].Value = StateData.;
            //dgvMonitor.Rows[12].Cells[1].Value = StateData.;
            //dgvMonitor.Rows[13].Cells[1].Value = StateData.;


            if (!AllEquipStateData.DicSDSOscilloscope_StateData.ContainsKey(StateData.ChargerID))
            {
                AllEquipStateData.DicSDSOscilloscope_StateData.Add(StateData.ChargerID, StateData);
            }
            else
            {
                AllEquipStateData.DicSDSOscilloscope_StateData[StateData.ChargerID] = StateData;
            }
            */
        }

        private void SetACSourceSPWY_StateData(object monitorData)
        {
            ACSource_StateData StateData = monitorData as ACSource_StateData;

            string XHPhaseNumber = ConfigurationManager.AppSettings["XHPhaseNumber"];
            if (XHPhaseNumber != null && XHPhaseNumber.Equals("1"))
            {
                if (dgvMonitor.Rows.Count == 0)
                {
                    dgvMonitor.Rows.Add(3);

                    dgvMonitor.Rows[0].Cells[0].Value = LanguageManager.GetByKey("电压") + "(V)";
                    dgvMonitor.Rows[1].Cells[0].Value = LanguageManager.GetByKey("电流") + "(A)";
                    dgvMonitor.Rows[2].Cells[0].Value = LanguageManager.GetByKey("频率") + "(Hz)";
                }

                dgvMonitor.Rows[0].Cells[1].Value = StateData.Volt;
                dgvMonitor.Rows[1].Cells[1].Value = StateData.Current;
                dgvMonitor.Rows[2].Cells[1].Value = StateData.Freq;
            }
            else
            {
                if (dgvMonitor.Rows.Count == 0)
                {
                    dgvMonitor.Rows.Add(7);

                    dgvMonitor.Rows[0].Cells[0].Value = "A_" + LanguageManager.GetByKey("电压") + "(V)";
                    dgvMonitor.Rows[1].Cells[0].Value = "B_" + LanguageManager.GetByKey("电压") + "(V)";
                    dgvMonitor.Rows[2].Cells[0].Value = "C_" + LanguageManager.GetByKey("电压") + "(V)";
                    dgvMonitor.Rows[3].Cells[0].Value = "A_" + LanguageManager.GetByKey("电流") + "(A)";
                    dgvMonitor.Rows[4].Cells[0].Value = "B_" + LanguageManager.GetByKey("电流") + "(A)";
                    dgvMonitor.Rows[5].Cells[0].Value = "C_" + LanguageManager.GetByKey("电流") + "(A)";
                    dgvMonitor.Rows[6].Cells[0].Value = LanguageManager.GetByKey("频率") + "(Hz)";
                }

                dgvMonitor.Rows[0].Cells[1].Value = StateData.Volt;
                dgvMonitor.Rows[1].Cells[1].Value = StateData.Volt_B;
                dgvMonitor.Rows[2].Cells[1].Value = StateData.Volt_C;
                dgvMonitor.Rows[3].Cells[1].Value = StateData.Current;
                dgvMonitor.Rows[4].Cells[1].Value = StateData.Current_B;
                dgvMonitor.Rows[5].Cells[1].Value = StateData.Current_C;
                dgvMonitor.Rows[6].Cells[1].Value = StateData.Freq;
            }


            if (!AllEquipStateData.DicACSource_StateData.ContainsKey(StateData.ChargerID))
            {
                AllEquipStateData.DicACSource_StateData.Add(StateData.ChargerID, StateData);
            }
            else
            {
                AllEquipStateData.DicACSource_StateData[StateData.ChargerID] = StateData;
            }
        }

        private void SetResisLoad_StateData(object monitorData)
        {
            string curr_rate = ConfigurationManager.AppSettings["LoadRate"];//负载电流倍率
            ResisLoad_StateData StateData = monitorData as ResisLoad_StateData;
            if (StateData.emType == EmChargerType.Charger_GB_AC)
            {
                if (dgvMonitor.Rows.Count == 0)
                {
                    dgvMonitor.Rows.Add(13);
                    dgvMonitor.Rows[0].Cells[0].Value = LanguageManager.GetByKey("在线设备(台)");
                    dgvMonitor.Rows[1].Cells[0].Value = LanguageManager.GetByKey("需求") + " " + LanguageManager.GetByKey("电压") + "(V)";
                    dgvMonitor.Rows[2].Cells[0].Value = LanguageManager.GetByKey("需求") + " " + LanguageManager.GetByKey("电阻") + "(Ω)";
                    dgvMonitor.Rows[3].Cells[0].Value = LanguageManager.GetByKey("需求") + " " + LanguageManager.GetByKey("电流") + "(A)";
                    dgvMonitor.Rows[4].Cells[0].Value = LanguageManager.GetByKey("需求") + " " + LanguageManager.GetByKey("功率") + "(KW)";
                    dgvMonitor.Rows[5].Cells[0].Value = LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("功率") + "(KW)";
                    dgvMonitor.Rows[6].Cells[0].Value = LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电阻") + "(Ω)";
                    dgvMonitor.Rows[7].Cells[0].Value = "A_" + LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电压") + "(V)";
                    dgvMonitor.Rows[8].Cells[0].Value = "A_" + LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电流") + "(A)";
                    dgvMonitor.Rows[9].Cells[0].Value = "B_" + LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电压") + "(V)";
                    dgvMonitor.Rows[10].Cells[0].Value = "B" + LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电流") + "(A)";
                    dgvMonitor.Rows[11].Cells[0].Value = "C_" + LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电压") + "(V)";
                    dgvMonitor.Rows[12].Cells[0].Value = "C_" + LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电流") + "(A)";

                }
                dgvMonitor.Rows[0].Cells[1].Value = StateData.OnlineEquip;
                dgvMonitor.Rows[1].Cells[1].Value = StateData.DemandVolt;
                dgvMonitor.Rows[2].Cells[1].Value = StateData.DemandResis;
                dgvMonitor.Rows[3].Cells[1].Value = string.IsNullOrEmpty(curr_rate) ? StateData.DemandCurrent : StateData.DemandCurrent / (Convert.ToInt32(curr_rate) / 100.0);
                dgvMonitor.Rows[4].Cells[1].Value = StateData.DemandPower;
                dgvMonitor.Rows[5].Cells[1].Value = StateData.ActualPower;
                dgvMonitor.Rows[6].Cells[1].Value = StateData.ActualResis;
                dgvMonitor.Rows[7].Cells[1].Value = StateData.ActualVolt_A;
                dgvMonitor.Rows[8].Cells[1].Value = StateData.ActualVolt_B;
                dgvMonitor.Rows[9].Cells[1].Value = StateData.ActualVolt_C;
                dgvMonitor.Rows[10].Cells[1].Value = StateData.ActualCurrent_A;
                dgvMonitor.Rows[11].Cells[1].Value = StateData.ActualCurrent_B;
                dgvMonitor.Rows[12].Cells[1].Value = StateData.ActualCurrent_C;
            }
            else if (StateData.emType == EmChargerType.Charger_GB_DC)
            {
                if (dgvMonitor.Rows.Count == 0)
                {
                    dgvMonitor.Rows.Add(9);
                    dgvMonitor.Rows[0].Cells[0].Value = LanguageManager.GetByKey("在线设备(台)");
                    dgvMonitor.Rows[1].Cells[0].Value = LanguageManager.GetByKey("需求") + " " + LanguageManager.GetByKey("电压") + "(V)";
                    dgvMonitor.Rows[2].Cells[0].Value = LanguageManager.GetByKey("需求") + " " + LanguageManager.GetByKey("电阻") + "(Ω)";
                    dgvMonitor.Rows[3].Cells[0].Value = LanguageManager.GetByKey("需求") + " " + LanguageManager.GetByKey("电流") + "(Ω)";
                    dgvMonitor.Rows[4].Cells[0].Value = LanguageManager.GetByKey("需求") + " " + LanguageManager.GetByKey("功率") + "(KW)";
                    dgvMonitor.Rows[5].Cells[0].Value = LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("功率") + "(KW)";
                    dgvMonitor.Rows[6].Cells[0].Value = LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电阻") + "(Ω)";
                    dgvMonitor.Rows[7].Cells[0].Value = LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电压") + "(V)";
                    dgvMonitor.Rows[8].Cells[0].Value = LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电流") + "(A)";

                }
                dgvMonitor.Rows[0].Cells[1].Value = StateData.OnlineEquip;
                dgvMonitor.Rows[1].Cells[1].Value = StateData.DemandVolt;
                dgvMonitor.Rows[2].Cells[1].Value = StateData.DemandResis;
                dgvMonitor.Rows[3].Cells[1].Value = StateData.DemandCurrent;
                dgvMonitor.Rows[4].Cells[1].Value = StateData.DemandPower;
                dgvMonitor.Rows[5].Cells[1].Value = StateData.ActualPower;
                dgvMonitor.Rows[6].Cells[1].Value = StateData.ActualResis;
                dgvMonitor.Rows[7].Cells[1].Value = StateData.ActualVolt_A;
                dgvMonitor.Rows[8].Cells[1].Value = StateData.ActualCurrent_A;
            }


            if (!AllEquipStateData.DicResisLoad_StateData.ContainsKey(StateData.ChargerID))
            {
                AllEquipStateData.DicResisLoad_StateData.Add(StateData.ChargerID, StateData);
            }
            else
            {
                AllEquipStateData.DicResisLoad_StateData[StateData.ChargerID] = StateData;
            }
        }


        private void SetZJLeakageCurrent_StateData(object monitorData)
        {
            ZJLeakageCurrent_StateData StateData = monitorData as ZJLeakageCurrent_StateData;

            if (dgvMonitor.Rows.Count == 0)
            {
                dgvMonitor.Rows.Add(10);
                dgvMonitor.Rows[0].Cells[0].Value = LanguageManager.GetByKey("实时电压(V)");
                dgvMonitor.Rows[1].Cells[0].Value = LanguageManager.GetByKey("实时电流(mA)");
                dgvMonitor.Rows[2].Cells[0].Value = LanguageManager.GetByKey("测试电流(mA)");
                dgvMonitor.Rows[3].Cells[0].Value = LanguageManager.GetByKey("测试时间(mS)");
                dgvMonitor.Rows[4].Cells[0].Value = LanguageManager.GetByKey("报警显示");
                dgvMonitor.Rows[5].Cells[0].Value = LanguageManager.GetByKey("预先调整动作电流");
                dgvMonitor.Rows[6].Cells[0].Value = LanguageManager.GetByKey("ABC三相切换");
                dgvMonitor.Rows[7].Cells[0].Value = LanguageManager.GetByKey("S3开关状态");
                dgvMonitor.Rows[8].Cells[0].Value = LanguageManager.GetByKey("S2开关状态");
                dgvMonitor.Rows[9].Cells[0].Value = LanguageManager.GetByKey("S1开关状态");
            }
            dgvMonitor.Rows[0].Cells[1].Value = StateData.NowVoltage;
            dgvMonitor.Rows[1].Cells[1].Value = StateData.NowCurrent;
            dgvMonitor.Rows[2].Cells[1].Value = StateData.TestCurrent;
            dgvMonitor.Rows[3].Cells[1].Value = StateData.TestTime;
            dgvMonitor.Rows[4].Cells[1].Value = StateData.AlarmInfo;
            dgvMonitor.Rows[5].Cells[1].Value = StateData.PresetCurrent;
            dgvMonitor.Rows[6].Cells[1].Value = StateData.Phase;
            dgvMonitor.Rows[7].Cells[1].Value = StateData.S3_State;
            dgvMonitor.Rows[8].Cells[1].Value = StateData.S2_State;
            dgvMonitor.Rows[9].Cells[1].Value = StateData.S1_State;


            if (!AllEquipStateData.DicZJLeakageCurrent_StateData.ContainsKey(StateData.ChargerID))
            {
                AllEquipStateData.DicZJLeakageCurrent_StateData.Add(StateData.ChargerID, StateData);
            }
            else
            {
                AllEquipStateData.DicZJLeakageCurrent_StateData[StateData.ChargerID] = StateData;
            }
        }




        private void SetQCLeakageCurrent_StateData(object monitorData)
        {
            QCLeakageCurrent_StateData StateData = monitorData as QCLeakageCurrent_StateData;

            if (dgvMonitor.Rows.Count == 0)
            {
                dgvMonitor.Rows.Add(8);    //加入10行 
                dgvMonitor.Rows[0].Cells[0].Value = "分断电流(mA)";
                dgvMonitor.Rows[1].Cells[0].Value = "分断时间(ms)";
                dgvMonitor.Rows[2].Cells[0].Value = "结果状态";
                dgvMonitor.Rows[3].Cells[0].Value = "合闸状态";
                dgvMonitor.Rows[4].Cells[0].Value = "电压使能状态";
                dgvMonitor.Rows[5].Cells[0].Value = "电流使能状态";
                dgvMonitor.Rows[6].Cells[0].Value = "设备运行状态";
                dgvMonitor.Rows[7].Cells[0].Value = "设备受控状态";
            }
            dgvMonitor.Rows[0].Cells[1].Value = StateData.TripCurrent;
            dgvMonitor.Rows[1].Cells[1].Value = StateData.TripTime;
            dgvMonitor.Rows[2].Cells[1].Value = "未检测到分断(NO TRIP),检测到分断,过压保护分断,超时未闭合,安全检测未通过,自检失败,设备过流保护测试失败,设备电压异常测试失败".Split(',')[StateData.TestResult];
            dgvMonitor.Rows[3].Cells[1].Value = StateData.TestSW == 0 ? "未检测到合闸" : "检测到合闸";
            dgvMonitor.Rows[4].Cells[1].Value = StateData.EnableVoltage == 0 ? "已断开" : "已使能";
            dgvMonitor.Rows[5].Cells[1].Value = StateData.EnableCurrent == 0 ? "已断开" : "已使能";
            dgvMonitor.Rows[6].Cells[1].Value = "空闲,测试中,报警,自检1失败,自检2失败,自检3失败,自检4失败,自检5失败,过流保护,电压异常".Split(',')[StateData.RunTime];
            dgvMonitor.Rows[7].Cells[1].Value = StateData.RemoteStatus == 0 ? "手动模式" : "上位机控制模式";

            if (!AllEquipStateData.DicQCLeakageCurrent_StateData.ContainsKey(StateData.ChargerID))
            {
                AllEquipStateData.DicQCLeakageCurrent_StateData.Add(StateData.ChargerID, StateData);
            }
            else
            {
                AllEquipStateData.DicQCLeakageCurrent_StateData[StateData.ChargerID] = StateData;
            }
        }

        private void SetPowerAnalyzer_StateData(object monitorData)
        {
            PowerAnalyzer_StateData StateData = monitorData as PowerAnalyzer_StateData;

            if (Customer != null && Customer.Equals("HYQCP"))
            {
                if (dgvMonitor.Rows.Count == 0)
                {
                    dgvMonitor.Rows.Add(25);
                    dgvMonitor.Rows[0].Cells[0].Value = LanguageManager.GetByKey("效率值") + "η1(%)";
                    dgvMonitor.Rows[1].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 均方根电压(V)"), 1);
                    dgvMonitor.Rows[2].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 均方根电流(A)"), 1);
                    dgvMonitor.Rows[3].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 功率(kW)"), 1);
                    dgvMonitor.Rows[4].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 功率因数"), 1);
                    dgvMonitor.Rows[5].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 无功功率(Var)"), 1);

                    dgvMonitor.Rows[6].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 均方根电压(V)"), 2);
                    dgvMonitor.Rows[7].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 均方根电流(A)"), 2);
                    dgvMonitor.Rows[8].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 功率(kW)"), 2);
                    dgvMonitor.Rows[9].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 功率因数"), 2);
                    dgvMonitor.Rows[10].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 无功功率(Var)"), 2);

                    dgvMonitor.Rows[11].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 均方根电压(V)"), 3);
                    dgvMonitor.Rows[12].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 均方根电流(A)"), 3);
                    dgvMonitor.Rows[13].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 功率(kW)"), 3);
                    dgvMonitor.Rows[14].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 功率因数"), 3);
                    dgvMonitor.Rows[15].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 无功功率(Var)"), 3);

                    dgvMonitor.Rows[16].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 均方根电压(V)"), 4);
                    dgvMonitor.Rows[17].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 均方根电流(A)"), 4);
                    dgvMonitor.Rows[18].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 功率(kW)"), 4);
                    dgvMonitor.Rows[19].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 功率因数"), 4);
                    dgvMonitor.Rows[20].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 无功功率(Var)"), 4);

                    //dgvMonitor.Rows[21].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 均方根电压(V)"), 5);
                    //dgvMonitor.Rows[22].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 均方根电流(A)"), 5);
                    //dgvMonitor.Rows[23].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 功率(kW)"), 5);
                    //dgvMonitor.Rows[24].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 功率因数"), 5);
                    //dgvMonitor.Rows[25].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 无功功率(Var)"), 5);

                    //dgvMonitor.Rows[26].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 均方根电压(V)"), 6);
                    //dgvMonitor.Rows[27].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 均方根电流(A)"), 6);
                    //dgvMonitor.Rows[28].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 功率(kW)"), 6);
                    //dgvMonitor.Rows[29].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 功率因数"), 6);
                    //dgvMonitor.Rows[30].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 无功功率(Var)"), 6);

                    dgvMonitor.Rows[21].Cells[0].Value = LanguageManager.GetByKey("ΣA组三相总电压值(V)");
                    dgvMonitor.Rows[22].Cells[0].Value = LanguageManager.GetByKey("ΣA组三相总电流值(V)");
                    dgvMonitor.Rows[23].Cells[0].Value = LanguageManager.GetByKey("ΣA组三相总功率(kW)");
                    dgvMonitor.Rows[24].Cells[0].Value = LanguageManager.GetByKey("ΣA组三相功率因数");
                }
                dgvMonitor.Rows[0].Cells[1].Value = StateData.Efficiency;
                dgvMonitor.Rows[1].Cells[1].Value = StateData.Channel1RMSVolt;
                dgvMonitor.Rows[2].Cells[1].Value = StateData.Channel1RMSCurrent;
                dgvMonitor.Rows[3].Cells[1].Value = StateData.Channel1Power;
                dgvMonitor.Rows[4].Cells[1].Value = StateData.Channel1PowerFactor;
                dgvMonitor.Rows[5].Cells[1].Value = StateData.Channel1ReactivePower;
                dgvMonitor.Rows[6].Cells[1].Value = StateData.Channel2RMSVolt;
                dgvMonitor.Rows[7].Cells[1].Value = StateData.Channel2RMSCurrent;
                dgvMonitor.Rows[8].Cells[1].Value = StateData.Channel2Power;
                dgvMonitor.Rows[9].Cells[1].Value = StateData.Channel2PowerFactor;
                dgvMonitor.Rows[10].Cells[1].Value = StateData.Channel2ReactivePower;
                dgvMonitor.Rows[11].Cells[1].Value = StateData.Channel3RMSVolt;
                dgvMonitor.Rows[12].Cells[1].Value = StateData.Channel3RMSCurrent;
                dgvMonitor.Rows[13].Cells[1].Value = StateData.Channel3Power;
                dgvMonitor.Rows[14].Cells[1].Value = StateData.Channel3PowerFactor;
                dgvMonitor.Rows[15].Cells[1].Value = StateData.Channel3ReactivePower;
                dgvMonitor.Rows[16].Cells[1].Value = StateData.Channel4RMSVolt;
                dgvMonitor.Rows[17].Cells[1].Value = StateData.Channel4RMSCurrent;
                dgvMonitor.Rows[18].Cells[1].Value = StateData.Channel4Power;
                dgvMonitor.Rows[19].Cells[1].Value = StateData.Channel4PowerFactor;
                dgvMonitor.Rows[20].Cells[1].Value = StateData.Channel4ReactivePower;
                //dgvMonitor.Rows[21].Cells[1].Value = StateData.Channel5RMSVolt;
                //dgvMonitor.Rows[22].Cells[1].Value = StateData.Channel5RMSCurrent;
                //dgvMonitor.Rows[23].Cells[1].Value = StateData.Channel5Power;
                //dgvMonitor.Rows[24].Cells[1].Value = StateData.Channel5PowerFactor;
                //dgvMonitor.Rows[25].Cells[1].Value = StateData.Channel5ReactivePower;
                //dgvMonitor.Rows[26].Cells[1].Value = StateData.Channel6RMSVolt;
                //dgvMonitor.Rows[27].Cells[1].Value = StateData.Channel6RMSCurrent;
                //dgvMonitor.Rows[28].Cells[1].Value = StateData.Channel6Power;
                //dgvMonitor.Rows[29].Cells[1].Value = StateData.Channel6PowerFactor;
                //dgvMonitor.Rows[30].Cells[1].Value = StateData.Channel6ReactivePower;
                dgvMonitor.Rows[26 - 5].Cells[1].Value = StateData.TotalVoltage;
                dgvMonitor.Rows[27 - 5].Cells[1].Value = StateData.TotalCurrent;
                dgvMonitor.Rows[28 - 5].Cells[1].Value = StateData.TotalPower;
                dgvMonitor.Rows[29 - 5].Cells[1].Value = StateData.TotalPowerFactor;
            }
            else
            {
                if (dgvMonitor.Rows.Count == 0)
                {
                    dgvMonitor.Rows.Add(25);
                    dgvMonitor.Rows[0].Cells[0].Value = LanguageManager.GetByKey("效率值") + "η1(%)";
                    dgvMonitor.Rows[1].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 均方根电压(V)"), 1);
                    dgvMonitor.Rows[2].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 均方根电流(A)"), 1);
                    dgvMonitor.Rows[3].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 功率(kW)"), 1);
                    dgvMonitor.Rows[4].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 功率因数"), 1);
                    dgvMonitor.Rows[5].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 无功功率(Var)"), 1);

                    dgvMonitor.Rows[6].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 均方根电压(V)"), 2);
                    dgvMonitor.Rows[7].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 均方根电流(A)"), 2);
                    dgvMonitor.Rows[8].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 功率(kW)"), 2);
                    dgvMonitor.Rows[9].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 功率因数"), 2);
                    dgvMonitor.Rows[10].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 无功功率(Var)"), 2);

                    dgvMonitor.Rows[11].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 均方根电压(V)"), 3);
                    dgvMonitor.Rows[12].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 均方根电流(A)"), 3);
                    dgvMonitor.Rows[13].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 功率(kW)"), 3);
                    dgvMonitor.Rows[14].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 功率因数"), 3);
                    dgvMonitor.Rows[15].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 无功功率(Var)"), 3);

                    dgvMonitor.Rows[16].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 均方根电压(V)"), 4);
                    dgvMonitor.Rows[17].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 均方根电流(A)"), 4);
                    dgvMonitor.Rows[18].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 功率(kW)"), 4);
                    dgvMonitor.Rows[19].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 功率因数"), 4);
                    dgvMonitor.Rows[20].Cells[0].Value = string.Format(LanguageManager.GetByKey("通道{0} 无功功率(Var)"), 4);


                    dgvMonitor.Rows[21].Cells[0].Value = LanguageManager.GetByKey("ΣA组三相总电压值(V)");
                    dgvMonitor.Rows[22].Cells[0].Value = LanguageManager.GetByKey("ΣA组三相总电流值(V)");
                    dgvMonitor.Rows[23].Cells[0].Value = LanguageManager.GetByKey("ΣA组三相总功率(kW)");
                    dgvMonitor.Rows[24].Cells[0].Value = LanguageManager.GetByKey("ΣA组三相功率因数");

                }
                dgvMonitor.Rows[0].Cells[1].Value = StateData.Efficiency;
                dgvMonitor.Rows[1].Cells[1].Value = StateData.Channel1RMSVolt;
                dgvMonitor.Rows[2].Cells[1].Value = StateData.Channel1RMSCurrent;
                dgvMonitor.Rows[3].Cells[1].Value = StateData.Channel1Power;
                dgvMonitor.Rows[4].Cells[1].Value = StateData.Channel1PowerFactor;
                dgvMonitor.Rows[5].Cells[1].Value = StateData.Channel1ReactivePower;
                dgvMonitor.Rows[6].Cells[1].Value = StateData.Channel2RMSVolt;
                dgvMonitor.Rows[7].Cells[1].Value = StateData.Channel2RMSCurrent;
                dgvMonitor.Rows[8].Cells[1].Value = StateData.Channel2Power;
                dgvMonitor.Rows[9].Cells[1].Value = StateData.Channel2PowerFactor;
                dgvMonitor.Rows[10].Cells[1].Value = StateData.Channel2ReactivePower;
                dgvMonitor.Rows[11].Cells[1].Value = StateData.Channel3RMSVolt;
                dgvMonitor.Rows[12].Cells[1].Value = StateData.Channel3RMSCurrent;
                dgvMonitor.Rows[13].Cells[1].Value = StateData.Channel3Power;
                dgvMonitor.Rows[14].Cells[1].Value = StateData.Channel3PowerFactor;
                dgvMonitor.Rows[15].Cells[1].Value = StateData.Channel3ReactivePower;
                dgvMonitor.Rows[16].Cells[1].Value = StateData.Channel4RMSVolt;
                dgvMonitor.Rows[17].Cells[1].Value = StateData.Channel4RMSCurrent;
                dgvMonitor.Rows[18].Cells[1].Value = StateData.Channel4Power;
                dgvMonitor.Rows[19].Cells[1].Value = StateData.Channel4PowerFactor;
                dgvMonitor.Rows[20].Cells[1].Value = StateData.Channel4ReactivePower;
                //dgvMonitor.Rows[21].Cells[1].Value = StateData.Channel5RMSVolt;
                //dgvMonitor.Rows[22].Cells[1].Value = StateData.Channel5RMSCurrent;
                //dgvMonitor.Rows[23].Cells[1].Value = StateData.Channel5Power;
                //dgvMonitor.Rows[24].Cells[1].Value = StateData.Channel5PowerFactor;
                //dgvMonitor.Rows[25].Cells[1].Value = StateData.Channel5ReactivePower;
                dgvMonitor.Rows[26 - 5].Cells[1].Value = StateData.TotalVoltage;
                dgvMonitor.Rows[27 - 5].Cells[1].Value = StateData.TotalCurrent;
                dgvMonitor.Rows[28 - 5].Cells[1].Value = StateData.TotalPower;
                dgvMonitor.Rows[29 - 5].Cells[1].Value = StateData.TotalPowerFactor;
            }


            if (!AllEquipStateData.DicPowerAnalyzer_StateData.ContainsKey(StateData.ChargerID))
            {
                AllEquipStateData.DicPowerAnalyzer_StateData.Add(StateData.ChargerID, StateData);
            }
            else
            {
                AllEquipStateData.DicPowerAnalyzer_StateData[StateData.ChargerID] = StateData;
            }
        }


        private void SetElectronicLoad_StateData(object monitorData)
        {
            ElectronicLoad_StateData StateData = monitorData as ElectronicLoad_StateData;

            if (dgvMonitor.Rows.Count == 0)
            {
                dgvMonitor.Rows.Add(7);
                dgvMonitor.Rows[0].Cells[0].Value = LanguageManager.GetByKey("输入电压值(V)");
                dgvMonitor.Rows[1].Cells[0].Value = LanguageManager.GetByKey("输入电流值(A)");
                dgvMonitor.Rows[2].Cells[0].Value = LanguageManager.GetByKey("输入功率值(W)");
                dgvMonitor.Rows[3].Cells[0].Value = LanguageManager.GetByKey("操作状态") + "1";
                dgvMonitor.Rows[4].Cells[0].Value = LanguageManager.GetByKey("操作状态") + "2";
                dgvMonitor.Rows[5].Cells[0].Value = LanguageManager.GetByKey("系统状态") + "1";
                dgvMonitor.Rows[6].Cells[0].Value = LanguageManager.GetByKey("系统状态") + "2";

            }
            dgvMonitor.Rows[0].Cells[1].Value = StateData.InputVoltage;
            dgvMonitor.Rows[1].Cells[1].Value = StateData.InputCurrent;
            dgvMonitor.Rows[2].Cells[1].Value = StateData.InputPower;
            dgvMonitor.Rows[3].Cells[1].Value = StateData.OperateState1;
            dgvMonitor.Rows[4].Cells[1].Value = StateData.OperateState2;
            dgvMonitor.Rows[5].Cells[1].Value = StateData.SystemState1;
            dgvMonitor.Rows[6].Cells[1].Value = StateData.SystemState2;



            if (!AllEquipStateData.DicElectronicLoad_StateData.ContainsKey(StateData.ChargerID))
            {
                AllEquipStateData.DicElectronicLoad_StateData.Add(StateData.ChargerID, StateData);
            }
            else
            {
                AllEquipStateData.DicElectronicLoad_StateData[StateData.ChargerID] = StateData;
            }
        }

        private void SetFeedbackLoad_StateData(object monitorData)
        {
            FeedbackLoad_StateData StateData = monitorData as FeedbackLoad_StateData;

            if (dgvMonitor.Rows.Count == 0)
            {
                dgvMonitor.Rows.Add(3);
                dgvMonitor.Rows[0].Cells[0].Value = LanguageManager.GetByKey("通道");
                dgvMonitor.Rows[1].Cells[0].Value = LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电压") + "(V)";
                dgvMonitor.Rows[2].Cells[0].Value = LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电流") + "(A)";

            }
            dgvMonitor.Rows[0].Cells[1].Value = StateData.ChargerID;
            dgvMonitor.Rows[1].Cells[1].Value = StateData.Voltage;
            dgvMonitor.Rows[2].Cells[1].Value = StateData.Current;




            if (!AllEquipStateData.DicFeedbackLoad_StateData.ContainsKey(StateData.ChargerID))
            {
                AllEquipStateData.DicFeedbackLoad_StateData.Add(StateData.ChargerID, StateData);
            }
            else
            {
                AllEquipStateData.DicFeedbackLoad_StateData[StateData.ChargerID] = StateData;
            }
        }
        private void SetResisLoad_MultiChannel_AC_StateData(object monitorData)
        {
            ResisLoad_MultiChannel_AC_StateData StateData = monitorData as ResisLoad_MultiChannel_AC_StateData;

            if (dgvMonitor.Rows.Count == 0)
            {
                dgvMonitor.Rows.Add(3);
                dgvMonitor.Rows[0].Cells[0].Value = LanguageManager.GetByKey("通道");
                dgvMonitor.Rows[1].Cells[0].Value = LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电压") + "(V)";
                dgvMonitor.Rows[2].Cells[0].Value = LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电流") + "(A)";

            }
            dgvMonitor.Rows[0].Cells[1].Value = StateData.ChargerID;
            dgvMonitor.Rows[1].Cells[1].Value = StateData.Voltage;
            dgvMonitor.Rows[2].Cells[1].Value = StateData.Current;




            if (!AllEquipStateData.DicResisLoad_MultiChannel_AC_StateData.ContainsKey(StateData.ChargerID))
            {
                AllEquipStateData.DicResisLoad_MultiChannel_AC_StateData.Add(StateData.ChargerID, StateData);
            }
            else
            {
                AllEquipStateData.DicResisLoad_MultiChannel_AC_StateData[StateData.ChargerID] = StateData;
            }
        }
        private void SetResisLoad_MultiChannel_DC_StateData(object monitorData)
        {
            ResisLoad_MultiChannel_DC_StateData StateData = monitorData as ResisLoad_MultiChannel_DC_StateData;

            if (dgvMonitor.Rows.Count == 0)
            {
                dgvMonitor.Rows.Add(3);
                dgvMonitor.Rows[0].Cells[0].Value = LanguageManager.GetByKey("通道");
                dgvMonitor.Rows[1].Cells[0].Value = LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电压") + "(V)";
                dgvMonitor.Rows[2].Cells[0].Value = LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电流") + "(A)";

            }
            dgvMonitor.Rows[0].Cells[1].Value = StateData.ChargerID;
            dgvMonitor.Rows[1].Cells[1].Value = StateData.Voltage;
            dgvMonitor.Rows[2].Cells[1].Value = StateData.Current;




            if (!AllEquipStateData.DicResisLoad_MultiChannel_DC_StateData.ContainsKey(StateData.ChargerID))
            {
                AllEquipStateData.DicResisLoad_MultiChannel_DC_StateData.Add(StateData.ChargerID, StateData);
            }
            else
            {
                AllEquipStateData.DicResisLoad_MultiChannel_DC_StateData[StateData.ChargerID] = StateData;
            }
        }

        private void SetFeedbackLoadAC_StateData(object monitorData)
        {
            FeedbackLoadAC_StateData StateData = monitorData as FeedbackLoadAC_StateData;

            if (dgvMonitor.Rows.Count == 0)
            {
                dgvMonitor.Rows.Add(7);
                dgvMonitor.Rows[0].Cells[0].Value = "A_" + LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电压") + "(V)";
                dgvMonitor.Rows[1].Cells[0].Value = "A_" + LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电流") + "(A)";
                dgvMonitor.Rows[2].Cells[0].Value = "B_" + LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电压") + "(V)";
                dgvMonitor.Rows[3].Cells[0].Value = "B_" + LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电流") + "(A)";
                dgvMonitor.Rows[4].Cells[0].Value = "C_" + LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电压") + "(V)";
                dgvMonitor.Rows[5].Cells[0].Value = "C_" + LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电流") + "(A)";
                dgvMonitor.Rows[6].Cells[0].Value = "ID";

            }
            dgvMonitor.Rows[0].Cells[1].Value = StateData.ActualVolt_A;
            dgvMonitor.Rows[1].Cells[1].Value = StateData.ActualCurrent_A;
            dgvMonitor.Rows[2].Cells[1].Value = StateData.ActualVolt_B;
            dgvMonitor.Rows[3].Cells[1].Value = StateData.ActualCurrent_B;
            dgvMonitor.Rows[4].Cells[1].Value = StateData.ActualVolt_C;
            dgvMonitor.Rows[5].Cells[1].Value = StateData.ActualCurrent_C;
            dgvMonitor.Rows[6].Cells[1].Value = StateData.ChargerID;



            if (!AllEquipStateData.DicFeedbackLoadAC_StateData.ContainsKey(StateData.ChargerID))
            {
                AllEquipStateData.DicFeedbackLoadAC_StateData.Add(StateData.ChargerID, StateData);
            }
            else
            {
                AllEquipStateData.DicFeedbackLoadAC_StateData[StateData.ChargerID] = StateData;
            }
        }

        private void SetLoopFeedbackLoad_StateData(object monitorData)
        {
            LoopFeedbackLoad_StateData StateData = monitorData as LoopFeedbackLoad_StateData;

            if (dgvMonitor.Rows.Count == 0)
            {
                dgvMonitor.Rows.Add(48);
                dgvMonitor.Rows[0].Cells[0].Value = LanguageManager.GetByKey("通道") + 1 + LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电压") + "(V)";
                dgvMonitor.Rows[1].Cells[0].Value = LanguageManager.GetByKey("通道") + 1 + LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电流") + "(A)";
                dgvMonitor.Rows[2].Cells[0].Value = LanguageManager.GetByKey("通道") + 1 + LanguageManager.GetByKey("运行状态");

                dgvMonitor.Rows[3].Cells[0].Value = LanguageManager.GetByKey("通道") + 2 + LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电压") + "(V)";
                dgvMonitor.Rows[4].Cells[0].Value = LanguageManager.GetByKey("通道") + 2 + LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电流") + "(A)";
                dgvMonitor.Rows[5].Cells[0].Value = LanguageManager.GetByKey("通道") + 2 + LanguageManager.GetByKey("运行状态");


                dgvMonitor.Rows[6].Cells[0].Value = LanguageManager.GetByKey("通道") +3 +LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电压") + "(V)";
                dgvMonitor.Rows[7].Cells[0].Value = LanguageManager.GetByKey("通道") +3 +LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电流") + "(A)";
                dgvMonitor.Rows[8].Cells[0].Value = LanguageManager.GetByKey("通道") +3 +LanguageManager.GetByKey("运行状态");

                dgvMonitor.Rows[9].Cells[0].Value = LanguageManager.GetByKey("通道") +4 +LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电压") + "(V)";
                dgvMonitor.Rows[10].Cells[0].Value = LanguageManager.GetByKey("通道") + 4+LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电流") + "(A)";
                dgvMonitor.Rows[11].Cells[0].Value = LanguageManager.GetByKey("通道") + 4+LanguageManager.GetByKey("运行状态");

                dgvMonitor.Rows[12].Cells[0].Value = LanguageManager.GetByKey("通道") + 5+LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电压") + "(V)";
                dgvMonitor.Rows[13].Cells[0].Value = LanguageManager.GetByKey("通道") +5 +LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电流") + "(A)";
                dgvMonitor.Rows[14].Cells[0].Value = LanguageManager.GetByKey("通道") +5 +LanguageManager.GetByKey("运行状态");

                dgvMonitor.Rows[15].Cells[0].Value = LanguageManager.GetByKey("通道") +6 +LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电压") + "(V)";
                dgvMonitor.Rows[16].Cells[0].Value = LanguageManager.GetByKey("通道") +6 +LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电流") + "(A)";
                dgvMonitor.Rows[17].Cells[0].Value = LanguageManager.GetByKey("通道") +6 +LanguageManager.GetByKey("运行状态");

                dgvMonitor.Rows[18].Cells[0].Value = LanguageManager.GetByKey("通道") + 7+LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电压") + "(V)";
                dgvMonitor.Rows[19].Cells[0].Value = LanguageManager.GetByKey("通道") + 7+LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电流") + "(A)";
                dgvMonitor.Rows[20].Cells[0].Value = LanguageManager.GetByKey("通道") + 7+LanguageManager.GetByKey("运行状态");

                dgvMonitor.Rows[21].Cells[0].Value = LanguageManager.GetByKey("通道") + 8+LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电压") + "(V)";
                dgvMonitor.Rows[22].Cells[0].Value = LanguageManager.GetByKey("通道") +8 +LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电流") + "(A)";
                dgvMonitor.Rows[23].Cells[0].Value = LanguageManager.GetByKey("通道") +8 +LanguageManager.GetByKey("运行状态");

                dgvMonitor.Rows[24].Cells[0].Value = LanguageManager.GetByKey("通道") + 9+LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电压") + "(V)";
                dgvMonitor.Rows[25].Cells[0].Value = LanguageManager.GetByKey("通道") + 9+LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电流") + "(A)";
                dgvMonitor.Rows[26].Cells[0].Value = LanguageManager.GetByKey("通道") + 9+LanguageManager.GetByKey("运行状态");

                dgvMonitor.Rows[27].Cells[0].Value = LanguageManager.GetByKey("通道") + 10 + LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电压") + "(V)";
                dgvMonitor.Rows[28].Cells[0].Value = LanguageManager.GetByKey("通道") + 10 + LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电流") + "(A)";
                dgvMonitor.Rows[29].Cells[0].Value = LanguageManager.GetByKey("通道") + 10 + LanguageManager.GetByKey("运行状态");

                dgvMonitor.Rows[30].Cells[0].Value = LanguageManager.GetByKey("通道") + 11 + LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电压") + "(V)";
                dgvMonitor.Rows[31].Cells[0].Value = LanguageManager.GetByKey("通道") + 11 + LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电流") + "(A)";
                dgvMonitor.Rows[32].Cells[0].Value = LanguageManager.GetByKey("通道") + 11 + LanguageManager.GetByKey("运行状态");

                dgvMonitor.Rows[33].Cells[0].Value = LanguageManager.GetByKey("通道") + 12 + LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电压") + "(V)";
                dgvMonitor.Rows[34].Cells[0].Value = LanguageManager.GetByKey("通道") + 12 + LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电流") + "(A)";
                dgvMonitor.Rows[35].Cells[0].Value = LanguageManager.GetByKey("通道") + 12 + LanguageManager.GetByKey("运行状态");

                dgvMonitor.Rows[36].Cells[0].Value = LanguageManager.GetByKey("通道") + 13 + LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电压") + "(V)";
                dgvMonitor.Rows[37].Cells[0].Value = LanguageManager.GetByKey("通道") + 13 + LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电流") + "(A)";
                dgvMonitor.Rows[38].Cells[0].Value = LanguageManager.GetByKey("通道") + 13 + LanguageManager.GetByKey("运行状态");

                dgvMonitor.Rows[39].Cells[0].Value = LanguageManager.GetByKey("通道") + 14 + LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电压") + "(V)";
                dgvMonitor.Rows[40].Cells[0].Value = LanguageManager.GetByKey("通道") + 14 + LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电流") + "(A)";
                dgvMonitor.Rows[41].Cells[0].Value = LanguageManager.GetByKey("通道") + 14 + LanguageManager.GetByKey("运行状态");

                dgvMonitor.Rows[42].Cells[0].Value = LanguageManager.GetByKey("通道") + 15 + LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电压") + "(V)";
                dgvMonitor.Rows[43].Cells[0].Value = LanguageManager.GetByKey("通道") + 15 + LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电流") + "(A)";
                dgvMonitor.Rows[44].Cells[0].Value = LanguageManager.GetByKey("通道") + 15 + LanguageManager.GetByKey("运行状态");

                dgvMonitor.Rows[45].Cells[0].Value = LanguageManager.GetByKey("通道") + 16 + LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电压") + "(V)";
                dgvMonitor.Rows[46].Cells[0].Value = LanguageManager.GetByKey("通道") + 16 + LanguageManager.GetByKey("实际") + " " + LanguageManager.GetByKey("电流") + "(A)";
                dgvMonitor.Rows[47].Cells[0].Value = LanguageManager.GetByKey("通道") + 16 + LanguageManager.GetByKey("运行状态");

            }

            dgvMonitor.Rows[0].Cells[1].Value = StateData.Voltage_1;
            dgvMonitor.Rows[1].Cells[1].Value = StateData.Current_1;
            dgvMonitor.Rows[2].Cells[1].Value = StateData.RunState_1;

            dgvMonitor.Rows[3].Cells[1].Value = StateData.Voltage_2;
            dgvMonitor.Rows[4].Cells[1].Value = StateData.Current_2;
            dgvMonitor.Rows[5].Cells[1].Value = StateData.RunState_2;

            dgvMonitor.Rows[6].Cells[1].Value = StateData.Voltage_3;
            dgvMonitor.Rows[7].Cells[1].Value = StateData.Current_3;
            dgvMonitor.Rows[8].Cells[1].Value = StateData.RunState_3;

            dgvMonitor.Rows[9].Cells[1].Value = StateData.Voltage_4;
            dgvMonitor.Rows[10].Cells[1].Value = StateData.Current_4;
            dgvMonitor.Rows[11].Cells[1].Value = StateData.RunState_4;

            dgvMonitor.Rows[12].Cells[1].Value = StateData.Voltage_5;
            dgvMonitor.Rows[13].Cells[1].Value = StateData.Current_5;
            dgvMonitor.Rows[14].Cells[1].Value = StateData.RunState_5;

            dgvMonitor.Rows[15].Cells[1].Value = StateData.Voltage_6;
            dgvMonitor.Rows[16].Cells[1].Value = StateData.Current_6;
            dgvMonitor.Rows[17].Cells[1].Value = StateData.RunState_6;

            dgvMonitor.Rows[18].Cells[1].Value = StateData.Voltage_7;
            dgvMonitor.Rows[19].Cells[1].Value = StateData.Current_7;
            dgvMonitor.Rows[20].Cells[1].Value = StateData.RunState_7;
            
            dgvMonitor.Rows[21].Cells[1].Value = StateData.Voltage_8;
            dgvMonitor.Rows[22].Cells[1].Value = StateData.Current_8;
            dgvMonitor.Rows[23].Cells[1].Value = StateData.RunState_8;

            dgvMonitor.Rows[24].Cells[1].Value = StateData.Voltage_9;
            dgvMonitor.Rows[25].Cells[1].Value = StateData.Current_9;
            dgvMonitor.Rows[26].Cells[1].Value = StateData.RunState_9;

            dgvMonitor.Rows[27].Cells[1].Value = StateData.Voltage_10;
            dgvMonitor.Rows[28].Cells[1].Value = StateData.Current_10;
            dgvMonitor.Rows[29].Cells[1].Value = StateData.RunState_10;

            dgvMonitor.Rows[30].Cells[1].Value = StateData.Voltage_11;
            dgvMonitor.Rows[31].Cells[1].Value = StateData.Current_11;
            dgvMonitor.Rows[32].Cells[1].Value = StateData.RunState_11;

            dgvMonitor.Rows[33].Cells[1].Value = StateData.Voltage_12;
            dgvMonitor.Rows[34].Cells[1].Value = StateData.Current_12;
            dgvMonitor.Rows[35].Cells[1].Value = StateData.RunState_12;

            dgvMonitor.Rows[36].Cells[1].Value = StateData.Voltage_13;
            dgvMonitor.Rows[37].Cells[1].Value = StateData.Current_13;
            dgvMonitor.Rows[38].Cells[1].Value = StateData.RunState_13;

            dgvMonitor.Rows[39].Cells[1].Value = StateData.Voltage_14;
            dgvMonitor.Rows[40].Cells[1].Value = StateData.Current_14;
            dgvMonitor.Rows[41].Cells[1].Value = StateData.RunState_14;

            dgvMonitor.Rows[42].Cells[1].Value = StateData.Voltage_15;
            dgvMonitor.Rows[43].Cells[1].Value = StateData.Current_15;
            dgvMonitor.Rows[44].Cells[1].Value = StateData.RunState_15;

            dgvMonitor.Rows[45].Cells[1].Value = StateData.Voltage_16;
            dgvMonitor.Rows[46].Cells[1].Value = StateData.Current_16;
            dgvMonitor.Rows[47].Cells[1].Value = StateData.RunState_16;



            if (!AllEquipStateData.DicLoopFeedbackLoad_StateData.ContainsKey(StateData.ChargerID))
            {
                AllEquipStateData.DicLoopFeedbackLoad_StateData.Add(StateData.ChargerID, StateData);
            }
            else
            {
                AllEquipStateData.DicLoopFeedbackLoad_StateData[StateData.ChargerID] = StateData;
            }
        }
        private void SetMultiMeter_StateData(object monitorData)
        {
            MultiMeter_StateData StateData = monitorData as MultiMeter_StateData;

            if (dgvMonitor.Rows.Count == 0)
            {
                dgvMonitor.Rows.Add(1);
                dgvMonitor.Rows[0].Cells[0].Value = LanguageManager.GetByKey("输出电压(V)");
            }
            dgvMonitor.Rows[0].Cells[1].Value = StateData.OutPutVoltage;





            if (!AllEquipStateData.MultiMeter_StateData.ContainsKey(StateData.ChargerID))
            {
                AllEquipStateData.MultiMeter_StateData.Add(StateData.ChargerID, StateData);
            }
            else
            {
                AllEquipStateData.MultiMeter_StateData[StateData.ChargerID] = StateData;
            }
        }



        private void SetAuxiliaryLoadCtrl_StateData(object monitorData)
        {
            AuxiliaryLoadCtrl_StateData StateData = monitorData as AuxiliaryLoadCtrl_StateData;

            if (dgvMonitor.Rows.Count == 0)
            {
                dgvMonitor.Rows.Add(5);
                dgvMonitor.Rows[0].Cells[0].Value = LanguageManager.GetByKey("12V过压状态");
                dgvMonitor.Rows[1].Cells[0].Value = LanguageManager.GetByKey("24V过压状态");
                dgvMonitor.Rows[2].Cells[0].Value = LanguageManager.GetByKey("辅源短路状态");
                dgvMonitor.Rows[3].Cells[0].Value = "12V" + LanguageManager.GetByKey("辅源电流(A)");
                dgvMonitor.Rows[4].Cells[0].Value = "24V" + LanguageManager.GetByKey("辅源电流(A)");
            }
            dgvMonitor.Rows[0].Cells[1].Value = StateData.VoltOver_12V;
            dgvMonitor.Rows[1].Cells[1].Value = StateData.VoltOver_24V;
            dgvMonitor.Rows[2].Cells[1].Value = StateData.ShortCircuite;
            dgvMonitor.Rows[3].Cells[1].Value = StateData.AuxiCurrent_12V;
            dgvMonitor.Rows[4].Cells[1].Value = StateData.AuxiCurrent_24V;


            if (!AllEquipStateData.DicAuxiliaryLoadCtrl_StateData.ContainsKey(StateData.ChargerID))
            {
                AllEquipStateData.DicAuxiliaryLoadCtrl_StateData.Add(StateData.ChargerID, StateData);
            }
            else
            {
                AllEquipStateData.DicAuxiliaryLoadCtrl_StateData[StateData.ChargerID] = StateData;
            }
        }

        private void SetChargerNTGXCtrl_StateData(object monitorData)
        {
            Charger_NTGX_StateData StateData = monitorData as Charger_NTGX_StateData;

            if (dgvMonitor.Rows.Count == 0)
            {
                dgvMonitor.Rows.Add(5);
                dgvMonitor.Rows[0].Cells[0].Value = LanguageManager.GetByKey("电压") + "(V)";
                dgvMonitor.Rows[1].Cells[0].Value = LanguageManager.GetByKey("电流") + "(A)";
                dgvMonitor.Rows[2].Cells[0].Value = LanguageManager.GetByKey("系统状态") ;
            }
            dgvMonitor.Rows[0].Cells[1].Value = StateData.ChargingVoltage;
            dgvMonitor.Rows[1].Cells[1].Value = StateData.ChargingCurrent;
            dgvMonitor.Rows[2].Cells[1].Value = StateData.ChargingState;


            if (!AllEquipStateData.DicChargerNTGXCtrl_StateData.ContainsKey(StateData.ChargerID))
            {
                AllEquipStateData.DicChargerNTGXCtrl_StateData.Add(StateData.ChargerID, StateData);
            }
            else
            {
                AllEquipStateData.DicChargerNTGXCtrl_StateData[StateData.ChargerID] = StateData;
            }
        }

        /// <summary>
        /// 初始化控件
        /// </summary>
        /// <param name="monitorData"></param>
        private void IniControl(object monitorData)
        {
            try
            {
                //注：此处在程序初始化的时候 ，入参无法转化成StateDataBase，所以会被捕捉到异常
                //直接忽略异常即可
                StateDataBase data = (StateDataBase)monitorData;
                ChargerID = data.ChargerID;
                EquipName = data.EquipName;
                gpbEquipName.Text = LanguageManager.GetByKey(EquipName) + ChargerID.ToString() + LanguageManager.GetByKey("号枪");
                gpbEquipName.Padding = new Padding(5, 20, 5, 3);
            }
            catch (Exception)
            {
                //重要：此处忽略异常
                // Log.Log.LogException(ex);
            }
        }
    }
}
