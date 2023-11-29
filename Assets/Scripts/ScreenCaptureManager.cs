using StrongSideAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class ScreenCaptureManager : MonoBehaviour
{
    public static ScreenCaptureManager instance { get; private set; }

    [SerializeField] List<Transform> offenseCaptrePositions = new List<Transform>();
    int captureCount = 0;
    private List<byte[]> imagesBytes = new List<byte[]>();
    private Vector3 cameraInitPos;
    private Quaternion cameraInitRot;
    public static event Action OnCaptureFinished;

    private void Awake()
    {
        if (instance != null && instance != this)
            Destroy(instance);
        else
            instance = this;
    }

    private void Start()
    {
        cameraInitPos = Camera.main.transform.position;
        cameraInitRot = Camera.main.transform.rotation;

        captureCount = 0;
    }

    private void ChangeCameraPosition(Transform newPos)
    {
        Camera.main.transform.position = newPos.position;
        Camera.main.transform.rotation = newPos.rotation;
    }

    public void StartCapture(string formationId)
    {
        captureCount = 0;
        LoadingManager.Instance.CloseLoadingScreen("");
        StartCoroutine(CaptureScreenAndSaveCoroutine(formationId, CaptureScreenshotsFinished));
    }

    private void CaptureScreenshotsFinished(List<byte[]> list, string formationId)
    {
        captureCount = 0;
        StartCoroutine(UploadScreenShotToDb(OnScreenshotsUploaded, formationId));
    }

    private void OnScreenshotsUploaded()
    {
        LoadingManager.Instance.CloseLoadingScreen("Uploading captures finished!", 1f);
        imagesBytes.Clear();
        OnCaptureFinished?.Invoke();
    }

    IEnumerator CaptureScreenAndSaveCoroutine(string formationId, Action<List<byte[]>, string> callback)
    {

        while (captureCount < offenseCaptrePositions.Count)
        {
            ChangeCameraPosition(offenseCaptrePositions[captureCount]);
            yield return new WaitForEndOfFrame();

            int width = Screen.width;
            int height = Screen.height;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            byte[] screenshotBytes = tex.EncodeToPNG();
            imagesBytes.Add(screenshotBytes);
            captureCount++;
            Destroy(tex);
        }

        captureCount = 0;
        callback?.Invoke(imagesBytes, formationId);
    }


    private IEnumerator UploadScreenShotToDb(Action callback, string formationId)
    {
        while (captureCount < imagesBytes.Count)
        {
            WWWForm form = new WWWForm();

            form.AddField("FormationId", formationId);
            form.AddBinaryData("files", imagesBytes[captureCount], Guid.NewGuid() + ".png");

            UnityWebRequest uwr = UnityWebRequest.Post("https://strongside-api-prod.azurewebsites.net/web-api/api/v2/formation-add-screenshots", form);
            uwr.SetRequestHeader("Authorization", "Bearer " + AppManager.Instance.Token);
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
            {
                print(uwr.error);
            }
            else
            {
                print(uwr.downloadHandler.text);
            }
            captureCount++;
        }


        callback?.Invoke();
    }

}



