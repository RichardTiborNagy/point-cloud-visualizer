using Unity.Entities;
using Unity.Mathematics;

namespace PointCloudVisualizer
{
    public struct PointPosition : IComponentData
    {
        public float3 Value;

        public PointPosition(float3 newValue) { Value = newValue; }

        public static implicit operator float3(PointPosition data) => data.Value;
        public static implicit operator PointPosition(float3 data) => new PointPosition(data);
    }

    public struct PointRotation : IComponentData
    {
        public float3 Value;

        public PointRotation(float3 newValue) { Value = newValue; }

        public static implicit operator float3(PointRotation data) => data.Value;
        public static implicit operator PointRotation(float3 data) => new PointRotation(data);
    }

    public struct PointColor : IComponentData
    {
        public float4 Value;

        public PointColor(float4 newValue) { Value = newValue; }

        public static implicit operator float4(PointColor data) => data.Value;
        public static implicit operator PointColor(float4 data) => new PointColor(data);
    }
}
