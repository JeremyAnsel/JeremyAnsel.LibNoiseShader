using System;
using System.Diagnostics.CodeAnalysis;

namespace JeremyAnsel.LibNoiseShader
{
    public struct Float4 : IEquatable<Float4>
    {
        [SuppressMessage("Design", "CA1051:Ne pas déclarer de champs d'instances visibles", Justification = "Reviewed.")]
        public float X;

        [SuppressMessage("Design", "CA1051:Ne pas déclarer de champs d'instances visibles", Justification = "Reviewed.")]
        public float Y;

        [SuppressMessage("Design", "CA1051:Ne pas déclarer de champs d'instances visibles", Justification = "Reviewed.")]
        public float Z;

        [SuppressMessage("Design", "CA1051:Ne pas déclarer de champs d'instances visibles", Justification = "Reviewed.")]
        public float W;

        public Float4(float x, float y, float z, float w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        public static Float4 operator +(Float4 x, Float4 y)
        {
            return new Float4(
                x.X + y.X,
                x.Y + y.Y,
                x.Z + y.Z,
                x.W + y.W);
        }

        public static Float4 operator +(Float4 x, float y)
        {
            return new Float4(
                x.X + y,
                x.Y + y,
                x.Z + y,
                x.W + y);
        }

        public static Float4 operator +(float y, Float4 x)
        {
            return new Float4(
                x.X + y,
                x.Y + y,
                x.Z + y,
                x.W + y);
        }

        public static Float4 operator -(Float4 x, Float4 y)
        {
            return new Float4(
                x.X - y.X,
                x.Y - y.Y,
                x.Z - y.Z,
                x.W - y.W);
        }

        public static Float4 operator -(Float4 x, float y)
        {
            return new Float4(
                x.X - y,
                x.Y - y,
                x.Z - y,
                x.W - y);
        }

        public static Float4 operator -(float y, Float4 x)
        {
            return new Float4(
                y - x.X,
                y - x.Y,
                y - x.Z,
                y - x.W);
        }

        public static Float4 operator *(Float4 x, Float4 y)
        {
            return new Float4(
                x.X * y.X,
                x.Y * y.Y,
                x.Z * y.Z,
                x.W * y.W);
        }

        public static Float4 operator *(Float4 x, float y)
        {
            return new Float4(
                x.X * y,
                x.Y * y,
                x.Z * y,
                x.W * y);
        }

        public static Float4 operator *(float y, Float4 x)
        {
            return new Float4(
                x.X * y,
                x.Y * y,
                x.Z * y,
                x.W * y);
        }

        public static bool operator ==(Float4 left, Float4 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Float4 left, Float4 right)
        {
            return !(left == right);
        }

        public static Float4 Floor(Float4 x)
        {
            return new Float4(
                (float)Math.Floor(x.X),
                (float)Math.Floor(x.Y),
                (float)Math.Floor(x.Z),
                (float)Math.Floor(x.W));
        }

        public static float Dot(Float4 x, Float4 y)
        {
            return x.X * y.X + x.Y * y.Y + x.Z * y.Z + x.W * y.W;
        }

        public override bool Equals(object obj)
        {
            return obj is Float4 @float && Equals(@float);
        }

        public bool Equals(Float4 other)
        {
            return X == other.X &&
                   Y == other.Y &&
                   Z == other.Z &&
                   W == other.W;
        }

        public override int GetHashCode()
        {
            var hashCode = -307843816;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            hashCode = hashCode * -1521134295 + Z.GetHashCode();
            hashCode = hashCode * -1521134295 + W.GetHashCode();
            return hashCode;
        }

        public static Float4 Add(Float4 left, Float4 right)
        {
            return left + right;
        }

        public static Float4 Multiply(Float4 left, Float4 right)
        {
            return left * right;
        }

        public static Float4 Subtract(Float4 left, Float4 right)
        {
            return left - right;
        }

        public static Float4 Frac(Float4 x)
        {
            return new Float4(
                x.X - (float)Math.Truncate(x.X),
                x.Y - (float)Math.Truncate(x.Y),
                x.Z - (float)Math.Truncate(x.Z),
                x.W - (float)Math.Truncate(x.W));
        }

        public static Float4 Step(Float4 y, Float4 x)
        {
            return new Float4(
                y.X >= x.X ? 1 : 0,
                y.Y >= x.Y ? 1 : 0,
                y.Z >= x.Z ? 1 : 0,
                y.W >= x.W ? 1 : 0);
        }

        public static Float4 Min(Float4 x, float y)
        {
            return new Float4(
                Math.Min(x.X, y),
                Math.Min(x.Y, y),
                Math.Min(x.Z, y),
                Math.Min(x.Z, y));
        }

        public static Float4 Max(Float4 x, float y)
        {
            return new Float4(
                Math.Max(x.X, y),
                Math.Max(x.Y, y),
                Math.Max(x.Z, y),
                Math.Max(x.Z, y));
        }
    }
}
