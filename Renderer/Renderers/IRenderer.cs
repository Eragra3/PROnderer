using Renderer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Renderer
{
    public interface IRenderer
    {
        int[] RenderFrame(SceneSettings settings);

        void SetModel(WavefrontObj model);
    }
}
