using System;
using System.Collections.Generic;
using System.Text;
//Extra libraries I added.
using System.Drawing;
using System.Drawing.Drawing2D;


namespace ShapeClasses
{
    /******************************************************/
    /* Shape10C -- General base class for PIC 10C shapes. */
    public abstract class Shape10C
    {
        protected Color shapeColor;
        public bool isVisible = false;  //Sometimes we want to hide shapes.

        public abstract void Move(int dx, int dy);
        public abstract void Draw(Graphics g);
        public abstract bool IsPointInside(Point p);
        public abstract void MouseDownCreating(Point p, Graphics g);
        public abstract void MouseMoveCreating(Point p, Graphics g);

        public Color Get_color()
        {
            return shapeColor;
        }

        public void Set_color(Color c)
        {
            shapeColor = c;
        }
    }  //end Shape10C class

    /*******************************************/
    /* Rectangle10C -- PIC 10C Rectangle class */
    public class Rectangle10C : Shape10C
    {
        public Point upperLeftCorner;
        public int width;
        public int height;

        public Rectangle10C()
        {
            isVisible = false;
            shapeColor = Color.White;
            upperLeftCorner = new Point(0, 0);
            width = 0;
            height = 0;
        }

        public Rectangle10C(Color c)
        {
            isVisible = false;
            shapeColor = c;
            upperLeftCorner = new Point(0, 0);
            width = 0;
            height = 0;
        }

        public Rectangle10C(Color c, Point p, int w, int h)
        {
            isVisible = true;
            shapeColor = c;
            upperLeftCorner = p;
            width = w;
            height = h;
        }

        public override void Move(int dx, int dy)
        {
            upperLeftCorner.X += dx;
            upperLeftCorner.Y += dy;
        }

        public override void Draw(Graphics g)
        {
            SolidBrush myBrush = new SolidBrush(shapeColor);
            if (isVisible == true)
                g.FillRectangle(myBrush, upperLeftCorner.X, upperLeftCorner.Y, width, height);
        }

        public override bool IsPointInside(Point p)
        {
            return (p.X >= upperLeftCorner.X && p.Y >= upperLeftCorner.Y
                && p.X <= upperLeftCorner.X + width && p.Y <= upperLeftCorner.Y + height);
        }

        public override void MouseDownCreating(Point p, Graphics g)
        {
            isVisible = true;
            upperLeftCorner = p;
        }

        public override void MouseMoveCreating(Point p, Graphics g)
        {
            width = p.X - upperLeftCorner.X;
            height = p.Y - upperLeftCorner.Y;
        }
    } //end Rectangle10C class


}
