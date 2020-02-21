using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ImageViewer
{
    class Line
    {
        public List<Point> points = new List<Point>();

        public void addPoint(Point p)
        {
            points.Add(p);
        }

        public void draw(Graphics g)
        {
            if (points.Count == 0)
                return;

            Point previousP;

            previousP = points[0];

            using (Pen pen = new Pen(Color.FromArgb(255, 255, 0, 0), 5))
            {
                foreach (Point p in points)
                {
                    g.DrawLine(pen, previousP, p);
                    previousP = p;
                }
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

        public void draw(Graphics g)
        {
            foreach (Line l in lines)
            {
                l.draw(g);
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
