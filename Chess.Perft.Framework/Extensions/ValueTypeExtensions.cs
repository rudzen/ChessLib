using System.Runtime.InteropServices;

namespace Perft.Extensions
{
    public static class ValueTypeExtensions
    {
        public static int ToInt(this bool @this)
        {
            //return *(byte*)&@this;
            var myBoolSpan = MemoryMarshal.CreateReadOnlySpan(ref @this, 1);
            return MemoryMarshal.AsBytes(myBoolSpan)[0];
        }
    }
}