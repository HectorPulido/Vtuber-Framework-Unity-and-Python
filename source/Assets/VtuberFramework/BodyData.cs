using UnityEngine;

[System.Serializable]
public struct BodyData
{
    public enum BodyParts : int
    {
        right_elbow = 3,
        left_elbow = 6,
        right_wri = 4,
        left_wri = 7,
        neck = 1,
        head = 0
    }

    public Vector2 scale;
    public Vector3 offset;

    [HideInInspector] public Vector3 neck;
    [HideInInspector] public Vector3 rightWri;
    [HideInInspector] public Vector3 leftWri;
    [HideInInspector] public Vector3 leftElbow;
    [HideInInspector] public Vector3 rightElbow;
    [HideInInspector] public Vector3 head;

    [HideInInspector] public bool rightWriIsActive;
    [HideInInspector] public bool leftWriIsActive;
    [HideInInspector] public bool leftElbowIsActive;
    [HideInInspector] public bool rightElbowIsActive;
    [HideInInspector] public bool headIsActive;
    [HideInInspector] public Vector3 neckPos;
    public int[][] debugData;

    public void SetNodes(int[][] data)
    {
        if (data.Length == 0)
        {
            return;
        }

        debugData = data;
        Debug.Log("new data");

        //Neck
        neckPos = GetNodeVector(data[(int)BodyParts.neck]) - offset;
        neck = offset;// 
        rightWri = GetNodeVector(data[(int)BodyParts.right_wri]) - neckPos;
        leftWri = GetNodeVector(data[(int)BodyParts.left_wri]) - neckPos;
        rightElbow = GetNodeVector(data[(int)BodyParts.right_elbow]) - neckPos;
        leftElbow = GetNodeVector(data[(int)BodyParts.left_elbow]) - neckPos;
        head = GetNodeVector(data[(int)BodyParts.head]) - neckPos;

        rightElbowIsActive = DataWasDetected(data[(int)BodyParts.right_elbow]);
        leftElbowIsActive = DataWasDetected(data[(int)BodyParts.left_elbow]);
        rightWriIsActive = DataWasDetected(data[(int)BodyParts.right_wri]);
        leftWriIsActive = DataWasDetected(data[(int)BodyParts.left_wri]);
        headIsActive = DataWasDetected(data[(int)BodyParts.head]);
    }

    public Vector3 GetNodeVector(int[] data)
    {
        return new Vector3(data[0] / scale.x, data[1] / scale.y);
    }

    private bool DataWasDetected(int[] data)
    {
        return !(data[0] == -1 && data[1] == -1);
    }
}
