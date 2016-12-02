// NoticeRef.cs
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
	/// The NoticeRef element names an organization and identifies by
	/// numbers a group of textual statements prepared by that organization,
	/// so that the application could get the explicit notices from a notices file.
	/// </summary>
	public class NoticeRef
	{
		#region Private variables
		private string organization;
		private NoticeNumbers noticeNumbers;
		#endregion

		#region Public properties
		/// <summary>
		/// Organization issuing the signature policy
		/// </summary>
		public string Organization
		{
			get
			{
				return this.organization;
			}
			set
			{
				this.organization = value;
			}
		}

		/// <summary>
		/// Numerical identification of textual statements prepared by the organization,
		/// so that the application can get the explicit notices from a notices file.
		/// </summary>
		public NoticeNumbers NoticeNumbers
		{
			get
			{
				return this.noticeNumbers;
			}
			set
			{
				this.noticeNumbers = value;
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Default constructor
		/// </summary>
		public NoticeRef()
		{
			this.noticeNumbers = new NoticeNumbers();
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

			if (!String.IsNullOrEmpty(this.organization))
			{
				retVal = true;
			}

			if (this.noticeNumbers != null && this.noticeNumbers.HasChanged())
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
			xmlNamespaceManager.AddNamespace("xsd", XadesSignedXml.XadesNamespaceUri);

			xmlNodeList = xmlElement.SelectNodes("xsd:Organization", xmlNamespaceManager);
			if (xmlNodeList.Count == 0)
			{
				throw new CryptographicException("Organization missing");
			}
			this.organization = xmlNodeList.Item(0).InnerText;

			xmlNodeList = xmlElement.SelectNodes("xsd:NoticeNumbers", xmlNamespaceManager);
			if (xmlNodeList.Count == 0)
			{
				throw new CryptographicException("NoticeNumbers missing");
			}
			this.noticeNumbers = new NoticeNumbers();
			this.noticeNumbers.LoadXml((XmlElement)xmlNodeList.Item(0));
		}

		/// <summary>
		/// Returns the XML representation of the this object
		/// </summary>
		/// <returns>XML element containing the state of this object</returns>
		public XmlElement GetXml()
		{
			XmlDocument creationXmlDocument;
			XmlElement bufferXmlElement;
			XmlElement retVal;

			creationXmlDocument = new XmlDocument();
			retVal = creationXmlDocument.CreateElement("NoticeRef", XadesSignedXml.XadesNamespaceUri);

			if (this.organization == null)
			{
				throw new CryptographicException("Organization can't be null");
			}
			bufferXmlElement = creationXmlDocument.CreateElement("Organization", XadesSignedXml.XadesNamespaceUri);
			bufferXmlElement.InnerText = this.organization;
			retVal.AppendChild(bufferXmlElement);

			if (this.noticeNumbers == null)
			{
				throw new CryptographicException("NoticeNumbers can't be null");
			}
			retVal.AppendChild(creationXmlDocument.ImportNode(this.noticeNumbers.GetXml(), true));

			return retVal;
		}
		#endregion
	}
}