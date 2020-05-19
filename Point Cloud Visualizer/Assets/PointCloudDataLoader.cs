using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using laszip.net;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using PointCloudVisualizer;
using UnityEngine.UI;

public class PointCloudDataLoader : MonoBehaviour
{
    public Text LoadingText;
    public Dropdown FileDropdown;
    public Toggle ColorationToggle;

    public bool UseHeightColoration;

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
    private string file;

    void Awake()
    {
        manager = World.DefaultGameObjectInjectionWorld.EntityManager;

        var directory = new DirectoryInfo(Application.streamingAssetsPath + "/Point Clouds/");
        var files = directory.GetFiles("*.*");
        FileDropdown.ClearOptions();
        FileDropdown.AddOptions(files.Where(f => f.Name.EndsWith(".laz")).Select(f => f.Name).ToList());
    }

    void Update ()
    {
        LoadingText.text = loadingText;

        if (fileLoaded && timer < 0 && numberOfPoints >= counter * batchSize)
        {
            StartCoroutine("LoadPointCloud");
            counter++;
            timer = 0.2f;
        }
        
        timer -= Time.deltaTime;
    }

    public void LoadClicked()
    {
        if (!fileLoaded && !fileLoadStarted && timer < 0)
        {
            file = FileDropdown.options[FileDropdown.value].text;
            fileLoadStarted = true;
            StartCoroutine("ParseFile");
        }
    }

    public void SetUseHeightColoration()
    {
        UseHeightColoration = ColorationToggle.isOn;
    }

    IEnumerator ParseFile()
    {
        string fileName = Application.streamingAssetsPath + "/Point Clouds/" + file;
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

            if (UseHeightColoration)
            {
                float relativeZ = ((float)coordArray[2] - minZ) / maxZ;
                pointColors[j] = new float4(relativeZ, Math.Abs(0.5f - relativeZ), 1 - relativeZ, 1);
            }
            else
            {
                pointColors[j] = GetColorFromClassification(lazReader.point.classification);
            }

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
            var loadedPercentage = Math.Round((double) pointIndex * 100 / numberOfPoints, 2);
            loadingText = Math.Abs(loadedPercentage - 100) < 0.1
                ? ""
                : $"Creating point cloud: {Math.Round((double) pointIndex * 100 / numberOfPoints, 2)}%";

            var entity = manager.CreateEntity();

            manager.AddComponentData(entity, pointPositions[pointIndex]);
            manager.AddComponentData(entity, pointColors[pointIndex]);
        }

        manager.CreateEntity(typeof(PointPositionEvent));
        manager.CreateEntity(typeof(PointColorEvent));

        yield return null;
    }

    private float4 GetColorFromClassification(byte classification)
    {
        switch (classification)
        {
            case 1:
                return new float4(191, 191, 191, 1);
            case 2:
                return new float4(132, 64, 0, 1);
            case 3:
                return new float4(0, 130, 0, 1);
            case 4:
                return new float4(0, 191, 0, 1);
            case 5:
                return new float4(0, 255, 0, 1);
            case 6:
                return new float4(0, 129, 194, 1);
            case 7:
                return new float4(254, 0, 0, 1);
            case 8:
                return new float4(255, 255, 0, 1);
            case 9:
                return new float4(0, 0, 251, 1);
            case 0:
            default:
                return new float4(133,133,133,1);
        }
    }
}
