namespace PointCloudVisualizer
{
    public class PointCloudColorSystem : PointCloudSystemBase<PointColor, PointColorEvent>
    {
        protected override void OnCreate()
        {
            base.OnCreate();

            RenderTextureName = "Color Map";
            ComputeShaderName = "float4ComputeShader";
            PropertyCount = 4;
        }
    }
}