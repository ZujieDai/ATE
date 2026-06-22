using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aspose.Words;
using Aspose.Words.Saving;
using Spire.Doc;

namespace SaiTer.ATE.IDAL
{
    public class FileConvertHelper
    {
        /// <summary>
        /// 将Word文件转换为PDF文件
        /// </summary>
        /// <param name="sSourcePath">源文件路径</param>
        /// <param name="sTargetPath">存储目标文件路径</param>
        /// <returns>返回结果</returns>
        public static bool ConvertFile_WordToPdf_Aspose(string sSourcePath, string sTargetPath)
        {
            bool bret = false;
            try
            {
                Aspose.Words.Document doc = new Aspose.Words.Document(sSourcePath);
                doc.Save(sTargetPath, SaveFormat.Pdf);

                bret = true;
            }
            catch (Exception ex)
            {
                bret = false;
                Log.Log.LogException(ex);
            }


            return bret;
        }


        /// <summary>
        /// 将Word文件转换为Excel文件
        /// </summary>
        /// <param name="sSourcePath"></param>
        /// <param name="sTargetPath"></param>
        /// <returns></returns>
        public static bool ConvertFile_WordToExcel_Spire(string sSourcePath, string sTargetPath)
        {
            bool bret = false;
            try
            {
                Spire.Doc. Document doc = new Spire.Doc.Document(sSourcePath);
                doc.SaveToFile(sTargetPath, Spire.Doc.FileFormat.Xml);

                bret = true;
            }
            catch (Exception ex)
            {
                bret = false;
                Log.Log.LogException(ex);
            }


            return bret;
        }

    }
}
