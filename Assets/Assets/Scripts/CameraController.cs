using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Camera cameraObj;

    [SerializeField]
    bool isPanning;
    private float panSpeed = 10f;
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

    private float scrollSpeed = 500f;
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
        cameraObj = gameObject.GetComponent<Camera>();
    }

    void Update()
    {
        Vector3 pos = transform.position;

        //Pan
        if (!isDragging)
        {
            Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
            moveDirection = Input.mousePosition - screenCenter;
            moveDirection.Normalize();

            Vector2 startBorders = new Vector2(panBorderThickness, panBorderThickness);
            Vector3 endBorders = new Vector2(Screen.width - panBorderThickness, Screen.height - panBorderThickness);

            if (Input.mousePosition.x >= endBorders.x || Input.mousePosition.y >= endBorders.y || Input.mousePosition.x <= startBorders.x || Input.mousePosition.y <= startBorders.y)
            {
                isPanning = true;

                pos += PanSpeed * Time.deltaTime * moveDirection;
                panningTimer += Time.deltaTime;
            }
            else
            {
                isPanning = false;

                panningTimer = 0;
            }
        }

        //Drag
        if (Input.GetMouseButtonDown(2))
            dragOrigin = cameraObj.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButton(2))
        {
            isDragging = true;

            Vector3 dragMove = cameraObj.ScreenToWorldPoint(Input.mousePosition) - dragOrigin;
            pos -= dragMove * dragSpeed;
        }
        else
            isDragging = false;

        //Scroll
        cameraObj.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * scrollSpeed * Time.deltaTime;
        cameraObj.orthographicSize = Mathf.Clamp(cameraObj.orthographicSize, minCameraSize, maxCameraSize);

        //Borders
        float cameraSize = cameraObj.orthographicSize;
        pos.x = Mathf.Clamp(pos.x, -(GameManager.instance.gameArea.x - cameraSize), GameManager.instance.gameArea.x - cameraSize);
        pos.y = Mathf.Clamp(pos.y, -(GameManager.instance.gameArea.y - cameraSize), GameManager.instance.gameArea.y - cameraSize);

        transform.position = pos;
    }
}
