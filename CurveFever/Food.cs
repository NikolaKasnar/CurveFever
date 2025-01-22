using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CurveFever
{
    public partial class Food : UserControl
    {
        public enum Effects // tip efekta koji hrana može imati
        {
            None,
            Erase, // brisu se svi tragovi
            Faster, // zmija ide brze
            Slower, // zmija ide sporije
            OtherFaster, // sve ostale zmije idu brze
            Thicker, // podebljanje traga zmije
            AfterLast,
            First = Erase,
        }
        public Point Point { get; set; }
        public Effects type { get; set; }
        public Effects AntiType
        {
            get
            {
                return type switch
                {
                    Effects.OtherFaster => Effects.Faster,
                    _ => Effects.None,
                };
            }
        }
        public int width { get; set; }
        public int height { get; set; }
        public Color color { get; set; }


        public Food(int width, int height)
        {
            InitializeComponent();
            Random rnd = new Random();
            Point = new Point(rnd.Next(10, width - 10), rnd.Next(10, height - 10));
            type = (Effects) rnd.Next((int) Effects.First, (int) Effects.AfterLast);

            switch (type)
            {
                case Effects.Erase:
                    color = Color.SeaGreen;
                    break;
                case Effects.Faster:
                    color = Color.HotPink;
                    break;
                case Effects.Slower:
                    color = Color.SaddleBrown;
                    break;
                case Effects.OtherFaster:
                    color = Color.Silver;
                    break;
                case Effects.Thicker:
                    color = Color.White;
                    break;
            }

            this.width = width;
            this.height = height;
            Size = new Size(15,15);
        }

        public bool checkHunger(Player player)
        {
            if (Math.Abs(Point.X - player.LastPoints[1].X) <= Size.Width
                && Math.Abs(Point.Y - player.LastPoints[1].Y) <= Size.Height)
            {
                //igrac se zabio u hranu
                return true;
            }
            return false;
        }
    }
}
