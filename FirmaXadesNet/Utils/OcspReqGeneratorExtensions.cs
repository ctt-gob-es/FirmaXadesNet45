// --------------------------------------------------------------------------------------------------------------------
// DotNetOcspReqGenerator.cs
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

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Security;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace FirmaXadesNet.Utils
{
    /// <summary>
    /// Se extiende la clase OcspReqGenerator 
    /// </summary>
    static class OcspReqGeneratorExtensions
    {

        public static OcspReq Generate(this OcspReqGenerator ocspRegGenerator,                    
                    RSACryptoServiceProvider rsa,
                    X509Chain chain)
        {
            Asn1EncodableVector requests = new Asn1EncodableVector();
            DerObjectIdentifier signingAlgorithm = PkcsObjectIdentifiers.Sha1WithRsaEncryption;

            IList list = null;

            Type OcspReqGeneratorInfo_Type = typeof(OcspReqGenerator);

            FieldInfo ListInfo_m_parameters = OcspReqGeneratorInfo_Type.GetField("list", BindingFlags.NonPublic | BindingFlags.Instance);
            list = (IList)ListInfo_m_parameters.GetValue(ocspRegGenerator);

            Type RequestObjectType = OcspReqGeneratorInfo_Type.GetNestedType("RequestObject", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo toRequestMethod = RequestObjectType.GetMethod("ToRequest");

            foreach (object reqObj in list)
            {
                try
                {
                    requests.Add((Request)toRequestMethod.Invoke(reqObj, null));
                }
                catch (Exception e)
                {
                    throw new OcspException("exception creating Request", e);
                }
            }

            GeneralName requestorName;

            FieldInfo GeneralNameInfo_m_parameters = OcspReqGeneratorInfo_Type.GetField("requestorName", BindingFlags.NonPublic | BindingFlags.Instance);
            requestorName = (GeneralName)GeneralNameInfo_m_parameters.GetValue(ocspRegGenerator);

            X509Extensions requestExtensions = null;

            FieldInfo requestExtensions_parameters = OcspReqGeneratorInfo_Type.GetField("requestExtensions", BindingFlags.NonPublic | BindingFlags.Instance);
            requestExtensions = (X509Extensions)requestExtensions_parameters.GetValue(ocspRegGenerator);

            TbsRequest tbsReq = new TbsRequest(requestorName, new DerSequence(requests), requestExtensions);

            Org.BouncyCastle.Asn1.Ocsp.Signature signature = null;

            if (signingAlgorithm != null)
            {
                if (requestorName == null)
                {
                    throw new OcspException("requestorName must be specified if request is signed.");
                }

                DerBitString bitSig = null;

                try
                {
                    byte[] encoded = tbsReq.GetEncoded();

                    byte[] signedData = rsa.SignData(encoded, new SHA1CryptoServiceProvider());

                    bitSig = new DerBitString(signedData);
                }
                catch (Exception e)
                {
                    throw new OcspException("exception processing TBSRequest: " + e, e);
                }

                AlgorithmIdentifier sigAlgId = new AlgorithmIdentifier(signingAlgorithm, DerNull.Instance);

                if (chain != null && chain.ChainElements.Count > 0)
                {
                    Asn1EncodableVector v = new Asn1EncodableVector();
                    try
                    {
                        for (int i = 0; i != chain.ChainElements.Count; i++)
                        {
                            v.Add(
                                X509CertificateStructure.GetInstance(
                                    Asn1Object.FromByteArray(chain.ChainElements[i].Certificate.RawData)));
                        }
                    }
                    catch (Exception e)
                    {
                        throw new OcspException("error processing certs", e);
                    }

                    signature = new Org.BouncyCastle.Asn1.Ocsp.Signature(sigAlgId, bitSig, new DerSequence(v));
                }
                else
                {
                    signature = new Org.BouncyCastle.Asn1.Ocsp.Signature(sigAlgId, bitSig);
                }
            }

            return new OcspReq(new OcspRequest(tbsReq, signature));
        }
    }
}
