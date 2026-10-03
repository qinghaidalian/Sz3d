namespace Sz3d.Core.Data;

/// <summary>三维点（double 精度，单位毫米）。</summary>
public readonly struct Point3 : IEquatable<Point3>
{
    public readonly double X;
    public readonly double Y;
    public readonly double Z;

    public Point3(double x, double y, double z)
    {
        X = x; Y = y; Z = z;
    }

    public static Point3 operator +(Point3 a, Point3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Point3 operator -(Point3 a, Point3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Point3 operator *(Point3 a, double s) => new(a.X * s, a.Y * s, a.Z * s);

    public double LengthSquared => X * X + Y * Y + Z * Z;
    public double Length => Math.Sqrt(LengthSquared);

    public double DistanceTo(Point3 other)
    {
        double dx = X - other.X, dy = Y - other.Y, dz = Z - other.Z;
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    public bool Equals(Point3 other) => X == other.X && Y == other.Y && Z == other.Z;
    public override bool Equals(object? obj) => obj is Point3 p && Equals(p);
    public override int GetHashCode() => HashCode.Combine(X, Y, Z);
    public override string ToString() => $"({X:F3}, {Y:F3}, {Z:F3})";
}
