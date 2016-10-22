/*************************************
 * Form1.cs
 * A space invaders game.
 * Joseph Silverstin, 5/23/11
 * ***********************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ShapeClasses;
using WMPLib;
using System.Media;

namespace BouncingRectangle
{
    public partial class Form1 : Form
    {
        private Bitmap myCanvas;  //The canvas which we will draw upon.
        private Rectangle10C paddleTop; // base of the turret
        private Rectangle10C paddleTopTop; // top of the turret
        private Rectangle10C laser; // the laser
        private Rectangle10C[,] aliens; // the aliens (represented here as a matrix)
        private Rectangle10C[,] alienLasers; // each alien gets one laser (they can fire through each other)
        private double step = 2.0;  //How fast to move the rectangle.
        private Rectangle10C paddle;
        private int paddle_step = 4;  //How fast to move the paddle with one key press.
        private int numBounces = 0;  //The number of bounces off the paddle.
        char motionDirection = 'r'; // the direction of motion of the aliens
        int score = 0;
        int lives = 3;
        int level = 1; // affects the rate at which the aliens fire their lasers
        int count = 0; // number of times the timer has ticked
        int numInvisible; // the number of aliens that have been killed
        bool isGameOver = false; // whether the game is over or not
        Random r; // randomly determines which alien shoots next
        WMPLib.WindowsMediaPlayer player;
        SoundPlayer theme; // the sound player for the theme music

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            r = new Random();
            score = 0;
            lives = 3;
            myCanvas = new Bitmap(this.ClientRectangle.Width, this.ClientRectangle.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Graphics g = Graphics.FromImage(myCanvas);
            g.Clear(Color.Black);
            animationTimer.Interval = 10;
            animationTimer.Start();
            paddle = new Rectangle10C(Color.LightGreen, new Point(this.ClientRectangle.Width / 2-20, this.ClientRectangle.Height - 20), 40, 15);
            paddleTop = new Rectangle10C(Color.LightGreen, new Point(this.ClientRectangle.Width / 2 - 20 + 15, this.ClientRectangle.Height - 20 - 5), 10, 5);
            paddleTopTop = new Rectangle10C(Color.LightGreen, new Point(this.ClientRectangle.Width / 2 - 20 + 15 + 3, this.ClientRectangle.Height - 20 - 5 - 5), 4, 5);
            // when invisible, laser is always positioned in same spot as the top of the turret
            laser = new Rectangle10C(Color.Red, new Point(this.ClientRectangle.Width / 2 - 20 + 15 + 5, this.ClientRectangle.Height - 20 - 5 - 5), 1, 15);
            laser.isVisible = false; // initially invisible
            aliens = new Rectangle10C[13,5];
            alienLasers = new Rectangle10C[11, 5];
            for (int i = 0; i < 13; i++)
                for (int j = 0; j < 5; j++)
                {
                    if (j == 0)
                        aliens[i, j] = new Rectangle10C(Color.Magenta, new Point(81 + 40 * i, 100 + 40 * j), 20, 20);
                    if (j == 1 || j == 2)
                        aliens[i, j] = new Rectangle10C(Color.Yellow, new Point(81 + 40 * i, 100 + 40 * j), 20, 20);
                    if (j == 3 || j == 4)
                        aliens[i, j] = new Rectangle10C(Color.White, new Point(81 + 40 * i, 100 + 40 * j), 20, 20);
                    if (i >= 11) // for some reason, 2nd-to-last row aliens get bigger when they hit the right wall
                        aliens[i, j].isVisible = false;
                    if (i < 11)
                    {
                        alienLasers[i, j] = new Rectangle10C(Color.LightBlue, new Point(81 + 40 * i + 10, 100 + 40 * j + 20), 1, 15); // position the alien lasers
                        alienLasers[i, j].isVisible = false; // initially invisible
                    }
                }

            
            theme = new SoundPlayer();
            theme.SoundLocation = @"theme.wav";
            theme.Load();
            theme.PlayLooping();
            
            player = new WindowsMediaPlayer();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.DrawImageUnscaled(myCanvas, 0, 0);
        }

        private void animationTimer_Tick(object sender, EventArgs e)
        {
            // determine which alien shoots next
            int nextAlienCol = r.Next(0, 11);
            int nextAlienRow = r.Next(0, 5);

            Graphics g = Graphics.FromImage(myCanvas);
            g.Clear(Color.Black);
            paddle.Draw(g);
            paddleTop.Draw(g);
            paddleTopTop.Draw(g);
            for (int i = 0; i < 13; i++)
                for (int j = 0; j < 5; j++)
                {
                    Rectangle test = new Rectangle(0, 0, 10, 10);
                    if (aliens[i, j].upperLeftCorner.X <= 10)
                        motionDirection = 'r'; // start moving to the right
                    if (aliens[i, j].upperLeftCorner.X >= this.ClientRectangle.Width - 15 + 30*2)
                        motionDirection = 'l'; // start moving to the left
                    if (motionDirection == 'r')
                    {
                        aliens[i, j].Move(1, 0); // move to the right
                        if (i < 11 && alienLasers[i,j].isVisible == false)
                            alienLasers[i, j].Move(1, 0);
                    }
                    else
                    {
                        aliens[i, j].Move(-1, 0); // move to the left
                        if (i < 11 && alienLasers[i, j].isVisible == false)
                            alienLasers[i, j].Move(-1, 0);
                    }
                    if (aliens[i, j].IsPointInside(laser.upperLeftCorner) && aliens[i, j].isVisible) // if laser hits alien
                    { 
                        aliens[i, j].isVisible = false; // make alien invisible
                        numInvisible++;
                        laser.upperLeftCorner.X = paddleTopTop.upperLeftCorner.X + 2;
                        laser.upperLeftCorner.Y = paddleTopTop.upperLeftCorner.Y; // move laser back to turret
                        laser.isVisible = false;
                        // increment score (scaled exponentially by level)
                        if (j == 0)
                            score += (int)(40 * Math.Pow(level, 2));
                        if (j == 1 || j == 2)
                            score += (int)(20 * Math.Pow(level, 2));
                        if (j == 3 || j == 4)
                            score += (int)(10 * Math.Pow(level, 2));
                        scoreLabel.Text = "Score: " + score.ToString();
                    }
                    aliens[i, j].Draw(g); // draw the aliens
                    if (i < 11 && alienLasers[i, j].isVisible == true) // if alien laser is moving
                    {
                        alienLasers[i, j].Move(0, 4);
                        alienLasers[i, j].Draw(g); // alien fires laser at turret
                        if (alienLasers[i, j].upperLeftCorner.Y > this.ClientRectangle.Height)
                        {// if laser went off the screen, reposition it and make it invisible
                            alienLasers[i, j].isVisible = false;
                            alienLasers[i, j].upperLeftCorner.X = aliens[i, j].upperLeftCorner.X + 10;
                            alienLasers[i, j].upperLeftCorner.Y = aliens[i, j].upperLeftCorner.Y + 20;
                        }
                        if (paddle.IsPointInside(alienLasers[i, j].upperLeftCorner) || paddleTop.IsPointInside(alienLasers[i, j].upperLeftCorner) || paddleTopTop.IsPointInside(alienLasers[i, j].upperLeftCorner))
                        { // player loses a life
                            lives--;
                            if (lives < 0)
                            {
                                animationTimer.Stop(); // game over
                                gameOverLabel.Visible = true;
                                isGameOver = true;
                            }
                            alienLasers[i, j].isVisible = false;
                            alienLasers[i, j].upperLeftCorner.X = aliens[i, j].upperLeftCorner.X + 10;
                            alienLasers[i, j].upperLeftCorner.Y = aliens[i, j].upperLeftCorner.Y + 20;
                        }
                    }
                }
            if (count % (100 / level) == 0) // rate changes with the level
                if (aliens[nextAlienCol, nextAlienRow].isVisible == true) // if this alien is still alive
                    alienLasers[nextAlienCol, nextAlienRow].isVisible = true; // this alien starts firing laser
            if (laser.isVisible == true)
            {
                laser.Move(0, -8);
                laser.Draw(g);
            }
            else // return laser to turret
            {
                laser.upperLeftCorner.X = paddleTopTop.upperLeftCorner.X + 2;
                laser.upperLeftCorner.Y = paddleTopTop.upperLeftCorner.Y;
            }
            if (laser.upperLeftCorner.Y < -10)
                laser.isVisible = false;
            if (numInvisible >= 55)
            {
                level++; // go next level
                levelLabel.Text = "Level: " + level.ToString();
                lives = 3; // reset lives
                livesLabel.Text = "Lives: 3";
                numInvisible = 0;
                for (int i = 0; i < 11; i++)
                    for (int j = 0; j < 5; j++)
                        aliens[i, j].isVisible = true; // make all the aliens visible again
            }
            g.Dispose();
            this.Refresh();

            if (lives >= 0)
                livesLabel.Text = "Health: " + lives.ToString();
            count++;
        }

        // returns to the selection screen
        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*************************************************
             * CODE TO RETURN TO THE SELECTION SCREEN GOES HERE
             * **********************************************/
        }

        // moves paddle right or left
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left && paddle.upperLeftCorner.X > 0)
            {
                paddle.Move(-paddle_step, 0);
                paddleTop.Move(-paddle_step, 0);
                paddleTopTop.Move(-paddle_step, 0);
                if (laser.isVisible == false) // move laser to correct position to be fired
                    laser.Move(-paddle_step, 0);
            }
            if (e.KeyCode == Keys.Right && paddle.upperLeftCorner.X + paddle.width < this.ClientRectangle.Width)
            {
                paddle.Move(paddle_step, 0);
                paddleTop.Move(paddle_step, 0);
                paddleTopTop.Move(paddle_step, 0);
                if (laser.isVisible == false) // move laser to correct position to be fired
                    laser.Move(paddle_step, 0);
            }
            if (e.KeyCode == Keys.Space) // fire laser
            {
                if (isGameOver == false && laser.isVisible == false)
                {
                    player.URL = @"laser1.wav";
                    player.controls.play();
                }
                laser.isVisible = true;
            }
            if (e.KeyCode == Keys.P) // pause (or unpause) the game
                animationTimer.Enabled = !animationTimer.Enabled;
            if (isGameOver == true && e.KeyCode == Keys.Space)
            {
                /*************************************************
                 * CODE TO RETURN TO THE SELECTION SCREEN GOES HERE
                 * **********************************************/
            }
            //We should not need to draw the paddle here if the timer Interval is small.
            //Drawing the graphics here could result in flicker.
        }

        // brings up help menu
        private void helpToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Form myForm = new helpDialog();
            myForm.Show();
            animationTimer.Stop(); // pause the game
        }
    }
}