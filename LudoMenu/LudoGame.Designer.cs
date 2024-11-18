namespace LudoMenu
{
    partial class LudoGame
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnRollDice = new System.Windows.Forms.Button();
            this.lblCurrentPlayer = new System.Windows.Forms.Label();
            this.lblNarrator = new System.Windows.Forms.Label();
            this.lblDadoNum = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnRollDice
            // 
            this.btnRollDice.Location = new System.Drawing.Point(684, 40);
            this.btnRollDice.Name = "btnRollDice";
            this.btnRollDice.Size = new System.Drawing.Size(104, 46);
            this.btnRollDice.TabIndex = 0;
            this.btnRollDice.Text = "Tirar Dado";
            this.btnRollDice.UseVisualStyleBackColor = true;
            this.btnRollDice.Click += new System.EventHandler(this.btnRollDice_Click);
            // 
            // lblCurrentPlayer
            // 
            this.lblCurrentPlayer.AutoSize = true;
            this.lblCurrentPlayer.Location = new System.Drawing.Point(681, 9);
            this.lblCurrentPlayer.Name = "lblCurrentPlayer";
            this.lblCurrentPlayer.Size = new System.Drawing.Size(53, 13);
            this.lblCurrentPlayer.TabIndex = 1;
            this.lblCurrentPlayer.Text = "Turno de:";
            // 
            // lblNarrator
            // 
            this.lblNarrator.AutoSize = true;
            this.lblNarrator.Location = new System.Drawing.Point(470, 428);
            this.lblNarrator.Name = "lblNarrator";
            this.lblNarrator.Size = new System.Drawing.Size(48, 13);
            this.lblNarrator.TabIndex = 2;
            this.lblNarrator.Text = "Narrador";
            // 
            // lblDadoNum
            // 
            this.lblDadoNum.AutoSize = true;
            this.lblDadoNum.Location = new System.Drawing.Point(681, 89);
            this.lblDadoNum.Name = "lblDadoNum";
            this.lblDadoNum.Size = new System.Drawing.Size(94, 13);
            this.lblDadoNum.TabIndex = 3;
            this.lblDadoNum.Text = "Jugador x sacó un";
            // 
            // LudoGame
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.lblDadoNum);
            this.Controls.Add(this.lblNarrator);
            this.Controls.Add(this.lblCurrentPlayer);
            this.Controls.Add(this.btnRollDice);
            this.Name = "LudoGame";
            this.Text = "LudoGame";
            this.Load += new System.EventHandler(this.LudoGame_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnRollDice;
        private System.Windows.Forms.Label lblCurrentPlayer;
        private System.Windows.Forms.Label lblNarrator;
        private System.Windows.Forms.Label lblDadoNum;
    }
}