// --------------------------------------------------------------------------------------------------------------------
// FrmSeleccionarFirma.cs
//
// FirmaXadesNet - Librería la para generación de firmas XADES
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

using FirmaXadesNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestFirmaXades
{
    public partial class FrmSeleccionarFirma : Form
    {
        private FirmaXades[] _firmas = null;
        
        public FirmaXades FirmaSeleccionada
        {
            get
            {
                return _firmas[lstFirmas.SelectedIndex];
            }
        }
        
        public FrmSeleccionarFirma(FirmaXades[] firmas)
        {
            InitializeComponent();

            _firmas = firmas;

            foreach (var firma in firmas)
            {
                string textoFirma = string.Format("{0} - {1}",
                    firma.XadesSignature.XadesObject.QualifyingProperties.SignedProperties.SignedSignatureProperties.SigningTime,
                    firma.Certificate.Subject);

                lstFirmas.Items.Add(textoFirma);
            }

            lstFirmas.SelectedIndex = 0;
        }
    }
}