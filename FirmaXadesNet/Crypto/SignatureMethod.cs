using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirmaXadesNet.Crypto
{
    public class SignatureMethod
    {
        public static SignatureMethod RSAwithSHA1 = new SignatureMethod("RSAwithSHA1", "http://www.w3.org/2000/09/xmldsig#rsa-sha1");
        public static SignatureMethod RSAwithSHA256 = new SignatureMethod("RSAwithSHA256", "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256");
        public static SignatureMethod RSAwithSHA512 = new SignatureMethod("RSAwithSHA512", "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512");

        private string _name;
        private string _uri;

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public string URI
        {
            get
            {
                return _uri;
            }
        }

        private SignatureMethod(string name, string uri)
        {
            _name = name;
            _uri = uri;
        }
    }

}
