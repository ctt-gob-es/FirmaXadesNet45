// --------------------------------------------------------------------------------------------------------------------
// SignatureParameters.cs
//
// FirmaXadesNet - Librería para la generación de firmas XADES
// Copyright (C) 2016 Dpto. de Nuevas Tecnologías de la Dirección General de Urbanismo del Ayto. de Cartagena
//
// This program is free software: you can redistribute it and/or modify
// it under the +terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see http://www.gnu.org/licenses/. 
//
// E-Mail: informatica@gemuc.es
// 
// --------------------------------------------------------------------------------------------------------------------

using FirmaXadesNet.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace FirmaXadesNet.Parameters
{
   
    public class SignatureParameters
    {
        #region Public properties

        public X509Certificate2 SigningCertificate { get; set; }

        public SignatureMethod SignatureMethod { get; set; }

        public DigestMethod DigestMethod { get; set; }

        public DateTime? SigningDate { get; set; }

        public List<string> XPathTransformations { get; private set; }

        public SignaturePolicyInfo SignaturePolicyInfo { get; set; }

        public SignatureDestination SignatureDestination { get; set; }

        #endregion

        public SignatureParameters()
        {
            this.XPathTransformations = new List<string>();
            this.SignatureMethod = SignatureMethod.RSAwithSHA256;
            this.DigestMethod = DigestMethod.SHA256;
        }
    }
}
