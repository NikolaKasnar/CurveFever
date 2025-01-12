using System;
using System.Windows.Input;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using Microsoft.VisualBasic.Devices;

namespace CurveFever
{
    public partial class Game : UserControl
    {
        string pix;
        int width, height; //visina i sirina ekrana, jednaka kao za Panel1 u splitcontaineru
        List<Player> players;
        List<Keys> LeftKeys;
        List<Keys> RightKeys;
        List<Color> colors;
        int numberOfPlayers;
        static int penSize = 5;
        Pen[] pens = { new Pen(Color.Red, penSize), new Pen(Color.Yellow, penSize), new Pen(Color.Azure, penSize),
            new Pen(Color.Green, penSize), new Pen(Color.Violet, penSize), new Pen(Color.Blue, penSize)}; //boje igraca
        bool start; //je li pocetna pozicija ili ne
        Bitmap currentState; //slika u koju se sprema trenutni izgled ekrana
        const float curve = 0.05f; //kut za koji skrece, treba neka slozenija trig kod MoveLeft i Right
        int radius = 5; //radijus kruznice po kojoj skrece

        public Game(List<Player> players)
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            Dock = DockStyle.Fill;
            BackColor = Color.Black;
            this.width = Width;
            this.height = Height;
            this.players = players;
            numberOfPlayers = players.Count;
            LeftKeys = new List<Keys>(numberOfPlayers);
            RightKeys = new List<Keys>(numberOfPlayers);
            colors = new List<Color>(numberOfPlayers);
            for (int i = 0; i < numberOfPlayers; i++)
            {
                players[i].lastPoints = new Point[2];
                players[i].Pen = pens[i];
                players[i].left = false;
                players[i].right = false;
                LeftKeys.Add(players[i].LeftKey);
                RightKeys.Add(players[i].RightKey);
                Color c = new Color();
                c = players[i].Pen.Color;
                colors.Add(c);
                players[i].heading = 0.0f;
            }

            currentState = new Bitmap(width, height);
            start = true;
            Paint += GamePaint;
            KeyDown += GameKeyPress; //kod pritiska tipke
            KeyUp += GameKeyUp;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            Refresh();
            //za svaki otkucaj sata zove se GamePaint fja
            //tako se zmija neprestano krece gore
            //postavke timera su u Game.cs [Design]
        }

        private void GameKeyPress(object? sender, KeyEventArgs e)
        {
            /* //alt verzija, dođe na isto
            int left = LeftKeys.FindIndex(key => key == e.KeyCode);
            if (left != -1)
            {
                MoveLeft(left);
            }
            int right = RightKeys.FindIndex(key => key == e.KeyCode);
            if (right != -1)
            {
                MoveRight(right);
            }
            */
            for (int i = 0; i < numberOfPlayers; i++)
            {
                if (players[i].LeftKey == e.KeyCode)
                {
                    players[i].left = true;
                }
                //pritisnuta je tipka za lijevo kod i-tog igraca
            }
            for (int i = 0; i < numberOfPlayers; i++)
            {
                if (players[i].RightKey == e.KeyCode)
                {
                    players[i].right = true;
                }
                //pritisnuta je tipka za desno kod i-tog igraca
            }

        }

        private void GameKeyUp(object? sender, KeyEventArgs e)
        {
            for (int i = 0; i < numberOfPlayers; i++)
            {
                if (players[i].LeftKey == e.KeyCode) players[i].left = false;
            }
            for (int i = 0; i < numberOfPlayers; i++)
            {
                if (players[i].RightKey == e.KeyCode) players[i].right = false;
            }
        }


        private void MoveLeft(int player)
        {
            players[player].heading -= curve;
        }

        private void MoveRight(int player)
        {
            players[player].heading += curve;
        }

        private void collide(Player i)
        {
            if (players.Contains(i))
            {
                players.Remove(i);
                numberOfPlayers--;
            }

        }

        private void GamePaint(object sender, PaintEventArgs e)
        {
            Graphics novi = Graphics.FromImage(currentState);
            //ovaj novi i bitmap currentState sluze za pamcenje stanja na grafici
            //ako netko zna pametniji nacin da se to radi nek napravi
            //ako se stanje ne pamti, pri svakom pozivu GamePaint izbrisat ce se dosad nacrtano
            var g = e.Graphics;

            if (start) //dodjeljivanje pocetnih pozicija
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                Random rnd = new Random();

                for (int i = 0; i < numberOfPlayers; i++)
                {
                    Point start = new Point(rnd.Next(10, width - 10), rnd.Next(10, height - 10));
                    //da nije bas na rubu ekrana na startu
                    players[i].lastPoints[0] = start;
                    players[i].lastPoints[1] = new Point(start.X, start.Y - 1);
                    //svatko dobije random pocetnu poziciju
                    novi.DrawCurve(players[i].Pen, players[i].lastPoints);
                    Color pixColor = currentState.GetPixel(players[i].lastPoints[0].X, players[i].lastPoints[0].Y);
                    pix = pixColor.Name;
                    //novi.DrawString(pixColor.Name, new Font("Arial", 14), Brushes.White, new Point(0, 0));

                    //pojavi se mala linija gdje je pocetak zmije, to se crta na sliku
                    g.DrawImage(currentState, new Point(0, 0)); //sad tu sliku crtamo na ekran
                }
                start = false;
                foreach (var col in colors)
                {
                    //MessageBox.Show(col.Name);
                }
            }
            else
            {
                for (int i = 0; i < numberOfPlayers; i++)
                {
                    novi.DrawCurve(players[i].Pen, players[i].lastPoints); //spajamo zadnje dvije pozicije zmije
                    g.DrawImage(currentState, new Point(0, 0));
                    Point p = new Point(
                        Convert.ToInt32(players[i].lastPoints[0].X + radius * Math.Cos(players[i].heading)),
                        Convert.ToInt32(players[i].lastPoints[1].Y + radius * Math.Sin(players[i].heading)));
                    players[i].lastPoints[0] = players[i].lastPoints[1];
                    players[i].lastPoints[1] = p;
                    //dosad zadnja pozicija postaje predzadnja
                    //novonapravljena pozicija postaje zadnja
                    if (p.X < 0 || p.Y < 0 || p.X > width || p.Y > height)
                    {
                        //kad se zabije u zid umire, njegov trag ostaje na ekranu
                        if (i < players.Count)
                        {
                            collide(players[i]);
                            i = -1; //for petlja krece iz pocetka bez mrtve zmije
                        }
                    }
                    else
                    {
                        //sudar zmije s nekim tragom
                        //ovo ne radi tbh
                        if (p.X < width && p.Y < height)
                        {
                            Color pixColor = currentState.GetPixel(p.X, p.Y);

                            if (colors.Contains(pixColor))
                            {
                                collide(players[i]);
                                i = -1;
                            }
                            else //nema sudara za i-tog igraca
                            {
                                if (players[i].left) MoveLeft(i);
                                if (players[i].right) MoveRight(i);
                            }
                        }

                    }
                }

            }

            if (numberOfPlayers == 0)
            {
                novi.DrawString("gotovo", new Font("Arial", 14), Brushes.White, new Point(0, 0));
                g.DrawImage(currentState, new Point(0, 0));
                //frozen slika tragova nakon sto svi umru
            }

            novi.Dispose(); //ovo mora idk ne pitajte nista
        }

        private void Game_Load(object sender, EventArgs e)
        {

        }
    }
}
