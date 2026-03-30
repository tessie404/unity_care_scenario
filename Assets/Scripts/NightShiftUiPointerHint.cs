using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Play-mode hover text for a UI button. Inspector [Tooltip] does not run in Play.</summary>
public class NightShiftUiPointerHint : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Text _target;
    private string _hint;

    public void Configure(Text target, string hint)
    {
        _target = target;
        _hint = hint ?? "";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_target != null && _hint.Length > 0)
            _target.text = _hint;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_target != null)
            _target.text = "";
    }
}
