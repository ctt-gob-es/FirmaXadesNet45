// SigPolicyQualifierCollection.cs
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
using System.Collections;

namespace Microsoft.Xades
{
	/// <summary>
	/// Collection class that derives from ArrayList.  It provides the minimally
	/// required functionality to add instances of typed classes and obtain typed
	/// elements through a custom indexer.
	/// </summary>
	public class SigPolicyQualifierCollection : ArrayList
	{
		/// <summary>
		/// New typed indexer for the collection
		/// </summary>
		/// <param name="index">Index of the object to retrieve from collection</param>
		public new SigPolicyQualifier this[int index]
		{
			get
			{
				return (SigPolicyQualifier)base[index];
			}
			set
			{
				base[index] = value;
			}
		}

		/// <summary>
		/// Add typed object to the collection
		/// </summary>
		/// <param name="objectToAdd">Typed object to be added to collection</param>
		/// <returns>The object that has been added to collection</returns>
		public SigPolicyQualifier Add(SigPolicyQualifier objectToAdd)
		{
			base.Add(objectToAdd);

			return objectToAdd;
		}

		/// <summary>
		/// Add new typed object to the collection
		/// </summary>
		/// <returns>The newly created object that has been added to collection</returns>
		public SigPolicyQualifier Add()
		{
			return this.Add(new SigPolicyQualifier());
		}
	}
}