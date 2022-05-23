using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurserVisibility : MonoBehaviour
{

    private void Update()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void OnEnable()
    {
        Cursor.visible = true;

    }

    private void OnDisable()
    {
        Cursor.visible = false;
    }



}
