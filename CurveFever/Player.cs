using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurveFever
{
    // Klasa u koju spremamo informacije igraca
    public class Player
    {
        public double curve = 0.06f; //kut za koji skrece
        public double speed = 4.0; //radijus kruznice po kojoj skrece

        public Player()
        {
            left = false;
            right = false;
            alive = true;
            effects = new List<PlayerEffect> { };
        }

        public bool alive;
        public string Name { get; set; }

        public Keys LeftKey { get; set; }
        public bool left { get; set; }
        //je li tipka za lijevo trenutno pritisnuta

        public Keys RightKey { get; set; }
        public bool right { get; set; }
        //je li tipka za desno trenutno pritisnuta

        public Pen Pen { get; set; }
        //olovka kojom se crta zmija, za svaku razlièite boje
        public int score { get; set; }

        private double last_x, last_y;
        private double prelast_x, prelast_y;
        private double cur_x, cur_y;
        private int counter;

        public int game_width { get; set; }
        public int game_height { get; set; }
        public class PlayerEffect
        {
            public Food.Effects type;
            public int countdown;
            public PlayerEffect(Food.Effects type)
            {
                this.type = type;
                countdown = 300;
            }
        }
        public List<PlayerEffect> effects;
        private Rectangle dot;

        public void GeneratePosition(int game_width, int game_height)
        {
            this.game_width = game_width;
            this.game_height = game_height;

            Random rnd = new Random();

            heading = rnd.NextDouble() * Math.PI * 2;

            cur_x = Convert.ToDouble(rnd.Next(200, game_width - 200));
            cur_y = Convert.ToDouble(rnd.Next(200, game_height - 200));
            //da nije bas preblizu rubu na startu
            last_x = cur_x - speed * Math.Cos(heading);
            last_y = cur_y - speed * Math.Sin(heading);
            prelast_x = last_x - speed * Math.Cos(heading);
            prelast_y = last_y - speed * Math.Sin(heading);
            last_points = new Point[2];
            last_points[0] = new Point();
            last_points[1] = new Point();
            counter = 0;
            GetDot();
        }
        private Point[] last_points;
        public Point[] LastPoints
        { //zadnje dvije tocke u kojima se zmija nalazila
            get
            {
                last_points[0].X = Convert.ToInt32(prelast_x);
                last_points[0].Y = Convert.ToInt32(prelast_y);
                last_points[1].X = Convert.ToInt32(last_x);
                last_points[1].Y = Convert.ToInt32(last_y);
                return last_points;
            }
        }

        public Point CurrentPoint
        { // Trenutna tocka na kojoj se zmija nalazi
            get
            {
                return new Point(Convert.ToInt32(cur_x), Convert.ToInt32(cur_y));
            }
        }
        private double heading; //smjer u kojem se zmija krece
        private void LoopAroundScreen()
        {
            bool changed = false;
            while (true) // da osiguramo da su trenutne pozicije unutar ekrana!
            {
                changed = false;
                if (cur_x < 0)
                {
                    cur_x += game_width;
                    changed = true;
                }
                if (cur_y < 0)
                {
                    cur_y += game_height;
                    changed = true;
                }
                if (cur_x >= game_width)
                {
                    cur_x -= game_width;
                    changed = true;
                }
                if (cur_y >= game_height)
                {
                    cur_y -= game_height;
                    changed = true;
                }
                if (changed)
                {
                    last_x = cur_x;
                    last_y = cur_y;
                    prelast_x = cur_x;
                    prelast_y = cur_y;
                }
                else
                {
                    break;
                }
            }
        }
        public void Move()
        {
            counter++;
            if (right) MoveRight();
            if (left) MoveLeft();
            prelast_x = last_x;
            prelast_y = last_y;
            last_x = cur_x;
            last_y = cur_y;
            cur_x = last_x + speed * Math.Cos(heading);
            cur_y = last_y + speed * Math.Sin(heading);
            LoopAroundScreen();
            foreach (PlayerEffect effect in effects)
            {
                effect.countdown--;
            }
            if (effects.Count > 0 && effects[0].countdown <= 0)
            {
                UnEat(effects[0].type);
                effects.RemoveAt(0);
            }
        }
        private Rectangle GetDot()
        {
            dot = new Rectangle();
            dot.X = Convert.ToInt32(cur_x - Pen.Width * 0.7);
            dot.Y = Convert.ToInt32(cur_y - Pen.Width * 0.7);
            dot.Width = Convert.ToInt32(Pen.Width * 1.4);
            dot.Height = Convert.ToInt32(Pen.Width * 1.4);
            return dot;
        }
        public void DrawDot(Graphics g)
        {
            g.FillEllipse(new SolidBrush(Color.Green), GetDot());
        }
        public void Draw(Graphics novi)
        {
            if (counter % 100 < 95)
            {
                novi.DrawLine(Pen, LastPoints[0], LastPoints[1]);
            }
        }
        private void MoveLeft()
        {
            heading -= curve;
        }
        private void MoveRight()
        {
            heading += curve;
        }
        public bool CollidedWithWall()
        {
            return cur_x > game_width || cur_x < 0 || cur_y > game_height || cur_y < 0;
        }
        public void Eat(Food.Effects effect)
        {
            switch (effect)
            {
                case Food.Effects.Faster:
                    speed += 2;
                    break;
                case Food.Effects.Slower:
                    speed -= 1;
                    break;
                case Food.Effects.Thicker:
                    Pen = new Pen(Pen.Color, Pen.Width + 3);
                    break;
                default:
                    return;
            }
            effects.Add(new PlayerEffect(effect));
        }
        public void UnEat(Food.Effects effect)
        {
            switch (effect)
            {
                case Food.Effects.Faster:
                    speed -= 2;
                    break;
                case Food.Effects.Slower:
                    speed += 1;
                    break;
                case Food.Effects.Thicker:
                    Pen = new Pen(Pen.Color, Pen.Width - 3);
                    break;
                default:
                    return;
            }
        }
    }
}
