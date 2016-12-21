// --------------------------------------------------------------------------------------------------------------------
// UpgradeParameters.cs
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

using FirmaXadesNet.Clients;
using FirmaXadesNet.Crypto;
using Org.BouncyCastle.X509;
using System.Collections.Generic;
using System.IO;

namespace FirmaXadesNet.Upgraders.Parameters
{
    public class UpgradeParameters
    {
        #region Private variables

        private List<OcspServer> _ocspServers;

        private List<X509Crl> _crls;

        private DigestMethod _digestMethod;

        private TimeStampClient _timeStampClient;

        private X509CrlParser _crlParser;

        private DigestMethod _defaultDigestMethod = DigestMethod.SHA1;

        private bool _getOcspUrlFromCertificate;

        #endregion

        #region Public properties

        public List<OcspServer> OCSPServers
        {
            get
            {
                return _ocspServers;
            }
        }

        public IEnumerable<X509Crl> CRL
        {
            get
            {
                return _crls;
            }
        }

        public DigestMethod DigestMethod
        {
            get
            {
                return _digestMethod;
            }

            set
            {
                _digestMethod = value;
            }
        }

        public TimeStampClient TimeStampClient
        {
            get
            {
                return _timeStampClient;
            }

            set
            {
                _timeStampClient = value;
            }
        }

        public bool GetOcspUrlFromCertificate
        {
            get
            {
                return _getOcspUrlFromCertificate;
            }

            set
            {
                _getOcspUrlFromCertificate = value;
            }
        }

        #endregion

        #region Constructors

        public UpgradeParameters()
        {
            _ocspServers = new List<OcspServer>();
            _crls = new List<X509Crl>();
            _digestMethod = _defaultDigestMethod;
            _crlParser = new X509CrlParser();
            _getOcspUrlFromCertificate = true;
        }

        #endregion

        #region Public methods

        public void AddCRL(Stream stream)
        {
            var x509crl = _crlParser.ReadCrl(stream);

            _crls.Add(x509crl);
        }

        public void ClearCRL()
        {
            _crls.Clear();
        }

        #endregion
    }
}
