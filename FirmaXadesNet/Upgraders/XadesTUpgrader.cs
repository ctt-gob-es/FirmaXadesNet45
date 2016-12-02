// --------------------------------------------------------------------------------------------------------------------
// XadesTUpgrader.cs
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

using FirmaXadesNet.Signature;
using FirmaXadesNet.Upgraders.Parameters;
using FirmaXadesNet.Utils;
using Microsoft.Xades;
using System;
using System.Collections;

namespace FirmaXadesNet.Upgraders
{
    class XadesTUpgrader : IXadesUpgrader
    {

        #region Public methods

        public void Upgrade(SignatureDocument signatureDocument, UpgradeParameters parameters)
        {
            TimeStamp signatureTimeStamp;
            ArrayList signatureValueElementXpaths;
            byte[] signatureValueHash;
            UnsignedProperties unsignedProperties = signatureDocument.XadesSignature.UnsignedProperties;

            try
            {
                if (unsignedProperties.UnsignedSignatureProperties.SignatureTimeStampCollection.Count > 0)
                {
                    throw new Exception("La firma ya contiene un sello de tiempo");
                }

                signatureValueElementXpaths = new ArrayList();
                signatureValueElementXpaths.Add("ds:SignatureValue");
                signatureValueHash = DigestUtil.ComputeHashValue(XMLUtil.ComputeValueOfElementList(signatureDocument.XadesSignature, signatureValueElementXpaths), parameters.DigestMethod);

                byte[] tsa = parameters.TimeStampClient.GetTimeStamp(signatureValueHash, parameters.DigestMethod, true);

                signatureTimeStamp = new TimeStamp("SignatureTimeStamp");
                signatureTimeStamp.Id = "SignatureTimeStamp-" + signatureDocument.XadesSignature.Signature.Id;
                signatureTimeStamp.EncapsulatedTimeStamp.PkiData = tsa;
                signatureTimeStamp.EncapsulatedTimeStamp.Id = "SignatureTimeStamp-" + Guid.NewGuid().ToString();

                unsignedProperties.UnsignedSignatureProperties.SignatureTimeStampCollection.Add(signatureTimeStamp);

                signatureDocument.XadesSignature.UnsignedProperties = unsignedProperties;

                signatureDocument.UpdateDocument();
            }
            catch (Exception ex)
            {
                throw new Exception("Ha ocurrido un error al insertar el sellado de tiempo.", ex);
            }
        }

        #endregion
    }
}
