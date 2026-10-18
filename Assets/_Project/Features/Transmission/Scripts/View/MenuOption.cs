using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuOption : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private GameObject check;
    [SerializeField] private GameObject underline;

    private bool _interactable;

    public event Action<MenuOption> Clicked;

    public void ResetState()
    {
        _interactable = false;
        check.SetActive(false);
        underline.SetActive(false);
    }

    public void SetInteractable(bool interactable)
    {
        _interactable = interactable;
    }

    public void Choose()
    {
        check.SetActive(true);
        underline.SetActive(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_interactable)
        {
            underline.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_interactable)
        {
            underline.SetActive(false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_interactable)
        {
            Clicked?.Invoke(this);
        }
    }
}
