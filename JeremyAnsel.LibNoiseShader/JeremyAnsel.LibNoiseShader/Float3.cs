using System;
using System.Diagnostics.CodeAnalysis;

namespace JeremyAnsel.LibNoiseShader
{
    public struct Float3 : IEquatable<Float3>
    {
        [SuppressMessage("Design", "CA1051:Ne pas déclarer de champs d'instances visibles", Justification = "Reviewed.")]
        public float X;

        [SuppressMessage("Design", "CA1051:Ne pas déclarer de champs d'instances visibles", Justification = "Reviewed.")]
        public float Y;

        [SuppressMessage("Design", "CA1051:Ne pas déclarer de champs d'instances visibles", Justification = "Reviewed.")]
        public float Z;

        public Float3(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public static Float3 operator +(Float3 x, Float3 y)
        {
            return new Float3(
                x.X + y.X,
                x.Y + y.Y,
                x.Z + y.Z);
        }

        public static Float3 operator +(Float3 x, float y)
        {
            return new Float3(
                x.X + y,
                x.Y + y,
                x.Z + y);
        }

        public static Float3 operator +(float y, Float3 x)
        {
            return new Float3(
                x.X + y,
                x.Y + y,
                x.Z + y);
        }

        public static Float3 operator -(Float3 x, Float3 y)
        {
            return new Float3(
                x.X - y.X,
                x.Y - y.Y,
                x.Z - y.Z);
        }

        public static Float3 operator -(Float3 x, float y)
        {
            return new Float3(
                x.X - y,
                x.Y - y,
                x.Z - y);
        }

        public static Float3 operator -(float y, Float3 x)
        {
            return new Float3(
                y - x.X,
                y - x.Y,
                y - x.Z);
        }

        public static Float3 operator *(Float3 x, Float3 y)
        {
            return new Float3(
                x.X * y.X,
                x.Y * y.Y,
                x.Z * y.Z);
        }

        public static Float3 operator *(Float3 x, float y)
        {
            return new Float3(
                x.X * y,
                x.Y * y,
                x.Z * y);
        }

        public static Float3 operator *(float y, Float3 x)
        {
            return new Float3(
                x.X * y,
                x.Y * y,
                x.Z * y);
        }

        public static bool operator ==(Float3 left, Float3 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Float3 left, Float3 right)
        {
            return !(left == right);
        }

        public static Float3 Floor(Float3 x)
        {
            return new Float3(
                (float)Math.Floor(x.X),
                (float)Math.Floor(x.Y),
                (float)Math.Floor(x.Z));
        }

        public static float Dot(Float3 x, Float3 y)
        {
            return x.X * y.X + x.Y * y.Y + x.Z * y.Z;
        }

        public override string ToString()
        {
            return $"{X}; {Y}; {Z}";
        }

        public override bool Equals(object? obj)
        {
            return obj is Float3 @float && Equals(@float);
        }

        public bool Equals(Float3 other)
        {
            return X == other.X &&
                   Y == other.Y &&
                   Z == other.Z;
        }

        public override int GetHashCode()
        {
            var hashCode = -307843816;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            hashCode = hashCode * -1521134295 + Z.GetHashCode();
            return hashCode;
        }

        public static Float3 Add(Float3 left, Float3 right)
        {
            return left + right;
        }

        public static Float3 Multiply(Float3 left, Float3 right)
        {
            return left * right;
        }

        public static Float3 Subtract(Float3 left, Float3 right)
        {
            return left - right;
        }

        public static Float3 Step(Float3 y, Float3 x)
        {
            return new Float3(
                x.X >= y.X ? 1 : 0,
                x.Y >= y.Y ? 1 : 0,
                x.Z >= y.Z ? 1 : 0);
        }

        public static Float3 Min(Float3 x, float y)
        {
            return new Float3(
                Math.Min(x.X, y),
                Math.Min(x.Y, y),
                Math.Min(x.Z, y));
        }

        public static Float3 Max(Float3 x, float y)
        {
            return new Float3(
                Math.Max(x.X, y),
                Math.Max(x.Y, y),
                Math.Max(x.Z, y));
        }
    }
}
