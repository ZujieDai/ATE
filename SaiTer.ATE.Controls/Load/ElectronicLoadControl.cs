using SaiTer.ATE.EquipMent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Controls
{
    public class ElectronicLoadControl : ElectronicLoadBase
    {
        private string[] ClassNames = new string[] { "emtElectronicLoad" };
        public override void ElectronicLoad_ON(List<int> lstIDs)
        {
            try
            {
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
                {
                    //int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    //if (id.Length > 0)
                    {

                        item.Value.ElectronicLoad_ON();

                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }
        public override void ElectronicLoad_OFF(List<int> lstIDs)
        {
            try
            {
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
                {
                    //int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    //if (id.Length > 0)
                    {

                        item.Value.ElectronicLoad_OFF();

                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        /// <summary>
        /// 设置电子负载参数
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public override void SetElectronicLoadParams(List<int> lstIDs, byte tCom, UInt32 tOperate)
        {
            try
            {
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
                {
                    //int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    //if (id.Length > 0)
                    {

                        item.Value.SetElectronicLoadParams(tCom, tOperate);

                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        /// <summary>
        /// 设置电子负载参数
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public override void SetElectronicLoadParams(List<int> lstIDs, byte tCom, byte tOperate)
        {
            try
            {
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
                {
                    //int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    //if (id.Length > 0)
                    {

                        item.Value.SetElectronicLoadParams(tCom, tOperate);

                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        /// <summary>
        /// 读取电子负载参数
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public override Dictionary<int, UInt32> ReadElectronicLoadParams(List<int> lstIDs, byte tCom)
        {
            Dictionary<int, UInt32> dic = new Dictionary<int, uint>();
            try
            {
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
                {
                    //int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    //if (id.Length > 0)
                    {

                        var result = item.Value.ReadElectronicLoadParams(tCom);
                        dic.Add(item.Key, result);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
            return dic;
        }
    }
}
