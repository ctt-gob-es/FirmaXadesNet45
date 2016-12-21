// --------------------------------------------------------------------------------------------------------------------
// OcspServer.cs
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

using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace FirmaXadesNet.Upgraders.Parameters
{
    public class OcspServer
    {
        public const int DirectoryName = 4;
        public const int DnsName = 2;
        public const int EdiPartyName = 5;
        public const int IPAddress = 7;
        public const int OtherName = 0;
        public const int RegisteredID = 8;
        public const int Rfc822Name = 1;
        public const int UniformResourceIdentifier = 6;
        public const int X400Address = 3;

        public string Url { get; set; }

        public GeneralName RequestorName { get; private set; }

        public X509Certificate2 SignCertificate { get; set; }

        public OcspServer(string url)
        {
            this.Url = url;
        }

        public void SetRequestorName(int tag, string name)
        {
            this.RequestorName = new GeneralName(tag, name);
        }
    }
}
