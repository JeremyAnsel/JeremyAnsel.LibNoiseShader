using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JeremyAnsel.LibNoiseShader.Modules
{
    public sealed class TerraceModule : ModuleBase
    {
        public const int MinimumControlPointsCount = 2;

        public TerraceModule(IModule module)
        {
            this.SetSourceModule(0, module);

            this.MakePoints(MinimumControlPointsCount);
            this.IsInverted = false;
        }

        public ICollection<float> ControlPoints { get; } = new SortedSet<float>();

        public int ControlPointsCount
        {
            get
            {
                return this.ControlPoints.Count;
            }

            set
            {
                this.MakePoints(value);
            }
        }

        public bool IsInverted { get; set; }

        public override int RequiredSourceModuleCount => 1;

        public void MakePoints(int count)
        {
            IList<float> points = BuildControlPoints(count);

            this.ControlPoints.Clear();

            for (int i = 0; i < points.Count; i++)
            {
                this.ControlPoints.Add(points[i]);
            }
        }

        public static IList<float> BuildControlPoints(int count)
        {
            if (count < TerraceModule.MinimumControlPointsCount)
            {
                count = TerraceModule.MinimumControlPointsCount;
            }

            var points = new List<float>(count);

            float terraceStep = 2.0f / (count - 1.0f);
            float curValue = -1.0f;

            for (int i = 0; i < count; i++)
            {
                points.Add(curValue);

                curValue += terraceStep;
            }

            return points;
        }

        public override float GetValue(float x, float y, float z)
        {
            if (this.ControlPoints.Count < TerraceModule.MinimumControlPointsCount)
            {
                this.MakePoints(MinimumControlPointsCount);
            }

            // Get the output value from the source module.
            float sourceModuleValue = this.GetSourceModule(0).GetValue(x, y, z);

            // Find the first element in the control point array that has a value
            // larger than the output value from the source module.
            int indexPos = 0;
            for (; indexPos < this.ControlPoints.Count; indexPos++)
            {
                if (sourceModuleValue < this.ControlPoints.ElementAt(indexPos))
                {
                    break;
                }
            }

            // Find the two nearest control points so that we can map their values
            // onto a quadratic curve.
            int index0 = Math.Min(Math.Max(indexPos - 1, 0), this.ControlPoints.Count - 1);
            int index1 = Math.Min(Math.Max(indexPos, 0), this.ControlPoints.Count - 1);

            // If some control points are missing (which occurs if the output value from
            // the source module is greater than the largest value or less than the
            // smallest value of the control point array), get the value of the nearest
            // control point and exit now.
            if (index0 == index1)
            {
                return this.ControlPoints.ElementAt(index1);
            }

            // Compute the alpha value used for linear interpolation.
            float value0 = this.ControlPoints.ElementAt(index0);
            float value1 = this.ControlPoints.ElementAt(index1);
            float alpha = (sourceModuleValue - value0) / (value1 - value0);

            if (this.IsInverted)
            {
                alpha = 1.0f - alpha;
                float temp = value0;
                value0 = value1;
                value1 = temp;
            }

            // Squaring the alpha produces the terrace effect.
            alpha *= alpha;

            // Now perform the linear interpolation given the alpha value.
            return Interpolation.Linear(value0, value1, alpha);
        }

        public override string GetHlslBody(HlslContext context)
        {
            if (this.ControlPoints.Count < TerraceModule.MinimumControlPointsCount)
            {
                this.MakePoints(MinimumControlPointsCount);
            }

            var sb = new StringBuilder();
            string module0 = context.GetModuleName(this.GetSourceModule(0));

            sb.AppendTabFormatLine(context.GetModuleFunctionDefinition(this));
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "static const float controlPoints[{0}] =", this.ControlPoints.Count);
            sb.AppendTabFormatLine(1, "{");

            foreach (float point in this.ControlPoints)
            {
                sb.AppendTabFormatLine(2, "{0},", point);
            }

            sb.AppendTabFormatLine(1, "};");
            sb.AppendTabFormatLine();
            sb.AppendTabFormatLine(1, "float sourceModuleValue = {0}(x, y, z);", module0);
            sb.AppendTabFormatLine();
            sb.AppendTabFormatLine(1, "int indexPos = 0;");
            sb.AppendTabFormatLine(1, "[fastopt] for (; indexPos < {0}; indexPos++)", this.ControlPoints.Count);
            sb.AppendTabFormatLine(1, "{");
            sb.AppendTabFormatLine(2, "if (sourceModuleValue < controlPoints[indexPos])");
            sb.AppendTabFormatLine(2, "{");
            sb.AppendTabFormatLine(3, "break;");
            sb.AppendTabFormatLine(2, "}");
            sb.AppendTabFormatLine(1, "}");
            sb.AppendTabFormatLine();
            sb.AppendTabFormatLine(1, "int index0 = clamp( indexPos - 1, 0, {0} );", this.ControlPoints.Count - 1);
            sb.AppendTabFormatLine(1, "int index1 = clamp( indexPos, 0, {0} );", this.ControlPoints.Count - 1);
            sb.AppendTabFormatLine();
            sb.AppendTabFormatLine(1, "if (index0 == index1)");
            sb.AppendTabFormatLine(1, "{");
            sb.AppendTabFormatLine(2, "return controlPoints[index1];");
            sb.AppendTabFormatLine(1, "}");
            sb.AppendTabFormatLine();
            sb.AppendTabFormatLine(1, "float value0 = controlPoints[index0];");
            sb.AppendTabFormatLine(1, "float value1 = controlPoints[index1];");
            sb.AppendTabFormatLine(1, "float alpha = (sourceModuleValue - value0) / (value1 - value0);");
            sb.AppendTabFormatLine();

            if (this.IsInverted)
            {
                sb.AppendTabFormatLine(1, "alpha = 1.0f - alpha;");
                sb.AppendTabFormatLine(1, "float temp = value0;");
                sb.AppendTabFormatLine(1, "value0 = value1;");
                sb.AppendTabFormatLine(1, "value1 = temp;");
                sb.AppendTabFormatLine();
            }

            sb.AppendTabFormatLine(1, "alpha *= alpha;");
            sb.AppendTabFormatLine(1, "return Interpolation_Linear(value0, value1, alpha);");
            sb.AppendTabFormatLine("}");

            return sb.ToString();
        }

        public override string GetCSharpBody(CSharpContext context)
        {
            var sb = new StringBuilder();
            string module0 = context.GetModuleName(this.GetSourceModule(0));
            string name = context.GetModuleName(this);
            string type = context.GetModuleType(this);

            sb.AppendTabFormatLine("{0} {1} = new({2})", type, name, module0);
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "IsInverted = {0},", this.IsInverted);
            sb.AppendTabFormatLine("};");
            sb.AppendTabFormatLine("{0}.ControlPoints.Clear();");

            foreach (float controlPoint in this.ControlPoints)
            {
                sb.AppendTabFormatLine("{0}.ControlPoints.Add({1});", name, controlPoint);
            }

            return sb.ToString();
        }
    }
}
