using UnityEngine;
using UnityEngine.EventSystems;
using FMODUnity;

public class ButtonAudio : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, ISelectHandler, ISubmitHandler
{
    [SerializeField] private EventReference hoverSound;
    [SerializeField] private EventReference clickSound;

    // Called when the mouse pointer enters (for mouse users)
    public void OnPointerEnter(PointerEventData eventData)
    {
        AudioManager.instance.PlayOneShotUI(hoverSound);
    }

    // Called when the mouse pointer clicks (for mouse users)
    public void OnPointerClick(PointerEventData eventData)
    {
        AudioManager.instance.PlayOneShotUI(clickSound);
    }

    // Called when this UI element is selected by a keyboard/controller
    public void OnSelect(BaseEventData eventData)
    {
        AudioManager.instance.PlayOneShotUI(hoverSound);
    }

    // Called when this UI element is activated/submitted (for controller/keyboard users)
    public void OnSubmit(BaseEventData eventData)
    {
        AudioManager.instance.PlayOneShotUI(clickSound);
    }
}
