// --------------------------------------------------------------------------------------------------------------------
// SignatureCommitmentType.cs
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
// along with this program.  If not, see https://www.gnu.org/licenses/lgpl-3.0.txt. 
//
// E-Mail: informatica@gemuc.es
// 
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirmaXadesNet.Signature.Parameters
{
    public class SignatureCommitmentType
    {
        #region Private variables

        private string _uri;

        #endregion

        #region Public properties

        public static SignatureCommitmentType ProofOfOrigin = new SignatureCommitmentType("http://uri.etsi.org/01903/v1.2.2#ProofOfOrigin");
        public static SignatureCommitmentType ProofOfReceipt = new SignatureCommitmentType("http://uri.etsi.org/01903/v1.2.2#ProofOfReceipt");
        public static SignatureCommitmentType ProofOfDelivery = new SignatureCommitmentType("http://uri.etsi.org/01903/v1.2.2#ProofOfDelivery");
        public static SignatureCommitmentType ProofOfSender = new SignatureCommitmentType("http://uri.etsi.org/01903/v1.2.2#ProofOfSender");
        public static SignatureCommitmentType ProofOfApproval = new SignatureCommitmentType("http://uri.etsi.org/01903/v1.2.2#ProofOfApproval");
        public static SignatureCommitmentType ProofOfCreation = new SignatureCommitmentType("http://uri.etsi.org/01903/v1.2.2#ProofOfCreation");

        public string URI
        {
            get
            {
                return _uri;
            }
        }

        #endregion

        #region Constructors

        public SignatureCommitmentType(string uri)
        {
            _uri = uri;
        }

        #endregion
    }
}
