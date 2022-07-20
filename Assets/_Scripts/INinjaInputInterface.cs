using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INinjaInputInterface
{
    public Vector2 MousePosition { get; }
    public bool GetKeyDown(KeyCode key);
    public bool GetKey(KeyCode key);
    public bool GetKeyUp(KeyCode key);

    public bool GetMouseButtonUp(int button);
    public bool GetMouseButtonDown(int button);
    public bool GetMouseButton(int button);

}
