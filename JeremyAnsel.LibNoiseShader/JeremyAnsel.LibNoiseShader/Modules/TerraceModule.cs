using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JeremyAnsel.LibNoiseShader.Modules
{
    public sealed class TerraceModule : ModuleBase
    {
        public const int MinimumControlPointsCount = 2;

        public TerraceModule(IModule? module)
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
            float sourceModuleValue = this.GetSourceModule(0)!.GetValue(x, y, z);

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

        public override int EmitHlslMaxDepth()
        {
            return 1;
        }

        public override void EmitHlsl(HlslContext context)
        {
            context.EmitHeader(this);
            this.GetSourceModule(0)!.EmitHlsl(context);
            context.EmitSettings(this);
            context.EmitFunction(this, false);
        }

        public override void EmitHlslHeader(HlslContext context, StringBuilder header)
        {
            if (this.ControlPoints.Count < MinimumControlPointsCount)
            {
                this.MakePoints(MinimumControlPointsCount);
            }

            string key = nameof(TerraceModule);

            int maxPointsCount = context.GetTraverseValue<TerraceModule>(
                this,
                module => module.ControlPoints.Count,
                (a, b) => Math.Max(a, b));

            header.AppendTabFormatLine("int {0}_ControlPoints_Count = {1};", key, maxPointsCount);

            header.AppendTabFormatLine(
                "float {0}_ControlPoints[{1}] = {{ {2} }};",
                key,
                maxPointsCount,
                string.Concat(Enumerable.Repeat("0.0f,", maxPointsCount)));

            header.AppendTabFormatLine("bool {0}_IsInverted = false;", key);
        }

        public override bool HasHlslSettings()
        {
            return true;
        }

        public override void EmitHlslSettings(StringBuilder body)
        {
            string key = nameof(TerraceModule);

            body.AppendTabFormatLine(2, "{0}_ControlPoints_Count = {1};", key, this.ControlPoints.Count);

            for (int index = 0; index < this.ControlPoints.Count; index++)
            {
                var point = this.ControlPoints.ElementAt(index);
                body.AppendTabFormatLine(2, "{0}_ControlPoints[{1}] = {2};", key, index, point);
            }

            body.AppendTabFormatLine(2, "{0}_IsInverted = {1};", key, this.IsInverted ? "true" : "false");
        }

        public override bool HasHlslCoords(int index)
        {
            return false;
        }

        public override void EmitHlslCoords(StringBuilder body, int index)
        {
        }

        public override int GetHlslFunctionParametersCount()
        {
            return 1;
        }

        public override void EmitHlslFunction(StringBuilder body)
        {
            string key = nameof(TerraceModule);

            body.AppendTabFormatLine(2, "float sourceModuleValue = param0;");
            body.AppendTabFormatLine();
            body.AppendTabFormatLine(2, "int indexPos = 0;");
            body.AppendTabFormatLine(2, "[fastopt] for (; indexPos < {0}_ControlPoints_Count; indexPos++)", key);
            body.AppendTabFormatLine(2, "{");
            body.AppendTabFormatLine(3, "if (sourceModuleValue < {0}_ControlPoints[indexPos])", key);
            body.AppendTabFormatLine(3, "{");
            body.AppendTabFormatLine(4, "break;");
            body.AppendTabFormatLine(3, "}");
            body.AppendTabFormatLine(2, "}");
            body.AppendTabFormatLine();
            body.AppendTabFormatLine(2, "int index0 = clamp( indexPos - 1, 0, {0}_ControlPoints_Count - 1 );", key);
            body.AppendTabFormatLine(2, "int index1 = clamp( indexPos, 0, {0}_ControlPoints_Count - 1 );", key);
            body.AppendTabFormatLine();
            body.AppendTabFormatLine(2, "[branch] if (index0 == index1)");
            body.AppendTabFormatLine(2, "{");
            body.AppendTabFormatLine(3, "result = {0}_ControlPoints[index1];", key);
            body.AppendTabFormatLine(2, "}");
            body.AppendTabFormatLine(2, "else");
            body.AppendTabFormatLine(2, "{");
            body.AppendTabFormatLine(3, "float value0 = {0}_ControlPoints[index0];", key);
            body.AppendTabFormatLine(3, "float value1 = {0}_ControlPoints[index1];", key);
            body.AppendTabFormatLine(3, "float alpha = (sourceModuleValue - value0) / (value1 - value0);");
            body.AppendTabFormatLine();
            body.AppendTabFormatLine(3, "[branch] if ({0}_IsInverted)", key);
            body.AppendTabFormatLine(3, "{");
            body.AppendTabFormatLine(4, "alpha = 1.0f - alpha;");
            body.AppendTabFormatLine(4, "float temp = value0;");
            body.AppendTabFormatLine(4, "value0 = value1;");
            body.AppendTabFormatLine(4, "value1 = temp;");
            body.AppendTabFormatLine(3, "}");
            body.AppendTabFormatLine(3, "alpha *= alpha;");
            body.AppendTabFormatLine(3, "result = Interpolation_Linear(value0, value1, alpha);");
            body.AppendTabFormatLine(2, "}");
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
