using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView, RequireComponent(typeof(ComputeShader))]

public class RayMaster : MonoBehaviour
{

    public ComputeShader rayMarcher;

    [Range (1f,16f)] public float power = 7;
    [Range(2,64) ] public int iterations = 6;
    public bool julia = false;
    public bool cross = false;
    public Vector3 c;

    [Header ("Color Settings")]
    public Color color_1;
    public Color color_2;
    public Color color_3;
    public Color interior_color;
    public Color bg_color;

    int KernelID;
    RenderTexture target;
    Camera cam;
    Light lightSource;
    float t;
    float y = 0f;
    float it = 10f;
    private int init = 0;

    void Start() {
        Application.targetFrameRate = 190;
        power = 1f;
        c.z = 0;
        iterations = 32;
    }

    void Init(){
        cam = Camera.current;
        lightSource = FindObjectOfType<Light> ();
        KernelID = rayMarcher.FindKernel("CSMain");
    }
/*
    void Update () {
        //Debug.Log(t);
        //Debug.Log(iterations);
        
        if(t<13){
            julia = false; cross = false;
            power = Mathf.SmoothStep(1f,16f,Mathf.Sin(0.05f*t)*1.2f);
        }

        else if(t<18){

            julia = true; cross = false;
            c.x = 0.2f; c.y = -0.6f;
            c.x -= 0.0002f; c.y -= 0.0006f;

            power = Mathf.SmoothStep(2f, 16f, Mathf.Sin(t*0.1f));
            //power = Mathf.Abs(Mathf.Sin(t*0.2f)*15)+1;
            c.z = Mathf.Sin(t*0.5f);
            bg_color = new Color(0.8f,Mathf.SmoothStep(0.1f,1f,0.01f*t),0.34f);
            transform.Translate (0.0035f, 0f, 0f);
            transform.LookAt(Vector3.zero);

        }

        else if(t<30){
            
             c.x = 0f; c.y = 0f;
            julia = true; cross = false;

            power = Mathf.Lerp(2f, 16f, Mathf.Sin(t*0.1f));
            c.z = Mathf.Lerp(-1f, 1.1f, 0.52f*Mathf.Sin(t*0.5f));
            color_3 += new Color(0,0.001f, 0.001f, 1);
            bg_color = new Color(Mathf.SmoothStep(0.2f,1f,0.1f*t),0.06f,0.34f);
            cam.transform.Translate(0.008f, 0, 0f);
            cam.transform.LookAt(Vector3.zero);
            
        }

        else if(t<40){
            
            cross = true;
            c.z = 0.46f;
            power = Mathf.Lerp(3.5f,3.8f,0.02f*t);
            bg_color = new Color(0.5f,0.06f,Mathf.Lerp(0.2f,1f,0.1f*t));
            color_2 += new Color(0.002f,0f,0f);

            if(y>0.02f){
                transform.Translate (0.0005f, 0f, 0f);
                transform.LookAt(Vector3.zero);
            }

            else {
                y+= 0.0001f;
                transform.Translate (0.0035f, y, 0f);
                transform.LookAt(Vector3.zero);
            }
        }

        else if(t<50) {

            julia = cross = true;

            if (init == 0) {
                c.x = c.y = 0.01f;
                c.z = 0.52f;
                power = 3f;
                init = 1;
                iterations = 64;
                it = iterations;
            }

            if(power<4.5f) power += 0.003f;

            c.z = Mathf.Lerp(0.52f,0.9f,0.001f);
            c.x = Mathf.Lerp(0.0f,0.2f,0.0002f);
            c.y = Mathf.Lerp(0.0f,0.2f,0.0002f);
            transform.Translate(0.0035f, 0f, 0f);
            transform.LookAt(Vector3.zero);
            bg_color = new Color(0.1f,Mathf.Lerp(0.2f,1f,0.1f*t),0.34f);
            interior_color = new Color(1.0f,0.1f,0.0f);

        }

        
        else if(t<60){

            iterations = 32;
            cross = false;
            c.x = 0.45f; c.y = 0.60f;
            c.z = Mathf.Lerp(0f, 1.1f, Mathf.Sin(t*0.08f));
            power = Mathf.Lerp(1f, 16f, Mathf.Sin(t*0.0525f));
            bg_color = new Color(0.1f,0.5f,Mathf.Lerp(0.4f,1f,0.2f*t));
            if(y>0){
                y-= 0.0001f;
                transform.Translate (0.0005f, -y, -0.0005f);
                transform.LookAt(Vector3.zero);
            }
            else {
                transform.Translate (0.0035f, 0f, 0f);
                transform.LookAt(Vector3.zero);
            }
        }     
        
    }
    */
    void OnRenderImage (RenderTexture src, RenderTexture dest){
        Init();
        InitRenderTexture();
        SetParameters();
        t = Time.time;


        //distributing the pixels to calculate evenly through the threads
        int gridX = Mathf.CeilToInt ( cam.pixelWidth / 8.0f);
        int gridY = Mathf.CeilToInt ( cam.pixelHeight / 8.0f);
        rayMarcher.Dispatch(KernelID,gridX,gridY,1);

        Graphics.Blit(target,dest);

    }
    
    void InitRenderTexture () {
        if (target == null || target.width != cam.pixelWidth || target.height != cam.pixelHeight) {
            if (target != null) {
                target.Release ();
            }
            target = new RenderTexture (cam.pixelWidth, cam.pixelHeight, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            target.enableRandomWrite = true;
            target.Create ();
        }
    }
    
    void SetParameters () {
        rayMarcher.SetFloat("_Pow",power);
        rayMarcher.SetInt("_Iterations", iterations);
        rayMarcher.SetBool("_julia",julia);
        rayMarcher.SetBool("_Cross",cross);
        rayMarcher.SetVector("_C",c);
        rayMarcher.SetVector("_col1",color_1);
        rayMarcher.SetVector("_col2",color_2);
        rayMarcher.SetVector("_col3",color_3);
        rayMarcher.SetVector("_iColor",interior_color);
        rayMarcher.SetVector("_BgColor",bg_color);
        rayMarcher.SetTexture(0,"Dest",target);
        rayMarcher.SetFloat("_Time", Time.time);
        rayMarcher.SetMatrix ("_CameraToWorld", cam.cameraToWorldMatrix);
        rayMarcher.SetMatrix ("_CameraInverseProjection", cam.projectionMatrix.inverse);
        rayMarcher.SetVector ("_LightDirection", lightSource.transform.forward);

    }
}