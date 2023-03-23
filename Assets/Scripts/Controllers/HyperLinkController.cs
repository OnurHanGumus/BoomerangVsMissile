using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HyperLinkController : MonoBehaviour
{
    public void Clicked(string text)
    {
        Application.OpenURL(text);
    }

}
