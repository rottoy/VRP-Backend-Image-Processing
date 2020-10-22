using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AutoRotator : MonoBehaviour
{
    // Start is called before the first frame update

    private int resWidth;
    private int resHeight;
    private bool takeHiResShot = false;
    private string outputDirectory;
    private string inputDirectory;

    private Texture2D source;
    private Texture2D texture2d;
    private Cubemap cubemap;
    public int CubemapResolution;
    private String arr;

    [System.NonSerialized]
    public String panoramaImage= "sinput6";

    [System.NonSerialized]
    public String panoramaPartialImage= "screen";
    /// <summary>
    /// These are the faces of a cube
    /// </summary>
    private Vector3[][] faces =
    {
         new Vector3[] {
             new Vector3(1.0f, 1.0f, -1.0f),
             new Vector3(1.0f, 1.0f, 1.0f),
             new Vector3(1.0f, -1.0f, -1.0f),
             new Vector3(1.0f, -1.0f, 1.0f)
         },
         new Vector3[] {
             new Vector3(-1.0f, 1.0f, 1.0f),
             new Vector3(-1.0f, 1.0f, -1.0f),
             new Vector3(-1.0f, -1.0f, 1.0f),
             new Vector3(-1.0f, -1.0f, -1.0f)
         },
         new Vector3[] {
             new Vector3(-1.0f, 1.0f, 1.0f),
             new Vector3(1.0f, 1.0f, 1.0f),
             new Vector3(-1.0f, 1.0f, -1.0f),
             new Vector3(1.0f, 1.0f, -1.0f)
         },
         new Vector3[] {
             new Vector3(-1.0f, -1.0f, -1.0f),
             new Vector3(1.0f, -1.0f, -1.0f),
             new Vector3(-1.0f, -1.0f, 1.0f),
             new Vector3(1.0f, -1.0f, 1.0f)
         },
         new Vector3[] {
             new Vector3(-1.0f, 1.0f, -1.0f),
             new Vector3(1.0f, 1.0f, -1.0f),
             new Vector3(-1.0f, -1.0f, -1.0f),
             new Vector3(1.0f, -1.0f, -1.0f)
         },
         new Vector3[] {
             new Vector3(1.0f, 1.0f, 1.0f),
             new Vector3(-1.0f, 1.0f, 1.0f),
             new Vector3(1.0f, -1.0f, 1.0f),
             new Vector3(-1.0f, -1.0f, 1.0f)
         }
     };

    void Awake() // Settings of Directory and Resolution
    {
        //panoramaImage = "sinput4";
        //panoramaPartialImage = "screen";
        Debug.Log("파노라마 이미지 이름은 다음과 같이 설정되어있습니다 : " + panoramaImage);
        Debug.Log("파노라마 부분 이미지 이름은 다음과 같이 설정되어있습니다 : " + panoramaPartialImage);
        resWidth = Screen.width;
        resHeight = Screen.height;
        outputDirectory = string.Format("{0}/screenShots", Application.dataPath);
        inputDirectory = string.Format("{0}/requestScreenShots", Application.dataPath);

    }
    
    void Start()
    {
       
        CreateScreenShotDirectory();

        FindImageFromDirectory();
        InitializeCubemap();

        MakePanoramaPartialImage();
        QuitApplication();

       
            Debug.Log(arr);
       
       
        
        
    }

    private void CreateScreenShotDirectory()
    {
        Debug.Log("==================================");
        Debug.Log("프로젝트 경로에 디렉토리를 생성합니다... 출력 디렉토리 : " + outputDirectory);
        Debug.Log("프로젝트 경로에 디렉토리를 생성합니다... 입력 디렉토리 : " + inputDirectory);
        System.IO.Directory.CreateDirectory(outputDirectory);
        System.IO.Directory.CreateDirectory(inputDirectory);
    }

    private void QuitApplication()
    {
        Debug.Log("프로젝트를 종료합니다");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

   
 
    public string ScreenShotName(int width, int height)
    {

        return string.Format("{0}/screenShots/" + panoramaPartialImage + "_{1}_{2}.png",
                             Application.dataPath,
                             width, height
                            );
    }
    
    private void FindImageFromDirectory()
    {
        Debug.Log("==================================");
        //path find

        // string path = string.Format("{0}/requestScreenShots/panorama_image.jpg", Application.dataPath);

        string path = string.Format("{0}/requestScreenShots/"+panoramaImage+".jpg", Application.dataPath);
        Debug.Log("전체 파노라마 이미지 찾기. 경로 : "+path);
        byte[] byteTexture = System.IO.File.ReadAllBytes(path);


        source = new Texture2D(1024, 512);
        
        //if image found
        if (byteTexture.Length > 0)
        {
            Debug.Log("파노라마 이미지를 찾았습니다. 이미지 용량 : "+byteTexture.Length + "kilobytes.");
            source.LoadImage(byteTexture);
            Debug.Log("이미지를 유니티 텍스쳐로 변환했습니다.");
        }
        return ;
    }

    private void SetSkyboxFromCubemap()// Set Texture of Material and Apply to Skybox
    {
        Debug.Log("==================================");
        Debug.Log("큐브맵을 배경 스카이박스로 초기화 합니다.");
        Material cubeMapMaterial = new Material(Shader.Find("Skybox/Cubemap"));
        cubeMapMaterial.SetTexture("_Tex", cubemap);
        RenderSettings.skybox = cubeMapMaterial;
    }

    private void InitializeCubemap()
    {
        Debug.Log("==================================");
        Debug.Log("큐브맵을 초기화 합니다.");
        cubemap = new Cubemap(CubemapResolution, TextureFormat.RGBA32, false);

        Color[] CubeMapColors;

        for (int i = 0; i < 6; i++)
        {
            CubeMapColors = CreateCubemapTexture(CubemapResolution, (CubemapFace)i);
            cubemap.SetPixels(CubeMapColors, (CubemapFace)i);
        }

        // we set the cubemap from the texture pixel by pixel
        cubemap.Apply();

        //Destroy all unused textures
        DestroyImmediate(source);
        
        Texture2D[] texs = FindObjectsOfType<Texture2D>();
        for (int i = 0; i < texs.Length; i++)
        {
            DestroyImmediate(texs[i]);
        }
        
        SetSkyboxFromCubemap();
    }
    private void MakePanoramaPartialImage()
    {
        Debug.Log("==================================");
        Debug.Log("파노라마 이미지를 생성합니다......");
        for (int yaw = 0; yaw < 8; yaw++)
        {
            for (int pitch = 0; pitch < 8; pitch++)
            {
                int cur_count = yaw * 8 + pitch;
                RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
                GetComponent<Camera>().targetTexture = rt;
                Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
                GetComponent<Camera>().Render();
                RenderTexture.active = rt;
                screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
                GetComponent<Camera>().targetTexture = null;
                RenderTexture.active = null; // JC: added to avoid errors
                Destroy(rt);

                // 파일로 저장.
                byte[] bytes = screenShot.EncodeToPNG();
                string filename = ScreenShotName(yaw, pitch);
                //Debug.Log("****** :" + filename);
                //Debug.Log(transform.rotation.eulerAngles);
                System.IO.File.WriteAllBytes(filename, bytes);
                Debug.Log(string.Format("Took screenshot to:{0}, {1}, {2}", yaw, pitch, transform.eulerAngles));
                arr += string.Format("({0}, {1}): ", yaw, pitch);

                arr =arr+ transform.eulerAngles.ToString();

                arr += "\n";
                
                transform.Rotate(45, 0, 0);
                


            }
            transform.Rotate(0, 45, 0);

        }
    }

    /// <summary>
    /// Generates a Texture that represents the given face for the cubemap.
    /// </summary>
    /// <param name="resolution">The targetresolution in pixels</param>
    /// <param name="face">The target face</param>
    /// <returns></returns>
    private Color[] CreateCubemapTexture(int resolution, CubemapFace face)
    {
        Texture2D texture = new Texture2D(resolution, resolution, TextureFormat.RGB24, false);

        Vector3 texelX_Step = (faces[(int)face][1] - faces[(int)face][0]) / resolution;
        Vector3 texelY_Step = (faces[(int)face][3] - faces[(int)face][2]) / resolution;

        float texelSize = 1.0f / resolution;
        float texelIndex = 0.0f;

        //Create textured face
        Color[] cols = new Color[resolution];
        for (int y = 0; y < resolution; y++)
        {
            Vector3 texelX = faces[(int)face][0];
            Vector3 texelY = faces[(int)face][2];
            for (int x = 0; x < resolution; x++)
            {
                cols[x] = Project(Vector3.Lerp(texelX, texelY, texelIndex).normalized);
                texelX += texelX_Step;
                texelY += texelY_Step;
            }
            texture.SetPixels(0, y, resolution, 1, cols);
            texelIndex += texelSize;
        }
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();

        Color[] colors = texture.GetPixels();
        DestroyImmediate(texture);

        return colors;
    }

    /// <summary>
    /// Projects a directional vector to the texture using spherical mapping
    /// </summary>
    /// <param name="direction">The direction in which you view</param>
    /// <returns></returns>
    private Color Project(Vector3 direction)
    {
        float theta = Mathf.Atan2(direction.z, direction.x) + Mathf.PI / 180.0f;
        float phi = Mathf.Acos(direction.y);

        int texelX = (int)(((theta / Mathf.PI) * 0.5f + 0.5f) * source.width);
        if (texelX < 0) texelX = 0;
        if (texelX >= source.width) texelX = source.width - 1;
        int texelY = (int)((phi / Mathf.PI) * source.height);
        if (texelY < 0) texelY = 0;
        if (texelY >= source.height) texelY = source.height - 1;

        return source.GetPixel(texelX, source.height - texelY - 1);
    }
}
