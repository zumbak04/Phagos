using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    private float panSpeed = 10f;
    private float panBorderThickness = 10f;

    [SerializeField]
    private bool isMoving = false;

    [SerializeField]
    private float movingTimer;
    private float secsBeforeSpeedUp = 0.75f;
    private float speedupFactor = 2f;

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

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = transform.position;

        if (Input.mousePosition.x >= Screen.width - panBorderThickness)
            pos.x += PanSpeed * Time.deltaTime;
        if (Input.mousePosition.x <= panBorderThickness)
            pos.x -= PanSpeed * Time.deltaTime;
        if (Input.mousePosition.y >= Screen.height - panBorderThickness)
            pos.y += PanSpeed * Time.deltaTime;
        if (Input.mousePosition.y <= panBorderThickness)
            pos.y -= PanSpeed * Time.deltaTime;

        if (pos == transform.position)
            isMoving = false;
        else
            isMoving = true;

        if (isMoving)
            movingTimer += Time.deltaTime;
        else
            movingTimer = 0;

        transform.position = pos;
    }
}
