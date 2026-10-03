using System.Globalization;

namespace Sz3d.Core.Data;

/// <summary>
/// 点云文件读取：支持 PLY（ASCII）、XYZ / TXT（每行 "x y z [r g b]"）。
/// </summary>
public static class PointCloudIO
{
    public static PointCloudData? Load(string path)
    {
        if (!File.Exists(path)) return null;
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext == ".ply" ? LoadPly(path) : LoadText(path);
    }

    private static PointCloudData? LoadPly(string path)
    {
        try
        {
            var lines = File.ReadAllLines(path);
            int vertexCount = 0;
            int xi = -1, yi = -1, zi = -1, ri = -1, gi = -1, bi = -1;
            int propCount = 0;
            int i = 0;
            bool ascii = false;

            // 解析头部
            for (; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (line.StartsWith("format")) ascii = line.Contains("ascii");
                else if (line.StartsWith("element vertex")) int.TryParse(line.Split(' ', StringSplitOptions.RemoveEmptyEntries)[2], out vertexCount);
                else if (line.StartsWith("property"))
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var name = parts.Length > 0 ? parts[^1].ToLowerInvariant() : "";
                    switch (name)
                    {
                        case "x": xi = propCount; break;
                        case "y": yi = propCount; break;
                        case "z": zi = propCount; break;
                        case "red": case "r": ri = propCount; break;
                        case "green": case "g": gi = propCount; break;
                        case "blue": case "b": bi = propCount; break;
                    }
                    propCount++;
                }
                else if (line.StartsWith("end_header")) { i++; break; }
            }

            if (!ascii || xi < 0 || yi < 0 || zi < 0 || vertexCount <= 0) return null;

            var pts = new Point3[vertexCount];
            byte[]? cols = (ri >= 0 && gi >= 0 && bi >= 0) ? new byte[vertexCount * 4] : null;

            for (int v = 0; v < vertexCount && i < lines.Length; v++, i++)
            {
                var parts = lines[i].Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length <= propCount && parts.Length <= Math.Max(Math.Max(xi, yi), zi)) break;
                double x = Parse(parts[xi]), y = Parse(parts[yi]), z = Parse(parts[zi]);
                pts[v] = new Point3(x, y, z);
                if (cols != null)
                {
                    cols[v * 4] = (byte)Clamp(Parse(parts[ri]));
                    cols[v * 4 + 1] = (byte)Clamp(Parse(parts[gi]));
                    cols[v * 4 + 2] = (byte)Clamp(Parse(parts[bi]));
                    cols[v * 4 + 3] = 255;
                }
            }
            return new PointCloudData(pts, cols);
        }
        catch
        {
            return null;
        }
    }

    private static PointCloudData? LoadText(string path)
    {
        try
        {
            var lines = File.ReadAllLines(path);
            var pts = new List<Point3>();
            var cols = new List<byte>();
            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (line.Length == 0 || line[0] == '#') continue;
                var parts = line.Split(new[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) continue;
                if (!double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)) continue;
                double y = double.Parse(parts[1], CultureInfo.InvariantCulture);
                double z = double.Parse(parts[2], CultureInfo.InvariantCulture);
                pts.Add(new Point3(x, y, z));
                if (parts.Length >= 6)
                {
                    cols.Add((byte)Clamp(Parse(parts[3])));
                    cols.Add((byte)Clamp(Parse(parts[4])));
                    cols.Add((byte)Clamp(Parse(parts[5])));
                    cols.Add(255);
                }
            }
            if (pts.Count == 0) return null;
            return new PointCloudData(pts.ToArray(), cols.Count == pts.Count * 4 ? cols.ToArray() : null);
        }
        catch
        {
            return null;
        }
    }

    private static double Parse(string s) => double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : 0.0;

    private static double Clamp(double v) => v < 0 ? 0 : v > 255 ? 255 : v;
}
