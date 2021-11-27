using UnityEngine;

public class Tooltip : MonoBehaviour
{
    [TextArea(10, 10)]
    public string message;

    private void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TooltipManager._instance.SetActiveTooltip(this);
            GameManager._instance.camCtrl.FocusCameraOn(gameObject);
        }
    }
}
