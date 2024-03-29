﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class WaterBody : MonoBehaviour
{
    public ComputeShader computeShader;
    public int waterLength = 128;

    public float waveSpeed = 1.0f;
    public float waveHeight = 1.0f;
    public float waveFrequency = 20.0f;

    
    int hComputeKernal;
    private RenderTexture RT;

    [HideInInspector]
    public float waveMultipler = 5.0f, waveHeightScale = 1.0f, waveOffset = 1.0f;

    private Vector3[] vertices;
    private Mesh mesh;
    private Renderer renderer;

    private List<Vector3> verts;

    public void Awake()
    {
        GenerateMesh();
        renderer = GetComponent<Renderer>();

        verts = new List<Vector3>();

        //! Create Render texture for computeShader shader output!
        RT = new RenderTexture(waterLength, waterLength, 24);
        RT.enableRandomWrite = true;
        RT.format = RenderTextureFormat.ARGBFloat;
        RT.Create();

        //! compute shader related variable linking!
        int hComputeKernal = computeShader.FindKernel("CSWater");
    }

    private Texture2D GenerateTexture()
    {
        Texture2D texture = new Texture2D(waterLength, waterLength);

        for (int x = 0; x < waterLength; x++)
        {
            for (int y = 0; y < waterLength; y++)
            {
                Color color = CalculateColor(x, y);
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        return texture;
    }

    private void UpdateTextureFromCompute()
    {
        computeShader.SetInt("waterLength", waterLength);
        computeShader.SetFloat("waveFrequency", waveFrequency);
        computeShader.SetFloat("waveSpeed", Time.time * waveSpeed);
        computeShader.SetTexture(hComputeKernal, "Result", RT);

        computeShader.Dispatch(hComputeKernal, waterLength / 8, waterLength / 8, 1);

        renderer.material.SetFloat("_WaveHeight", waveHeight);
        renderer.material.SetTexture("_NoiseTex", RT);
    }

    private Color CalculateColor(int x, int y)
    {
        float xCoord = (float)x / waterLength * waveFrequency + Time.time;
        float yCoord = (float)y / waterLength * waveFrequency + Time.time;

        float sample = Mathf.PerlinNoise(xCoord, yCoord);
        return new Color(sample, sample, sample);
    }

    private void UpdateMesh()
    {
        verts.Clear();
        mesh.GetVertices(verts);

        for(int i = 0; i < verts.Count; ++i)
        {
            float height = waveHeightScale * CalculateHeight((int)(verts[i].x * waveMultipler), (int)(verts[i].z * waveMultipler));
            verts[i] = new Vector3(verts[i].x, height, verts[i].z);
        }

        mesh.SetVertices(verts);
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
    }

    private float CalculateHeight(int x, int y)
    {
        float xCoord = (float)x / waterLength * waveFrequency + Time.time * waveOffset;
        float yCoord = (float)y / waterLength * waveFrequency + Time.time * waveOffset;

        float sample = Mathf.PerlinNoise(xCoord, yCoord);
        return sample;
    }

    private void GenerateMesh()
    {
        // create vertices...
        vertices = new Vector3[(waterLength + 1) * (waterLength + 1)];
        Vector2[] uv = new Vector2[vertices.Length];

        // set position
        for(int i = 0, y = 0 ; y <= waterLength; y++)
        {
            for(int x = 0; x <= waterLength; x++, i++)
            {
                vertices[i] = new Vector3(x, 0, y);
                uv[i] = new Vector2((float)x / waterLength, (float)y / waterLength);
            }
        }

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.vertices = vertices;
        mesh.uv = uv;

        int[] triangles = new int[waterLength * waterLength * 6];
        for (int ti = 0, vi = 0, y = 0; y < waterLength; y++, vi++)
        {
            for (int x = 0; x < waterLength; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + waterLength + 1;
                triangles[ti + 5] = vi + waterLength + 2;
            }
        }
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    //private void OnDrawGizmos()
    //{
    //    if (vertices == null)
    //        return;
    //
    //    Gizmos.color = Color.black;
    //    for(int i = 0; i < vertices.Length; i++)
    //    {
    //        Gizmos.DrawSphere(vertices[i], 0.1f);
    //    }
    //}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTextureFromCompute();
        //UpdateMesh();
    }
}
