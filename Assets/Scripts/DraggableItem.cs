using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DraggableItem : MonoBehaviour
{
    private Vector3 offset;
    public Vector3 initialPosition;


    [SerializeField] private Color normal, pressed;
    [SerializeField] private SkinnedMeshRenderer body;

    private Vector3 originalPosition;
    private bool isDragging = false;
    private float dragThreshold = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        originalPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            // Perform a raycast from the input position
            Ray ray = Camera.main.ScreenPointToRay(Input.GetMouseButtonDown(0) ? Input.mousePosition : (Vector3)Input.GetTouch(0).position);
            RaycastHit hit;

            // Check if the ray hits this object
            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject)
            {
                // Calculate the offset from the object's position to the touch/mouse position
                offset = hit.point - transform.position;
                body.material.color = pressed;
                // Set a positive offset on the z-axis, slightly above the cursor/finger
                offset.z = Mathf.Abs(offset.z) - 3.5f;
                ChangeColor(pressed);
                // Reset dragging state
                isDragging = false;
            }
        }

        // Check if the mouse is held down or there is a touch
        if (Input.GetMouseButton(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved))
        {
            // Move the object to the new position
            MoveObject();

            // Object is being dragged
            isDragging = true;
        }

        // Check if the mouse is released or touch ends
        if (Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended))
        {
            // If the object was not dragged, reset its position
            if (!isDragging)
            {

                ResetObject();
            }
            ChangeColor(normal);
            Debug.Log("released");
            // Reset dragging state
            isDragging = false;
        }
    }

    private void MoveObject()
    {
        Vector3 targetPosition = GetInputWorldPosition() - offset;
        targetPosition.y = transform.position.y; // Ensure consistent height
        transform.position = targetPosition;
    }
    private void ChangeColor(Color newColor)
    {
        body.material.color = newColor;
    }
    private void OnMouseDown()
    {
        OnPlayerDragStart?.Invoke();
    }

    private void OnMouseUp()
    {
        OnPlayerDragStop?.Invoke();
    }

    private void ResetObject()
    {
        transform.position = originalPosition;
        ChangeColor(normal);
    }

    private Vector3 GetInputWorldPosition()
    {
        if (Input.GetMouseButton(0) || Input.touchCount > 0)
        {
            Vector3 inputPosition = Input.GetMouseButton(0)
                ? Input.mousePosition
                : (Vector3)Input.GetTouch(0).position;

            inputPosition.z = Camera.main.transform.position.y;
            return Camera.main.ScreenToWorldPoint(inputPosition);
        }

        return Vector3.zero;
    }

    public static event Action OnPlayerDragStart, OnPlayerDragStop;
}
