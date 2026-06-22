using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.EquipStateData
{
    /// <summary>
    /// 鼎阳示波器实时状态数据
    /// </summary>
    public class SDSOscilloscope_StateData: StateDataBase
    {
        public SDSOscilloscope_StateData()
        {
            EquipName = "SDS示波器";
        }
    }
}
