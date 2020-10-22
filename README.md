# 범죄현장 재구성

개요

VRP 사용자에게 현장, 증거물 사진을 입력받아 일치하는 좌표를 웹 클라이언트에게 반환하는 어플리케이션입니다.

 

구동 환경

- Unity 5(C#) - 2019.4.9f
- python - 3.7 이하

## Unity Settings

**기능**

파노라마 부분 이미지 추출 (8*8장)

**전제 조건**

- 다음 두 경로가 존재해야 함.
    - Assets/requestScreenShots (input directory)
    - Assets/screenShots (output directory)
- Assets/requestScreenShots 경로에 최소 한장 이상의 파노라마 이미지가 존재해야 함
- 회전 순서는 Yaw, Pitch 순이어야 합니다.(바뀌면 새로운 수식 필요.)

**Unity Camera Parameter**

- Projection type - Perspective
- HFOV : 90
- VFOV : 90

 

**Geometry**

- Geometry Type : Cubemap
- Skybox Material : Skybox/Cubemap

**작동 원리**

- 이미지를  2d 텍스쳐로 변환
- 텍스쳐를 geometry cubemap 에 매핑
- Skybox에 geometry(material) 매핑
- 플레이어 카메라로 회전하며 촬영
    - Rotate ΔYaw : 45° , ΔPitch : 45°

```csharp
private void MakePanoramaPartialImage()
    {
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
                string filename = ScreenShotName(i, j);

                System.IO.File.WriteAllBytes(filename, bytes);
                
								//Rotate 45 degrees around axis "pitch"
                transform.Rotate(45, 0, 0);               

            }
						//Rotate 45 degrees around axis "yaw"
            transform.Rotate(0, 45, 0);

        }
    }
```

**결과 예시**

![input_panorama_image.jpg](./readme_image/input_panorama_image.jpg)

<원본 파노라마 이미지 예시>

![input_partial_image.jpg](./readme_image/input_partial_image.png)

<자른 부분 이미지 예시>

## Python Settings

**기능**

파노라마 이미지 - 부분 이미지 유사 지점 추출

**전제 조건**

- 라이브러리
    - opencv-contrib-python==3.4.2.16(python 3.7이하에서만 설치 가능)
    - opencv-python==3.4.2.16
- 빌드된 유니티 어플리케이션이  최초 한번 실행되어야 함.

### Feature Matching

SIFT 특징점 매칭 알고리즘이 사용됨.

이미지간 유사 지점을 keypoint로 추출 후 반환.

```python
kp1, des1 = sift.detectAndCompute(img1,None)
kp2, des2 = sift.detectAndCompute(img2,None)

bf = cv2.BFMatcher()
matches = bf.knnMatch(des1,des2, k=2)

detected = []
for m,n in matches:
    if m.distance < 0.4*n.distance:
        detected.append([m])

img3 = cv2.drawMatchesKnn(img1,kp1,img2,kp2,detected,None,flags=2)
```

결과 예시

![feature_matching_output.jpg](./readme_image/feature_matching_output.png)

**개선 사항**

- sift 의 느린 속도
- 최적의 정확도 선정 필요(현재 0.4~0.5)
- 이미지간 In - Plane Rotation이 존재하는 경우, Feature Matching 적용 불가능.
