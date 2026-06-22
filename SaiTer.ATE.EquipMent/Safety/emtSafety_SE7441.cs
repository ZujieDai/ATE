using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.DataBaseModel;
using SaiTer.ATE.IDAL.SQLiteIDAL;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 设备-安规
    /// </summary>
    public class emtSafety_SE7441 : EquipMentBase
    {
        public bool isSafetyInit = false;
        private static object SynLock = new object();
        public emtSafety_SE7441(int type)
        {
            this.AutoReadData = true;
            this.EquipMentName = LanguageManager.GetByKey("安规");

        }

        public override bool SafetyInit(string SchemeID, string SchemeName, bool isSave = false)
        {
            int sleepTime = 100;
            try
            {
                isSafetyInit = true;
                this.AutoReadData = false;
                string res = "";
                // 档案存在检查名字是否正确，ST则认为测试步骤是软件设置的
                if (SafetyReadParam($"FL {SchemeID}", "\n", "\n", ref res))
                {
                    Thread.Sleep(sleepTime);
                    SafetyReadParam("LF?", "\n", "\n", ref res);
                    Thread.Sleep(sleepTime);
                    if (res.Equals($"{SchemeID},{SchemeName}\n") && !isSave)
                        return true;
                    // 不是程序添加的档案会影响测试项测试，查询方案的步骤数，并循环删除
                    SafetyReadParam("ST?", "\n", "\n", ref res);
                    Thread.Sleep(sleepTime);
                    if (int.TryParse(res, out int stepCount))
                    {
                        for (int i = stepCount; i > 0; i--)
                        {
                            SafetySetParam("SD " + i, "\n", "\n");
                            Thread.Sleep(sleepTime);
                        }
                    }
                    //原本的方案不对肯定需要重新保存
                    isSave = true;
                }
                else
                {
                    //新建档案
                    SafetySetParam($"FN {SchemeID},{SchemeName}", "\n", "\n");
                    Thread.Sleep(1000);
                    bool isSuccess = SafetyReadParam($"FL {SchemeID}", "\n", "\n", ref res);
                    Thread.Sleep(sleepTime);
                    //新档案肯定得保存
                    isSave = true;

                    //如果前面有空缺的方案会导致失败
                    if (!isSuccess)
                    {
                        SendMsgToFile(EquipMentName + "新建方案失败");
                        return false;
                    }
                }

                //如果需要保存
                if (isSave)
                {
                    var listConfigs = EquipmentConfigManage.GetEquipConfigs();
                    int sid = listConfigs.FirstOrDefault(s => s.EquipmentName.Equals("Safety_SE7441") && s.ConfigType.Equals("Safety_Scheme") && s.Params1 == SchemeID).ChargerType;
                    EquipmentConfigModel EquipmentConfig = EquipmentConfigManage.GetEquipConfigs()?.Find(s => s.EquipmentName.Equals("Safety_SE7441")
                        && s.ConfigType.Equals("Safety_Params") && s.ChargerType == sid);
                    if (EquipmentConfig != null)
                    {
                        string[] param_IR1 = EquipmentConfig.Params1.Split(';')[0].Split('|');
                        string[] param_IR2 = EquipmentConfig.Params1.Split(';')[1].Split('|');
                        string[] param_IR3 = EquipmentConfig.Params1.Split(';')[2].Split('|');
                        string[] param_ACW1 = EquipmentConfig.Params2.Split(';')[0].Split('|');
                        string[] param_ACW2 = EquipmentConfig.Params2.Split(';')[1].Split('|');
                        string[] param_ACW3 = EquipmentConfig.Params2.Split(';')[2].Split('|');
                        string[] param_DCW1 = EquipmentConfig.Params3.Split(';')[0].Split('|');
                        string[] param_DCW2 = EquipmentConfig.Params3.Split(';')[1].Split('|');
                        string[] param_DCW3 = EquipmentConfig.Params3.Split(';')[2].Split('|');
                        string[] param_GND1 = EquipmentConfig.Remark.Split(';')[0].Split('|');
                        string[] param_GND2 = EquipmentConfig.Remark.Split(';')[1].Split('|');
                        string[] param_GND3 = EquipmentConfig.Remark.Split(';')[2].Split('|');
                        string[] param_GND4 = EquipmentConfig.Remark.Split(';')[3].Split('|');

                        SafetySetParam("SAG", "\n", "\n");    // 新增GND测试步骤
                        Thread.Sleep(500);
                        SafetySetParam("EV " + param_GND4[0].Split('=')[1], "\n", "\n");    // 测试电压（V）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EC " + param_GND4[1].Split('=')[1], "\n", "\n");    // 测试电流（A）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EH " + param_GND4[2].Split('=')[1], "\n", "\n");    // 阻抗上限（mΩ）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EL " + param_GND4[3].Split('=')[1], "\n", "\n");    // 阻抗下限（mΩ）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EDW " + param_GND4[4].Split('=')[1], "\n", "\n");   // 测试时间（s）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EF " + param_GND4[5].Split('=')[1], "\n", "\n");    // 输出频率(Hz)
                        Thread.Sleep(sleepTime);
                        SafetySetParam("ESN 1", "\n", "\n");    // 扫描通道
                        Thread.Sleep(500);

                        SafetySetParam("SAG", "\n", "\n");    // 新增GND测试步骤
                        Thread.Sleep(500);
                        SafetySetParam("EV " + param_GND3[0].Split('=')[1], "\n", "\n");    // 测试电压（V）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EC " + param_GND3[1].Split('=')[1], "\n", "\n");    // 测试电流（A）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EH " + param_GND3[2].Split('=')[1], "\n", "\n");    // 阻抗上限（mΩ）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EL " + param_GND3[3].Split('=')[1], "\n", "\n");    // 阻抗下限（mΩ）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EDW " + param_GND3[4].Split('=')[1], "\n", "\n");   // 测试时间（s）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EF " + param_GND3[5].Split('=')[1], "\n", "\n");    // 输出频率(Hz)
                        Thread.Sleep(sleepTime);
                        SafetySetParam("ESN 4", "\n", "\n");    // 扫描通道
                        Thread.Sleep(500);

                        SafetySetParam("SAG", "\n", "\n");    // 新增GND测试步骤
                        Thread.Sleep(500);
                        SafetySetParam("EV " + param_GND2[0].Split('=')[1], "\n", "\n");    // 测试电压（V）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EC " + param_GND2[1].Split('=')[1], "\n", "\n");    // 测试电流（A）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EH " + param_GND2[2].Split('=')[1], "\n", "\n");    // 阻抗上限（mΩ）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EL " + param_GND2[3].Split('=')[1], "\n", "\n");    // 阻抗下限（mΩ）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EDW " + param_GND2[4].Split('=')[1], "\n", "\n");   // 测试时间（s）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EF " + param_GND2[5].Split('=')[1], "\n", "\n");    // 输出频率(Hz)
                        Thread.Sleep(sleepTime);
                        SafetySetParam("ESN 3", "\n", "\n");    // 扫描通道
                        Thread.Sleep(500);

                        SafetySetParam("SAG", "\n", "\n");    // 新增GND测试步骤
                        Thread.Sleep(500);
                        SafetySetParam("EV " + param_GND1[0].Split('=')[1], "\n", "\n");    // 测试电压（V）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EC " + param_GND1[1].Split('=')[1], "\n", "\n");    // 测试电流（A）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EH " + param_GND1[2].Split('=')[1], "\n", "\n");    // 阻抗上限（mΩ）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EL " + param_GND1[3].Split('=')[1], "\n", "\n");    // 阻抗下限（mΩ）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EDW " + param_GND1[4].Split('=')[1], "\n", "\n");   // 测试时间（s）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EF " + param_GND1[5].Split('=')[1], "\n", "\n");    // 输出频率(Hz)
                        Thread.Sleep(sleepTime);
                        SafetySetParam("ESN 2", "\n", "\n");    // 扫描通道
                        Thread.Sleep(500);
                        //SafetySetParam("FS", "\n", "\n");     // 储存目前档案
                        //Thread.Sleep(sleepTime);

                        SafetySetParam("SAD", "\n", "\n");    // 新增DCW测试步骤
                        Thread.Sleep(500);
                        SafetySetParam("EV " + param_DCW3[0].Split('=')[1], "\n", "\n");    // 测试电压（V）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EH " + param_DCW3[1].Split('=')[1], "\n", "\n");    // 电流上限（uA）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EL " + param_DCW3[2].Split('=')[1], "\n", "\n");    // 电流下限（uA）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EDW " + param_DCW3[3].Split('=')[1], "\n", "\n");   // 测试时间（s）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("ERU " + param_DCW3[4].Split('=')[1], "\n", "\n");   // 缓升时间（s）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("ERD " + param_DCW3[5].Split('=')[1], "\n", "\n");   // 缓降时间（s）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EA " + param_DCW3[6].Split('=')[1], "\n", "\n");    // 电弧灵敏度
                        Thread.Sleep(sleepTime);
                        SafetySetParam("ES OHLOOOOOOOOOOOOOOOOOOOOO", "\n", "\n");          // 扫描器
                        Thread.Sleep(sleepTime);

                        SafetySetParam("SAD", "\n", "\n");    // 新增DCW测试步骤
                        Thread.Sleep(500);
                        SafetySetParam("EV " + param_DCW2[0].Split('=')[1], "\n", "\n");    // 测试电压（V）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EH " + param_DCW2[1].Split('=')[1], "\n", "\n");    // 电流上限（uA）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EL " + param_DCW2[2].Split('=')[1], "\n", "\n");    // 电流下限（uA）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EDW " + param_DCW2[3].Split('=')[1], "\n", "\n");   // 测试时间（s）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("ERU " + param_DCW2[4].Split('=')[1], "\n", "\n");   // 缓升时间（s）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("ERD " + param_DCW2[5].Split('=')[1], "\n", "\n");   // 缓降时间（s）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EA " + param_DCW2[6].Split('=')[1], "\n", "\n");    // 电弧灵敏度
                        Thread.Sleep(sleepTime);
                        SafetySetParam("ES HOLOOOOOOOOOOOOOOOOOOOOO", "\n", "\n");          // 扫描器
                        Thread.Sleep(sleepTime);

                        SafetySetParam("SAD", "\n", "\n");    // 新增DCW测试步骤
                        Thread.Sleep(500);
                        SafetySetParam("EV " + param_DCW1[0].Split('=')[1], "\n", "\n");    // 测试电压（V）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EH " + param_DCW1[1].Split('=')[1], "\n", "\n");    // 电流上限（uA）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EL " + param_DCW1[2].Split('=')[1], "\n", "\n");    // 电流下限（uA）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EDW " + param_DCW1[3].Split('=')[1], "\n", "\n");   // 测试时间（s）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("ERU " + param_DCW1[4].Split('=')[1], "\n", "\n");   // 缓升时间（s）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("ERD " + param_DCW1[5].Split('=')[1], "\n", "\n");   // 缓降时间（s）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EA " + param_DCW1[6].Split('=')[1], "\n", "\n");    // 电弧灵敏度
                        Thread.Sleep(sleepTime);
                        SafetySetParam("ES HLOOOOOOOOOOOOOOOOOOOOOO", "\n", "\n");          // 扫描器
                        Thread.Sleep(sleepTime);
                        //SafetySetParam("FS", "\n", "\n");     // 储存目前档案
                        //Thread.Sleep(sleepTime);

                        SafetySetParam("SAA", "\n", "\n");    // 新增ACW测试步骤
                        Thread.Sleep(500);
                        SafetySetParam("EV " + param_ACW3[0].Split('=')[1], "\n", "\n");    // 测试电压（V）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EHT " + param_ACW3[1].Split('=')[1], "\n", "\n");   // 电流总上限（mA）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("ELT " + param_ACW3[2].Split('=')[1], "\n", "\n");   // 电流总下限（mA）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EDW " + param_ACW3[3].Split('=')[1], "\n", "\n");   // 测试时间（s）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("ERU " + param_ACW3[4].Split('=')[1], "\n", "\n");   // 缓升时间（s）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("ERD " + param_ACW3[5].Split('=')[1], "\n", "\n");   // 缓降时间（s）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EA " + param_ACW3[6].Split('=')[1], "\n", "\n");    // 电弧灵敏度
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EF " + param_ACW3[7].Split('=')[1], "\n", "\n");    // 输出频率（Hz）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("ES OHLOOOOOOOOOOOOOOOOOOOOO", "\n", "\n");          // 扫描器
                        Thread.Sleep(sleepTime);

                        SafetySetParam("SAA", "\n", "\n");    // 新增ACW测试步骤
                        Thread.Sleep(500);
                        SafetySetParam("EV " + param_ACW2[0].Split('=')[1], "\n", "\n");    // 测试电压（V）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EHT " + param_ACW2[1].Split('=')[1], "\n", "\n");   // 电流总上限（mA）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("ELT " + param_ACW2[2].Split('=')[1], "\n", "\n");   // 电流总下限（mA）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EDW " + param_ACW2[3].Split('=')[1], "\n", "\n");   // 测试时间（s）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("ERU " + param_ACW2[4].Split('=')[1], "\n", "\n");   // 缓升时间（s）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("ERD " + param_ACW2[5].Split('=')[1], "\n", "\n");   // 缓降时间（s）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EA " + param_ACW2[6].Split('=')[1], "\n", "\n");    // 电弧灵敏度
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EF " + param_ACW2[7].Split('=')[1], "\n", "\n");    // 输出频率（Hz）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("ES HOLOOOOOOOOOOOOOOOOOOOOO", "\n", "\n");          // 扫描器
                        Thread.Sleep(sleepTime);

                        SafetySetParam("SAA", "\n", "\n");    // 新增ACW测试步骤
                        Thread.Sleep(500);
                        SafetySetParam("EV " + param_ACW1[0].Split('=')[1], "\n", "\n");    // 测试电压（V）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EHT " + param_ACW1[1].Split('=')[1], "\n", "\n");   // 电流总上限（mA）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("ELT " + param_ACW1[2].Split('=')[1], "\n", "\n");   // 电流总下限（mA）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EDW " + param_ACW1[3].Split('=')[1], "\n", "\n");   // 测试时间（s）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("ERU " + param_ACW1[4].Split('=')[1], "\n", "\n");   // 缓升时间（s）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("ERD " + param_ACW1[5].Split('=')[1], "\n", "\n");   // 缓降时间（s）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EA " + param_ACW1[6].Split('=')[1], "\n", "\n");    // 电弧灵敏度
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EF " + param_ACW1[7].Split('=')[1], "\n", "\n");    // 输出频率（Hz）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("ES HLOOOOOOOOOOOOOOOOOOOOOO", "\n", "\n");          // 扫描器
                        Thread.Sleep(sleepTime);
                        //SafetySetParam("FS", "\n", "\n");     // 储存目前档案
                        //Thread.Sleep(sleepTime);

                        SafetySetParam("SAI", "\n", "\n");    // 新增IR测试步骤
                        Thread.Sleep(500);
                        SafetySetParam("EV " + param_IR3[0].Split('=')[1], "\n", "\n");    // 测试电压（V）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EH " + param_IR3[1].Split('=')[1], "\n", "\n");    // 阻抗上限（MΩ）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EL " + param_IR3[2].Split('=')[1], "\n", "\n");    // 阻抗下限（MΩ）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EDW " + param_IR3[3].Split('=')[1], "\n", "\n");   // 测试时间（s）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("ERU " + param_IR3[4].Split('=')[1], "\n", "\n");   // 缓升时间（s）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("ERD " + param_IR3[5].Split('=')[1], "\n", "\n");   // 缓降时间（s）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EDE " + param_IR3[6].Split('=')[1], "\n", "\n");   // 延迟时间（s）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("ES OHLOOOOOOOOOOOOOOOOOOOOO", "\n", "\n");          // 扫描器
                        Thread.Sleep(sleepTime);

                        SafetySetParam("SAI", "\n", "\n");    // 新增IR测试步骤
                        Thread.Sleep(500);
                        SafetySetParam("EV " + param_IR2[0].Split('=')[1], "\n", "\n");    // 测试电压（V）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EH " + param_IR2[1].Split('=')[1], "\n", "\n");    // 阻抗上限（MΩ）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EL " + param_IR2[2].Split('=')[1], "\n", "\n");    // 阻抗下限（MΩ）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EDW " + param_IR2[3].Split('=')[1], "\n", "\n");   // 测试时间（s）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("ERU " + param_IR2[4].Split('=')[1], "\n", "\n");   // 缓升时间（s）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("ERD " + param_IR2[5].Split('=')[1], "\n", "\n");   // 缓降时间（s）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EDE " + param_IR2[6].Split('=')[1], "\n", "\n");   // 延迟时间（s）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("ES HOLOOOOOOOOOOOOOOOOOOOOO", "\n", "\n");          // 扫描器
                        Thread.Sleep(sleepTime);

                        SafetySetParam("SAI", "\n", "\n");    // 新增IR测试步骤
                        Thread.Sleep(500);
                        SafetySetParam("EV " + param_IR1[0].Split('=')[1], "\n", "\n");    // 测试电压（V）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EH " + param_IR1[1].Split('=')[1], "\n", "\n");    // 阻抗上限（MΩ）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EL " + param_IR1[2].Split('=')[1], "\n", "\n");    // 阻抗下限（MΩ）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EDW " + param_IR1[3].Split('=')[1], "\n", "\n");   // 测试时间（s）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("ERU " + param_IR1[4].Split('=')[1], "\n", "\n");   // 缓升时间（s）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("ERD " + param_IR1[5].Split('=')[1], "\n", "\n");   // 缓降时间（s）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("EDE " + param_IR1[6].Split('=')[1], "\n", "\n");   // 延迟时间（s）
                        Thread.Sleep(sleepTime);
                        SafetySetParam("ES HLOOOOOOOOOOOOOOOOOOOOOO", "\n", "\n");          // 扫描器
                        Thread.Sleep(sleepTime);

                        //是否保存成功
                        SafetyReadParam("LF?", "\n", "\n", ref res);
                        Thread.Sleep(sleepTime);
                        if (!res.Equals($"{SchemeID},{SchemeName}\n"))
                            return false;
                        // 指针回到步骤一
                        SafetySetParam("FS", "\n", "\n");     // 储存目前档案
                        Thread.Sleep(1000);
                        SafetySetParam("FL " + SchemeID, "\n", "\n");
                        Thread.Sleep(sleepTime);
                        SafetySetParam("SS 01", "\n", "\n");
                        Thread.Sleep(sleepTime);
                        return true;
                    }
                    else
                    {
                        //没有在设备操作界面保存过数据
                        //SystemEvent.MessageInfo(true, "请在设备操作界面先保存配置后再测试", true);
                        SendMsgToFile(EquipMentName + "数据库没有找到对应的EquipmentConfig");
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                SendExMsg(ex);
                return false;
            }
            finally
            {
                this.AutoReadData = true;
                isSafetyInit = false;
            }
        }

        public override void Safety_OFF()
        {
            try
            {
                this.AutoReadData = false;
                lock (SynLock)
                {
                    SafetySetParam("RESET", "\n", "\n");
                }
            }
            catch (Exception ex)
            {
                SendExMsg(ex);
            }
            finally { this.AutoReadData = true; }
        }


        public override void SafetySetParam(string AsciiSendStr, string AsciiOutEndFlag, string AsciiInEndFlag)
        {
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;
                    byte[] writeBuffer = GetBuffer(AsciiSendStr, AsciiOutEndFlag, AsciiInEndFlag);

                    if (EquipMentPort != null)
                    {
                        //DataBuf.Clear();
                        //RevEquipMentData();
                        string strTemp = BitConverter.ToString(writeBuffer).Replace('-', ' ');
                        // SendMsgToFile("安规发送数据：" + strTemp);
                        EquipMentPort.SendData(writeBuffer);
                    }
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                }
                finally { this.AutoReadData = true; }
            }
        }

        public override bool SafetyReadParam(string AsciiSendStr, string AsciiOutEndFlag, string AsciiInEndFlag, ref string strResult)
        {
            lock (SynLock)
            {
                try
                {
                    this.AutoReadData = false;
                    //RevEquipMentData();//先取出已有的数据扔掉
                    byte[] writeBuffer = GetBuffer(AsciiSendStr, AsciiOutEndFlag, AsciiInEndFlag);

                    if (EquipMentPort != null)
                    {
                        DataBuf.Clear();
                        //RevEquipMentData();
                        string strTemp = BitConverter.ToString(writeBuffer).Replace('-', ' ');
                        SendMsgToFile("安规发送数据：" + strTemp);
                        Thread.Sleep(10);
                        EquipMentPort.SendData(writeBuffer);
                        byte[] RevMsgData = RevEquipMentData();
                        if (RevMsgData != null)
                        {
                            strResult = System.Text.Encoding.ASCII.GetString(RevMsgData.ToArray());
                            SendMsgToFile("安规收到数据：" + strResult);
                            if (RevMsgData.Length >= 2 && RevMsgData[0] == 0x06 && RevMsgData[1] == 0x0A)
                                return true;
                        }

                    }
                    return false;
                }
                catch (Exception ex)
                {
                    SendExMsg(ex);
                    return false;
                }
                finally { this.AutoReadData = true; }
            }
        }


        public override void ReadSafetyStateData()
        {
            try
            {
                SystemEvent.SendConnectState(false, this);
                while (true)
                {
                    if (this.AutoReadData && !isSafetyInit)
                    {
                        byte[] writeBuffer = GetBuffer("*idn?", "\r\n", "\r\n");
                        string strResult = string.Empty;
                        if (EquipMentPort != null)
                        {
                            byte[] RevMsgData = null;
                            lock (SynLock)
                            {
                                //DataBuf.Clear();
                                RevEquipMentData();
                                string strTemp = BitConverter.ToString(writeBuffer).Replace('-', ' ');
                                // SendMsgToFile("安规发送数据：" + strTemp);
                                EquipMentPort.SendData(writeBuffer);
                                RevMsgData = RevEquipMentData();
                            }
                            if (RevMsgData != null)
                            {
                                strResult = System.Text.Encoding.ASCII.GetString(RevMsgData.ToArray());
                                SystemEvent.SendConnectState(true, this);
                            }
                            else
                            {
                                SystemEvent.SendConnectState(false, this);
                            }
                            // SystemEvent.SendMonitorMessage(strResult);
                        }
                        else
                        {
                            SystemEvent.SendConnectState(false, this);
                            //SendMsgToFile("安规通道对象不存在，请检查安规通道");
                        }
                    }
                    Thread.Sleep(300);
                }

            }
            catch (Exception ex)
            {
                SendExMsg(ex);

            }
        }


    }
}
