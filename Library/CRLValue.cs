// CRLValue.cs
//
// XAdES Starter Kit for Microsoft .NET 3.5 (and above)
// 2010 Microsoft France
//
// Originally published under the CECILL-B Free Software license agreement,
// modified by Dpto. de Nuevas Tecnologías de la Dirección General de Urbanismo del Ayto. de Cartagena
// and published under the GNU Lesser General Public License version 3.
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

using System;
using System.Xml;

namespace Microsoft.Xades
{
	/// <summary>
	/// This class consist of a sequence of at least one Certificate Revocation
	/// List. Each EncapsulatedCRLValue will contain the base64 encoding of a
	/// DER-encoded X509 CRL.
	/// </summary>
	public class CRLValue : EncapsulatedPKIData
	{
		#region Constructors
		/// <summary>
		/// Default constructor
		/// </summary>
		public CRLValue()
		{
			this.TagName = "EncapsulatedCRLValue";
		}
		#endregion
	}
}
