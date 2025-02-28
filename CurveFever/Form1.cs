using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Windows.Forms;


namespace CurveFever
{
    public partial class Form1 : Form
    {
        private Button btnStartGame;
        private Button btnExit;
        public SplitContainer splitContainer;
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
            /*
            Player p = new Player();
            p.LeftKey = Keys.A; p.RightKey = Keys.D;
            Player p2 = new Player();
            p2.LeftKey = Keys.J; p2.RightKey = Keys.L;
            List<Player> list = new List<Player>() { p, p2 };
            StartGame(list);
            */

            
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
            this.Controls.Add(btnExit);
            
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

        // Dictionary potreban za pamcenje svih rezultata igraca
        private Dictionary<Player, Label> playerScoreLabels = new Dictionary<Player, Label>();

        private void StartGame(List<Player> players)
        {
            // Obrisemo prijasnji ekran
            this.Controls.Clear();

            this.Size = new System.Drawing.Size(1400, 800);

            int numberOfPlayers = players.Count;

            // Napravimo SplitContainer; lijevo je igra, a desno prikaz rezultata
            splitContainer = new SplitContainer
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
            // Saljemo i funkciju za azuriranje rezultata u drugom split containeru
            Game game = new Game(players, UpdateScores);
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

            // Dodajemo dinamicki rezultate igraca
            string[] colors = { "Red", "Yellow", "Azure", "Green", "Violet", "Blue" };
            playerScoreLabels.Clear();// Obrisemo sve prije dodavanja novog rezultata

            for (int i = 0; i < players.Count; i++)
            {
                Label playerScoreLabel = new Label
                {
                    Text = $"{players[i].Name} {players[i].score}",
                    Font = new System.Drawing.Font("Arial", 16, System.Drawing.FontStyle.Bold),
                    ForeColor = Color.FromName(colors[i % colors.Length]),
                    Location = new System.Drawing.Point(10, 210 + (30 * i)),
                    AutoSize = true
                };
                // Pohranimo reference za azuriranja rezultata
                playerScoreLabels[players[i]] = playerScoreLabel;
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

        // Metoda za azuriranje rezultata
        private void UpdateScores(Player player)
        {
            if (playerScoreLabels.ContainsKey(player))
            {
                playerScoreLabels[player].Text = $"{player.Name} {player.score}";
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

                if(playerControl.LeftKey == playerControl.RightKey)
                {
                    MessageBox.Show("Odaberite dvije razlicite tipke za lijevo i desno!");
                    return;
                }

                List<Keys> forbiddenKeys = new List<Keys>
                    { Keys.Space, Keys.Escape, Keys.Up, Keys.Down, Keys.Left, Keys.Right};


                if (forbiddenKeys.Contains(playerControl.LeftKey) || forbiddenKeys.Contains(playerControl.RightKey))
                {
                    MessageBox.Show("Odaberite tipke razlicite od strelica, ESC i SPACE");
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
