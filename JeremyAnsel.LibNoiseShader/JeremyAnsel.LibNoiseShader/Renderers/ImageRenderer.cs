using JeremyAnsel.LibNoiseShader.Builders;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace JeremyAnsel.LibNoiseShader.Renderers
{
    public sealed class ImageRenderer : RendererBase
    {
        private readonly GradientColor gradient = new();

        private readonly IBuilder sourceBuilder;

        public ImageRenderer(IBuilder builder)
        {
            this.sourceBuilder = builder ?? throw new ArgumentNullException(nameof(builder));
            this.IsWrapEnabled = false;
            this.IsLightEnabled = false;
            this.LightAzimuth = 45.0f;
            this.LightBrightness = 1.0f;
            this.LightColor = Color.White;
            this.LightContrast = 1.0f;
            this.LightElevation = 45.0f;
            this.LightIntensity = 1.0f;

            this.BuildGrayscaleGradient();
        }

        public ImageRenderer(
            IBuilder builder,
            bool isWrapEnabled)
        {
            this.sourceBuilder = builder ?? throw new ArgumentNullException(nameof(builder));
            this.IsWrapEnabled = isWrapEnabled;
            this.IsLightEnabled = false;
            this.LightAzimuth = 45.0f;
            this.LightBrightness = 1.0f;
            this.LightColor = Color.White;
            this.LightContrast = 1.0f;
            this.LightElevation = 45.0f;
            this.LightIntensity = 1.0f;

            this.BuildGrayscaleGradient();
        }

        public ImageRenderer(
            IBuilder builder,
            bool isWrapEnabled,
            bool isLightEnabled)
        {
            this.sourceBuilder = builder ?? throw new ArgumentNullException(nameof(builder));
            this.IsWrapEnabled = isWrapEnabled;
            this.IsLightEnabled = isLightEnabled;
            this.LightAzimuth = 45.0f;
            this.LightBrightness = 1.0f;
            this.LightColor = Color.White;
            this.LightContrast = 1.0f;
            this.LightElevation = 45.0f;
            this.LightIntensity = 1.0f;

            this.BuildGrayscaleGradient();
        }

        public ImageRenderer(
            IBuilder builder,
            bool isWrapEnabled,
            float lightAzimuth,
            float lightBrightness,
            Color lightColor,
            float lightContrast,
            float lightElevation,
            float lightIntensity)
        {
            this.sourceBuilder = builder ?? throw new ArgumentNullException(nameof(builder));
            this.IsWrapEnabled = isWrapEnabled;
            this.IsLightEnabled = true;
            this.LightAzimuth = lightAzimuth;
            this.LightBrightness = lightBrightness;
            this.LightColor = lightColor;
            this.LightContrast = lightContrast;
            this.LightElevation = lightElevation;
            this.LightIntensity = lightIntensity;

            this.BuildGrayscaleGradient();
        }

        public override int RequiredSourceRendererCount => 0;

        public bool IsWrapEnabled { get; set; }

        public bool IsLightEnabled { get; set; }

        public float LightAzimuth { get; set; }

        public float LightBrightness { get; set; }

        public Color LightColor { get; set; }

        public float LightContrast { get; set; }

        public float LightElevation { get; set; }

        public float LightIntensity { get; set; }

        public void ClearGradient()
        {
            this.gradient.GradientPoints.Clear();
        }

        public IDictionary<float, Color> GetGradient()
        {
            return this.gradient.GradientPoints;
        }

        public void AddGradientPoint(float position, Color color)
        {
            this.gradient.GradientPoints.Add(position, color);
        }

        public void BuildGrayscaleGradient()
        {
            this.SetGradientPoints(GetDefaultGrayscaleGradient());
        }

        public static IDictionary<float, Color> GetDefaultGrayscaleGradient()
        {
            var points = new SortedList<float, Color>
            {
                { -1.0f, Color.Black },
                { 1.0f, Color.White }
            };

            return points;
        }

        public void BuildTerrainGradient()
        {
            this.SetGradientPoints(GetDefaultTerrainGradient());
        }

        public static IDictionary<float, Color> GetDefaultTerrainGradient()
        {
            var points = new SortedList<float, Color>
            {
                { -1.00f, Color.FromArgb(255, 0, 0, 128) },
                { -0.20f, Color.FromArgb(255, 32, 64, 128) },
                { -0.04f, Color.FromArgb(255, 64, 96, 192) },
                { -0.02f, Color.FromArgb(255, 192, 192, 128) },
                { 0.00f, Color.FromArgb(255, 0, 192, 0) },
                { 0.25f, Color.FromArgb(255, 192, 192, 0) },
                { 0.50f, Color.FromArgb(255, 160, 96, 64) },
                { 0.75f, Color.FromArgb(255, 128, 255, 255) },
                { 1.00f, Color.FromArgb(255, 255, 255, 255) }
            };

            return points;
        }

        public void SetGradientPoints(IEnumerable<KeyValuePair<float, Color>> points)
        {
            if (points is null)
            {
                throw new ArgumentNullException(nameof(points));
            }

            this.gradient.GradientPoints.Clear();

            foreach (var point in points)
            {
                this.gradient.GradientPoints.Add(point);
            }
        }

        public IBuilder GetSourceBuilder()
        {
            return this.sourceBuilder;
        }

        public override void GenerateModuleContext(HlslContext context)
        {
            base.GenerateModuleContext(context);

            context.AddBuilder(this.GetSourceBuilder());
        }

        public override void GenerateModuleContext(CSharpContext context)
        {
            base.GenerateModuleContext(context);

            context.AddBuilder(this.GetSourceBuilder());
        }

        public override Color GetColor(float x, float y, int width, int height)
        {
            float dx = 2.0f / width;
            float dy = 2.0f / height;
            float dx_half = dx * 0.5f;
            float dy_half = dy * 0.5f;

            // Get the color based on the value at the current point in the noise
            // map.
            Color destColor = this.gradient.GetColor(this.GetSourceBuilder().GetValue(x, y));

            // If lighting is enabled, calculate the light intensity based on the
            // rate of change at the current point in the noise map.
            float lightIntensity;

            if (this.IsLightEnabled)
            {
                // Calculate the positions of the current point's four-neighbors.
                float xLeftOffset;
                float xRightOffset;
                float yUpOffset;
                float yDownOffset;

                if (this.IsWrapEnabled)
                {
                    if (Math.Abs(-1.0f - x) < dx_half)
                    {
                        xLeftOffset = 1.0f - dx;
                        xRightOffset = dx;
                    }
                    else if (Math.Abs(1.0f - dx - x) < dx_half)
                    {
                        xLeftOffset = -dx;
                        xRightOffset = -(1.0f - dx);
                    }
                    else
                    {
                        xLeftOffset = -dx;
                        xRightOffset = dx;
                    }

                    if (Math.Abs(-1.0f - y) < dy_half)
                    {
                        yDownOffset = 1.0f - dy;
                        yUpOffset = dy;
                    }
                    else if (Math.Abs(1.0f - dy - y) < dy_half)
                    {
                        yDownOffset = -dy;
                        yUpOffset = -(1.0f - dy);
                    }
                    else
                    {
                        yDownOffset = -dy;
                        yUpOffset = dy;
                    }
                }
                else
                {
                    if (Math.Abs(-1.0f - x) < dx_half)
                    {
                        xLeftOffset = 0.0f;
                        xRightOffset = dx;
                    }
                    else if (Math.Abs(1.0f - dx - x) < dx_half)
                    {
                        xLeftOffset = -dx;
                        xRightOffset = 0.0f;
                    }
                    else
                    {
                        xLeftOffset = -dx;
                        xRightOffset = dx;
                    }

                    if (Math.Abs(-1.0f - y) < dy_half)
                    {
                        yDownOffset = 0.0f;
                        yUpOffset = dy;
                    }
                    else if (Math.Abs(1.0f - dy - y) < dy_half)
                    {
                        yDownOffset = -dy;
                        yUpOffset = 0.0f;
                    }
                    else
                    {
                        yDownOffset = -dy;
                        yUpOffset = dy;
                    }
                }

                // Get the noise value of the current point in the source noise map
                // and the noise values of its four-neighbors.
                float nc = this.GetSourceBuilder().GetValue(x, y);
                float nl = this.GetSourceBuilder().GetValue(x + xLeftOffset, y);
                float nr = this.GetSourceBuilder().GetValue(x + xRightOffset, y);
                float nd = this.GetSourceBuilder().GetValue(x, y + yDownOffset);
                float nu = this.GetSourceBuilder().GetValue(x, y + yUpOffset);

                // Now we can calculate the lighting intensity.
                lightIntensity = this.CalcLightIntensity(nc, nl, nr, nd, nu);
                lightIntensity *= this.LightBrightness;
            }
            else
            {
                // These values will apply no lighting to the destination image.
                lightIntensity = 1.0f;
            }

            // Blend the destination color, background color, and the light
            // intensity together, then update the destination image with that
            // color.
            Color color = this.CalcDestinationColor(destColor, lightIntensity);
            return color;
        }

        private float CalcLightIntensity(float center, float left, float right, float down, float up)
        {
            float cosAzimuth = (float)Math.Cos(this.LightAzimuth * (float)Math.PI / 180.0f);
            float sinAzimuth = (float)Math.Sin(this.LightAzimuth * (float)Math.PI / 180.0f);
            float cosElevation = (float)Math.Cos(this.LightElevation * (float)Math.PI / 180.0f);
            float sinElevation = (float)Math.Sin(this.LightElevation * (float)Math.PI / 180.0f);

            const float maxIntensity = 1.0f;
            float sqrt2 = (float)Math.Sqrt(2.0f);

            float io = maxIntensity * sqrt2 * sinElevation / 2.0f;
            float ix = (maxIntensity - io) * this.LightContrast * sqrt2 * cosElevation * cosAzimuth;
            float iy = (maxIntensity - io) * this.LightContrast * sqrt2 * cosElevation * sinAzimuth;

            float intensity = ix * (left - right) + iy * (down - up) + io;

            if (intensity < 0.0f)
            {
                intensity = 0.0f;
            }

            return intensity;
        }

        private string CalcLightIntensityHlsl(string functionPrefix)
        {
            var sb = new StringBuilder();

            sb.AppendTabFormatLine(0, "float {0}_CalcLightIntensity(float center, float left, float right, float down, float up)", functionPrefix);
            sb.AppendTabFormatLine(0, "{");
            sb.AppendTabFormatLine(1, "float sinAzimuth;");
            sb.AppendTabFormatLine(1, "float cosAzimuth;");
            sb.AppendTabFormatLine(1, "sincos(radians({0}), sinAzimuth, cosAzimuth);", this.LightAzimuth);
            sb.AppendTabFormatLine(1, "float sinElevation;");
            sb.AppendTabFormatLine(1, "float cosElevation;");
            sb.AppendTabFormatLine(1, "sincos(radians({0}), sinElevation, cosElevation);", this.LightElevation);
            sb.AppendTabFormatLine();
            sb.AppendTabFormatLine(1, "float maxIntensity = 1.0f;");
            sb.AppendTabFormatLine(1, "float sqrt2 = sqrt(2.0f);");
            sb.AppendTabFormatLine(1, "float io = maxIntensity * sqrt2 * sinElevation / 2.0f;");
            sb.AppendTabFormatLine(1, "float ix = (maxIntensity - io) * {0} * sqrt2 * cosElevation * cosAzimuth;", this.LightContrast);
            sb.AppendTabFormatLine(1, "float iy = (maxIntensity - io) * {0} * sqrt2 * cosElevation * sinAzimuth;", this.LightContrast);
            sb.AppendTabFormatLine(1, "float intensity = ix * (left - right) + iy * (down - up) + io;");
            sb.AppendTabFormatLine();
            sb.AppendTabFormatLine(1, "intensity = max(intensity, 0.0f);");
            sb.AppendTabFormatLine();
            sb.AppendTabFormatLine(1, "return intensity;");
            sb.AppendTabFormatLine(0, "}");

            return sb.ToString();
        }

        private Color CalcDestinationColor(Color source, float lightValue)
        {
            float sourceRed = source.R / 255.0f;
            float sourceGreen = source.G / 255.0f;
            float sourceBlue = source.B / 255.0f;
            float sourceAlpha = source.A / 255.0f;

            // First, blend the source color to the background color using the alpha
            // of the source color.
            float red = Interpolation.Linear(1.0f, sourceRed, sourceAlpha);
            float green = Interpolation.Linear(1.0f, sourceGreen, sourceAlpha);
            float blue = Interpolation.Linear(1.0f, sourceBlue, sourceAlpha);

            if (this.IsLightEnabled)
            {
                // Now calculate the light color.
                float lightRed = lightValue * this.LightColor.R / 255.0f;
                float lightGreen = lightValue * this.LightColor.G / 255.0f;
                float lightBlue = lightValue * this.LightColor.B / 255.0f;

                // Apply the light color to the new color.
                red *= lightRed;
                green *= lightGreen;
                blue *= lightBlue;
            }

            // Clamp the color channels to the (0..1) range.
            red = (red < 0.0f) ? 0.0f : red;
            red = (red > 1.0f) ? 1.0f : red;
            green = (green < 0.0f) ? 0.0f : green;
            green = (green > 1.0f) ? 1.0f : green;
            blue = (blue < 0.0f) ? 0.0f : blue;
            blue = (blue > 1.0f) ? 1.0f : blue;

            // Rescale the color channels to the (0..255) range and return the new color.
            return Color.FromArgb(
                source.A,
                (byte)(red * 255.0f),
                (byte)(green * 255.0f),
                (byte)(blue * 255.0f));
        }

        private string CalcDestinationColorHlsl(string functionPrefix)
        {
            var sb = new StringBuilder();

            sb.AppendTabFormatLine(0, "float4 {0}_CalcDestinationColor(float4 source, float lightValue)", functionPrefix);
            sb.AppendTabFormatLine(0, "{");

            // First, blend the source color to the background color using the alpha
            // of the source color.
            sb.AppendTabFormatLine(1, "float red = Interpolation_Linear(1.0f, source.x, source.w);");
            sb.AppendTabFormatLine(1, "float green = Interpolation_Linear(1.0f, source.y, source.w);");
            sb.AppendTabFormatLine(1, "float blue = Interpolation_Linear(1.0f, source.z, source.w);");
            sb.AppendTabFormatLine();

            if (this.IsLightEnabled)
            {
                // Now calculate the light color.
                sb.AppendTabFormatLine(1, "float lightRed = lightValue * {0};", this.LightColor.R / 255.0f);
                sb.AppendTabFormatLine(1, "float lightGreen = lightValue * {0};", this.LightColor.G / 255.0f);
                sb.AppendTabFormatLine(1, "float lightBlue = lightValue * {0};", this.LightColor.B / 255.0f);

                // Apply the light color to the new color.
                sb.AppendTabFormatLine(1, "red *= lightRed;");
                sb.AppendTabFormatLine(1, "green *= lightGreen;");
                sb.AppendTabFormatLine(1, "blue *= lightBlue;");
            }

            // clamp the color channels to the (0..1) range.
            sb.AppendTabFormatLine(1, "red = clamp(red, 0.0f, 1.0f);");
            sb.AppendTabFormatLine(1, "green = clamp(green, 0.0f, 1.0f);");
            sb.AppendTabFormatLine(1, "blue = clamp(blue, 0.0f, 1.0f);");
            sb.AppendTabFormatLine();

            // Rescale the color channels to the (0..255) range and return the new color.
            sb.AppendTabFormatLine(1, "return float4(red, green, blue, source.w);");
            sb.AppendTabFormatLine(0, "}");

            return sb.ToString();
        }

        public override string GetHlslBody(HlslContext context)
        {
            var sb = new StringBuilder();
            string rendererName = context.GetRendererName(this);

            sb.AppendLine(this.gradient.GetHlslBody(rendererName));
            sb.AppendLine(this.CalcLightIntensityHlsl(rendererName));
            sb.AppendLine(this.CalcDestinationColorHlsl(rendererName));

            sb.AppendTabFormatLine(context.GetRendererFunctionDefinition(this));
            sb.AppendTabFormatLine(0, "{");
            //sb.AppendTabFormatLine(1, "float dx = 2.0f / width;");
            //sb.AppendTabFormatLine(1, "float dy = 2.0f / height;");
            sb.AppendTabFormatLine(1, "float dx = abs(ddx(x));");
            sb.AppendTabFormatLine(1, "float dy = abs(ddy(y));");
            sb.AppendTabFormatLine(1, "float dx_half = dx * 0.5f;");
            sb.AppendTabFormatLine(1, "float dy_half = dy * 0.5f;");
            sb.AppendTabFormatLine();

            // Get the color based on the value at the current point in the noise
            // map.
            sb.AppendTabFormatLine(1, "float builderValue = {0}(x, y);", context.GetBuilderName(this.GetSourceBuilder()));
            sb.AppendTabFormatLine(1, "float4 destColor = {0}_GetGradientColor(builderValue);", rendererName);
            sb.AppendTabFormatLine();

            // If lighting is enabled, calculate the light intensity based on the
            // rate of change at the current point in the noise map.

            if (this.IsLightEnabled)
            {
                // Calculate the positions of the current point's four-neighbors.
                sb.AppendTabFormatLine(1, "float xLeftOffset;");
                sb.AppendTabFormatLine(1, "float xRightOffset;");
                sb.AppendTabFormatLine(1, "float yUpOffset;");
                sb.AppendTabFormatLine(1, "float yDownOffset;");
                sb.AppendTabFormatLine();

                if (this.IsWrapEnabled)
                {
                    sb.AppendTabFormatLine(1, "float x1 = -1.0f - x;");
                    sb.AppendTabFormatLine(1, "float x2 = 1.0f - dx - x;");
                    sb.AppendTabFormatLine(1, "if (x1 * sign(x1) < dx_half)");
                    sb.AppendTabFormatLine(1, "{");
                    sb.AppendTabFormatLine(2, "xLeftOffset = 1.0f - dx;");
                    sb.AppendTabFormatLine(2, "xRightOffset = dx;");
                    sb.AppendTabFormatLine(1, "}");
                    sb.AppendTabFormatLine(1, "else if (x2 * sign(x2) < dx_half)");
                    sb.AppendTabFormatLine(1, "{");
                    sb.AppendTabFormatLine(2, "xLeftOffset = -dx;");
                    sb.AppendTabFormatLine(2, "xRightOffset = -(1.0f - dx);");
                    sb.AppendTabFormatLine(1, "}");
                    sb.AppendTabFormatLine(1, "else");
                    sb.AppendTabFormatLine(1, "{");
                    sb.AppendTabFormatLine(2, "xLeftOffset = -dx;");
                    sb.AppendTabFormatLine(2, "xRightOffset = dx;");
                    sb.AppendTabFormatLine(1, "}");
                    sb.AppendTabFormatLine();
                    sb.AppendTabFormatLine(1, "float y1 = -1.0f - y;");
                    sb.AppendTabFormatLine(1, "float y2 = 1.0f - dy - y;");
                    sb.AppendTabFormatLine(1, "if (y1 * sign(y1) < dy_half)");
                    sb.AppendTabFormatLine(1, "{");
                    sb.AppendTabFormatLine(2, "yDownOffset = 1.0f - dy;");
                    sb.AppendTabFormatLine(2, "yUpOffset = dy;");
                    sb.AppendTabFormatLine(1, "}");
                    sb.AppendTabFormatLine(1, "else if (y2 * sign(y2) < dy_half)");
                    sb.AppendTabFormatLine(1, "{");
                    sb.AppendTabFormatLine(2, "yDownOffset = -dy;");
                    sb.AppendTabFormatLine(2, "yUpOffset = -(1.0f - dy);");
                    sb.AppendTabFormatLine(1, "}");
                    sb.AppendTabFormatLine(1, "else");
                    sb.AppendTabFormatLine(1, "{");
                    sb.AppendTabFormatLine(2, "yDownOffset = -dy;");
                    sb.AppendTabFormatLine(2, "yUpOffset = dy;");
                    sb.AppendTabFormatLine(1, "}");
                }
                else
                {
                    sb.AppendTabFormatLine(1, "float x1 = -1.0f - x;");
                    sb.AppendTabFormatLine(1, "float x2 = 1.0f - dx - x;");
                    sb.AppendTabFormatLine(1, "if (x1 * sign(x1) < dx_half)");
                    sb.AppendTabFormatLine(1, "{");
                    sb.AppendTabFormatLine(2, "xLeftOffset = 0.0f;");
                    sb.AppendTabFormatLine(2, "xRightOffset = dx;");
                    sb.AppendTabFormatLine(1, "}");
                    sb.AppendTabFormatLine(1, "else if (x2 * sign(x2) < dx_half)");
                    sb.AppendTabFormatLine(1, "{");
                    sb.AppendTabFormatLine(2, "xLeftOffset = -dx;");
                    sb.AppendTabFormatLine(2, "xRightOffset = 0.0f;");
                    sb.AppendTabFormatLine(1, "}");
                    sb.AppendTabFormatLine(1, "else");
                    sb.AppendTabFormatLine(1, "{");
                    sb.AppendTabFormatLine(2, "xLeftOffset = -dx;");
                    sb.AppendTabFormatLine(2, "xRightOffset = dx;");
                    sb.AppendTabFormatLine(1, "}");
                    sb.AppendTabFormatLine();
                    sb.AppendTabFormatLine(1, "float y1 = -1.0f - y;");
                    sb.AppendTabFormatLine(1, "float y2 = 1.0f - dy - y;");
                    sb.AppendTabFormatLine(1, "if (y1 * sign(y1) < dy_half)");
                    sb.AppendTabFormatLine(1, "{");
                    sb.AppendTabFormatLine(2, "yDownOffset = 0.0f;");
                    sb.AppendTabFormatLine(2, "yUpOffset = dy;");
                    sb.AppendTabFormatLine(1, "}");
                    sb.AppendTabFormatLine(1, "else if (y2 * sign(y2) < dy_half)");
                    sb.AppendTabFormatLine(1, "{");
                    sb.AppendTabFormatLine(2, "yDownOffset = -dy;");
                    sb.AppendTabFormatLine(2, "yUpOffset = 0.0f;");
                    sb.AppendTabFormatLine(1, "}");
                    sb.AppendTabFormatLine(1, "else");
                    sb.AppendTabFormatLine(1, "{");
                    sb.AppendTabFormatLine(2, "yDownOffset = -dy;");
                    sb.AppendTabFormatLine(2, "yUpOffset = dy;");
                    sb.AppendTabFormatLine(1, "}");
                }

                sb.AppendTabFormatLine();

                // Get the noise value of the current point in the source noise map
                // and the noise values of its four-neighbors.
                sb.AppendTabFormatLine(1, "float nc = {0}(x, y);", context.GetBuilderName(this.GetSourceBuilder()));
                sb.AppendTabFormatLine(1, "float nl = {0}(x + xLeftOffset, y);", context.GetBuilderName(this.GetSourceBuilder()));
                sb.AppendTabFormatLine(1, "float nr = {0}(x + xRightOffset, y);", context.GetBuilderName(this.GetSourceBuilder()));
                sb.AppendTabFormatLine(1, "float nd = {0}(x, y + yDownOffset);", context.GetBuilderName(this.GetSourceBuilder()));
                sb.AppendTabFormatLine(1, "float nu = {0}(x, y + yUpOffset);", context.GetBuilderName(this.GetSourceBuilder()));

                // Now we can calculate the lighting intensity.
                sb.AppendTabFormatLine(1, "float lightIntensity = {0}_CalcLightIntensity(nc, nl, nr, nd, nu);", rendererName);
                sb.AppendTabFormatLine(1, "lightIntensity *= {0};", this.LightBrightness);
            }
            else
            {
                // These values will apply no lighting to the destination image.
                sb.AppendTabFormatLine(1, "float lightIntensity = 1.0f;");
            }

            // Blend the destination color, background color, and the light
            // intensity together, then update the destination image with that
            // color.
            sb.AppendTabFormatLine(1, "float4 color = {0}_CalcDestinationColor(destColor, lightIntensity);", rendererName);
            sb.AppendTabFormatLine(1, "return color;");
            sb.AppendTabFormatLine(0, "}");

            return sb.ToString();
        }

        public override string GetCSharpBody(CSharpContext context)
        {
            var sb = new StringBuilder();
            string builder0 = context.GetBuilderName(this.GetSourceBuilder());
            string name = context.GetRendererName(this);
            string type = context.GetRendererType(this);

            sb.AppendTabFormatLine("{0} {1} = new({2});", type, name, builder0);
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "IsWrapEnabled = {0},", this.IsWrapEnabled);
            sb.AppendTabFormatLine(1, "IsLightEnabled = {0},", this.IsLightEnabled);
            sb.AppendTabFormatLine(1, "LightAzimuth = {0},", this.LightAzimuth);
            sb.AppendTabFormatLine(1, "LightBrightness = {0},", this.LightBrightness);
            sb.AppendTabFormatLine(1, "LightColor = Color.FromArgb({0}, {1}, {2}, {3}),", this.LightColor.A, this.LightColor.R, this.LightColor.G, this.LightColor.B);
            sb.AppendTabFormatLine(1, "LightContrast = {0},", this.LightContrast);
            sb.AppendTabFormatLine(1, "LightElevation = {0},", this.LightElevation);
            sb.AppendTabFormatLine(1, "LightIntensity = {0},", this.LightIntensity);
            sb.AppendTabFormatLine("};");
            sb.AppendTabFormatLine("{0}.ClearGradient();", name);

            foreach (KeyValuePair<float, Color> point in this.GetGradient())
            {
                sb.AppendTabFormatLine(
                    "{0}.AddGradientPoint({1}, Color.FromArgb({2}, {3}, {4}, {5}));",
                    name,
                    point.Key,
                    point.Value.A,
                    point.Value.R,
                    point.Value.G,
                    point.Value.B);
            }

            return sb.ToString();
        }
    }
}
