// --------------------------------------------------------------------------------------------------------------------
// FirmaXades.cs
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

using FirmaXadesNet.Signature;
using FirmaXadesNet.Signature.Parameters;
using FirmaXadesNet.Utils;
using FirmaXadesNet.Validation;
using Microsoft.Xades;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;


namespace FirmaXadesNet
{

    public class XadesService 
    {

        #region Private variables

        private Reference _refContent;
        private string _mimeType;
        private string _encoding;

        #endregion

        #region Public methods

        #region Métodos de firma

        /// <summary>
        /// Realiza el proceso de firmado
        /// </summary>
        /// <param name="input"></param>
        /// <param name="parameters"></param>
        public SignatureDocument Sign(Stream input, SignatureParameters parameters)
        {
            if (parameters.Signer == null)
            {
                throw new Exception("Es necesario un certificado válido para la firma");
            }

            if (input == null && string.IsNullOrEmpty(parameters.ExternalContentUri))
            {
                throw new Exception("No se ha especificado ningún contenido a firmar");
            }

            SignatureDocument signatureDocument = new SignatureDocument();

            switch (parameters.SignaturePackaging)
            {
                case SignaturePackaging.INTERNALLY_DETACHED:
                    if (string.IsNullOrEmpty(parameters.InputMimeType))
                    {
                        throw new NullReferenceException("Se necesita especificar el tipo MIME del elemento a firmar.");
                    }

                    if (!string.IsNullOrEmpty(parameters.ElementIdToSign))
                    {
                        SetContentInternallyDetached(signatureDocument, XMLUtil.LoadDocument(input), parameters.ElementIdToSign, parameters.InputMimeType);
                    }
                    else
                    {
                        SetContentInternallyDetached(signatureDocument, input, parameters.InputMimeType);
                    }
                    break;

                case SignaturePackaging.ENVELOPED:
                    SetContentEnveloped(signatureDocument, XMLUtil.LoadDocument(input));
                    break;

                case SignaturePackaging.ENVELOPING:
                    SetContentEveloping(signatureDocument, XMLUtil.LoadDocument(input));
                    break;

                case SignaturePackaging.EXTERNALLY_DETACHED:
                    SetContentExternallyDetached(signatureDocument, parameters.ExternalContentUri);
                    break;
            }

            SetSignatureId(signatureDocument.XadesSignature);

            PrepareSignature(signatureDocument, parameters);

            ComputeSignature(signatureDocument);

            signatureDocument.UpdateDocument();

            return signatureDocument;
        }

        /// <summary>
        /// Añade una firma al documento
        /// </summary>
        /// <param name="sigDocument"></param>
        /// <param name="parameters"></param>
        public SignatureDocument CoSign(SignatureDocument sigDocument, SignatureParameters parameters)
        {
            SignatureDocument.CheckSignatureDocument(sigDocument);

            _refContent = sigDocument.XadesSignature.GetContentReference();

            if (_refContent == null)
            {
                throw new Exception("No se ha podido encontrar la referencia del contenido firmado.");
            }

            _mimeType = string.Empty;
            _encoding = string.Empty;

            foreach (DataObjectFormat dof in sigDocument.XadesSignature.XadesObject.QualifyingProperties.SignedProperties.SignedDataObjectProperties.DataObjectFormatCollection)
            {
                if (dof.ObjectReferenceAttribute == ("#" + _refContent.Id))
                {
                    _mimeType = dof.MimeType;
                    _encoding = dof.Encoding;
                    break;
                }
            }

            SignatureDocument coSignatureDocument = new SignatureDocument();
            coSignatureDocument.Document = (XmlDocument)sigDocument.Document.Clone();
            coSignatureDocument.Document.PreserveWhitespace = true;

            coSignatureDocument.XadesSignature = new XadesSignedXml(coSignatureDocument.Document);
            coSignatureDocument.XadesSignature.LoadXml(sigDocument.XadesSignature.GetXml());

            var destination = coSignatureDocument.XadesSignature.GetSignatureElement().ParentNode;

            coSignatureDocument.XadesSignature = new XadesSignedXml(coSignatureDocument.Document);

            _refContent.Id = "Reference-" + Guid.NewGuid().ToString();

            if (_refContent.Type != XadesSignedXml.XmlDsigObjectType)
            {
                _refContent.Type = "";
            }

            coSignatureDocument.XadesSignature.AddReference(_refContent);

            if (destination.NodeType != XmlNodeType.Document)
            {
                coSignatureDocument.XadesSignature.SignatureNodeDestination = (XmlElement)destination;
            }
            else
            {
                coSignatureDocument.XadesSignature.SignatureNodeDestination = ((XmlDocument)destination).DocumentElement;
            }


            SetSignatureId(coSignatureDocument.XadesSignature);

            PrepareSignature(coSignatureDocument, parameters);

            ComputeSignature(coSignatureDocument);

            coSignatureDocument.UpdateDocument();

            return coSignatureDocument;
        }


        /// <summary>
        /// Realiza la contrafirma de la firma actual
        /// </summary>
        /// <param name="sigDocument"></param>
        /// <param name="parameters"></param>
        public SignatureDocument CounterSign(SignatureDocument sigDocument, SignatureParameters parameters)
        {
            if (parameters.Signer == null)
            {
                throw new Exception("Es necesario un certificado válido para la firma.");
            }

            SignatureDocument.CheckSignatureDocument(sigDocument);

            SignatureDocument counterSigDocument = new SignatureDocument();
            counterSigDocument.Document = (XmlDocument)sigDocument.Document.Clone();
            counterSigDocument.Document.PreserveWhitespace = true;

            XadesSignedXml counterSignature = new XadesSignedXml(counterSigDocument.Document);
            SetSignatureId(counterSignature);

            counterSignature.SigningKey = parameters.Signer.SigningKey;

            _refContent = new Reference();
            _refContent.Uri = "#" + sigDocument.XadesSignature.SignatureValueId;
            _refContent.Id = "Reference-" + Guid.NewGuid().ToString();
            _refContent.Type = "http://uri.etsi.org/01903#CountersignedSignature";
            _refContent.AddTransform(new XmlDsigC14NTransform());
            counterSignature.AddReference(_refContent);

            _mimeType = "text/xml";
            _encoding = "UTF-8";

            KeyInfo keyInfo = new KeyInfo();
            keyInfo.Id = "KeyInfoId-" + counterSignature.Signature.Id;
            keyInfo.AddClause(new KeyInfoX509Data((X509Certificate)parameters.Signer.Certificate));
            keyInfo.AddClause(new RSAKeyValue((RSA)parameters.Signer.SigningKey));
            counterSignature.KeyInfo = keyInfo;

            Reference referenceKeyInfo = new Reference();
            referenceKeyInfo.Id = "ReferenceKeyInfo-" + counterSignature.Signature.Id;
            referenceKeyInfo.Uri = "#KeyInfoId-" + counterSignature.Signature.Id;
            counterSignature.AddReference(referenceKeyInfo);

            XadesObject counterSignatureXadesObject = new XadesObject();
            counterSignatureXadesObject.Id = "CounterSignatureXadesObject-" + Guid.NewGuid().ToString();
            counterSignatureXadesObject.QualifyingProperties.Target = "#" + counterSignature.Signature.Id;
            counterSignatureXadesObject.QualifyingProperties.SignedProperties.Id = "SignedProperties-" + counterSignature.Signature.Id;

            AddSignatureProperties(counterSigDocument, counterSignatureXadesObject.QualifyingProperties.SignedProperties.SignedSignatureProperties,
                counterSignatureXadesObject.QualifyingProperties.SignedProperties.SignedDataObjectProperties,
                counterSignatureXadesObject.QualifyingProperties.UnsignedProperties.UnsignedSignatureProperties, parameters);

            counterSignature.AddXadesObject(counterSignatureXadesObject);

            foreach (Reference signReference in counterSignature.SignedInfo.References)
            {
                signReference.DigestMethod = parameters.DigestMethod.URI;
            }

            counterSignature.SignedInfo.SignatureMethod = parameters.SignatureMethod.URI;

            counterSignature.AddXadesNamespace = true;
            counterSignature.ComputeSignature();

            counterSigDocument.XadesSignature = new XadesSignedXml(counterSigDocument.Document);
            counterSigDocument.XadesSignature.LoadXml(sigDocument.XadesSignature.GetXml());

            UnsignedProperties unsignedProperties = counterSigDocument.XadesSignature.UnsignedProperties;
            unsignedProperties.UnsignedSignatureProperties.CounterSignatureCollection.Add(counterSignature);
            counterSigDocument.XadesSignature.UnsignedProperties = unsignedProperties;

            counterSigDocument.UpdateDocument();

            return counterSigDocument;
        }

        #endregion

        #region Carga de firmas

        /// <summary>
        /// Carga un archivo de firma.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public SignatureDocument[] Load(Stream input)
        {           
            return Load(XMLUtil.LoadDocument(input));
        }

        /// <summary>
        /// Carga un archivo de firma.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public SignatureDocument[] Load(string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            {
                return Load(fs);
            }
        }

        /// <summary>
        /// Carga un archivo de firma.
        /// </summary>
        /// <param name="xmlDocument"></param>
        public SignatureDocument[] Load(XmlDocument xmlDocument)
        {
            XmlNodeList signatureNodeList = xmlDocument.GetElementsByTagName("Signature", SignedXml.XmlDsigNamespaceUrl);

            if (signatureNodeList.Count == 0)
            {
                throw new Exception("No se ha encontrado ninguna firma.");
            }

            List<SignatureDocument> firmas = new List<SignatureDocument>();

            foreach (var signatureNode in signatureNodeList)
            {
                SignatureDocument sigDocument = new SignatureDocument();
                sigDocument.Document = (XmlDocument)xmlDocument.Clone();
                sigDocument.Document.PreserveWhitespace = true;
                sigDocument.XadesSignature = new XadesSignedXml(sigDocument.Document);
                sigDocument.XadesSignature.LoadXml((XmlElement)signatureNode);

                firmas.Add(sigDocument);
            }
            
            return firmas.ToArray();
        }

        #endregion

        #region Validación

        /// <summary>
        /// Realiza la validación de una firma XAdES
        /// </summary>
        /// <param name="sigDocument"></param>
        /// <returns></returns>
        public ValidationResult Validate(SignatureDocument sigDocument)
        {
            SignatureDocument.CheckSignatureDocument(sigDocument);
            
            XadesValidator validator = new XadesValidator();

            return validator.Validate(sigDocument);
        }

        #endregion

        #endregion

        #region Private methods


        /// <summary>
        /// Establece el identificador para la firma
        /// </summary>
        private void SetSignatureId(XadesSignedXml xadesSignedXml)
        {
            string id = Guid.NewGuid().ToString();

            xadesSignedXml.Signature.Id = "Signature-" + id;
            xadesSignedXml.SignatureValueId = "SignatureValue-" + id;
        }

        /// <summary>
        /// Carga el documento XML especificado y establece para firmar el elemento especificado en elementId
        /// </summary>
        /// <param name="xmlDocument"></param>
        /// <param name="elementId"></param>
        /// <param name="mimeType"></param>
        private void SetContentInternallyDetached(SignatureDocument sigDocument, XmlDocument xmlDocument, string elementId, string mimeType)
        {
            sigDocument.Document = xmlDocument;

            _refContent = new Reference();

            _refContent.Uri = "#" + elementId;
            _refContent.Id = "Reference-" + Guid.NewGuid().ToString();

            _mimeType = mimeType;

            if (mimeType == "text/xml")
            {
                XmlDsigC14NTransform transform = new XmlDsigC14NTransform();
                _refContent.AddTransform(transform);

                _encoding = "UTF-8";
            }
            else
            {
                XmlDsigBase64Transform transform = new XmlDsigBase64Transform();
                _refContent.AddTransform(transform);

                _encoding = transform.Algorithm;
            }

            sigDocument.XadesSignature = new XadesSignedXml(sigDocument.Document);

            sigDocument.XadesSignature.AddReference(_refContent);
        }

        /// <summary>
        /// Inserta un documento para generar una firma internally detached.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="mimeType"></param>
        private void SetContentInternallyDetached(SignatureDocument sigDocument, Stream input, string mimeType)
        {
            sigDocument.Document = new XmlDocument();

            XmlElement rootElement = sigDocument.Document.CreateElement("DOCFIRMA");
            sigDocument.Document.AppendChild(rootElement);

            string id = "CONTENT-" + Guid.NewGuid().ToString();

            _refContent = new Reference();

            _refContent.Uri = "#" + id;
            _refContent.Id = "Reference-" + Guid.NewGuid().ToString();
            _refContent.Type = XadesSignedXml.XmlDsigObjectType;

            _mimeType = mimeType;

            XmlElement contentElement = sigDocument.Document.CreateElement("CONTENT");

            if (mimeType == "text/xml")
            {
                _encoding = "UTF-8";

                XmlDocument doc = new XmlDocument();
                doc.PreserveWhitespace = true;
                doc.Load(input);

                contentElement.InnerXml = doc.DocumentElement.OuterXml;

                XmlDsigC14NTransform transform = new XmlDsigC14NTransform();
                _refContent.AddTransform(transform);

            }
            else
            {
                XmlDsigBase64Transform transform = new XmlDsigBase64Transform();
                _refContent.AddTransform(transform);

                _encoding = transform.Algorithm;

                if (mimeType == "hash/sha256")
                {
                    using (SHA256 sha2 = SHA256.Create())
                    {
                        contentElement.InnerText = Convert.ToBase64String(sha2.ComputeHash(input));
                    }
                }
                else
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        input.CopyTo(ms);
                        contentElement.InnerText = Convert.ToBase64String(ms.ToArray());
                    }
                    
                }
            }

            contentElement.SetAttribute("Id", id);
            contentElement.SetAttribute("MimeType", _mimeType);
            contentElement.SetAttribute("Encoding", _encoding);


            rootElement.AppendChild(contentElement);

            sigDocument.XadesSignature = new XadesSignedXml(sigDocument.Document);

            sigDocument.XadesSignature.AddReference(_refContent);
        }

        /// <summary>
        /// Inserta un contenido XML para generar una firma enveloping.
        /// </summary>
        /// <param name="xmlDocument"></param>
        private void SetContentEveloping(SignatureDocument sigDocument, XmlDocument xmlDocument)
        {
            _refContent = new Reference();

            sigDocument.XadesSignature = new XadesSignedXml();

            XmlDocument doc = (XmlDocument)xmlDocument.Clone();
            doc.PreserveWhitespace = true;

            if (doc.ChildNodes[0].NodeType == XmlNodeType.XmlDeclaration)
            {
                doc.RemoveChild(doc.ChildNodes[0]);
            }

            //Add an object
            string dataObjectId = "DataObject-" + Guid.NewGuid().ToString();
            System.Security.Cryptography.Xml.DataObject dataObject = new System.Security.Cryptography.Xml.DataObject();
            dataObject.Data = doc.ChildNodes;
            dataObject.Id = dataObjectId;
            sigDocument.XadesSignature.AddObject(dataObject);

            _refContent.Id = "Reference-" + Guid.NewGuid().ToString();
            _refContent.Uri = "#" + dataObjectId;
            _refContent.Type = XadesSignedXml.XmlDsigObjectType; 

            XmlDsigC14NTransform transform = new XmlDsigC14NTransform();
            _refContent.AddTransform(transform);

            _mimeType = "text/xml";
            _encoding = "UTF-8";

            sigDocument.XadesSignature.AddReference(_refContent);
        }


        /// <summary>
        /// Especifica el nodo en el cual se añadira la firma
        /// </summary>
        /// <param name="elementXPath"></param>
        /// <param name="namespaces"></param>
        private void SetSignatureDestination(SignatureDocument sigDocument, SignatureXPathExpression destination)
        {
            XmlNode nodo;

            if (destination.Namespaces.Count > 0)
            {
                XmlNamespaceManager xmlnsMgr = new XmlNamespaceManager(sigDocument.Document.NameTable);
                foreach (var item in destination.Namespaces)
                {
                    xmlnsMgr.AddNamespace(item.Key, item.Value);
                }

                nodo = sigDocument.Document.SelectSingleNode(destination.XPathExpression, xmlnsMgr);
            }
            else
            {
                nodo = sigDocument.Document.SelectSingleNode(destination.XPathExpression);
            }

            if (nodo == null)
            {
                throw new Exception("Elemento no encontrado");
            }

            sigDocument.XadesSignature.SignatureNodeDestination = (XmlElement)nodo;
        }


        /// <summary>
        /// Inserta un documento para generar una firma externally detached.
        /// </summary>
        /// <param name="fileName"></param>
        private void SetContentExternallyDetached(SignatureDocument sigDocument, string fileName)
        {
            _refContent = new Reference();

            sigDocument.Document = new XmlDocument();
            sigDocument.XadesSignature = new XadesSignedXml(sigDocument.Document);

            _refContent.Uri = new Uri(fileName).AbsoluteUri;
            _refContent.Id = "Reference-" + Guid.NewGuid().ToString();

            if (_refContent.Uri.EndsWith(".xml") || _refContent.Uri.EndsWith(".XML"))
            {
                _mimeType = "text/xml";
                _refContent.AddTransform(new XmlDsigC14NTransform());
            }


            sigDocument.XadesSignature.AddReference(_refContent);
        }

        /// <summary>
        /// Añade una transformación XPath al contenido a firmar
        /// </summary>
        /// <param name="XPathString"></param>
        private void AddXPathTransform(SignatureDocument sigDocument, Dictionary<string, string> namespaces, string XPathString)
        {
            XmlDocument document;

            if (sigDocument.Document != null)
            {
                document = sigDocument.Document;
            }
            else
            {
                document = new XmlDocument();
            }

            XmlElement xPathElem = document.CreateElement("XPath");

            foreach (var ns in namespaces)
            {
                var attr = document.CreateAttribute("xmlns:" + ns.Key);
                attr.Value = ns.Value;

                xPathElem.Attributes.Append(attr);
            }

            xPathElem.InnerText = XPathString;

            XmlDsigXPathTransform transform = new XmlDsigXPathTransform();

            transform.LoadInnerXml(xPathElem.SelectNodes("."));

            Reference reference = sigDocument.XadesSignature.SignedInfo.References[0] as Reference;

            reference.AddTransform(transform);
        }


        /// <summary>
        /// Inserta un contenido XML para generar una firma enveloped.
        /// </summary>
        /// <param name="xmlDocument"></param>
        private void SetContentEnveloped(SignatureDocument sigDocument, XmlDocument xmlDocument)
        {
            sigDocument.Document = xmlDocument;

            _refContent = new Reference();

            sigDocument.XadesSignature = new XadesSignedXml(sigDocument.Document);

            _refContent.Id = "Reference-" + Guid.NewGuid().ToString();
            _refContent.Uri = "";

            _mimeType = "text/xml";
            _encoding = "UTF-8";

            for (int i = 0; i < sigDocument.Document.DocumentElement.Attributes.Count; i++)
            {
                if (sigDocument.Document.DocumentElement.Attributes[i].Name.Equals("id", StringComparison.InvariantCultureIgnoreCase))
                {
                    _refContent.Uri = "#" + sigDocument.Document.DocumentElement.Attributes[i].Value;
                    break;
                }
            }

            XmlDsigEnvelopedSignatureTransform xmlDsigEnvelopedSignatureTransform = new XmlDsigEnvelopedSignatureTransform();
            _refContent.AddTransform(xmlDsigEnvelopedSignatureTransform);


            sigDocument.XadesSignature.AddReference(_refContent);
        }

        private void PrepareSignature(SignatureDocument sigDocument, SignatureParameters parameters)
        {
            sigDocument.XadesSignature.SignedInfo.SignatureMethod = parameters.SignatureMethod.URI;

            AddCertificateInfo(sigDocument, parameters);
            AddXadesInfo(sigDocument, parameters);

            foreach (Reference reference in sigDocument.XadesSignature.SignedInfo.References)
            {
                reference.DigestMethod = parameters.DigestMethod.URI;
            }

            if (parameters.SignatureDestination != null)
            {
                SetSignatureDestination(sigDocument, parameters.SignatureDestination);
            }

            if (parameters.XPathTransformations.Count > 0)
            {
                foreach (var xPathTrans in parameters.XPathTransformations)
                {
                    AddXPathTransform(sigDocument, xPathTrans.Namespaces, xPathTrans.XPathExpression);
                }
            }
        }

        private void ComputeSignature(SignatureDocument sigDocument)
        {
            try
            {
                sigDocument.XadesSignature.ComputeSignature();

                XmlElement signatureElement = sigDocument.XadesSignature.GetXml();
                sigDocument.XadesSignature.LoadXml(signatureElement);
            }
            catch (Exception ex)
            {
                throw new Exception("Ha ocurrido un error durante el proceso de firmado", ex);
            }
        }

        #region Información y propiedades de la firma

        private void AddXadesInfo(SignatureDocument sigDocument, SignatureParameters parameters)
        {
            XadesObject xadesObject = new XadesObject();
            xadesObject.Id = "XadesObjectId-" + Guid.NewGuid().ToString();
            xadesObject.QualifyingProperties.Id = "QualifyingProperties-" + Guid.NewGuid().ToString();
            xadesObject.QualifyingProperties.Target = "#" + sigDocument.XadesSignature.Signature.Id;
            xadesObject.QualifyingProperties.SignedProperties.Id = "SignedProperties-" + sigDocument.XadesSignature.Signature.Id;

            AddSignatureProperties(sigDocument,
                xadesObject.QualifyingProperties.SignedProperties.SignedSignatureProperties,
                xadesObject.QualifyingProperties.SignedProperties.SignedDataObjectProperties,
                xadesObject.QualifyingProperties.UnsignedProperties.UnsignedSignatureProperties, parameters);

            sigDocument.XadesSignature.AddXadesObject(xadesObject);
        }


        private void AddCertificateInfo(SignatureDocument sigDocument, SignatureParameters parameters)
        {            
            sigDocument.XadesSignature.SigningKey = parameters.Signer.SigningKey;

            KeyInfo keyInfo = new KeyInfo();
            keyInfo.Id = "KeyInfoId-" + sigDocument.XadesSignature.Signature.Id;
            keyInfo.AddClause(new KeyInfoX509Data((X509Certificate)parameters.Signer.Certificate));
            keyInfo.AddClause(new RSAKeyValue((RSA)parameters.Signer.SigningKey));

            sigDocument.XadesSignature.KeyInfo = keyInfo;

            Reference reference = new Reference();

            reference.Id = "ReferenceKeyInfo";
            reference.Uri = "#KeyInfoId-" + sigDocument.XadesSignature.Signature.Id;

            sigDocument.XadesSignature.AddReference(reference);
        }


        private void AddSignatureProperties(SignatureDocument sigDocument, SignedSignatureProperties signedSignatureProperties, SignedDataObjectProperties signedDataObjectProperties,
                   UnsignedSignatureProperties unsignedSignatureProperties, SignatureParameters parameters)
        {
            Cert cert;

            cert = new Cert();
            cert.IssuerSerial.X509IssuerName = parameters.Signer.Certificate.IssuerName.Name;
            cert.IssuerSerial.X509SerialNumber = parameters.Signer.Certificate.GetSerialNumberAsDecimalString();
            DigestUtil.SetCertDigest(parameters.Signer.Certificate.GetRawCertData(), parameters.DigestMethod, cert.CertDigest);
            signedSignatureProperties.SigningCertificate.CertCollection.Add(cert);

            if (parameters.SignaturePolicyInfo != null)
            {
                if (!string.IsNullOrEmpty(parameters.SignaturePolicyInfo.PolicyIdentifier))
                {
                    signedSignatureProperties.SignaturePolicyIdentifier.SignaturePolicyImplied = false;
                    signedSignatureProperties.SignaturePolicyIdentifier.SignaturePolicyId.SigPolicyId.Identifier.IdentifierUri = parameters.SignaturePolicyInfo.PolicyIdentifier;
                }

                if (!string.IsNullOrEmpty(parameters.SignaturePolicyInfo.PolicyUri))
                {
                    SigPolicyQualifier spq = new SigPolicyQualifier();
                    spq.AnyXmlElement = sigDocument.Document.CreateElement(XadesSignedXml.XmlXadesPrefix, "SPURI", XadesSignedXml.XadesNamespaceUri);
                    spq.AnyXmlElement.InnerText = parameters.SignaturePolicyInfo.PolicyUri;

                    signedSignatureProperties.SignaturePolicyIdentifier.SignaturePolicyId.SigPolicyQualifiers.SigPolicyQualifierCollection.Add(spq);
                }

                if (!string.IsNullOrEmpty(parameters.SignaturePolicyInfo.PolicyHash))
                {
                    signedSignatureProperties.SignaturePolicyIdentifier.SignaturePolicyId.SigPolicyHash.DigestMethod.Algorithm = parameters.SignaturePolicyInfo.PolicyDigestAlgorithm.URI;
                    signedSignatureProperties.SignaturePolicyIdentifier.SignaturePolicyId.SigPolicyHash.DigestValue = Convert.FromBase64String(parameters.SignaturePolicyInfo.PolicyHash);
                }
            }

            signedSignatureProperties.SigningTime = parameters.SigningDate.HasValue ? parameters.SigningDate.Value : DateTime.Now;

            if (!string.IsNullOrEmpty(_mimeType))
            {
                DataObjectFormat newDataObjectFormat = new DataObjectFormat();

                newDataObjectFormat.MimeType = _mimeType;
                newDataObjectFormat.Encoding = _encoding;
                newDataObjectFormat.ObjectReferenceAttribute = "#" + _refContent.Id;

                signedDataObjectProperties.DataObjectFormatCollection.Add(newDataObjectFormat);
            }

            if (parameters.SignerRole != null &&
                (parameters.SignerRole.CertifiedRoles.Count > 0 || parameters.SignerRole.ClaimedRoles.Count > 0))
            {
                signedSignatureProperties.SignerRole = new Microsoft.Xades.SignerRole();

                foreach (X509Certificate certifiedRole in parameters.SignerRole.CertifiedRoles)
                {
                    signedSignatureProperties.SignerRole.CertifiedRoles.CertifiedRoleCollection.Add(new CertifiedRole() { PkiData = certifiedRole.GetRawCertData() });
                }

                foreach (string claimedRole in parameters.SignerRole.ClaimedRoles)
                {
                    signedSignatureProperties.SignerRole.ClaimedRoles.ClaimedRoleCollection.Add(new ClaimedRole() { InnerText = claimedRole });
                }
            }

        }

        #endregion

        #endregion
    }
}
