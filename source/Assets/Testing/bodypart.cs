using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bodypart : MonoBehaviour
{
    public TextMesh textmesh;

    public void Setup(string text)
    {
        textmesh.text = text;
    }
}
