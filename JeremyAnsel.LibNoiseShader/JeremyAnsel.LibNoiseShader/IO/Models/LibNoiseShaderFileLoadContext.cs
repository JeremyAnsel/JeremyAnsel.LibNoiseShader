using JeremyAnsel.LibNoiseShader.Builders;
using JeremyAnsel.LibNoiseShader.IO.FileBuilders;
using JeremyAnsel.LibNoiseShader.IO.FileModules;
using JeremyAnsel.LibNoiseShader.IO.FileRenderers;
using JeremyAnsel.LibNoiseShader.Modules;
using JeremyAnsel.LibNoiseShader.Renderers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace JeremyAnsel.LibNoiseShader.IO.Models
{
    public sealed class LibNoiseShaderFileLoadContext
    {
        private readonly Dictionary<IFileModule, IModule> _modules = new();

        private readonly Dictionary<IFileBuilder, IBuilder> _builders = new();

        private readonly Dictionary<IFileRenderer, IRenderer> _renderers = new();

        private IModule AddModule(IFileModule moduleFile, IModule module)
        {
            if (!_modules.ContainsKey(moduleFile))
            {
                _modules.Add(moduleFile, module);
            }

            return _modules[moduleFile];
        }

        private IModule GetModule(IFileModule moduleFile)
        {
            if (moduleFile is null)
            {
                return null;
            }

            if (!_modules.TryGetValue(moduleFile, out IModule module))
            {
                return null;
            }

            return module;
        }

        private IBuilder AddBuilder(IFileBuilder builderFile, IBuilder builder)
        {
            if (!_builders.ContainsKey(builderFile))
            {
                _builders.Add(builderFile, builder);
            }

            return _builders[builderFile];
        }

        private IBuilder GetBuilder(IFileBuilder builderFile)
        {
            if (builderFile is null)
            {
                return null;
            }

            if (!_builders.TryGetValue(builderFile, out IBuilder builder))
            {
                return null;
            }

            return builder;
        }

        private IRenderer AddRenderer(IFileRenderer rendererFile, IRenderer renderer)
        {
            if (!_renderers.ContainsKey(rendererFile))
            {
                _renderers.Add(rendererFile, renderer);
            }

            return _renderers[rendererFile];
        }

        private IRenderer GetRenderer(IFileRenderer rendererFile)
        {
            if (rendererFile is null)
            {
                return null;
            }

            if (!_renderers.TryGetValue(rendererFile, out IRenderer renderer))
            {
                return null;
            }

            return renderer;
        }

        public static IModule LoadModule(IFileModule moduleFile, Noise3D noise)
        {
            if (moduleFile is null)
            {
                throw new ArgumentNullException(nameof(moduleFile));
            }

            if (noise is null)
            {
                throw new ArgumentNullException(nameof(noise));
            }

            var context = new LibNoiseShaderFileLoadContext();

            IModule module = moduleFile switch
            {
                AbsFileModule m => BuildAbsModule(noise, context, m),
                AddFileModule m => BuildAddModule(noise, context, m),
                BillowFileModule m => BuildBillowModule(noise, context, m),
                BlendFileModule m => BuildBlendModule(noise, context, m),
                CacheFileModule m => BuildCacheModule(noise, context, m),
                CheckerboardFileModule m => BuildCheckerboardModule(noise, context, m),
                ClampFileModule m => BuildClampModule(noise, context, m),
                ConstantFileModule m => BuildConstantModule(noise, context, m),
                CurveFileModule m => BuildCurveModule(noise, context, m),
                CylinderFileModule m => BuildCylinderModule(noise, context, m),
                DisplaceFileModule m => BuildDisplaceModule(noise, context, m),
                ExponentFileModule m => BuildExponentModule(noise, context, m),
                InvertFileModule m => BuildInvertModule(noise, context, m),
                LineFileModule m => BuildLineModule(noise, context, m),
                MaxFileModule m => BuildMaxModule(noise, context, m),
                MinFileModule m => BuildMinModule(noise, context, m),
                MultiplyFileModule m => BuildMultiplyModule(noise, context, m),
                PerlinFileModule m => BuildPerlinModule(noise, context, m),
                PowerFileModule m => BuildPowerModule(noise, context, m),
                RidgedMultiFileModule m => BuildRidgedMultiModule(noise, context, m),
                RotatePointFileModule m => BuildRotatePointModule(noise, context, m),
                ScaleBiasFileModule m => BuildScaleBiasModule(noise, context, m),
                ScalePointFileModule m => BuildScalePointModule(noise, context, m),
                SelectorFileModule m => BuildSelectorModule(noise, context, m),
                SphereFileModule m => BuildSphereModule(noise, context, m),
                TerraceFileModule m => BuildTerraceModule(noise, context, m),
                TranslatePointFileModule m => BuildTranslatePointModule(noise, context, m),
                TurbulenceFileModule m => BuildTurbulenceModule(noise, context, m),
                VoronoiFileModule m => BuildVoronoiModule(noise, context, m),
                _ => throw new NotSupportedException($"Not supported module type found: {moduleFile.GetType().Name}"),
            };

            return module;
        }

        private static void BuildModule(Noise3D noise, LibNoiseShaderFileLoadContext context, IFileModule moduleFile)
        {
            if (moduleFile is null)
            {
                throw new ArgumentNullException(nameof(moduleFile));
            }

            if (context.GetModule(moduleFile) is null)
            {
                context.AddModule(moduleFile, LoadModule(moduleFile, noise));
            }
        }

        private static IModule BuildAbsModule(Noise3D noise, LibNoiseShaderFileLoadContext context, AbsFileModule m)
        {
            BuildModule(noise, context, m.Input1);

            var module = new AbsModule(
                context.GetModule(m.Input1))
            {
                Name = m.Name,
            };

            return module;
        }

        private static IModule BuildAddModule(Noise3D noise, LibNoiseShaderFileLoadContext context, AddFileModule m)
        {
            BuildModule(noise, context, m.Input1);
            BuildModule(noise, context, m.Input2);

            var module = new AddModule(
                context.GetModule(m.Input1),
                context.GetModule(m.Input2))
            {
                Name = m.Name,
            };

            return module;
        }

        private static IModule BuildBillowModule(Noise3D noise, LibNoiseShaderFileLoadContext context, BillowFileModule m)
        {
            var module = new BillowModule(
                noise)
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

        private static IModule BuildBlendModule(Noise3D noise, LibNoiseShaderFileLoadContext context, BlendFileModule m)
        {
            BuildModule(noise, context, m.Input1);
            BuildModule(noise, context, m.Input2);
            BuildModule(noise, context, m.Control);

            var module = new BlendModule(
                context.GetModule(m.Input1),
                context.GetModule(m.Input2),
                context.GetModule(m.Control))
            {
                Name = m.Name,
            };

            return module;
        }

        private static IModule BuildCacheModule(Noise3D noise, LibNoiseShaderFileLoadContext context, CacheFileModule m)
        {
            BuildModule(noise, context, m.Input1);

            var module = new CacheModule(
                context.GetModule(m.Input1))
            {
                Name = m.Name,
            };

            return module;
        }

        private static IModule BuildCheckerboardModule(Noise3D noise, LibNoiseShaderFileLoadContext context, CheckerboardFileModule m)
        {
            var module = new CheckerboardModule()
            {
                Name = m.Name,
            };

            return module;
        }

        private static IModule BuildClampModule(Noise3D noise, LibNoiseShaderFileLoadContext context, ClampFileModule m)
        {
            BuildModule(noise, context, m.Input1);

            var module = new ClampModule(
                context.GetModule(m.Input1))
            {
                Name = m.Name,
                LowerBound = m.LowerBound,
                UpperBound = m.UpperBound,
            };

            return module;
        }

        private static IModule BuildConstantModule(Noise3D noise, LibNoiseShaderFileLoadContext context, ConstantFileModule m)
        {
            var module = new ConstantModule()
            {
                Name = m.Name,
                ConstantValue = m.ConstantValue,
            };

            return module;
        }

        private static IModule BuildCurveModule(Noise3D noise, LibNoiseShaderFileLoadContext context, CurveFileModule m)
        {
            BuildModule(noise, context, m.Input1);

            var module = new CurveModule(
                context.GetModule(m.Input1))
            {
                Name = m.Name,
            };

            foreach (KeyValuePair<float, float> point in m.ControlPoints)
            {
                module.ControlPoints.Add(point.Key, point.Value);
            }

            return module;
        }

        private static IModule BuildCylinderModule(Noise3D noise, LibNoiseShaderFileLoadContext context, CylinderFileModule m)
        {
            var module = new CylinderModule()
            {
                Name = m.Name,
                Frequency = m.Frequency,
            };

            return module;
        }

        private static IModule BuildDisplaceModule(Noise3D noise, LibNoiseShaderFileLoadContext context, DisplaceFileModule m)
        {
            BuildModule(noise, context, m.Input1);
            BuildModule(noise, context, m.DisplaceX);
            BuildModule(noise, context, m.DisplaceY);
            BuildModule(noise, context, m.DisplaceZ);

            var module = new DisplaceModule(
                context.GetModule(m.Input1),
                context.GetModule(m.DisplaceX),
                context.GetModule(m.DisplaceY),
                context.GetModule(m.DisplaceZ))
            {
                Name = m.Name,
            };

            return module;
        }

        private static IModule BuildExponentModule(Noise3D noise, LibNoiseShaderFileLoadContext context, ExponentFileModule m)
        {
            BuildModule(noise, context, m.Input1);

            var module = new ExponentModule(
                context.GetModule(m.Input1))
            {
                Name = m.Name,
                ExponentValue = m.ExponentValue,
            };

            return module;
        }

        private static IModule BuildInvertModule(Noise3D noise, LibNoiseShaderFileLoadContext context, InvertFileModule m)
        {
            BuildModule(noise, context, m.Input1);

            var module = new InvertModule(
                context.GetModule(m.Input1))
            {
                Name = m.Name,
            };

            return module;
        }

        private static IModule BuildLineModule(Noise3D noise, LibNoiseShaderFileLoadContext context, LineFileModule m)
        {
            BuildModule(noise, context, m.Input1);

            var module = new LineModule(
                context.GetModule(m.Input1))
            {
                Name = m.Name,
                Attenuate = m.Attenuate,
                StartPointX = m.StartPointX,
                StartPointY = m.StartPointY,
                StartPointZ = m.StartPointZ,
                EndPointX = m.EndPointX,
                EndPointY = m.EndPointY,
                EndPointZ = m.EndPointZ,
            };

            return module;
        }

        private static IModule BuildMaxModule(Noise3D noise, LibNoiseShaderFileLoadContext context, MaxFileModule m)
        {
            BuildModule(noise, context, m.Input1);
            BuildModule(noise, context, m.Input2);

            var module = new MaxModule(
                context.GetModule(m.Input1),
                context.GetModule(m.Input2))
            {
                Name = m.Name,
            };

            return module;
        }

        private static IModule BuildMinModule(Noise3D noise, LibNoiseShaderFileLoadContext context, MinFileModule m)
        {
            BuildModule(noise, context, m.Input1);
            BuildModule(noise, context, m.Input2);

            var module = new MinModule(
                context.GetModule(m.Input1),
                context.GetModule(m.Input2))
            {
                Name = m.Name,
            };

            return module;
        }

        private static IModule BuildMultiplyModule(Noise3D noise, LibNoiseShaderFileLoadContext context, MultiplyFileModule m)
        {
            BuildModule(noise, context, m.Input1);
            BuildModule(noise, context, m.Input2);

            var module = new MultiplyModule(
                context.GetModule(m.Input1),
                context.GetModule(m.Input2))
            {
                Name = m.Name,
            };

            return module;
        }

        private static IModule BuildPerlinModule(Noise3D noise, LibNoiseShaderFileLoadContext context, PerlinFileModule m)
        {
            var module = new PerlinModule(
                noise)
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

        private static IModule BuildPowerModule(Noise3D noise, LibNoiseShaderFileLoadContext context, PowerFileModule m)
        {
            BuildModule(noise, context, m.Input1);
            BuildModule(noise, context, m.Input2);

            var module = new PowerModule(
                context.GetModule(m.Input1),
                context.GetModule(m.Input2))
            {
                Name = m.Name,
            };

            return module;
        }

        private static IModule BuildRidgedMultiModule(Noise3D noise, LibNoiseShaderFileLoadContext context, RidgedMultiFileModule m)
        {
            var module = new RidgedMultiModule(
                noise)
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

        private static IModule BuildRotatePointModule(Noise3D noise, LibNoiseShaderFileLoadContext context, RotatePointFileModule m)
        {
            BuildModule(noise, context, m.Input1);

            var module = new RotatePointModule(
                context.GetModule(m.Input1))
            {
                Name = m.Name,
                AngleX = m.AngleX,
                AngleY = m.AngleY,
                AngleZ = m.AngleZ,
            };

            return module;
        }

        private static IModule BuildScaleBiasModule(Noise3D noise, LibNoiseShaderFileLoadContext context, ScaleBiasFileModule m)
        {
            BuildModule(noise, context, m.Input1);

            var module = new ScaleBiasModule(
                context.GetModule(m.Input1))
            {
                Name = m.Name,
                Bias = m.Bias,
                Scale = m.Scale,
            };

            return module;
        }

        private static IModule BuildScalePointModule(Noise3D noise, LibNoiseShaderFileLoadContext context, ScalePointFileModule m)
        {
            BuildModule(noise, context, m.Input1);

            var module = new ScalePointModule(
                context.GetModule(m.Input1))
            {
                Name = m.Name,
                ScaleX = m.ScaleX,
                ScaleY = m.ScaleY,
                ScaleZ = m.ScaleZ,
            };

            return module;
        }

        private static IModule BuildSelectorModule(Noise3D noise, LibNoiseShaderFileLoadContext context, SelectorFileModule m)
        {
            BuildModule(noise, context, m.Input1);
            BuildModule(noise, context, m.Input2);
            BuildModule(noise, context, m.Control);

            var module = new SelectorModule(
                context.GetModule(m.Input1),
                context.GetModule(m.Input2),
                context.GetModule(m.Control))
            {
                Name = m.Name,
                EdgeFalloff = m.EdgeFalloff,
                LowerBound = m.LowerBound,
                UpperBound = m.UpperBound,
            };

            return module;
        }

        private static IModule BuildSphereModule(Noise3D noise, LibNoiseShaderFileLoadContext context, SphereFileModule m)
        {
            var module = new SphereModule()
            {
                Name = m.Name,
                Frequency = m.Frequency,
            };

            return module;
        }

        private static IModule BuildTerraceModule(Noise3D noise, LibNoiseShaderFileLoadContext context, TerraceFileModule m)
        {
            BuildModule(noise, context, m.Input1);

            var module = new TerraceModule(
                context.GetModule(m.Input1))
            {
                Name = m.Name,
                IsInverted = m.IsInverted,
            };

            foreach (float point in m.ControlPoints)
            {
                module.ControlPoints.Add(point);
            }

            return module;
        }

        private static IModule BuildTranslatePointModule(Noise3D noise, LibNoiseShaderFileLoadContext context, TranslatePointFileModule m)
        {
            BuildModule(noise, context, m.Input1);

            var module = new TranslatePointModule(
                context.GetModule(m.Input1))
            {
                Name = m.Name,
                TranslateX = m.TranslateX,
                TranslateY = m.TranslateY,
                TranslateZ = m.TranslateZ,
            };

            return module;
        }

        private static IModule BuildTurbulenceModule(Noise3D noise, LibNoiseShaderFileLoadContext context, TurbulenceFileModule m)
        {
            BuildModule(noise, context, m.Input1);

            var module = new TurbulenceModule(
                noise,
                context.GetModule(m.Input1))
            {
                Name = m.Name,
                Frequency = m.Frequency,
                Power = m.Power,
                Roughness = m.Roughness,
                SeedOffset = m.SeedOffset,
            };

            return module;
        }

        private static IModule BuildVoronoiModule(Noise3D noise, LibNoiseShaderFileLoadContext context, VoronoiFileModule m)
        {
            var module = new VoronoiModule(
                noise)
            {
                Name = m.Name,
                IsDistanceApplied = m.IsDistanceApplied,
                Displacement = m.Displacement,
                Frequency = m.Frequency,
                SeedOffset = m.SeedOffset,
            };

            return module;
        }

        public static IBuilder LoadBuilder(IFileBuilder builderFile, Noise3D noise)
        {
            if (builderFile is null)
            {
                throw new ArgumentNullException(nameof(builderFile));
            }

            if (noise is null)
            {
                throw new ArgumentNullException(nameof(noise));
            }

            var context = new LibNoiseShaderFileLoadContext();

            IBuilder builder = builderFile switch
            {
                CylinderFileBuilder m => BuildCylinderBuilder(noise, context, m),
                PlaneFileBuilder m => BuildPlaneBuilder(noise, context, m),
                SphereFileBuilder m => BuildSphereBuilder(noise, context, m),
                _ => throw new NotSupportedException($"Not supported builder type found: {builderFile.GetType().Name}"),
            };

            return builder;
        }

        private static void BuildBuilder(Noise3D noise, LibNoiseShaderFileLoadContext context, IFileBuilder builderFile)
        {
            if (builderFile is null)
            {
                throw new ArgumentNullException(nameof(builderFile));
            }

            if (context.GetBuilder(builderFile) is null)
            {
                context.AddBuilder(builderFile, LoadBuilder(builderFile, noise));
            }
        }

        private static IBuilder BuildCylinderBuilder(Noise3D noise, LibNoiseShaderFileLoadContext context, CylinderFileBuilder m)
        {
            BuildModule(noise, context, m.Source);

            var builder = new CylinderBuilder(
                context.GetModule(m.Source),
                noise.Seed)
            {
                Name = m.Name,
                LowerAngleBound = m.LowerAngleBound,
                LowerHeightBound = m.LowerHeightBound,
                UpperAngleBound = m.UpperAngleBound,
                UpperHeightBound = m.UpperHeightBound,
            };

            return builder;
        }

        private static IBuilder BuildPlaneBuilder(Noise3D noise, LibNoiseShaderFileLoadContext context, PlaneFileBuilder m)
        {
            BuildModule(noise, context, m.Source);

            var builder = new PlaneBuilder(
                context.GetModule(m.Source),
                noise.Seed)
            {
                Name = m.Name,
                IsSeamless = m.IsSeamless,
                LowerBoundX = m.LowerBoundX,
                UpperBoundX = m.UpperBoundX,
                LowerBoundY = m.LowerBoundY,
                UpperBoundY = m.UpperBoundY,
            };

            return builder;
        }

        private static IBuilder BuildSphereBuilder(Noise3D noise, LibNoiseShaderFileLoadContext context, SphereFileBuilder m)
        {
            BuildModule(noise, context, m.Source);

            var builder = new SphereBuilder(
                context.GetModule(m.Source),
                noise.Seed)
            {
                Name = m.Name,
                SouthLatBound = m.SouthLatBound,
                NorthLatBound = m.NorthLatBound,
                WestLonBound = m.WestLonBound,
                EastLonBound = m.EastLonBound,
            };

            return builder;
        }

        public static IRenderer LoadRenderer(IFileRenderer rendererFile, Noise3D noise)
        {
            if (rendererFile is null)
            {
                throw new ArgumentNullException(nameof(rendererFile));
            }

            if (noise is null)
            {
                throw new ArgumentNullException(nameof(noise));
            }

            var context = new LibNoiseShaderFileLoadContext();

            IRenderer renderer = rendererFile switch
            {
                BlendFileRenderer m => BuildBlendRenderer(noise, context, m),
                ImageFileRenderer m => BuildImageRenderer(noise, context, m),
                NormalFileRenderer m => BuildNormalRenderer(noise, context, m),
                _ => throw new NotSupportedException($"Not supported renderer type found: {rendererFile.GetType().Name}"),
            };

            return renderer;
        }

        private static void BuildRenderer(Noise3D noise, LibNoiseShaderFileLoadContext context, IFileRenderer rendererFile)
        {
            if (rendererFile is null)
            {
                throw new ArgumentNullException(nameof(rendererFile));
            }

            if (context.GetRenderer(rendererFile) is null)
            {
                context.AddRenderer(rendererFile, LoadRenderer(rendererFile, noise));
            }
        }

        private static IRenderer BuildBlendRenderer(Noise3D noise, LibNoiseShaderFileLoadContext context, BlendFileRenderer m)
        {
            BuildRenderer(noise, context, m.Input1);
            BuildRenderer(noise, context, m.Input2);

            var renderer = new BlendRenderer(
                context.GetRenderer(m.Input1),
                context.GetRenderer(m.Input2))
            {
                Name = m.Name,
            };

            return renderer;
        }

        private static IRenderer BuildImageRenderer(Noise3D noise, LibNoiseShaderFileLoadContext context, ImageFileRenderer m)
        {
            BuildBuilder(noise, context, m.Source);

            var renderer = new ImageRenderer(
                context.GetBuilder(m.Source))
            {
                Name = m.Name,
                IsWrapEnabled = m.IsWrapEnabled,
                IsLightEnabled = m.IsLightEnabled,
                LightAzimuth = m.LightAzimuth,
                LightBrightness = m.LightBrightness,
                LightColor = Color.FromArgb(
                    m.LightColor.A,
                    m.LightColor.R,
                    m.LightColor.G,
                    m.LightColor.B),
                LightContrast = m.LightContrast,
                LightElevation = m.LightElevation,
                LightIntensity = m.LightIntensity,
            };

            renderer.SetGradientPoints(m.GradientPoints.ToDictionary(
                t => t.Key,
                t => Color.FromArgb(t.Value.A, t.Value.R, t.Value.G, t.Value.B)));

            return renderer;
        }

        private static IRenderer BuildNormalRenderer(Noise3D noise, LibNoiseShaderFileLoadContext context, NormalFileRenderer m)
        {
            BuildBuilder(noise, context, m.Source);

            var renderer = new NormalRenderer(
                context.GetBuilder(m.Source))
            {
                Name = m.Name,
            };

            return renderer;
        }
    }
}
