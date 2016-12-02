// Cert.cs
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
	/// This class contains certificate identification information
	/// </summary>
	public class Cert
	{
		#region Private variables
		private DigestAlgAndValueType certDigest;
		private IssuerSerial issuerSerial;
		#endregion

		#region Public properties
		/// <summary>
		/// The element CertDigest contains the digest of one of the
		/// certificates referenced in the sequence
		/// </summary>
		public DigestAlgAndValueType CertDigest
		{
			get
			{
				return this.certDigest;
			}
			set
			{
				this.certDigest = value;
			}
		}

		/// <summary>
		/// The element IssuerSerial contains the identifier of one of the
		/// certificates referenced in the sequence. Should the
		/// X509IssuerSerial element appear in the signature to denote the same
		/// certificate, its value MUST be consistent with the corresponding
		/// IssuerSerial element.
		/// </summary>
		public IssuerSerial IssuerSerial
		{
			get
			{
				return this.issuerSerial;
			}
			set
			{
				this.issuerSerial = value;
			}
		}

        /// <summary>
        /// Element's URI
        /// </summary>
        public string URI { get; set; }

		#endregion

		#region Constructors
		/// <summary>
		/// Default constructor
		/// </summary>
		public Cert()
		{
			this.certDigest = new DigestAlgAndValueType("CertDigest");
			this.issuerSerial = new IssuerSerial();
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

			if (this.certDigest != null && this.certDigest.HasChanged())
			{
				retVal = true;
			}

			if (this.issuerSerial != null && this.issuerSerial.HasChanged())
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

            if (xmlElement.HasAttribute("URI"))
            {
                this.URI = xmlElement.GetAttribute("URI");
            }

			xmlNamespaceManager = new XmlNamespaceManager(xmlElement.OwnerDocument.NameTable);
			xmlNamespaceManager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
			xmlNamespaceManager.AddNamespace("xsd", XadesSignedXml.XadesNamespaceUri);

			xmlNodeList = xmlElement.SelectNodes("xsd:CertDigest", xmlNamespaceManager);
			if (xmlNodeList.Count == 0)
			{
				throw new CryptographicException("CertDigest missing");
			}
			this.certDigest = new DigestAlgAndValueType("CertDigest");
			this.certDigest.LoadXml((XmlElement)xmlNodeList.Item(0));

			xmlNodeList = xmlElement.SelectNodes("xsd:IssuerSerial", xmlNamespaceManager);
			if (xmlNodeList.Count == 0)
			{
				throw new CryptographicException("IssuerSerial missing");
			}
			this.issuerSerial = new IssuerSerial();
			this.issuerSerial.LoadXml((XmlElement)xmlNodeList.Item(0));
		}

		/// <summary>
		/// Returns the XML representation of the this object
		/// </summary>
		/// <returns>XML element containing the state of this object</returns>
		public XmlElement GetXml()
		{
			XmlDocument creationXmlDocument;
			XmlElement retVal;

			creationXmlDocument = new XmlDocument();
			retVal = creationXmlDocument.CreateElement(XadesSignedXml.XmlXadesPrefix, "Cert", XadesSignedXml.XadesNamespaceUri);
            retVal.SetAttribute("xmlns:ds", SignedXml.XmlDsigNamespaceUrl);

            if (!string.IsNullOrEmpty(URI))
            {
                retVal.SetAttribute("URI", URI);
            }

			if (this.certDigest != null && this.certDigest.HasChanged())
			{
				retVal.AppendChild(creationXmlDocument.ImportNode(this.certDigest.GetXml(), true));
			}

			if (this.issuerSerial != null && this.issuerSerial.HasChanged())
			{
				retVal.AppendChild(creationXmlDocument.ImportNode(this.issuerSerial.GetXml(), true));
			}

			return retVal;
		}
		#endregion
	}
}