using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Xades
{
    public class ReferenceCollection: ArrayList
    {
        public new Reference this[int index]
        {
            get
            {
                return (Reference)base[index];
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
        public Reference Add(Reference objectToAdd)
        {
            base.Add(objectToAdd);

            return objectToAdd;
        }
    }
}
