// --------------------------------------------------------------------------------------------------------------------
// SignatureDocument.cs
//
// FirmaXadesNet - Librería para la generación de firmas XADES
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

using Microsoft.Xades;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FirmaXadesNet.Signature
{
    public class SignatureDocument
    {
        #region Private variables        

        private XadesSignedXml _xadesSignedXml;
        private XmlDocument _document;
        
        #endregion

        #region Public properties

        public XmlDocument Document
        {
            get
            {
                return _document;
            }

            set
            {
                _document = value;
            }
        }

        public XadesSignedXml XadesSignature
        {
            get
            {
                return _xadesSignedXml;
            }

            set
            {
                _xadesSignedXml = value;
            }
        }
        
        #endregion

        #region Public methods

        public X509Certificate2 GetSigningCertificate()
        {
            CheckSignatureDocument(this);
            
            XmlNode keyXml = _xadesSignedXml.KeyInfo.GetXml().GetElementsByTagName("X509Certificate", SignedXml.XmlDsigNamespaceUrl)[0];

            if (keyXml == null)
            {
                throw new Exception("No se ha podido obtener el certificado de firma");
            }

            return new X509Certificate2(Convert.FromBase64String(keyXml.InnerText));
        }

        /// <summary>
        /// Guardar la firma en el fichero especificado.
        /// </summary>
        /// <param name="fileName"></param>
        public void Save(string fileName)
        {
            CheckSignatureDocument(this);

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = new UTF8Encoding();
            using (var writer = XmlWriter.Create(fileName, settings))
            {
                this.Document.Save(writer);
            }
        }

        /// <summary>
        /// Guarda la firma en el destino especificado
        /// </summary>
        /// <param name="output"></param>
        public void Save(Stream output)
        {
            this.Document.Save(output);
        }

        #endregion

        #region Private methods

        internal static void CheckSignatureDocument(SignatureDocument sigDocument)
        {
            if (sigDocument == null)
            {
                throw new Exception("Se necesita un documento de firma válido");
            }
            
            if (sigDocument.Document == null || sigDocument.XadesSignature == null)
            {
                throw new Exception("No existe información sobre la firma");
            }
        }

        #endregion
    }
}
