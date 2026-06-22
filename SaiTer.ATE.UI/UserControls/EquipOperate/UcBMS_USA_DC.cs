using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaiTer.ATE.UI.UserControls.EquipOperate
{
    /// <summary>
    /// 美标标导引参数读取和设置
    /// </summary>
    public partial class UcBMS_USA_DC : UcEquipOperateBase
    {
        #region 参数读取/设置
        /////////////////////充电参数1读取////////////////////////
        //0   1   PLC状态
        //1   1   EVCC-SECC SDP 连接状态
        //2   4   EVSE状态
        //6   1   由EVSE完成收费请求
        //7   1   EVSE进程状态

        //8   2   EVSE隔离状态
        //10  4   充电器连接器类型支持EVSE
        //14  2   EVSE给EV的通知

        //16  1   供电状态
        //17  1   SECC (GQ模块)收费完成请求
        //18  3   CP状态
        //21  1   S2开关状态
        //22  2   PD状态

        //24  7   PWM占空比
        //31  1   锁状态

        //32  8   平均衰减增益
        //40  8   PLC错误码
        //48  8   EVSE-EV充电步骤
        //56  8   进行EVSE通知的时间限制
        string[] EuropeAmericaDCAParametersS0Name = { "PLC状态", "EVCC-SECC SDP 连接状态", "EVSE状态", "由EVSE完成收费请求", "EVSE进程状态",
                "EVSE隔离状态","充电器连接器类型支持EVSE", "EVSE给EV的通知",
                "供电状态", "SECC (GQ模块)收费完成请求", "CP状态", "S2开关状态", "PD状态",
                "PWM占空比","锁状态",
                "平均衰减增益","PLC错误码","EVSE-EV充电步骤","进行EVSE通知的时间限制"
                };
        double[] EuropeAmericaDCAParametersS0Value = { 0, 0, 0, 0, 0,
                0,0, 0,
                0, 0, 0, 0, 0,
                0, 0,
                0, 0, 0, 0
                };
        /////////////////////充电参数1设置/读取////////////////////////
        //1   电动汽车已经准备好进行能量转移
        //4   电动汽车开始
        //1   接触器状态
        //1   接触器接通后充电启动和停止
        //1   充电完成标志

        //1   充满完成标志
        //1   充电完成标志
        //1   连接检测请求
        //4   EV支持的充电口类型
        //1   S2打开/关闭请求

        //1   入口锁请求
        //2   AAG 值匹配状态

        //8   RESS SoC值
        //16  BMS最大电流值
        //16  BMS最大电压值
        string[] EuropeAmericaDCAParametersS1Name = { "电动汽车已经准备好进行能量转移", "电动汽车开始", "接触器状态", "接触器接通后充电启动和停止", "充电完成标志",
                "充满完成标志","充电完成标志", "连接检测请求", "EV支持的充电口类型", "S2打开/关闭请求",
                "入口锁请求", "AAG 值匹配状态", "RESS SoC值(%)", "BMS最大电流值(A)", "BMS最大电压值(V)"};
        double[] EuropeAmericaDCAParametersS1Value = { 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0,
                0, 0, 0.5, 0.0, 0.0 };
        /////////////////////充电参数2读取////////////////////////
        //0   16  EVSE最大输出电压
        //16  16  EVSE最大输出电流
        //32  16  EVSE当前输出电压
        //48  16  EVSE当前输出电流
        string[] EuropeAmericaDCAParametersS2Name = { "EVSE最大输出电压", "EVSE最大输出电流", "EVSE当前输出电压", "EVSE当前输出电流" };
        double[] EuropeAmericaDCAParametersS2Value = { 0, 0, 0, 0 };
        ///////////////充电参数2设置/读取////////////////
        //0   8   Full SoC值
        //8   8   Bulk SoC值
        //16  16  EV目标需求电压
        //32  16  EV目标需求电流
        //48  16  预充充电电压
        string[] EuropeAmericaDCAParametersS3Name = { "Full SoC值(%)", "Bulk SoC值(%)", "EV目标需求电压(V)", "EV目标需求电流(A)", "预充充电电压(V)" };
        double[] EuropeAmericaDCAParametersS3Value = { 0.5, 0.5, 500.1, 20, 500.5 };
        /////////////////////充电参数3读取////////////////////////
        //0   16  EVSE的最小电压
        //16  16  EVSE的最小电流
        //32  16  EVSE的最大功率
        //48  2   锁状态
        //50  2   状态锁定报警
        string[] EuropeAmericaDCAParametersS4Name = { "EVSE的最小电压", "EVSE的最小电流", "EVSE的最大功率", "锁状态", "状态锁定报警" };
        double[] EuropeAmericaDCAParametersS4Value = { 0, 0, 0, 0, 0 };
        ///////////////充电参数3设置/读取////////////
        //0   16  剩余时间到满SoC
        //16  16  剩余时间到Bulk SoC
        string[] EuropeAmericaDCAParametersS5Name = { "剩余时间到满SoC(S)", "剩余时间到Bulk SoC(S)" };
        double[] EuropeAmericaDCAParametersS5Value = { 1, 0 };

        private string ToDescription(string name, double value)
        {
            string description = "";
            switch (name)
            {
                case "PLC状态":
                    description = value == 0 ? "没有运行" : "运行";
                    return description;
                case "EVCC-SECC SDP 连接状态":
                    description = value == 0 ? "关闭" : "打开";
                    return description;
                case "由EVSE完成收费请求":
                case "EVSE进程状态":
                    description = value == 0 ? "完成" : "正在进行";
                    return description;
                case "EVSE隔离状态":
                    if (value == 0)
                        description = "无效";
                    else if (value == 1)
                        description = "有效";
                    else if (value == 2)
                        description = "警告";
                    else if (value == 3)
                        description = "故障";
                    return description;
                case "EVSE给EV的通知":
                    if (value == 0)
                        description = "无";
                    else if (value == 1)
                        description = "停止计费";
                    else if (value == 2)
                        description = "重新协商";
                    return description;
                case "供电状态":
                    description = value == 0 ? "完成" : "停止";
                    return description;
                case "SECC (GQ模块)收费完成请求":
                    description = value == 0 ? "完成" : "未完成";
                    return description;
                case "CP状态":
                    if (value == 0)
                        description = "未拔枪(A)";
                    else if (value == 1)
                        description = "B1";
                    else if (value == 2)
                        description = "B2";
                    else if (value == 3)
                        description = "C1";
                    else if (value == 3)
                        description = "C2";
                    return description;
                case "S2开关状态":
                case "S2打开/关闭请求":
                    description = value == 0 ? "打开" : "闭合";
                    return description;
                case "PD状态":
                    description = value == 0 ? "未插入" : "插入";
                    return description;
                case "锁状态":
                    description = value == 0 ? "解锁" : "锁定";
                    return description;
                case "电动汽车已经准备好进行能量转移":
                    description = value == 0 ? "S2打开" : "S2关闭&&接触器打开";
                    return description;
                case "电动汽车开始":
                    if (value == 0)
                        description = "未准备好";
                    else if (value == 1)
                        description = "没有错误";
                    else if (value == 2)
                        description = "EvStatCode";
                    return description;
                case "接触器状态":
                    description = value == 0 ? "开" : "关";
                    return description;
                case "接触器接通后充电启动和停止":
                    description = value == 0 ? "停止" : "启动";
                    return description;
                case "充电完成标志":
                    description = value == 0 ? "未完成" : "完成";
                    return description;
                case "充满完成标志":
                    description = value == 0 ? "未满" : "已满";
                    return description;
                case "状态锁定报警":
                    if (value == 0)
                        description = "不报警";
                    else if (value == 1)
                        description = "锁报警";
                    else if (value == 2)
                        description = "解锁失败报警";
                    else if (value == 3)
                        description = "保留";
                    return description;
                default:
                    return value.ToString();
            }
        }
        #endregion

        public UcBMS_USA_DC()
        {
            InitializeComponent();
        }

        private void UcBMS_USA_DC_Load(object sender, EventArgs e)
        {
            GetChargerID();
            LoadParm();
        }

        private void LoadParm()
        {
            //参数1读取
            for (int i = 0; i < EuropeAmericaDCAParametersS0Name.Length; i++)
            {
                dgvParam1.AddRow(EuropeAmericaDCAParametersS0Name[i], ToDescription(EuropeAmericaDCAParametersS0Name[i], EuropeAmericaDCAParametersS0Value[i]));
                //dgvParam1.AddRow(1);
                //dgvParam1.Rows[i].Cells[0].Value = EuropeAmericaDCAParametersS0Name[i];
                //dgvParam1.Rows[i].Cells[1].Value = ToDescription(EuropeAmericaDCAParametersS0Name[i], EuropeAmericaDCAParametersS0Value[i]);
            }
            //参数1读取/设置
            for (int i = 0; i < EuropeAmericaDCAParametersS1Name.Length; i++)
            {
                dgvParamSet1.AddRow(EuropeAmericaDCAParametersS1Name[i], EuropeAmericaDCAParametersS1Value[i]);
            }
            //参数2读取
            for (int i = 0; i < EuropeAmericaDCAParametersS2Name.Length; i++)
            {
                dgvParam2.AddRow(EuropeAmericaDCAParametersS2Name[i], ToDescription(EuropeAmericaDCAParametersS2Name[i], EuropeAmericaDCAParametersS2Value[i]));
            }
            //参数2读取/设置
            for (int i = 0; i < EuropeAmericaDCAParametersS3Name.Length; i++)
            {
                dgvParamSet2.AddRow(EuropeAmericaDCAParametersS3Name[i], EuropeAmericaDCAParametersS3Value[i]);
            }
            //参数3读取
            for (int i = 0; i < EuropeAmericaDCAParametersS4Name.Length; i++)
            {
                dgvParam3.AddRow(EuropeAmericaDCAParametersS4Name[i], ToDescription(EuropeAmericaDCAParametersS4Name[i], EuropeAmericaDCAParametersS4Value[i]));
            }
            //参数3读取/设置
            for (int i = 0; i < EuropeAmericaDCAParametersS5Name.Length; i++)
            {
                dgvParamSet3.AddRow(EuropeAmericaDCAParametersS5Name[i], EuropeAmericaDCAParametersS5Value[i]);
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            EquipMentControl.BMS.BMS_ON(lstChargerID, new string[] { "emtBMS_USA_DC" });
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            EquipMentControl.BMS.BMS_OFF(lstChargerID, new string[] { "emtBMS_USA_DC" });
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            RefreshDgv(dgvParam1, EuropeAmericaDCAParametersS0Name, true, 0x90);
            RefreshDgv(dgvParam2, EuropeAmericaDCAParametersS2Name, true, 0x91);
            RefreshDgv(dgvParam3, EuropeAmericaDCAParametersS4Name, true, 0x92);
            RefreshDgv(dgvParamSet1, null, false, 0x97);
            RefreshDgv(dgvParamSet2, null, false, 0x98);
            RefreshDgv(dgvParamSet3, null, false, 0x99);
        }
        private void RefreshDgv(UIDataGridView dgv, string[] names, bool isRead, byte tComm)
        {
            try
            {
                var dic = EquipMentControl.BMS.BMSGetParameter_EU_DC(lstChargerID, tComm, new string[] { "emtBMS_USA_DC" });
                foreach (int CID in lstChargerID)
                {
                    var data = dic[CID];
                    if (data != null && data.Count == dgv.Rows.Count)
                    {
                        for (int i = 0; i < dgv.Rows.Count; i++)
                        {
                            if (isRead)
                                dgv.Rows[i].Cells[1].Value = ToDescription(names[i], data[i]);
                            else
                                dgv.Rows[i].Cells[1].Value = data[i];
                        }
                    }
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        private void btnSet1_Click(object sender, EventArgs e)
        {
            if (dgvParamSet1.Rows.Count == 15)
            {
                List<int> para = new List<int>();
                for (int i = 0; i < dgvParamSet1.Rows.Count - 3; i++)
                {
                    para.Add(Convert.ToInt32(dgvParamSet1.Rows[i].Cells[1].Value));
                }
                double RESS_SoC = Convert.ToDouble(dgvParamSet1.Rows[dgvParamSet1.Rows.Count - 3].Cells[1].Value);
                double MaxCurrent = Convert.ToDouble(dgvParamSet1.Rows[dgvParamSet1.Rows.Count - 2].Cells[1].Value);
                double MaxVoltage = Convert.ToDouble(dgvParamSet1.Rows[dgvParamSet1.Rows.Count - 1].Cells[1].Value);
                EquipMentControl.BMS.BMSSetPara1_EU_DC(lstChargerID, para, RESS_SoC, MaxCurrent, MaxVoltage, new string[] { "emtBMS_USA_DC" });
            }
        }

        private void btnSet2_Click(object sender, EventArgs e)
        {
            if (dgvParamSet2.Rows.Count == 5)
            {
                double FullSOC = Convert.ToDouble(dgvParamSet2.Rows[0].Cells[1].Value);
                double BulkSOC = Convert.ToDouble(dgvParamSet2.Rows[1].Cells[1].Value);
                double TargetVolt = Convert.ToDouble(dgvParamSet2.Rows[2].Cells[1].Value);
                double TargetCurrent = Convert.ToDouble(dgvParamSet2.Rows[3].Cells[1].Value);
                double ReadyVolt = Convert.ToDouble(dgvParamSet2.Rows[4].Cells[1].Value);
                EquipMentControl.BMS.BMSSetPara2_EU_DC(lstChargerID, FullSOC, BulkSOC, TargetVolt, TargetCurrent, ReadyVolt, new string[] { "emtBMS_USA_DC" });
            }
        }

        private void btnSet3_Click(object sender, EventArgs e)
        {
            if (dgvParamSet3.Rows.Count == 2)
            {
                double FullSOCRemainTime = Convert.ToDouble(dgvParamSet3.Rows[0].Cells[1].Value);
                double BulkSOCRemainTime = Convert.ToDouble(dgvParamSet3.Rows[1].Cells[1].Value);
                EquipMentControl.BMS.BMSSetPara3_EU_DC(lstChargerID, FullSOCRemainTime, BulkSOCRemainTime, new string[] { "emtBMS_USA_DC" });
            }
        }
    }
}
