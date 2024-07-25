using System;
using System.Text;

namespace JeremyAnsel.LibNoiseShader.Modules
{
    public sealed class RotatePointModule : ModuleBase
    {
        private float angleX;

        private float angleY;

        private float angleZ;

        private float matrixX1;

        private float matrixX2;

        private float matrixX3;

        private float matrixY1;

        private float matrixY2;

        private float matrixY3;

        private float matrixZ1;

        private float matrixZ2;

        private float matrixZ3;

        public RotatePointModule(IModule? module0)
        {
            this.SetSourceModule(0, module0);

            this.SetAngles(0.0f, 0.0f, 0.0f);
        }

        public float AngleX
        {
            get
            {
                return this.angleX;
            }

            set
            {
                this.SetAngles(value, this.AngleY, this.AngleZ);
            }
        }

        public float AngleY
        {
            get
            {
                return this.angleY;
            }

            set
            {
                this.SetAngles(this.AngleX, value, this.AngleZ);
            }
        }

        public float AngleZ
        {
            get
            {
                return this.angleZ;
            }

            set
            {
                this.SetAngles(this.AngleX, this.AngleY, value);
            }
        }

        public override int RequiredSourceModuleCount => 1;

        public void SetAngles(float angleX, float angleY, float angleZ)
        {
            this.angleX = angleX;
            this.angleY = angleY;
            this.angleZ = angleZ;

            float radX = angleX * (float)Math.PI / 180.0f;
            float radY = angleY * (float)Math.PI / 180.0f;
            float radZ = angleZ * (float)Math.PI / 180.0f;

            float cosX = (float)Math.Cos(radX);
            float cosY = (float)Math.Cos(radY);
            float cosZ = (float)Math.Cos(radZ);
            float sinX = (float)Math.Sin(radX);
            float sinY = (float)Math.Sin(radY);
            float sinZ = (float)Math.Sin(radZ);

            this.matrixX1 = sinY * sinX * sinZ + cosY * cosZ;
            this.matrixY1 = cosX * sinZ;
            this.matrixZ1 = sinY * cosZ - cosY * sinX * sinZ;
            this.matrixX2 = sinY * sinX * cosZ - cosY * sinZ;
            this.matrixY2 = cosX * cosZ;
            this.matrixZ2 = -cosY * sinX * cosZ - sinY * sinZ;
            this.matrixX3 = -sinY * cosX;
            this.matrixY3 = sinX;
            this.matrixZ3 = cosY * cosX;
        }

        public override float GetValue(float x, float y, float z)
        {
            float nx = (this.matrixX1 * x) + (this.matrixY1 * y) + (this.matrixZ1 * z);
            float ny = (this.matrixX2 * x) + (this.matrixY2 * y) + (this.matrixZ2 * z);
            float nz = (this.matrixX3 * x) + (this.matrixY3 * y) + (this.matrixZ3 * z);

            return this.GetSourceModule(0)!.GetValue(nx, ny, nz);
        }

        public override int EmitHlslMaxDepth()
        {
            return 1;
        }

        public override void EmitHlsl(HlslContext context)
        {
            context.EmitHeader(this);
            context.EmitCoords(this, 0, false);
            this.GetSourceModule(0)!.EmitHlsl(context);
            context.EmitSettings(this);
            context.EmitFunction(this, true);
        }

        public override void EmitHlslHeader(HlslContext context, StringBuilder header)
        {
            string key = nameof(RotatePointModule);

            header.AppendTabFormatLine("float3x3 {0}_RotateMatrix = (float3x3)0;", key);
        }

        public override bool HasHlslSettings()
        {
            return true;
        }

        public override void EmitHlslSettings(StringBuilder body)
        {
            string key = nameof(RotatePointModule);

            body.AppendTabFormatLine(
                2,
                "{0}_RotateMatrix = float3x3( {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9} );",
                key,
                this.matrixX1, this.matrixY1, this.matrixZ1,
                this.matrixX2, this.matrixY2, this.matrixZ2,
                this.matrixX3, this.matrixY3, this.matrixZ3);
        }

        public override bool HasHlslCoords(int index)
        {
            return true;
        }

        public override void EmitHlslCoords(StringBuilder body, int index)
        {
            body.AppendTabFormatLine(
                2,
                "coords = mul( float3x3( {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8} ), coords);",
                this.matrixX1, this.matrixY1, this.matrixZ1,
                this.matrixX2, this.matrixY2, this.matrixZ2,
                this.matrixX3, this.matrixY3, this.matrixZ3);
        }

        public override int GetHlslFunctionParametersCount()
        {
            return 1;
        }

        public override void EmitHlslFunction(StringBuilder body)
        {
            body.AppendTabFormatLine(2, "result = param0;");
        }

        public override string GetCSharpBody(CSharpContext context)
        {
            var sb = new StringBuilder();
            string module0 = context.GetModuleName(this.GetSourceModule(0));
            string name = context.GetModuleName(this);
            string type = context.GetModuleType(this);

            sb.AppendTabFormatLine("{0} {1} = new({2});", type, name, module0);
            sb.AppendTabFormatLine("{0}.SetAngles({1}, {2}, {3});", name, this.AngleX, this.AngleY, this.AngleZ);

            return sb.ToString();
        }
    }
}
