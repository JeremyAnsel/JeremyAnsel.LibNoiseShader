using JeremyAnsel.LibNoiseShader.IO.FileBuilders;
using JeremyAnsel.LibNoiseShader.IO.FileModules;
using JeremyAnsel.LibNoiseShader.IO.FileRenderers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JeremyAnsel.LibNoiseShader.IO
{
    public sealed class LibNoiseShaderFileContext
    {
        private readonly Dictionary<IFileModule, int> _modules = new();

        private readonly Dictionary<IFileBuilder, int> _builders = new();

        private readonly Dictionary<IFileRenderer, int> _renderers = new();

        internal int AddModule(IFileModule? module)
        {
            if (module == null)
            {
                return -1;
            }

            if (!_modules.ContainsKey(module))
            {
                _modules.Add(module, _modules.Count);
            }

            return _modules[module];
        }

        internal int GetModuleIndex(IFileModule? module)
        {
            if (module == null)
            {
                return -1;
            }

            if (!_modules.TryGetValue(module, out int value))
            {
                return -1;
            }

            return value;
        }

        internal IFileModule? GetModule(int index)
        {
            if (index == -1)
            {
                return null;
            }

            if (index < 0 || index >= _modules.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return _modules.Keys.ElementAt(index);
        }

        internal int AddBuilder(IFileBuilder? builder)
        {
            if (builder == null)
            {
                return -1;
            }

            if (!_builders.ContainsKey(builder))
            {
                _builders.Add(builder, _builders.Count);
            }

            return _builders[builder];
        }

        internal int GetBuilderIndex(IFileBuilder? builder)
        {
            if (builder == null)
            {
                return -1;
            }

            if (!_builders.TryGetValue(builder, out int value))
            {
                return -1;
            }

            return value;
        }

        internal IFileBuilder? GetBuilder(int index)
        {
            if (index == -1)
            {
                return null;
            }

            if (index < 0 || index >= _builders.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return _builders.Keys.ElementAt(index);
        }

        internal int AddRenderer(IFileRenderer? renderer)
        {
            if (renderer == null)
            {
                return -1;
            }

            if (!_renderers.ContainsKey(renderer))
            {
                _renderers.Add(renderer, _renderers.Count);
            }

            return _renderers[renderer];
        }

        internal int GetRendererIndex(IFileRenderer? renderer)
        {
            if (renderer == null)
            {
                return -1;
            }

            if (!_renderers.TryGetValue(renderer, out int value))
            {
                return -1;
            }

            return value;
        }

        internal IFileRenderer? GetRenderer(int index)
        {
            if (index == -1)
            {
                return null;
            }

            if (index < 0 || index >= _renderers.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return _renderers.Keys.ElementAt(index);
        }
    }
}
