using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject tooltipObject;
    public TMP_Text tooltipText;
    public string message;

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltipText.text = message;
        tooltipObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipObject.SetActive(false);
    }
}