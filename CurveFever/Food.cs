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
        /*popis tipova (ucinaka hrane):
        1 - brisu se svi tragovi
        2 - zmija ide brze
        3 - zmija ide sporije
        4 - sve ostale zmije idu brze
        5 - podebljanje traga zmije
        */

        public Point Point { get; set; }
        public int type { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public Color color { get; set; }


        public Food(int type, Point point, int width, int height)
        {
            InitializeComponent();

            this.type = type;
            if(type == 1) color = Color.SeaGreen;
            if(type == 2) color = Color.HotPink;
            if(type == 3) color = Color.SaddleBrown;
            if(type == 4) color = Color.Silver;
            if(type == 5) color = Color.White;

            this.width = width;
            this.height = height;
            Size = new Size(15,15);
            this.Point = point;
        }


        public bool checkHunger(Player player)
        {
            if (Math.Abs(Point.X - player.LastPoints[1].X) <= Size.Width
                & Math.Abs(Point.Y - player.LastPoints[1].Y) <= Size.Height)
            {
                //igrac se zabio u hranu
                return true;
            }
            return false;
        }

        public int eat(Player player)
        {
            //ucinci tipova 1 i 4 kontroliraju se u klasi Game
            if(type == 1)
            {
                return 1;
            }
            if (type == 2)
            {
                player.speed += 1;
            }
            if (type == 3)
            {
                player.speed -= 0.5;
            }
            if (type == 4)
            {
                return 4;
            }
            if(type == 5)
            {
                float size = player.Pen.Width;
                player.Pen = new Pen(player.Pen.Color, size + 3);
            }
            return 0;
        }
    }
}
