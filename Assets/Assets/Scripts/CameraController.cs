﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Camera cameraObj;

    private float panSpeed = 15f;
    private float scrollSpeed = 500f;
    private float panBorderThickness = 10f;
    [SerializeField]
    private bool isMoving = false;
    [SerializeField]
    private Vector3 moveDirection;

    [SerializeField]
    private float movingTimer;
    private float secsBeforeSpeedUp = 0.5f;
    private float speedupFactor = 2f;

    private float minCameraSize = 5f;
    private float maxCameraSize = 20f;

    private float PanSpeed
    {
        get
        {
            if (movingTimer >= secsBeforeSpeedUp)
                return panSpeed * speedupFactor;
            else
                return panSpeed;
        }
    }

    private void Awake()
    {
        cameraObj = gameObject.GetComponent<Camera>();
    }

    void Update()
    {
        Vector3 pos = transform.position;

        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        moveDirection = Input.mousePosition - screenCenter;
        moveDirection.Normalize();

        Vector2 startBorders = new Vector2(panBorderThickness, panBorderThickness);
        Vector3 endBorders = new Vector2(Screen.width - panBorderThickness, Screen.height - panBorderThickness);

        if (Input.mousePosition.x >= endBorders.x || Input.mousePosition.y >= endBorders.y || Input.mousePosition.x <= startBorders.x || Input.mousePosition.y <= startBorders.y)
            pos += PanSpeed * Time.deltaTime * moveDirection;

        cameraObj.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * scrollSpeed * Time.deltaTime;
        cameraObj.orthographicSize = Mathf.Clamp(cameraObj.orthographicSize, minCameraSize, maxCameraSize);

        //Is it moving?
        if (pos.x == transform.position.x && pos.y == transform.position.y)
            isMoving = false;
        else
            isMoving = true;

        //Timers
        if (isMoving)
            movingTimer += Time.deltaTime;
        else
            movingTimer = 0;

        //Borders
        float cameraSize = cameraObj.orthographicSize;
        pos.x = Mathf.Clamp(pos.x, -(GameManager.instance.gameArea.x - cameraSize), GameManager.instance.gameArea.x - cameraSize);
        pos.y = Mathf.Clamp(pos.y, -(GameManager.instance.gameArea.y - cameraSize), GameManager.instance.gameArea.y - cameraSize);

        transform.position = pos;
    }
}
