// IssuerSerial.cs
//
// XAdES Starter Kit for Microsoft .NET 3.5 (and above)
// 2010 Microsoft France
//
// Originally published under the CECILL-B Free Software license agreement,
// modified by Dpto. de Nuevas Tecnologías de la Dirección General de Urbanismo del Ayto. de Cartagena
// and published under the GNU Lesser General Public License version 3.
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

using System;
using System.Xml;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;

namespace Microsoft.Xades
{
	/// <summary>
	/// The element IssuerSerial contains the identifier of one of the
	/// certificates referenced in the sequence
	/// </summary>
	public class IssuerSerial
	{
		#region Private variables
		private string x509IssuerName;
		private string x509SerialNumber;
		#endregion

		#region Public properties
		/// <summary>
		/// Name of the X509 certificate issuer
		/// </summary>
		public string X509IssuerName
		{
			get
			{
				return this.x509IssuerName;
			}
			set
			{
				this.x509IssuerName = value;
			}
		}

		/// <summary>
		/// Serial number of the X509 certificate
		/// </summary>
		public string X509SerialNumber
		{
			get
			{
				return this.x509SerialNumber;
			}
			set
			{
				this.x509SerialNumber = value;
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Default constructor
		/// </summary>
		public IssuerSerial()
		{
		}
		#endregion

		#region Public methods
		/// <summary>
		/// Check to see if something has changed in this instance and needs to be serialized
		/// </summary>
		/// <returns>Flag indicating if a member needs serialization</returns>
		public bool HasChanged()
		{
			bool retVal = false;

			if (!String.IsNullOrEmpty(this.x509IssuerName))
			{
				retVal = true;
			}

			if (!String.IsNullOrEmpty(this.x509SerialNumber))
			{
				retVal = true;
			}

			return retVal;
		}

		/// <summary>
		/// Load state from an XML element
		/// </summary>
		/// <param name="xmlElement">XML element containing new state</param>
		public void LoadXml(System.Xml.XmlElement xmlElement)
		{
			XmlNamespaceManager xmlNamespaceManager;
			XmlNodeList xmlNodeList;
			
			if (xmlElement == null)
			{
				throw new ArgumentNullException("xmlElement");
			}

			xmlNamespaceManager = new XmlNamespaceManager(xmlElement.OwnerDocument.NameTable);
			xmlNamespaceManager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);

			xmlNodeList = xmlElement.SelectNodes("ds:X509IssuerName", xmlNamespaceManager);
			if (xmlNodeList.Count == 0)
			{
				throw new CryptographicException("X509IssuerName missing");
			}
			this.x509IssuerName = xmlNodeList.Item(0).InnerText;

			xmlNodeList = xmlElement.SelectNodes("ds:X509SerialNumber", xmlNamespaceManager);
			if (xmlNodeList.Count == 0)
			{
				throw new CryptographicException("X509SerialNumber missing");
			}
			this.x509SerialNumber = xmlNodeList.Item(0).InnerText;
		}

		/// <summary>
		/// Returns the XML representation of the this object
		/// </summary>
		/// <returns>XML element containing the state of this object</returns>
		public XmlElement GetXml()
		{
			XmlDocument creationXmlDocument;
			XmlElement retVal;
			XmlElement bufferXmlElement;

			creationXmlDocument = new XmlDocument();
			retVal = creationXmlDocument.CreateElement(XadesSignedXml.XmlXadesPrefix, "IssuerSerial", XadesSignedXml.XadesNamespaceUri);
            retVal.SetAttribute("xmlns:ds", SignedXml.XmlDsigNamespaceUrl);

			bufferXmlElement = creationXmlDocument.CreateElement(XadesSignedXml.XmlDSigPrefix, "X509IssuerName", SignedXml.XmlDsigNamespaceUrl);
            bufferXmlElement.SetAttribute("xmlns:xades", XadesSignedXml.XadesNamespaceUri);
			bufferXmlElement.InnerText = this.x509IssuerName;
			retVal.AppendChild(bufferXmlElement);

			bufferXmlElement = creationXmlDocument.CreateElement(XadesSignedXml.XmlDSigPrefix, "X509SerialNumber", SignedXml.XmlDsigNamespaceUrl);
            bufferXmlElement.SetAttribute("xmlns:xades", XadesSignedXml.XadesNamespaceUri);
			bufferXmlElement.InnerText = this.x509SerialNumber;

            retVal.AppendChild(bufferXmlElement);

			return retVal;
		}
		#endregion
	}
}