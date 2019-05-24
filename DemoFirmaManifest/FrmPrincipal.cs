// --------------------------------------------------------------------------------------------------------------------
// FrmPrincipal.cs
//
// FirmaXadesNet - Demo de generación de firma internally detached especificando el nodo de destino de la firma
// Copyright (C) 2019 Dpto. de Nuevas Tecnologías de la Dirección General de Urbanismo del Ayto. de Cartagena
//
// This program is free software: you can redistribute it and/or modify
// it under the +terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with this program.  If not, see http://www.gnu.org/licenses/. 
//
// E-Mail: informatica@gemuc.es
// 
// --------------------------------------------------------------------------------------------------------------------

using FirmaXadesNet;
using FirmaXadesNet.Crypto;
using FirmaXadesNet.Signature;
using FirmaXadesNet.Signature.Parameters;
using FirmaXadesNet.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace DemoFirmaManifest
{
    public partial class FrmPrincipal : Form
    {
        public FrmPrincipal()
        {
            InitializeComponent();
        }

        private void btnFirmar_Click(object sender, EventArgs e)
        {
            XadesService xadesService = new XadesService();
            SignatureParameters parametros = new SignatureParameters();
            byte[] hashValue = Convert.FromBase64String("sbsJSZJb4Qb2oea6uENdJCAgc+Q=");

            List<Reference> referencias = new List<Reference>()
            {
                new Reference("xsdBOE-A-2011-13169_ex_XAdES_Internally_detached.xml")
                {
                    DigestMethod = DigestMethod.SHA1.URI,
                    Id = "Ref-1",
                    DigestValue = hashValue
                }
            };

            XmlElement manifestXmlElement = ManifestUtil.CreateManifest("Manifest-1", referencias);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.AppendChild(xmlDoc.ImportNode(manifestXmlElement, true));

            parametros.SignaturePackaging = SignaturePackaging.ENVELOPING;
            parametros.DataFormat = new DataFormat();
            parametros.DataFormat.MimeType = "text/xml";
            parametros.SignaturePolicyInfo = new SignaturePolicyInfo
            {
                PolicyIdentifier = "rn:oid:2.16.724.1.3.1.1.2.1.8",
                PolicyDigestAlgorithm = DigestMethod.SHA1,
                PolicyHash = "VYICYpNOjso9g1mBiXDVxNORpKk=",
                PolicyUri = "http://administracionelectronica.gob.es/es/ctt/politicafirma/politica_firma_AGE_v1_8.pdf"
            };

            SignatureDocument documentoFirma;

            using (parametros.Signer = new Signer(FirmaXadesNet.Utils.CertUtil.SelectCertificate()))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    xmlDoc.Save(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    documentoFirma = xadesService.Sign(ms, parametros);
                }
            }

            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (FileStream fs = new FileStream(saveFileDialog1.FileName, FileMode.Create))
                {
                    documentoFirma.Save(fs);
                }

                MessageBox.Show("Fichero guardado correctamente.");
            }

        }
    }
}
