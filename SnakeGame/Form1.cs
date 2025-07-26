using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SnakeGame
{
    public partial class Form1 : Form
    {
        private Image foodImage;
        private Image headImage;
        private const int BlockSize = 20;
        private List<Point> snake = new List<Point>();
        private Timer gameTimer;
        private SoundPlayer eatSound;
        private Point direction = new Point(1, 0);
        private bool isGameOver = false;
        private bool keyPressed = false;
        //private bool foodWasEaten = false;
        private Random random = new Random();
        private Point food;
        private int score = 0;
        Button retryButton = new Button();
        private Point? bonusFood = null;
        private int foodsEaten = 0;
        private SoundPlayer bonusSound;
        public Form1()
        {
            InitializeComponent();
            headImage = Image.FromFile("head.png");
            eatSound = new SoundPlayer("eat.wav");
            bonusSound = new SoundPlayer("bonus.wav");
            food = newFood();
            this.ClientSize = new Size(500, 380);
            foodImage = Image.FromFile("food.png");
            
            this.DoubleBuffered = true;

            snake.Add(new Point(11, 7)); // Head
            snake.Add(new Point(10, 7));
            snake.Add(new Point(9, 7));
            gameTimer = new Timer();
            gameTimer.Interval = 150; 
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();
            this.KeyDown += Form1_KeyDown;
            this.KeyPreview = true;
            SetupRetryButton();

        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawGrid(e.Graphics);
            DrawGridBorderLines(e.Graphics);
            DrawFood(e.Graphics);
            DrawSnake(e.Graphics);

            Font scoreFont = new Font("Arial", 16, FontStyle.Bold);
            Brush scoreBrush = Brushes.Yellow;
            string scoreText = $"Score: {score}";
            e.Graphics.DrawString(scoreText, scoreFont, scoreBrush, 0, 0);

            if (isGameOver)
            {
                string msg = "GAME OVER";
                Font font = new Font("Arial", 32, FontStyle.Bold);
                SizeF textSize = e.Graphics.MeasureString(msg, font);

                
                float x = (ClientSize.Width - textSize.Width) / 2;
                float y = (ClientSize.Height - textSize.Height) / 2;

                e.Graphics.DrawString(msg, font, Brushes.Red, x, y);
            }

        }

        private void SetupRetryButton()
        {
            retryButton.Text = "Retry";
            retryButton.Size = new Size(120, 50);
            retryButton.Font = new Font("Arial", 20, FontStyle.Bold);
            retryButton.ForeColor = Color.LimeGreen; 
            retryButton.BackColor = Color.Transparent; // No background
            retryButton.FlatStyle = FlatStyle.Flat;
            retryButton.FlatAppearance.BorderSize = 0; // No border
            retryButton.FlatAppearance.MouseOverBackColor = Color.Transparent;
            retryButton.FlatAppearance.MouseDownBackColor = Color.Transparent;
            retryButton.Cursor = Cursors.Hand; 

            retryButton.Location = new Point(
                (ClientSize.Width - retryButton.Width) / 2,
                (ClientSize.Height / 2) + 50
            );

            retryButton.Visible = false;
            retryButton.Click += (s, e) => RestartGame();
            this.Controls.Add(retryButton);
        }

        private Point newFood() 
        {
            Point newFood = new Point(random.Next(1, 23), random.Next(1, 17));
            while (snake.Contains(newFood))
            {
                newFood = new Point(random.Next(1, 23), random.Next(1, 17));
            }
            return newFood;
        }

        private void DrawFood(Graphics g)
        {
            
            g.DrawImage(foodImage, food.X * BlockSize, food.Y * BlockSize, BlockSize, BlockSize);

            // Bonus food (if exists)
            if (bonusFood.HasValue)
            {
                Brush bonusBrush = Brushes.Purple;
                g.FillEllipse(bonusBrush, bonusFood.Value.X * BlockSize, bonusFood.Value.Y * BlockSize, BlockSize, BlockSize);
            }
        }



        private void DrawGrid(Graphics g)
        {
            Pen gridPen = Pens.LightGray;
            int width = 500;
            int height = 380;

            // Draw vertical lines
            for (int x = 0; x <= width; x += BlockSize)
            {
                g.DrawLine(gridPen, x, 0, x, height);
            }

            // Draw horizontal lines
            for (int y = 0; y <= height; y += BlockSize)
            {
                g.DrawLine(gridPen, 0, y, width, y);
            }
        }


        private void DrawGridBorderLines(Graphics g)
        {
            using (Pen thickPen = new Pen(Color.Black, 20)) 
            {
                int width = 500;
                int height = 380;
                int halfThickness = 10; 

                g.DrawLine(thickPen, 0, halfThickness, width, halfThickness);

                g.DrawLine(thickPen, halfThickness, 0, halfThickness, height);

                g.DrawLine(thickPen, 0, height - halfThickness, width, height - halfThickness);

                g.DrawLine(thickPen, width - halfThickness, 0, width - halfThickness, height);
            }
        }

        private void DrawSnake(Graphics g)
        {
            for (int i = 0; i < snake.Count; i++)
            {
                Point p = snake[i];

                if (i == 0 && headImage != null)
                {
                    g.DrawImage(headImage, p.X * BlockSize, p.Y * BlockSize, BlockSize, BlockSize);
                }
                else
                {
                    Brush brush = Brushes.DarkGreen;
                    g.FillRectangle(brush, p.X * BlockSize, p.Y * BlockSize, BlockSize, BlockSize);
                }

               
            }
        }
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            MoveSnake();
            Invalidate(); 
            keyPressed = false;
        }

        private void MoveSnake()
        {
            Point head = snake[0];
            Point newHead = new Point(head.X + direction.X, head.Y + direction.Y);

            if (snake.Contains(newHead))
            { 
                isGameOver= true;
                retryButton.Visible = true;
                gameTimer.Stop();
                Invalidate();
                return;
            }

            if (newHead.X < 1 || newHead.X >= 24 || newHead.Y < 1 || newHead.Y >= 18)
            {
                isGameOver = true;
                retryButton.Visible = true;
                gameTimer.Stop();
                Invalidate();
                return;
            }

            snake.Insert(0, newHead);

            if (newHead == food)
            {
                food = newFood();
                score++;
                foodsEaten++;
                eatSound.Play();

                if (foodsEaten % 10 == 0)
                {
                    bonusFood = newBonusFood();
                }
            }
            else if (bonusFood.HasValue && newHead == bonusFood.Value)
            {
                bonusFood = null;
                score += 5;     
                bonusSound.Play();
            }
            else
            {
                snake.RemoveAt(snake.Count - 1);
            }
        }

        private Point newBonusFood()
        {
            Point p;
            do
            {
                p = new Point(random.Next(1, 23), random.Next(1, 17));
            } while (snake.Contains(p) || p == food);
            return p;
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (isGameOver) return;
            if (keyPressed) return;

            switch (e.KeyCode)
            {
                case Keys.Up:
                    if (direction.Y != 1)
                    {
                        direction = new Point(0, -1);
                        keyPressed = true;
                    }
                    break;
                case Keys.Down:
                    if (direction.Y != -1)
                    {
                        direction = new Point(0, 1);
                        keyPressed = true;
                    }
                    break;
                case Keys.Left:
                    if (direction.X != 1)
                    {                            
                        direction = new Point(-1, 0);
                        keyPressed = true;
                    }
                    break;
                case Keys.Right:
                    if (direction.X != -1)
                    {
                        direction = new Point(1, 0);
                        keyPressed = true;
                    }
                    break;
            }
        }

        private void RestartGame()
        {
       
            score = 0;
            isGameOver = false;
            keyPressed = false;
            direction = new Point(1, 0);
            snake.Clear();
            bonusFood = null;
            foodsEaten = 0;

           
            snake.Add(new Point(11, 7));
            snake.Add(new Point(10, 7));
            snake.Add(new Point(9, 7));

            food = newFood();

            retryButton.Visible = false;

            gameTimer.Start();

            Invalidate();
        }





    }
}