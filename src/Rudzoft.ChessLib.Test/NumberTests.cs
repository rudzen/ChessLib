using System.Runtime.CompilerServices;

namespace Rudzoft.ChessLib.Test;

public sealed class NumberTests
{
    [Fact]
    [SkipLocalsInit]
    public void NumberTest()
    {
        const int numMask = 0b0000_0000_0000_0000_0000_0000_0000_1111;

        var strNum = "12345678";
        var strNum2 = "45678945";

        var compacted = (ulong.Parse(strNum) << 32) | ulong.Parse(strNum2);

        var reverseNum = ((int)(compacted >> 32)).ToString();
        var reverseNum2 = ((int)(compacted & 0xFFFFFFFF)).ToString();

        Assert.Equal(strNum, reverseNum);
        Assert.Equal(strNum2, reverseNum2);

        var numSpan = strNum.AsSpan();

        //101111101011110000011111111

        var result = ulong.MinValue;

        for (var index = 0; index < numSpan.Length; index++)
        {
            var shift = index * 4;
            var c = numSpan[index];
            var b = (byte)c;
            result |= (ulong)(b & numMask) << shift;
        }

        Span<char> resultSpan = stackalloc char[8];
        for (var i = 0; i < 8; i++)
        {
            var shift = i * 4;
            var b = (byte)((result >> shift) & numMask);
            resultSpan[i] = (char)(b + 48);
        }

        Assert.Equal(resultSpan.ToString(), strNum);
    }
}