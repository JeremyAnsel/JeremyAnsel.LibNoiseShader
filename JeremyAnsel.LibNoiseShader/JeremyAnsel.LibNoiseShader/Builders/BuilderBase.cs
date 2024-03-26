using JeremyAnsel.LibNoiseShader.Modules;
using System;
using System.Text;

namespace JeremyAnsel.LibNoiseShader.Builders
{
    public abstract class BuilderBase : IBuilder
    {
        private readonly IModule source;

        protected BuilderBase(IModule source, int seed)
        {
            this.source = source ?? throw new ArgumentNullException(nameof(source));
            this.Seed = seed;
        }

        public string Name { get; set; }

        public int Seed { get; set; }

        public IModule GetSourceModule()
        {
            return this.source;
        }

        public virtual void GenerateModuleContext(HlslContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.AddModule(this.GetSourceModule());
        }

        public virtual void GenerateModuleContext(CSharpContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.AddModule(this.GetSourceModule());
        }

        public abstract float GetValue(float x, float y);

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
