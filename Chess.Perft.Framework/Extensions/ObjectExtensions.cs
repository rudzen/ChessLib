using System.Collections.Generic;
using System.Linq;

namespace Perft.Extensions
{
    using System;

    public static class ObjectExtensions
    {
        /// <summary>
        /// Concats parameters onto an object.
        /// The resulting array is a copy of the objects, which makes this fast for small array sizes.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static object[] ConcatParametersCopy(this object @this, params object[] args)
        {
            var result = new object[args.Length + 1];
            result[0] = @this;
            // forced to use Array copy as there is no way to get a byte size of an managed object in c#
            // as per : http://blogs.msdn.com/cbrumme/archive/2003/04/15/51326.aspx
            Array.Copy(args, 0, result, 1, args.Length);
            return result;
        }

        /// <summary>
        /// Concats parameters onto an object.
        /// The resulting array is deferred object which means this will potentially be faster for larger object arrays
        /// </summary>
        /// <param name="this"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IEnumerable<object> ConcatParameters(this object @this, params object[] args)
        {
            var result = new[] {@this};
            return result.Concat(args);
        }
    }
}