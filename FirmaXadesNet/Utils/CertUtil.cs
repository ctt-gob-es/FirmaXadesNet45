// --------------------------------------------------------------------------------------------------------------------
// CertUtil.cs
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

using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace FirmaXadesNet.Utils
{
    class CertUtil
    {
        #region Public methods

        public static X509Chain GetCertChain(X509Certificate2 certificate, X509Certificate2[] certificates = null)
        {
            X509Chain chain = new X509Chain();

            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.IgnoreWrongUsage;

            if (certificates != null)
            {
                chain.ChainPolicy.ExtraStore.AddRange(certificates);
            }

            if (!chain.Build(certificate))
            {
                throw new Exception("No se puede construir la cadena de certificación");
            }

            return chain;
        }

        public static Org.BouncyCastle.X509.X509Certificate ConvertToX509Certificate(X509Certificate2 cert)
        {
            return Org.BouncyCastle.Security.DotNetUtilities.FromX509Certificate(cert);
        }

        public static string HexToDecimal(string hex)
        {
            List<int> dec = new List<int> { 0 };

            foreach (char c in hex)
            {
                int carry = Convert.ToInt32(c.ToString(), 16);

                for (int i = 0; i < dec.Count; ++i)
                {
                    int val = dec[i] * 16 + carry;
                    dec[i] = val % 10;
                    carry = val / 10;
                }

                while (carry > 0)
                {
                    dec.Add(carry % 10);
                    carry /= 10;
                }
            }

            var chars = dec.Select(d => (char)('0' + d));
            var cArr = chars.Reverse().ToArray();
            return new string(cArr);
        }

        #endregion
    }
}
