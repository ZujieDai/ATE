using SaiTer.ATE.Controls.WaveRecorder;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.EquipMent;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace SaiTer.ATE.Controls
{
    /// <summary>
    /// 设备控制管理
    /// </summary>
    public class ControlsListManager
    {
        private BMSBase _BMS = null;
        /// <summary>
        /// BMS控制
        /// </summary>
        public BMSBase BMS
        {
            get { return _BMS; }
        }


        private SafetyBase _Safety = null;
        /// <summary>
        /// 安规控制
        /// </summary>
        public SafetyBase Safety
        {
            get { return _Safety; }
        }


        private OscilloscopeBase _Oscilloscope = null;
        /// <summary>
        /// 示波器控制
        /// </summary>
        public OscilloscopeBase Oscilloscope
        {
            get { return _Oscilloscope; }
        }

        private ACSourceBase _ACSource = null;
        /// <summary>
        /// 交流源控制
        /// </summary>
        public ACSourceBase ACSource
        {
            get { return _ACSource; }
        }

        private ResistanceLoadBase _ResistanceLoad = null;
        /// <summary>
        /// 电阻负载控制
        /// </summary>
        public ResistanceLoadBase ResistanceLoad
        {
            get { return _ResistanceLoad; }
        }

        private ControlBoardBase _ControlBoard = null;
        /// <summary>
        /// 程控板控制
        /// </summary>
        public ControlBoardBase ControlBoard
        {
            get { return _ControlBoard; }
        }

        private LeakageCurrentBase _LeakageCurrent = null;
        /// <summary>
        /// 漏电流测试仪控制
        /// </summary>
        public LeakageCurrentBase LeakageCurrent
        {
            get { return _LeakageCurrent; }
        }
        private PowerAnalyzerBase _PowerAnalyzer = null;
        /// <summary>
        /// 功率分析仪控制
        /// </summary>
        public PowerAnalyzerBase PowerAnalyzer
        {
            get { return _PowerAnalyzer; }
        }

        private ElectronicLoadBase _ElectronicLoad = null;
        /// <summary>
        /// 电子负载控制
        /// </summary>
        public ElectronicLoadBase ElectronicLoad
        {
            get { return _ElectronicLoad; }
        }

        private FeedbackLoadBase _FeedbackLoad = null;
        /// <summary>
        /// 回馈负载控制
        /// </summary>
        public FeedbackLoadBase FeedbackLoad
        {
            get { return _FeedbackLoad; }
        }

        private LoopFeedbackLoadBase _LoopFeedbackLoad = null;
        /// <summary>
        /// 手拉手环式回馈负载控制
        /// </summary>
        public LoopFeedbackLoadBase LoopFeedbackLoad
        {
            get { return _LoopFeedbackLoad; }
        }

        private LoopFeedbackLoadBase _StarLoopFeedbackLoad = null;
        /// <summary>
        /// 手拉手环式回馈负载控制
        /// </summary>
        public LoopFeedbackLoadBase StarLoopFeedbackLoad
        {
            get { return _StarLoopFeedbackLoad; }
        }

        private ElectricMeterBase _ElectricMeter = null;
        /// <summary>
        /// 电表控制
        /// </summary>
        public ElectricMeterBase ElectricMeter
        {
            get { return _ElectricMeter; }
        }
        private OscillographBase _OscillographBase = null;
        /// <summary>
        /// 录波仪控制
        /// </summary>
        public OscillographBase Oscillograph
        {
            get { return _OscillographBase; }
        }

        private AuxiliaryLoadCtrlBase _AuxiliaryLoadCtrl = null;
        /// <summary>
        /// 辅源负载（程控板）控制
        /// </summary>
        public AuxiliaryLoadCtrlBase AuxiliaryLoadCtrl
        {
            get { return _AuxiliaryLoadCtrl; }
        }

        private WaveRecoderBase _WaveRecoderCtrl = null;
        /// <summary>
        /// 辅源负载（程控板）控制
        /// </summary>
        public WaveRecoderBase WaveRecoderCtrl
        {
            get { return _WaveRecoderCtrl; }
        }

        private ChargerBase _ChargerCtrl = null;
        /// <summary>
        /// 桩模拟器控制
        /// </summary>
        public ChargerBase ChargerCtrl
        {
            get { return _ChargerCtrl; }
        }
        /// <summary>
        /// 设置设备控制信息
        /// </summary>
        /// <param name="EquipMentName">设备名</param>
        /// <param name="DitEquipMentBase">设备编号-设备  字典</param>
        /// <returns></returns>
        public bool SetControls(string EquipMentName, Dictionary<int, EquipMentBase> DitEquipMentBase)
        {
            if (SetBMSControl(EquipMentName))
            {
                _BMS.DitEquipMentBase = DitEquipMentBase;
                return true;
            }

            else if (SetSafety(EquipMentName))
            {
                _Safety.DitEquipMentBase = DitEquipMentBase;
                return true;
            }
            else if (SetOscilloscope(EquipMentName))
            {
                _Oscilloscope.DitEquipMentBase = DitEquipMentBase;
                return true;
            }
            else if (SetACSource(EquipMentName))
            {
                _ACSource.DitEquipMentBase = DitEquipMentBase;
                return true;
            }
            else if (SetResistanceLoad(EquipMentName))
            {
                _ResistanceLoad.DitEquipMentBase = DitEquipMentBase;
                return true;
            }
            else if (SetControlBoard(EquipMentName))
            {
                _ControlBoard.DitEquipMentBase = DitEquipMentBase;
                return true;
            }
            else if (SetLeakageCurrent(EquipMentName))
            {
                _LeakageCurrent.DitEquipMentBase = DitEquipMentBase;
                return true;
            }
            else if (SetPowerAnalyzer(EquipMentName))
            {
                _PowerAnalyzer.DitEquipMentBase = DitEquipMentBase;
                return true;
            }
            else if (SetElectronicLoad(EquipMentName))
            {
                _ElectronicLoad.DitEquipMentBase = DitEquipMentBase;
                return true;
            }
            else if (SetFeedbackLoad(EquipMentName))
            {
                _FeedbackLoad.DitEquipMentBase = DitEquipMentBase;
                return true;
            }
            else if (SetFeedbackLoadAC(EquipMentName))
            {
                _FeedbackLoad.DitEquipMentBase = DitEquipMentBase;
                return true;
            }
            else if (SetStarLoopFeedbackLoad(EquipMentName))
            {
                _StarLoopFeedbackLoad.DitEquipMentBase = DitEquipMentBase;
                return true;
            }
            else if (SetLoopFeedbackLoad(EquipMentName))
            {
                _LoopFeedbackLoad.DitEquipMentBase = DitEquipMentBase;
                return true;
            }
            else if (SetElectricMeter(EquipMentName))
            {
                _ElectricMeter.DitEquipMentBase = DitEquipMentBase;
                return true;
            }
            else if (SetOscillograph(EquipMentName))
            {
                _OscillographBase.DitEquipMentBase = DitEquipMentBase;
                return true;
            }
            else if (SetAuxiliaryLoadCtrl(EquipMentName))
            {
                _AuxiliaryLoadCtrl.DitEquipMentBase = DitEquipMentBase;
                return true;
            }
            else if(SetWaveRecoder(EquipMentName))
            {
                _WaveRecoderCtrl.DitEquipMentBase = DitEquipMentBase;
                return true;
            }
            else if (SetChargerNTGX(EquipMentName))
            {
                _ChargerCtrl.DitEquipMentBase = DitEquipMentBase;
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 设置设备控制信息
        /// </summary>
        /// <returns></returns>
        public void SetChargerInfo(List<ChargerInfoModel> chargerInfos)
        {
            if (_BMS != null)
            {
                _BMS.lstChargerInfo = chargerInfos;
            }

            if (_Safety != null)
            {
                _Safety.lstChargerInfo = chargerInfos;
            }
            if (_Oscilloscope != null)
            {
                _Oscilloscope.lstChargerInfo = chargerInfos;
            }
            if (_ACSource != null)
            {
                _ACSource.lstChargerInfo = chargerInfos;
            }
            if (_ResistanceLoad != null)
            {
                _ResistanceLoad.lstChargerInfo = chargerInfos;
            }
            if (_ControlBoard != null)
            {
                _ControlBoard.lstChargerInfo = chargerInfos;
            }
            if (_LeakageCurrent != null)
            {
                _LeakageCurrent.lstChargerInfo = chargerInfos;
            }
            if (_PowerAnalyzer != null)
            {
                _PowerAnalyzer.lstChargerInfo = chargerInfos;
            }
            if (_ElectronicLoad != null)
            {
                _ElectronicLoad.lstChargerInfo = chargerInfos;
            }
            if (_FeedbackLoad != null)
            {
                _FeedbackLoad.lstChargerInfo = chargerInfos;
            }
            if (_ElectricMeter != null)
            {
                _ElectricMeter.lstChargerInfo = chargerInfos;
            }
            if (_OscillographBase != null)
            {
                _OscillographBase.lstChargerInfo = chargerInfos;
            }
            if (_AuxiliaryLoadCtrl != null)
            {
                _AuxiliaryLoadCtrl.lstChargerInfo = chargerInfos;
            }
            if (_WaveRecoderCtrl != null)
            {
                _WaveRecoderCtrl.lstChargerInfo = chargerInfos;
            }
            if (_ChargerCtrl != null)
            {
                _ChargerCtrl.lstChargerInfo = chargerInfos;
            }
        }
        /// <summary>
        /// 重新设置设备控制信息
        /// </summary>
        /// <returns></returns>
        public void UpdateChargerInfo(List<ChargerInfoModel> chargerInfos)
        {
            if (_BMS != null)
            {
                _BMS.lstChargerInfo = chargerInfos;
            }

            if (_Safety != null)
            {
                _Safety.lstChargerInfo = chargerInfos;
            }
            if (_Oscilloscope != null)
            {
                _Oscilloscope.lstChargerInfo = chargerInfos;
            }
            if (_ACSource != null)
            {
                _ACSource.lstChargerInfo = chargerInfos;
            }
            if (_ResistanceLoad != null)
            {
                _ResistanceLoad.lstChargerInfo = chargerInfos;
            }
            if (_ControlBoard != null)
            {
                _ControlBoard.lstChargerInfo = chargerInfos;
            }
            if (_LeakageCurrent != null)
            {
                _LeakageCurrent.lstChargerInfo = chargerInfos;
            }
            if (_PowerAnalyzer != null)
            {
                _PowerAnalyzer.lstChargerInfo = chargerInfos;
            }
            if (_ElectronicLoad != null)
            {
                _ElectronicLoad.lstChargerInfo = chargerInfos;
            }
            if (_FeedbackLoad != null)
            {
                _FeedbackLoad.lstChargerInfo = chargerInfos;
            }
            if (_ElectricMeter != null)
            {
                _ElectricMeter.lstChargerInfo = chargerInfos;
            }
            if (_OscillographBase != null)
            {
                _OscillographBase.lstChargerInfo = chargerInfos;
            }
            if (_AuxiliaryLoadCtrl != null)
            {
                _AuxiliaryLoadCtrl.lstChargerInfo = chargerInfos;
            }
            if (_WaveRecoderCtrl != null)
            {
                _WaveRecoderCtrl.lstChargerInfo = chargerInfos;
            }
            if (_ChargerCtrl != null)
            {
                _ChargerCtrl.lstChargerInfo = chargerInfos;
            }
        }
        /// <summary>
        /// 实例化录波仪初始化控制
        /// </summary>
        /// <param name="equipMentName"></param>
        /// <returns></returns>
        private bool SetOscillograph(string EquipMentName)
        {
            if (EquipMentName.Contains("录波仪"))
            {
                _OscillographBase = new OscillographControl();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 实例化BMS控制
        /// </summary>
        /// <param name="EquipMentName">设备名称</param>
        /// <returns></returns>
        private bool SetBMSControl(string EquipMentName)
        {
            if (EquipMentName.Contains("BMS"))
            {
                _BMS = new BMSControl();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 实例化安规控制
        /// </summary>
        /// <param name="EquipMentName">设备名称</param>
        /// <returns></returns>
        private bool SetSafety(string EquipMentName)
        {
            if (EquipMentName.Contains("安规"))
            {
                _Safety = new SafetyControl();
                return true;
            }
            return false;
        }
        private bool SetOscilloscope(string EquipMentName)
        {
            if (EquipMentName.Contains("示波器"))
            {
                _Oscilloscope = new OscilloscopeControl();
                return true;
            }
            return false;
        }
        private bool SetACSource(string EquipMentName)
        {
            if (EquipMentName.Contains("交流源"))
            {
                _ACSource = new ACSourceControl();
                return true;
            }
            return false;
        }

        //
        private bool SetResistanceLoad(string EquipMentName)
        {
            if (EquipMentName.Contains("电阻负载"))
            {
                _ResistanceLoad = new ResistanceLoadControl();
                return true;
            }
            return false;
        }

        private bool SetControlBoard(string EquipMentName)
        {
            if (EquipMentName.Contains("程控板"))
            {
                _ControlBoard = new ControlBoardControl();
                return true;
            }
            return false;
        }
        private bool SetLeakageCurrent(string EquipMentName)
        {
            if (EquipMentName.Contains("剩余电流保护测试仪"))
            {
                _LeakageCurrent = new LeakageCurrentControl();
                return true;
            }
            return false;
        }

        private bool SetPowerAnalyzer(string EquipMentName)
        {
            if (EquipMentName.Contains("功率分析仪"))
            {
                _PowerAnalyzer = new PowerAnalyzerControl();
                return true;
            }
            return false;
        }
        private bool SetElectronicLoad(string EquipMentName)
        {
            if (EquipMentName.Contains("电子负载"))
            {
                _ElectronicLoad = new ElectronicLoadControl();
                return true;
            }
            return false;
        }

        private bool SetFeedbackLoad(string EquipMentName)
        {
            if (EquipMentName.Equals("回馈负载"))
            {
                _FeedbackLoad = new FeedbackLoadControl();
                return true;
            }
            return false;
        }

        private bool SetFeedbackLoadAC(string EquipMentName)
        {
            if (EquipMentName.Equals("交流回馈负载"))
            {
                _FeedbackLoad = new FeedbackLoadACControl();
                return true;
            }
            return false;
        }

        private bool SetLoopFeedbackLoad(string EquipMentName)
        {
            if (EquipMentName.Contains("Loop回馈负载"))
            {
                _LoopFeedbackLoad = new LoopFeedbackLoadControl();
                return true;
            }
            return false;
        }
        private bool SetStarLoopFeedbackLoad(string EquipMentName)
        {
            if (EquipMentName.Contains("Star回馈负载"))
            {
                _StarLoopFeedbackLoad = new StarLoopFeedbackLoadControl();
                return true;
            }
            return false;
        }

        private bool SetElectricMeter(string EquipMentName)
        {
            if (EquipMentName.Contains("电表"))
            {
                _ElectricMeter = new ElectricMeterControl();
                return true;
            }
            return false;
        }

        private bool SetAuxiliaryLoadCtrl(string EquipMentName)
        {
            if (EquipMentName.Contains("辅源负载"))
            {
                _AuxiliaryLoadCtrl = new AuxiliaryLoadCtrlControl();
                return true;
            }
            return false;
        }

        private bool SetWaveRecoder(string EquipMentName)
        {
            if (EquipMentName.Contains("录波板"))
            {
                _WaveRecoderCtrl = new WaveRecoderControl();
                return true;
            }
            return false;
        }

        private bool SetChargerNTGX(string EquipMentName)
        {
            if (EquipMentName.Contains("充电桩模拟器"))
            {
                _ChargerCtrl = new ChargerControl();
                return true;
            }
            return false;
        }
    }
}
