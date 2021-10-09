using UnityEngine;

public class Tooltip : MonoBehaviour
{
    public string message;

    private void OnMouseEnter()
    {
        TooltipManager._instance.SetAndShow(message);
    }
    private void OnMouseExit()
    {
        TooltipManager._instance.Hide();
    }
}
