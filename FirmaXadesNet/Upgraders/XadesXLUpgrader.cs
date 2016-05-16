// --------------------------------------------------------------------------------------------------------------------
// XadesXLUpgrader.cs
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
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Tsp;
using Org.BouncyCastle.X509.Store;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FirmaXadesNet.Upgraders
{
    class XadesXLUpgrader : XadesUpgrader
    {
        #region Constructors

        public XadesXLUpgrader(FirmaXades firma)
            : base(firma)
        {

        }

        #endregion

        #region Public methods

        public override void Upgrade()
        {
            UnsignedProperties unsignedProperties = null;
            CertificateValues certificateValues = null;

            unsignedProperties = _firma.XadesSignature.UnsignedProperties;
            unsignedProperties.UnsignedSignatureProperties.CompleteCertificateRefs = new CompleteCertificateRefs();
            unsignedProperties.UnsignedSignatureProperties.CompleteCertificateRefs.Id = "CompleteCertificates-" + Guid.NewGuid().ToString();

            unsignedProperties.UnsignedSignatureProperties.CertificateValues = new CertificateValues();
            certificateValues = unsignedProperties.UnsignedSignatureProperties.CertificateValues;
            certificateValues.Id = "CertificatesValues-" + Guid.NewGuid().ToString();

            unsignedProperties.UnsignedSignatureProperties.CompleteRevocationRefs = new CompleteRevocationRefs();
            unsignedProperties.UnsignedSignatureProperties.CompleteRevocationRefs.Id = "CompleteRev-" + Guid.NewGuid().ToString();

            unsignedProperties.UnsignedSignatureProperties.RevocationValues = new RevocationValues();
            unsignedProperties.UnsignedSignatureProperties.RevocationValues.Id = "RevocationValues-" + Guid.NewGuid().ToString();

            AddCertificate(_firma.Certificate, unsignedProperties, false);

            AddTSACertificates(unsignedProperties);

            _firma.XadesSignature.UnsignedProperties = unsignedProperties;

            TimeStampCertRefs();

            _firma.UpdateDocument();

        }

        #endregion

        #region Private methods

        private string RevertIssuerName(string issuer)
        {
            string[] tokens = issuer.Split(',');
            string result = "";

            for (int i = tokens.Length - 1; i >= 0; i--)
            {
                if (!string.IsNullOrEmpty(result))
                {
                    result += ",";
                }

                result += tokens[i];
            }

            return result;
        }


        private string GetResponderName(ResponderID responderId, ref bool byKey)
        {
            Org.BouncyCastle.Asn1.DerTaggedObject dt = (Org.BouncyCastle.Asn1.DerTaggedObject)responderId.ToAsn1Object();

            if (dt.TagNo == 1)
            {
                Org.BouncyCastle.Asn1.X509.X509Name name = Org.BouncyCastle.Asn1.X509.X509Name.GetInstance(dt.GetObject());
                byKey = false;

                return name.ToString();
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
        /// Determina si un certificado ya ha sido añadido a la colección de certificados
        /// </summary>
        /// <param name="cert"></param>
        /// <param name="unsignedProperties"></param>
        /// <returns></returns>
        private bool CertificateChecked(X509Certificate2 cert, UnsignedProperties unsignedProperties)
        {
            string certHash = null;

            using (var hashAlg = DigestUtil.GetHashAlg(_firma.RefsDigestMethod))
            {
                certHash = Convert.ToBase64String(hashAlg.ComputeHash(cert.GetRawCertData()));
            }

            foreach (Cert item in unsignedProperties.UnsignedSignatureProperties.CompleteCertificateRefs.CertRefs.CertCollection)
            {
                if (Convert.ToBase64String(item.CertDigest.DigestValue) == certHash)
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
        private void AddCertificate(X509Certificate2 cert, UnsignedProperties unsignedProperties, bool addCert, X509Certificate2[] extraCerts = null)
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
                chainCert.IssuerSerial.X509SerialNumber = CertUtil.HexToDecimal(cert.SerialNumber);
                DigestUtil.SetCertDigest(cert.GetRawCertData(), _firma.RefsDigestMethod, chainCert.CertDigest);
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

                bool valid = ValidateCertificateByCRL(unsignedProperties, cert, enumerator.Current.Certificate);

                if (!valid)
                {
                    var ocspCerts = ValidateCertificateByOCSP(unsignedProperties, cert, enumerator.Current.Certificate);

                    if (ocspCerts != null)
                    {
                        X509Certificate2 startOcspCert = DetermineStartCert(new List<X509Certificate2>(ocspCerts));

                        if (startOcspCert.IssuerName.Name != enumerator.Current.Certificate.SubjectName.Name)
                        {
                            var chainOcsp = CertUtil.GetCertChain(startOcspCert, ocspCerts);

                            AddCertificate(chainOcsp.ChainElements[1].Certificate, unsignedProperties, true, ocspCerts);
                        }
                    }
                }

                AddCertificate(enumerator.Current.Certificate, unsignedProperties, true, extraCerts);
            }
        }

        private bool ExistsCRL(CRLRefCollection collection, string issuer)
        {
            foreach (CRLRef clrRef in collection)
            {
                if (clrRef.CRLIdentifier.Issuer == issuer)
                {
                    return true;
                }
            }

            return false;
        }

        private long? GetCRLNumber(Org.BouncyCastle.X509.X509Crl crlEntry)
        {
            Asn1OctetString extValue = crlEntry.GetExtensionValue(Org.BouncyCastle.Asn1.X509.X509Extensions.CrlNumber);

            if (extValue != null)
            {
                Asn1Object asn1Value = Org.BouncyCastle.X509.Extension.X509ExtensionUtilities.FromExtensionValue(extValue);

                return DerInteger.GetInstance(asn1Value).PositiveValue.LongValue;
            }

            return null;
        }

        private bool ValidateCertificateByCRL(UnsignedProperties unsignedProperties, X509Certificate2 certificate, X509Certificate2 issuer)
        {
            Org.BouncyCastle.X509.X509Certificate clientCert = CertUtil.ConvertToX509Certificate(certificate);
            Org.BouncyCastle.X509.X509Certificate issuerCert = CertUtil.ConvertToX509Certificate(issuer);

            foreach (var crlEntry in _firma.CRLEntries)
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
                            DigestUtil.SetCertDigest(crlEncoded, _firma.RefsDigestMethod, crlRef.CertDigest);

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

        private X509Certificate2[] ValidateCertificateByOCSP(UnsignedProperties unsignedProperties, X509Certificate2 client, X509Certificate2 issuer)
        {
            bool byKey = false;
            List<string> ocspServers = new List<string>();
            Org.BouncyCastle.X509.X509Certificate clientCert = CertUtil.ConvertToX509Certificate(client);
            Org.BouncyCastle.X509.X509Certificate issuerCert = CertUtil.ConvertToX509Certificate(issuer);

            OcspClient ocsp = new OcspClient();
            string certOcspUrl = ocsp.GetAuthorityInformationAccessOcspUrl(issuerCert);

            if (!string.IsNullOrEmpty(certOcspUrl))
            {
                ocspServers.Add(certOcspUrl);
            }

            foreach (var ocspUrl in _firma.OCSPServers)
            {
                ocspServers.Add(ocspUrl);
            }

            foreach (var ocspUrl in ocspServers)
            {
                byte[] resp = ocsp.QueryBinary(clientCert, issuerCert, ocspUrl);

                FirmaXadesNet.Clients.CertificateStatus status = ocsp.ProcessOcspResponse(clientCert, issuerCert, resp);

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
                    DigestUtil.SetCertDigest(rEncoded, _firma.RefsDigestMethod, ocspRef.CertDigest);

                    Org.BouncyCastle.Asn1.Ocsp.ResponderID rpId = or.ResponderId.ToAsn1Object();
                    string name = GetResponderName(rpId, ref byKey);

                    if (!byKey)
                    {
                        ocspRef.OCSPIdentifier.ResponderID = RevertIssuerName(name);
                    }
                    else
                    {
                        ocspRef.OCSPIdentifier.ResponderID = name;
                        ocspRef.OCSPIdentifier.ByKey = true;
                    }

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

        private X509Certificate2 DetermineStartCert(IList<X509Certificate2> certs)
        {
            X509Certificate2 currentCert = null;
            bool isIssuer = true;

            for (int i = 0; i < certs.Count && isIssuer; i++)
            {
                currentCert = certs[i];
                isIssuer = false;

                for (int j = 0; j < certs.Count; j++)
                {
                    if (certs[j].IssuerName.Name == currentCert.SubjectName.Name)
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
        private void AddTSACertificates(UnsignedProperties unsignedProperties)
        {
            TimeStampToken token = new TimeStampToken(new Org.BouncyCastle.Cms.CmsSignedData(unsignedProperties.UnsignedSignatureProperties.SignatureTimeStampCollection[0].EncapsulatedTimeStamp.PkiData));
            IX509Store store = token.GetCertificates("Collection");

            Org.BouncyCastle.Cms.SignerID signerId = token.SignerID;

            List<X509Certificate2> tsaCerts = new List<X509Certificate2>();
            foreach (var tsaCert in store.GetMatches(null))
            {
                X509Certificate2 cert = new X509Certificate2(((Org.BouncyCastle.X509.X509Certificate)tsaCert).GetEncoded());
                tsaCerts.Add(cert);
            }

            X509Certificate2 startCert = DetermineStartCert(tsaCerts);
            AddCertificate(startCert, unsignedProperties, true, tsaCerts.ToArray());
        }

        private void TimeStampCertRefs()
        {
            TimeStamp xadesXTimeStamp;
            ArrayList signatureValueElementXpaths;
            byte[] signatureValueHash;

            XmlElement nodoFirma = _firma.XadesSignature.GetSignatureElement();

            XmlNamespaceManager nm = new XmlNamespaceManager(_firma.Document.NameTable);
            nm.AddNamespace("xades", XadesSignedXml.XadesNamespaceUri);
            nm.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);

            XmlNode xmlCompleteCertRefs = nodoFirma.SelectSingleNode("ds:Object/xades:QualifyingProperties/xades:UnsignedProperties/xades:UnsignedSignatureProperties/xades:CompleteCertificateRefs", nm);

            if (xmlCompleteCertRefs == null)
            {
                _firma.UpdateDocument();
            }

            signatureValueElementXpaths = new ArrayList();
            signatureValueElementXpaths.Add("ds:SignatureValue");
            signatureValueElementXpaths.Add("ds:Object/xades:QualifyingProperties/xades:UnsignedProperties/xades:UnsignedSignatureProperties/xades:SignatureTimeStamp");
            signatureValueElementXpaths.Add("ds:Object/xades:QualifyingProperties/xades:UnsignedProperties/xades:UnsignedSignatureProperties/xades:CompleteCertificateRefs");
            signatureValueElementXpaths.Add("ds:Object/xades:QualifyingProperties/xades:UnsignedProperties/xades:UnsignedSignatureProperties/xades:CompleteRevocationRefs");
            signatureValueHash = DigestUtil.ComputeHashValue(XMLUtil.ComputeValueOfElementList(_firma.XadesSignature, signatureValueElementXpaths), DigestMethod.SHA1);

            byte[] tsa = TimeStampClient.GetTimeStamp(_firma.TSAServer, signatureValueHash, DigestMethod.SHA1, true);

            xadesXTimeStamp = new TimeStamp("SigAndRefsTimeStamp");
            xadesXTimeStamp.Id = "SigAndRefsStamp-" + _firma.XadesSignature.Signature.Id;
            xadesXTimeStamp.EncapsulatedTimeStamp.PkiData = tsa;
            xadesXTimeStamp.EncapsulatedTimeStamp.Id = "SigAndRefsStamp-" + Guid.NewGuid().ToString();
            UnsignedProperties unsignedProperties = _firma.XadesSignature.UnsignedProperties;

            unsignedProperties.UnsignedSignatureProperties.RefsOnlyTimeStampFlag = false;
            unsignedProperties.UnsignedSignatureProperties.SigAndRefsTimeStampCollection.Add(xadesXTimeStamp);


            _firma.XadesSignature.UnsignedProperties = unsignedProperties;
        }

        #endregion
    }
}
