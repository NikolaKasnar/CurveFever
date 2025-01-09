using System;
using System.Windows.Forms;

namespace CurveFever
{
    public partial class Form1 : Form
    {
        private Button btnStartGame;
        private Button btnExit;
        public Form1()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            
            //u svrhu testiranja Game klase
            //ona zasad ne radi za vise playera jer nisu definirane kontrole za njih
            //za vidjeti meni treba ove dvije linije zakomentirati
            //a ostalo u ovoj funkciji otkomentirati
            int numberOfPlayers = 1;
            StartGame(numberOfPlayers);
            
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
                    int numberOfPlayers = numPlayersForm.NumberOfPlayers;
                    StartGame(numberOfPlayers);
                }
            }
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void StartGame(int numberOfPlayers)
        {
            // Obrisemo prijasnji ekran
            this.Controls.Clear();

            this.Size = new System.Drawing.Size(1400, 800);

            // Napravimo SplitContainer; lijevo je igra, a desno prikaz rezultata
            SplitContainer splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 3*Width/4,
                BorderStyle = BorderStyle.None,
                IsSplitterFixed = true
            };
            this.Controls.Add(splitContainer);

            // Lijevi container(igra)
            /*Panel gamePanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.Black
            };
            // Dodavanje zutog bordera
            gamePanel.Paint += GamePanel_Paint;
            splitContainer.Panel1.Controls.Add(gamePanel);*/

            //lijevi container, s novim Game objektom
            Game game = new Game(numberOfPlayers);
            splitContainer.Panel1.Controls.Add(game);
            splitContainer.Panel1.Paint += GamePanel_Paint;

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
        // Stavio sam ovu opciju da igrac ne moze unesti manje od 2 igraca niti bilo sta drugo
        // Ako zelite mozemo i drugacije, pogotovo ako cemo stavljati i onaj odabir kontrola
        // Ovo je samo za pocetak
        private NumericUpDown numericUpDown;
        private Button btnOk;
        public int NumberOfPlayers { get; private set; }

        public NumberOfPlayersForm()
        {
            this.Text = "Number of Players";
            this.Size = new System.Drawing.Size(450, 150);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            numericUpDown = new NumericUpDown
            {
                Minimum = 2,
                Maximum = 8, // Ovo mozemo promijeniti ako zelimo, samo za pocetak nek je tu
                Value = 2,
                Location = new System.Drawing.Point(150, 20),
                Size = new System.Drawing.Size(150, 20)
            };
            this.Controls.Add(numericUpDown);

            // OK gumb
            btnOk = new Button
            {
                Text = "OK",
                Location = new System.Drawing.Point(200, 60),
                Size = new System.Drawing.Size(75, 30)
            };
            btnOk.Click += BtnOk_Click;
            this.Controls.Add(btnOk);
        }

        // Pri kliku spremimo broj igraca
        private void BtnOk_Click(object sender, EventArgs e)
        {
            NumberOfPlayers = (int)numericUpDown.Value;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
