// --------------------------------------------------------------------------------------------------------------------
// FrmPrincipal.cs
//
// FirmaXadesNet - Demo de generación de fichero factura-e
// Copyright (C) 2016 Dpto. de Nuevas Tecnologías de la Dirección General de Urbanismo del Ayto. de Cartagena
//
// This program is free software: you can redistribute it and/or modify
// it under the +terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see http://www.gnu.org/licenses/. 
//
// E-Mail: informatica@gemuc.es
// 
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FirmaXadesNet;
using System.IO;

namespace DemoFacturae
{
    public partial class FrmPrincipal : Form
    {
        public FrmPrincipal()
        {
            InitializeComponent();
        }

        private void btnGenerar_Click(object sender, EventArgs e)
        {
            FirmaXades firmaXades = new FirmaXades();
            string ficheroFactura = Application.StartupPath + "\\Facturae.xml";

            firmaXades.SetContentEnveloped(ficheroFactura);

            // Política de firma de factura-e 3.1
            firmaXades.PolicyIdentifier = "http://www.facturae.es/politica_de_firma_formato_facturae/politica_de_firma_formato_facturae_v3_1.pdf";
            firmaXades.PolicyHash = "Ohixl6upD6av8N7pEvDABhEL6hM=";

            firmaXades.Sign(firmaXades.SelectCertificate());

            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                firmaXades.Save(saveFileDialog1.FileName);
                MessageBox.Show("Fichero guardado correctamente.");
            }
        }
    }
}
