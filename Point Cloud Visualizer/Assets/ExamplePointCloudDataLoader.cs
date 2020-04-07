using System;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using PointCloudVisualizer;
using Random = Unity.Mathematics.Random;

public class ExamplePointCloudDataLoader : MonoBehaviour
{
    void Awake()
    {
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;

        var random = new Random((uint) DateTime.Now.Millisecond);

        for (int positionX = 0; positionX < 1000; positionX++)
        {
            for (int positionZ = 0; positionZ < 1000; positionZ++)
            {
                float positionY = random.NextFloat(0, 1000);

                float rotationX = random.NextFloat(0, 360);
                float rotationY = random.NextFloat(0, 360);
                float rotationZ = random.NextFloat(0, 360);

                var entity = manager.CreateEntity();
                manager.AddComponentData(entity, new PointPosition(new float3(positionX, positionY, positionZ) / 10));
                manager.AddComponentData(entity, new PointRotation(new float3(rotationX, rotationY, rotationZ)));

                var unityColor = Color.HSVToRGB(Mathf.PerlinNoise(positionX * 0.01f, positionZ * 0.01f) - 0.1f, 0.7f, 1);
                var color = new float4(unityColor.r, unityColor.g, unityColor.b, 1);
                manager.AddComponentData(entity, new PointColor(color));
            }
        }

        manager.CreateEntity(typeof(PointPositionEvent));
        manager.CreateEntity(typeof(PointRotationEvent));
        manager.CreateEntity(typeof(PointColorEvent));
    }
}
