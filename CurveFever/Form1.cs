using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Windows.Forms;


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
            heading = 0.0f;
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

        public int game_width {  get; set; }
        public int game_height { get; set; }
        private class PlayerEffect
        {
            public Food.Effects type;
            public int countdown;
            public PlayerEffect(Food.Effects type)
            {
                this.type = type;
                countdown = 300;
            }
        }
        private List<PlayerEffect> effects;
        private Rectangle dot;

        public void GeneratePosition(int game_width, int game_height)
        {
            this.game_width = game_width;
            this.game_height = game_height;

            Random rnd = new Random();
            cur_x = Convert.ToDouble(rnd.Next(20, game_width - 200));
            cur_y = Convert.ToDouble(rnd.Next(20, game_height - 20));
            //da nije bas na preblizu (desnom) rubu na startu
            prelast_x = cur_x - 10;
            prelast_y = cur_y;
            last_x = cur_x - 5;
            last_y = cur_y;
            last_points = new Point[2];
            last_points[0] = new Point();
            last_points[1] = new Point();
            counter = 0;
            GetDot();
        }
        private Point[] last_points;
        public Point[] LastPoints
        { //zadnje dvije tocke u kojima se zmija nalazila
            get {
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
        private double heading;
        //smjer u kojem se zmija krece
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
    public partial class Form1 : Form
    {
        private Button btnStartGame;
        private Button btnExit;
        public Form1()
        {
            InitializeComponents();

            // Potrebno dodati ovo dvoje kako bi Esc radio u cijeloj formi prije ostalih kontrola
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;
        }

        // Pritiskom tipke Esc vracamo se na main menu
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Controls.Clear();
                InitializeComponents();
            }
        }

        private void InitializeComponents()
        {
            //za testiranje same igrice
            Player p = new Player();
            p.LeftKey = Keys.A; p.RightKey = Keys.D;
            Player p2 = new Player();
            p2.LeftKey = Keys.J; p2.RightKey = Keys.L;
            List<Player> list = new List<Player>() { p, p2 };
            StartGame(list);
            /*
            // Crtanje pocetne forme i naslova
            this.Text = "CurveFever";
            this.Size = new System.Drawing.Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Label lblTitle = new Label
            {
                Text = "CurveFever",
                Font = new System.Drawing.Font("Arial", 24, System.Drawing.FontStyle.Bold),
                AutoSize = true,
                Location = new System.Drawing.Point(300, 50)
            };
            this.Controls.Add(lblTitle);

            // Start game gumb
            btnStartGame = new Button
            {
                Text = "Start Game",
                Size = new System.Drawing.Size(100, 40),
                Location = new System.Drawing.Point(360, 120)
            };
            btnStartGame.Click += BtnStartGame_Click;
            this.Controls.Add(btnStartGame);

            // Exit game gumb
            btnExit = new Button
            {
                Text = "Exit",
                Size = new System.Drawing.Size(100, 40),
                Location = new System.Drawing.Point(360, 180)
            };
            btnExit.Click += BtnExit_Click;
            this.Controls.Add(btnExit);*/
        }

        // Klikom na gumb Start Game otvara se forma za odabir broja igraca
        private void BtnStartGame_Click(object sender, EventArgs e)
        {
            using (NumberOfPlayersForm numPlayersForm = new NumberOfPlayersForm())
            {
                if (numPlayersForm.ShowDialog() == DialogResult.OK)
                {
                    List<Player> players = numPlayersForm.Players;
                    StartGame(players);
                }
            }
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void StartGame(List<Player> players)
        {
            // Obrisemo prijasnji ekran
            this.Controls.Clear();

            this.Size = new System.Drawing.Size(1400, 800);

            int numberOfPlayers = players.Count;

            // Napravimo SplitContainer; lijevo je igra, a desno prikaz rezultata
            SplitContainer splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 5*Width/7,
                BorderStyle = BorderStyle.None,
                IsSplitterFixed = true
            };
            this.Controls.Add(splitContainer);
            //MessageBox.Show(splitContainer.Panel1.Width.ToString());

            //lijevi container, s novim Game objektom
            Game game = new Game(players);
            splitContainer.Panel1.Controls.Add(game);

            // Desni container
            Panel scorePanel = new Panel 
            {
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.Black 
            };
            
            // Sav tekst u desnom containeru
            Label lblRaceTo = new Label
            {
                Text = "Race to",
                Font = new System.Drawing.Font("Arial", 24, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.White,
                Location = new System.Drawing.Point(45, 10),
                AutoSize = true
            };
            scorePanel.Controls.Add(lblRaceTo);

            Label lbl10 = new Label
            {
                Text = "10",
                Font = new System.Drawing.Font("Arial", 50, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.White,
                Location = new System.Drawing.Point(50, 50),
                AutoSize = true
            };
            scorePanel.Controls.Add(lbl10);

            Label lblPointDifference = new Label
            {
                Text = "2 point difference",
                Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Regular),
                ForeColor = System.Drawing.Color.White,
                Location = new System.Drawing.Point(40, 170),
                AutoSize = true
            };
            scorePanel.Controls.Add(lblPointDifference);

            // Dodajemo dinamicki rezultate igraca
            string[] colors = { "Red", "Yellow", "Azure", "Green", "Violet", "Blue" };
            for (int i = 0; i < players.Count; i++)
            {
                Label playerScoreLabel = new Label
                {
                    Text = $"{players[i].Name} {players[i].score}",
                    Font = new System.Drawing.Font("Arial", 16, System.Drawing.FontStyle.Bold),
                    ForeColor = Color.FromName(colors[i % colors.Length]),
                    Location = new System.Drawing.Point(10, 210 + (30 * i)), // Adjust vertical spacing
                    AutoSize = true
                };
                scorePanel.Controls.Add(playerScoreLabel);
            }

            Label lblSpaceToPlay = new Label
            {
                Text = "SPACE to play",
                Font = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Regular),
                ForeColor = System.Drawing.Color.White,
                Location = new System.Drawing.Point(10, this.Height - 110),
                AutoSize = true
            };
            scorePanel.Controls.Add(lblSpaceToPlay);

            Label lblEscapeToQuit = new Label
            {
                Text = "ESCAPE to quit",
                Font = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Regular),
                ForeColor = System.Drawing.Color.White,
                Location = new System.Drawing.Point(10, this.Height - 80),
                AutoSize = true
            };
            scorePanel.Controls.Add(lblEscapeToQuit);
            
            splitContainer.Panel2.Controls.Add(scorePanel);
        }

        // Panel nema svojstvo za boju bordera tako da ga moramo dodati ovako
        private void GamePanel_Paint(object sender, PaintEventArgs e)
        {
            Panel panel = sender as Panel;
            if (panel != null)
            {
                using (System.Drawing.Pen yellowPen = new System.Drawing.Pen(System.Drawing.Color.Yellow, 5))
                {
                    e.Graphics.DrawRectangle(yellowPen, 0, 0, panel.Width - 1, panel.Height - 1);
                }
            }
        }
    }

    // Forma za unos broja igraca
    public class NumberOfPlayersForm : Form
    {
        private NumericUpDown numericUpDown;
        private Button btnOk;
        private FlowLayoutPanel playersPanel;
        private List<PlayerControls> playerControlsList = new List<PlayerControls>();

        public int NumberOfPlayers { get; private set; }
        // Lista svih igraca
        public List<Player> Players { get; private set; } = new List<Player>();

        public NumberOfPlayersForm()
        {
            this.Text = "Number of Players";
            this.Size = new Size(600, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Label lblNumberOfPlayers = new Label
            {
                Text = "Select Number of Players:",
                Font = new Font("Arial", 12, FontStyle.Regular),
                Location = new Point(20, 20),
                AutoSize = true
            };
            this.Controls.Add(lblNumberOfPlayers);

            numericUpDown = new NumericUpDown
            {
                Minimum = 2,
                Maximum = 6,
                Value = 2,
                Location = new Point(270, 20),
                Size = new Size(50, 20)
            };
            numericUpDown.ValueChanged += NumericUpDown_ValueChanged;
            this.Controls.Add(numericUpDown);

            playersPanel = new FlowLayoutPanel
            {
                Location = new Point(20, 60),
                Size = new Size(540, 400),
                AutoScroll = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(playersPanel);

            btnOk = new Button
            {
                Text = "OK",
                Location = new Point(260, 500),
                Size = new Size(75, 30)
            };
            btnOk.Click += BtnOk_Click;
            this.Controls.Add(btnOk);

            GeneratePlayerControls((int)numericUpDown.Value);
        }

        private void NumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            GeneratePlayerControls((int)numericUpDown.Value);
        }

        // Ova funkcija generira broj igraca ovisno o gornjoj vrijednosti
        private void GeneratePlayerControls(int playerCount)
        {
            playersPanel.Controls.Clear();
            playerControlsList.Clear();

            for (int i = 1; i <= playerCount; i++)
            {
                PlayerControls controls = new PlayerControls(i);
                playersPanel.Controls.Add(controls);
                playerControlsList.Add(controls);
            }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            Players.Clear();

            HashSet<Keys> usedKeys = new HashSet<Keys>();
            foreach (var playerControl in playerControlsList)
            {
                // Greske u slucaju ako je neko ime prazno ili se dupliciraju kontrole
                if (string.IsNullOrWhiteSpace(playerControl.PlayerName))
                {
                    MessageBox.Show($"Player {playerControl.PlayerIndex}'s name cannot be empty!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (usedKeys.Contains(playerControl.LeftKey) || usedKeys.Contains(playerControl.RightKey))
                {
                    MessageBox.Show($"Duplicate keys found for Player {playerControl.PlayerIndex}. Each key must be unique.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                usedKeys.Add(playerControl.LeftKey);
                usedKeys.Add(playerControl.RightKey);

                Players.Add(new Player
                {
                    Name = playerControl.PlayerName,
                    LeftKey = playerControl.LeftKey,
                    RightKey = playerControl.RightKey,
                    score = 0
                });
            }

            NumberOfPlayers = Players.Count;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private class PlayerControls : Panel
        {
            public int PlayerIndex { get; }
            private TextBox txtName;
            private Button btnLeftKey;
            private Button btnRightKey;
            public string PlayerName => txtName.Text;
            public Keys LeftKey { get; private set; }
            public Keys RightKey { get; private set; }

            public PlayerControls(int index)
            {
                PlayerIndex = index;

                this.Size = new Size(500, 80);
                this.BorderStyle = BorderStyle.FixedSingle;

                Label lblPlayer = new Label
                {
                    Text = $"Player {index}:",
                    Font = new Font("Arial", 10, FontStyle.Bold),
                    Location = new Point(10, 10),
                    AutoSize = true
                };
                this.Controls.Add(lblPlayer);

                txtName = new TextBox
                {
                    PlaceholderText = "Enter name...",
                    Location = new Point(100, 10),
                    Size = new Size(120, 20)
                };
                this.Controls.Add(txtName);

                btnLeftKey = new Button
                {
                    Text = "Set Left Key",
                    Location = new Point(240, 10),
                    Size = new Size(100, 30)
                };
                btnLeftKey.Click += BtnLeftKey_Click;
                this.Controls.Add(btnLeftKey);

                btnRightKey = new Button
                {
                    Text = "Set Right Key",
                    Location = new Point(360, 10),
                    Size = new Size(120, 30)
                };
                btnRightKey.Click += BtnRightKey_Click;
                this.Controls.Add(btnRightKey);
            }

            // Kontrole za biranje gumba za micanje lijevo/desno
            private void BtnLeftKey_Click(object sender, EventArgs e)
            {
                Keys selectedKey = ShowKeySelectionDialog("Press the key for Left Control");
                if (selectedKey != Keys.None)
                {
                    LeftKey = selectedKey;
                    // Ispisemo gumb koji je odabran za tu kontrolu
                    btnLeftKey.Text = LeftKey.ToString();
                }
            }

            private void BtnRightKey_Click(object sender, EventArgs e)
            {
                Keys selectedKey = ShowKeySelectionDialog("Press the key for Right Control");
                if (selectedKey != Keys.None)
                {
                    RightKey = selectedKey;
                    btnRightKey.Text = RightKey.ToString();
                }
            }

            // Funkcija koja prihvaca tipku tipkovnice za input
            private Keys ShowKeySelectionDialog(string prompt)
            {
                Keys selectedKey = Keys.None;

                Form keySelectionForm = new Form
                {
                    Text = "Key Selection",
                    Size = new Size(300, 150),
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false
                };

                Label lblPrompt = new Label
                {
                    Text = prompt,
                    AutoSize = true,
                    Location = new Point(50, 30),
                    Font = new Font("Arial", 10, FontStyle.Regular)
                };
                keySelectionForm.Controls.Add(lblPrompt);

                keySelectionForm.KeyPreview = true;
                keySelectionForm.KeyDown += (s, e) =>
                {
                    selectedKey = e.KeyCode;
                    keySelectionForm.Close();
                };

                keySelectionForm.ShowDialog();
                return selectedKey;
            }
        }
    }
}
