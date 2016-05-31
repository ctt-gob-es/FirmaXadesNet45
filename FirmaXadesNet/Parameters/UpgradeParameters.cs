// --------------------------------------------------------------------------------------------------------------------
// UpgradeParameters.cs
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

using FirmaXadesNet.Clients;
using FirmaXadesNet.Crypto;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirmaXadesNet.Parameters
{
    public class UpgradeParameters
    {
        private List<string> _ocspServers;

        private List<X509Crl> _crls;
        
        private DigestMethod _digestMethod;

        private TimeStampClient _timeStampClient;

        public List<string> OCSPServers
        {
            get
            {
                return _ocspServers;
            }
        }

        public List<X509Crl> CRL
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


        public UpgradeParameters()
        {
            _ocspServers = new List<string>();
            _crls = new List<X509Crl>();
            _digestMethod = DigestMethod.SHA1;
        }

        public void AddCRL(Stream stream)
        {
            Org.BouncyCastle.X509.X509CrlParser parser = new Org.BouncyCastle.X509.X509CrlParser();
            var x509crl = parser.ReadCrl(stream);

            _crls.Add(x509crl);
        }
    }
}
