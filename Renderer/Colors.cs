using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Renderer
{
    public static class Colors
    {
        public const int RED = 255 << 24 | 255 << 16 | 0 << 8 | 0;
        public const int GREEN = 255 << 24 | 0 << 16 | 255 << 8 | 0;
        public const int BLUE = 255 << 24 | 0 << 16 | 0 << 8 | 255;
        public const int BLACK = 255 << 24 | 0 << 16 | 0 << 8 | 0;
        public const int WHITE = 255 << 24 | 255 << 16 | 255 << 8 | 255;

        public static int GetGray(double factor)
        {
            if (factor > 1.0)
            {
                factor = 1;
            }
            else if (factor < 0)
            {
                factor = 0;
            }

            var a = 255;
            var r = (int)(255 * factor);
            var g = (int)(255 * factor);
            var b = (int)(255 * factor);

            return a << 24 | r << 16 | g << 8 | b;
        }
    }
}
