using JeremyAnsel.LibNoiseShader.Builders;
using JeremyAnsel.LibNoiseShader.Modules;
using JeremyAnsel.LibNoiseShader.Renderers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace JeremyAnsel.LibNoiseShader
{
    public sealed class CSharpContext
    {
        private readonly Dictionary<IModule, int> _modules = new();

        private readonly Dictionary<IBuilder, int> _builders = new();

        private readonly Dictionary<IRenderer, int> _renderers = new();

        public static string GetHeader()
        {
            var sb = new StringBuilder();

            sb.AppendLine("Noise3D noise = new(0);");

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
                sb.AppendLine(entry.Key.GetCSharpBody(this));
            }

            sb.AppendTabFormatLine("IModule Module_root = {0};", rootName);

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

        internal string GetModuleType(IModule module)
        {
            if (module is null)
            {
                throw new ArgumentNullException(nameof(module));
            }

            return module.GetType().Name;
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
                sb.AppendLine(entry.Key.GetCSharpBody(this));
            }

            sb.AppendLine(root.GetCSharpBody(this));

            sb.AppendTabFormatLine("IBuilder Builder_root = {0};", rootName);

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

        internal string GetBuilderType(IBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.GetType().Name;
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
                sb.AppendLine(entry.Key.GetCSharpBody(this));
            }

            foreach (KeyValuePair<IBuilder, int> entry in this._builders.OrderBy(t => t.Value))
            {
                sb.AppendLine(entry.Key.GetCSharpBody(this));
            }

            foreach (KeyValuePair<IRenderer, int> entry in this._renderers.OrderBy(t => t.Value))
            {
                sb.AppendLine(entry.Key.GetCSharpBody(this));
            }

            sb.AppendTabFormatLine("IRenderer Renderer_root = {0};", rootName);

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

        internal string GetRendererType(IRenderer renderer)
        {
            if (renderer is null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            return renderer.GetType().Name;
        }
    }
}
