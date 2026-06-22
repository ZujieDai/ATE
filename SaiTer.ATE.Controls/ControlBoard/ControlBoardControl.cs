using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.EquipMent;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Controls
{
    /// <summary>
    /// 程控板控制
    /// </summary>
    public class ControlBoardControl : ControlBoardBase
    {
        public override void ControlResistanceSetRelay( List<bool> lstConditionState)
        {
            string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
            if (Customer.Equals("TS"))
            {
                //勾选测试国标枪
                if (lstChargerInfo.Find(s => s.IsCheck && s.ChargerType == EmChargerType.Charger_GB_AC) != null)
                    lstConditionState[12] = true;
                else
                    lstConditionState[12] = false;
            }
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent("emtControlBoard"))
            {

                ThreadPool.QueueUserWorkItem(state => { item.Value.ControlResistanceSetRelay(lstConditionState); });

            }
        }

        public override void ControlResistanceSetRelay(List<int> lstIDs, List<bool> lstConditionState)
        {
            string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
            if (Customer.Equals("TS"))
            {
                //勾选测试国标枪
                if (lstChargerInfo.Find(s => s.IsCheck && s.ChargerType == EmChargerType.Charger_GB_AC) != null)
                    lstConditionState[12] = true;
                else
                    lstConditionState[12] = false;
            }
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent("emtControlBoard"))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    ThreadPool.QueueUserWorkItem(state => { item.Value.ControlResistanceSetRelay(lstConditionState); });
                }
            }
        }

        public override void SetLightColor(EmLightColor color)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent("emtControlBoard"))
            {
                ThreadPool.QueueUserWorkItem(state => { item.Value.SetLightColor(color); });
            }
        }

        public override List<bool> ControlBoardReadState()
        {
            List<bool> bools = new List<bool>();
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent("emtControlBoard"))
            {
                bools = item.Value.ControlBoardReadState();
            }
            return bools;
        }

        public override List<bool> ControlBoardReadState(List<int> lstIDs)
        {
            List<bool> bools = new List<bool>();
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent("emtControlBoard"))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    bools = item.Value.ControlBoardReadState();
                }
            }
            return bools;
        }

        public override void SetRelaySwitch(uint Register, bool OnOff)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent("emtDIORelay"))
            {
                item.Value.SetRelaySwitch(Register, OnOff);
            }
        }

        public override void SetRelaySwitch(List<int> lstIDs, uint Register, bool OnOff)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent("emtDIORelay"))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.SetRelaySwitch(Register, OnOff);
                }
            }
        }

        public override List<bool> ReadRelaySwitch(int StratIndex, int RelayCount)
        {
            List<bool> res = new List<bool>();
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent("emtDIORelay"))
            {
                res = item.Value.ReadRelaySwitch(StratIndex, RelayCount);
            }
            return res;
        }
    }
}
