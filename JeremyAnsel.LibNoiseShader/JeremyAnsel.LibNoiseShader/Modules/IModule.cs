using System.Text;

namespace JeremyAnsel.LibNoiseShader.Modules
{
    public interface IModule
    {
        string? Name { get; set; }

        int RequiredSourceModuleCount { get; }

        IModule? GetSourceModule(int index);

        void GenerateModuleContext(HlslContext? context);

        void GenerateModuleContext(CSharpContext? context);

        float GetValue(float x, float y, float z);

        float GetValue(Float3 coords);

        string GetHlslBody(HlslContext context);

        string GetFullHlsl();


        int EmitHlslMaxDepth();

        void EmitHlsl(HlslContext context);

        void EmitHlslHeader(HlslContext context, StringBuilder header);

        bool HasHlslSettings();

        void EmitHlslSettings(StringBuilder body);

        bool HasHlslCoords(int index);

        void EmitHlslCoords(StringBuilder body, int index);

        int GetHlslFunctionParametersCount();

        void EmitHlslFunction(StringBuilder body);

        string EmitFullHlsl();

        string GetCSharpBody(CSharpContext context);

        string GetFullCSharp();
    }
}
