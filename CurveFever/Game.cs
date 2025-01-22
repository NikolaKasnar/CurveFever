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
        public int width, height; //visina i sirina ekrana, jednaka kao za Panel1 u splitcontaineru
        List<Player> players;
        List<Keys> LeftKeys;
        List<Keys> RightKeys;
        List<Color> colors;
        List<Food> foods; //hrana koja je trenutno na ekranu
        int numberOfPlayers;
        int livingPlayers;
        static int penSize = 5;
        Pen[] pens = { new Pen(Color.Red, penSize), new Pen(Color.Yellow, penSize), new Pen(Color.Azure, penSize),
            new Pen(Color.Green, penSize), new Pen(Color.Violet, penSize), new Pen(Color.Blue, penSize)}; //boje igraca
        bool beginGame; //je li igra zapoceta/pauzirana (pritiskom SPACE)
        bool pauseGame;
        Bitmap currentState; //slika u koju se sprema trenutni izgled ekrana

        public Game(List<Player> players)
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            Dock = DockStyle.Fill;
            BackColor = Color.Black;
            this.width = Width;
            this.height = Height;
            Resize += ResizeGame;
            this.players = players;
            numberOfPlayers = players.Count;
            livingPlayers = numberOfPlayers;
            LeftKeys = new List<Keys>(numberOfPlayers);
            RightKeys = new List<Keys>(numberOfPlayers);
            colors = new List<Color>(numberOfPlayers);
            foods = new List<Food>(3);
            for (int i = 0; i < numberOfPlayers; i++)
            {
                players[i].GeneratePosition(width, height);
                players[i].Pen = pens[i];
                LeftKeys.Add(players[i].LeftKey);
                RightKeys.Add(players[i].RightKey);
                Color c = new Color();
                c = players[i].Pen.Color;
                colors.Add(c);
            }
            for (int i = 0; i < foods.Capacity; i++)
            {
                foods.Add(new Food(width, height));
            }

            currentState = new Bitmap(width, height);
            beginGame = false;
            pauseGame = true;
            Paint += GamePaint;
            KeyDown += GameKeyPress; //kod pritiska tipke
            KeyUp += GameKeyUp;
        }

        private void ResizeGame(object? sender, EventArgs e)
        {
            this.width = Width;
            this.height = Height;

            foreach (Player player in players) {
                player.game_height = Height;
                player.game_width = Width;
            }

            Bitmap newState = new Bitmap(width, height);
            Graphics novi = Graphics.FromImage(newState);
            novi.DrawImage(currentState, new Point(0, 0));
            currentState = newState;
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
            if(e.KeyCode == Keys.Space)
            {
                beginGame = true;
                pauseGame = !pauseGame;
            }

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



        private void collide(Player player)
        {
            if (player.alive)
            {
                player.alive = false;
                livingPlayers--;
            }
                
        }
        private void SetupGame(Graphics novi, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            novi.FillRectangle(new SolidBrush(Color.Black), new Rectangle(0, 0, width, height));

            for (int i = 0; i < numberOfPlayers; i++)
            {
                novi.DrawCurve(players[i].Pen, players[i].LastPoints);
                //pojavi se mala linija gdje je pocetak zmije, to se crta na sliku
            }
        }
        private bool CheckFoodCollision(Graphics novi, Player player)
        {
            for (int j = 0; j < foods.Count; j++)
            {
                if (foods[j].checkHunger(player))
                {
                    player.Eat(foods[j]);
                    if (foods[j].type == 1)
                    {
                        //tip 1
                        currentState = new Bitmap(width, height);
                    }
                    if (foods[j].type == 4)
                    {
                        //tip 4 - sve druge zmije postaju brze
                        for (int k = 0; k < numberOfPlayers; k++)
                        {
                            if (!players[k].Equals(player)) players[k].speed += 2;
                        }
                    }

                    //micanje hrane
                    novi.FillRectangle(new SolidBrush(Color.Black), new Rectangle(foods[j].Point, foods[j].Size));
                    foods.RemoveAt(j);
                    j = -1;

                    //dodavanje nove hrane, da uvijek bude 3 na terenu
                    foods.Add(new Food(width, height));

                    return true;
                }
            }
            return false;
        }
        private void ActivePaint(Graphics novi)
        {
            foreach (Food food in foods)
            {
                //crtanje hrane
                novi.FillRectangle(new SolidBrush(food.color), new Rectangle(food.Point, food.Size));
            }

            foreach (Player player in players)
            {
                if (player.alive)
                {
                    novi.DrawLine(player.Pen, player.LastPoints[0],
                    player.LastPoints[1]); //spajamo zadnje dvije pozicije zmije

                    player.Move();
                    if (player.CollidedWithWall())
                    {
                        collide(player);
                        continue;
                        //kad se zabije u zid umire, njegov trag ostaje na ekranu
                    }
                    Color pixColor = currentState.GetPixel(player.LastPoints[1].X,
                        player.LastPoints[1].Y);
                    // Ako boja nije skroz crna to znaci da se u nesto zabio!
                    if (pixColor.R != 0 || pixColor.G != 0 || pixColor.B != 0)
                    {
                        //prvo provjera je li se zabio u hranu
                        if (!CheckFoodCollision(novi, player))
                        {
                            //ako se zabio u nesto osim hrane, umire
                            collide(player);
                        }
                    }
                }
            }
    }
    private void GamePaint(object sender, PaintEventArgs e)
        {
            Graphics novi = Graphics.FromImage(currentState);
            //ovaj novi i bitmap currentState sluze za pamcenje stanja na grafici
            //ako netko zna pametniji nacin da se to radi nek napravi
            //ako se stanje ne pamti, pri svakom pozivu GamePaint izbrisat ce se dosad nacrtano
            var g = e.Graphics;

            if (!beginGame) //dodjeljivanje pocetnih pozicija
            {
                SetupGame(novi, e);
            }
            else if (!pauseGame)
            {
                ActivePaint(novi);
            }
            if (livingPlayers == 1)
            {
                pauseGame = true;
                novi.DrawString("gotovo", new Font("Arial", 14), Brushes.White, new Point(0, 0));
                //frozen slika tragova nakon sto svi umru
            }

            g.DrawImage(currentState, new Point(0, 0));

            novi.Dispose(); //ovo mora idk ne pitajte nista
        }

    }
}
