namespace Perft.Extensions
{
    using System;

    public static class ObjectExtensions
    {
        public static object[] ToResolveParams(this object @this, params object[] args)
        {
            var result = new object[args.Length + 1];
            result[0] = @this;
            // forced to use Array copy as there is no way to get a byte size of an managed object in c#
            // as per : http://blogs.msdn.com/cbrumme/archive/2003/04/15/51326.aspx
            Array.Copy(args, 0, result, 1, args.Length);
            return result;
        }
    }
}