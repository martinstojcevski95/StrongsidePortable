using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DraggableItem : MonoBehaviour
{
    private Vector3 offset;
    private Vector3 initialPosition;
    private bool isDragging = false;

    [SerializeField] private Color normal, pressed;
    [SerializeField] private SkinnedMeshRenderer body;

    private void Awake()
    {
        initialPosition = transform.position;
        body.material.color = normal;
    }

    private void OnMouseDown()
    {
        offset = transform.position - GetInputWorldPosition();
        isDragging = true;
        body.material.color = pressed;
        initialPosition = transform.position;
        OnPlayerDragStart?.Invoke();
    }

    private void OnMouseUp()
    {
        body.material.color = normal;
        isDragging = false;
        OnPlayerDragStop?.Invoke();
    }

    private void Update()
    {
        if (isDragging)
        {
            Vector3 targetPosition = GetInputWorldPosition() + offset;
            targetPosition.y = transform.position.y;
            transform.position = targetPosition;
        }
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

