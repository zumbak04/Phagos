﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Camera cam;

    GameObject focusObj;

    [SerializeField]
    //bool isPanning;
    private float panSpeed = 20f;
    private float panBorderThickness = 10f;
    [SerializeField]
    private float panningTimer;
    private float secsBeforeSpeedUp = 0.5f;
    private float speedupFactor = 2f;
    private Vector3 moveDirection;

    [SerializeField]
    bool isDragging;
    Vector3 dragOrigin = Vector3.zero;
    float dragSpeed = 0.3f;

    private float scrollSpeed = 1000f;
    private float minCameraSize = 5f;
    private float maxCameraSize = 20f;

    private float PanSpeed
    {
        get
        {
            if (panningTimer >= secsBeforeSpeedUp)
                return panSpeed * speedupFactor;
            else
                return panSpeed;
        }
    }

    private void Awake()
    {
        cam = gameObject.GetComponent<Camera>();
    }
    private void Start()
    {
        maxCameraSize = GameManager._instance.LargestGameAreaEdge;
    }

    void Update()
    {
        Vector3 pos = transform.position;

        if (focusObj != null)
        {
            pos.x = focusObj.transform.position.x;
            pos.y = focusObj.transform.position.y;
        }

        //Pan
        if (!isDragging && focusObj == null)
        {
            Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
            moveDirection = Input.mousePosition - screenCenter;
            moveDirection.Normalize();

            Vector2 startBorders = new Vector2(panBorderThickness, panBorderThickness);
            Vector3 endBorders = new Vector2(Screen.width - panBorderThickness, Screen.height - panBorderThickness);

            if (Input.mousePosition.x >= endBorders.x || Input.mousePosition.y >= endBorders.y || Input.mousePosition.x <= startBorders.x || Input.mousePosition.y <= startBorders.y)
            {
                //isPanning = true;

                pos += PanSpeed * Time.deltaTime * moveDirection;
                panningTimer += Time.deltaTime;
            }
            else
            {
                //isPanning = false;

                panningTimer = 0;
            }
        }

        //Drag
        if (Input.GetMouseButtonDown(2))
            dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButton(2))
        {
            isDragging = true;

            Vector3 dragMove = cam.ScreenToWorldPoint(Input.mousePosition) - dragOrigin;
            pos -= dragMove * dragSpeed;

            StopFocusCamera();
        }
        else
            isDragging = false;

        //Scroll
        cam.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * scrollSpeed * Time.deltaTime;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minCameraSize, maxCameraSize);

        //Borders
        float cameraSize = cam.orthographicSize;
        pos.x = Mathf.Clamp(pos.x, -(GameManager._instance.gameArea.x - cameraSize), GameManager._instance.gameArea.x - cameraSize);
        pos.y = Mathf.Clamp(pos.y, -(GameManager._instance.gameArea.y - cameraSize), GameManager._instance.gameArea.y - cameraSize);

        transform.position = pos;
    }

    public void FocusCameraOn(GameObject obj)
    {
        focusObj = obj;
    }
    public void StopFocusCamera()
    {
        focusObj = null;
    }
}
