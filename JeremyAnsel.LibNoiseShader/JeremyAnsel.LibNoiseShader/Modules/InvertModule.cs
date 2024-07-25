﻿using System.Text;

namespace JeremyAnsel.LibNoiseShader.Modules
{
    public sealed class InvertModule : ModuleBase
    {
        public InvertModule(IModule? module0)
        {
            this.SetSourceModule(0, module0);
        }

        public override int RequiredSourceModuleCount => 1;

        public override float GetValue(float x, float y, float z)
        {
            return -this.GetSourceModule(0)!.GetValue(x, y, z);
        }

        public override int EmitHlslMaxDepth()
        {
            return 1;
        }

        public override void EmitHlsl(HlslContext context)
        {
            this.GetSourceModule(0)!.EmitHlsl(context);
            context.EmitFunction(this, false);
        }

        public override void EmitHlslHeader(HlslContext context, StringBuilder header)
        {
        }

        public override bool HasHlslSettings()
        {
            return false;
        }

        public override void EmitHlslSettings(StringBuilder body)
        {
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
            body.AppendTabFormatLine(2, "result = -param0;");
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
