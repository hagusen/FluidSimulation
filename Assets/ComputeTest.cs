using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.Serialization;
using Sirenix.OdinInspector;
using System.Linq;

public class ComputeTest : SerializedMonoBehaviour
{


    float[,] testArray;
    float[] array;
    public ComputeShader shader;

    public RenderTexture result;


    public int iterations = 4;

    ComputeBuffer floats;



    // Start is called before the first frame update
    void Start() {
        testArray = new float[1000, 1000];


        for (int i = 0; i < testArray.GetLength(0); i++) {
            for (int j = 0; j < testArray.GetLength(1); j++) {

                testArray[i, j] = Random.value * 100;
            }
        }



        // Flatten the array wow!
        array = testArray.Cast<float>().ToArray();

        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

        timer.Start();
        //TestSqrt();
        timer.Stop();

        Debug.Log(timer.ElapsedMilliseconds);





        int kernel = shader.FindKernel("test"); // Can be null!


        result = new RenderTexture(128, 128, 32);
        result.enableRandomWrite = true;
        result.Create();

        shader.SetTexture(kernel, "Result", result);


        floats = new ComputeBuffer(array.Length, 4);

        shader.SetBuffer(kernel, "floats", floats);

        shader.SetInt("columnsize", testArray.GetLength(0));


        timer.Reset();
        timer.Start();
        floats.SetData(array);
        //
        for (int i = 0; i < iterations; i++) {

            shader.Dispatch(kernel, 1000 /32, 1000 / 32, 1);
        }
        //
        floats.GetData(array);

        timer.Stop();

        Debug.Log(timer.ElapsedMilliseconds);


        // shader.SetVectorArray(kernel, )













    }



    void TestSqrt() {
        for (int k = 0; k < iterations; k++) {

            for (int i = 1; i < testArray.GetLength(0) - 1; i++) {
                for (int j = 1; j < testArray.GetLength(1) - 1; j++) {

                    testArray[i, j] = testArray[i, j]
                        + (testArray[i + 1, j] +
                        testArray[i - 1, j] +
                        testArray[i, j + 1] +
                        testArray[i, j - 1]) / 4;
                }
            }
        }

    }


    // Update is called once per frame
    void Update() {





    }

    private void OnDestroy() {
        floats.Dispose();
    }
}
