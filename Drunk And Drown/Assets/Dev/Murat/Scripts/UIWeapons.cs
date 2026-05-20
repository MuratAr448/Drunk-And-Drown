using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIWeapons : MonoBehaviour
{
    [SerializeField] private List<Sprite> weaponsUI;
    [SerializeField] private GameObject weaponHolder;
    public void SwitchWeaponUI(int toWeapon)
    {
        weaponHolder.GetComponent<Image>().sprite = weaponsUI[toWeapon];
    }
}
