using SaiTer.ATE.DataModel.WaveRecoder;
using SaiTer.ATE.EquipMent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Controls.WaveRecorder
{
    public class WaveRecoderControl : WaveRecoderBase
    {
        private string[] ClassNames = new string[] { "emtWaveRecoderBoard", "emtWaveRecoderBoard30" };

        public override void ReadWaveRecoderBoard_StateData(List<int> lstIDs)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.ReadWaveRecoderBoard_StateData();
                }
                else
                {
                    continue;
                }
            }
        }

        public override void WaveRecoder_SetSamplingRate(List<int> lstIDs, double data)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.WaveRecoder_SetSamplingRate(data);
                }
                else
                {
                    continue;
                }
            }
        }

        public override void WaveRecoder_Start(List<int> lstIDs)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.WaveRecoder_Start();
                }
                else
                {
                    continue;
                }
            }
        }

        public override void WaveRecoder_Stop(List<int> lstIDs)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.WaveRecoder_Stop();
                }
                else
                {
                    continue;
                }
            }
        }

        public override void WaveRecoder_ReadChannelData(List<int> lstIDs, int channnel, ref WaveData data, string linName = "")
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    data.LineName = linName;
                    item.Value.WaveRecoder_ReadChannelData(channnel, ref data);
                }
                else
                {
                    continue;
                }
            }
        }

        public override void WaveRecoder_ReadDigitalChannelData(List<int> lstIDs, int channnel, int subchannel, ref WaveData data, string linName = "")
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    data.LineName = linName;
                    item.Value.WaveRecoder_ReadDigitalChannelData(channnel, subchannel, ref data);
                }
                else
                {
                    continue;
                }
            }
        }

        public override void WaveRecoder_SetCursor(List<int> lstIDs, int iCursor, double dPoint)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.WaveRecoder_SetCursor(iCursor, dPoint);
                }
                else
                {
                    continue;
                }
            }
        }

        public override Dictionary<int, double> WaveRecoder_GetCursorData(List<int> lstIDs, int iType = 0)
        {
            Dictionary<int, double> dicCursorDatas = new Dictionary<int, double>();
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    dicCursorDatas.Add(id[0], item.Value.WaveRecoder_GetCursorData(iType));
                }
                else
                {
                    continue;
                }
            }

            return dicCursorDatas;
        }


        public override Dictionary<int, string> WaveRecoderSaveScreen(List<int> lstIDs)
        {
            Dictionary<int, string> dicPath = new Dictionary<int, string>();
            string path = "";
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.WaveRecoderSaveScreen(ref path);
                    dicPath.Add(id[0], path);
                }
            }
            return dicPath;

        }


    }
}
