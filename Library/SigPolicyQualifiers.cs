// SigPolicyQualifiers.cs
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
using System.Collections;

namespace Microsoft.Xades
{
	/// <summary>
	/// This class contains a collection of SigPolicyQualifiers
	/// </summary>
	public class SigPolicyQualifiers
	{
		#region Private variables
		private SigPolicyQualifierCollection sigPolicyQualifierCollection;
		#endregion

		#region Public properties
		/// <summary>
		/// A collection of sig policy qualifiers
		/// </summary>
		public SigPolicyQualifierCollection SigPolicyQualifierCollection
		{
			get
			{
				return this.sigPolicyQualifierCollection;
			}
			set
			{
				this.sigPolicyQualifierCollection = value;
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Default constructor
		/// </summary>
		public SigPolicyQualifiers()
		{
			this.sigPolicyQualifierCollection = new SigPolicyQualifierCollection();
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

			if (this.sigPolicyQualifierCollection.Count > 0)
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
			SPUri newSPUri;
			SPUserNotice newSPUserNotice;
			SigPolicyQualifier newSigPolicyQualifier;
			IEnumerator enumerator;
			XmlElement iterationXmlElement;
			XmlElement subElement;
			
			if (xmlElement == null)
			{
				throw new ArgumentNullException("xmlElement");
			}

			xmlNamespaceManager = new XmlNamespaceManager(xmlElement.OwnerDocument.NameTable);
			xmlNamespaceManager.AddNamespace("xsd", XadesSignedXml.XadesNamespaceUri);

			this.sigPolicyQualifierCollection.Clear();
			xmlNodeList = xmlElement.SelectNodes("xsd:SigPolicyQualifier", xmlNamespaceManager);
			enumerator = xmlNodeList.GetEnumerator();
			try 
			{
				while (enumerator.MoveNext()) 
				{
					iterationXmlElement = enumerator.Current as XmlElement;
					if (iterationXmlElement != null)
					{
						subElement = (XmlElement)iterationXmlElement.SelectSingleNode("xsd:SPURI", xmlNamespaceManager);
						if (subElement != null)
						{
							newSPUri = new SPUri();
							newSPUri.LoadXml(iterationXmlElement);
							this.sigPolicyQualifierCollection.Add(newSPUri);
						}
						else
						{
							subElement = (XmlElement)iterationXmlElement.SelectSingleNode("xsd:SPUserNotice", xmlNamespaceManager);
							if (subElement != null)
							{
								newSPUserNotice = new SPUserNotice();
								newSPUserNotice.LoadXml(iterationXmlElement);
								this.sigPolicyQualifierCollection.Add(newSPUserNotice);
							}
							else
							{
								newSigPolicyQualifier = new SigPolicyQualifier();
								newSigPolicyQualifier.LoadXml(iterationXmlElement);
								this.sigPolicyQualifierCollection.Add(newSigPolicyQualifier);
							}
						}
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
			retVal = creationXmlDocument.CreateElement(XadesSignedXml.XmlXadesPrefix, "SigPolicyQualifiers", XadesSignedXml.XadesNamespaceUri);

			if (this.sigPolicyQualifierCollection.Count > 0)
			{
				foreach (SigPolicyQualifier sigPolicyQualifier in this.sigPolicyQualifierCollection)
				{
					if (sigPolicyQualifier.HasChanged())
					{
						retVal.AppendChild(creationXmlDocument.ImportNode(sigPolicyQualifier.GetXml(), true));
					}
				}
			}

			return retVal;
		}
		#endregion
	}
}
