namespace TestFirmaXades
{
    partial class FrmSeleccionarFirma
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
            this.label1 = new System.Windows.Forms.Label();
            this.lstFirmas = new System.Windows.Forms.ListBox();
            this.btnAceptar = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Listado de firmas";
            // 
            // lstFirmas
            // 
            this.lstFirmas.FormattingEnabled = true;
            this.lstFirmas.HorizontalScrollbar = true;
            this.lstFirmas.Location = new System.Drawing.Point(24, 47);
            this.lstFirmas.Name = "lstFirmas";
            this.lstFirmas.ScrollAlwaysVisible = true;
            this.lstFirmas.Size = new System.Drawing.Size(473, 121);
            this.lstFirmas.TabIndex = 1;
            // 
            // btnAceptar
            // 
            this.btnAceptar.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnAceptar.Location = new System.Drawing.Point(422, 184);
            this.btnAceptar.Name = "btnAceptar";
            this.btnAceptar.Size = new System.Drawing.Size(75, 23);
            this.btnAceptar.TabIndex = 2;
            this.btnAceptar.Text = "Aceptar";
            this.btnAceptar.UseVisualStyleBackColor = true;
            // 
            // FrmSeleccionarFirma
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(522, 219);
            this.Controls.Add(this.btnAceptar);
            this.Controls.Add(this.lstFirmas);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "FrmSeleccionarFirma";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Seleccionar firma";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox lstFirmas;
        private System.Windows.Forms.Button btnAceptar;
    }
}