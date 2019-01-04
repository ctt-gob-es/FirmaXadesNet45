using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Microsoft.Xades
{
    public class Manifest
    {
        #region Private variables
        private string id;
        private ReferenceCollection referenceCollection;
        #endregion

        #region Public properties
        public string Id
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
            }
        }

        public ReferenceCollection ReferenceCollection
        {
            get
            {
                return this.referenceCollection;
            }

            set
            {
                this.referenceCollection = value;
            }
        }
        #endregion

        #region Constructors
        public Manifest()
        {
            this.referenceCollection = new ReferenceCollection();
        }

        public Manifest(string id) : this()
        {
            this.id = id;
        }
        #endregion

        #region Public methods
        public XmlElement GetXml()
        {
            XmlDocument creationXmlDocument;
            XmlElement retVal;

            creationXmlDocument = new XmlDocument();
            retVal = creationXmlDocument.CreateElement("Manifest", XadesSignedXml.XmlDsigNamespaceUrl);
            if (!string.IsNullOrEmpty(this.id))
            {
                retVal.SetAttribute("Id", this.id);
            }

            foreach (Reference reference in this.referenceCollection)
            {
                retVal.AppendChild(creationXmlDocument.ImportNode(reference.GetXml(), true));
            }

            return retVal;
        }
        #endregion
    }
}
