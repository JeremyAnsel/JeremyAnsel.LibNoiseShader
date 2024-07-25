using JeremyAnsel.LibNoiseShader.Builders;
using JeremyAnsel.LibNoiseShader.IO.FileBuilders;
using JeremyAnsel.LibNoiseShader.IO.FileModules;
using JeremyAnsel.LibNoiseShader.IO.FileRenderers;
using JeremyAnsel.LibNoiseShader.Modules;
using JeremyAnsel.LibNoiseShader.Renderers;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace JeremyAnsel.LibNoiseShader.IO.Models
{
    public sealed class LibNoiseShaderFileWriteContext
    {
        private readonly Dictionary<IModule, IFileModule> _modules = new();

        private readonly Dictionary<IBuilder, IFileBuilder> _builders = new();

        private readonly Dictionary<IRenderer, IFileRenderer> _renderers = new();

        private IFileModule AddModule(IModule module, IFileModule moduleFile)
        {
            if (!_modules.ContainsKey(module))
            {
                _modules.Add(module, moduleFile);
            }

            return _modules[module];
        }

        private IFileModule? GetModule(IModule? module)
        {
            if (module is null)
            {
                return null;
            }

            if (!_modules.TryGetValue(module, out IFileModule? moduleFile))
            {
                return null;
            }

            return moduleFile;
        }

        private IFileBuilder AddBuilder(IBuilder builder, IFileBuilder builderFile)
        {
            if (!_builders.ContainsKey(builder))
            {
                _builders.Add(builder, builderFile);
            }

            return _builders[builder];
        }

        private IFileBuilder? GetBuilder(IBuilder? builder)
        {
            if (builder is null)
            {
                return null;
            }

            if (!_builders.TryGetValue(builder, out IFileBuilder? builderFile))
            {
                return null;
            }

            return builderFile;
        }

        private IFileRenderer AddRenderer(IRenderer renderer, IFileRenderer rendererFile)
        {
            if (!_renderers.ContainsKey(renderer))
            {
                _renderers.Add(renderer, rendererFile);
            }

            return _renderers[renderer];
        }

        private IFileRenderer? GetRenderer(IRenderer? renderer)
        {
            if (renderer is null)
            {
                return null;
            }

            if (!_renderers.TryGetValue(renderer, out IFileRenderer? rendererFile))
            {
                return null;
            }

            return rendererFile;
        }

        public static LibNoiseShaderFile BuildLibNoiseShaderFile(IModule? module, Noise3D? noise)
        {
            if (module is null)
            {
                throw new ArgumentNullException(nameof(module));
            }

            if (noise is null)
            {
                throw new ArgumentNullException(nameof(noise));
            }

            var context = new LibNoiseShaderFileWriteContext();

            var file = new LibNoiseShaderFile
            {
                HasPositions = false,
                NoiseSeed = noise.Seed
            };

            BuildFileModule(file, context, module);

            return file;
        }

        public static LibNoiseShaderFile BuildLibNoiseShaderFile(IBuilder? builder, Noise3D? noise)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (noise is null)
            {
                throw new ArgumentNullException(nameof(noise));
            }

            var context = new LibNoiseShaderFileWriteContext();

            var file = new LibNoiseShaderFile
            {
                HasPositions = false,
                NoiseSeed = noise.Seed
            };

            BuildFileBuilder(file, context, builder);

            return file;
        }

        public static LibNoiseShaderFile BuildLibNoiseShaderFile(IRenderer? renderer, Noise3D? noise)
        {
            if (renderer is null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            if (noise is null)
            {
                throw new ArgumentNullException(nameof(noise));
            }

            var context = new LibNoiseShaderFileWriteContext();

            var file = new LibNoiseShaderFile
            {
                HasPositions = false,
                NoiseSeed = noise.Seed
            };

            BuildFileRenderer(file, context, renderer);

            return file;
        }

        private static IFileModule? BuildFileModule(LibNoiseShaderFile file, LibNoiseShaderFileWriteContext context, IModule? module)
        {
            if (module is null)
            {
                return null;
            }

            IFileModule? moduleFile = context.GetModule(module);

            if (moduleFile is not null)
            {
                return moduleFile;
            }

            moduleFile = module switch
            {
                AbsModule m => BuildAbsFileModule(file, context, m),
                AddModule m => BuildAddFileModule(file, context, m),
                BillowModule m => BuildBillowFileModule(file, context, m),
                BlendModule m => BuildBlendFileModule(file, context, m),
                CacheModule m => BuildCacheFileModule(file, context, m),
                CheckerboardModule m => BuildCheckerboardFileModule(file, context, m),
                ClampModule m => BuildClampFileModule(file, context, m),
                ConstantModule m => BuildConstantFileModule(file, context, m),
                CurveModule m => BuildCurveFileModule(file, context, m),
                CylinderModule m => BuildCylinderFileModule(file, context, m),
                DisplaceModule m => BuildDisplaceFileModule(file, context, m),
                ExponentModule m => BuildExponentFileModule(file, context, m),
                InvertModule m => BuildInvertFileModule(file, context, m),
                LineModule m => BuildLineFileModule(file, context, m),
                MaxModule m => BuildMaxFileModule(file, context, m),
                MinModule m => BuildMinFileModule(file, context, m),
                MultiplyModule m => BuildMultiplyFileModule(file, context, m),
                PerlinModule m => BuildPerlinFileModule(file, context, m),
                PowerModule m => BuildPowerFileModule(file, context, m),
                RidgedMultiModule m => BuildRidgedMultiFileModule(file, context, m),
                RotatePointModule m => BuildRotatePointFileModule(file, context, m),
                ScaleBiasModule m => BuildScaleBiasFileModule(file, context, m),
                ScalePointModule m => BuildScalePointFileModule(file, context, m),
                SelectorModule m => BuildSelectorFileModule(file, context, m),
                SphereModule m => BuildSphereFileModule(file, context, m),
                TerraceModule m => BuildTerraceFileModule(file, context, m),
                TranslatePointModule m => BuildTranslatePointFileModule(file, context, m),
                TurbulenceModule m => BuildTurbulenceFileModule(file, context, m),
                VoronoiModule m => BuildVoronoiFileModule(file, context, m),
                _ => throw new NotSupportedException($"Not supported module type found: {module.GetType().Name}"),
            };

            context.AddModule(module, moduleFile);

            file.Modules.Add(moduleFile);
            return moduleFile;
        }

        private static IFileModule BuildAbsFileModule(LibNoiseShaderFile file, LibNoiseShaderFileWriteContext context, AbsModule m)
        {
            var module = new AbsFileModule
            {
                Name = m.Name,
                Input1 = BuildFileModule(file, context, m.GetSourceModule(0))!,
            };

            return module;
        }

        private static IFileModule BuildAddFileModule(LibNoiseShaderFile file, LibNoiseShaderFileWriteContext context, AddModule m)
        {
            var module = new AddFileModule
            {
                Name = m.Name,
                Input1 = BuildFileModule(file, context, m.GetSourceModule(0))!,
                Input2 = BuildFileModule(file, context, m.GetSourceModule(1))!,
            };

            return module;
        }

        private static IFileModule BuildBillowFileModule(LibNoiseShaderFile file, LibNoiseShaderFileWriteContext context, BillowModule m)
        {
            var module = new BillowFileModule
            {
                Name = m.Name,
                Frequency = m.Frequency,
                Lacunarity = m.Lacunarity,
                OctaveCount = m.OctaveCount,
                Persistence = m.Persistence,
                SeedOffset = m.SeedOffset,
            };

            return module;
        }

        private static IFileModule BuildBlendFileModule(LibNoiseShaderFile file, LibNoiseShaderFileWriteContext context, BlendModule m)
        {
            var module = new BlendFileModule
            {
                Name = m.Name,
                Input1 = BuildFileModule(file, context, m.GetSourceModule(0))!,
                Input2 = BuildFileModule(file, context, m.GetSourceModule(1))!,
                Control = BuildFileModule(file, context, m.GetSourceModule(2))!,
            };

            return module;
        }

        private static IFileModule BuildCacheFileModule(LibNoiseShaderFile file, LibNoiseShaderFileWriteContext context, CacheModule m)
        {
            var module = new CacheFileModule
            {
                Name = m.Name,
                Input1 = BuildFileModule(file, context, m.GetSourceModule(0))!,
            };

            return module;
        }

        private static IFileModule BuildCheckerboardFileModule(LibNoiseShaderFile file, LibNoiseShaderFileWriteContext context, CheckerboardModule m)
        {
            var module = new CheckerboardFileModule
            {
                Name = m.Name,
            };

            return module;
        }

        private static IFileModule BuildClampFileModule(LibNoiseShaderFile file, LibNoiseShaderFileWriteContext context, ClampModule m)
        {
            var module = new ClampFileModule
            {
                Name = m.Name,
                LowerBound = m.LowerBound,
                UpperBound = m.UpperBound,
                Input1 = BuildFileModule(file, context, m.GetSourceModule(0))!,
            };

            return module;
        }

        private static IFileModule BuildConstantFileModule(LibNoiseShaderFile file, LibNoiseShaderFileWriteContext context, ConstantModule m)
        {
            var module = new ConstantFileModule
            {
                Name = m.Name,
                ConstantValue = m.ConstantValue,
            };

            return module;
        }

        private static IFileModule BuildCurveFileModule(LibNoiseShaderFile file, LibNoiseShaderFileWriteContext context, CurveModule m)
        {
            var module = new CurveFileModule
            {
                Name = m.Name,
                Input1 = BuildFileModule(file, context, m.GetSourceModule(0))!,
            };

            foreach (KeyValuePair<float, float> point in m.ControlPoints)
            {
                module.ControlPoints.Add(point);
            }

            return module;
        }

        private static IFileModule BuildCylinderFileModule(LibNoiseShaderFile file, LibNoiseShaderFileWriteContext context, CylinderModule m)
        {
            var module = new CylinderFileModule
            {
                Name = m.Name,
                Frequency = m.Frequency,
            };

            return module;
        }

        private static IFileModule BuildDisplaceFileModule(LibNoiseShaderFile file, LibNoiseShaderFileWriteContext context, DisplaceModule m)
        {
            var module = new DisplaceFileModule
            {
                Name = m.Name,
                Input1 = BuildFileModule(file, context, m.GetSourceModule(0))!,
                DisplaceX = BuildFileModule(file, context, m.GetSourceModule(1))!,
                DisplaceY = BuildFileModule(file, context, m.GetSourceModule(2))!,
                DisplaceZ = BuildFileModule(file, context, m.GetSourceModule(3))!,
            };

            return module;
        }

        private static IFileModule BuildExponentFileModule(LibNoiseShaderFile file, LibNoiseShaderFileWriteContext context, ExponentModule m)
        {
            var module = new ExponentFileModule
            {
                Name = m.Name,
                ExponentValue = m.ExponentValue,
                Input1 = BuildFileModule(file, context, m.GetSourceModule(0))!,
            };

            return module;
        }

        private static IFileModule BuildInvertFileModule(LibNoiseShaderFile file, LibNoiseShaderFileWriteContext context, InvertModule m)
        {
            var module = new InvertFileModule
            {
                Name = m.Name,
                Input1 = BuildFileModule(file, context, m.GetSourceModule(0))!,
            };

            return module;
        }

        private static IFileModule BuildLineFileModule(LibNoiseShaderFile file, LibNoiseShaderFileWriteContext context, LineModule m)
        {
            var module = new LineFileModule
            {
                Name = m.Name,
                Attenuate = m.Attenuate,
                StartPointX = m.StartPointX,
                StartPointY = m.StartPointY,
                StartPointZ = m.StartPointZ,
                EndPointX = m.EndPointX,
                EndPointY = m.EndPointY,
                EndPointZ = m.EndPointZ,
                Input1 = BuildFileModule(file, context, m.GetSourceModule(0))!,
            };

            return module;
        }

        private static IFileModule BuildMaxFileModule(LibNoiseShaderFile file, LibNoiseShaderFileWriteContext context, MaxModule m)
        {
            var module = new MaxFileModule
            {
                Name = m.Name,
                Input1 = BuildFileModule(file, context, m.GetSourceModule(0))!,
                Input2 = BuildFileModule(file, context, m.GetSourceModule(1))!,
            };

            return module;
        }

        private static IFileModule BuildMinFileModule(LibNoiseShaderFile file, LibNoiseShaderFileWriteContext context, MinModule m)
        {
            var module = new MinFileModule
            {
                Name = m.Name,
                Input1 = BuildFileModule(file, context, m.GetSourceModule(0))!,
                Input2 = BuildFileModule(file, context, m.GetSourceModule(1))!,
            };

            return module;
        }

        private static IFileModule BuildMultiplyFileModule(LibNoiseShaderFile file, LibNoiseShaderFileWriteContext context, MultiplyModule m)
        {
            var module = new MultiplyFileModule
            {
                Name = m.Name,
                Input1 = BuildFileModule(file, context, m.GetSourceModule(0))!,
                Input2 = BuildFileModule(file, context, m.GetSourceModule(1))!,
            };

            return module;
        }

        private static IFileModule BuildPerlinFileModule(LibNoiseShaderFile file, LibNoiseShaderFileWriteContext context, PerlinModule m)
        {
            var module = new PerlinFileModule
            {
                Name = m.Name,
                Frequency = m.Frequency,
                Lacunarity = m.Lacunarity,
                OctaveCount = m.OctaveCount,
                Persistence = m.Persistence,
                SeedOffset = m.SeedOffset,
            };

            return module;
        }

        private static IFileModule BuildPowerFileModule(LibNoiseShaderFile file, LibNoiseShaderFileWriteContext context, PowerModule m)
        {
            var module = new PowerFileModule
            {
                Name = m.Name,
                Input1 = BuildFileModule(file, context, m.GetSourceModule(0))!,
                Input2 = BuildFileModule(file, context, m.GetSourceModule(1))!,
            };

            return module;
        }

        private static IFileModule BuildRidgedMultiFileModule(LibNoiseShaderFile file, LibNoiseShaderFileWriteContext context, RidgedMultiModule m)
        {
            var module = new RidgedMultiFileModule
            {
                Name = m.Name,
                Frequency = m.Frequency,
                Lacunarity = m.Lacunarity,
                Offset = m.Offset,
                Gain = m.Gain,
                Exponent = m.Exponent,
                OctaveCount = m.OctaveCount,
                SeedOffset = m.SeedOffset,
            };

            return module;
        }

        private static IFileModule BuildRotatePointFileModule(LibNoiseShaderFile file, LibNoiseShaderFileWriteContext context, RotatePointModule m)
        {
            var module = new RotatePointFileModule
            {
                Name = m.Name,
                AngleX = m.AngleX,
                AngleY = m.AngleY,
                AngleZ = m.AngleZ,
                Input1 = BuildFileModule(file, context, m.GetSourceModule(0))!,
            };

            return module;
        }

        private static IFileModule BuildScaleBiasFileModule(LibNoiseShaderFile file, LibNoiseShaderFileWriteContext context, ScaleBiasModule m)
        {
            var module = new ScaleBiasFileModule
            {
                Name = m.Name,
                Bias = m.Bias,
                Scale = m.Scale,
                Input1 = BuildFileModule(file, context, m.GetSourceModule(0))!,
            };

            return module;
        }

        private static IFileModule BuildScalePointFileModule(LibNoiseShaderFile file, LibNoiseShaderFileWriteContext context, ScalePointModule m)
        {
            var module = new ScalePointFileModule
            {
                Name = m.Name,
                ScaleX = m.ScaleX,
                ScaleY = m.ScaleY,
                ScaleZ = m.ScaleZ,
                Input1 = BuildFileModule(file, context, m.GetSourceModule(0))!,
            };

            return module;
        }

        private static IFileModule BuildSelectorFileModule(LibNoiseShaderFile file, LibNoiseShaderFileWriteContext context, SelectorModule m)
        {
            var module = new SelectorFileModule
            {
                Name = m.Name,
                EdgeFalloff = m.EdgeFalloff,
                LowerBound = m.LowerBound,
                UpperBound = m.UpperBound,
                Input1 = BuildFileModule(file, context, m.GetSourceModule(0))!,
                Input2 = BuildFileModule(file, context, m.GetSourceModule(1))!,
                Control = BuildFileModule(file, context, m.GetSourceModule(2))!,
            };

            return module;
        }

        private static IFileModule BuildSphereFileModule(LibNoiseShaderFile file, LibNoiseShaderFileWriteContext context, SphereModule m)
        {
            var module = new SphereFileModule
            {
                Name = m.Name,
                Frequency = m.Frequency,
            };

            return module;
        }

        private static IFileModule BuildTerraceFileModule(LibNoiseShaderFile file, LibNoiseShaderFileWriteContext context, TerraceModule m)
        {
            var module = new TerraceFileModule
            {
                Name = m.Name,
                IsInverted = m.IsInverted,
                Input1 = BuildFileModule(file, context, m.GetSourceModule(0))!,
            };

            foreach (float point in m.ControlPoints)
            {
                module.ControlPoints.Add(point);
            }

            return module;
        }

        private static IFileModule BuildTranslatePointFileModule(LibNoiseShaderFile file, LibNoiseShaderFileWriteContext context, TranslatePointModule m)
        {
            var module = new TranslatePointFileModule
            {
                Name = m.Name,
                TranslateX = m.TranslateX,
                TranslateY = m.TranslateY,
                TranslateZ = m.TranslateZ,
                Input1 = BuildFileModule(file, context, m.GetSourceModule(0))!,
            };

            return module;
        }

        private static IFileModule BuildTurbulenceFileModule(LibNoiseShaderFile file, LibNoiseShaderFileWriteContext context, TurbulenceModule m)
        {
            var module = new TurbulenceFileModule
            {
                Name = m.Name,
                Frequency = m.Frequency,
                Power = m.Power,
                Roughness = m.Roughness,
                SeedOffset = m.SeedOffset,
                Input1 = BuildFileModule(file, context, m.GetSourceModule(0))!,
            };

            return module;
        }

        private static IFileModule BuildVoronoiFileModule(LibNoiseShaderFile file, LibNoiseShaderFileWriteContext context, VoronoiModule m)
        {
            var module = new VoronoiFileModule
            {
                Name = m.Name,
                IsDistanceApplied = m.IsDistanceApplied,
                Displacement = m.Displacement,
                Frequency = m.Frequency,
                SeedOffset = m.SeedOffset,
            };

            return module;
        }

        private static IFileBuilder? BuildFileBuilder(LibNoiseShaderFile file, LibNoiseShaderFileWriteContext context, IBuilder? builder)
        {
            if (builder is null)
            {
                return null;
            }

            IFileBuilder? builderFile = context.GetBuilder(builder);

            if (builderFile is not null)
            {
                return builderFile;
            }

            builderFile = builder switch
            {
                CylinderBuilder m => BuildCylinderFileBuilder(file, context, m),
                PlaneBuilder m => BuildPlaneFileBuilder(file, context, m),
                SphereBuilder m => BuildSphereFileBuilder(file, context, m),
                _ => throw new NotSupportedException($"Not supported builder type found: {builder.GetType().Name}"),
            };

            context.AddBuilder(builder, builderFile);

            file.Builders.Add(builderFile);
            return builderFile;
        }

        private static IFileBuilder BuildCylinderFileBuilder(LibNoiseShaderFile file, LibNoiseShaderFileWriteContext context, CylinderBuilder m)
        {
            var builder = new CylinderFileBuilder
            {
                Name = m.Name,
                LowerAngleBound = m.LowerAngleBound,
                LowerHeightBound = m.LowerHeightBound,
                UpperAngleBound = m.UpperAngleBound,
                UpperHeightBound = m.UpperHeightBound,
                Source = BuildFileModule(file, context, m.GetSourceModule())!,
            };

            return builder;
        }

        private static IFileBuilder BuildPlaneFileBuilder(LibNoiseShaderFile file, LibNoiseShaderFileWriteContext context, PlaneBuilder m)
        {
            var builder = new PlaneFileBuilder
            {
                Name = m.Name,
                IsSeamless = m.IsSeamless,
                LowerBoundX = m.LowerBoundX,
                UpperBoundX = m.UpperBoundX,
                LowerBoundY = m.LowerBoundY,
                UpperBoundY = m.UpperBoundY,
                Source = BuildFileModule(file, context, m.GetSourceModule())!,
            };

            return builder;
        }

        private static IFileBuilder BuildSphereFileBuilder(LibNoiseShaderFile file, LibNoiseShaderFileWriteContext context, SphereBuilder m)
        {
            var builder = new SphereFileBuilder
            {
                Name = m.Name,
                SouthLatBound = m.SouthLatBound,
                NorthLatBound = m.NorthLatBound,
                WestLonBound = m.WestLonBound,
                EastLonBound = m.EastLonBound,
                Source = BuildFileModule(file, context, m.GetSourceModule())!,
            };

            return builder;
        }

        private static IFileRenderer? BuildFileRenderer(LibNoiseShaderFile file, LibNoiseShaderFileWriteContext context, IRenderer? renderer)
        {
            if (renderer is null)
            {
                return null;
            }

            IFileRenderer? rendererFile = context.GetRenderer(renderer);

            if (rendererFile is not null)
            {
                return rendererFile;
            }

            rendererFile = renderer switch
            {
                BlendRenderer m => BuildBlendFileRenderer(file, context, m),
                ImageRenderer m => BuildImageFileRenderer(file, context, m),
                NormalRenderer m => BuildNormalFileRenderer(file, context, m),
                _ => throw new NotSupportedException($"Not supported renderer type found: {renderer.GetType().Name}"),
            };

            context.AddRenderer(renderer, rendererFile);

            file.Renderers.Add(rendererFile);
            return rendererFile;
        }

        private static IFileRenderer BuildBlendFileRenderer(LibNoiseShaderFile file, LibNoiseShaderFileWriteContext context, BlendRenderer m)
        {
            var renderer = new BlendFileRenderer
            {
                Name = m.Name,
                Input1 = BuildFileRenderer(file, context, m.GetSourceRenderer(0))!,
                Input2 = BuildFileRenderer(file, context, m.GetSourceRenderer(1))!,
            };

            return renderer;
        }

        private static IFileRenderer BuildImageFileRenderer(LibNoiseShaderFile file, LibNoiseShaderFileWriteContext context, ImageRenderer m)
        {
            var renderer = new ImageFileRenderer
            {
                Name = m.Name,
                IsWrapEnabled = m.IsWrapEnabled,
                IsLightEnabled = m.IsLightEnabled,
                LightAzimuth = m.LightAzimuth,
                LightBrightness = m.LightBrightness,
                LightColor = Color.FromArgb(m.LightColor.A, m.LightColor.R, m.LightColor.G, m.LightColor.B),
                LightContrast = m.LightContrast,
                LightElevation = m.LightElevation,
                LightIntensity = m.LightIntensity,
                Source = BuildFileBuilder(file, context, m.GetSourceBuilder())!,
            };

            foreach (KeyValuePair<float, Color> point in m.GetGradient())
            {
                float key = point.Key;
                Color color = Color.FromArgb(point.Value.A, point.Value.R, point.Value.G, point.Value.B);

                renderer.GradientPoints.Add(key, color);
            }

            return renderer;
        }

        private static IFileRenderer BuildNormalFileRenderer(LibNoiseShaderFile file, LibNoiseShaderFileWriteContext context, NormalRenderer m)
        {
            var renderer = new NormalFileRenderer
            {
                Name = m.Name,
                Source = BuildFileBuilder(file, context, m.GetSourceBuilder())!,
            };

            return renderer;
        }
    }
}
