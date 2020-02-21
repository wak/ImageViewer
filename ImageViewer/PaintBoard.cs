using System.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace ImageViewer
{
    class Line
    {
        public List<Point> points = new List<Point>();

        public void addPoint(Point p)
        {
            points.Add(p);
        }

        public void draw(Graphics g, Point baseLocation)
        {
            if (points.Count <= 1)
                return;

            var drawPoints = points.Select(p => new Point(p.X + baseLocation.X, p.Y + baseLocation.Y));

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

        public void addPoint(Point p)
        {
            currentLine.addPoint(p);
        }

        public void draw(Graphics g, Point baseLocation)
        {
            foreach (Line l in lines)
            {
                l.draw(g, baseLocation);
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
