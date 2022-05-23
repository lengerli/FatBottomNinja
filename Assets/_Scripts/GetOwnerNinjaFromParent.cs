using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetOwnerNinjaFromParent : MonoBehaviour
{
    WeaponDamage weaponDamageComponent;

    private void OnEnable()
    {
        weaponDamageComponent = GetComponent<WeaponDamage>();
        weaponDamageComponent.OwnerNinja = transform.parent.gameObject.GetComponent<WeaponDamage>().OwnerNinja;
    }
}
