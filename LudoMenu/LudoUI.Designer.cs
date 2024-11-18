namespace LudoMenu
{
    partial class LudoUI
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.cmbDirecciones = new System.Windows.Forms.ComboBox();
            this.txtPuertoCliente = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnConectar = new System.Windows.Forms.Button();
            this.txtServidor = new System.Windows.Forms.TextBox();
            this.btnIniciarServidor = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtPuertoServidor = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtIPCliente = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cmbDirecciones
            // 
            this.cmbDirecciones.FormattingEnabled = true;
            this.cmbDirecciones.Location = new System.Drawing.Point(346, 138);
            this.cmbDirecciones.Name = "cmbDirecciones";
            this.cmbDirecciones.Size = new System.Drawing.Size(121, 21);
            this.cmbDirecciones.TabIndex = 0;
            // 
            // txtPuertoCliente
            // 
            this.txtPuertoCliente.Location = new System.Drawing.Point(106, 178);
            this.txtPuertoCliente.Name = "txtPuertoCliente";
            this.txtPuertoCliente.Size = new System.Drawing.Size(75, 20);
            this.txtPuertoCliente.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(103, 162);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 13);
            this.label3.TabIndex = 28;
            this.label3.Text = "Puerto:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(343, 123);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(25, 13);
            this.label1.TabIndex = 27;
            this.label1.Text = "IPs:";
            // 
            // btnConectar
            // 
            this.btnConectar.Location = new System.Drawing.Point(106, 216);
            this.btnConectar.Name = "btnConectar";
            this.btnConectar.Size = new System.Drawing.Size(100, 23);
            this.btnConectar.TabIndex = 29;
            this.btnConectar.Text = "Conectarse";
            this.btnConectar.UseVisualStyleBackColor = true;
            this.btnConectar.Click += new System.EventHandler(this.btnConectar_Click);
            // 
            // txtServidor
            // 
            this.txtServidor.Location = new System.Drawing.Point(601, 377);
            this.txtServidor.Name = "txtServidor";
            this.txtServidor.Size = new System.Drawing.Size(187, 20);
            this.txtServidor.TabIndex = 30;
            this.txtServidor.TextChanged += new System.EventHandler(this.txtServidor_TextChanged);
            // 
            // btnIniciarServidor
            // 
            this.btnIniciarServidor.Location = new System.Drawing.Point(346, 216);
            this.btnIniciarServidor.Name = "btnIniciarServidor";
            this.btnIniciarServidor.Size = new System.Drawing.Size(100, 23);
            this.btnIniciarServidor.TabIndex = 31;
            this.btnIniciarServidor.Text = "Iniciar Servidor";
            this.btnIniciarServidor.UseVisualStyleBackColor = true;
            this.btnIniciarServidor.Click += new System.EventHandler(this.btnIniciarServidor_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(598, 361);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(151, 13);
            this.label2.TabIndex = 32;
            this.label2.Text = "Inicializar Servidor (Verificador)";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // txtPuertoServidor
            // 
            this.txtPuertoServidor.Location = new System.Drawing.Point(346, 178);
            this.txtPuertoServidor.Name = "txtPuertoServidor";
            this.txtPuertoServidor.Size = new System.Drawing.Size(69, 20);
            this.txtPuertoServidor.TabIndex = 33;
            this.txtPuertoServidor.TextChanged += new System.EventHandler(this.txtPuertoServidor_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(343, 162);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 13);
            this.label5.TabIndex = 35;
            this.label5.Text = "Puerto:";
            // 
            // txtIPCliente
            // 
            this.txtIPCliente.Location = new System.Drawing.Point(106, 139);
            this.txtIPCliente.Name = "txtIPCliente";
            this.txtIPCliente.Size = new System.Drawing.Size(100, 20);
            this.txtIPCliente.TabIndex = 36;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(103, 123);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(20, 13);
            this.label4.TabIndex = 37;
            this.label4.Text = "IP:";
            // 
            // LudoUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtIPCliente);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtPuertoServidor);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnIniciarServidor);
            this.Controls.Add(this.txtServidor);
            this.Controls.Add(this.btnConectar);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtPuertoCliente);
            this.Controls.Add(this.cmbDirecciones);
            this.Name = "LudoUI";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.LudoUI_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbDirecciones;
        private System.Windows.Forms.TextBox txtPuertoCliente;
        internal System.Windows.Forms.Label label3;
        internal System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnConectar;
        private System.Windows.Forms.TextBox txtServidor;
        private System.Windows.Forms.Button btnIniciarServidor;
        internal System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtPuertoServidor;
        internal System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtIPCliente;
        internal System.Windows.Forms.Label label4;
    }
}

