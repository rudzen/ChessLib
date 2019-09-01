namespace Perft.Extensions
{
    public static class ArrayExtensions
    {
        public static bool IsEmpty<T>(T[] arr)
        {
            return arr == null || arr.Length == 0;
        }
    }
}