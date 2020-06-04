using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FirmaXadesNet.Utils
{
    public class BouncyKeyInfoData: KeyInfoClause
    {
        private Org.BouncyCastle.X509.X509Certificate _certificate;

        public BouncyKeyInfoData(Org.BouncyCastle.X509.X509Certificate cert)
        {
            _certificate = cert;
        }

        public override XmlElement GetXml()
        {
            XmlDocument xmlDocument = new XmlDocument
            {
                PreserveWhitespace = true
            };

            XmlElement element = xmlDocument.CreateElement("X509Data", "http://www.w3.org/2000/09/xmldsig#");

            XmlElement certificateElement = xmlDocument.CreateElement("X509Certificate", "http://www.w3.org/2000/09/xmldsig#");
            certificateElement.AppendChild(xmlDocument.CreateTextNode(Convert.ToBase64String(_certificate.GetEncoded())));
            element.AppendChild(certificateElement);

            return element;
        }

        public override void LoadXml(XmlElement element)
        {
            throw new NotImplementedException();
        }
    }
}
