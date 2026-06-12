using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWeapons : MonoBehaviour
{
    [SerializeField] private List<Sprite> weaponsUI;
    [SerializeField] private GameObject weaponHolder;
    public void SwitchWeaponUI(int toWeapon)
    {
        if (weaponsUI == null || toWeapon < 0 || toWeapon >= weaponsUI.Count)
        {
            Debug.LogWarning($"UIWeapons: weapon index {toWeapon} is out of range for weaponsUI list (count: {weaponsUI.Count})");
            return;
        }
        weaponHolder.GetComponent<Image>().sprite = weaponsUI[toWeapon];
    }
}
