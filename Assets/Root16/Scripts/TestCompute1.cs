using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCompute1 : MonoBehaviour
{
    public ComputeShader shader;
    public int texResolution = 256;

    private RenderTexture RT;

    // Start is called before the first frame update
    void Start()
    {
        RT = new RenderTexture(texResolution, texResolution, 24);
        RT.enableRandomWrite = true;
        RT.Create();
    }

    public void UpdateTextureFromCompute()
    {
        int kernHandle = shader.FindKernel("CSMain");

        int randomInt = Random.Range(1, 256);

        shader.SetTexture(kernHandle, "Result", RT);
        shader.SetInt("randomInt", randomInt);
        shader.Dispatch(kernHandle, texResolution / 8, texResolution / 8, 1);

        GetComponent<Renderer>().material.SetTexture("_MainTex", RT);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            UpdateTextureFromCompute();
        }
    }
}
