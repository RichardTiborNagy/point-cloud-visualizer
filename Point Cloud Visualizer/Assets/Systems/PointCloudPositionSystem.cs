namespace PointCloudVisualizer
{
    public class PointCloudPositionSystem : PointCloudSystemBase<PointPosition, PointPositionEvent>
    {
        protected override void OnCreate()
        {
            base.OnCreate();

            RenderTextureName = "Position Map";
            ComputeShaderName = "float3ComputeShader";
            PropertyCount = 3;
        }
    }
}