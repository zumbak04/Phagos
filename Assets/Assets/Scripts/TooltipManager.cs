using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager _instance;

    public TextMeshProUGUI textObj;
    [HideInInspector]
    public GameObject attachObj = null;
    public GameObject cameraObj;
    private Camera cameraCom;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else if (_instance != this)
            Destroy(gameObject);

        cameraCom = cameraObj.GetComponent<Camera>();
    }

    private void Start()
    {
        Cursor.visible = true;
        gameObject.SetActive(false);
    }

	private void Update()
    {
        if(attachObj != null)
            transform.position = cameraCom.WorldToScreenPoint(attachObj.transform.position);
    }

    public void SetAndShow(string message, GameObject gameObj)
    {
        attachObj = gameObj;
        gameObject.SetActive(true);
        textObj.text = message;
    }
    public void Hide()
    {
        attachObj = null;
        gameObject.SetActive(false);
        textObj.text = string.Empty;
    }
}
