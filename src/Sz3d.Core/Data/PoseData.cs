namespace Sz3d.Core.Data;

/// <summary>
/// 位姿：位置（毫米）+ 姿态（四元数）。
/// </summary>
public sealed class PoseData
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public double Qx { get; set; }
    public double Qy { get; set; }
    public double Qz { get; set; }
    public double Qw { get; set; } = 1.0;

    public static PoseData Identity() => new() { X = 0, Y = 0, Z = 0, Qx = 0, Qy = 0, Qz = 0, Qw = 1 };

    public override string ToString() => $"pos=({X:F2}, {Y:F2}, {Z:F2}) quat=({Qx:F4}, {Qy:F4}, {Qz:F4}, {Qw:F4})";
}
