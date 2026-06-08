using UnityEngine;
using UnityEngine.InputSystem;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private GameObject _shopUI;

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        {
            ToggleShop();
        }
    }

    public void ToggleShop()
    {
        bool shouldBeActive = !_shopUI.activeSelf;

        _shopUI.SetActive(shouldBeActive);

        if (shouldBeActive)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}