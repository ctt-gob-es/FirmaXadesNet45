// --------------------------------------------------------------------------------------------------------------------
// DigestUtil.cs
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

using Microsoft.Xades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using FirmaXadesNet.Crypto;

namespace FirmaXadesNet.Utils
{
    class DigestUtil
    {
        #region Public methods

        public static void SetCertDigest(byte[] rawCert, FirmaXadesNet.Crypto.DigestMethod digestMethod, DigestAlgAndValueType destination)
        {
            using (var hashAlg = digestMethod.GetHashAlgorithm())
            {
                destination.DigestMethod.Algorithm = digestMethod.URI;
                destination.DigestValue = hashAlg.ComputeHash(rawCert);
            }
        }

        public static byte[] ComputeHashValue(byte[] value, FirmaXadesNet.Crypto.DigestMethod digestMethod)
        {
            using (var alg = digestMethod.GetHashAlgorithm())
            {
                return alg.ComputeHash(value);
            }
        }

        #endregion
    }
}
