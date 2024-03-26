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

        public override int EmitHlslMaxDepth()
        {
            return 1;
        }

        public override void EmitHlsl(HlslContext context)
        {
            context.EmitHeader(this);
            this.GetSourceModule(0).EmitHlsl(context);
            context.EmitSettings(this);
            context.EmitFunction(this, false);
        }

        public override void EmitHlslHeader(HlslContext context, StringBuilder header)
        {
            if (this.ControlPoints.Count < 4)
            {
                SetDefaultPoints();
            }

            string key = nameof(CurveModule);

            int maxPointsCount = context.GetTraverseValue<CurveModule>(
                this,
                module => module.ControlPoints.Count,
                (a, b) => Math.Max(a, b));

            header.AppendTabFormatLine("int {0}_ControlPoints_Count = {1};", key, maxPointsCount);

            header.AppendTabFormatLine("float2 {0}_ControlPoints[{1}] =", key, maxPointsCount);
            header.AppendTabFormatLine("{");

            for (int index = 0; index < maxPointsCount; index++)
            {
                header.AppendTabFormatLine("{{ 0.0f, 0.0f }},");
            }

            header.AppendTabFormatLine("};");
        }

        public override bool HasHlslSettings()
        {
            return true;
        }

        public override void EmitHlslSettings(StringBuilder body)
        {
            string key = nameof(CurveModule);

            body.AppendTabFormatLine(2, "{0}_ControlPoints_Count = {1};", key, this.ControlPoints.Count);

            for (int index = 0; index < this.ControlPoints.Count; index++)
            {
                var point = this.ControlPoints.ElementAt(index);
                body.AppendTabFormatLine(2, "{0}_ControlPoints[{1}] = float2({2}, {3});", key, index, point.Key, point.Value);
            }
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
            string key = nameof(CurveModule);

            body.AppendTabFormatLine(2, "float sourceModuleValue = param0;");
            body.AppendTabFormatLine();
            body.AppendTabFormatLine(2, "int indexPos = 0;");
            body.AppendTabFormatLine(2, "[fastopt] for (; indexPos < {0}_ControlPoints_Count; indexPos++)", key);
            body.AppendTabFormatLine(2, "{");
            body.AppendTabFormatLine(3, "if (sourceModuleValue < {0}_ControlPoints[indexPos].x)", key);
            body.AppendTabFormatLine(3, "{");
            body.AppendTabFormatLine(4, "break;");
            body.AppendTabFormatLine(3, "}");
            body.AppendTabFormatLine(2, "}");
            body.AppendTabFormatLine();
            body.AppendTabFormatLine(2, "int index0 = clamp( indexPos - 2, 0, {0}_ControlPoints_Count - 1 );", key);
            body.AppendTabFormatLine(2, "int index1 = clamp( indexPos - 1, 0, {0}_ControlPoints_Count - 1 );", key);
            body.AppendTabFormatLine(2, "int index2 = clamp( indexPos, 0, {0}_ControlPoints_Count - 1 );", key);
            body.AppendTabFormatLine(2, "int index3 = clamp( indexPos + 1, 0, {0}_ControlPoints_Count - 1 );", key);
            body.AppendTabFormatLine();
            body.AppendTabFormatLine(2, "if (index1 == index2)");
            body.AppendTabFormatLine(2, "{");
            body.AppendTabFormatLine(3, "result = {0}_ControlPoints[index1].y;", key);
            body.AppendTabFormatLine(2, "}");
            body.AppendTabFormatLine(2, "else");
            body.AppendTabFormatLine(2, "{");
            body.AppendTabFormatLine(3, "float input0 = {0}_ControlPoints[index1].x;", key);
            body.AppendTabFormatLine(3, "float input1 = {0}_ControlPoints[index2].x;", key);
            body.AppendTabFormatLine(3, "float alpha = (sourceModuleValue - input0) / (input1 - input0);");
            body.AppendTabFormatLine();
            body.AppendTabFormatLine(3, "result = Interpolation_Cubic( {0}_ControlPoints[index0].y, {0}_ControlPoints[index1].y, {0}_ControlPoints[index2].y, {0}_ControlPoints[index3].y, alpha );", key);
            body.AppendTabFormatLine(2, "}");
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
