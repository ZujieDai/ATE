using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel
{
    public class IirFilter
    {
        private readonly double[] aCoefficients;
        private readonly double[] bCoefficients;
        private double[] xBuffer;
        private double[] yBuffer;
        private int bufferIndex;

        public IirFilter(double[] bCoefficients, double[] aCoefficients)
        {
            this.aCoefficients = aCoefficients;
            this.bCoefficients = bCoefficients;
            xBuffer = new double[bCoefficients.Length];
            yBuffer = new double[aCoefficients.Length];
            bufferIndex = 0;
        }

        public double ProcessSample(double sample)
        {
            // Shift the buffers
            for (int i = xBuffer.Length - 1; i > 0; i--)
            {
                xBuffer[i] = xBuffer[i - 1];
                yBuffer[i] = yBuffer[i - 1];
            }
            xBuffer[0] = sample;

            // Calculate the output sample
            double output = 0;                      
            for (int i = 0; i < bCoefficients.Length; i++)
            {
                output += bCoefficients[i] * xBuffer[i];
            }
            for (int i = 1; i < aCoefficients.Length; i++)
            {
                output -= aCoefficients[i] * yBuffer[i];
            }
            yBuffer[0] = output;

            return output;
        }
    }
}
