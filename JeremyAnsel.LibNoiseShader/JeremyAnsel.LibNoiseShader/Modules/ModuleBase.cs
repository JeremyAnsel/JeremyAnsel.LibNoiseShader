using System;
using System.Text;
using System.Text.RegularExpressions;

namespace JeremyAnsel.LibNoiseShader.Modules
{
    public abstract class ModuleBase : IModule
    {
        private IModule[] sourceModules;

        private string _name;

        public string Name
        {
            get => _name;

            set
            {
                if (value is null)
                {
                    _name = null;
                }
                else
                {
                    _name = Regex.Replace(value, "[^a-zA-Z0-9_]", string.Empty, RegexOptions.CultureInvariant);
                }
            }
        }

        public abstract int RequiredSourceModuleCount { get; }

        public IModule GetSourceModule(int index)
        {
            if (this.sourceModules is null)
            {
                return null;
            }

            return this.sourceModules[index];
        }

        protected void SetSourceModule(int index, IModule source)
        {
            if (index < 0 || index >= this.RequiredSourceModuleCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (this.sourceModules is null)
            {
                this.sourceModules = new IModule[this.RequiredSourceModuleCount];
            }

            this.sourceModules[index] = source;
        }

        public virtual void GenerateModuleContext(HlslContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            for (int i = 0; i < this.RequiredSourceModuleCount; i++)
            {
                context.AddModule(this.GetSourceModule(i));
            }
        }

        public virtual void GenerateModuleContext(CSharpContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            for (int i = 0; i < this.RequiredSourceModuleCount; i++)
            {
                context.AddModule(this.GetSourceModule(i));
            }
        }

        public abstract float GetValue(float x, float y, float z);

        public float GetValue(Float3 coords)
        {
            return this.GetValue(coords.X, coords.Y, coords.Z);
        }

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
