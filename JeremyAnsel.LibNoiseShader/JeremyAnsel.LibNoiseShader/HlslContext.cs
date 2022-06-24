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
        private readonly Dictionary<IModule, int> _modules = new();

        private readonly Dictionary<IBuilder, int> _builders = new();

        private readonly Dictionary<IRenderer, int> _renderers = new();

        public static string GetHeader()
        {
            var sb = new StringBuilder();

            sb.Append(Noise3D.HeaderHlsl());

            sb.Append(Interpolation.CubicHlsl());
            sb.Append(Interpolation.LinearHlsl());
            sb.Append(Interpolation.SCurve3Hlsl());
            sb.Append(Interpolation.SCurve5Hlsl());

            sb.Append(Noise3D.PermHlsl());
            sb.Append(Noise3D.Perm2DHlsl());
            sb.Append(Noise3D.IntValueHlsl());
            sb.Append(Noise3D.GradPermHlsl());
            sb.Append(Noise3D.GradientCoherentHlsl());

            sb.Append(LatLon.ToXYZHlsl());

            sb.Append(CylinderModel.GetHlsl());
            sb.Append(PlaneModel.GetHlsl());
            sb.Append(SphereModel.GetHlsl());

            return sb.ToString();
        }

        public string GetFullBody(IModule root)
        {
            if (root is null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            string rootName = this.GetModuleName(root);

            var sb = new StringBuilder();

            foreach (KeyValuePair<IModule, int> entry in this._modules.OrderBy(t => t.Value))
            {
                sb.AppendLine(entry.Key.GetHlslBody(this));
            }

            sb.AppendTabFormatLine("float Module_root(float x, float y, float z)");
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "return {0}( x, y, z );", rootName);
            sb.AppendTabFormatLine("}");
            sb.AppendLine();
            sb.AppendTabFormatLine("float Module_root(float3 coords)");
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "return {0}( coords.x, coords.y, coords.z );", rootName);
            sb.AppendTabFormatLine("}");

            return sb.ToString();
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

        internal string GetModuleName(IModule module)
        {
            if (module is null)
            {
                throw new ArgumentNullException(nameof(module));
            }

            this.AddModule(module);

            int index = this._modules[module];
            return string.Format(CultureInfo.InvariantCulture, "Module_{0}_{1}_{2}", index, module.GetType().Name, module.Name);
        }

        internal string GetModuleFunctionDefinition(IModule module)
        {
            if (module is null)
            {
                throw new ArgumentNullException(nameof(module));
            }

            string name = this.GetModuleName(module);
            return string.Format(CultureInfo.InvariantCulture, "float {0}(float x, float y, float z)", name);
        }

        public string GetFullBody(IBuilder root)
        {
            if (root is null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            string rootName = this.GetBuilderName(root);

            var sb = new StringBuilder();

            foreach (KeyValuePair<IModule, int> entry in this._modules.OrderBy(t => t.Value))
            {
                sb.AppendLine(entry.Key.GetHlslBody(this));
            }

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

            foreach (KeyValuePair<IModule, int> entry in this._modules.OrderBy(t => t.Value))
            {
                sb.AppendLine(entry.Key.GetHlslBody(this));
            }

            foreach (KeyValuePair<IBuilder, int> entry in this._builders.OrderBy(t => t.Value))
            {
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
