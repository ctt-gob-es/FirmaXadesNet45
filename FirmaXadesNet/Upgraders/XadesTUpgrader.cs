// --------------------------------------------------------------------------------------------------------------------
// XadesTUpgrader.cs
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
using FirmaXadesNet.Utils;
using Microsoft.Xades;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirmaXadesNet.Upgraders
{
    class XadesTUpgrader : XadesUpgrader
    {
        #region Constructors

        public XadesTUpgrader(FirmaXades firma)
            : base(firma)
        {

        }

        #endregion

        #region Public methods

        public override void Upgrade()
        {
            TimeStamp signatureTimeStamp;
            ArrayList signatureValueElementXpaths;
            byte[] signatureValueHash;
            UnsignedProperties unsignedProperties = _firma.XadesSignature.UnsignedProperties;

            try
            {
                if (unsignedProperties.UnsignedSignatureProperties.SignatureTimeStampCollection.Count > 0)
                {
                    throw new Exception("La firma ya contiene un sello de tiempo");
                }

                signatureValueElementXpaths = new ArrayList();
                signatureValueElementXpaths.Add("ds:SignatureValue");
                signatureValueHash = DigestUtil.ComputeHashValue(XMLUtil.ComputeValueOfElementList(_firma.XadesSignature, signatureValueElementXpaths), DigestMethod.SHA1);

                byte[] tsa = TimeStampClient.GetTimeStamp(_firma.TSAServer, signatureValueHash, DigestMethod.SHA1, true);

                signatureTimeStamp = new TimeStamp("SignatureTimeStamp");
                signatureTimeStamp.Id = "SignatureTimeStamp-" + _firma.XadesSignature.Signature.Id;
                signatureTimeStamp.EncapsulatedTimeStamp.PkiData = tsa;
                signatureTimeStamp.EncapsulatedTimeStamp.Id = "SignatureTimeStamp-" + Guid.NewGuid().ToString();

                unsignedProperties.UnsignedSignatureProperties.SignatureTimeStampCollection.Add(signatureTimeStamp);

                _firma.XadesSignature.UnsignedProperties = unsignedProperties;

                _firma.UpdateDocument();
            }
            catch (Exception ex)
            {
                throw new Exception("Ha ocurrido un error al insertar el sellado de tiempo.", ex);
            }
        }

        #endregion
    }
}
