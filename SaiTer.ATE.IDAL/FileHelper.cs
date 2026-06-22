using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace SaiTer.ATE.IDAL
{
    public class FileHelper
    {
        public static bool SaveContent(string fileName, string content)
        {
            try
            {
                // 创建并配置保存文件对话框
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    // 设置默认文件名
                    saveFileDialog.FileName = $"{fileName}.txt";

                    // 设置默认文件过滤器
                    saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

                    // 设置默认文件类型
                    saveFileDialog.DefaultExt = "*.txt";

                    // 显示对话框
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        // 获取用户选择的完整文件路径
                        string filePath = saveFileDialog.FileName;

                        // 要保存的字符串数据
                        string dataToSave = content;

                        // 使用StreamWriter写入数据到文件
                        using (StreamWriter writer = new StreamWriter(filePath))
                        {
                            writer.Write(dataToSave);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                return false;
            }
        }
    }
}
