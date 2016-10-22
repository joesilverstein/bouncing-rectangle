using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ShapeClasses;

namespace BouncingRectangle
{
    public partial class Form1 : Form
    {
        private Bitmap myCanvas;  //The canvas which we will draw upon.
        private Rectangle10C redRect;
        private double step = 2.0;  //How fast to move the rectangle.
        private double theta;  //Direction to move the rectangle in.
        private Rectangle10C paddle;
        private int paddle_step = 10;  //How fast to move the paddle with one key press.
        private int numBounces = 0;  //The number of bounces off the paddle.

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            myCanvas = new Bitmap(this.ClientRectangle.Width, this.ClientRectangle.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Graphics g = Graphics.FromImage(myCanvas);
            g.Clear(Color.White);
            redRect = new Rectangle10C(Color.Red, new Point(100, 10), 40, 40);
            Random r = new Random();
            //Create random initial direction.
            theta = 2*Math.PI* (1 + r.Next(999)) / 1000.0;
            //If direction is a multiple of PI/2, try again.
            while (Math.Abs(theta / (Math.PI/2) - Math.Round(theta / (Math.PI/2) )) < 0.1)
                theta = 2 * Math.PI * (1 + r.Next(999)) / 1000.0;
            animationTimer.Interval = 10;
            animationTimer.Start();
            paddle = new Rectangle10C(Color.Blue, new Point(this.ClientRectangle.Width / 2-20, this.ClientRectangle.Height - 20), 40, 15);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            label1.Text = "# Bounces = 0";
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.DrawImageUnscaled(myCanvas, 0, 0);
        }

        private void animationTimer_Tick(object sender, EventArgs e)
        {
            UpdateTheta();
            int dx = (int) Math.Round(step * Math.Cos(theta));
            int dy = (int) Math.Round(step * Math.Sin(theta));
            redRect.Move(dx,dy);
            Graphics g = Graphics.FromImage(myCanvas);
            g.Clear(Color.White);
            redRect.Draw(g);
            paddle.Draw(g);
            g.Dispose();
            this.Refresh();
        }

        private void UpdateTheta()
        {
            //Predict the nextCorner of the rectangle by moving in direction theta.
            //If the nextCorner takes us outside the form, change the direction theta.
            //Note this predicts when we will have collision, so technically we bounce just BEFORE the collision occurs.
            //If the value of step is small enough, we should not notice this difference.
            int dx = (int)Math.Round(step * Math.Cos(theta));
            int dy = (int)Math.Round(step * Math.Sin(theta));
            Point nextCorner = redRect.upperLeftCorner;
            nextCorner.X += dx;
            nextCorner.Y += dy;
            //Check for collision with top edge.
            if (nextCorner.Y < 0)
                theta = 2 * Math.PI - theta;
            //Check for collision with left edge.
            if (nextCorner.X < 0)
                theta = Math.PI - theta;
            //Check for collision with right edge.
            if (nextCorner.X + redRect.width > this.ClientRectangle.Width)
                theta = Math.PI - theta;
            //Check for collision with bottom edge.  GAME OVER. 
            if (nextCorner.Y + redRect.height > this.ClientRectangle.Height)
            {
                //theta = 2 * Math.PI - theta;
                label1.Text = "GAME OVER!  Total # Bounces = " + numBounces.ToString();
                animationTimer.Stop();
            }
            //Check if redRect hits the paddle. 
            if (nextCorner.Y + redRect.height >= paddle.upperLeftCorner.Y
                && nextCorner.X + redRect.width > paddle.upperLeftCorner.X
                && nextCorner.X < paddle.upperLeftCorner.X + paddle.width)
            {
                numBounces++;
                label1.Text = "# Bounces = " + numBounces.ToString();
                theta = 2 * Math.PI - theta;
                step *= 1.2;  //Speed up the bouncing rectangle after each bounce.
            }
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            myCanvas = new Bitmap(this.ClientRectangle.Width, this.ClientRectangle.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Graphics g = Graphics.FromImage(myCanvas);
            g.Clear(Color.White);
            redRect.Draw(g);
            this.Refresh();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left && paddle.upperLeftCorner.X > 0)
                paddle.Move(-paddle_step, 0);
            if (e.KeyCode == Keys.Right && paddle.upperLeftCorner.X + paddle.width < this.ClientRectangle.Width)
                paddle.Move(paddle_step, 0);
            //We should not need to draw the paddle here if the timer Interval is small.
            //Drawing the graphics here could result in flicker.
        }


    }
}