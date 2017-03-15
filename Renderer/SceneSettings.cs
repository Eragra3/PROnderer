namespace Renderer
{
    public struct SceneSettings
    {
        public double ObserverX;
        public double ObserverY;
        public double ObserverZ;

        //(0.0, inf>
        public double Scale;

        public CameraSettings CameraSettings;

        public RenderingMode RenderingMode;
    }
}