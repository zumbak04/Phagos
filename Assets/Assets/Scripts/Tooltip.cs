using UnityEngine;

public class Tooltip : MonoBehaviour
{
    public string message;

    public bool onClick = false;
    public bool onMouseOver = false;

    private void OnMouseEnter()
    {
        if(onMouseOver)
            TooltipManager._instance.SetAndShow(message, gameObject);
    }
    private void OnMouseExit()
    {
        if (onMouseOver)
            TooltipManager._instance.Hide();
    }
}
