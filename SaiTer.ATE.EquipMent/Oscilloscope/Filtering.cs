using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.EquipMent
{

    class Filtering
    {
        /// <summary>
        /// 中值滤波,去毛刺
        /// </summary>
        /// <param name="rawData"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        unsafe public double[] MedianFilter(double[] rawData, int step = 3)
        {
            double[] recordData = new double[step];
            if (rawData.Length >= step)
            {
                for (int i = 0; i < step; i++)
                { 
                    recordData[i] = rawData[i]; 
                }
            }



            int length = step * 2 + 1;
            double[] smooth = new double[rawData.Length];
            double[] median = new double[length];
            fixed (double* o = smooth, r = rawData, m = median)
            {
                for (int i = step; i < rawData.Length; i++)
                {
                    int s = i - step;
                    int k = 0;
                    for (int j = i - step; j < i + step + 1; j++)
                    {
                        if (j < rawData.Length)
                        {
                            m[k] = r[j];
                        }
                        else
                        {
                            break;
                        }
                        k++;
                    }
                    o[i] = SortBubbleAscendingOrder(median)[step];//排序取中间值,在我的上一篇博客有源码
                }
                //Head fill
                for (int i = 0; i < step; i++)
                {
                    o[i] = o[step];
                }
                //Tail fill
                int tail = rawData.Length - (rawData.Length % (step + 1)) - 1;
                for (int j = tail; j < rawData.Length; j++)
                {
                    o[j] = o[tail - 1];
                }

            }
            if (rawData.Length >= step)
            {
                for (int i = 0; i < step; i++)
                {
                    smooth[i] = recordData[i];
                }
            }
            return smooth;
        }


        /// <summary>
        /// 冒泡升序
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        unsafe public double[] SortBubbleAscendingOrder(double[] rawData)
        {
            double[] outResult = new double[rawData.Length];
            fixed (double* o = outResult, r = rawData)
            {
                for (int i = 0; i < rawData.Length; i++)
                {
                    o[i] = r[i];
                    for (int j = i; j > 0; j--)
                    {
                        if (o[j] < o[j - 1])
                        {
                            double t = o[j];
                            o[j] = o[j - 1];
                            o[j - 1] = t;
                        }
                    }
                }
            }
            return outResult;
        }


        /// <summary>
        /// 冒泡降序
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        unsafe public double[] SortBubbleDescendingOrder(double[] rawData)
        {
            double[] outResult = new double[rawData.Length];
            fixed (double* o = outResult, r = rawData)
            {
                for (int i = 0; i < rawData.Length; i++)
                {
                    o[i] = r[i];
                    for (int j = i; j > 0; j--)
                    {
                        if (o[j] > o[j - 1])
                        {
                            double t = o[j];
                            o[j] = o[j - 1];
                            o[j - 1] = t;
                        }
                    }
                }
            }
            return outResult;
        }

    }
}
