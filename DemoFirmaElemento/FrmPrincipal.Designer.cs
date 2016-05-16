namespace DemoFirmaElemento
{
    partial class FrmPrincipal
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
            this.btnFirmar = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.SuspendLayout();
            // 
            // btnFirmar
            // 
            this.btnFirmar.Location = new System.Drawing.Point(120, 96);
            this.btnFirmar.Name = "btnFirmar";
            this.btnFirmar.Size = new System.Drawing.Size(163, 23);
            this.btnFirmar.TabIndex = 0;
            this.btnFirmar.Text = "Generar firma";
            this.btnFirmar.UseVisualStyleBackColor = true;
            this.btnFirmar.Click += new System.EventHandler(this.btnFirmar_Click);
            // 
            // FrmPrincipal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(401, 236);
            this.Controls.Add(this.btnFirmar);
            this.Name = "FrmPrincipal";
            this.Text = "Demo Firma Elemento";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnFirmar;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
    }
}

