using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace JeremyAnsel.LibNoiseShader.Renderers
{
    internal sealed class GradientColor
    {
        public IDictionary<float, Color> GradientPoints { get; } = new SortedList<float, Color>()
        {
            { -1.0f, Color.Black },
            { 1.0f, Color.White }
        };

        public void SetDefaultPoints()
        {
            this.GradientPoints.Clear();
            this.GradientPoints.Add(-1.0f, Color.Black);
            this.GradientPoints.Add(1.0f, Color.White);
        }

        public Color GetColor(float position)
        {
            if (this.GradientPoints.Count < 2)
            {
                SetDefaultPoints();
            }

            // Find the first element in the gradient point array that has a gradient
            // position larger than the gradient position passed to this method.
            int index = 0;
            for (; index < this.GradientPoints.Count; index++)
            {
                if (position < this.GradientPoints.ElementAt(index).Key)
                {
                    break;
                }
            }

            // Find the two nearest gradient points so that we can perform linear
            // interpolation on the color.
            int index0 = Math.Min(Math.Max(index - 1, 0), this.GradientPoints.Count - 1);
            int index1 = Math.Min(Math.Max(index, 0), this.GradientPoints.Count - 1);

            // If some gradient points are missing (which occurs if the gradient
            // position passed to this method is greater than the largest gradient
            // position or less than the smallest gradient position in the array), get
            // the corresponding gradient color of the nearest gradient point and exit
            // now.
            if (index0 == index1)
            {
                return this.GradientPoints.ElementAt(index1).Value;
            }

            KeyValuePair<float, Color> point0 = this.GradientPoints.ElementAt(index0);
            KeyValuePair<float, Color> point1 = this.GradientPoints.ElementAt(index1);

            float alpha = (position - point0.Key) / (point1.Key - point0.Key);

            return LinearInterpColor(point0.Value, point1.Value, alpha);
        }

        private static byte BlendChannel(byte channel0, byte channel1, float alpha)
        {
            float c0 = channel0 / 255.0f;
            float c1 = channel1 / 255.0f;

            return (byte)((c0 + (c1 - c0) * alpha) * 255.0f);
        }

        private static Color LinearInterpColor(Color color0, Color color1, float alpha)
        {
            byte a = BlendChannel(color0.A, color1.A, alpha);
            byte r = BlendChannel(color0.R, color1.R, alpha);
            byte g = BlendChannel(color0.G, color1.G, alpha);
            byte b = BlendChannel(color0.B, color1.B, alpha);

            return Color.FromArgb(a, r, g, b);
        }

        public string GetHlslBody(string functionPrefix)
        {
            if (this.GradientPoints.Count < 2)
            {
                SetDefaultPoints();
            }

            var sb = new StringBuilder();

            sb.AppendTabFormatLine(0, "float4 {0}_GetGradientColor(float position)", functionPrefix);
            sb.AppendTabFormatLine(0, "{");

            sb.AppendTabFormatLine(1, "static const float gradientPointKeys[{0}] =", this.GradientPoints.Count);
            sb.AppendTabFormatLine(1, "{");

            for (int i = 0; i < this.GradientPoints.Count; i++)
            {
                float key = this.GradientPoints.ElementAt(i).Key;
                sb.AppendTabFormatLine(2, "{0},", key);
            }

            sb.AppendTabFormatLine(1, "};");
            sb.AppendTabFormatLine();

            sb.AppendTabFormatLine(1, "static const float4 gradientPointValues[{0}] =", this.GradientPoints.Count);
            sb.AppendTabFormatLine(1, "{");

            for (int i = 0; i < this.GradientPoints.Count; i++)
            {
                Color value = this.GradientPoints.ElementAt(i).Value;
                sb.AppendTabFormatLine(2, "float4({0}, {1}, {2}, {3}),", value.R / 255.0f, value.G / 255.0f, value.B / 255.0f, value.A / 255.0f);
            }

            sb.AppendTabFormatLine(1, "};");
            sb.AppendTabFormatLine();

            // Find the first element in the gradient point array that has a gradient
            // position larger than the gradient position passed to this method.
            sb.AppendTabFormatLine(1, "int gradientPointIndex = 0;");
            sb.AppendTabFormatLine(1, "[fastopt] for (; gradientPointIndex < {0}; gradientPointIndex++)", this.GradientPoints.Count);
            sb.AppendTabFormatLine(1, "{");
            sb.AppendTabFormatLine(2, "if (position < gradientPointKeys[gradientPointIndex])");
            sb.AppendTabFormatLine(2, "{");
            sb.AppendTabFormatLine(3, "break;");
            sb.AppendTabFormatLine(2, "};");
            sb.AppendTabFormatLine(1, "};");
            sb.AppendTabFormatLine();

            // Find the two nearest gradient points so that we can perform linear
            // interpolation on the color.
            sb.AppendTabFormatLine(1, "int index0 = min(max(gradientPointIndex - 1, 0), {0} - 1);", this.GradientPoints.Count);
            sb.AppendTabFormatLine(1, "int index1 = min(max(gradientPointIndex, 0), {0} - 1);", this.GradientPoints.Count);
            sb.AppendTabFormatLine();

            // If some gradient points are missing (which occurs if the gradient
            // position passed to this method is greater than the largest gradient
            // position or less than the smallest gradient position in the array), get
            // the corresponding gradient color of the nearest gradient point and exit
            // now.
            sb.AppendTabFormatLine(1, "if (index0 == index1)");
            sb.AppendTabFormatLine(1, "{");
            sb.AppendTabFormatLine(2, "return gradientPointValues[index1];");
            sb.AppendTabFormatLine(1, "}");
            sb.AppendTabFormatLine();
            sb.AppendTabFormatLine(1, "float alpha = (position - gradientPointKeys[index0]) / (gradientPointKeys[index1] - gradientPointKeys[index0]);");
            sb.AppendTabFormatLine(1, "float4 value0 = gradientPointValues[index0];");
            sb.AppendTabFormatLine(1, "float4 value1 = gradientPointValues[index1];");
            sb.AppendTabFormatLine();
            sb.AppendTabFormatLine(1, "return float4(");
            sb.AppendTabFormatLine(2, "Interpolation_Linear(value0.x, value1.x, alpha),");
            sb.AppendTabFormatLine(2, "Interpolation_Linear(value0.y, value1.y, alpha),");
            sb.AppendTabFormatLine(2, "Interpolation_Linear(value0.z, value1.z, alpha),");
            sb.AppendTabFormatLine(2, "Interpolation_Linear(value0.w, value1.w, alpha)");
            sb.AppendTabFormatLine(1, ");");

            sb.AppendTabFormatLine(0, "}");

            return sb.ToString();
        }
    }
}
