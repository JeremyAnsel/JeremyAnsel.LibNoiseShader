using System.Text;

namespace JeremyAnsel.LibNoiseShader.Modules
{
    public sealed class CacheModule : ModuleBase
    {
        private bool isCached;

        private float cachedValue;

        private float cachedX;

        private float cachedY;

        private float cachedZ;

        public CacheModule(IModule module0)
        {
            this.isCached = false;
            this.SetSourceModule(0, module0);
        }

        public override int RequiredSourceModuleCount => 1;

        public override float GetValue(float x, float y, float z)
        {
            IModule module0 = this.GetSourceModule(0);

            if (!this.isCached || x != this.cachedX || y != this.cachedY || z != this.cachedZ)
            {
                this.cachedValue = module0.GetValue(x, y, z);
                this.cachedX = x;
                this.cachedY = y;
                this.cachedZ = z;
                this.isCached = true;
            }

            return this.cachedValue;
        }

        public override string GetHlslBody(HlslContext context)
        {
            var sb = new StringBuilder();
            string module0 = context.GetModuleName(this.GetSourceModule(0));

            sb.AppendTabFormatLine(context.GetModuleFunctionDefinition(this));
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "static bool isCached = false;");
            sb.AppendTabFormatLine(1, "static float cachedValue = 0;");
            sb.AppendTabFormatLine(1, "static float cachedX = 0;");
            sb.AppendTabFormatLine(1, "static float cachedY = 0;");
            sb.AppendTabFormatLine(1, "static float cachedZ = 0;");
            sb.AppendLine();
            sb.AppendTabFormatLine(1, "[branch] if (!isCached || x != cachedX || y != cachedY || z != cachedZ)");
            sb.AppendTabFormatLine(1, "{");
            sb.AppendTabFormatLine(2, "cachedValue = {0}(x, y, z);", module0);
            sb.AppendTabFormatLine(2, "cachedX = x;");
            sb.AppendTabFormatLine(2, "cachedY = y;");
            sb.AppendTabFormatLine(2, "cachedZ = z;");
            sb.AppendTabFormatLine(1, "}");
            sb.AppendLine();
            sb.AppendTabFormatLine(1, "return cachedValue;");
            sb.AppendTabFormatLine("}");

            return sb.ToString();
        }

        public override string GetCSharpBody(CSharpContext context)
        {
            var sb = new StringBuilder();
            string module0 = context.GetModuleName(this.GetSourceModule(0));
            string name = context.GetModuleName(this);
            string type = context.GetModuleType(this);

            sb.AppendTabFormatLine("{0} {1} = new({2});", type, name, module0);

            return sb.ToString();
        }
    }
}
