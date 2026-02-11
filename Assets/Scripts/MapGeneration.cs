using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MapGeneration : MonoBehaviour
{
    public int width = 100;
    public int depth = 100;
    public float scale = 20f;
    public float heightMultiplier = 10f;
    public int octaves = 4;
    public float persistence = 0.5f;
    public float lacunarity = 2f;
    public float roadWidth = 5f; // Largeur de la route
    public float roadAmplitude = 10f; // Amplitude de la courbure de la route
    public float roadFrequency = 0.1f; // Fréquence de la courbure de la route

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateShape();
        UpdateMesh();
    }

    void CreateShape()
    {
        vertices = new Vector3[(width + 1) * (depth + 1)];

        for (int i = 0, z = 0; z <= depth; z++)
        {
            for (int x = 0; x <= width; x++)
            {
                float y = CalculateHeight(x, z);
                vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }

        triangles = new int[width * depth * 6];
        int vert = 0;
        int tris = 0;

        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + width + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + width + 1;
                triangles[tris + 5] = vert + width + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }
    }

    float CalculateHeight(int x, int z)
    {
        float amplitude = 1;
        float frequency = 1;
        float height = 0;

        // Calculer la position centrale de la route en fonction d'une sinusoïde
        float roadCenter = Mathf.Sin(z * roadFrequency) * roadAmplitude + width / 2f;
        float distanceFromRoad = Mathf.Abs(x - roadCenter);

        // Si la position est proche de la route sinueuse, forcez la hauteur à zéro (ou une valeur basse pour une route plate)
        if (distanceFromRoad < roadWidth)
        {
            height = 0; // Route plate
        }
        else
        {
            for (int i = 0; i < octaves; i++)
            {
                float sampleX = x / scale * frequency;
                float sampleZ = z / scale * frequency;

                float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ) * 2 - 1;
                height += perlinValue * amplitude;

                amplitude *= persistence;
                frequency *= lacunarity;
            }

            height *= heightMultiplier;
        }

        return height;
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}
