// CRLValues.cs
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
using System.Collections;

namespace Microsoft.Xades
{
	/// <summary>
	/// This class contains a collection of CRL values
	/// </summary>
	public class CRLValues
	{
		#region Private variables
		private CRLValueCollection crlValueCollection;
		#endregion

		#region Public properties
		/// <summary>
		/// Collection of CRLValues
		/// </summary>
		public CRLValueCollection CRLValueCollection
		{
			get
			{
				return this.crlValueCollection;
			}
			set
			{
				this.crlValueCollection = value;
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Default constructor
		/// </summary>
		public CRLValues()
		{
			this.crlValueCollection = new CRLValueCollection();
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

			if (this.crlValueCollection.Count > 0)
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
			CRLValue newCRLValue;
			IEnumerator enumerator;
			XmlElement iterationXmlElement;
			
			if (xmlElement == null)
			{
				throw new ArgumentNullException("xmlElement");
			}

			xmlNamespaceManager = new XmlNamespaceManager(xmlElement.OwnerDocument.NameTable);
			xmlNamespaceManager.AddNamespace("xsd", XadesSignedXml.XadesNamespaceUri);

			this.crlValueCollection.Clear();
			xmlNodeList = xmlElement.SelectNodes("xsd:EncapsulatedCRLValue", xmlNamespaceManager);
			enumerator = xmlNodeList.GetEnumerator();
			try 
			{
				while (enumerator.MoveNext()) 
				{
					iterationXmlElement = enumerator.Current as XmlElement;
					if (iterationXmlElement != null)
					{
						newCRLValue = new CRLValue();
						newCRLValue.LoadXml(iterationXmlElement);
						this.crlValueCollection.Add(newCRLValue);
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
			retVal = creationXmlDocument.CreateElement(XadesSignedXml.XmlXadesPrefix, "CRLValues", XadesSignedXml.XadesNamespaceUri);

			if (this.crlValueCollection.Count > 0)
			{
				foreach (CRLValue crlValue in this.crlValueCollection)
				{
					if (crlValue.HasChanged())
					{
						retVal.AppendChild(creationXmlDocument.ImportNode(crlValue.GetXml(), true));
					}
				}
			}

			return retVal;
		}
		#endregion
	}
}
