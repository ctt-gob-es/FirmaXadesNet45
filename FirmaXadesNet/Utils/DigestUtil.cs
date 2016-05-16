// --------------------------------------------------------------------------------------------------------------------
// DigestUtil.cs
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

using Microsoft.Xades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace FirmaXadesNet.Utils
{
    class DigestUtil
    {
        #region Public methods

        public static HashAlgorithm GetHashAlg(DigestMethod digestMethod)
        {
            if (digestMethod == DigestMethod.SHA1)
            {
                return SHA1.Create();
            }
            else if (digestMethod == DigestMethod.SHA256)
            {
                return SHA256.Create();
            }
            else if (digestMethod == DigestMethod.SHA512)
            {
                return SHA512.Create();
            }
            else
            {
                throw new Exception("Algoritmo no soportado");
            }
        }

        public static HashAlgorithm GetHashAlg(string digestAlgorithm)
        {
            if (digestAlgorithm == FirmaXades.SHA1Uri)
            {
                return SHA1.Create();
            }
            else if (digestAlgorithm == FirmaXades.SHA256Uri)
            {
                return SHA256.Create();
            }
            else if (digestAlgorithm == FirmaXades.SHA512Uri)
            {
                return SHA512.Create();
            }
            else
            {
                throw new Exception("Algoritmo no soportado");
            }
        }

        public static void SetCertDigest(byte[] rawCert, string digestAlgorithm, DigestAlgAndValueType destination)
        {
            using (var hashAlg = GetHashAlg(digestAlgorithm))
            {
                destination.DigestMethod.Algorithm = digestAlgorithm;
                destination.DigestValue = hashAlg.ComputeHash(rawCert);
            }
        }

        public static void SetCertDigest(byte[] rawCert, DigestMethod digestMethod, DigestAlgAndValueType destination)
        {
            string digestAlgorithm = null;
            
            switch (digestMethod)
            {
                case DigestMethod.SHA1:
                    digestAlgorithm = FirmaXades.SHA1Uri;
                    break;

                case DigestMethod.SHA256:
                    digestAlgorithm = FirmaXades.SHA256Uri;
                    break;

                case DigestMethod.SHA512:
                    digestAlgorithm = FirmaXades.SHA512Uri;
                    break;
            }

            SetCertDigest(rawCert, digestAlgorithm, destination);
        }

        public static byte[] ComputeHashValue(byte[] value, DigestMethod digestMethod)
        {
            using (var alg = DigestUtil.GetHashAlg(digestMethod))
            {
                return alg.ComputeHash(value);
            }
        }

        #endregion
    }
}
