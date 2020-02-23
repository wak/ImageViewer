using System.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace ImageViewer
{
    class Line
    {
        public List<PointF> points = new List<PointF>();

        public void addPoint(PointF p)
        {
            points.Add(p);
        }

        public void draw(Graphics g, Point baseLocation, float zoom)
        {
            if (points.Count <= 1)
                return;

            var drawPoints = points.Select(p => new PointF(p.X * zoom + baseLocation.X, p.Y * zoom + baseLocation.Y));

            using (Pen pen = new Pen(Color.FromArgb(255, 255, 0, 0), 5))
            {
                g.DrawLines(pen, drawPoints.ToArray());
            }
        }

        public bool IsEmpty()
        {
            return points.Count == 0;
        }
    }

    class PaintBoard
    {
        private List<Line> lines;
        private Line currentLine;

        public PaintBoard()
        {
            clear();
        }

        public void newLine()
        {
            if (currentLine.IsEmpty())
                return;

            currentLine = new Line();
            lines.Add(currentLine);
        }

        public void addPoint(PointF p)
        {
            currentLine.addPoint(p);
        }

        public void draw(Graphics g, Point baseLocation, float zoom)
        {
            foreach (Line l in lines)
            {
                l.draw(g, baseLocation, zoom);
            }
        }

        public void clear()
        {
            lines = new List<Line>();
            currentLine = new Line();

            lines.Add(currentLine);
        }
    }
}
