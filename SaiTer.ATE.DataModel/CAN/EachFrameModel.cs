using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.CAN
{
    public class EachFrameModel
    {
        public List<byte> Buffer;
        public int Len;
        public string Cmd;

        public EachFrameModel()
        {
            Buffer = new List<byte>();
            Len = 0;
        }
        public void Reset()
        {
            Buffer.Clear();
            Cmd = string.Empty;
            Len = 0;
        }
        public void CopyData(ref EachFrameModel model)
        {
            if (this.Buffer != null)
            {
                foreach (byte e in this.Buffer)
                {
                    model.Len = this.Len;
                    model.Cmd = this.Cmd;
                    model.Buffer.Add(e);
                }
            }
        }
        public void SetFrameName(string cmd, int len)
        {
            this.Cmd = cmd;
            this.Len = len / 2;
        }
    }
}
