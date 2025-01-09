using System;
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

namespace CurveFever
{
    public partial class Game : UserControl
    {
        int width, height; //visina i sirina ekrana, jednaka kao za Panel1 u splitcontaineru
        int numberOfPlayers;
        bool start; //je li pocetna pozicija ili ne
        Point[][] lastPoints; //zadnje dvije tocke na kojima se nalaze igraci, koje ce se onda povezati linijom
        static int penSize = 5;
        Pen[] pens = { new Pen(Color.Red, penSize), new Pen(Color.Yellow, penSize), new Pen(Color.Azure, penSize),
            new Pen(Color.Green, penSize), new Pen(Color.Violet, penSize), new Pen(Color.Blue, penSize)}; //boje igraca
        int[][] controls; //za svakog igraca sadrzi koje su tipke za lijevo i desno
        //enum za Keys: https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.keys?view=windowsdesktop-9.0
        Bitmap currentState; //slika u koju se sprema trenutni izgled ekrana
        int curve = 30; //kut za koji skrece, treba neka slozenija trig kod MoveLeft i Right
        int radius = 20; //radijus kruznice po kojoj skrece

        public Game(int numberOfPlayers)
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
            this.width = Width;
            this.height = Height;
            this.numberOfPlayers = numberOfPlayers;
            lastPoints = new Point[numberOfPlayers][];
            controls = new int[numberOfPlayers][];
            for (int i = 0; i < numberOfPlayers; i++)
            {
                lastPoints[i] = new Point[2];
                controls[i] = new int[2];
            }
            //trenutno stavljeno i testirano za jednog igraca,
            //kasnije cemo valjda prosljeđivat kontrole za svakog u ovaj konstruktor
            controls[0][0] = 65; //'a' za lijevo
            controls[0][1] = 68; //'d' za desno
            currentState = new Bitmap(width, height);
            start = true;
            Paint += GamePaint;
            KeyDown += GameKeyPress; //kod pritiska tipke
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
            var key = (e as KeyEventArgs).KeyValue;
            for (int i = 0; i < numberOfPlayers; i++)
            {
                if (controls[i][0] == key) MoveLeft(i);
                //pritisnuta je tipka za lijevo kod i-tog igraca
            }
            for (int i = 0; i < numberOfPlayers; i++)
            {
                if (controls[i][1] == key) MoveRight(i);
                //pritisnuta je tipka za desno kod i-tog igraca

            }
        }

        private void MoveLeft(int player)
        {
            //zadnje koordinate na kojima je zmija
            int x = lastPoints[player][1].X;
            int y = lastPoints[player][1].Y;

            lastPoints[player][0] = new Point(x, y); //ovo je stara pozicija
            lastPoints[player][1] = new Point(x - radius + Convert.ToInt32(Math.Round(radius* Math.Cos(curve))), y + Convert.ToInt32(Math.Round(radius * Math.Sin(curve)))); //ovo je nova ljevija pozicija

            Refresh();
        }

        private void MoveRight(int player)
        {
            //zadnje koordinate na kojima je zmija
            int x = lastPoints[player][1].X;
            int y = lastPoints[player][1].Y;

            lastPoints[player][0] = new Point(x, y); //ovo je stara pozicija
            lastPoints[player][1] = new Point(x + radius + Convert.ToInt32(Math.Round(radius * Math.Cos(curve))), y + Convert.ToInt32(Math.Round(radius * Math.Sin(curve)))); //ovo je nova desnija pozicija

            Refresh();
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
                novi.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                Random rnd = new Random();

                for (int i = 0; i < numberOfPlayers; i++)
                {
                    Point start = new Point(rnd.Next(10, width - 10), rnd.Next(10, height - 10));
                    //da nije bas na rubu ekrana na startu
                    lastPoints[i][0] = start; //svatko dobije random pocetnu poziciju
                    lastPoints[i][1] = new Point(start.X, start.Y-1); //svatko dobije random pocetnu poziciju
                    novi.DrawCurve(pens[i], lastPoints[i]); 
                    //pojavi se mala linija gdje je pocetak zmije, to se crta na sliku
                    g.DrawImage(currentState, new Point(0, 0)); //sad tu sliku crtamo na ekran
                }
                start = false;
            }
            else
            {
                for (int i = 0; i < numberOfPlayers; i++)
                {
                    novi.DrawCurve(pens[i], lastPoints[i]); //spajamo zadnje dvije pozicije zmije
                    g.DrawImage(currentState, new Point(0, 0));
                    Point p = new Point(lastPoints[i][1].X, lastPoints[i][1].Y - 1);
                    lastPoints[i][0] = lastPoints[i][1]; //dosad zadnja pozicija postaje predzadnja
                    lastPoints[i][1] = p; //novonapravljena pozicija postaje zadnja
                }

            }

            novi.Dispose(); //ovo mora idk ne pitajte nista
        }

    }
}
