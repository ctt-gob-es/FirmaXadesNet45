// DataObjectFormat.cs
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
	/// The DataObjectFormat element provides information that describes the
	/// format of the signed data object. This element must be present when it
	/// is mandatory to present the signed data object to human users on
	/// verification.
	/// This is a signed property that qualifies one specific signed data
	/// object. In consequence, a XAdES signature may contain more than one
	/// DataObjectFormat elements, each one qualifying one signed data object.
	/// </summary>
	public class DataObjectFormat
	{
		#region Private variables
		private string objectReferenceAttribute;
		private string description;
		private ObjectIdentifier objectIdentifier;
		private string mimeType;
		private string encoding;
		#endregion

		#region Public properties
		/// <summary>
		/// The mandatory ObjectReference attribute refers to the Reference element
		/// of the signature corresponding with the data object qualified by this
		/// property.
		/// </summary>
		public string ObjectReferenceAttribute
		{
			get
			{
				return this.objectReferenceAttribute;
			}
			set
			{
				this.objectReferenceAttribute = value;
			}
		}

		/// <summary>
		/// Textual information related to the signed data object
		/// </summary>
		public string Description
		{
			get
			{
				return this.description;
			}
			set
			{
				this.description = value;
			}
		}

		/// <summary>
		/// An identifier indicating the type of the signed data object
		/// </summary>
		public ObjectIdentifier ObjectIdentifier
		{
			get
			{
				return this.objectIdentifier;
			}
			set
			{
				this.objectIdentifier = value;
			}
		}

		/// <summary>
		/// An indication of the MIME type of the signed data object
		/// </summary>
		public string MimeType
		{
			get
			{
				return this.mimeType;
			}
			set
			{
				this.mimeType = value;
			}
		}

		/// <summary>
		/// An indication of the encoding format of the signed data object
		/// </summary>
		public string Encoding
		{
			get
			{
				return this.encoding;
			}
			set
			{
				this.encoding = value;
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Default constructor
		/// </summary>
		public DataObjectFormat()
		{
			this.objectIdentifier = new ObjectIdentifier("ObjectIdentifier");
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

			if (!String.IsNullOrEmpty(this.objectReferenceAttribute))
			{
				retVal = true;
			}

			if (!String.IsNullOrEmpty(this.description))
			{
				retVal = true;
			}

			if (this.objectIdentifier != null && this.objectIdentifier.HasChanged())
			{
				retVal = true;
			}

			if (!String.IsNullOrEmpty(this.mimeType))
			{
				retVal = true;
			}

			if (!String.IsNullOrEmpty(this.encoding))
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

			if (xmlElement.HasAttribute("ObjectReference"))
			{
				this.objectReferenceAttribute = xmlElement.GetAttribute("ObjectReference");
			}
			else
			{
				this.objectReferenceAttribute = "";
				throw new CryptographicException("ObjectReference attribute missing");
			}

			xmlNamespaceManager = new XmlNamespaceManager(xmlElement.OwnerDocument.NameTable);
			xmlNamespaceManager.AddNamespace("xsd", XadesSignedXml.XadesNamespaceUri);

			xmlNodeList = xmlElement.SelectNodes("xsd:Description", xmlNamespaceManager);
			if (xmlNodeList.Count != 0)
			{
				this.description = xmlNodeList.Item(0).InnerText;
			}

			xmlNodeList = xmlElement.SelectNodes("xsd:ObjectIdentifier", xmlNamespaceManager);
			if (xmlNodeList.Count != 0)
			{
				this.objectIdentifier = new ObjectIdentifier("ObjectIdentifier");
				this.objectIdentifier.LoadXml((XmlElement)xmlNodeList.Item(0));
			}

			xmlNodeList = xmlElement.SelectNodes("xsd:MimeType", xmlNamespaceManager);
			if (xmlNodeList.Count != 0)
			{
				this.mimeType = xmlNodeList.Item(0).InnerText;
			}

			xmlNodeList = xmlElement.SelectNodes("xsd:Encoding", xmlNamespaceManager);
			if (xmlNodeList.Count != 0)
			{
				this.encoding = xmlNodeList.Item(0).InnerText;
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
			XmlElement bufferXmlElement;

			creationXmlDocument = new XmlDocument();
			retVal = creationXmlDocument.CreateElement(XadesSignedXml.XmlXadesPrefix, "DataObjectFormat", XadesSignedXml.XadesNamespaceUri);

			if ((this.objectReferenceAttribute != null) && ((this.objectReferenceAttribute != "")))
			{
				retVal.SetAttribute("ObjectReference", this.objectReferenceAttribute);
			}
			else
			{
				throw new CryptographicException("Attribute ObjectReference missing");
			}

            if (!String.IsNullOrEmpty(this.description))
			{
				bufferXmlElement = creationXmlDocument.CreateElement(XadesSignedXml.XmlXadesPrefix, "Description", XadesSignedXml.XadesNamespaceUri);
				bufferXmlElement.InnerText = this.description;
				retVal.AppendChild(bufferXmlElement);
			}

			if (this.objectIdentifier != null && this.objectIdentifier.HasChanged())
			{
				retVal.AppendChild(creationXmlDocument.ImportNode(this.objectIdentifier.GetXml(), true));
			}

            if (!String.IsNullOrEmpty(this.mimeType))
			{
				bufferXmlElement = creationXmlDocument.CreateElement(XadesSignedXml.XmlXadesPrefix, "MimeType", XadesSignedXml.XadesNamespaceUri);
				bufferXmlElement.InnerText = this.mimeType;
				retVal.AppendChild(bufferXmlElement);
			}

            if (!String.IsNullOrEmpty(this.encoding))
			{
				bufferXmlElement = creationXmlDocument.CreateElement(XadesSignedXml.XmlXadesPrefix, "Encoding", XadesSignedXml.XadesNamespaceUri);
				bufferXmlElement.InnerText = this.encoding;
				retVal.AppendChild(bufferXmlElement);
			}

			return retVal;
		}
		#endregion
	}
}
