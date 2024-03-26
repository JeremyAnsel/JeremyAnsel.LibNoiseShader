using JeremyAnsel.LibNoiseShader.Builders;
using JeremyAnsel.LibNoiseShader.Models;
using JeremyAnsel.LibNoiseShader.Modules;
using JeremyAnsel.LibNoiseShader.Renderers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace JeremyAnsel.LibNoiseShader
{
    public sealed class HlslContext
    {
        private readonly Dictionary<object, int> _objectsHeader = new();

        private readonly Dictionary<object, int> _objectsBody = new();

        private readonly Dictionary<IModule, int> _modules = new();

        private readonly Dictionary<IBuilder, int> _builders = new();

        private readonly Dictionary<IRenderer, int> _renderers = new();

        public readonly List<int> Instructions = new();

        public readonly StringBuilder Header = new();

        public readonly StringBuilder Body = new();

        public static string GetHeader()
        {
            var sb = new StringBuilder();

            sb.Append(Interpolation.CubicHlsl());
            sb.Append(Interpolation.LinearHlsl());
            sb.Append(Interpolation.SCurve3Hlsl());
            sb.Append(Interpolation.SCurve5Hlsl());

            sb.Append(Noise3D.IntValueHlsl());
            sb.Append(Noise3D.GradientCoherentHlsl());

            sb.Append(LatLon.ToXYZHlsl());

            sb.Append(CylinderModel.GetHlsl());
            sb.Append(PlaneModel.GetHlsl());
            sb.Append(SphereModel.GetHlsl());

            return sb.ToString();
        }

        private int GetOrAddObjectHeader(object obj, out bool isNew)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            isNew = !this._objectsHeader.ContainsKey(obj);

            if (isNew)
            {
                this._objectsHeader.Add(obj, this._objectsHeader.Count + 1);
            }

            return this._objectsHeader[obj];
        }

        private int GetOrAddObjectBody(object obj, out bool isNew)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            isNew = !this._objectsBody.ContainsKey(obj);

            if (isNew)
            {
                this._objectsBody.Add(obj, this._objectsBody.Count + 1);
            }

            return this._objectsBody[obj];
        }


        public int GetTraverseValue<T>(IModule root, Func<T, int> getValue, Func<int, int, int> combineValue) where T : IModule
        {
            if (root is null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            if (getValue is null)
            {
                throw new ArgumentNullException(nameof(getValue));
            }

            if (combineValue is null)
            {
                throw new ArgumentNullException(nameof(combineValue));
            }

            this.AddModule(root);

            int result = 0;

            foreach (T module in this._modules.Keys.OfType<T>())
            {
                int value = getValue(module);
                result = combineValue(result, value);
            }

            return result;
        }

        public string GetFullBody(IModule root)
        {
            if (root is null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            return root.EmitFullHlsl();
        }

        public void EmitHeader(IModule module)
        {
            int objectIndex = this.GetOrAddObjectHeader((module.GetType(), -1), out bool isNew);

            if (!isNew)
            {
                return;
            }

            module.EmitHlslHeader(this, this.Header);
        }

        public void EmitSettings(IModule module)
        {
            if (!module.HasHlslSettings())
            {
                return;
            }

            var sb = new StringBuilder();
            module.EmitHlslSettings(sb);

            int objectIndex = this.GetOrAddObjectBody(sb.ToString(), out bool isNew);
            this.Instructions.Add(objectIndex);

            if (!isNew)
            {
                return;
            }

            this.Body.AppendTabFormatLine(1, "case {0}:", objectIndex);
            module.EmitHlslSettings(this.Body);
            this.Body.AppendTabFormatLine(2, "break;");
        }

        public void EmitCoords(IModule module, int index, bool hasPreviousCoords)
        {
            if (!module.HasHlslCoords(index))
            {
                return;
            }

            var sb = new StringBuilder();
            module.EmitHlslCoords(sb, index);
            int objectIndex = this.GetOrAddObjectBody((sb.ToString(), index), out bool isNew);

            this.Instructions.Add(objectIndex);

            if (!isNew)
            {
                return;
            }

            this.Body.AppendTabFormatLine(1, "case {0}:", objectIndex);

            if (hasPreviousCoords)
            {
                this.Body.AppendTabFormatLine(2, "modules_coords_index--;");
            }

            this.Body.AppendTabFormatLine(2, "{");
            this.Body.AppendTabFormatLine(2, "coords = modules_coords[modules_coords_index - 1];");
            module.EmitHlslCoords(this.Body, index);
            this.Body.AppendTabFormatLine(2, "modules_coords[modules_coords_index++] = coords;");
            this.Body.AppendTabFormatLine(2, "}");

            this.Body.AppendTabFormatLine(2, "break;");
        }

        public void EmitFunction(IModule module, bool hasPreviousCoords)
        {
            int objectIndex = this.GetOrAddObjectBody(module.GetType(), out bool isNew);
            this.Instructions.Add(objectIndex);

            if (!isNew)
            {
                return;
            }

            this.Body.AppendTabFormatLine(1, "case {0}:", objectIndex);
            this.Body.AppendTabFormatLine(1, "{");

            if (hasPreviousCoords)
            {
                this.Body.AppendTabFormatLine(2, "modules_coords_index--;");
            }

            int parametersCount = module.GetHlslFunctionParametersCount();

            if (parametersCount > 0)
            {
                this.Body.AppendTabFormatLine(2, "modules_results_index -= {0};", parametersCount);
            }

            this.Body.AppendTabFormatLine(2, "float3 p = modules_coords[modules_coords_index - 1];");

            for (int index = 0; index < parametersCount; index++)
            {
                if (index == 0)
                {
                    this.Body.AppendTabFormatLine(2, "float param{0} = modules_results[modules_results_index];", index);
                }
                else
                {
                    this.Body.AppendTabFormatLine(2, "float param{0} = modules_results[modules_results_index + {0}];", index);
                }
            };

            module.EmitHlslFunction(this.Body);

            this.Body.AppendTabFormatLine(2, "modules_results[modules_results_index++] = result;");
            this.Body.AppendTabFormatLine(2, "break;");
            this.Body.AppendTabFormatLine(1, "}");
        }

        internal void AddModule(IModule module)
        {
            if (module is null)
            {
                throw new ArgumentNullException(nameof(module));
            }

            if (!this._modules.ContainsKey(module))
            {
                module.GenerateModuleContext(this);
                this._modules.Add(module, this._modules.Count + 1);
            }
        }

        //internal string GetModuleName(IModule module)
        //{
        //    if (module is null)
        //    {
        //        throw new ArgumentNullException(nameof(module));
        //    }

        //    this.AddModule(module);

        //    int index = this._modules[module];
        //    return string.Format(CultureInfo.InvariantCulture, "Module_{0}_{1}_{2}", index, module.GetType().Name, module.Name);
        //}

        //internal string GetModuleFunctionDefinition(IModule module)
        //{
        //    if (module is null)
        //    {
        //        throw new ArgumentNullException(nameof(module));
        //    }

        //    string name = this.GetModuleName(module);
        //    return string.Format(CultureInfo.InvariantCulture, "float {0}(float x, float y, float z)", name);
        //}

        public string GetFullBody(IBuilder root)
        {
            if (root is null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            string rootName = this.GetBuilderName(root);

            var sb = new StringBuilder();

            //foreach (KeyValuePair<IModule, int> entry in this._modules.OrderBy(t => t.Value))
            //{
            //    sb.AppendLine(entry.Key.GetHlslBody(this));
            //}

            string moduleName = rootName + "_Module";
            var module = root.GetSourceModule();
            sb.Append(module.EmitFullHlsl().Replace("Module_root", moduleName));

            sb.AppendLine(root.GetHlslBody(this));

            sb.AppendTabFormatLine("float Builder_root(float x, float y)");
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "return {0}( x, y );", rootName);
            sb.AppendTabFormatLine("}");

            return sb.ToString();
        }

        internal void AddBuilder(IBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (!this._builders.ContainsKey(builder))
            {
                builder.GenerateModuleContext(this);
                this._builders.Add(builder, this._builders.Count + 1);
            }
        }

        internal string GetBuilderName(IBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            this.AddBuilder(builder);

            int index = this._builders[builder];
            return string.Format(CultureInfo.InvariantCulture, "Builder_{0}_{1}_{2}", index, builder.GetType().Name, builder.Name);
        }

        internal string GetBuilderFunctionDefinition(IBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            string name = this.GetBuilderName(builder);
            return string.Format(CultureInfo.InvariantCulture, "float {0}(float x, float y)", name);
        }

        public string GetFullBody(IRenderer root)
        {
            if (root is null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            string rootName = this.GetRendererName(root);

            var sb = new StringBuilder();

            //foreach (KeyValuePair<IModule, int> entry in this._modules.OrderBy(t => t.Value))
            //{
            //    sb.AppendLine(entry.Key.GetHlslBody(this));
            //}

            foreach (KeyValuePair<IBuilder, int> entry in this._builders.OrderBy(t => t.Value))
            {
                string moduleName = this.GetBuilderName(entry.Key) + "_Module";
                var module = entry.Key.GetSourceModule();
                sb.Append(module.EmitFullHlsl().Replace("Module_root", moduleName));

                sb.AppendLine(entry.Key.GetHlslBody(this));
            }

            foreach (KeyValuePair<IRenderer, int> entry in this._renderers.OrderBy(t => t.Value))
            {
                sb.AppendLine(entry.Key.GetHlslBody(this));
            }

            //sb.AppendTabFormatLine("float4 Renderer_root(float x, float y, int width, int height)");
            sb.AppendTabFormatLine("float4 Renderer_root(float x, float y)");
            sb.AppendTabFormatLine("{");
            //sb.AppendTabFormatLine(1, "return {0}( x, y, width, height );", rootName);
            sb.AppendTabFormatLine(1, "return {0}( x, y );", rootName);
            sb.AppendTabFormatLine("}");

            return sb.ToString();
        }

        internal void AddRenderer(IRenderer renderer)
        {
            if (renderer is null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            if (!this._renderers.ContainsKey(renderer))
            {
                renderer.GenerateModuleContext(this);
                this._renderers.Add(renderer, this._renderers.Count + 1);
            }
        }

        internal string GetRendererName(IRenderer renderer)
        {
            if (renderer is null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            this.AddRenderer(renderer);

            int index = this._renderers[renderer];
            return string.Format(CultureInfo.InvariantCulture, "Renderer_{0}_{1}_{2}", index, renderer.GetType().Name, renderer.Name);
        }

        internal string GetRendererFunctionDefinition(IRenderer renderer)
        {
            if (renderer is null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            string name = this.GetRendererName(renderer);
            //return string.Format(CultureInfo.InvariantCulture, "float4 {0}(float x, float y, int width, int height)", name);
            return string.Format(CultureInfo.InvariantCulture, "float4 {0}(float x, float y)", name);
        }
    }
}
