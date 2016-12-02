// --------------------------------------------------------------------------------------------------------------------
// SignatureMethod.cs
//
// FirmaXadesNet - Librería para la generación de firmas XADES
// Copyright (C) 2016 Dpto. de Nuevas Tecnologías de la Dirección General de Urbanismo del Ayto. de Cartagena
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
//
// E-Mail: informatica@gemuc.es
// 
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirmaXadesNet.Crypto
{
    public class SignatureMethod
    {
        #region Private variables

        private string _name;
        private string _uri;

        #endregion

        #region Public properties

        public static SignatureMethod RSAwithSHA1 = new SignatureMethod("RSAwithSHA1", "http://www.w3.org/2000/09/xmldsig#rsa-sha1");
        public static SignatureMethod RSAwithSHA256 = new SignatureMethod("RSAwithSHA256", "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256");
        public static SignatureMethod RSAwithSHA512 = new SignatureMethod("RSAwithSHA512", "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512");        

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

        #endregion

        #region Constructors

        private SignatureMethod(string name, string uri)
        {
            _name = name;
            _uri = uri;
        }

        #endregion
    }

}
