using UnityEngine;
using UnityEngine.InputSystem;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private GameObject _shopUI;

    public void ToggleShop(bool isActive)
    {
        _shopUI.SetActive(isActive);

        if (isActive)
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