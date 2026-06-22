using SaiTer.ATE.EquipMent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Controls
{
    public class ElectricMeterControl:ElectricMeterBase
    {
        private string[] ClassNames = new string[] { "emtElectricMeter_DTSD336D" };
        public override Dictionary<int, double> EM_GetKeyValue(List<int> lstIDs, int JcqAdd, int JcqCount, double XZoom)
        {
            Dictionary<int, double> dicTmp = new Dictionary<int, double>();
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                double dtmp = 0;
                if (id.Length > 0)
                {
                    dtmp = item.Value.EM_GetKeyValue(JcqAdd, JcqCount, XZoom);
                    if (dtmp < 0)
                    {
                        dtmp = item.Value.EM_GetKeyValue(JcqAdd, JcqCount, XZoom);
                    }
                    dicTmp.Add(id[0], dtmp);
                }
            }
            return dicTmp;
        }

        public override Dictionary<int, double[]> EM_GetVolt(List<int> lstIDs)
        {
            Dictionary<int, double[]> dicTmp = new Dictionary<int, double[]>();
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                double[] dtmp = null;
                if (id.Length > 0)
                {
                    dtmp = item.Value.EM_GetVolt();
                    if (dtmp ==null)
                    {
                        dtmp = item.Value.EM_GetVolt();
                    }
                    dicTmp.Add(id[0], dtmp);
                }
            }
            return dicTmp;
        }

        public override Dictionary<int, double[]> EM_GetCurrent(List<int> lstIDs)
        {
            Dictionary<int, double[]> dicTmp = new Dictionary<int, double[]>();
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                double[] dtmp = null;
                if (id.Length > 0)
                {
                    dtmp = item.Value.EM_GetCurrent();
                    if (dtmp == null)
                    {
                        dtmp = item.Value.EM_GetCurrent();
                    }
                    dicTmp.Add(id[0], dtmp);
                }
            }
            return dicTmp;
        }

        public override Dictionary<int, double[]> EM_GetPower(List<int> lstIDs)
        {
            Dictionary<int, double[]> dicTmp = new Dictionary<int, double[]>();
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                double[] dtmp = null;
                if (id.Length > 0)
                {
                    dtmp = item.Value.EM_GetPower();
                    if (dtmp == null)
                    {
                        dtmp = item.Value.EM_GetPower();
                    }
                    dicTmp.Add(id[0], dtmp);
                }
            }
            return dicTmp;
        }

        public override Dictionary<int, double[]> EM_GetPowerFactor(List<int> lstIDs)
        {
            Dictionary<int, double[]> dicTmp = new Dictionary<int, double[]>();
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                double[] dtmp = null;
                if (id.Length > 0)
                {
                    dtmp = item.Value.EM_GetPowerFactor();
                    if (dtmp == null)
                    {
                        dtmp = item.Value.EM_GetPowerFactor();
                    }
                    dicTmp.Add(id[0], dtmp);
                }
            }
            return dicTmp;
        }

        public override Dictionary<int, double[]> EM_GetPhaseAngle(List<int> lstIDs)
        {
            Dictionary<int, double[]> dicTmp = new Dictionary<int, double[]>();
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                double[] dtmp = null;
                if (id.Length > 0)
                {
                    dtmp = item.Value.EM_GetPhaseAngle();
                    if (dtmp == null)
                    {
                        dtmp = item.Value.EM_GetPhaseAngle();
                    }
                    dicTmp.Add(id[0], dtmp);
                }
            }
            return dicTmp;
        }

        public override Dictionary<int, double> EM_GetTotalPower(List<int> lstIDs)
        {
            Dictionary<int, double> dicTmp = new Dictionary<int, double>();
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                double dtmp = 0;
                if (id.Length > 0)
                {
                    dtmp = item.Value.EM_GetTotalPower();
                    if (dtmp < 0)
                    {
                        dtmp = item.Value.EM_GetTotalPower();
                    }
                    dicTmp.Add(id[0], dtmp);
                }
            }
            return dicTmp;
        }

        public override Dictionary<int, double> EM_GetTotalPower_ZH(List<int> lstIDs)
        {
            Dictionary<int, double> dicTmp = new Dictionary<int, double>();
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent("emtElectricMeter_ZH4041"))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                double dtmp = 0;
                if (id.Length > 0)
                {
                    dtmp = item.Value.EM_GetTotalPower_ZH();
                    if (dtmp < 0)
                    {
                        dtmp = item.Value.EM_GetTotalPower_ZH();
                    }
                    dicTmp.Add(id[0], dtmp);
                }
            }
            return dicTmp;
        }
    }
}
