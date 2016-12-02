// --------------------------------------------------------------------------------------------------------------------
// CryptoConst.cs
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

namespace FirmaXadesNet.Crypto
{
    class CryptoConst
    {
        public const string MS_DEF_PROV = "Microsoft Base Cryptographic Provider v1.0";
        public const string MS_ENHANCED_PROV = "Microsoft Enhanced Cryptographic Provider v1.0";
        public const string MS_STRONG_PROV = "Microsoft Strong Cryptographic Provider";
        public const string MS_DEF_RSA_SIG_PROV = "Microsoft RSA Signature Cryptographic Provider";
        public const string MS_DEF_RSA_SCHANNEL_PROV = "Microsoft RSA SChannel Cryptographic Provider";
        public const string MS_DEF_DSS_PROV = "Microsoft Base DSS Cryptographic Provider";
        public const string MS_DEF_DSS_DH_PROV = "Microsoft Base DSS and Diffie-Hellman Cryptographic Provider";
        public const string MS_ENH_DSS_DH_PROV = "Microsoft Enhanced DSS and Diffie-Hellman Cryptographic Provider";
        public const string MS_DEF_DH_SCHANNEL_PROV = "Microsoft DH SChannel Cryptographic Provider";
        public const string MS_SCARD_PROV = "Microsoft Base Smart Card Crypto Provider";
        public const string MS_ENH_RSA_AES_PROV = "Microsoft Enhanced RSA and AES Cryptographic Provider";

        public const int PROV_RSA_FULL = 1;
        public const int PROV_RSA_SIG = 2;
        public const int PROV_DSS = 3;
        public const int PROV_FORTEZZA = 4;
        public const int PROV_MS_EXCHANGE = 5;
        public const int PROV_SSL = 6;
        public const int PROV_RSA_SCHANNEL = 12;
        public const int PROV_DSS_DH = 13;
        public const int PROV_EC_ECDSA_SIG = 14;
        public const int PROV_EC_ECNRA_SIG = 15;
        public const int PROV_EC_ECDSA_FULL = 16;
        public const int PROV_EC_ECNRA_FULL = 17;
        public const int PROV_DH_SCHANNEL = 18;
        public const int PROV_SPYRUS_LYNKS = 20;
        public const int PROV_RNG = 21;
        public const int PROV_INTEL_SEC = 22;
        public const int PROV_REPLACE_OWF = 23;
        public const int PROV_RSA_AES = 24;
    }
}
