using UnityEngine;

public class Tooltip : MonoBehaviour
{
    [TextArea(10, 10)]
    public string message;

    public bool onClick = false;
    public bool onMouseOver = false;

    private void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0) && onClick)
        {
            TooltipManager._instance.SetActiveTooltip(this, gameObject);
        }
    }
    private void OnMouseEnter()
    {
        if(onMouseOver)
            TooltipManager._instance.SetActiveTooltip(this, gameObject);
    }
    private void OnMouseExit()
    {
        if (onMouseOver)
            TooltipManager._instance.ClearActiveTooltip();
    }
}
