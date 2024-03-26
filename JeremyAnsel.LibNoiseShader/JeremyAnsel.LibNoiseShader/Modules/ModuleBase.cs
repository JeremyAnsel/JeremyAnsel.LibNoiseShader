using System;
using System.Collections.Generic;
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

        public string GetHlslBody(HlslContext context)
        {
            var sb = new StringBuilder();

            sb.AppendLine(this.EmitFullHlsl());

            return sb.ToString();
        }

        public string GetFullHlsl()
        {
            var sb = new StringBuilder();
            var context = new HlslContext();

            sb.AppendLine(HlslContext.GetHeader());
            sb.AppendLine(context.GetFullBody(this));

            return sb.ToString();
        }

        public abstract int EmitHlslMaxDepth();

        public abstract void EmitHlsl(HlslContext context);

        public abstract void EmitHlslHeader(HlslContext context, StringBuilder header);

        public abstract bool HasHlslSettings();

        public abstract void EmitHlslSettings(StringBuilder body);

        public abstract bool HasHlslCoords(int index);

        public abstract void EmitHlslCoords(StringBuilder body, int index);

        public abstract int GetHlslFunctionParametersCount();

        public abstract void EmitHlslFunction(StringBuilder body);

        public string EmitFullHlsl()
        {
            var context = new HlslContext();

            context.AddModule(this);
            this.EmitHlsl(context);

            var sb = new StringBuilder();

            sb.AppendTabFormatLine("float Module_root(float3 root_coords)");
            sb.AppendTabFormatLine("{");

            BuildHlslInstructions(sb, context.Instructions);

            int depth = this.EmitHlslMaxDepth() + 1;

            for (int i = 0; i < this.RequiredSourceModuleCount; i++)
            {
                depth += this.GetSourceModule(i).EmitHlslMaxDepth();
            }

            sb.AppendTabFormatLine("static const int modules_results_count = {0};", depth);
            sb.AppendTabFormatLine("static const int modules_coords_count = {0};", depth);
            sb.AppendTabFormatLine("float3 modules_coords[modules_coords_count];");
            sb.AppendTabFormatLine("int modules_coords_index = 0;");
            sb.AppendTabFormatLine("float modules_results[modules_results_count];");
            sb.AppendTabFormatLine("int modules_results_index = 0;");
            sb.AppendTabFormatLine("modules_coords[modules_coords_index++] = root_coords;");

            sb.AppendLine();
            sb.Append(context.Header);

            sb.AppendLine();
            sb.AppendTabFormatLine(0, "[fastopt]");
            sb.AppendTabFormatLine(0, "for (int instruction_index = 0; instruction_index < modules_instructions_count; instruction_index++)");
            sb.AppendTabFormatLine(0, "{");
            sb.AppendTabFormatLine(1, "int instruction = modules_instructions[instruction_index>>2][instruction_index&3];");
            sb.AppendTabFormatLine(1, "float result;");
            sb.AppendTabFormatLine(1, "float3 coords;");
            sb.AppendTabFormatLine();
            sb.AppendTabFormatLine(1, "[call]");
            sb.AppendTabFormatLine(1, "switch (instruction)");
            sb.AppendTabFormatLine(1, "{");
            sb.Append(context.Body);
            sb.AppendTabFormatLine(1, "}");
            sb.AppendTabFormatLine(0, "}");

            sb.AppendLine();
            sb.AppendTabFormatLine("float modules_result = modules_results[modules_results_index - 1];");
            sb.AppendTabFormatLine("return modules_result;");
            sb.AppendTabFormatLine("}");

            sb.AppendLine();
            sb.AppendTabFormatLine("float Module_root(float x, float y, float z)");
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "return Module_root(float3(x, y, z));");
            sb.AppendTabFormatLine("}");

            return sb.ToString();
        }

        private static void BuildHlslInstructions(StringBuilder sb, List<int> instructions)
        {
            sb.AppendTabFormatLine(0, "static const int modules_instructions_count = {0};", instructions.Count);

            if (instructions.Count % 4 != 0)
            {
                int padding = 4 - (instructions.Count % 4);
                for (int i = 0; i < padding; i++)
                {
                    instructions.Add(0);
                }
            }

            sb.AppendTabFormatLine(0, "int4 modules_instructions[] =");
            sb.AppendTabFormatLine(0, "{");

            for (int index = 0; index < instructions.Count; index += 4)
            {
                sb.AppendTabFormatLine("int4({0},{1},{2},{3}),", instructions[index], instructions[index + 1], instructions[index + 2], instructions[index + 3]);
            }

            sb.AppendTabFormatLine(0, "};");
            sb.AppendLine();
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
