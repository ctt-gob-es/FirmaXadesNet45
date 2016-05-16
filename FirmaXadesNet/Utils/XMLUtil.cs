// --------------------------------------------------------------------------------------------------------------------
// XMLUtil.cs
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Security.Cryptography.Xml;
using System.Collections;
using Microsoft.Xades;
using System.Security.Cryptography;

namespace FirmaXadesNet.Utils
{
    class XMLUtil
    {
        #region Public methods

        /// <summary>
        /// Aplica una transformación al elemento especificado
        /// </summary>
        /// <param name="element"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static byte[] ApplyTransform(XmlElement element, System.Security.Cryptography.Xml.Transform transform)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(element.OuterXml);

            using (MemoryStream ms = new MemoryStream(buffer))
            {
                transform.LoadInput(ms);
                using (MemoryStream transformedStream = (MemoryStream)transform.GetOutput(typeof(Stream)))
                {
                    return transformedStream.ToArray();
                }
            }
        }

        /// <summary>
        /// Obtiene el valor canonicalizado de los elementos especificados en elementXpaths
        /// </summary>
        /// <param name="xadesSignedXml"></param>
        /// <param name="elementXpaths"></param>
        /// <returns></returns>
        public static byte[] ComputeValueOfElementList(XadesSignedXml xadesSignedXml, ArrayList elementXpaths)
        {
            XmlDocument xmlDocument;
            XmlNamespaceManager xmlNamespaceManager;
            XmlNodeList searchXmlNodeList;

            var signatureXmlElement = xadesSignedXml.GetSignatureElement();
            var namespaces = xadesSignedXml.GetAllNamespaces(signatureXmlElement);

            xmlDocument = signatureXmlElement.OwnerDocument;
            xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
            xmlNamespaceManager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
            xmlNamespaceManager.AddNamespace("xades", XadesSignedXml.XadesNamespaceUri);

            using (MemoryStream msResult = new MemoryStream())
            {
                foreach (string elementXpath in elementXpaths)
                {
                    searchXmlNodeList = signatureXmlElement.SelectNodes(elementXpath, xmlNamespaceManager);

                    if (searchXmlNodeList.Count == 0)
                    {
                        throw new CryptographicException("Element " + elementXpath + " not found while calculating hash");
                    }

                    foreach (XmlNode xmlNode in searchXmlNodeList)
                    {
                        XmlAttribute dsNamespace = xmlDocument.CreateAttribute("xmlns:" + XadesSignedXml.XmlDSigPrefix);
                        dsNamespace.Value = XadesSignedXml.XmlDsigNamespaceUrl;
                        xmlNode.Attributes.Append(dsNamespace);
                       
                        foreach (var attr in namespaces)
                        {
                            XmlAttribute attrNamespace = xmlDocument.CreateAttribute(attr.Name);
                            attrNamespace.Value = attr.Value;
                            xmlNode.Attributes.Append(attrNamespace);
                        }

                        byte[] canonicalizedElement = ApplyTransform((XmlElement)xmlNode, new XmlDsigC14NTransform());
                        msResult.Write(canonicalizedElement, 0, canonicalizedElement.Length);
                    }
                }

                return msResult.ToArray();
            }
        }

        #endregion
    }
}