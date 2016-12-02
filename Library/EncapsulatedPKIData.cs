// EncapsulatedPKIData.cs
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
	/// EncapsulatedPKIData is used to incorporate a piece of PKI data
	/// into an XML structure whereas the PKI data is encoded using an ASN.1
	/// encoding mechanism. Examples of such PKI data that are widely used at
	/// the time include X509 certificates and revocation lists, OCSP responses,
	/// attribute certificates and time-stamps.
	/// </summary>
	public class EncapsulatedPKIData
	{
		#region Private variables
		private string tagName;
		private string id;
		private byte[] pkiData;
		#endregion
			
		#region Public properties
		/// <summary>
		/// The name of the element when serializing
		/// </summary>
		public string TagName
		{
			get
			{
				return this.tagName;
			}
			set
			{
				this.tagName = value;
			}
		}

		/// <summary>
		/// The optional ID attribute can be used to make a reference to an element
		/// of this data type.
		/// </summary>
		public string Id
		{
			get
			{
				return this.id;
			}
			set
			{
				this.id = value;
			}
		}

		/// <summary>
		/// Base64 encoded content of this data type 
		/// </summary>
		public byte[] PkiData
		{
			get
			{
				return this.pkiData;
			}
			set
			{
				this.pkiData = value;
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Default constructor
		/// </summary>
		public EncapsulatedPKIData()
		{
		}

		/// <summary>
		/// Constructor with TagName
		/// </summary>
		/// <param name="tagName">Name of the tag when serializing with GetXml</param>
		public EncapsulatedPKIData(string tagName)
		{
			this.tagName = tagName;
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

            if (!String.IsNullOrEmpty(this.id))
			{
				retVal = true;
			}

			if (this.pkiData != null && this.pkiData.Length > 0)
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
			if (xmlElement == null)
			{
				throw new ArgumentNullException("xmlElement");
			}

			if (xmlElement.HasAttribute("Id"))
			{
				this.id = xmlElement.GetAttribute("Id");
			}
			else
			{
				this.id = "";
			}

			this.pkiData = Convert.FromBase64String(xmlElement.InnerText);
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
			retVal = creationXmlDocument.CreateElement(XadesSignedXml.XmlXadesPrefix, this.tagName, XadesSignedXml.XadesNamespaceUri);
            retVal.SetAttribute("xmlns:ds", SignedXml.XmlDsigNamespaceUrl);
            retVal.SetAttribute("Encoding", "http://uri.etsi.org/01903/v1.2.2#DER");

            if (!String.IsNullOrEmpty(this.id))
			{
				retVal.SetAttribute("Id", this.id);
			}

			if (this.pkiData != null && this.pkiData.Length > 0)
			{
				retVal.InnerText = Convert.ToBase64String(this.pkiData);
			}

			return retVal;
		}
		#endregion
	}
}
