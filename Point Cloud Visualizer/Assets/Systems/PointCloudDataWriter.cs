using UnityEngine;
using Unity.Entities;
using Unity.Collections;

namespace PointCloudVisualizer
{
    public class PointCloudDataWriter<T> where T : struct, IComponentData
    {
        public RenderTexture RenderTexture;
        public ComputeShader ComputeShader;

        public NativeArray<T> RenderData;

        public int PropertyCount;

        private ComputeBuffer dataBuffer;
        private RenderTexture tempRenderTexture;

        public void WriteToRenderTexture()
        {
            if (!ValidateRenderTexture())
            {
                return;
            }

            int mapWidth = RenderTexture.width;
            int mapHeight = RenderTexture.height;

            int totalData = RenderData.Length;
            int totalProperties = totalData * PropertyCount;

            // Release the temporary objects when the size of them don't match the input.

            if (dataBuffer != null && dataBuffer.count != totalProperties)
            {
                dataBuffer.Dispose();
                dataBuffer = null;
            }

            if (tempRenderTexture != null && (tempRenderTexture.width != mapWidth || tempRenderTexture.height != mapHeight))
            {
                Object.Destroy(tempRenderTexture);
                tempRenderTexture = null;
            }

            // Lazy initialization of temporary objects

            if (dataBuffer == null)
            {
                dataBuffer = new ComputeBuffer(totalProperties, sizeof(float));
            }

            if (tempRenderTexture == null)
            {
                tempRenderTexture = CreateRenderTexture(RenderTexture);
            }

            // Set data and execute the bake task.

            ComputeShader.SetInt("DataCount", totalData);
            dataBuffer.SetData(RenderData);

            const int kernel = 0;

            ComputeShader.SetBuffer(kernel, "DataBuffer", dataBuffer);
            ComputeShader.SetTexture(kernel, "DataMap", tempRenderTexture);

            ComputeShader.Dispatch(kernel, mapWidth / 8, mapHeight / 8, 1);

            GL.Flush();

            // once complete, write the results back on to the real data map file

            Graphics.CopyTexture(tempRenderTexture, RenderTexture);

            RenderData.Dispose();

            dataBuffer?.Dispose();
            dataBuffer = null;

            if (tempRenderTexture != null)
            {
                Object.Destroy(tempRenderTexture);
            }

            tempRenderTexture = null;
        }

        private bool ValidateRenderTexture()
        {
            if (RenderTexture.width % 8 != 0 || RenderTexture.height % 8 != 0)
            {
                Debug.LogError("Render Texture dimensions should be a multiple of 8.");
                return false;
            }

            if (RenderTexture.format != RenderTextureFormat.ARGBHalf && RenderTexture.format != RenderTextureFormat.ARGBFloat)
            {
                Debug.LogError("Render Texture format should be ARGBHalf or ARGBFloat.");
                return false;
            }

            return true;
        }

        private static RenderTexture CreateRenderTexture(RenderTexture source)
        {
            var rt = new RenderTexture(source.width, source.height, 0, source.format)
            {
                enableRandomWrite = true
            };
            rt.Create();
            return rt;
        }
    }
}
