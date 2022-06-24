namespace JeremyAnsel.LibNoiseShader.Modules
{
    public interface IModule
    {
        string Name { get; set; }

        int RequiredSourceModuleCount { get; }

        IModule GetSourceModule(int index);

        void GenerateModuleContext(HlslContext context);

        void GenerateModuleContext(CSharpContext context);

        float GetValue(float x, float y, float z);

        float GetValue(Float3 coords);

        string GetHlslBody(HlslContext context);

        string GetFullHlsl();

        string GetCSharpBody(CSharpContext context);

        string GetFullCSharp();
    }
}
