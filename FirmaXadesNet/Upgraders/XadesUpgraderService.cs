// --------------------------------------------------------------------------------------------------------------------
// XadesUpgrader.cs
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirmaXadesNet.Upgraders
{
    public enum SignatureFormat
    {
        XAdES_T,
        XAdES_XL
    }

    public class XadesUpgraderService
    {
        #region Public methods

        public void Upgrade(SignatureDocument sigDocument, SignatureFormat toFormat, UpgradeParameters parameters)
        {
            XadesTUpgrader xadesTUpgrader = null;
            XadesXLUpgrader xadesXLUpgrader = null;

            SignatureDocument.CheckSignatureDocument(sigDocument);

            if (toFormat == SignatureFormat.XAdES_T)
            {
                xadesTUpgrader = new XadesTUpgrader();
                xadesTUpgrader.Upgrade(sigDocument, parameters);
            }
            else
            {
                if (sigDocument.XadesSignature.UnsignedProperties.UnsignedSignatureProperties.SignatureTimeStampCollection.Count == 0)
                {
                    xadesTUpgrader = new XadesTUpgrader();
                    xadesTUpgrader.Upgrade(sigDocument, parameters);
                }

                xadesXLUpgrader = new XadesXLUpgrader();
                xadesXLUpgrader.Upgrade(sigDocument, parameters);
            }
        }

        #endregion
    }
}
