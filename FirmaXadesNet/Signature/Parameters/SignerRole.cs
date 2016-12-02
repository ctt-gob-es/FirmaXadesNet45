// --------------------------------------------------------------------------------------------------------------------
// SignerRole.cs
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

using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace FirmaXadesNet.Signature.Parameters
{
    public class SignerRole
    {
        #region Private variables

        private List<X509Certificate> _certifiedRoles;
        private List<string> _claimedRoles;

        #endregion


        #region Public properties

        public List<X509Certificate> CertifiedRoles
        {
            get
            {
                return _certifiedRoles;
            }
        }

        public List<string> ClaimedRoles
        {
            get
            {
                return _claimedRoles;
            }
        }

        #endregion

        #region Constructors

        public SignerRole()
        {
            _certifiedRoles = new List<X509Certificate>();
            _claimedRoles = new List<string>();
        }

        #endregion
    }
}
