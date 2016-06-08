// --------------------------------------------------------------------------------------------------------------------
// Signer.cs
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace FirmaXadesNet.Crypto
{
    public class Signer : IDisposable
    {
        #region Private variables

        private X509Certificate2 _signingCertificate;
        private AsymmetricAlgorithm _signingKey;
        private bool _disposeCryptoProvider;

        #endregion

        #region Public properties

        public X509Certificate2 Certificate
        {
            get
            {
                return _signingCertificate;
            }
        }

        public AsymmetricAlgorithm SigningKey
        {
            get
            {
                return _signingKey;
            }
        }

        #endregion

        #region Constructors

        public Signer(X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException("certificate");
            }
            
            if (!certificate.HasPrivateKey)
            {
                throw new Exception("El certificado no contiene ninguna clave privada");
            }

            _signingCertificate = certificate;

            SetSigningKey(_signingCertificate);
        }

        #endregion

        #region Public methods

        public void Dispose()
        {
            if (_disposeCryptoProvider && _signingKey != null)
            {
                _signingKey.Dispose();
            }
        }

        #endregion

        #region Private methods

        private void SetSigningKey(X509Certificate2 certificate)
        {
            string providerName = "Microsoft Enhanced RSA and AES Cryptographic Provider";
            int providerType = 24;

            var key = (RSACryptoServiceProvider)certificate.PrivateKey;

            if (key.CspKeyContainerInfo.ProviderName == "Microsoft Strong Cryptographic Provider" ||
                key.CspKeyContainerInfo.ProviderName == "Microsoft Enhanced Cryptographic Provider v1.0" ||
                key.CspKeyContainerInfo.ProviderName == "Microsoft Base Cryptographic Provider v1.0")
            {
                Type CspKeyContainerInfo_Type = typeof(CspKeyContainerInfo);

                FieldInfo CspKeyContainerInfo_m_parameters = CspKeyContainerInfo_Type.GetField("m_parameters", BindingFlags.NonPublic | BindingFlags.Instance);
                CspParameters parameters = (CspParameters)CspKeyContainerInfo_m_parameters.GetValue(key.CspKeyContainerInfo);

                var cspparams = new CspParameters(providerType, providerName, key.CspKeyContainerInfo.KeyContainerName);
                cspparams.KeyNumber = parameters.KeyNumber;
                cspparams.Flags = parameters.Flags;
                _signingKey = new RSACryptoServiceProvider(cspparams);

                _disposeCryptoProvider = true;
            }
            else
            {
                _signingKey = key;
                _disposeCryptoProvider = false;
            }
        }

        #endregion
    }
}
