namespace PointCloudVisualizer
{
    public class PointCloudRotationSystem : PointCloudSystemBase<PointRotation, PointRotationEvent>
    {
        protected override void OnCreate()
        {
            base.OnCreate();

            RenderTextureName = "Rotation Map";
            ComputeShaderName = "float3ComputeShader";
            PropertyCount = 3;
        }
    }
}