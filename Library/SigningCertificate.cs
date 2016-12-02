// SigningCertificate.cs
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
using System.Collections;

namespace Microsoft.Xades
{
	/// <summary>
	/// This class has as purpose to provide the simple substitution of the
	/// certificate. It contains references to certificates and digest values
	/// computed on them
	/// </summary>
	public class SigningCertificate
	{
		#region Private variables
		private CertCollection certCollection;
		#endregion

		#region Public properties
		/// <summary>
		/// A collection of certs
		/// </summary>
		public CertCollection CertCollection
		{
			get
			{
				return this.certCollection;
			}
			set
			{
				this.certCollection = value;
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Default constructor
		/// </summary>
		public SigningCertificate()
		{
			this.certCollection = new CertCollection();
		}
		#endregion

		#region Public methods
		/// <summary>
		/// Check to see if something has changed in this instance and needs to be serialized
		/// </summary>
		/// <returns>Flag indicating if a member needs serialization</returns>
		public bool HasChanged()
		{
            return true; //Should always be considered dirty
		}

		/// <summary>
		/// Load state from an XML element
		/// </summary>
		/// <param name="xmlElement">XML element containing new state</param>
		public void LoadXml(System.Xml.XmlElement xmlElement)
		{
			XmlNamespaceManager xmlNamespaceManager;
			XmlNodeList xmlNodeList;
			IEnumerator enumerator;
			XmlElement iterationXmlElement;
			Cert newCert;
			
			if (xmlElement == null)
			{
				throw new ArgumentNullException("xmlElement");
			}

			xmlNamespaceManager = new XmlNamespaceManager(xmlElement.OwnerDocument.NameTable);
			xmlNamespaceManager.AddNamespace("xsd", XadesSignedXml.XadesNamespaceUri);

			this.certCollection.Clear();
			xmlNodeList = xmlElement.SelectNodes("xsd:Cert", xmlNamespaceManager);
			enumerator = xmlNodeList.GetEnumerator();
			try 
			{
				while (enumerator.MoveNext()) 
				{
					iterationXmlElement = enumerator.Current as XmlElement;
					if (iterationXmlElement != null)
					{
						newCert = new Cert();
						newCert.LoadXml(iterationXmlElement);
						this.certCollection.Add(newCert);
					}
				}
			}
			finally 
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
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
            retVal = creationXmlDocument.CreateElement(XadesSignedXml.XmlXadesPrefix, "SigningCertificate", XadesSignedXml.XadesNamespaceUri);

			if (this.certCollection.Count > 0)
			{
				foreach (Cert cert in this.certCollection)
				{
					if (cert.HasChanged())
					{
						retVal.AppendChild(creationXmlDocument.ImportNode(cert.GetXml(), true));
					}
				}
			}
			else
			{
				throw new CryptographicException("SigningCertificate.Certcollection should have count > 0");
			}

			return retVal;
		}
		#endregion
	}
}
