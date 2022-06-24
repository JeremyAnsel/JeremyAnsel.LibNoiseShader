using System;
using System.Text;

namespace JeremyAnsel.LibNoiseShader.Modules
{
    public sealed class SelectorModule : ModuleBase
    {
        private float edgeFalloff;

        private float lowerBound;

        private float upperBound;

        public SelectorModule(IModule module0, IModule module1, IModule controlModule)
        {
            this.SetSourceModule(0, module0);
            this.SetSourceModule(1, module1);
            this.SetSourceModule(2, controlModule);

            this.edgeFalloff = 0.0f;
            this.lowerBound = -1.0f;
            this.upperBound = 1.0f;
        }

        public float EdgeFalloff
        {
            get
            {
                return this.edgeFalloff;
            }

            set
            {
                this.UpdateEdgeFalloff(value);
            }
        }

        public float LowerBound
        {
            get
            {
                return this.lowerBound;
            }

            set
            {
                this.lowerBound = value;
                this.UpdateEdgeFalloff(this.edgeFalloff);
            }
        }

        public float UpperBound
        {
            get
            {
                return this.upperBound;
            }

            set
            {
                this.upperBound = value;
                this.UpdateEdgeFalloff(this.edgeFalloff);
            }
        }

        public override int RequiredSourceModuleCount => 3;

        public override float GetValue(float x, float y, float z)
        {
            float controlValue = this.GetSourceModule(2).GetValue(x, y, z);

            if (this.EdgeFalloff > 0.0f)
            {
                if (controlValue < (this.LowerBound - this.EdgeFalloff))
                {
                    // The output value from the control module is below the selector
                    // threshold; return the output value from the first source module.

                    return this.GetSourceModule(0).GetValue(x, y, z);
                }
                else if (controlValue < (this.LowerBound + this.EdgeFalloff))
                {
                    // The output value from the control module is near the lower end of the
                    // selector threshold and within the smooth curve. Interpolate between
                    // the output values from the first and second source modules.

                    float lowerCurve = (this.LowerBound - this.EdgeFalloff);
                    float upperCurve = (this.LowerBound + this.EdgeFalloff);

                    float alpha = Interpolation.SCurve3((controlValue - lowerCurve) / (upperCurve - lowerCurve));

                    return Interpolation.Linear(
                        this.GetSourceModule(0).GetValue(x, y, z),
                        this.GetSourceModule(1).GetValue(x, y, z),
                        alpha);
                }
                else if (controlValue < (this.UpperBound - this.EdgeFalloff))
                {
                    // The output value from the control module is within the selector
                    // threshold; return the output value from the second source module.

                    return this.GetSourceModule(1).GetValue(x, y, z);
                }
                else if (controlValue < (this.UpperBound + this.EdgeFalloff))
                {
                    // The output value from the control module is near the upper end of the
                    // selector threshold and within the smooth curve. Interpolate between
                    // the output values from the first and second source modules.

                    float lowerCurve = (this.UpperBound - this.EdgeFalloff);
                    float upperCurve = (this.UpperBound + this.EdgeFalloff);

                    float alpha = Interpolation.SCurve3((controlValue - lowerCurve) / (upperCurve - lowerCurve));

                    return Interpolation.Linear(
                        this.GetSourceModule(1).GetValue(x, y, z),
                        this.GetSourceModule(0).GetValue(x, y, z),
                        alpha);
                }
                else
                {
                    // Output value from the control module is above the selector threshold;
                    // return the output value from the first source module.

                    return this.GetSourceModule(0).GetValue(x, y, z);
                }
            }
            else
            {
                if (controlValue < this.LowerBound || controlValue > this.UpperBound)
                {
                    return this.GetSourceModule(0).GetValue(x, y, z);
                }
                else
                {
                    return this.GetSourceModule(1).GetValue(x, y, z);
                }
            }
        }

        public override string GetHlslBody(HlslContext context)
        {
            var sb = new StringBuilder();
            string module0 = context.GetModuleName(this.GetSourceModule(0));
            string module1 = context.GetModuleName(this.GetSourceModule(1));
            string controlModule = context.GetModuleName(this.GetSourceModule(2));

            sb.AppendTabFormatLine(context.GetModuleFunctionDefinition(this));
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "float controlValue = {0}(x, y, z);", controlModule);
            sb.AppendTabFormatLine();

            if (this.EdgeFalloff > 0.0f)
            {
                sb.AppendTabFormatLine(1, "if (controlValue < ({0} - {1}))", this.LowerBound, this.EdgeFalloff);
                sb.AppendTabFormatLine(1, "{");
                sb.AppendTabFormatLine(2, "return {0}(x, y, z);", module0);
                sb.AppendTabFormatLine(1, "}");
                sb.AppendTabFormatLine();
                sb.AppendTabFormatLine(1, "if (controlValue < ({0} + {1}))", this.LowerBound, this.EdgeFalloff);
                sb.AppendTabFormatLine(1, "{");
                sb.AppendTabFormatLine(2, "float lowerCurve = ({0} - {1});", this.LowerBound, this.EdgeFalloff);
                sb.AppendTabFormatLine(2, "float upperCurve = ({0} + {1});", this.LowerBound, this.EdgeFalloff);
                sb.AppendTabFormatLine(2, "float alpha = Interpolation_SCurve3((controlValue - lowerCurve) / (upperCurve - lowerCurve));");
                sb.AppendTabFormatLine(2, "return Interpolation_Linear({0}(x, y, z), {1}(x, y, z), alpha);", module0, module1);
                sb.AppendTabFormatLine(1, "}");
                sb.AppendTabFormatLine();
                sb.AppendTabFormatLine(1, "if (controlValue < ({0} - {1}))", this.UpperBound, this.EdgeFalloff);
                sb.AppendTabFormatLine(1, "{");
                sb.AppendTabFormatLine(2, "return {0}(x, y, z);", module1);
                sb.AppendTabFormatLine(1, "}");
                sb.AppendTabFormatLine();
                sb.AppendTabFormatLine(1, "if (controlValue < ({0} + {1}))", this.UpperBound, this.EdgeFalloff);
                sb.AppendTabFormatLine(1, "{");
                sb.AppendTabFormatLine(2, "float lowerCurve = ({0} - {1});", this.UpperBound, this.EdgeFalloff);
                sb.AppendTabFormatLine(2, "float upperCurve = ({0} + {1});", this.UpperBound, this.EdgeFalloff);
                sb.AppendTabFormatLine(2, "float alpha = Interpolation_SCurve3((controlValue - lowerCurve) / (upperCurve - lowerCurve));");
                sb.AppendTabFormatLine(2, "return Interpolation_Linear({0}(x, y, z), {1}(x, y, z), alpha);", module1, module0);
                sb.AppendTabFormatLine(1, "}");
                sb.AppendTabFormatLine();
                sb.AppendTabFormatLine(1, "return {0}(x, y, z);", module0);
            }
            else
            {
                sb.AppendTabFormatLine(1, "if (controlValue < {0} || controlValue > {1})", this.LowerBound, this.UpperBound);
                sb.AppendTabFormatLine(1, "{");
                sb.AppendTabFormatLine(2, "return {0}(x, y, z);", module0);
                sb.AppendTabFormatLine(1, "}");
                sb.AppendTabFormatLine();
                sb.AppendTabFormatLine(1, "return {0}(x, y, z);", module1);
            }

            sb.AppendTabFormatLine("}");

            return sb.ToString();
        }

        public override string GetCSharpBody(CSharpContext context)
        {
            var sb = new StringBuilder();
            string module0 = context.GetModuleName(this.GetSourceModule(0));
            string module1 = context.GetModuleName(this.GetSourceModule(1));
            string controlModule = context.GetModuleName(this.GetSourceModule(2));
            string name = context.GetModuleName(this);
            string type = context.GetModuleType(this);

            sb.AppendTabFormatLine("{0} {1} = new({2}, {3}, {4})", type, name, module0, module1, controlModule);
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "EdgeFalloff = {0},", this.EdgeFalloff);
            sb.AppendTabFormatLine("};");
            sb.AppendTabFormatLine("{0}.SetBounds({1}, {2});", name, this.LowerBound, this.UpperBound);

            return sb.ToString();
        }

        private void UpdateEdgeFalloff(float edgeFalloffValue)
        {
            // Make sure that the edge falloff curves do not overlap.
            float boundSize = Math.Abs(this.upperBound - this.lowerBound) * 0.5f;
            this.edgeFalloff = (edgeFalloffValue > boundSize) ? boundSize : edgeFalloffValue;
        }

        public void SetBounds(float lowerBound, float upperBound)
        {
            this.lowerBound = lowerBound;
            this.upperBound = upperBound;
            this.UpdateEdgeFalloff(this.edgeFalloff);
        }
    }
}
