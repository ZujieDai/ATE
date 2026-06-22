using SaiTer.ATE.EquipMent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Controls
{
    public class ChargerControl : ChargerBase
    {
        private string[] ClassNames = new string[] { "emtCharger_NTGX"};

        public override void ReadCharger_StateData(List<int> lstIDs)
        {
            try
            {
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {
                        item.Value.ReadCharger_StateData();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        public override void SetCMLParam(List<int> lstIDs, double MaxU, double MinU, double MaxI, double MinI)
        {
            try
            {
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {
                        item.Value.SetCMLParam(MaxU, MinU, MaxI, MinI);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        public override void ChargerStart(List<int> lstIDs)
        {
            try
            {
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {
                        item.Value.ChargerStart();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }


        public override void ChargerStop(List<int> lstIDs)
        {
            try
            {
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {
                        item.Value.ChargerStop();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }


        public override void LoadStart_Charger(List<int> lstIDs)
        {
            try
            {
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {
                        item.Value.LoadStart_Charger();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }


        public override void LoadStop_Charger(List<int> lstIDs)
        {
            try
            {
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {
                        item.Value.LoadStop_Charger();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }


        public override void SetLoadParam_Charger(List<int> lstIDs, double dVoltage, double dCurrent)
        {
            try
            {
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {
                        item.Value.SetLoadParam_Charger(dVoltage, dCurrent);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }


        public override void CANMsgAutoUpLoad_Charger(List<int> lstIDs, bool isUpLoad)
        {
            try
            {
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {
                        item.Value.CANMsgAutoUpLoad_Charger(isUpLoad);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }



    }
}
