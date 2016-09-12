﻿// --------------------------------------------------------------------------------------------------------------------
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
    public class CertUtil
    {
        #region Public methods

        public static X509Chain GetCertChain(X509Certificate2 certificate, X509Certificate2[] certificates = null)
        {
            var chain = new X509Chain
            {
                ChainPolicy =
                {
                    RevocationMode = X509RevocationMode.NoCheck,
                    VerificationFlags = X509VerificationFlags.IgnoreWrongUsage
                }
            };


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

        /// <summary>
        /// Selecciona un certificado del almacén de certificados
        /// </summary>
        /// <returns></returns>
        public static X509Certificate2 SelectCertificate(string message = null, string title = null)
        {
            X509Certificate2 cert = null;

            try
            {
                // Open the store of personal certificates.
                var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

                var collection = store.Certificates;
                var fcollection = collection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);

                if (string.IsNullOrEmpty(message))
                {
                    message = "Seleccione un certificado.";
                }

                if (string.IsNullOrEmpty(title))
                {
                    title = "Firmar archivo";
                }

                var scollection = X509Certificate2UI.SelectFromCollection(fcollection, title, message, X509SelectionFlag.SingleSelection);

                if (scollection != null && scollection.Count == 1)
                {
                    cert = scollection[0];

                    if (cert.HasPrivateKey == false)
                    {
                        throw new ArgumentException("El certificado no tiene asociada una clave privada.", nameof(cert.PrivateKey));
                    }
                }

                store.Close();
            }
            catch (Exception exc)
            {
                throw new Exception("No se ha podido obtener la clave privada.", exc);
            }

            return cert;
        }


        #endregion
    }
}
