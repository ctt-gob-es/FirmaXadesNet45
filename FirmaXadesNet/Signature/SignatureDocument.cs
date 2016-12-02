// --------------------------------------------------------------------------------------------------------------------
// SignatureDocument.cs
//
// FirmaXadesNet - Librería para la generación de firmas XADES
// Copyright (C) 2016 Dpto. de Nuevas Tecnologías de la Dirección General de Urbanismo del Ayto. de Cartagena
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

using FirmaXadesNet.Utils;
using Microsoft.Xades;
using System;
using System.IO;
using System.Security.Cryptography.Xml;
using System.Text;
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

        public byte[] GetDocumentBytes()
        {
            CheckSignatureDocument(this);

            using (MemoryStream ms = new MemoryStream())
            {
                Save(ms);

                return ms.ToArray();
            }
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
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = new UTF8Encoding();
            using (var writer = XmlWriter.Create(output, settings))
            {
                this.Document.Save(writer);
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Actualiza el documento resultante
        /// </summary>
        internal void UpdateDocument()
        {
            if (_document == null)
            {
                _document = new XmlDocument();
            }

            if (_document.DocumentElement != null)
            {
                XmlNode xmlNode = _document.SelectSingleNode("//*[@Id='" + _xadesSignedXml.Signature.Id + "']");

                if (xmlNode != null)
                {

                    XmlNamespaceManager nm = new XmlNamespaceManager(_document.NameTable);
                    nm.AddNamespace("xades", XadesSignedXml.XadesNamespaceUri);
                    nm.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);

                    XmlNode xmlQPNode = xmlNode.SelectSingleNode("ds:Object/xades:QualifyingProperties", nm);
                    XmlNode xmlUnsingedPropertiesNode = xmlNode.SelectSingleNode("ds:Object/xades:QualifyingProperties/xades:UnsignedProperties", nm);

                    if (xmlUnsingedPropertiesNode != null)
                    {
                        xmlUnsingedPropertiesNode.InnerXml = _xadesSignedXml.XadesObject.QualifyingProperties.UnsignedProperties.GetXml().InnerXml;
                    }
                    else
                    {
                        xmlUnsingedPropertiesNode = _document.ImportNode(_xadesSignedXml.XadesObject.QualifyingProperties.UnsignedProperties.GetXml(), true);
                        xmlQPNode.AppendChild(xmlUnsingedPropertiesNode);
                    }

                }
                else
                {
                    XmlElement xmlSigned = _xadesSignedXml.GetXml();

                    byte[] canonicalizedElement = XMLUtil.ApplyTransform(xmlSigned, new XmlDsigC14NTransform());

                    XmlDocument doc = new XmlDocument();
                    doc.PreserveWhitespace = true;
                    doc.LoadXml(Encoding.UTF8.GetString(canonicalizedElement));

                    XmlNode canonSignature = _document.ImportNode(doc.DocumentElement, true);

                    _xadesSignedXml.GetSignatureElement().AppendChild(canonSignature);
                }
            }
            else
            {
                _document.LoadXml(_xadesSignedXml.GetXml().OuterXml);
            }
        }


        internal static void CheckSignatureDocument(SignatureDocument sigDocument)
        {
            if (sigDocument == null)
            {
                throw new ArgumentNullException("sigDocument");
            }
            
            if (sigDocument.Document == null || sigDocument.XadesSignature == null)
            {
                throw new Exception("No existe información sobre la firma");
            }
        }

        #endregion
    }
}
