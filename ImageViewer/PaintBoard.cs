using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace ImageViewer
{
    class Line
    {
        const int POINTS_INC = 100;
        private int pointsSize = 0;
        private PointF[] pointsCache = new PointF[POINTS_INC];
        private float pointsCacheZoom;
        private Point pointsCacheBaseLocation;

        private PointF[] points = new PointF[POINTS_INC];

        public void addPoint(PointF p)
        {
            if (pointsSize >= points.Length)
            {
                Array.Resize(ref points, points.Length + POINTS_INC);
                Array.Resize(ref pointsCache, pointsCache.Length + POINTS_INC);
            }
            points[pointsSize] = p;
            pointsCache[pointsSize] = new PointF(p.X * pointsCacheZoom + pointsCacheBaseLocation.X, p.Y * pointsCacheZoom + pointsCacheBaseLocation.Y);
            pointsSize += 1;

            for (var i = pointsSize; i < points.Length; i++)
            {
                points[i] = points[pointsSize - 1];
                pointsCache[i] = pointsCache[pointsSize - 1];
            }
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
            if (pointsSize <= 1)
                return;

            updateCache(baseLocation, zoom);

            using (Pen pen = new Pen(Color.FromArgb(255, 255, 0, 0), 5))
            {
                g.DrawLines(pen, pointsCache);
            }
        }

        public bool IsEmpty()
        {
            return pointsSize == 0;
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

        public bool IsEmpty()
        {
            return lines.Count == 0 || (lines.Count == 1 && lines[0].IsEmpty());
        }
    }
}
