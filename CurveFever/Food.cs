﻿using CurveFever.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Resources;
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
            RemoveWall, // zidovi privremeno nestanu
            AfterLast,
            First = Erase,
        }
        public Point Point { get; set; } // gdje se hrana nalazi
        public Effects type { get; set; } // tip efekta koji hrana ima
        public Effects AntiType
        { // crvene kuglice oznacavaju da sve druge zmije trebaju dobiti neki efekt
            // za takve kuglice je AntiType jednak efektu koji ce druge zmije osjetiti
            // inace je None
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
        public Bitmap Picture { get; set; } // slika kuglice

        public Food(int width, int height)
        {
            InitializeComponent();
            Random rnd = new Random();
            type = (Effects) rnd.Next((int) Effects.First, (int) Effects.AfterLast);
            MemoryStream ImageData;

            switch (type) // ovisno o tipu ucitavamo resurs
            {
                case Effects.Erase:
                    ImageData = new MemoryStream(Properties.Resources.izbrisi);
                    break;
                case Effects.Faster:
                    ImageData = new MemoryStream(Properties.Resources.ubrzanje);
                    break;
                case Effects.Slower:
                    ImageData = new MemoryStream(Properties.Resources.usporavanje);
                    break;
                case Effects.OtherFaster:
                    ImageData = new MemoryStream(Properties.Resources.ubrzanje_drugih);
                    break;
                case Effects.Thicker:
                    ImageData = new MemoryStream(Properties.Resources.udebljavanje);
                    break;
                case Effects.RemoveWall:
                    ImageData = new MemoryStream(Properties.Resources.zidovi);
                    break;
                default:
                    ImageData = new MemoryStream(Properties.Resources.izbrisi);
                    break;
            }

            Picture = new Bitmap(ImageData); // i od tog resursa radimo sliku 
            this.width = width;
            this.height = height;
            Size = Picture.Size;
            Point = new Point(rnd.Next(10, width - 10 - Size.Width),
                              rnd.Next(10, height - 10 - Size.Height));
        }

        public bool checkHunger(Player player)
        {
            if (Math.Abs(Point.X - player.CurrentPoint.X) <= Size.Width
                && Math.Abs(Point.Y - player.CurrentPoint.Y) <= Size.Height)
            {
                //igrac se zabio u hranu
                return true;
            }
            return false;
        }
    }
}
