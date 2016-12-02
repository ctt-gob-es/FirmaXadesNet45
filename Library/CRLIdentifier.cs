// CRLIdentifier.cs
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

namespace Microsoft.Xades
{
	/// <summary>
	/// This class includes the issuer (Issuer element), the time when the CRL
	/// was issued (IssueTime element) and optionally the number of the CRL
	/// (Number element).
	/// The Identifier element can be dropped if the CRL could be inferred from
	/// other information. Its URI attribute could serve to	indicate where the
	/// identified CRL is archived.
	/// </summary>
	public class CRLIdentifier
	{
		#region Private variables
		private string uriAttribute;
		private string issuer;
		private DateTime issueTime;
		private long number;
		#endregion

		#region Public properties
		/// <summary>
		/// The optional URI attribute could serve to indicate where the OCSP
		/// response identified is archived.
		/// </summary>
		public string UriAttribute
		{
			get
			{
				return this.uriAttribute;
			}
			set
			{
				this.uriAttribute = value;
			}
		}

		/// <summary>
		/// Issuer of the CRL
		/// </summary>
		public string Issuer
		{
			get
			{
				return this.issuer;
			}
			set
			{
				this.issuer = value;
			}
		}

		/// <summary>
		/// Date of issue of the CRL
		/// </summary>
		public DateTime IssueTime
		{
			get
			{
				return this.issueTime;
			}
			set
			{
				this.issueTime = value;
			}
		}

		/// <summary>
		/// Optional number of the CRL
		/// </summary>
		public long Number
		{
			get
			{
				return this.number;
			}
			set
			{
				this.number = value;
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Default constructor
		/// </summary>
		public CRLIdentifier()
		{
			this.issueTime = DateTime.MinValue;
			this.number = long.MinValue; //Impossible value
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

			if (!String.IsNullOrEmpty(this.uriAttribute))
			{
				retVal = true;
			}

			if (!String.IsNullOrEmpty(this.issuer))
			{
				retVal = true;
			}

			if (this.issueTime != DateTime.MinValue)
			{
				retVal = true;
			}

			if (this.number != long.MinValue)
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
				this.uriAttribute = xmlElement.GetAttribute("URI");
			}

			xmlNamespaceManager = new XmlNamespaceManager(xmlElement.OwnerDocument.NameTable);
			xmlNamespaceManager.AddNamespace("xsd", XadesSignedXml.XadesNamespaceUri);

			xmlNodeList = xmlElement.SelectNodes("xsd:Issuer", xmlNamespaceManager);
			if (xmlNodeList.Count != 0)
			{
				this.issuer = xmlNodeList.Item(0).InnerText;
			}

			xmlNodeList = xmlElement.SelectNodes("xsd:IssueTime", xmlNamespaceManager);
			if (xmlNodeList.Count != 0)
			{
                this.issueTime = XmlConvert.ToDateTime(xmlNodeList.Item(0).InnerText, XmlDateTimeSerializationMode.Local);
			}

			xmlNodeList = xmlElement.SelectNodes("xsd:Number", xmlNamespaceManager);
			if (xmlNodeList.Count != 0)
			{
				this.number = long.Parse(xmlNodeList.Item(0).InnerText);
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
			retVal = creationXmlDocument.CreateElement(XadesSignedXml.XmlXadesPrefix, "CRLIdentifier", XadesSignedXml.XadesNamespaceUri);

			retVal.SetAttribute("URI", this.uriAttribute);

			if (!String.IsNullOrEmpty(this.issuer))
			{
				bufferXmlElement = creationXmlDocument.CreateElement(XadesSignedXml.XmlXadesPrefix, "Issuer", XadesSignedXml.XadesNamespaceUri);
				bufferXmlElement.InnerText = this.issuer;
				retVal.AppendChild(bufferXmlElement);
			}

			if (this.issueTime != DateTime.MinValue)
			{
				bufferXmlElement = creationXmlDocument.CreateElement(XadesSignedXml.XmlXadesPrefix, "IssueTime", XadesSignedXml.XadesNamespaceUri);

                DateTime truncatedDateTime = this.issueTime.AddTicks(-(this.issueTime.Ticks % TimeSpan.TicksPerSecond));

                bufferXmlElement.InnerText = XmlConvert.ToString(truncatedDateTime, XmlDateTimeSerializationMode.Local);        

				retVal.AppendChild(bufferXmlElement);
			}

			if (this.number != long.MinValue)
			{
				bufferXmlElement = creationXmlDocument.CreateElement(XadesSignedXml.XmlXadesPrefix, "Number", XadesSignedXml.XadesNamespaceUri);
				bufferXmlElement.InnerText = this.number.ToString();
				retVal.AppendChild(bufferXmlElement);
			}

			return retVal;
		}
		#endregion
	}
}
