// UnsignedProperties.cs
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

namespace Microsoft.Xades
{
	/// <summary>
	/// The UnsignedProperties element contains a number of properties that are
	/// not signed by the XMLDSIG signature
	/// </summary>
	public class UnsignedProperties
	{
		#region Private variables
		private string id;
		private UnsignedSignatureProperties unsignedSignatureProperties;
		private UnsignedDataObjectProperties unsignedDataObjectProperties;
		#endregion

		#region Public properties
		/// <summary>
		/// The optional Id attribute can be used to make a reference to the
		/// UnsignedProperties element
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
		/// UnsignedSignatureProperties may contain properties that qualify XML
		/// signature itself or the signer
		/// </summary>
		public UnsignedSignatureProperties UnsignedSignatureProperties
		{
			get
			{
				return this.unsignedSignatureProperties;
			}
			set
			{
				this.unsignedSignatureProperties = value;
			}
		}

		/// <summary>
		/// The UnsignedDataObjectProperties element may contain properties that
		/// qualify some of the signed data objects
		/// </summary>
		public UnsignedDataObjectProperties UnsignedDataObjectProperties
		{
			get
			{
				return this.unsignedDataObjectProperties;
			}
			set
			{
				this.unsignedDataObjectProperties = value;
			}
		}
		#endregion
		
		#region Constructors
		/// <summary>
		/// Default constructor
		/// </summary>
		public UnsignedProperties()
		{
			this.UnsignedSignatureProperties = new UnsignedSignatureProperties();
			this.unsignedDataObjectProperties = new UnsignedDataObjectProperties();
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

			if (this.unsignedSignatureProperties != null && this.unsignedSignatureProperties.HasChanged())
			{
				retVal = true;
			}

			if (this.unsignedDataObjectProperties != null && this.unsignedDataObjectProperties.HasChanged())
			{
				retVal = true;
			}

			return retVal;
		}

		/// <summary>
		/// Load state from an XML element
		/// </summary>
		/// <param name="xmlElement">XML element containing new state</param>
		/// <param name="counterSignedXmlElement">Element containing parent signature (needed if there are counter signatures)</param>
		public void LoadXml(System.Xml.XmlElement xmlElement, XmlElement counterSignedXmlElement)
		{
			XmlNamespaceManager xmlNamespaceManager;
			XmlNodeList xmlNodeList;
			
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

			xmlNamespaceManager = new XmlNamespaceManager(xmlElement.OwnerDocument.NameTable);
			xmlNamespaceManager.AddNamespace("xsd", XadesSignedXml.XadesNamespaceUri);

			xmlNodeList = xmlElement.SelectNodes("xsd:UnsignedSignatureProperties", xmlNamespaceManager);
			if (xmlNodeList.Count != 0)
			{
				this.unsignedSignatureProperties = new UnsignedSignatureProperties();
				this.unsignedSignatureProperties.LoadXml((XmlElement)xmlNodeList.Item(0), counterSignedXmlElement);
			}

			xmlNodeList = xmlElement.SelectNodes("xsd:UnsignedDataObjectProperties", xmlNamespaceManager);
			if (xmlNodeList.Count != 0)
			{
				this.unsignedDataObjectProperties = new UnsignedDataObjectProperties();
				this.unsignedDataObjectProperties.LoadXml((XmlElement)xmlNodeList.Item(0));
			}
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
			//retVal = creationXmlDocument.CreateElement("UnsignedProperties", XadesSignedXml.XadesNamespaceUri);
            retVal = creationXmlDocument.CreateElement(XadesSignedXml.XmlXadesPrefix, "UnsignedProperties", "http://uri.etsi.org/01903/v1.3.2#");
            if (!String.IsNullOrEmpty(this.id))
			{
				retVal.SetAttribute("Id", this.id);
			}

			if (this.unsignedSignatureProperties != null && this.unsignedSignatureProperties.HasChanged())
			{
				retVal.AppendChild(creationXmlDocument.ImportNode(this.unsignedSignatureProperties.GetXml(), true));
			}
			if (this.unsignedDataObjectProperties != null && this.unsignedDataObjectProperties.HasChanged())
			{
				retVal.AppendChild(creationXmlDocument.ImportNode(this.unsignedDataObjectProperties.GetXml(), true));
			}

			return retVal;
		}
		#endregion
	}
}
