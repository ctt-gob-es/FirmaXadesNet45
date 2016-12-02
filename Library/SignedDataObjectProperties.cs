// SignedDataObjectProperties.cs
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
	/// The SignedDataObjectProperties element contains properties that qualify
	/// some of the signed data objects
	/// </summary>
	public class SignedDataObjectProperties
	{
		#region Private variables
		private DataObjectFormatCollection dataObjectFormatCollection;
		private CommitmentTypeIndicationCollection commitmentTypeIndicationCollection;
		private AllDataObjectsTimeStampCollection allDataObjectsTimeStampCollection;
		private IndividualDataObjectsTimeStampCollection individualDataObjectsTimeStampCollection;
		#endregion

		#region Public properties
		/// <summary>
		/// Collection of signed data object formats
		/// </summary>
		public DataObjectFormatCollection DataObjectFormatCollection
		{
			get
			{
				return this.dataObjectFormatCollection;
			}
			set
			{
				this.dataObjectFormatCollection = value;
			}
		}

		/// <summary>
		/// Collection of commitment type indications
		/// </summary>
		public CommitmentTypeIndicationCollection CommitmentTypeIndicationCollection
		{
			get
			{
				return this.commitmentTypeIndicationCollection;
			}
			set
			{
				this.commitmentTypeIndicationCollection = value;
			}
		}

		/// <summary>
		/// Collection of all data object timestamps
		/// </summary>
		public AllDataObjectsTimeStampCollection AllDataObjectsTimeStampCollection
		{
			get
			{
				return this.allDataObjectsTimeStampCollection;
			}
			set
			{
				this.allDataObjectsTimeStampCollection = value;
			}
		}

		/// <summary>
		/// Collection of individual data object timestamps
		/// </summary>
		public IndividualDataObjectsTimeStampCollection IndividualDataObjectsTimeStampCollection
		{
			get
			{
				return this.individualDataObjectsTimeStampCollection;
			}
			set
			{
				this.individualDataObjectsTimeStampCollection = value;
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Default constructor
		/// </summary>
		public SignedDataObjectProperties()
		{
			this.dataObjectFormatCollection = new DataObjectFormatCollection();
			this.commitmentTypeIndicationCollection = new CommitmentTypeIndicationCollection();
			this.allDataObjectsTimeStampCollection = new AllDataObjectsTimeStampCollection();
			this.individualDataObjectsTimeStampCollection = new IndividualDataObjectsTimeStampCollection();
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

			if (this.dataObjectFormatCollection.Count > 0)
			{
				retVal = true;
			}

			if (this.commitmentTypeIndicationCollection.Count > 0)
			{
				retVal = true;
			}

			if (this.allDataObjectsTimeStampCollection.Count > 0)
			{
				retVal = true;
			}

			if (this.individualDataObjectsTimeStampCollection.Count > 0)
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
			IEnumerator enumerator;
			XmlElement iterationXmlElement;
			DataObjectFormat newDataObjectFormat;
			CommitmentTypeIndication newCommitmentTypeIndication;
			TimeStamp newTimeStamp;
			
			if (xmlElement == null)
			{
				throw new ArgumentNullException("xmlElement");
			}

			xmlNamespaceManager = new XmlNamespaceManager(xmlElement.OwnerDocument.NameTable);
			xmlNamespaceManager.AddNamespace("xsd", XadesSignedXml.XadesNamespaceUri);

			this.dataObjectFormatCollection.Clear();
			xmlNodeList = xmlElement.SelectNodes("xsd:DataObjectFormat", xmlNamespaceManager);
			enumerator = xmlNodeList.GetEnumerator();
			try 
			{
				while (enumerator.MoveNext()) 
				{
					iterationXmlElement = enumerator.Current as XmlElement;
					if (iterationXmlElement != null)
					{
						newDataObjectFormat = new DataObjectFormat();
						newDataObjectFormat.LoadXml(iterationXmlElement);
						this.dataObjectFormatCollection.Add(newDataObjectFormat);
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

			//this.dataObjectFormatCollection.Clear();
			xmlNodeList = xmlElement.SelectNodes("xsd:CommitmentTypeIndication", xmlNamespaceManager);
			enumerator = xmlNodeList.GetEnumerator();
			try 
			{
				while (enumerator.MoveNext()) 
				{
					iterationXmlElement = enumerator.Current as XmlElement;
					if (iterationXmlElement != null)
					{
						newCommitmentTypeIndication = new CommitmentTypeIndication();
						newCommitmentTypeIndication.LoadXml(iterationXmlElement);
						this.commitmentTypeIndicationCollection.Add(newCommitmentTypeIndication);
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

			//this.dataObjectFormatCollection.Clear();
			xmlNodeList = xmlElement.SelectNodes("xsd:AllDataObjectsTimeStamp", xmlNamespaceManager);
			enumerator = xmlNodeList.GetEnumerator();
			try 
			{
				while (enumerator.MoveNext()) 
				{
					iterationXmlElement = enumerator.Current as XmlElement;
					if (iterationXmlElement != null)
					{
						newTimeStamp = new TimeStamp("AllDataObjectsTimeStamp");
						newTimeStamp.LoadXml(iterationXmlElement);
						this.allDataObjectsTimeStampCollection.Add(newTimeStamp);
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

			//this.dataObjectFormatCollection.Clear();
			xmlNodeList = xmlElement.SelectNodes("xsd:IndividualDataObjectsTimeStamp", xmlNamespaceManager);
			enumerator = xmlNodeList.GetEnumerator();
			try 
			{
				while (enumerator.MoveNext()) 
				{
					iterationXmlElement = enumerator.Current as XmlElement;
					if (iterationXmlElement != null)
					{
						newTimeStamp = new TimeStamp("IndividualDataObjectsTimeStamp");
						newTimeStamp.LoadXml(iterationXmlElement);
						this.individualDataObjectsTimeStampCollection.Add(newTimeStamp);
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
			retVal = creationXmlDocument.CreateElement(XadesSignedXml.XmlXadesPrefix, "SignedDataObjectProperties", XadesSignedXml.XadesNamespaceUri);

			if (this.dataObjectFormatCollection.Count > 0)
			{
				foreach (DataObjectFormat dataObjectFormat in this.dataObjectFormatCollection)
				{
					if (dataObjectFormat.HasChanged())
					{
						retVal.AppendChild(creationXmlDocument.ImportNode(dataObjectFormat.GetXml(), true));
					}
				}
			}

			if (this.commitmentTypeIndicationCollection.Count > 0)
			{
				foreach (CommitmentTypeIndication commitmentTypeIndication in this.commitmentTypeIndicationCollection)
				{
					if (commitmentTypeIndication.HasChanged())
					{
						retVal.AppendChild(creationXmlDocument.ImportNode(commitmentTypeIndication.GetXml(), true));
					}
				}
			}

			if (this.allDataObjectsTimeStampCollection.Count > 0)
			{
				foreach (TimeStamp timeStamp in this.allDataObjectsTimeStampCollection)
				{
					if (timeStamp.HasChanged())
					{
						retVal.AppendChild(creationXmlDocument.ImportNode(timeStamp.GetXml(), true));
					}
				}
			}

			if (this.individualDataObjectsTimeStampCollection.Count > 0)
			{
				foreach (TimeStamp timeStamp in this.individualDataObjectsTimeStampCollection)
				{
					if (timeStamp.HasChanged())
					{
						retVal.AppendChild(creationXmlDocument.ImportNode(timeStamp.GetXml(), true));
					}
				}
			}

			return retVal;
		}
		#endregion
	}
}
