using JeremyAnsel.LibNoiseShader.Builders;
using JeremyAnsel.LibNoiseShader.IO.FileBuilders;
using JeremyAnsel.LibNoiseShader.IO.FileModules;
using JeremyAnsel.LibNoiseShader.IO.FileRenderers;
using JeremyAnsel.LibNoiseShader.IO.Models;
using JeremyAnsel.LibNoiseShader.Modules;
using JeremyAnsel.LibNoiseShader.Renderers;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace JeremyAnsel.LibNoiseShader.IO
{
    public sealed class LibNoiseShaderFile
    {
        private const string HeaderMagic = "LibNoiseShaderFile";

        private const string MainModuleName = "main";

        private const string MainBuilderName = "main";

        private const string MainRendererName = "main";

        public LibNoiseShaderFile()
        {
            this.Version = 1;
        }

        public int Version { get; private set; }

        public bool HasPositions { get; set; }

        public int NoiseSeed { get; set; }

        public List<IFileModule> Modules { get; } = new List<IFileModule>();

        public List<IFileBuilder> Builders { get; } = new List<IFileBuilder>();

        public List<IFileRenderer> Renderers { get; } = new List<IFileRenderer>();

        public static LibNoiseShaderFile Load(string filename)
        {
            using FileStream file = File.OpenRead(filename);
            return Load(file);
        }

        public static LibNoiseShaderFile Load(Stream? stream)
        {
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            using GZipStream decompressStream = new(stream, CompressionMode.Decompress);
            using BinaryReader reader = new(decompressStream, Encoding.UTF8);
            var file = new LibNoiseShaderFile();
            var context = new LibNoiseShaderFileContext();

            string headerMagic = reader.ReadString();

            if (!headerMagic.Equals(HeaderMagic))
            {
                throw new InvalidDataException("Header Magic not found.");
            }

            file.Version = reader.ReadInt32();
            file.HasPositions = reader.ReadBoolean();
            file.NoiseSeed = reader.ReadInt32();

            int modulesCount = reader.ReadInt32();

            for (int moduleIndex = 0; moduleIndex < modulesCount; moduleIndex++)
            {
                string type = reader.ReadString();

                IFileModule module = type switch
                {
                    nameof(AbsFileModule) => new AbsFileModule(),
                    nameof(AddFileModule) => new AddFileModule(),
                    nameof(BillowFileModule) => new BillowFileModule(),
                    nameof(BlendFileModule) => new BlendFileModule(),
                    nameof(CacheFileModule) => new CacheFileModule(),
                    nameof(CheckerboardFileModule) => new CheckerboardFileModule(),
                    nameof(ClampFileModule) => new ClampFileModule(),
                    nameof(ConstantFileModule) => new ConstantFileModule(),
                    nameof(CurveFileModule) => new CurveFileModule(),
                    nameof(CylinderFileModule) => new CylinderFileModule(),
                    nameof(DisplaceFileModule) => new DisplaceFileModule(),
                    nameof(ExponentFileModule) => new ExponentFileModule(),
                    nameof(InvertFileModule) => new InvertFileModule(),
                    nameof(LineFileModule) => new LineFileModule(),
                    nameof(MaxFileModule) => new MaxFileModule(),
                    nameof(MinFileModule) => new MinFileModule(),
                    nameof(MultiplyFileModule) => new MultiplyFileModule(),
                    nameof(PerlinFileModule) => new PerlinFileModule(),
                    nameof(PowerFileModule) => new PowerFileModule(),
                    nameof(RidgedMultiFileModule) => new RidgedMultiFileModule(),
                    nameof(RotatePointFileModule) => new RotatePointFileModule(),
                    nameof(ScaleBiasFileModule) => new ScaleBiasFileModule(),
                    nameof(ScalePointFileModule) => new ScalePointFileModule(),
                    nameof(SelectorFileModule) => new SelectorFileModule(),
                    nameof(SphereFileModule) => new SphereFileModule(),
                    nameof(TerraceFileModule) => new TerraceFileModule(),
                    nameof(TranslatePointFileModule) => new TranslatePointFileModule(),
                    nameof(TurbulenceFileModule) => new TurbulenceFileModule(),
                    nameof(VoronoiFileModule) => new VoronoiFileModule(),
                    _ => throw new InvalidDataException($"Unknown module type found: {type}"),
                };

                module.Read(reader, context);
                file.Modules.Add(module);
            }

            int buildersCount = reader.ReadInt32();

            for (int builderIndex = 0; builderIndex < buildersCount; builderIndex++)
            {
                string type = reader.ReadString();

                IFileBuilder builder = type switch
                {
                    nameof(CylinderFileBuilder) => new CylinderFileBuilder(),
                    nameof(PlaneFileBuilder) => new PlaneFileBuilder(),
                    nameof(SphereFileBuilder) => new SphereFileBuilder(),
                    _ => throw new InvalidDataException($"Unknown builder type found: {type}"),
                };

                builder.Read(reader, context);
                file.Builders.Add(builder);
            }

            int renderersCount = reader.ReadInt32();

            for (int rendererIndex = 0; rendererIndex < renderersCount; rendererIndex++)
            {
                string type = reader.ReadString();

                IFileRenderer renderer = type switch
                {
                    nameof(BlendFileRenderer) => new BlendFileRenderer(),
                    nameof(ImageFileRenderer) => new ImageFileRenderer(),
                    nameof(NormalFileRenderer) => new NormalFileRenderer(),
                    _ => throw new InvalidDataException($"Unknown renderer type found: {type}"),
                };

                renderer.Read(reader, context);
                file.Renderers.Add(renderer);
            }

            return file;
        }

        public void Write(string filename)
        {
            using FileStream file = File.OpenWrite(filename);
            Write(file);
        }

        public void Write(Stream? stream)
        {
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            using GZipStream compressStream = new(stream, CompressionMode.Compress);
            using BinaryWriter writer = new(compressStream, Encoding.UTF8);
            var context = new LibNoiseShaderFileContext();

            writer.Write(HeaderMagic);
            writer.Write(Version);
            writer.Write(HasPositions);
            writer.Write(NoiseSeed);
            writer.Write(Modules.Count);

            foreach (IFileModule module in Modules)
            {
                module.Write(writer, context);
            }

            writer.Write(Builders.Count);

            foreach (IFileBuilder builder in Builders)
            {
                builder.Write(writer, context);
            }

            writer.Write(Renderers.Count);

            foreach (IFileRenderer renderer in Renderers)
            {
                renderer.Write(writer, context);
            }
        }

        public IFileModule? GetFileModuleByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            return Modules.FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public IFileBuilder? GetFileBuilderByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            return Builders.FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public IFileRenderer? GetFileRendererByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            return Renderers.FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public IModule? GetModuleByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            IFileModule? moduleFile = GetFileModuleByName(name);

            if (moduleFile is null)
            {
                return null;
            }

            var noise = new Noise3D(NoiseSeed);
            return LibNoiseShaderFileLoadContext.LoadModule(moduleFile, noise);
        }

        public IBuilder? GetBuilderByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            IFileBuilder? builderFile = GetFileBuilderByName(name);

            if (builderFile is null)
            {
                return null;
            }

            var noise = new Noise3D(NoiseSeed);
            return LibNoiseShaderFileLoadContext.LoadBuilder(builderFile, noise);
        }

        public IRenderer? GetRendererByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            IFileRenderer? rendererFile = GetFileRendererByName(name);

            if (rendererFile is null)
            {
                return null;
            }

            var noise = new Noise3D(NoiseSeed);
            return LibNoiseShaderFileLoadContext.LoadRenderer(rendererFile, noise);
        }

        public IFileModule? GetMainFileModule()
        {
            return GetFileModuleByName(MainModuleName);
        }

        public IFileBuilder? GetMainFileBuilder()
        {
            return GetFileBuilderByName(MainBuilderName);
        }

        public IFileRenderer? GetMainFileRenderer()
        {
            return GetFileRendererByName(MainRendererName);
        }

        public IModule? GetMainModule()
        {
            return GetModuleByName(MainModuleName);
        }

        public IBuilder? GetMainBuilder()
        {
            return GetBuilderByName(MainBuilderName);
        }

        public IRenderer? GetMainRenderer()
        {
            return GetRendererByName(MainRendererName);
        }

        public bool HasMain()
        {
            return GetMainFileModule() is not null
                || GetMainFileBuilder() is not null
                || GetMainFileRenderer() is not null;
        }
    }
}
