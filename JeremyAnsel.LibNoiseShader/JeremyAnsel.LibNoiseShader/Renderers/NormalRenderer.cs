using JeremyAnsel.LibNoiseShader.Builders;
using System;
using System.Drawing;
using System.Text;

namespace JeremyAnsel.LibNoiseShader.Renderers
{
    public sealed class NormalRenderer : RendererBase
    {
        private readonly IBuilder sourceBuilder;

        public NormalRenderer(IBuilder? builder)
        {
            this.sourceBuilder = builder ?? throw new ArgumentNullException(nameof(builder));
            this.BumpHeight = 1.0f;
            this.IsWrapEnabled = false;
        }

        public NormalRenderer(IBuilder? builder, float bumpHeight, bool isWrapEnabled)
        {
            this.sourceBuilder = builder ?? throw new ArgumentNullException(nameof(builder));
            this.BumpHeight = bumpHeight;
            this.IsWrapEnabled = isWrapEnabled;
        }

        public override int RequiredSourceRendererCount => 0;

        public override void SetSeed(int seed)
        {
            base.SetSeed(seed);
            this.sourceBuilder.Seed = seed;
        }

        public float BumpHeight { get; set; }

        public bool IsWrapEnabled { get; set; }

        public IBuilder GetSourceBuilder()
        {
            return this.sourceBuilder;
        }

        public override void GenerateModuleContext(HlslContext? context)
        {
            base.GenerateModuleContext(context);

            context!.AddBuilder(this.GetSourceBuilder());
        }

        public override void GenerateModuleContext(CSharpContext? context)
        {
            base.GenerateModuleContext(context);

            context!.AddBuilder(this.GetSourceBuilder());
        }

        public override Color GetColor(float x, float y, int width, int height)
        {
            float dx = 2.0f / width;
            float dy = 2.0f / height;
            float dx_half = dx * 0.5f;
            float dy_half = dy * 0.5f;

            // Calculate the positions of the current point's right and up
            // neighbors.
            float xRightOffset;
            float yUpOffset;

            if (this.IsWrapEnabled)
            {
                if (Math.Abs(1.0f - dx - x) < dx_half)
                {
                    xRightOffset = -(1.0f - dx);
                }
                else
                {
                    xRightOffset = dx;
                }

                if (Math.Abs(1.0f - dy - y) < dy_half)
                {
                    yUpOffset = -(1.0f - dy);
                }
                else
                {
                    yUpOffset = dy;
                }
            }
            else
            {
                if (Math.Abs(1.0f - dx - x) < dx_half)
                {
                    xRightOffset = 0.0f;
                }
                else
                {
                    xRightOffset = dx;
                }

                if (Math.Abs(1.0f - dy - y) < dy_half)
                {
                    yUpOffset = 0.0f;
                }
                else
                {
                    yUpOffset = dy;
                }
            }

            // Get the noise value of the current point in the source noise map
            // and the noise values of its right and up neighbors.
            float nc = this.GetSourceBuilder().GetValue(x, y);
            float nr = this.GetSourceBuilder().GetValue(x + xRightOffset, y);
            float nu = this.GetSourceBuilder().GetValue(x, y + yUpOffset);

            // Calculate the normal product.
            Color color = CalcNormalColor(nc, nr, nu, this.BumpHeight);
            return color;
        }

        private static Color CalcNormalColor(float nc, float nr, float nu, float bumpHeight)
        {
            // Calculate the surface normal.
            nc *= bumpHeight;
            nr *= bumpHeight;
            nu *= bumpHeight;
            float ncr = (nc - nr);
            float ncu = (nc - nu);
            float d = (float)Math.Sqrt((ncu * ncu) + (ncr * ncr) + 1);
            float vxc = (nc - nr) / d;
            float vyc = (nc - nu) / d;
            float vzc = 1.0f / d;

            // Map the normal range from the (-1.0 .. +1.0) range to the (0 .. 255) range.
            int xc = (byte)((uint)Math.Floor((vxc + 1.0f) * 127.5f) & 0xffu);
            int yc = (byte)((uint)Math.Floor((vyc + 1.0f) * 127.5f) & 0xffu);
            int zc = (byte)((uint)Math.Floor((vzc + 1.0f) * 127.5f) & 0xffu);

            return Color.FromArgb(xc, yc, zc);
        }

        public override string GetHlslBody(HlslContext context)
        {
            var sb = new StringBuilder();
            string builder0 = context.GetBuilderName(this.GetSourceBuilder());

            sb.AppendTabFormatLine(context.GetRendererFunctionDefinition(this));
            sb.AppendTabFormatLine(0, "{");

            //sb.AppendTabFormatLine(1, "float dx = 2.0f / width;");
            //sb.AppendTabFormatLine(1, "float dy = 2.0f / height;");
            sb.AppendTabFormatLine(1, "float dx = abs(ddx(x));");
            sb.AppendTabFormatLine(1, "float dy = abs(ddy(y));");
            sb.AppendTabFormatLine(1, "float dx_half = dx * 0.5f;");
            sb.AppendTabFormatLine(1, "float dy_half = dy * 0.5f;");
            sb.AppendTabFormatLine();

            // Calculate the positions of the current point's right and up
            // neighbors.
            sb.AppendTabFormatLine(1, "float xRightOffset;");
            sb.AppendTabFormatLine(1, "float yUpOffset;");
            sb.AppendTabFormatLine(1, "");

            if (this.IsWrapEnabled)
            {
                sb.AppendTabFormatLine(1, "float x1 = 1.0f - dx - x;");
                sb.AppendTabFormatLine(1, "if (x1 * sign(x1) < dx_half)");
                sb.AppendTabFormatLine(1, "{");
                sb.AppendTabFormatLine(2, "xRightOffset = -(1.0f - dx);");
                sb.AppendTabFormatLine(1, "}");
                sb.AppendTabFormatLine(1, "else");
                sb.AppendTabFormatLine(1, "{");
                sb.AppendTabFormatLine(2, "xRightOffset = dx;");
                sb.AppendTabFormatLine(1, "}");
                sb.AppendTabFormatLine();
                sb.AppendTabFormatLine(1, "float y1 = 1.0f - dy - y;");
                sb.AppendTabFormatLine(1, "if (y1 * sign(y1) < dy_half)");
                sb.AppendTabFormatLine(1, "{");
                sb.AppendTabFormatLine(2, "yUpOffset = -(1.0f - dy);");
                sb.AppendTabFormatLine(1, "}");
                sb.AppendTabFormatLine(1, "else");
                sb.AppendTabFormatLine(1, "{");
                sb.AppendTabFormatLine(2, "yUpOffset = dy;");
                sb.AppendTabFormatLine(1, "}");
            }
            else
            {
                sb.AppendTabFormatLine(1, "float x1 = 1.0f - dx - x;");
                sb.AppendTabFormatLine(1, "if (x1 * sign(x1) < dx_half)");
                sb.AppendTabFormatLine(1, "{");
                sb.AppendTabFormatLine(2, "xRightOffset = 0.0f;");
                sb.AppendTabFormatLine(1, "}");
                sb.AppendTabFormatLine(1, "else");
                sb.AppendTabFormatLine(1, "{");
                sb.AppendTabFormatLine(2, "xRightOffset = dx;");
                sb.AppendTabFormatLine(1, "}");
                sb.AppendTabFormatLine();
                sb.AppendTabFormatLine(1, "float y1 = 1.0f - dy - y;");
                sb.AppendTabFormatLine(1, "if (y1 * sign(y1) < dy_half)");
                sb.AppendTabFormatLine(1, "{");
                sb.AppendTabFormatLine(2, "yUpOffset = 0.0f;");
                sb.AppendTabFormatLine(1, "}");
                sb.AppendTabFormatLine(1, "else");
                sb.AppendTabFormatLine(1, "{");
                sb.AppendTabFormatLine(2, "yUpOffset = dy;");
                sb.AppendTabFormatLine(1, "}");
            }

            sb.AppendTabFormatLine();

            // Get the noise value of the current point in the source noise map
            // and the noise values of its right and up neighbors.
            sb.AppendTabFormatLine(1, "float nc = {0}(x, y);", builder0);
            sb.AppendTabFormatLine(1, "float nr = {0}(x + xRightOffset, y);", builder0);
            sb.AppendTabFormatLine(1, "float nu = {0}(x, y + yUpOffset);", builder0);

            // Calculate the surface normal.
            sb.AppendTabFormatLine(1, "nc *= {0};", this.BumpHeight);
            sb.AppendTabFormatLine(1, "nr *= {0};", this.BumpHeight);
            sb.AppendTabFormatLine(1, "nu *= {0};", this.BumpHeight);
            sb.AppendTabFormatLine(1, "float ncr = (nc - nr);");
            sb.AppendTabFormatLine(1, "float ncu = (nc - nu);");
            sb.AppendTabFormatLine(1, "float d = sqrt((ncu * ncu) + (ncr * ncr) + 1);");
            sb.AppendTabFormatLine(1, "float vxc = (nc - nr) / d;");
            sb.AppendTabFormatLine(1, "float vyc = (nc - nu) / d;");
            sb.AppendTabFormatLine(1, "float vzc = 1.0f / d;");

            //// Map the normal range from the (-1.0 .. +1.0) range to the (0 .. 255) range.
            sb.AppendTabFormatLine(1, "float xc = vxc * 0.5f + 0.5f;");
            sb.AppendTabFormatLine(1, "float yc = vyc * 0.5f + 0.5f;");
            sb.AppendTabFormatLine(1, "float zc = vzc * 0.5f + 0.5f;");
            sb.AppendTabFormatLine(1, "float4 color = float4(xc, yc, zc, 1.0f);");
            sb.AppendTabFormatLine(1, "return color;");
            sb.AppendTabFormatLine(0, "}");

            return sb.ToString();
        }

        public override string GetCSharpBody(CSharpContext context)
        {
            var sb = new StringBuilder();
            string builder0 = context.GetBuilderName(this.GetSourceBuilder());
            string name = context.GetRendererName(this);
            string type = context.GetRendererType(this);

            sb.AppendTabFormatLine(
                "{0} {1} = new({2}, {3}, {4});",
                type,
                name,
                builder0,
                this.BumpHeight,
                this.IsWrapEnabled);

            return sb.ToString();
        }
    }
}
