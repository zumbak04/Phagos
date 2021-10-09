using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager _instance;

    public TextMeshProUGUI textObj;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else if (_instance != this)
            Destroy(gameObject);
    }

    private void Start()
    {
        Cursor.visible = true;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        transform.position = Input.mousePosition;
    }

    public void SetAndShow(string message)
    {
        gameObject.SetActive(true);
        textObj.text = message;
    }
    public void Hide()
    {
        gameObject.SetActive(false);
        textObj.text = string.Empty;
    }
}
