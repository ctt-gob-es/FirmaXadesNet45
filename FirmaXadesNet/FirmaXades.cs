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
using FirmaXadesNet.Parameters;


namespace FirmaXadesNet
{

    public class FirmaXades : IDisposable
    {

        #region Private variables
        private XadesSignedXml _xadesSignedXml;
        private XmlDocument _document;
        private RSACryptoServiceProvider _rsaKey;
        private string _mimeType;
        private string _signatureId;
        private string _signatureValueId;
        private string _objectReference;

        private bool _disposeCryptoProvider;

        #endregion

        #region Public variables
        /// <summary>
        /// Documento XML resultante
        /// </summary>
        public XmlDocument Document
        {
            get
            {
                return _document;
            }
        }

        /// <summary>
        /// Bytes del documento XML resultante
        /// </summary>
        public byte[] RawDocument
        {
            get
            {
                if (_document != null)
                {
                    return UTF8Encoding.UTF8.GetBytes(_document.OuterXml);
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Objeto que encapsula la firma XADES
        /// </summary>
        public XadesSignedXml XadesSignature
        {
            get
            {
                return _xadesSignedXml;
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Carga el documento XML especificado y establece para firmar el elemento especificado en elementId
        /// </summary>
        /// <param name="xmlDocument"></param>
        /// <param name="elementId"></param>
        /// <param name="mimeType"></param>
        public void SetContentInternallyDetached(XmlDocument xmlDocument, string elementId, string mimeType)
        {
            _document = (XmlDocument)xmlDocument.Clone();
            _document.PreserveWhitespace = true;

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

            _xadesSignedXml = new XadesSignedXml(_document);

            _xadesSignedXml.AddReference(reference);
        }

        /// <summary>
        /// Inserta un documento para generar una firma internally detached.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="mimeType"></param>
        public void SetContentInternallyDetached(byte[] content, string mimeType, string fileName = null)
        {
            _document = new XmlDocument();

            XmlElement rootElement = _document.CreateElement("DOCFIRMA");
            _document.AppendChild(rootElement);

            string id = "CONTENT-" + Guid.NewGuid().ToString();

            Reference reference = new Reference();

            reference.Uri = "#" + id;
            reference.Id = "Reference-" + Guid.NewGuid().ToString();

            _objectReference = reference.Id;
            _mimeType = mimeType;

            XmlElement contentElement = _document.CreateElement("CONTENT");

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

                if (!string.IsNullOrEmpty(fileName))
                {
                    contentElement.SetAttribute("URI", Path.GetFileName(fileName));
                }

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

            _xadesSignedXml = new XadesSignedXml(_document);

            _xadesSignedXml.AddReference(reference);
        }

        /// <summary>
        /// Inserta un documento para generar una firma internally detached.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="mimeType"></param>
        public void SetContentInternallyDetached(string fileName, string mimeType)
        {
            byte[] content = File.ReadAllBytes(fileName);

            if (mimeType.StartsWith("hash"))
            {
                SetContentInternallyDetached(content, mimeType, fileName);
            }
            else
            {
                SetContentInternallyDetached(content, mimeType);
            }
        }

        /// <summary>
        /// Inserta un documento para generar una firma internally detached.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="mimeType"></param>
        public void SetContentInternallyDetached(Stream input, string mimeType)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                SetContentInternallyDetached(ms.ToArray(), mimeType);
            }
        }

        /// <summary>
        /// Inserta un documento para generar una firma externally detached.
        /// </summary>
        /// <param name="fileName"></param>
        public void SetContentExternallyDetached(string fileName)
        {
            Reference reference = new Reference();

            _document = new XmlDocument();
            _xadesSignedXml = new XadesSignedXml();

            reference.Uri = "file://" + fileName.Replace("\\", "/");
            reference.Id = "Reference-" + Guid.NewGuid().ToString();

            if (reference.Uri.EndsWith(".xml") || reference.Uri.EndsWith(".XML"))
            {
                _mimeType = "text/xml";
                reference.AddTransform(new XmlDsigC14NTransform());
            }

            _objectReference = reference.Id;

            _xadesSignedXml.AddReference(reference);
        }

        /// <summary>
        /// Inserta un documento XML para generar una firma enveloped.
        /// </summary>
        /// <param name="fileName"></param>
        public void SetContentEnveloped(string fileName)
        {
            _document = new XmlDocument();
            _document.PreserveWhitespace = true;
            _document.Load(fileName);

            CreateEnvelopedDocument();
        }

        /// <summary>
        /// Inserta un contenido XML para generar una firma enveloped.
        /// </summary>
        /// <param name="xmlDocument"></param>
        public void SetContentEnveloped(XmlDocument xmlDocument)
        {
            _document = (XmlDocument)xmlDocument.Clone();
            _document.PreserveWhitespace = true;

            CreateEnvelopedDocument();
        }

        /// <summary>
        /// Inserta un contenido XML para generar una firma enveloping.
        /// </summary>
        /// <param name="xmlDocument"></param>
        public void SetContentEveloping(XmlDocument xmlDocument)
        {
            Reference reference = new Reference();

            _xadesSignedXml = new XadesSignedXml();

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
            _xadesSignedXml.AddObject(dataObject);

            reference.Id = "Reference-" + Guid.NewGuid().ToString();
            reference.Uri = "#" + dataObjectId;
            reference.Type = SignedXml.XmlDsigNamespaceUrl + "Object";

            XmlDsigC14NTransform transform = new XmlDsigC14NTransform();
            reference.AddTransform(transform);

            _objectReference = reference.Id;
            _mimeType = "text/xml";

            _xadesSignedXml.AddReference(reference);

            _document = null;
        }


        public X509Certificate2 GetSigningCertificate()
        {
            XmlNode keyXml = _xadesSignedXml.KeyInfo.GetXml().GetElementsByTagName("X509Certificate", SignedXml.XmlDsigNamespaceUrl)[0];

            return new X509Certificate2(Convert.FromBase64String(keyXml.InnerText));
        }

        #region Métodos de firma

        /// <summary>
        /// Realiza el proceso de firmado
        /// </summary>
        /// <param name="certificate"></param>
        /// <param name="signMethod"></param>
        public void Sign(SignatureParameters parameters)
        {
            if (parameters.SigningCertificate == null)
            {
                throw new Exception("Es necesario un certificado válido para la firma.");
            }

            if (string.IsNullOrEmpty(_signatureId))
            {
                SetSignatureId();
            }
            
            AddCertificateInfo(parameters);
            AddXadesInfo(parameters);

            foreach (Reference reference in _xadesSignedXml.SignedInfo.References)
            {
                reference.DigestMethod = parameters.DigestMethod.URI;
            }

            _xadesSignedXml.SignedInfo.SignatureMethod = parameters.SignatureMethod.URI;

            if (parameters.SignatureDestination != null)
            {
                SetSignatureDestination(parameters.SignatureDestination);
            }

            if (parameters.XPathTransformations.Count > 0)
            {
                foreach (var xPathTrans in parameters.XPathTransformations)
                {
                    AddXPathTransform(xPathTrans);
                }
            }

            ComputeSignature();

            UpdateDocument();

            XmlNode xmlNode = _document.SelectSingleNode("//*[@Id='" + _signatureId + "']");
            _xadesSignedXml = new XadesSignedXml(_document);
            _xadesSignedXml.LoadXml((XmlElement)xmlNode);
        }

        /// <summary>
        /// Añade una firma al documento
        /// </summary>
        /// <param name="certificate"></param>
        /// <param name="signMethod"></param>
        public void CoSign(SignatureParameters parameters)
        {
            if (_xadesSignedXml == null)
            {
                throw new Exception("No hay ninguna firma XADES creada previamente.");
            }

            Reference refContent = _xadesSignedXml.SignedInfo.References[0] as Reference;

            if (refContent == null)
            {
                throw new Exception("No se ha podido encontrar la referencia del contenido firmado.");
            }

            if (_xadesSignedXml.XadesObject.QualifyingProperties.SignedProperties.SignedDataObjectProperties.DataObjectFormatCollection.Count > 0)
            {
                foreach (DataObjectFormat dof in _xadesSignedXml.XadesObject.QualifyingProperties.SignedProperties.SignedDataObjectProperties.DataObjectFormatCollection)
                {
                    if (dof.ObjectReferenceAttribute == ("#" + refContent.Id))
                    {
                        _mimeType = dof.MimeType;
                        break;
                    }
                }
            }

            var destination = _xadesSignedXml.GetSignatureElement().ParentNode;

            _xadesSignedXml = new XadesSignedXml(_document);

            refContent.Id = "Reference-" + Guid.NewGuid().ToString();
            _xadesSignedXml.AddReference(refContent);

            if (destination.NodeType != XmlNodeType.Document)
            {
                _xadesSignedXml.SignatureNodeDestination = (XmlElement)destination;
            }
            else
            {
                _xadesSignedXml.SignatureNodeDestination = ((XmlDocument)destination).DocumentElement;
            }

            _objectReference = refContent.Id;

            SetSignatureId();

            Sign(parameters);
        }


        /// <summary>
        /// Realiza la contrafirma de la firma actual
        /// </summary>
        /// <param name="certificate"></param>
        /// <param name="signMethod"></param>
        public void CounterSign(SignatureParameters parameters)
        {
            SetSignatureId();

            if (_xadesSignedXml == null)
            {
                throw new Exception("No hay ninguna firma XADES cargada previamente.");
            }

            if (parameters.SigningCertificate == null)
            {
                throw new Exception("Es necesario un certificado válido para la firma.");
            }

            XadesSignedXml counterSignature = new XadesSignedXml(_document);

            SetCryptoServiceProvider(parameters.SigningCertificate);

            counterSignature.SigningKey = _rsaKey;

            Reference reference = new Reference();
            reference.Uri = "#" + _xadesSignedXml.SignatureValueId;
            reference.Id = "Reference-" + Guid.NewGuid().ToString();
            reference.Type = "http://uri.etsi.org/01903#CountersignedSignature";
            reference.AddTransform(new XmlDsigC14NTransform());
            counterSignature.AddReference(reference);
            _objectReference = reference.Id;

            KeyInfo keyInfo = new KeyInfo();
            keyInfo.Id = "KeyInfoId-" + _signatureId;
            keyInfo.AddClause(new KeyInfoX509Data((X509Certificate)parameters.SigningCertificate));
            keyInfo.AddClause(new RSAKeyValue((RSA)_rsaKey));
            counterSignature.KeyInfo = keyInfo;

            Reference referenceKeyInfo = new Reference();
            referenceKeyInfo.Id = "ReferenceKeyInfo-" + _signatureId;
            referenceKeyInfo.Uri = "#KeyInfoId-" + _signatureId;
            counterSignature.AddReference(referenceKeyInfo);

            counterSignature.Signature.Id = _signatureId;
            counterSignature.SignatureValueId = _signatureValueId;

            XadesObject counterSignatureXadesObject = new XadesObject();
            counterSignatureXadesObject.Id = "CounterSignatureXadesObject-" + Guid.NewGuid().ToString();
            counterSignatureXadesObject.QualifyingProperties.Target = "#" + _signatureId;
            counterSignatureXadesObject.QualifyingProperties.SignedProperties.Id = "SignedProperties-" + _signatureId;

            AddSignatureProperties(counterSignatureXadesObject.QualifyingProperties.SignedProperties.SignedSignatureProperties,
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

            UnsignedProperties unsignedProperties = _xadesSignedXml.UnsignedProperties;
            unsignedProperties.UnsignedSignatureProperties.CounterSignatureCollection.Add(counterSignature);
            _xadesSignedXml.UnsignedProperties = unsignedProperties;

            UpdateDocument();

            _xadesSignedXml = new XadesSignedXml(_document);

            XmlNode xmlNode = _document.SelectSingleNode("//*[@Id='" + _signatureId + "']");

            _xadesSignedXml.LoadXml((XmlElement)xmlNode);
        }

        #endregion

        #region Guardado y carga de firma

        /// <summary>
        /// Guardar la firma en el fichero especificado.
        /// </summary>
        /// <param name="fileName"></param>
        public void Save(string fileName)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = new UTF8Encoding();
            using (var writer = XmlWriter.Create(fileName, settings))
            {
                this.Document.Save(writer);
            }
        }

        /// <summary>
        /// Guarda la firma en el destino especificado
        /// </summary>
        /// <param name="output"></param>
        public void Save(Stream output)
        {
            this.Document.Save(output);
        }


        /// <summary>
        /// Carga un archivo de firma.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static FirmaXades[] Load(Stream input)
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
        public static FirmaXades[] Load(string fileName)
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
        public static FirmaXades[] Load(XmlDocument xmlDocument)
        {
            XmlNodeList signatureNodeList = xmlDocument.GetElementsByTagName("Signature", SignedXml.XmlDsigNamespaceUrl);

            if (signatureNodeList.Count == 0)
            {
                throw new Exception("No se ha encontrado ninguna firma.");
            }

            List<FirmaXades> firmas = new List<FirmaXades>();

            foreach (var signatureNode in signatureNodeList)
            {
                FirmaXades firma = new FirmaXades();
                firma._document = xmlDocument;

                firma._xadesSignedXml = new XadesSignedXml(firma._document);
                firma._xadesSignedXml.LoadXml((XmlElement)signatureNode);
                firma._xadesSignedXml.FindContentElement();

                firmas.Add(firma);
            }

            return firmas.ToArray();
        }
        #endregion

        /// <summary>
        /// Selecciona un certificado del almacén de certificados
        /// </summary>
        /// <returns></returns>
        public X509Certificate2 SelectCertificate(string message = null, string title = null)
        {
            X509Certificate2 cert = null;

            try
            {
                // Open the store of personal certificates.
                X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

                X509Certificate2Collection collection = (X509Certificate2Collection)store.Certificates;
                X509Certificate2Collection fcollection = (X509Certificate2Collection)collection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);

                if (string.IsNullOrEmpty(message))
                {
                    message = "Seleccione un certificado.";
                }

                if (string.IsNullOrEmpty(title))
                {
                    title = "Firmar archivo";
                }

                X509Certificate2Collection scollection = X509Certificate2UI.SelectFromCollection(fcollection, title, message, X509SelectionFlag.SingleSelection);

                if (scollection != null && scollection.Count == 1)
                {
                    cert = scollection[0];

                    if (cert.HasPrivateKey == false)
                    {
                        throw new Exception("El certificado no tiene asociada una clave privada.");
                    }
                }

                store.Close();
            }
            catch (Exception)
            {
                throw new Exception("No se ha podido obtener la clave privada.");
            }

            return cert;
        }

        /// <summary>
        /// Amplia la firma actual a XADES-T.
        /// </summary>
        public void UpgradeToXadesT(UpgradeParameters parameters)
        {
            XadesTUpgrader upgrader = new XadesTUpgrader();
            upgrader.Upgrade(this, parameters);
        }


        /// <summary>
        /// Amplia la firma actual a XADES-XL.
        /// </summary>
        public void UpgradeToXadesXL(UpgradeParameters parameters)
        {
            XadesXLUpgrader upgrader = new XadesXLUpgrader();
            upgrader.Upgrade(this, parameters);
        }

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
        private void SetSignatureId()
        {
            string id = Guid.NewGuid().ToString();

            _signatureId = "Signature-" + id;
            _signatureValueId = "SignatureValue-" + id;
        }

        /// <summary>
        /// Especifica el nodo en el cual se añadira la firma
        /// </summary>
        /// <param name="elementXPath"></param>
        /// <param name="namespaces"></param>
        private void SetSignatureDestination(SignatureDestination destination)
        {
            XmlNode nodo;

            if (destination.Namespaces.Count > 0)
            {
                XmlNamespaceManager xmlnsMgr = new XmlNamespaceManager(_document.NameTable);
                foreach (var item in destination.Namespaces)
                {
                    xmlnsMgr.AddNamespace(item.Key, item.Value);
                }

                nodo = _document.SelectSingleNode(destination.XPathElement, xmlnsMgr);
            }
            else
            {
                nodo = _document.SelectSingleNode(destination.XPathElement);
            }

            if (nodo == null)
            {
                throw new Exception("Elemento no encontrado");
            }

            _xadesSignedXml.SignatureNodeDestination = (XmlElement)nodo;
        }

        /// <summary>
        /// Añade una transformación XPath al contenido a firmar
        /// </summary>
        /// <param name="XPathString"></param>
        private void AddXPathTransform(string XPathString)
        {
            XmlDocument document;

            if (_xadesSignedXml == null)
            {
                throw new NullReferenceException("No se ha establecido el contenido a firmar");
            }

            if (_document != null)
            {
                document = _document;
            }
            else
            {
                document = new XmlDocument();
            }

            XmlElement xPathElem = document.CreateElement("XPath");
            xPathElem.InnerText = XPathString;

            XmlDsigXPathTransform transform = new XmlDsigXPathTransform();
            transform.LoadInnerXml(xPathElem.SelectNodes("."));

            Reference reference = _xadesSignedXml.SignedInfo.References[0] as Reference;

            reference.AddTransform(transform);
        }


        /// <summary>
        /// Construye el documento enveloped
        /// </summary>
        private void CreateEnvelopedDocument()
        {
            Reference reference = new Reference();

            _xadesSignedXml = new XadesSignedXml(_document);

            reference.Id = "Reference-" + Guid.NewGuid().ToString();
            reference.Uri = "";

            for (int i = 0; i < _document.DocumentElement.Attributes.Count; i++)
            {
                if (_document.DocumentElement.Attributes[i].Name.Equals("id", StringComparison.InvariantCultureIgnoreCase))
                {
                    reference.Uri = "#" + _document.DocumentElement.Attributes[i].Value;
                    break;
                }
            }

            XmlDsigEnvelopedSignatureTransform xmlDsigEnvelopedSignatureTransform = new XmlDsigEnvelopedSignatureTransform();
            reference.AddTransform(xmlDsigEnvelopedSignatureTransform);

            _objectReference = reference.Id;

            _xadesSignedXml.AddReference(reference);
        }

        /// <summary>
        /// Actualiza el documento resultante
        /// </summary>
        internal void UpdateDocument()
        {
            if (_document == null)
            {
                _document = new XmlDocument();
            }

            if (_document.DocumentElement != null)
            {
                XmlNode xmlNode = _document.SelectSingleNode("//*[@Id='" + _xadesSignedXml.Signature.Id + "']");

                if (xmlNode != null)
                {

                    XmlNamespaceManager nm = new XmlNamespaceManager(_document.NameTable);
                    nm.AddNamespace("xades", XadesSignedXml.XadesNamespaceUri);
                    nm.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);

                    XmlNode xmlQPNode = xmlNode.SelectSingleNode("ds:Object/xades:QualifyingProperties", nm);
                    XmlNode xmlUnsingedPropertiesNode = xmlNode.SelectSingleNode("ds:Object/xades:QualifyingProperties/xades:UnsignedProperties", nm);

                    if (xmlUnsingedPropertiesNode != null)
                    {
                        xmlUnsingedPropertiesNode.InnerXml = _xadesSignedXml.XadesObject.QualifyingProperties.UnsignedProperties.GetXml().InnerXml;
                    }
                    else
                    {
                        xmlUnsingedPropertiesNode = _document.ImportNode(_xadesSignedXml.XadesObject.QualifyingProperties.UnsignedProperties.GetXml(), true);
                        xmlQPNode.AppendChild(xmlUnsingedPropertiesNode);
                    }

                }
                else
                {
                    XmlElement xmlSigned = _xadesSignedXml.GetXml();

                    byte[] canonicalizedElement = XMLUtil.ApplyTransform(xmlSigned, new XmlDsigC14NTransform());

                    XmlDocument doc = new XmlDocument();
                    doc.PreserveWhitespace = true;
                    doc.LoadXml(Encoding.UTF8.GetString(canonicalizedElement));

                    XmlNode canonSignature = _document.ImportNode(doc.DocumentElement, true);

                    _xadesSignedXml.GetSignatureElement().AppendChild(canonSignature);
                }
            }
            else
            {
                _document.LoadXml(_xadesSignedXml.GetXml().OuterXml);
            }
        }

        private void ComputeSignature()
        {
            try
            {
                _xadesSignedXml.ComputeSignature();
                _xadesSignedXml.SignatureValueId = _signatureValueId;
            }
            catch (Exception exception)
            {
                throw new Exception("Ha ocurrido durante el proceso de firmado: " + exception.Message);
            }
        }

        #region Información y propiedades de la firma

        private void AddXadesInfo(SignatureParameters parameters)
        {
            _xadesSignedXml.Signature.Id = _signatureId;
            XadesObject xadesObject = new XadesObject();
            xadesObject.Id = "XadesObjectId-" + Guid.NewGuid().ToString();
            xadesObject.QualifyingProperties.Id = "QualifyingProperties-" + Guid.NewGuid().ToString();
            xadesObject.QualifyingProperties.Target = "#" + _signatureId;
            xadesObject.QualifyingProperties.SignedProperties.Id = "SignedProperties-" + _signatureId;

            AddSignatureProperties(
                xadesObject.QualifyingProperties.SignedProperties.SignedSignatureProperties,
                xadesObject.QualifyingProperties.SignedProperties.SignedDataObjectProperties,
                xadesObject.QualifyingProperties.UnsignedProperties.UnsignedSignatureProperties,
                _mimeType, parameters);

            _xadesSignedXml.AddXadesObject(xadesObject);
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


        private void AddCertificateInfo(SignatureParameters parameters)
        {
            SetCryptoServiceProvider(parameters.SigningCertificate);

            _xadesSignedXml.SigningKey = _rsaKey;

            KeyInfo keyInfo = new KeyInfo();
            keyInfo.Id = "KeyInfoId-" + _signatureId;
            keyInfo.AddClause(new KeyInfoX509Data((X509Certificate)parameters.SigningCertificate));
            keyInfo.AddClause(new RSAKeyValue((RSA)_rsaKey));

            _xadesSignedXml.KeyInfo = keyInfo;

            Reference reference = new Reference();

            reference.Id = "ReferenceKeyInfo";
            reference.Uri = "#KeyInfoId-" + _signatureId;

            _xadesSignedXml.AddReference(reference);
        }


        private void AddSignatureProperties(SignedSignatureProperties signedSignatureProperties, SignedDataObjectProperties signedDataObjectProperties,
                   UnsignedSignatureProperties unsignedSignatureProperties, string mimeType, SignatureParameters parameters)
        {
            Cert cert;

            cert = new Cert();
            cert.IssuerSerial.X509IssuerName = parameters.SigningCertificate.IssuerName.Name;
            cert.IssuerSerial.X509SerialNumber = CertUtil.HexToDecimal(parameters.SigningCertificate.SerialNumber);
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
                    spq.AnyXmlElement = _document.CreateElement("SPURI", XadesSignedXml.XadesNamespaceUri);
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
