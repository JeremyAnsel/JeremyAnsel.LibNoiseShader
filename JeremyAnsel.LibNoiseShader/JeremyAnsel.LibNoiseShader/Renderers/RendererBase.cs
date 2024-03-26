using System;
using System.Drawing;
using System.Text;

namespace JeremyAnsel.LibNoiseShader.Renderers
{
    public abstract class RendererBase : IRenderer
    {
        private IRenderer[] sourceRenderers;

        protected RendererBase()
        {
        }

        public string Name { get; set; }

        public abstract int RequiredSourceRendererCount { get; }

        public virtual void SetSeed(int seed)
        {
            for (int i = 0; i < this.RequiredSourceRendererCount; i++)
            {
                this.GetSourceRenderer(i)?.SetSeed(seed);
            }
        }

        public IRenderer GetSourceRenderer(int index)
        {
            if (this.sourceRenderers is null)
            {
                return null;
            }

            return this.sourceRenderers[index];
        }

        protected void SetSourceRenderer(int index, IRenderer source)
        {
            if (index < 0 || index >= this.RequiredSourceRendererCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (this.sourceRenderers is null)
            {
                this.sourceRenderers = new IRenderer[this.RequiredSourceRendererCount];
            }

            this.sourceRenderers[index] = source;
        }

        public virtual void GenerateModuleContext(HlslContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            for (int i = 0; i < this.RequiredSourceRendererCount; i++)
            {
                context.AddRenderer(this.GetSourceRenderer(i));
            }
        }

        public virtual void GenerateModuleContext(CSharpContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            for (int i = 0; i < this.RequiredSourceRendererCount; i++)
            {
                context.AddRenderer(this.GetSourceRenderer(i));
            }
        }

        public abstract Color GetColor(float x, float y, int width, int height);

        public abstract string GetHlslBody(HlslContext context);

        public string GetFullHlsl()
        {
            var sb = new StringBuilder();
            var context = new HlslContext();

            sb.AppendLine(HlslContext.GetHeader());
            sb.AppendLine(context.GetFullBody(this));

            return sb.ToString();
        }

        public abstract string GetCSharpBody(CSharpContext context);

        public string GetFullCSharp()
        {
            var sb = new StringBuilder();
            var context = new CSharpContext();

            sb.AppendLine(CSharpContext.GetHeader());
            sb.AppendLine(context.GetFullBody(this));

            return sb.ToString();
        }
    }
}
