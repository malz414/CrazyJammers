using UnityEngine;
using System;

public class DropdownOpenListener : MonoBehaviour
{
    public Action onDropdownOpen;

    private bool wasActive = false;

    void Update()
    {
        if (gameObject.activeSelf && !wasActive)
        {

            wasActive = true;
            onDropdownOpen?.Invoke();
        }
        else if (!gameObject.activeSelf && wasActive)
        {

            wasActive = false;
        }
    }
}
