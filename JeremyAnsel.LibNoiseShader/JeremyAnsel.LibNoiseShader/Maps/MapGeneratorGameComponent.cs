﻿using JeremyAnsel.DirectX.D3D11;
using JeremyAnsel.DirectX.D3DCompiler;
using JeremyAnsel.DirectX.GameWindow;
using JeremyAnsel.LibNoiseShader.Renderers;
using System;
using System.Globalization;

namespace JeremyAnsel.LibNoiseShader.Maps
{
    internal sealed class MapGeneratorGameComponent : IGameComponent
    {
        private readonly Noise3D noise;

        private readonly IRenderer renderer;

        private DeviceResources? deviceResources;

        private D3D11VertexShader? vertexShader;

        private D3D11PixelShader? pixelShader;

        public MapGeneratorGameComponent(Noise3D? noise, IRenderer? renderer)
        {
            this.noise = noise ?? throw new ArgumentNullException(nameof(noise));
            this.renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        }

        public D3D11FeatureLevel MinimalFeatureLevel
        {
            get
            {
                return D3D11FeatureLevel.FeatureLevel100;
            }
        }

        public void CreateDeviceDependentResources(DeviceResources resources)
        {
            D3DCompileOptions compileOptions = D3DCompileOptions.SkipOptimization | D3DCompileOptions.SkipValidation;

            this.deviceResources = resources;

            string dx = (1.0f / this.deviceResources.BackBufferWidth).ToString(CultureInfo.InvariantCulture);
            string dy = (1.0f / this.deviceResources.BackBufferHeight).ToString(CultureInfo.InvariantCulture);
            string width = this.deviceResources.BackBufferWidth.ToString(CultureInfo.InvariantCulture);
            string height = this.deviceResources.BackBufferHeight.ToString(CultureInfo.InvariantCulture);

            string vertexShaderHlsl = @"
struct PixelShaderInput
{
    float4 pos : SV_POSITION;
    float2 tex : TEXCOORD0;
};

PixelShaderInput main(uint id : SV_VertexID)
{
    PixelShaderInput output;
    output.pos = float4(((id << 1) & 2) * 2.0f - 1.0f, (id & 2) * -2.0f + 1.0f, 0.5f, 1.0f);
    output.tex = float2(output.pos.x, -output.pos.y);
    return output;
}
".NormalizeEndLines();

            D3DCompile.Compile(
                vertexShaderHlsl,
                nameof(vertexShaderHlsl),
                "main",
                D3DTargets.VS_4_0,
                compileOptions,
                out byte[] vertexShaderBytecode,
                out string _);

            this.vertexShader = this.deviceResources.D3DDevice.CreateVertexShader(vertexShaderBytecode, null);

            string pixelShaderHlsl = this.renderer.GetFullHlsl() + @"
struct PixelShaderInput
{
    float4 pos : SV_POSITION;
    float2 tex : TEXCOORD0;
};

float4 main(PixelShaderInput input) : SV_TARGET
{
    //input.tex -= float2(" + dx + @", " + dy + @");
    input.tex -= float2( abs(ddx(input.tex.x)) * 0.5f, abs(ddy(input.tex.y)) * 0.5f );

    //float4 color = Renderer_root(input.tex.x, input.tex.y, " + width + @", " + height + @");
    float4 color = Renderer_root(input.tex.x, input.tex.y);
    return color;
}
".NormalizeEndLines();

            D3DCompile.Compile(
                pixelShaderHlsl,
                nameof(pixelShaderHlsl),
                "main",
                D3DTargets.PS_4_0,
                compileOptions,
                out byte[] pixelShaderBytecode,
                out string _);

            this.pixelShader = this.deviceResources.D3DDevice.CreatePixelShader(pixelShaderBytecode, null);
        }

        public void ReleaseDeviceDependentResources()
        {
            D3D11Utils.DisposeAndNull(ref this.vertexShader);
            D3D11Utils.DisposeAndNull(ref this.pixelShader);
        }

        public void CreateWindowSizeDependentResources()
        {
        }

        public void ReleaseWindowSizeDependentResources()
        {
        }

        public void Update(ITimer? timer)
        {
        }

        public void Render()
        {
            var context = this.deviceResources!.D3DContext;

            context.OutputMergerSetRenderTargets(new[] { this.deviceResources.D3DRenderTargetView }, null);
            //context.ClearRenderTargetView(this.deviceResources.D3DRenderTargetView, XMKnownColor.Black);

            context.VertexShaderSetShader(this.vertexShader, null);
            context.PixelShaderSetShader(this.pixelShader, null);
            context.InputAssemblerSetInputLayout(null);
            context.InputAssemblerSetPrimitiveTopology(D3D11PrimitiveTopology.TriangleStrip);
            context.Draw(4, 0);
        }
    }
}
