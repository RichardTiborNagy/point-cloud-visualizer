using UnityEngine;
using Unity.Entities;
using Unity.Collections;

namespace PointCloudVisualizer
{
    public abstract class PointCloudSystemBase<T0, T1> : ComponentSystem
        where T0 : struct, IComponentData where T1 : struct, IComponentData
    {
        protected string RenderTextureName;
        protected string ComputeShaderName;
        protected int PropertyCount;

        private EntityQuery dataQuery;
        private EntityQuery eventQuery;

        protected override void OnCreate()
        {
            base.OnCreate();

            dataQuery = EntityManager.CreateEntityQuery(typeof(T0));
            eventQuery = EntityManager.CreateEntityQuery(typeof(T1));

            RequireForUpdate(eventQuery);
        }

        protected override void OnUpdate()
        {
            new PointCloudDataWriter<T0>
            {
                RenderTexture = Resources.Load<RenderTexture>(RenderTextureName),
                ComputeShader = Resources.Load<ComputeShader>(ComputeShaderName),
                RenderData = dataQuery.ToComponentDataArray<T0>(Allocator.TempJob),
                PropertyCount = PropertyCount

            }.WriteToRenderTexture();

            var eventEntities = eventQuery.ToEntityArray(Allocator.TempJob);

            foreach (var eventEntity in eventEntities)
            {
                EntityManager.DestroyEntity(eventEntity);
            }

            eventEntities.Dispose();
        }
    }
}