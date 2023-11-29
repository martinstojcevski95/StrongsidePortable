using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanAndZoomController : MonoBehaviour
{
    //PUBLIC VARIABLES
    public bool isCameraRepositioningToPlayer = false;
    public bool isPanning;
    public bool InGame;
    public bool isZooming;

    public float speedH = 2.0f;
    public float speedV = 2.0f;


    // public Camera mainCamera;
    // public GameObject customTarget;

    public bool wasOver = false;
    public bool panStarted = false;


    // Use this for initialization
    void Start()
    {
    
        isPanning = true;

    }

    private void OnEnable()
    {
       // OffenseItem.OnItemClicked += EnablePanning;
        DraggableItem.OnPlayerDragStart += PausePanning;
        DraggableItem.OnPlayerDragStop += ResumePanning;
    }

    private void OnDisable()
    {
       // OffenseItem.OnItemClicked -= EnablePanning;
        DraggableItem.OnPlayerDragStart -= PausePanning;
        DraggableItem.OnPlayerDragStop -= ResumePanning;

    }

   // private void EnablePanning(APIManager.SingleFormationResponse obj)
   // {
   //     ResumePanning();
   // }

    public void PausePanning()
    {
        isZooming = false;
        InGame = false;
    }

    public void ResumePanning()
    {
        isZooming = true;
        InGame = true;
    }


    // Update is called once per frame
    void Update()
    {
        //ZOOM IN/OUT
        if (Camera.main != null && isZooming)
        {
            float zoomRate = 0;
#if UNITY_IOS || UNITY_ANDROID
            if (Input.touchCount == 2)
            {
                // Zoom IN/OUT using two touches on screen
                Touch touchA = Input.GetTouch(0);
                Touch touchB = Input.GetTouch(1);
                Vector2 touchAPrevPos = touchA.position - touchA.deltaPosition;
                Vector2 touchBPrevPos = touchB.position - touchB.deltaPosition;
                float prevMagnitude = (touchAPrevPos - touchBPrevPos).magnitude;
                float currentMagnitude = (touchA.position - touchB.position).magnitude;

                float difference = currentMagnitude - prevMagnitude;
                zoomRate = difference * 0.05f;
            }
#else
            zoomRate = Input.GetAxis("Mouse ScrollWheel");
#endif

            // -------------------Code for Zooming Out------------
            if (zoomRate < 0)
            {
                if (Camera.main.fieldOfView <= 30f)
                    Camera.main.fieldOfView += 0.3f;
                if (Camera.main.orthographicSize <= 20)
                    Camera.main.orthographicSize += 0.3f;
            }

            // ---------------Code for Zooming In------------------------
            if (zoomRate > 0)
            {
                if (Camera.main.fieldOfView > 6)
                    Camera.main.fieldOfView -= 0.3f;
                if (Camera.main.orthographicSize >= 1)
                    Camera.main.orthographicSize -= 0.3f;
            }


            //PANNING
            if (InGame)
            {
#if UNITY_IOS || UNITY_ANDROID
                var inputDown = Input.GetMouseButtonDown(0);
                var input = Input.GetMouseButton(0);
                var inputUp = Input.GetMouseButtonUp(0);
#else
                var inputDown = Input.GetMouseButtonDown(1);
                var input = Input.GetMouseButton(1);
                var inputUp = Input.GetMouseButtonUp(1);
#endif
                if (isPanning)
                {

                    if (inputDown)
                    {
                        wasOver = false;
                        lastPosition = Input.mousePosition;
                        panStarted = true;
                        // customTarget.transform.position = cameraSystem.GetTarget().position;
                        // customTarget.transform.rotation = cameraSystem.GetTarget().rotation;
                    }
                }

                if (input && panStarted && isPanning)
                {
                    var Vector3 = Input.mousePosition - lastPosition;
                    transform.Translate(Vector3 * mouseSensitivity * Time.deltaTime, Space.Self);
                    transform.position = new Vector3(
                      Mathf.Clamp(transform.position.x, -13f, 13f),
                      Mathf.Clamp(transform.position.y, transform.position.y, transform.position.y),
                      Mathf.Clamp(transform.position.z, -40, 40));               

                }

                if (inputUp && isPanning)
                {
                    panStarted = false;
                }
            }
        }
    }

    //PRIVATE VARIABLES 

    private float yaw = 0.0f;
    private float pitch = 0.0f;

    float mouseSensitivity = .070f;
    Vector3 lastPosition;
}


