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
using System.Numerics;

namespace CurveFever
{

    public partial class Game : UserControl
    {
        public int width, height; //visina i sirina ekrana, jednaka kao za Panel1 u splitcontaineru
        List<Player> players; //popis igraca
        List<Food> foods; //hrana koja je trenutno na ekranu
        int numberOfPlayers;
        int livingPlayers;
        int victoryScore = 2; //broj bodova potreban za pobjedu
        const int penSize = 6;
        Pen[] pens = { new Pen(Color.Red, penSize), new Pen(Color.Yellow, penSize), new Pen(Color.Azure, penSize),
            new Pen(Color.Green, penSize), new Pen(Color.Violet, penSize), new Pen(Color.Blue, penSize)}; //boje igraca
        bool beginGame; //je li igra zapoceta (pritiskom SPACE)
        bool pauseGame; //je li igra pauzirana (pritiskom SPACE)
        bool newRound = false; //je li zapoceta nova runda
        private int ticks_until_wall; // koliko jos timer tick-ova dok se zid opet ne pojavi
        // 0 ako se zid treba crtati
        private Pen wall_pen;
        private Rectangle wall_rect;
        Bitmap currentState; //slika u koju se sprema trenutni izgled ekrana
        // Metoda za azuriranje rezultata
        private Action<Player> updateScores;

        public Game(List<Player> players, Action<Player> updateScores)
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
            foods = new List<Food>(3);
            InitPlayers();
            for (int i = 0; i < foods.Capacity; i++)
            {
                foods.Add(new Food(width, height));
            }

            currentState = new Bitmap(width, height);
            beginGame = false;
            pauseGame = true;

            ticks_until_wall = 0;
            wall_pen = new Pen(new SolidBrush(Color.DarkBlue), 8);
            wall_rect = new Rectangle(0, 0, width, height);

            Paint += GamePaint;
            KeyDown += GameKeyPress; //kod pritiska tipke
            KeyUp += GameKeyUp;
            this.updateScores = updateScores;
        }

        private void InitPlayers()
        {
            //dodjeljivanje boje i pozicije igracu
            //poziva se na pocetku igre te svake nove runde
            for (int i = 0; i < numberOfPlayers; i++)
            {
                players[i].alive = true;
                players[i].Pen = pens[i];
                players[i].GeneratePosition(width, height);
            }
        }

        private void ResizeGame(object? sender, EventArgs e)
        {
            this.width = Width;
            this.height = Height;

            foreach (Player player in players)
            {
                player.game_height = Height;
                player.game_width = Width;
            }

            Bitmap newState = new Bitmap(width, height);
            Graphics novi = Graphics.FromImage(newState);
            novi.DrawImage(currentState, new Point(0, 0));
            currentState = newState;
            wall_rect = new Rectangle(2, 2, width - 5, height - 5);
        }

        private void TimerTick(object sender, EventArgs e)
        {
            Refresh();
            //za svaki otkucaj sata zove se GamePaint funkcija
            //tako se zmija neprestano krece dalje
            //postavke timera su u Game.cs [Design]
        }

        private void GameKeyPress(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space) //igra se pauzira/pokrece
            {
                beginGame = true;
                pauseGame = !pauseGame;
                if (newRound)
                {
                    //nova runda - crta se prazna slika
                    newRound = false;
                    currentState = new Bitmap(width, height);
                    pauseGame = true;
                }
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
                //zmija umire - vise se ne moze kretati do kraja runde
                //njen trag ostaje na ekranu
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
                players[i].Draw(novi);
                //pojavi se mala linija gdje je pocetak zmije, to se crta na sliku
            }
        }
        private bool CheckFoodCollision(Graphics novi, Player player)
        {
            for (int j = 0; j < foods.Count; j++)
            {
                if (foods[j].checkHunger(player))
                {
                    player.Eat(foods[j].type);

                    Food.Effects other = foods[j].AntiType;
                    if (other != Food.Effects.None)
                    {
                        for (int k = 0; k < numberOfPlayers; k++)
                        {
                            if (!players[k].Equals(player))
                                players[k].Eat(other);
                        }
                    }

                    if (foods[j].type == Food.Effects.Erase)
                    {
                        // brisanje cijelog ekrana
                        currentState = new Bitmap(width, height);
                    }


                    if (foods[j].type == Food.Effects.RemoveWall)
                    {
                        ticks_until_wall = 300;
                        novi.DrawRectangle(new Pen(Color.Black, 8), wall_rect);
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
            if (ticks_until_wall == 0) // crtaj zid
            {
                novi.DrawRectangle(wall_pen, wall_rect);
            }
            else
            {
                ticks_until_wall--;
            }
            foreach (Food food in foods)
            {
                //crtanje hrane
                novi.DrawImageUnscaled(food.Picture, food.Point);
            }

            foreach (Player player in players)
            {
                if (player.alive)
                {
                    player.Move();
                    Color pixColor = currentState.GetPixel(player.CurrentPoint.X,
                        player.CurrentPoint.Y);
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

                    player.Draw(novi);
                }
            }
        }


        private void GamePaint(object sender, PaintEventArgs e)
        {
            Graphics novi = Graphics.FromImage(currentState);
            //varijabla novi i bitmap currentState sluze za pamcenje stanja na grafici,
            //tj. trenutno nacrtanih tragova i hrane na ekranu

            var g = e.Graphics;

            if (!beginGame) //dodjeljivanje pocetnih pozicija
            {
                SetupGame(novi, e);

            }
            else if (!pauseGame)
            {
                ActivePaint(novi);
            }

            if (livingPlayers == 1) //zavrsetak runde
            {
                pauseGame = true;
                DialogResult ok = DialogResult.No;
                Player victor = null;
                foreach (Player player in players)
                {
                    //igracu koji je ziv povecat ce se score
                    if (player.alive)
                    {
                        player.score++;
                        updateScores?.Invoke(player); // Azuriraj rezultat u UI
                    }
                    if (player.score == victoryScore)
                    {
                        updateScores?.Invoke(player); // Azuriraj rezultat u UI
                        victor = player;
                        break;
                    }
                }

                //iduca runda: broj zivih igraca se resetira
                //te se oni inicijaliziraju
                newRound = true;
                livingPlayers = numberOfPlayers;
                InitPlayers();

                if(victor!=null) //poruka o pobjedi
                    ok = MessageBox.Show("Pobjednik je " + victor.Name + "!");

                if (ok == DialogResult.OK)
                {
                    foreach (Player p in players)
                    {
                        p.score = 0; //igra pocinje ispocetka, svi imaju 0 bodova
                        updateScores?.Invoke(p);
                    }
                    //odmah se crta prazna slika, da se ne mora dodatno pritisnuti SPACE
                    newRound = false;
                    currentState = new Bitmap(width, height);
                    pauseGame = true;
                }
            }

            g.DrawImage(currentState, new Point(0, 0));

            if (!newRound)
            {
                foreach (Player player in players)
                {
                    player.DrawDot(g);
                }
            }

            novi.Dispose();
        }
    }
}
