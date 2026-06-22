using SaiTer.ATE.IDAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SaiTer.ATE.DataModel.Consist;

namespace SaiTer.ATE.EquipMent
{
    public class NPIOUtil
    {
        public static void CreateExcelFile(string path, int ChargerID, Queue<string> QTitle)
        {
            if (!System.IO.Directory.Exists(Path.GetDirectoryName(path)))
            {
                System.IO.Directory.CreateDirectory(Path.GetDirectoryName(path));
            }


            NpioOperation.ExcelSaveFileName = path;
            //if(TSField.Name.Length>25)
            //{
            //    NpioOperation.ExcelSaveSheetName = TSField.Name.Substring(0,25);
            //}
            //else
            //{
            NpioOperation.ExcelSaveSheetName = "枪号" + ChargerID.ToString(); ;
            //}



            if (!System.IO.File.Exists(NpioOperation.ExcelSaveFileName))
            {
                NpioOperation.ExcelWR.CreatExcel2003(NpioOperation.ExcelSaveFileName, NpioOperation.ExcelSaveSheetName);
            }


            NpioOperation.ExcelWR.CreateSheet(NpioOperation.ExcelSaveFileName, NpioOperation.ExcelSaveSheetName);


            NpioOperation.RowNum = 0;
            NpioOperation.Cout = 0;
            NpioOperation.ExcelWR.OpenExceWorkBook(NpioOperation.ExcelSaveFileName, NpioOperation.ExcelSaveSheetName, QTitle);
            NpioOperation.ExcelWR.CloseExceWorkBook(NpioOperation.ExcelSaveFileName);
        }

    }
}
