using System;
using System.Drawing;
using System.Text;

namespace JeremyAnsel.LibNoiseShader.Renderers
{
    public sealed class BlendRenderer : RendererBase
    {
        public BlendRenderer(IRenderer renderer0, IRenderer renderer1)
        {
            this.SetSourceRenderer(0, renderer0);
            this.SetSourceRenderer(1, renderer1);
        }

        public override int RequiredSourceRendererCount => 2;

        public override void SetSeed(int seed)
        {
            base.SetSeed(seed);
        }

        public override Color GetColor(float x, float y, int width, int height)
        {
            Color rendererColor = this.GetSourceRenderer(0).GetColor(x, y, width, height);
            Color backgroundColor = this.GetSourceRenderer(1).GetColor(x, y, width, height);

            Color color = Color.FromArgb(
                Math.Max(rendererColor.A, backgroundColor.A),
                Interpolation.Linear(backgroundColor.R, rendererColor.R, rendererColor.A),
                Interpolation.Linear(backgroundColor.G, rendererColor.G, rendererColor.A),
                Interpolation.Linear(backgroundColor.B, rendererColor.B, rendererColor.A));

            return color;
        }

        public override string GetHlslBody(HlslContext context)
        {
            var sb = new StringBuilder();
            string renderer0 = context.GetRendererName(this.GetSourceRenderer(0));
            string renderer1 = context.GetRendererName(this.GetSourceRenderer(1));

            sb.AppendTabFormatLine(context.GetRendererFunctionDefinition(this));
            sb.AppendTabFormatLine(0, "{");
            //sb.AppendTabFormatLine(1, "float4 color0 = {0}( x, y, width, height );", renderer0);
            //sb.AppendTabFormatLine(1, "float4 color1 = {0}( x, y, width, height );", renderer1);
            sb.AppendTabFormatLine(1, "float4 color0 = {0}( x, y );", renderer0);
            sb.AppendTabFormatLine(1, "float4 color1 = {0}( x, y );", renderer1);
            sb.AppendTabFormatLine(1, "float3 colorXYZ = Interpolation_Linear( color1.xyz, color0.xyz, color0.www );");
            sb.AppendTabFormatLine(1, "float colorW = max(color0.w, color1.w);");
            sb.AppendTabFormatLine(1, "return float4(colorXYZ, colorW);");
            sb.AppendTabFormatLine(0, "}");

            return sb.ToString();
        }

        public override string GetCSharpBody(CSharpContext context)
        {
            var sb = new StringBuilder();
            string renderer0 = context.GetRendererName(this.GetSourceRenderer(0));
            string renderer1 = context.GetRendererName(this.GetSourceRenderer(1));
            string name = context.GetRendererName(this);
            string type = context.GetRendererType(this);

            sb.AppendTabFormatLine("{0} {1} = new({2}, {3});", type, name, renderer0, renderer1);

            return sb.ToString();
        }
    }
}
