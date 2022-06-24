using System.Drawing;

namespace JeremyAnsel.LibNoiseShader.Renderers
{
    public interface IRenderer
    {
        string Name { get; set; }

        int RequiredSourceRendererCount { get; }

        IRenderer GetSourceRenderer(int index);

        void GenerateModuleContext(HlslContext context);

        void GenerateModuleContext(CSharpContext context);

        Color GetColor(float x, float y, int width, int height);

        string GetHlslBody(HlslContext context);

        string GetFullHlsl();

        string GetCSharpBody(CSharpContext context);

        string GetFullCSharp();
    }
}
