using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager _instance;

    public TextMeshProUGUI textObj;
    public GameObject cameraObj;
    private Camera cameraCom;

    private Tooltip activeTooltip;

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
        textObj.text = activeTooltip.message;

        transform.position = cameraCom.WorldToScreenPoint(activeTooltip.gameObject.transform.position);
    }

    public void SetActiveTooltip(Tooltip tooltip, GameObject gameObj)
    {
        activeTooltip = tooltip;
        gameObject.SetActive(true);
    }
    public void ClearActiveTooltip()
    {
        activeTooltip = null;
        gameObject.SetActive(false);
        textObj.text = string.Empty;
    }
}
