// CertifiedRoles.cs
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
using System.Collections;
using System.Xml;

namespace Microsoft.Xades
{
	/// <summary>
	/// The CertifiedRoles element contains one or more wrapped attribute
	/// certificates for the signer
	/// </summary>
	public class CertifiedRoles
	{
		#region Private variables
		private CertifiedRoleCollection certifiedRoleCollection;
		#endregion

		#region Public properties
		/// <summary>
		/// Collection of certified roles
		/// </summary>
		public CertifiedRoleCollection CertifiedRoleCollection
		{
			get
			{
				return this.certifiedRoleCollection;
			}
			set
			{
				this.certifiedRoleCollection = value;
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Default constructor
		/// </summary>
		public CertifiedRoles()
		{
			this.certifiedRoleCollection = new CertifiedRoleCollection();
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

			if (this.certifiedRoleCollection.Count > 0)
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
			EncapsulatedPKIData newCertifiedRole;
			IEnumerator enumerator;
			XmlElement iterationXmlElement;
			
			if (xmlElement == null)
			{
				throw new ArgumentNullException("xmlElement");
			}

			xmlNamespaceManager = new XmlNamespaceManager(xmlElement.OwnerDocument.NameTable);
			xmlNamespaceManager.AddNamespace("xsd", XadesSignedXml.XadesNamespaceUri);

			this.certifiedRoleCollection.Clear();
			xmlNodeList = xmlElement.SelectNodes("xsd:CertifiedRole", xmlNamespaceManager);
			enumerator = xmlNodeList.GetEnumerator();
			try 
			{
				while (enumerator.MoveNext()) 
				{
					iterationXmlElement = enumerator.Current as XmlElement;
					if (iterationXmlElement != null)
					{
						newCertifiedRole = new EncapsulatedPKIData("CertifiedRole");
						newCertifiedRole.LoadXml(iterationXmlElement);
						this.certifiedRoleCollection.Add(newCertifiedRole);
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
			retVal = creationXmlDocument.CreateElement(XadesSignedXml.XmlXadesPrefix, "CertifiedRoles", XadesSignedXml.XadesNamespaceUri);

			if (this.certifiedRoleCollection.Count > 0)
			{
				foreach (EncapsulatedPKIData certifiedRole in this.certifiedRoleCollection)
				{
					if (certifiedRole.HasChanged())
					{
						retVal.AppendChild(creationXmlDocument.ImportNode(certifiedRole.GetXml(), true));
					}
				}
			}

			return retVal;
		}
		#endregion
	}
}
