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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Xades;
using System.IO;
using Org.BouncyCastle.Tsp;
using System.Net;
using Org.BouncyCastle.Math;
using System.Collections;
using FirmaXadesNet;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.X509.Store;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Asn1.Cmp;
using Org.BouncyCastle.Asn1.X509;
using System.Reflection;
using FirmaXadesNet.Clients;
using FirmaXadesNet.Utils;
using FirmaXadesNet.Upgraders;
using FirmaXadesNet.Signature;
using FirmaXadesNet.Signature.Parameters;


namespace FirmaXadesNet
{

    public class XadesServices : IDisposable
    {

        #region Private variables
        private RSACryptoServiceProvider _rsaKey;
        private string _mimeType;
        private string _objectReference;

        private bool _disposeCryptoProvider;

        #endregion

        #region Public methods

        #region Métodos de firma

        /// <summary>
        /// Realiza el proceso de firmado
        /// </summary>
        /// <param name="certificate"></param>
        /// <param name="signMethod"></param>
        public SignatureDocument Sign(Stream input, SignatureParameters parameters)
        {           
            if (parameters.SigningCertificate == null)
            {
                throw new NullReferenceException("Es necesario un certificado válido para la firma.");
            }

            if (input == null && string.IsNullOrEmpty(parameters.ExternalContentUri))
            {
                throw new NullReferenceException("No se ha especificado el contenido a firmar.");
            }

            SignatureDocument signatureDocument = new SignatureDocument();
            XmlDocument sourceDocument = null;

            switch (parameters.Packaging)
            {
                case SignaturePackaging.INTERNALLY_DETACHED:
                    if (string.IsNullOrEmpty(parameters.InputMimeType))
                    {
                        throw new NullReferenceException("Se necesita especificar el tipo MIME del elemento a firmar.");
                    }

                    if (!string.IsNullOrEmpty(parameters.ElementIdToSign))
                    {
                        sourceDocument = new XmlDocument();
                        sourceDocument.Load(input);

                        SetContentInternallyDetached(signatureDocument, sourceDocument, parameters.ElementIdToSign, parameters.InputMimeType);
                    }
                    else
                    {

                        SetContentInternallyDetached(signatureDocument, input, parameters.InputMimeType);
                    }
                    break;

                case SignaturePackaging.ENVELOPED:
                    sourceDocument = new XmlDocument();
                    sourceDocument.Load(input);

                    SetContentEnveloped(signatureDocument, sourceDocument);
                    break;

                case SignaturePackaging.ENVELOPING:
                    sourceDocument = new XmlDocument();
                    sourceDocument.Load(input);

                    SetContentEveloping(signatureDocument, sourceDocument);
                    break;

                case SignaturePackaging.EXTERNALLY_DETACHED:
                    SetContentExternallyDetached(signatureDocument, parameters.ExternalContentUri);
                    break;
            }

            SetSignatureId(signatureDocument.XadesSignature);

            PrepareSignature(signatureDocument, parameters);

            ComputeSignature(signatureDocument);

            XMLUtil.UpdateDocument(signatureDocument);

            return signatureDocument;
        }

        /// <summary>
        /// Añade una firma al documento
        /// </summary>
        /// <param name="certificate"></param>
        /// <param name="signMethod"></param>
        public SignatureDocument CoSign(SignatureDocument sigDocument, SignatureParameters parameters)
        {
            Reference refContent = sigDocument.XadesSignature.SignedInfo.References[0] as Reference;

            if (refContent == null)
            {
                throw new Exception("No se ha podido encontrar la referencia del contenido firmado.");
            }

            if (sigDocument.XadesSignature.XadesObject.QualifyingProperties.SignedProperties.SignedDataObjectProperties.DataObjectFormatCollection.Count > 0)
            {
                foreach (DataObjectFormat dof in sigDocument.XadesSignature.XadesObject.QualifyingProperties.SignedProperties.SignedDataObjectProperties.DataObjectFormatCollection)
                {
                    if (dof.ObjectReferenceAttribute == ("#" + refContent.Id))
                    {
                        _mimeType = dof.MimeType;
                        break;
                    }
                }
            }

            SignatureDocument coSignatureDocument = new SignatureDocument();
            coSignatureDocument.Document = (XmlDocument)sigDocument.Document.Clone();

            coSignatureDocument.XadesSignature = new XadesSignedXml(coSignatureDocument.Document);
            coSignatureDocument.XadesSignature.LoadXml(sigDocument.XadesSignature.GetXml());

            var destination = coSignatureDocument.XadesSignature.GetSignatureElement().ParentNode;

            coSignatureDocument.XadesSignature = new XadesSignedXml(coSignatureDocument.Document);

            refContent.Id = "Reference-" + Guid.NewGuid().ToString();
            coSignatureDocument.XadesSignature.AddReference(refContent);

            if (destination.NodeType != XmlNodeType.Document)
            {
                coSignatureDocument.XadesSignature.SignatureNodeDestination = (XmlElement)destination;
            }
            else
            {
                coSignatureDocument.XadesSignature.SignatureNodeDestination = ((XmlDocument)destination).DocumentElement;
            }

            _objectReference = refContent.Id;

            SetSignatureId(coSignatureDocument.XadesSignature);

            PrepareSignature(coSignatureDocument, parameters);

            ComputeSignature(coSignatureDocument);

            XMLUtil.UpdateDocument(coSignatureDocument);

            return coSignatureDocument;
        }


        /// <summary>
        /// Realiza la contrafirma de la firma actual
        /// </summary>
        /// <param name="certificate"></param>
        /// <param name="signMethod"></param>
        public SignatureDocument CounterSign(SignatureDocument sigDocument, SignatureParameters parameters)
        {            
            if (parameters.SigningCertificate == null)
            {
                throw new Exception("Es necesario un certificado válido para la firma.");
            }

            SignatureDocument counterSigDocument = new SignatureDocument();
            counterSigDocument.Document = (XmlDocument)sigDocument.Document.Clone();
            XadesSignedXml counterSignature = new XadesSignedXml(counterSigDocument.Document);
            SetSignatureId(counterSignature);
            
            SetCryptoServiceProvider(parameters.SigningCertificate);

            counterSignature.SigningKey = _rsaKey;

            Reference reference = new Reference();
            reference.Uri = "#" + sigDocument.XadesSignature.SignatureValueId;
            reference.Id = "Reference-" + Guid.NewGuid().ToString();
            reference.Type = "http://uri.etsi.org/01903#CountersignedSignature";
            reference.AddTransform(new XmlDsigC14NTransform());
            counterSignature.AddReference(reference);
            _objectReference = reference.Id;

            KeyInfo keyInfo = new KeyInfo();
            keyInfo.Id = "KeyInfoId-" + counterSignature.Signature.Id;
            keyInfo.AddClause(new KeyInfoX509Data((X509Certificate)parameters.SigningCertificate));
            keyInfo.AddClause(new RSAKeyValue((RSA)_rsaKey));
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
                counterSignatureXadesObject.QualifyingProperties.UnsignedProperties.UnsignedSignatureProperties,
                "text/xml", parameters);

            counterSignature.AddXadesObject(counterSignatureXadesObject);

            foreach (Reference signReference in counterSignature.SignedInfo.References)
            {
                signReference.DigestMethod = parameters.DigestMethod.URI;
            }

            counterSignature.AddXadesNamespace = true;
            counterSignature.ComputeSignature();

            counterSigDocument.XadesSignature = new XadesSignedXml(counterSigDocument.Document);
            counterSigDocument.XadesSignature.LoadXml(sigDocument.XadesSignature.GetXml());

            UnsignedProperties unsignedProperties = counterSigDocument.XadesSignature.UnsignedProperties;
            unsignedProperties.UnsignedSignatureProperties.CounterSignatureCollection.Add(counterSignature);
            counterSigDocument.XadesSignature.UnsignedProperties = unsignedProperties;

            XMLUtil.UpdateDocument(counterSigDocument);

            return counterSigDocument;
        }

        #endregion

        #region Guardado y carga de firma


        /// <summary>
        /// Carga un archivo de firma.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static SignatureDocument[] Load(Stream input)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.PreserveWhitespace = true;
            xmlDocument.Load(input);

            return Load(xmlDocument);
        }

        /// <summary>
        /// Carga un archivo de firma.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static SignatureDocument[] Load(string fileName)
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
        public static SignatureDocument[] Load(XmlDocument xmlDocument)
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
                sigDocument.Document = xmlDocument;
                sigDocument.XadesSignature = new XadesSignedXml(xmlDocument);
                sigDocument.XadesSignature.LoadXml((XmlElement)signatureNode);

                firmas.Add(sigDocument);
            }

            return firmas.ToArray();
        }

        #endregion

        public void Dispose()
        {
            if (_disposeCryptoProvider && _rsaKey != null)
            {
                _rsaKey.Dispose();
            }
        }

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
            sigDocument.Document = (XmlDocument)xmlDocument.Clone();
            sigDocument.Document.PreserveWhitespace = true;

            Reference reference = new Reference();

            reference.Uri = "#" + elementId;
            reference.Id = "Reference-" + Guid.NewGuid().ToString();

            _objectReference = reference.Id;
            _mimeType = mimeType;

            if (mimeType == "text/xml")
            {
                XmlDsigC14NTransform transform = new XmlDsigC14NTransform();
                reference.AddTransform(transform);
            }
            else
            {
                XmlDsigBase64Transform transform = new XmlDsigBase64Transform();
                reference.AddTransform(transform);
            }

            sigDocument.XadesSignature = new XadesSignedXml(sigDocument.Document);

            sigDocument.XadesSignature.AddReference(reference);
        }

        /// <summary>
        /// Inserta un documento para generar una firma internally detached.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="mimeType"></param>
        private void SetContentInternallyDetached(SignatureDocument sigDocument, byte[] content, string mimeType)
        {
            sigDocument.Document = new XmlDocument();

            XmlElement rootElement = sigDocument.Document.CreateElement("DOCFIRMA");
            sigDocument.Document.AppendChild(rootElement);

            string id = "CONTENT-" + Guid.NewGuid().ToString();

            Reference reference = new Reference();

            reference.Uri = "#" + id;
            reference.Id = "Reference-" + Guid.NewGuid().ToString();

            _objectReference = reference.Id;
            _mimeType = mimeType;

            XmlElement contentElement = sigDocument.Document.CreateElement("CONTENT");

            if (mimeType == "text/xml")
            {
                XmlDocument doc = new XmlDocument();
                doc.PreserveWhitespace = true;
                doc.Load(new MemoryStream(content));

                contentElement.InnerXml = doc.DocumentElement.OuterXml;

                XmlDsigC14NTransform transform = new XmlDsigC14NTransform();
                reference.AddTransform(transform);
            }
            else if (mimeType == "hash/sha256")
            {
                contentElement.SetAttribute("Encoding", "http://www.w3.org/2000/09/xmldsig#base64");
                contentElement.SetAttribute("MimeType", mimeType);

                using (SHA256 sha2 = SHA256.Create())
                {
                    contentElement.InnerText = Convert.ToBase64String(sha2.ComputeHash(content));
                }

                XmlDsigBase64Transform transform = new XmlDsigBase64Transform();
                reference.AddTransform(transform);
            }
            else
            {
                contentElement.SetAttribute("Encoding", "http://www.w3.org/2000/09/xmldsig#base64");
                contentElement.InnerText = Convert.ToBase64String(content);

                XmlDsigBase64Transform transform = new XmlDsigBase64Transform();
                reference.AddTransform(transform);
            }

            contentElement.SetAttribute("Id", id);

            rootElement.AppendChild(contentElement);

            sigDocument.XadesSignature = new XadesSignedXml(sigDocument.Document);

            sigDocument.XadesSignature.AddReference(reference);
        }


        /// <summary>
        /// Inserta un documento para generar una firma internally detached.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="mimeType"></param>
        private void SetContentInternallyDetached(SignatureDocument sigDocument, Stream input, string mimeType)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                SetContentInternallyDetached(sigDocument, ms.ToArray(), mimeType);
            }
        }


        /// <summary>
        /// Inserta un contenido XML para generar una firma enveloped.
        /// </summary>
        /// <param name="xmlDocument"></param>
        private void SetContentEnveloped(SignatureDocument sigDocument, XmlDocument xmlDocument)
        {
            sigDocument.Document = (XmlDocument)xmlDocument.Clone();
            sigDocument.Document.PreserveWhitespace = true;

            CreateEnvelopedDocument(sigDocument);
        }

        /// <summary>
        /// Inserta un contenido XML para generar una firma enveloping.
        /// </summary>
        /// <param name="xmlDocument"></param>
        private void SetContentEveloping(SignatureDocument sigDocument, XmlDocument xmlDocument)
        {
            Reference reference = new Reference();

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

            reference.Id = "Reference-" + Guid.NewGuid().ToString();
            reference.Uri = "#" + dataObjectId;
            reference.Type = SignedXml.XmlDsigNamespaceUrl + "Object";

            XmlDsigC14NTransform transform = new XmlDsigC14NTransform();
            reference.AddTransform(transform);

            _objectReference = reference.Id;
            _mimeType = "text/xml";

            sigDocument.XadesSignature.AddReference(reference);
        }


        /// <summary>
        /// Especifica el nodo en el cual se añadira la firma
        /// </summary>
        /// <param name="elementXPath"></param>
        /// <param name="namespaces"></param>
        private void SetSignatureDestination(SignatureDocument sigDocument, SignatureDestination destination)
        {
            XmlNode nodo;

            if (destination.Namespaces.Count > 0)
            {
                XmlNamespaceManager xmlnsMgr = new XmlNamespaceManager(sigDocument.Document.NameTable);
                foreach (var item in destination.Namespaces)
                {
                    xmlnsMgr.AddNamespace(item.Key, item.Value);
                }

                nodo = sigDocument.Document.SelectSingleNode(destination.XPathElement, xmlnsMgr);
            }
            else
            {
                nodo = sigDocument.Document.SelectSingleNode(destination.XPathElement);
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
            Reference reference = new Reference();

            sigDocument.Document = new XmlDocument();
            sigDocument.XadesSignature = new XadesSignedXml();

            reference.Uri = "file://" + fileName.Replace("\\", "/");
            reference.Id = "Reference-" + Guid.NewGuid().ToString();

            if (reference.Uri.EndsWith(".xml") || reference.Uri.EndsWith(".XML"))
            {
                _mimeType = "text/xml";
                reference.AddTransform(new XmlDsigC14NTransform());
            }

            _objectReference = reference.Id;

            sigDocument.XadesSignature.AddReference(reference);
        }

        /// <summary>
        /// Añade una transformación XPath al contenido a firmar
        /// </summary>
        /// <param name="XPathString"></param>
        private void AddXPathTransform(SignatureDocument sigDocument, string XPathString)
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
            xPathElem.InnerText = XPathString;

            XmlDsigXPathTransform transform = new XmlDsigXPathTransform();
            transform.LoadInnerXml(xPathElem.SelectNodes("."));

            Reference reference = sigDocument.XadesSignature.SignedInfo.References[0] as Reference;

            reference.AddTransform(transform);
        }


        /// <summary>
        /// Construye el documento enveloped
        /// </summary>
        private void CreateEnvelopedDocument(SignatureDocument sigDocument)
        {
            Reference reference = new Reference();

            sigDocument.XadesSignature = new XadesSignedXml(sigDocument.Document);

            reference.Id = "Reference-" + Guid.NewGuid().ToString();
            reference.Uri = "";

            for (int i = 0; i < sigDocument.Document.DocumentElement.Attributes.Count; i++)
            {
                if (sigDocument.Document.DocumentElement.Attributes[i].Name.Equals("id", StringComparison.InvariantCultureIgnoreCase))
                {
                    reference.Uri = "#" + sigDocument.Document.DocumentElement.Attributes[i].Value;
                    break;
                }
            }

            XmlDsigEnvelopedSignatureTransform xmlDsigEnvelopedSignatureTransform = new XmlDsigEnvelopedSignatureTransform();
            reference.AddTransform(xmlDsigEnvelopedSignatureTransform);

            _objectReference = reference.Id;

            sigDocument.XadesSignature.AddReference(reference);
        }

        private void PrepareSignature(SignatureDocument signatureDocument, SignatureParameters parameters)
        {
            AddCertificateInfo(signatureDocument, parameters);
            AddXadesInfo(signatureDocument, parameters);

            foreach (Reference reference in signatureDocument.XadesSignature.SignedInfo.References)
            {
                reference.DigestMethod = parameters.DigestMethod.URI;
            }

            signatureDocument.XadesSignature.SignedInfo.SignatureMethod = parameters.SignatureMethod.URI;

            if (parameters.SignatureDestination != null)
            {
                SetSignatureDestination(signatureDocument, parameters.SignatureDestination);
            }

            if (parameters.XPathTransformations.Count > 0)
            {
                foreach (var xPathTrans in parameters.XPathTransformations)
                {
                    AddXPathTransform(signatureDocument, xPathTrans);
                }
            }
        }

        private void ComputeSignature(SignatureDocument sigDocument)
        {
            try
            {
                sigDocument.XadesSignature.ComputeSignature();
            }
            catch (Exception exception)
            {
                throw new Exception("Ha ocurrido durante el proceso de firmado: " + exception.Message);
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
                xadesObject.QualifyingProperties.UnsignedProperties.UnsignedSignatureProperties,
                _mimeType, parameters);

            sigDocument.XadesSignature.AddXadesObject(xadesObject);
        }


        private void SetCryptoServiceProvider(X509Certificate2 certificate)
        {
            string providerName = "Microsoft Enhanced RSA and AES Cryptographic Provider";
            int providerType = 24;

            var key = (RSACryptoServiceProvider)certificate.PrivateKey;

            if (_rsaKey != null &&
                key.CspKeyContainerInfo.UniqueKeyContainerName == _rsaKey.CspKeyContainerInfo.UniqueKeyContainerName)
            {
                return;
            }
            else if (_rsaKey != null && _disposeCryptoProvider)
            {
                _rsaKey.Dispose();
            }


            if (key.CspKeyContainerInfo.ProviderName == "Microsoft Strong Cryptographic Provider" ||
                key.CspKeyContainerInfo.ProviderName == "Microsoft Enhanced Cryptographic Provider v1.0" ||
                key.CspKeyContainerInfo.ProviderName == "Microsoft Base Cryptographic Provider v1.0")
            {
                Type CspKeyContainerInfo_Type = typeof(CspKeyContainerInfo);

                FieldInfo CspKeyContainerInfo_m_parameters = CspKeyContainerInfo_Type.GetField("m_parameters", BindingFlags.NonPublic | BindingFlags.Instance);
                CspParameters parameters = (CspParameters)CspKeyContainerInfo_m_parameters.GetValue(key.CspKeyContainerInfo);

                var cspparams = new CspParameters(providerType, providerName, key.CspKeyContainerInfo.KeyContainerName);
                cspparams.Flags = parameters.Flags;
                _rsaKey = new RSACryptoServiceProvider(cspparams);

                _disposeCryptoProvider = true;
            }
            else
            {
                _rsaKey = key;
                _disposeCryptoProvider = false;
            }
        }


        private void AddCertificateInfo(SignatureDocument sigDocument, SignatureParameters parameters)
        {
            SetCryptoServiceProvider(parameters.SigningCertificate);

            sigDocument.XadesSignature.SigningKey = _rsaKey;

            KeyInfo keyInfo = new KeyInfo();
            keyInfo.Id = "KeyInfoId-" + sigDocument.XadesSignature.Signature.Id;
            keyInfo.AddClause(new KeyInfoX509Data((X509Certificate)parameters.SigningCertificate));
            keyInfo.AddClause(new RSAKeyValue((RSA)_rsaKey));

            sigDocument.XadesSignature.KeyInfo = keyInfo;

            Reference reference = new Reference();

            reference.Id = "ReferenceKeyInfo";
            reference.Uri = "#KeyInfoId-" + sigDocument.XadesSignature.Signature.Id;

            sigDocument.XadesSignature.AddReference(reference);
        }


        private void AddSignatureProperties(SignatureDocument sigDocument, SignedSignatureProperties signedSignatureProperties, SignedDataObjectProperties signedDataObjectProperties,
                   UnsignedSignatureProperties unsignedSignatureProperties, string mimeType, SignatureParameters parameters)
        {
            Cert cert;

            cert = new Cert();
            cert.IssuerSerial.X509IssuerName = parameters.SigningCertificate.IssuerName.Name;
            cert.IssuerSerial.X509SerialNumber = parameters.SigningCertificate.GetSerialNumberAsDecimalString();
            DigestUtil.SetCertDigest(parameters.SigningCertificate.GetRawCertData(), parameters.DigestMethod, cert.CertDigest);
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
                    spq.AnyXmlElement = sigDocument.Document.CreateElement("SPURI", XadesSignedXml.XadesNamespaceUri);
                    spq.AnyXmlElement.InnerText = parameters.SignaturePolicyInfo.PolicyUri;

                    signedSignatureProperties.SignaturePolicyIdentifier.SignaturePolicyId.SigPolicyQualifiers.SigPolicyQualifierCollection.Add(spq);
                }

                if (!string.IsNullOrEmpty(parameters.SignaturePolicyInfo.PolicyHash))
                {
                    signedSignatureProperties.SignaturePolicyIdentifier.SignaturePolicyId.SigPolicyHash.DigestMethod.Algorithm = SignedXml.XmlDsigSHA1Url;
                    signedSignatureProperties.SignaturePolicyIdentifier.SignaturePolicyId.SigPolicyHash.DigestValue = Convert.FromBase64String(parameters.SignaturePolicyInfo.PolicyHash);
                }
            }
            
            signedSignatureProperties.SigningTime = parameters.SigningDate.HasValue ? parameters.SigningDate.Value : DateTime.Now;

            if (!string.IsNullOrEmpty(mimeType))
            {
                DataObjectFormat newDataObjectFormat = new DataObjectFormat();

                newDataObjectFormat.MimeType = mimeType;
                newDataObjectFormat.ObjectReferenceAttribute = "#" + _objectReference;

                signedDataObjectProperties.DataObjectFormatCollection.Add(newDataObjectFormat);
            }

        }

        #endregion

        #endregion
    }
}
