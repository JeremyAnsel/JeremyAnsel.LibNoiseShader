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

        public void SetBounds(float lowerBound, float upperBound)
        {
            this.lowerBound = lowerBound;
            this.upperBound = upperBound;
            this.UpdateEdgeFalloff(this.edgeFalloff);
        }

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

        public override int EmitHlslMaxDepth()
        {
            return 3;
        }

        public override void EmitHlsl(HlslContext context)
        {
            context.EmitHeader(this);
            this.GetSourceModule(0).EmitHlsl(context);
            this.GetSourceModule(1).EmitHlsl(context);
            this.GetSourceModule(2).EmitHlsl(context);
            context.EmitSettings(this);
            context.EmitFunction(this, false);
        }

        public override void EmitHlslHeader(HlslContext context, StringBuilder header)
        {
            string key = nameof(SelectorModule);

            header.AppendTabFormatLine("float {0}_EdgeFalloff = 0.0f;", key);
            header.AppendTabFormatLine("float {0}_LowerBound = -1.0f;", key);
            header.AppendTabFormatLine("float {0}_UpperBound = 1.0f;", key);
        }

        public override bool HasHlslSettings()
        {
            return true;
        }

        public override void EmitHlslSettings(StringBuilder body)
        {
            string key = nameof(SelectorModule);

            body.AppendTabFormatLine(2, "{0}_EdgeFalloff = {1};", key, this.EdgeFalloff);
            body.AppendTabFormatLine(2, "{0}_LowerBound = {1};", key, this.LowerBound);
            body.AppendTabFormatLine(2, "{0}_UpperBound = {1};", key, this.UpperBound);
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
            return 3;
        }

        public override void EmitHlslFunction(StringBuilder body)
        {
            string key = nameof(SelectorModule);

            body.AppendTabFormatLine(2, "float controlValue = param2;");
            body.AppendTabFormatLine();
            body.AppendTabFormatLine(2, "[branch] if ({0}_EdgeFalloff > 0.0f)", key);
            body.AppendTabFormatLine(2, "{");
            body.AppendTabFormatLine(3, "[branch] if (controlValue < ({0}_LowerBound - {0}_EdgeFalloff))", key);
            body.AppendTabFormatLine(3, "{");
            body.AppendTabFormatLine(4, "result = param0;");
            body.AppendTabFormatLine(3, "}");
            body.AppendTabFormatLine(3, "else");
            body.AppendTabFormatLine(3, "[branch] if (controlValue < ({0}_LowerBound + {0}_EdgeFalloff))", key);
            body.AppendTabFormatLine(3, "{");
            body.AppendTabFormatLine(4, "float lowerCurve = ({0}_LowerBound - {0}_EdgeFalloff);", key);
            body.AppendTabFormatLine(4, "float upperCurve = ({0}_LowerBound + {0}_EdgeFalloff);", key);
            body.AppendTabFormatLine(4, "float alpha = Interpolation_SCurve3((controlValue - lowerCurve) / (upperCurve - lowerCurve));");
            body.AppendTabFormatLine(4, "result = Interpolation_Linear(param0, param1, alpha);");
            body.AppendTabFormatLine(3, "}");
            body.AppendTabFormatLine(3, "else");
            body.AppendTabFormatLine(3, "[branch] if (controlValue < ({0}_UpperBound - {0}_EdgeFalloff))", key);
            body.AppendTabFormatLine(3, "{");
            body.AppendTabFormatLine(4, "result = param1;");
            body.AppendTabFormatLine(3, "}");
            body.AppendTabFormatLine(3, "else");
            body.AppendTabFormatLine(3, "[branch] if (controlValue < ({0}_UpperBound + {0}_EdgeFalloff))", key);
            body.AppendTabFormatLine(3, "{");
            body.AppendTabFormatLine(4, "float lowerCurve = ({0}_UpperBound - {0}_EdgeFalloff);", key);
            body.AppendTabFormatLine(4, "float upperCurve = ({0}_UpperBound + {0}_EdgeFalloff);", key);
            body.AppendTabFormatLine(4, "float alpha = Interpolation_SCurve3((controlValue - lowerCurve) / (upperCurve - lowerCurve));");
            body.AppendTabFormatLine(4, "result = Interpolation_Linear(param1, param0, alpha);");
            body.AppendTabFormatLine(3, "}");
            body.AppendTabFormatLine(3, "else");
            body.AppendTabFormatLine(3, "{");
            body.AppendTabFormatLine(3, "result = param0;");
            body.AppendTabFormatLine(3, "}");
            body.AppendTabFormatLine(2, "}");
            body.AppendTabFormatLine(2, "else");
            body.AppendTabFormatLine(2, "{");
            body.AppendTabFormatLine(3, "if (controlValue < {0}_LowerBound || controlValue > {0}_UpperBound)", key);
            body.AppendTabFormatLine(3, "{");
            body.AppendTabFormatLine(4, "result = param0;");
            body.AppendTabFormatLine(3, "}");
            body.AppendTabFormatLine(3, "else");
            body.AppendTabFormatLine(3, "{");
            body.AppendTabFormatLine(4, "result = param1;");
            body.AppendTabFormatLine(3, "}");
            body.AppendTabFormatLine(2, "}");
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
    }
}
