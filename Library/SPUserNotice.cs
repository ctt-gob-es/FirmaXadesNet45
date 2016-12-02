// SPUserNotice.cs
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
	/// SPUserNotice element is intended for being displayed whenever the
	/// signature is validated.  The class derives from SigPolicyQualifier.
	/// </summary>
	public class SPUserNotice : SigPolicyQualifier
	{
		#region Private variables
		private NoticeRef noticeRef;
		private string explicitText;
		#endregion

		#region Public properties
		/// <summary>
		/// The NoticeRef element names an organization and identifies by
		/// numbers a group of textual statements prepared by that organization,
		/// so that the application could get the explicit notices from a notices file.
		/// </summary>
		public NoticeRef NoticeRef
		{
			get
			{
				return this.noticeRef;
			}
			set
			{
				this.noticeRef = value;
			}
		}

		/// <summary>
		/// The	ExplicitText element contains the text of the notice to be displayed
		/// </summary>
		public string ExplicitText
		{
			get
			{
				return this.explicitText;
			}
			set
			{
				this.explicitText = value;
			}
		}

		/// <summary>
		/// Inherited generic element, not used in the SPUserNotice class
		/// </summary>
		public override XmlElement AnyXmlElement
		{
			get
			{
				return null; //This does not make sense for SPUserNotice
			}
			set
			{
				throw new CryptographicException("Setting AnyXmlElement on a SPUserNotice is not supported");
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Default constructor
		/// </summary>
		public SPUserNotice()
		{
			noticeRef = new NoticeRef();
		}
		#endregion

		#region Public methods
		/// <summary>
		/// Check to see if something has changed in this instance and needs to be serialized
		/// </summary>
		/// <returns>Flag indicating if a member needs serialization</returns>
		public override bool HasChanged()
		{
			bool retVal = false;

			if (!String.IsNullOrEmpty(this.explicitText))
			{
				retVal = true;
			}

			if (this.noticeRef != null && this.noticeRef.HasChanged())
			{
				retVal = true;
			}

			return retVal;
		}

		/// <summary>
		/// Load state from an XML element
		/// </summary>
		/// <param name="xmlElement">XML element containing new state</param>
		public override void LoadXml(System.Xml.XmlElement xmlElement)
		{
			XmlNamespaceManager xmlNamespaceManager;
			XmlNodeList xmlNodeList;

			if (xmlElement == null)
			{
				throw new ArgumentNullException("xmlElement");
			}

			xmlNamespaceManager = new XmlNamespaceManager(xmlElement.OwnerDocument.NameTable);
			xmlNamespaceManager.AddNamespace("xsd", XadesSignedXml.XadesNamespaceUri);

			xmlNodeList = xmlElement.SelectNodes("xsd:SPUserNotice/xsd:NoticeRef", xmlNamespaceManager);
			if (xmlNodeList.Count != 0)
			{
				this.noticeRef = new NoticeRef();
				this.noticeRef.LoadXml((XmlElement)xmlNodeList.Item(0));
			}

			xmlNodeList = xmlElement.SelectNodes("xsd:SPUserNotice/xsd:ExplicitText", xmlNamespaceManager);
			if (xmlNodeList.Count != 0)
			{
				this.explicitText = xmlNodeList.Item(0).InnerText;
			}
		}

		/// <summary>
		/// Returns the XML representation of the this object
		/// </summary>
		/// <returns>XML element containing the state of this object</returns>
		public override XmlElement GetXml()
		{
			XmlDocument creationXmlDocument;
			XmlElement bufferXmlElement;
			XmlElement bufferXmlElement2;
			XmlElement retVal;

			creationXmlDocument = new XmlDocument();
			retVal = creationXmlDocument.CreateElement("SigPolicyQualifier", XadesSignedXml.XadesNamespaceUri);

			bufferXmlElement = creationXmlDocument.CreateElement("SPUserNotice", XadesSignedXml.XadesNamespaceUri);
			if (this.noticeRef != null && this.noticeRef.HasChanged())
			{
				bufferXmlElement.AppendChild(creationXmlDocument.ImportNode(this.noticeRef.GetXml(), true));
			}
			if (!String.IsNullOrEmpty(this.explicitText))
			{
				bufferXmlElement2 = creationXmlDocument.CreateElement("ExplicitText", XadesSignedXml.XadesNamespaceUri);
				bufferXmlElement2.InnerText = this.explicitText;
				bufferXmlElement.AppendChild(bufferXmlElement2);
			}

			retVal.AppendChild(creationXmlDocument.ImportNode(bufferXmlElement, true));

			return retVal;
		}
		#endregion
	}
}
