using System;
using System.Collections;
using laszip.net;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using PointCloudVisualizer;
using UnityEngine.UI;

public class PointCloudDataLoader : MonoBehaviour
{
    public Text Text;
    public string File;

    private float timer = 1f;
    private int counter;

    private laszip_dll lazReader;
    private uint numberOfPoints;
    private PointPosition[] pointPositions;
    private PointColor[] pointColors;
    private float maxZ;

    private bool fileLoadStarted = false;
    private bool fileLoaded = false;

    private EntityManager manager;

    private const int batchSize = 100000;

    private string loadingText;

    void Awake()
    {
        manager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    void Update ()
    {
        Text.text = loadingText;

        if (!fileLoaded && !fileLoadStarted && timer < 0)
        {
            fileLoadStarted = true;
            StartCoroutine("ParseFile");
        }

        if (fileLoaded && timer < 0 && numberOfPoints >= counter * batchSize)
        {
            StartCoroutine("LoadPointCloud");
            counter++;
            timer = 0.2f;
        }
        
        timer -= Time.deltaTime;
    }

    IEnumerator ParseFile()
    {
        string fileName = Application.streamingAssetsPath + "/Point Clouds/" + File + ".laz";
        lazReader = new laszip_dll();
        bool compressed = true;
        lazReader.laszip_open_reader(fileName, ref compressed);
        
        numberOfPoints = Math.Min(lazReader.header.number_of_point_records, 10000000);

        int selection = (int)Math.Truncate((double)(lazReader.header.number_of_point_records / numberOfPoints)) + 1;
        
        pointPositions = new PointPosition[numberOfPoints];
        pointColors = new PointColor[numberOfPoints];

        float minX = (float)lazReader.header.min_x;
        float minY = (float)lazReader.header.min_y;
        float minZ = (float)lazReader.header.min_z;
        maxZ = (float)lazReader.header.max_z - minZ;

        var coordArray = new double[3];

        int j = 0;

        for (int i = 0; i < lazReader.header.number_of_point_records; i++)
        {
            loadingText = $"Loading file: {Math.Round((double) i * 100 / numberOfPoints, 2)}%";
            lazReader.laszip_read_point();
            lazReader.laszip_get_coordinates(coordArray);

            if (i % selection != 0) continue;

            pointPositions[j] = new PointPosition(new float3(
                (float)coordArray[0] - minX,
                (float)coordArray[2] - minZ,
                (float)coordArray[1] - minY
            ));

            float relativeZ = ((float)coordArray[2] - minZ) / maxZ;
            pointColors[j] = new float4(relativeZ, Math.Abs(0.5f - relativeZ), 1 - relativeZ, 1);
            j++;
        }

        fileLoaded = true;

        yield return null;
    }

    IEnumerator LoadPointCloud()
    {
        var min = Math.Min(numberOfPoints, (counter + 1) * batchSize);
        for (int pointIndex = counter * batchSize; pointIndex < min; pointIndex++)
        {
            loadingText = $"Creating point cloud: {Math.Round((double)pointIndex * 100 / numberOfPoints, 2)}%";

            var entity = manager.CreateEntity();

            manager.AddComponentData(entity, pointPositions[pointIndex]);
            manager.AddComponentData(entity, pointColors[pointIndex]);
        }

        manager.CreateEntity(typeof(PointPositionEvent));
        manager.CreateEntity(typeof(PointColorEvent));

        yield return null;
    }
}
