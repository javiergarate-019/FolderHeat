namespace FolderHeat.App;

internal enum UiIconKind
{
    Open,
    Add,
    Pin,
    Unpin,
    Ignore,
    Restore,
    Settings,
    About,
}

internal static class UiIconFactory
{
    public static Bitmap Create(UiIconKind kind, int size = 16)
    {
        var bitmap = new Bitmap(size, size);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        graphics.Clear(Color.Transparent);

        using var pen = new Pen(Color.FromArgb(40, 45, 52), Math.Max(1.6f, size / 9f))
        {
            StartCap = System.Drawing.Drawing2D.LineCap.Round,
            EndCap = System.Drawing.Drawing2D.LineCap.Round,
            LineJoin = System.Drawing.Drawing2D.LineJoin.Round,
        };
        using var accentPen = new Pen(Color.FromArgb(230, 98, 42), Math.Max(1.8f, size / 8f))
        {
            StartCap = System.Drawing.Drawing2D.LineCap.Round,
            EndCap = System.Drawing.Drawing2D.LineCap.Round,
            LineJoin = System.Drawing.Drawing2D.LineJoin.Round,
        };
        using var brush = new SolidBrush(Color.FromArgb(40, 45, 52));
        using var accentBrush = new SolidBrush(Color.FromArgb(230, 98, 42));

        switch (kind)
        {
            case UiIconKind.Open:
                DrawFolder(graphics, pen, size);
                graphics.DrawLine(accentPen, size * 0.48f, size * 0.56f, size * 0.84f, size * 0.56f);
                graphics.DrawLine(accentPen, size * 0.68f, size * 0.38f, size * 0.84f, size * 0.56f);
                graphics.DrawLine(accentPen, size * 0.68f, size * 0.74f, size * 0.84f, size * 0.56f);
                break;
            case UiIconKind.Add:
                DrawFolder(graphics, pen, size);
                graphics.DrawLine(accentPen, size * 0.68f, size * 0.36f, size * 0.68f, size * 0.78f);
                graphics.DrawLine(accentPen, size * 0.47f, size * 0.57f, size * 0.89f, size * 0.57f);
                break;
            case UiIconKind.Pin:
                graphics.FillEllipse(accentBrush, size * 0.34f, size * 0.12f, size * 0.32f, size * 0.32f);
                graphics.DrawLine(pen, size * 0.5f, size * 0.42f, size * 0.5f, size * 0.86f);
                graphics.DrawLine(pen, size * 0.37f, size * 0.58f, size * 0.63f, size * 0.58f);
                break;
            case UiIconKind.Unpin:
                graphics.DrawLine(pen, size * 0.34f, size * 0.16f, size * 0.7f, size * 0.82f);
                graphics.DrawLine(pen, size * 0.66f, size * 0.16f, size * 0.3f, size * 0.82f);
                break;
            case UiIconKind.Ignore:
                graphics.DrawEllipse(pen, size * 0.17f, size * 0.17f, size * 0.66f, size * 0.66f);
                graphics.DrawLine(accentPen, size * 0.29f, size * 0.71f, size * 0.71f, size * 0.29f);
                break;
            case UiIconKind.Restore:
                graphics.DrawArc(pen, size * 0.18f, size * 0.18f, size * 0.64f, size * 0.64f, 35, 285);
                graphics.FillPolygon(brush, new[]
                {
                    new PointF(size * 0.2f, size * 0.28f),
                    new PointF(size * 0.48f, size * 0.24f),
                    new PointF(size * 0.32f, size * 0.48f),
                });
                break;
            case UiIconKind.Settings:
                graphics.FillEllipse(brush, size * 0.4f, size * 0.4f, size * 0.2f, size * 0.2f);
                for (var i = 0; i < 8; i++)
                {
                    var angle = Math.PI * 2 * i / 8;
                    var x1 = size * (0.5f + 0.26f * (float)Math.Cos(angle));
                    var y1 = size * (0.5f + 0.26f * (float)Math.Sin(angle));
                    var x2 = size * (0.5f + 0.41f * (float)Math.Cos(angle));
                    var y2 = size * (0.5f + 0.41f * (float)Math.Sin(angle));
                    graphics.DrawLine(pen, x1, y1, x2, y2);
                }
                graphics.DrawEllipse(pen, size * 0.28f, size * 0.28f, size * 0.44f, size * 0.44f);
                break;
            case UiIconKind.About:
                graphics.DrawEllipse(pen, size * 0.18f, size * 0.18f, size * 0.64f, size * 0.64f);
                graphics.FillEllipse(accentBrush, size * 0.44f, size * 0.26f, size * 0.12f, size * 0.12f);
                graphics.DrawLine(accentPen, size * 0.5f, size * 0.48f, size * 0.5f, size * 0.72f);
                break;
        }

        return bitmap;
    }

    private static void DrawFolder(Graphics graphics, Pen pen, int size)
    {
        graphics.DrawRectangle(pen, size * 0.16f, size * 0.35f, size * 0.68f, size * 0.42f);
        graphics.DrawLine(pen, size * 0.18f, size * 0.35f, size * 0.34f, size * 0.2f);
        graphics.DrawLine(pen, size * 0.34f, size * 0.2f, size * 0.52f, size * 0.35f);
    }
}
