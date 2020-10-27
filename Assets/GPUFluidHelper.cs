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

    public float viscosity; // Viscosity

    [Header("other")]
    public int kernel;
    ComputeBuffer velocities;
    ComputeBuffer velocities0;
    public FilterMode filtermode = FilterMode.Point;
    public Texture2D tex;
    public float[] d;


    //
    public RenderTexture result;
    public Vector2[] v;


    private void OnRenderImage(RenderTexture source, RenderTexture destination) {




        //Graphics.Blit(result, destination, (Vector2.right * Screen.width + Vector2.up * Screen.height) / 100, -Vector2.one * 2); // send to screen
        //Graphics.Blit(result, destination, (Vector2.right * Screen.width + Vector2.up * Screen.height) / 100, Vector2.zero); // send to screen
        Graphics.Blit(result, destination); // send to screen

    }

    int threads;

    int linsolve;
    int setboundsv0;
    int projectStart;
    int linsolve2;
    int projectEnd;
    int advect;
    int projectStart2;
    int linsolve22;
    int projectEnd2;

    int render;
    int addVelocity;

    private void Awake() {
        velocities = new ComputeBuffer(size * size, 8);
        velocities0 = new ComputeBuffer(size * size, 8);
        v = new Vector2[size * size];
        threads = size / 32;

        result = new RenderTexture(size, size, 24);
        result.filterMode = FilterMode.Point;
        result.wrapMode = TextureWrapMode.Clamp;
        result.enableRandomWrite = true;
        result.Create();

        tex = new Texture2D(size, size, TextureFormat.RGB24, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = filtermode;

        shader.SetTexture(kernel, "Result", result);

        Init();
    }

    // Start is called before the first frame update
    void Init() {





        shader.SetInt("size", size);
        shader.SetInt("iterations", iterations);
        shader.SetFloat("viscosity", viscosity);
        shader.SetFloat("dt", Time.deltaTime);


      



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

        //// Project  2
        // start2
        projectStart2 = shader.FindKernel("ProjectStart2");
        shader.SetBuffer(projectStart2, "v", velocities);
        shader.SetBuffer(projectStart2, "v0", velocities0);
        //Lin solve 22 
        linsolve22 = shader.FindKernel("LinearSolve22");
        shader.SetBuffer(linsolve22, "v0", velocities0);
        //end2
        projectEnd2 = shader.FindKernel("ProjectEnd2");
        shader.SetBuffer(projectEnd2, "v", velocities);
        shader.SetBuffer(projectEnd2, "v0", velocities0);
        ////


        render = shader.FindKernel("Render");
        shader.SetBuffer(render, "v", velocities);
        shader.SetTexture(render, "Result", result);


        addVelocity = shader.FindKernel("AddVelocity");
        shader.SetBuffer(addVelocity, "v", velocities);

        shader.SetFloats("velToAdd", d);
        int[] i = new int[2];
        i[0] = 50;
        i[1] = 50;
        shader.SetInts("position", i);



        FluidUpdate();

        




    }
    float a;
    private void FluidUpdate() {

        shader.SetInt("size", size);
        shader.SetInt("iterations", iterations);
        shader.SetFloat("dt", Time.deltaTime);

        a = Time.deltaTime * viscosity * (size - 2) * (size - 2);
        shader.SetFloat("a", a);
        shader.SetFloat("c", 1 + 4 * a);

        shader.SetFloat("dtx", Time.deltaTime * (size - 2));
        shader.SetFloat("dty", Time.deltaTime * (size - 2));
        shader.SetFloat("sizefloat", size);

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


        //Project 2
        shader.Dispatch(projectStart2, threads, threads, 1);
        for (int i = 0; i < iterations; i++) {
            shader.Dispatch(linsolve22, threads, threads, 1);
        }
        shader.Dispatch(projectEnd2, threads, threads, 1);
        //

        
        shader.Dispatch(render, threads, threads, 1);

    }

    Vector3 mouse0 = Vector2.zero;
    // Update is called once per frame
    void Update() {

        FluidUpdate();

        velocities.GetData(v);


        var s = Input.mousePosition - mouse0;
        s.Normalize();
        s *= 10;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit)) {
            Vector2Int vec = Vector2Int.RoundToInt(hit.textureCoord * size);
            if (Input.GetKey(KeyCode.Mouse0)) {
                v[vec.x * size + vec.y] += (Vector2)s * 40;

            }
        }

        for (int i = 0; i < 128; i++) {

            v[62*62 + i] += Vector2.right * 100;

        }
        RenderFluid();

        velocities.SetData(v);
        //velocities0.SetData(v);
        Init();
        if (Input.GetKeyDown(KeyCode.Space)) {

        }

        //shader.SetFloats("velToAdd", d);

        //shader.Dispatch(addVelocity, 1, 1, 1);

        mouse0 = Input.mousePosition;
    }


    void RenderFluid() {



        var stepsize = 1f / size;//
        for (int y = 0; y < size; y++) {
            for (int x = 0; x < size; x++) {
                //texture.SetPixel(x, y, Color.red);
                //tex.SetPixel(x, y, new Color(fluid.density[IX(x,y)],0f , 0f));
                //tex.SetPixel(x, y, new Color(fluid.Vx[IX(x, y)], fluid.Vy[IX(x, y)], fluid.density[IX(x, y)]));
                if (true) {
                    //tex.SetPixel(x, y, new Color( fluid.velocity[x, y].magnitude , fluid.velocity[x, y].magnitude, fluid.velocity[x, y].magnitude));
                    tex.SetPixel(x, y, new Color(v[x* size + y].x, -v[x * size + y].x, Mathf.Abs(v[x * size + y].y)));

                }
                else {

                   // tex.SetPixel(x, y, new Color(/*(Vector2. right * fluid.Vx[IX(x, y)] +  Vector2.up * fluid.Vy[IX(x, y)]).magnitude*/0, Mathf.Abs(fluid.velocity[x, y].x), Mathf.Abs(fluid.velocity[x, y].y)));
                }
            }

            tex.SetPixel(13, y, Color.blue);
        }

        // Apply all SetPixel calls
        tex.Apply();


        GetComponent<Renderer>().material.mainTexture = tex;
    }



    private void OnDestroy() {

        velocities.Release();
        velocities0.Release();
    }
}
