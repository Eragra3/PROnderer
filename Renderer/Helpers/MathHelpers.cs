using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Renderer.Helpers
{
    public static class MathHelpers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Max(params double[] values)
        {
            return values.Max();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Min(params double[] values)
        {
            return values.Min();
        }
    }
}
