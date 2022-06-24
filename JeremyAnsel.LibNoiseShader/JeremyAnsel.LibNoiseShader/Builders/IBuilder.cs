using JeremyAnsel.LibNoiseShader.Modules;

namespace JeremyAnsel.LibNoiseShader.Builders
{
    public interface IBuilder
    {
        string Name { get; set; }

        IModule GetSourceModule();

        void GenerateModuleContext(HlslContext context);

        void GenerateModuleContext(CSharpContext context);

        float GetValue(float x, float y);

        string GetHlslBody(HlslContext context);

        string GetFullHlsl();

        string GetCSharpBody(CSharpContext context);

        string GetFullCSharp();
    }
}
