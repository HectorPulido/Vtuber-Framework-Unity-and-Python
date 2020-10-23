using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VTuberMovement : MonoBehaviour
{
    public Transform mainNode;

    public Vector2 offset;
    public Vector2 scaling;
    public float z;

    public string[] bodyParts = new string[]{
        "nose", "neck",
        "r_sho", "r_elb", "r_wri", "l_sho", "l_elb", "l_wri",
        "r_hip", "r_knee", "r_ank", "l_hip", "l_knee", "l_ank",
        "r_eye", "l_eye",
        "r_ear", "l_ear"
    };

    public bodypart testPlaceHolder;
    public void ShowTrackingData(int[][] data)
    {

        var lastBp = GameObject.FindObjectsOfType<bodypart>();

        foreach (var item in lastBp)
        {
            Destroy(item.gameObject);
        }

        for (int i = 0; i < data.Length; i++)
        {
            if (data[i][0] == -1 && data[i][1] == -1)
            {
                continue;
            }

            Vector3 nodePostion = new Vector3(data[i][0] / scaling.x + offset.x, data[i][1] / scaling.y + offset.y, z);

            if (i == 1)
            {
                mainNode.position = nodePostion;
            }

            bodypart instantiatedObject = Instantiate(testPlaceHolder, nodePostion, Quaternion.identity);
            instantiatedObject.Setup(bodyParts[i]);
        }

    }

}
