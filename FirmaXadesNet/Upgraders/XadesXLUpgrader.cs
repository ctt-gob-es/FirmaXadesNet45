// --------------------------------------------------------------------------------------------------------------------
// XadesXLUpgrader.cs
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
using FirmaXadesNet.Signature;
using FirmaXadesNet.Upgraders.Parameters;
using FirmaXadesNet.Utils;
using Microsoft.Xades;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Tsp;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Extension;
using Org.BouncyCastle.X509.Store;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace FirmaXadesNet.Upgraders
{
    class XadesXLUpgrader : IXadesUpgrader
    {

        #region Public methods

        public void Upgrade(SignatureDocument signatureDocument, UpgradeParameters parameters)
        {
            UnsignedProperties unsignedProperties = null;
            CertificateValues certificateValues = null;

            X509Certificate2 signingCertificate = signatureDocument.XadesSignature.GetSigningCertificate();

            unsignedProperties = signatureDocument.XadesSignature.UnsignedProperties;
            unsignedProperties.UnsignedSignatureProperties.CompleteCertificateRefs = new CompleteCertificateRefs();
            unsignedProperties.UnsignedSignatureProperties.CompleteCertificateRefs.Id = "CompleteCertificates-" + Guid.NewGuid().ToString();

            unsignedProperties.UnsignedSignatureProperties.CertificateValues = new CertificateValues();
            certificateValues = unsignedProperties.UnsignedSignatureProperties.CertificateValues;
            certificateValues.Id = "CertificatesValues-" + Guid.NewGuid().ToString();

            unsignedProperties.UnsignedSignatureProperties.CompleteRevocationRefs = new CompleteRevocationRefs();
            unsignedProperties.UnsignedSignatureProperties.CompleteRevocationRefs.Id = "CompleteRev-" + Guid.NewGuid().ToString();

            unsignedProperties.UnsignedSignatureProperties.RevocationValues = new RevocationValues();
            unsignedProperties.UnsignedSignatureProperties.RevocationValues.Id = "RevocationValues-" + Guid.NewGuid().ToString();

            AddCertificate(signingCertificate, unsignedProperties, false, parameters.OCSPServers, parameters.CRL, parameters.DigestMethod, parameters.GetOcspUrlFromCertificate);

            AddTSACertificates(unsignedProperties, parameters.OCSPServers, parameters.CRL, parameters.DigestMethod, parameters.GetOcspUrlFromCertificate);

            signatureDocument.XadesSignature.UnsignedProperties = unsignedProperties;

            TimeStampCertRefs(signatureDocument, parameters);

            signatureDocument.UpdateDocument();
        }

        #endregion

        #region Private methods

        private string GetResponderName(ResponderID responderId, ref bool byKey)
        {
            DerTaggedObject dt = (DerTaggedObject)responderId.ToAsn1Object();

            if (dt.TagNo == 1)
            {
                byKey = false;

                return new X500DistinguishedName(dt.GetObject().GetEncoded()).Name;                
            }
            else if (dt.TagNo == 2)
            {
                Asn1TaggedObject tagger = (Asn1TaggedObject)responderId.ToAsn1Object();
                Asn1OctetString pubInfo = (Asn1OctetString)tagger.GetObject();
                byKey = true;

                return Convert.ToBase64String(pubInfo.GetOctets());
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Comprueba si dos DN son equivalentes
        /// </summary>
        /// <param name="dn"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        private bool EquivalentDN(X500DistinguishedName dn, X500DistinguishedName other)
        {
            return X509Name.GetInstance(Asn1Object.FromByteArray(dn.RawData)).Equivalent(X509Name.GetInstance(Asn1Object.FromByteArray(other.RawData)));
        }

        /// <summary>
        /// Determina si un certificado ya ha sido añadido a la colección de certificados
        /// </summary>
        /// <param name="cert"></param>
        /// <param name="unsignedProperties"></param>
        /// <returns></returns>
        private bool CertificateChecked(X509Certificate2 cert, UnsignedProperties unsignedProperties)
        {
            foreach (EncapsulatedX509Certificate item in unsignedProperties.UnsignedSignatureProperties.CertificateValues.EncapsulatedX509CertificateCollection)
            {
                X509Certificate2 certItem = new X509Certificate2(item.PkiData);

                if (certItem.Thumbprint == cert.Thumbprint)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Inserta en la lista de certificados el certificado y comprueba la valided del certificado.
        /// </summary>
        /// <param name="cert"></param>
        /// <param name="unsignedProperties"></param>
        /// <param name="addCertValue"></param>
        /// <param name="extraCerts"></param>
        private void AddCertificate(X509Certificate2 cert, UnsignedProperties unsignedProperties, bool addCert, IEnumerable<OcspServer> ocspServers,
            IEnumerable<X509Crl> crlList, FirmaXadesNet.Crypto.DigestMethod digestMethod, bool addCertificateOcspUrl, X509Certificate2[] extraCerts = null)
        {
            if (addCert)
            {
                if (CertificateChecked(cert, unsignedProperties))
                {
                    return;
                }

                string guidCert = Guid.NewGuid().ToString();

                Cert chainCert = new Cert();
                chainCert.IssuerSerial.X509IssuerName = cert.IssuerName.Name;
                chainCert.IssuerSerial.X509SerialNumber = cert.GetSerialNumberAsDecimalString();
                DigestUtil.SetCertDigest(cert.GetRawCertData(), digestMethod, chainCert.CertDigest);
                chainCert.URI = "#Cert" + guidCert;
                unsignedProperties.UnsignedSignatureProperties.CompleteCertificateRefs.CertRefs.CertCollection.Add(chainCert);

                EncapsulatedX509Certificate encapsulatedX509Certificate = new EncapsulatedX509Certificate();
                encapsulatedX509Certificate.Id = "Cert" + guidCert;
                encapsulatedX509Certificate.PkiData = cert.GetRawCertData();
                unsignedProperties.UnsignedSignatureProperties.CertificateValues.EncapsulatedX509CertificateCollection.Add(encapsulatedX509Certificate);
            }

            var chain = CertUtil.GetCertChain(cert, extraCerts).ChainElements;

            if (chain.Count > 1)
            {
                X509ChainElementEnumerator enumerator = chain.GetEnumerator();
                enumerator.MoveNext(); // el mismo certificado que el pasado por parametro

                enumerator.MoveNext();

                bool valid = ValidateCertificateByCRL(unsignedProperties, cert, enumerator.Current.Certificate, crlList, digestMethod);

                if (!valid)
                {
                    var ocspCerts = ValidateCertificateByOCSP(unsignedProperties, cert, enumerator.Current.Certificate, ocspServers, digestMethod, addCertificateOcspUrl);

                    if (ocspCerts != null)
                    {
                        X509Certificate2 startOcspCert = DetermineStartCert(ocspCerts);

                        if (!EquivalentDN(startOcspCert.IssuerName, enumerator.Current.Certificate.SubjectName))
                        {
                            var chainOcsp = CertUtil.GetCertChain(startOcspCert, ocspCerts);

                            AddCertificate(chainOcsp.ChainElements[1].Certificate, unsignedProperties, true, ocspServers, crlList, digestMethod, addCertificateOcspUrl, ocspCerts);
                        }
                    }
                }

                AddCertificate(enumerator.Current.Certificate, unsignedProperties, true, ocspServers, crlList, digestMethod, addCertificateOcspUrl, extraCerts);
            }
        }

        private bool ExistsCRL(CRLRefCollection collection, string issuer)
        {
            foreach (CRLRef crlRef in collection)
            {
                if (crlRef.CRLIdentifier.Issuer == issuer)
                {
                    return true;
                }
            }

            return false;
        }

        private long? GetCRLNumber(Org.BouncyCastle.X509.X509Crl crlEntry)
        {
            Asn1OctetString extValue = crlEntry.GetExtensionValue(X509Extensions.CrlNumber);

            if (extValue != null)
            {
                Asn1Object asn1Value = X509ExtensionUtilities.FromExtensionValue(extValue);

                return DerInteger.GetInstance(asn1Value).PositiveValue.LongValue;
            }

            return null;
        }

        private bool ValidateCertificateByCRL(UnsignedProperties unsignedProperties, X509Certificate2 certificate, X509Certificate2 issuer,
            IEnumerable<X509Crl> crlList, FirmaXadesNet.Crypto.DigestMethod digestMethod)
        {
            Org.BouncyCastle.X509.X509Certificate clientCert = certificate.ToBouncyX509Certificate();
            Org.BouncyCastle.X509.X509Certificate issuerCert = issuer.ToBouncyX509Certificate();

            foreach (var crlEntry in crlList)
            {
                if (crlEntry.IssuerDN.Equivalent(issuerCert.SubjectDN) && crlEntry.NextUpdate.Value > DateTime.Now)
                {
                    if (!crlEntry.IsRevoked(clientCert))
                    {
                        if (!ExistsCRL(unsignedProperties.UnsignedSignatureProperties.CompleteRevocationRefs.CRLRefs.CRLRefCollection,
                            issuer.Subject))
                        {
                            string idCrlValue = "CRLValue-" + Guid.NewGuid().ToString();

                            CRLRef crlRef = new CRLRef();
                            crlRef.CRLIdentifier.UriAttribute = "#" + idCrlValue;
                            crlRef.CRLIdentifier.Issuer = issuer.Subject;
                            crlRef.CRLIdentifier.IssueTime = crlEntry.ThisUpdate.ToLocalTime();

                            var crlNumber = GetCRLNumber(crlEntry);
                            if (crlNumber.HasValue)
                            {
                                crlRef.CRLIdentifier.Number = crlNumber.Value;
                            }

                            byte[] crlEncoded = crlEntry.GetEncoded();
                            DigestUtil.SetCertDigest(crlEncoded, digestMethod, crlRef.CertDigest);

                            CRLValue crlValue = new CRLValue();
                            crlValue.PkiData = crlEncoded;
                            crlValue.Id = idCrlValue;

                            unsignedProperties.UnsignedSignatureProperties.CompleteRevocationRefs.CRLRefs.CRLRefCollection.Add(crlRef);
                            unsignedProperties.UnsignedSignatureProperties.RevocationValues.CRLValues.CRLValueCollection.Add(crlValue);
                        }

                        return true;
                    }
                    else
                    {
                        throw new Exception("Certificado revocado");
                    }
                }
            }

            return false;
        }

        private X509Certificate2[] ValidateCertificateByOCSP(UnsignedProperties unsignedProperties, X509Certificate2 client, X509Certificate2 issuer,
            IEnumerable<OcspServer> ocspServers, FirmaXadesNet.Crypto.DigestMethod digestMethod, bool addCertificateOcspUrl)
        {
            bool byKey = false;
            List<OcspServer> finalOcspServers = new List<OcspServer>();
            Org.BouncyCastle.X509.X509Certificate clientCert = client.ToBouncyX509Certificate();
            Org.BouncyCastle.X509.X509Certificate issuerCert = issuer.ToBouncyX509Certificate();

            OcspClient ocsp = new OcspClient();

            if (addCertificateOcspUrl)
            {
                string certOcspUrl = ocsp.GetAuthorityInformationAccessOcspUrl(issuerCert);

                if (!string.IsNullOrEmpty(certOcspUrl))
                {
                    finalOcspServers.Add(new OcspServer(certOcspUrl));
                }
            }

            foreach (var ocspServer in ocspServers)
            {
                finalOcspServers.Add(ocspServer);
            }

            foreach (var ocspServer in finalOcspServers)
            {
                byte[] resp = ocsp.QueryBinary(clientCert, issuerCert, ocspServer.Url, ocspServer.RequestorName,
                    ocspServer.SignCertificate);

                FirmaXadesNet.Clients.CertificateStatus status = ocsp.ProcessOcspResponse(resp);

                if (status == FirmaXadesNet.Clients.CertificateStatus.Revoked)
                {
                    throw new Exception("Certificado revocado");
                }
                else if (status == FirmaXadesNet.Clients.CertificateStatus.Good)
                {
                    Org.BouncyCastle.Ocsp.OcspResp r = new OcspResp(resp);
                    byte[] rEncoded = r.GetEncoded();
                    BasicOcspResp or = (BasicOcspResp)r.GetResponseObject();

                    string guidOcsp = Guid.NewGuid().ToString();

                    OCSPRef ocspRef = new OCSPRef();
                    ocspRef.OCSPIdentifier.UriAttribute = "#OcspValue" + guidOcsp;
                    DigestUtil.SetCertDigest(rEncoded, digestMethod, ocspRef.CertDigest);

                    ResponderID rpId = or.ResponderId.ToAsn1Object();
                    ocspRef.OCSPIdentifier.ResponderID = GetResponderName(rpId, ref byKey);
                    ocspRef.OCSPIdentifier.ByKey = byKey;

                    ocspRef.OCSPIdentifier.ProducedAt = or.ProducedAt.ToLocalTime();
                    unsignedProperties.UnsignedSignatureProperties.CompleteRevocationRefs.OCSPRefs.OCSPRefCollection.Add(ocspRef);

                    OCSPValue ocspValue = new OCSPValue();
                    ocspValue.PkiData = rEncoded;
                    ocspValue.Id = "OcspValue" + guidOcsp;
                    unsignedProperties.UnsignedSignatureProperties.RevocationValues.OCSPValues.OCSPValueCollection.Add(ocspValue);

                    return (from cert in or.GetCerts()
                            select new X509Certificate2(cert.GetEncoded())).ToArray();
                }
            }

            throw new Exception("El certificado no ha podido ser validado");
        }

        private X509Certificate2 DetermineStartCert(X509Certificate2[] certs)
        {
            X509Certificate2 currentCert = null;
            bool isIssuer = true;

            for (int i = 0; i < certs.Length && isIssuer; i++)
            {
                currentCert = certs[i];
                isIssuer = false;

                for (int j = 0; j < certs.Length; j++)
                {
                    if (EquivalentDN(certs[j].IssuerName, currentCert.SubjectName))
                    {
                        isIssuer = true;
                        break;
                    }
                }
            }

            return currentCert;
        }

        /// <summary>
        /// Inserta y valida los certificados del servidor de sellado de tiempo.
        /// </summary>
        /// <param name="unsignedProperties"></param>
        private void AddTSACertificates(UnsignedProperties unsignedProperties, IEnumerable<OcspServer> ocspServers, IEnumerable<X509Crl> crlList, FirmaXadesNet.Crypto.DigestMethod digestMethod, bool addCertificateOcspUrl)
        {
            TimeStampToken token = new TimeStampToken(new CmsSignedData(unsignedProperties.UnsignedSignatureProperties.SignatureTimeStampCollection[0].EncapsulatedTimeStamp.PkiData));
            IX509Store store = token.GetCertificates("Collection");

            Org.BouncyCastle.Cms.SignerID signerId = token.SignerID;

            List<X509Certificate2> tsaCerts = new List<X509Certificate2>();
            foreach (var tsaCert in store.GetMatches(null))
            {
                X509Certificate2 cert = new X509Certificate2(((Org.BouncyCastle.X509.X509Certificate)tsaCert).GetEncoded());
                tsaCerts.Add(cert);
            }

            X509Certificate2 startCert = DetermineStartCert(tsaCerts.ToArray());
            AddCertificate(startCert, unsignedProperties, true, ocspServers, crlList, digestMethod, addCertificateOcspUrl, tsaCerts.ToArray());
        }

        private void TimeStampCertRefs(SignatureDocument signatureDocument, UpgradeParameters parameters)
        {
            TimeStamp xadesXTimeStamp;
            ArrayList signatureValueElementXpaths;
            byte[] signatureValueHash;

            XmlElement nodoFirma = signatureDocument.XadesSignature.GetSignatureElement();

            XmlNamespaceManager nm = new XmlNamespaceManager(signatureDocument.Document.NameTable);
            nm.AddNamespace("xades", XadesSignedXml.XadesNamespaceUri);
            nm.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);

            XmlNode xmlCompleteCertRefs = nodoFirma.SelectSingleNode("ds:Object/xades:QualifyingProperties/xades:UnsignedProperties/xades:UnsignedSignatureProperties/xades:CompleteCertificateRefs", nm);

            if (xmlCompleteCertRefs == null)
            {
                signatureDocument.UpdateDocument();
            }

            signatureValueElementXpaths = new ArrayList();
            signatureValueElementXpaths.Add("ds:SignatureValue");
            signatureValueElementXpaths.Add("ds:Object/xades:QualifyingProperties/xades:UnsignedProperties/xades:UnsignedSignatureProperties/xades:SignatureTimeStamp");
            signatureValueElementXpaths.Add("ds:Object/xades:QualifyingProperties/xades:UnsignedProperties/xades:UnsignedSignatureProperties/xades:CompleteCertificateRefs");
            signatureValueElementXpaths.Add("ds:Object/xades:QualifyingProperties/xades:UnsignedProperties/xades:UnsignedSignatureProperties/xades:CompleteRevocationRefs");
            signatureValueHash = DigestUtil.ComputeHashValue(XMLUtil.ComputeValueOfElementList(signatureDocument.XadesSignature, signatureValueElementXpaths), parameters.DigestMethod);

            byte[] tsa = parameters.TimeStampClient.GetTimeStamp(signatureValueHash, parameters.DigestMethod, true);

            xadesXTimeStamp = new TimeStamp("SigAndRefsTimeStamp");
            xadesXTimeStamp.Id = "SigAndRefsStamp-" + signatureDocument.XadesSignature.Signature.Id;
            xadesXTimeStamp.EncapsulatedTimeStamp.PkiData = tsa;
            xadesXTimeStamp.EncapsulatedTimeStamp.Id = "SigAndRefsStamp-" + Guid.NewGuid().ToString();
            UnsignedProperties unsignedProperties = signatureDocument.XadesSignature.UnsignedProperties;

            unsignedProperties.UnsignedSignatureProperties.RefsOnlyTimeStampFlag = false;
            unsignedProperties.UnsignedSignatureProperties.SigAndRefsTimeStampCollection.Add(xadesXTimeStamp);


            signatureDocument.XadesSignature.UnsignedProperties = unsignedProperties;
        }

        #endregion
    }
}
