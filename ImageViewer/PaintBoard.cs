using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace ImageViewer
{
    class Line
    {
        public PointF[] pointsCache = new PointF[0];
        public float pointsCacheZoom;
        public Point pointsCacheBaseLocation;

        public PointF[] points = new PointF[0];

        public void addPoint(PointF p)
        {
            Array.Resize(ref points, points.Length + 1);
            points[points.Length - 1] = p;

            Array.Resize(ref pointsCache, pointsCache.Length + 1);
            pointsCache[pointsCache.Length - 1] = new PointF(p.X * pointsCacheZoom + pointsCacheBaseLocation.X, p.Y * pointsCacheZoom + pointsCacheBaseLocation.Y);
        }

        private void updateCache(Point newBaseLocation, float newZoom)
        {
            if (newBaseLocation == pointsCacheBaseLocation && newZoom == pointsCacheZoom)
                return;

            pointsCacheZoom = newZoom;
            pointsCacheBaseLocation = newBaseLocation;

            for (var i = 0; i < points.Length; i++)
            {
                var p = points[i];
                pointsCache[i] = new PointF(p.X * pointsCacheZoom + pointsCacheBaseLocation.X, p.Y * pointsCacheZoom + pointsCacheBaseLocation.Y);
            }
        }
        public void draw(Graphics g, Point baseLocation, float zoom)
        {
            if (points.Length <= 1)
                return;

            updateCache(baseLocation, zoom);

            using (Pen pen = new Pen(Color.FromArgb(255, 255, 0, 0), 5))
            {
                g.DrawLines(pen, pointsCache);
            }
        }

        public bool IsEmpty()
        {
            return points.Length == 0;
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
