// DocumentationReference.cs
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
	/// DocumentationReference points to further explanatory documentation
	/// of the object identifier
	/// </summary>
	public class DocumentationReference
	{
		#region Private variables
		private string documentationReferenceUri;
		#endregion

		#region Public properties
		/// <summary>
		/// Pointer to further explanatory documentation of the object identifier
		/// </summary>
		public string DocumentationReferenceUri
		{
			get
			{
				return this.documentationReferenceUri;
			}
			set
			{
				this.documentationReferenceUri = value;
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Default constructor
		/// </summary>
		public DocumentationReference()
		{
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

			if (!String.IsNullOrEmpty(this.documentationReferenceUri))
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

			this.documentationReferenceUri = xmlElement.InnerText;
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
			retVal = creationXmlDocument.CreateElement("DocumentationReference", XadesSignedXml.XadesNamespaceUri);
			retVal.InnerText = this.documentationReferenceUri;

			return retVal;
		}
		#endregion
	}
}
