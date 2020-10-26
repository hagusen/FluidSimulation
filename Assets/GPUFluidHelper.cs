using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GPUFluidHelper : MonoBehaviour
{



    public ComputeShader shader;

    [Header("Fluid Inputs")]

    public int size;
    public int iterations = 4;
    [Range(0.000001f, 0.01f)]
    public float viscosity; // Viscosity

    [Header("other")]
    public int kernel;
    ComputeBuffer velocities;
    ComputeBuffer velocities0;




    //
    public RenderTexture result;


    private void OnRenderImage(RenderTexture source, RenderTexture destination) {




        Graphics.Blit(result, destination, (Vector2.right * Screen.width + Vector2.up * Screen.height) / 100, -Vector2.one * 2); // send to screen

    }

    int threads;

    int linsolve;
    int setboundsv0;
    int projectStart;
    int linsolve2;
    int projectEnd;
    int advect;

    int render;
    int addVelocity;

    // Start is called before the first frame update
    void Start() {

        velocities = new ComputeBuffer(size * size, 8);
        velocities0 = new ComputeBuffer(size * size, 8);

        shader.SetInt("size", size);
        shader.SetInt("iterations", iterations);
        shader.SetFloat("viscosity", viscosity);
        shader.SetFloat("dt", Time.deltaTime);

        threads = 8;//size / 32;

        result = new RenderTexture(128, 128, 32);
        result.enableRandomWrite = true;
        result.Create();

        shader.SetTexture(kernel, "Result", result);

        //Lin solve diffuse
        linsolve = shader.FindKernel("LinearSolve");
        shader.SetBuffer(linsolve, "v", velocities);
        shader.SetBuffer(linsolve, "v0", velocities0);
        float a = Time.deltaTime * viscosity * (size - 2) * (size - 2);
        shader.SetFloat("a", a);
        shader.SetFloat("c", 1 + 4 * a);
        //

        // Set bounds V0 true
        setboundsv0 = shader.FindKernel("Setboundsv0");
        shader.SetBuffer(setboundsv0, "v0", velocities0);
        //

        //// Project 
        // start
        projectStart = shader.FindKernel("ProjectStart");
        shader.SetBuffer(projectStart, "v", velocities);
        shader.SetBuffer(projectStart, "v0", velocities0);
        //Lin solve 2 
        linsolve2 = shader.FindKernel("LinearSolve2");
        shader.SetBuffer(linsolve2, "v", velocities);
        //end
        projectEnd = shader.FindKernel("ProjectEnd");
        shader.SetBuffer(projectEnd, "v", velocities);
        shader.SetBuffer(projectEnd, "v0", velocities0);
        ////


        // Advect
        advect = shader.FindKernel("Advect");
        shader.SetBuffer(advect, "v", velocities);
        shader.SetBuffer(advect, "v0", velocities0);
        shader.SetFloat("dtx", Time.deltaTime * (size - 2));
        shader.SetFloat("dty", Time.deltaTime * (size - 2));
        shader.SetFloat("sizefloat", size);
        //




        render = shader.FindKernel("Render");
        shader.SetBuffer(render, "v", velocities);
        shader.SetTexture(render, "Result", result);


        addVelocity = shader.FindKernel("AddVelocity");
        shader.SetBuffer(addVelocity, "v", velocities);
        float[] d = new float[2];
        d[0] = 5;
        d[1] = 5;
        shader.SetFloats("velToAdd", d);
        int[] i = new int[2];
        i[0] = 5;
        i[1] = 5;
        shader.SetInts("position", i);



        FluidUpdate();

        




    }

    private void FluidUpdate() {

        //Diffuse
        for (int i = 0; i < iterations; i++) {
            shader.Dispatch(linsolve, threads, threads, 1);
            shader.Dispatch(setboundsv0, threads, threads, 1);
        }

        //Project
        shader.Dispatch(projectStart, threads, threads, 1);
        for (int i = 0; i < iterations; i++) {
            shader.Dispatch(linsolve2, threads, threads, 1);
        }
        shader.Dispatch(projectEnd, threads, threads, 1);
        //

        //Advect
        shader.Dispatch(advect, threads, threads, 1);

        
        shader.Dispatch(render, threads, threads, 1);

    }



    // Update is called once per frame
    void Update() {

        FluidUpdate();



    }

    private void OnDestroy() {

        velocities.Release();
        velocities0.Release();
    }
}
