/*
 * Perlin noise Terrain height generator
*/
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TerrainGenerate : MonoBehaviour
{
    private Terrain myTerrain;
    private int xRes, yRes;
    private float xCoord, yCoord;
    private float[,] Heights;
    private float[,,] splatMap;

    public float perlinScale;

    void Start()
    {
        myTerrain = GetComponent<Terrain>();
        xRes = myTerrain.terrainData.heightmapHeight;
        yRes = myTerrain.terrainData.heightmapWidth;

        Heights = myTerrain.terrainData.GetHeights(0, 0, xRes, yRes);
        splatMap = new float[myTerrain.terrainData.alphamapWidth, myTerrain.terrainData.alphamapHeight, myTerrain.terrainData.alphamapLayers];

        GenerateTerrain();
        Paint();
    }

    private void GenerateTerrain()
    {
        for (int i = 0; i < xRes; i++)
        {
            for (int j = 0; j < yRes; j++)
            {
                xCoord = (float)i / xRes * perlinScale;
                yCoord = (float)j / yRes * perlinScale;
                float num = Mathf.PerlinNoise(xCoord, yCoord);
                
                //adds heights around the edges as a barrier
                if (i < 20 || i > xRes - 20 || j < 20 || j > yRes - 20)
                {
                    Heights[i, j] = 0.1f + num * 0.8f;
                }
                else
                {
                    Heights[i, j] = 0.1f + num * terrainScale;
                }
            }
        }
        
        myTerrain.terrainData.SetHeights(0, 0, Heights);
    }
    
    //paints terrain in different ratios based on height
    private void Paint()
    {
        int yAlpha = myTerrain.terrainData.alphamapHeight;
        int xAlpha = myTerrain.terrainData.alphamapWidth;

        for (int y = 0; y < yAlpha; y++)
        {
            for (int x = 0; x < xAlpha; x++)
            {
                float y_01 = (float)y / (float)yAlpha;
                float x_01 = (float)x / (float)xAlpha;

                float height = myTerrain.terrainData.GetHeight(Mathf.RoundToInt(y_01 * myTerrain.terrainData.heightmapHeight), Mathf.RoundToInt(x_01 * myTerrain.terrainData.heightmapWidth));
                float[] splatWeights = new float[myTerrain.terrainData.alphamapLayers];

                splatWeights[0] = 0f;
                splatWeights[1] = 0f;

                if (height > 33f)
                {
                    splatWeights[2] = 1f;
                    splatWeights[3] = 0f;
                }
                else if (height < 29f)
                {
                    splatWeights[2] = 0f;
                    splatWeights[0] = 0.7f;
                    splatWeights[3] = 0.3f;
                }
                else
                {
                    splatWeights[2] = 1f - height / 100f;
                    splatWeights[3] = height / 100f;
                }

                float z = splatWeights.Sum();

                for (int i = 0; i < myTerrain.terrainData.alphamapLayers; i++)
                {
                    splatWeights[i] /= z;
                    splatMap[x, y, i] = splatWeights[i];
                }
            }
        }

        myTerrain.terrainData.SetAlphamaps(0, 0, splatMap);
    }
