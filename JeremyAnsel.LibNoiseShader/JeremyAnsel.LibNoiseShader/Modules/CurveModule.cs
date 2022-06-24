using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JeremyAnsel.LibNoiseShader.Modules
{
    public sealed class CurveModule : ModuleBase
    {
        public CurveModule(IModule module0)
        {
            this.SetSourceModule(0, module0);
        }

        public IDictionary<float, float> ControlPoints { get; } = new SortedList<float, float>()
        {
            { -1.0f, -1.0f },
            { -0.5f, -0.5f },
            { 0.0f, 0.0f },
            { 0.5f, 0.5f },
            { 1.0f, 1.0f }
        };

        public override int RequiredSourceModuleCount => 1;

        public void SetDefaultPoints()
        {
            this.ControlPoints.Clear();
            this.ControlPoints.Add(-1.0f, -1.0f);
            this.ControlPoints.Add(-0.5f, -0.5f);
            this.ControlPoints.Add(0.0f, 0.0f);
            this.ControlPoints.Add(0.5f, 0.5f);
            this.ControlPoints.Add(1.0f, 1.0f);
        }

        public override float GetValue(float x, float y, float z)
        {
            if (this.ControlPoints.Count < 4)
            {
                SetDefaultPoints();
            }

            // Get the output value from the source module.
            float sourceModuleValue = this.GetSourceModule(0).GetValue(x, y, z);

            // Find the first element in the control point array that has an input value
            // larger than the output value from the source module.
            int indexPos = 0;
            for (; indexPos < this.ControlPoints.Count; indexPos++)
            {
                if (sourceModuleValue < this.ControlPoints.ElementAt(indexPos).Key)
                {
                    break;
                }
            }

            // Find the four nearest control points so that we can perform cubic
            // interpolation.
            int index0 = Math.Min(Math.Max(indexPos - 2, 0), this.ControlPoints.Count - 1);
            int index1 = Math.Min(Math.Max(indexPos - 1, 0), this.ControlPoints.Count - 1);
            int index2 = Math.Min(Math.Max(indexPos, 0), this.ControlPoints.Count - 1);
            int index3 = Math.Min(Math.Max(indexPos + 1, 0), this.ControlPoints.Count - 1);

            // If some control points are missing (which occurs if the value from the
            // source module is greater than the largest input value or less than the
            // smallest input value of the control point array), get the corresponding
            // output value of the nearest control point and exit now.
            if (index1 == index2)
            {
                return this.ControlPoints.ElementAt(index1).Value;
            }

            // Compute the alpha value used for cubic interpolation.
            float input0 = this.ControlPoints.ElementAt(index1).Key;
            float input1 = this.ControlPoints.ElementAt(index2).Key;
            float alpha = (sourceModuleValue - input0) / (input1 - input0);

            // Now perform the cubic interpolation given the alpha value.
            return Interpolation.Cubic(
                this.ControlPoints.ElementAt(index0).Value,
                this.ControlPoints.ElementAt(index1).Value,
                this.ControlPoints.ElementAt(index2).Value,
                this.ControlPoints.ElementAt(index3).Value,
                alpha);
        }

        public override string GetHlslBody(HlslContext context)
        {
            if (this.ControlPoints.Count < 4)
            {
                SetDefaultPoints();
            }

            var sb = new StringBuilder();
            string module0 = context.GetModuleName(this.GetSourceModule(0));

            sb.AppendTabFormatLine(context.GetModuleFunctionDefinition(this));
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "static const float2 controlPoints[{0}] =", this.ControlPoints.Count);
            sb.AppendTabFormatLine(1, "{");

            foreach (KeyValuePair<float, float> point in this.ControlPoints)
            {
                sb.AppendTabFormatLine(2, "{{ {0}, {1} }},", point.Key, point.Value);
            }

            sb.AppendTabFormatLine(1, "};");
            sb.AppendTabFormatLine();
            sb.AppendTabFormatLine(1, "float sourceModuleValue = {0}(x, y, z);", module0);
            sb.AppendTabFormatLine();
            sb.AppendTabFormatLine(1, "int indexPos = 0;");
            sb.AppendTabFormatLine(1, "[fastopt] for (; indexPos < {0}; indexPos++)", this.ControlPoints.Count);
            sb.AppendTabFormatLine(1, "{");
            sb.AppendTabFormatLine(2, "if (sourceModuleValue < controlPoints[indexPos].x)");
            sb.AppendTabFormatLine(2, "{");
            sb.AppendTabFormatLine(3, "break;");
            sb.AppendTabFormatLine(2, "}");
            sb.AppendTabFormatLine(1, "}");
            sb.AppendTabFormatLine();
            sb.AppendTabFormatLine(1, "int index0 = clamp( indexPos - 2, 0, {0} );", this.ControlPoints.Count - 1);
            sb.AppendTabFormatLine(1, "int index1 = clamp( indexPos - 1, 0, {0} );", this.ControlPoints.Count - 1);
            sb.AppendTabFormatLine(1, "int index2 = clamp( indexPos, 0, {0} );", this.ControlPoints.Count - 1);
            sb.AppendTabFormatLine(1, "int index3 = clamp( indexPos + 1, 0, {0} );", this.ControlPoints.Count - 1);
            sb.AppendTabFormatLine();
            sb.AppendTabFormatLine(1, "if (index1 == index2)");
            sb.AppendTabFormatLine(1, "{");
            sb.AppendTabFormatLine(2, "return controlPoints[index1].y;");
            sb.AppendTabFormatLine(1, "}");
            sb.AppendTabFormatLine();
            sb.AppendTabFormatLine(1, "float input0 = controlPoints[index1].x;");
            sb.AppendTabFormatLine(1, "float input1 = controlPoints[index2].x;");
            sb.AppendTabFormatLine(1, "float alpha = (sourceModuleValue - input0) / (input1 - input0);");
            sb.AppendTabFormatLine();
            sb.AppendTabFormatLine(1, "return Interpolation_Cubic( controlPoints[index0].y, controlPoints[index1].y, controlPoints[index2].y, controlPoints[index3].y, alpha );");
            sb.AppendTabFormatLine("}");

            return sb.ToString();
        }

        public void SetControlPoints(IEnumerable<KeyValuePair<float, float>> controlPoints)
        {
            if (controlPoints is null)
            {
                throw new ArgumentNullException(nameof(controlPoints));
            }

            this.ControlPoints.Clear();

            foreach (KeyValuePair<float, float> controlPoint in controlPoints)
            {
                this.ControlPoints.Add(controlPoint);
            }
        }

        public override string GetCSharpBody(CSharpContext context)
        {
            var sb = new StringBuilder();
            string module0 = context.GetModuleName(this.GetSourceModule(0));
            string name = context.GetModuleName(this);
            string type = context.GetModuleType(this);

            sb.AppendTabFormatLine("{0} {1} = new({2});", type, name, module0);
            sb.AppendTabFormatLine("{0}.ControlPoints.Clear();");

            foreach (KeyValuePair<float, float> controlPoint in this.ControlPoints)
            {
                sb.AppendTabFormatLine("{0}.ControlPoints.Add({1}, {2});", name, controlPoint.Key, controlPoint.Value);
            }

            return sb.ToString();
        }
    }
}
