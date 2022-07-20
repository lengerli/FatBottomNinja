using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NinjaHumanPlayerInput : MonoBehaviour, INinjaInputInterface
{
    public Vector2 MousePosition { get { return Input.mousePosition; } }

    public bool GetKeyDown(KeyCode key)
    {
        return Input.GetKeyDown(key);
    }
    public bool GetKey(KeyCode key)
    {
        return Input.GetKey(key);
    }
    public bool GetKeyUp(KeyCode key)
    {
        return Input.GetKeyUp(key);
    }

    public bool GetMouseButtonUp(int button)
    {
        return Input.GetMouseButtonUp(button);
    }
    public bool GetMouseButtonDown(int button)
    {
        return Input.GetMouseButtonDown(button);
    }
    public bool GetMouseButton(int button)
    {
        return Input.GetMouseButton(button);
    }
}
