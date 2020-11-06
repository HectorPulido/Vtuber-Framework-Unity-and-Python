using UnityEngine;

[System.Serializable]
public struct BodyData
{
    public enum BodyParts : int
    {
        right_eye = 14,
        left_eye = 15,
        right_elbow = 3,
        left_elbow = 6,
        right_wri = 4,
        left_wri = 7,
        neck = 1,
        head = 0
    }

    public Vector2 scale;
    public Vector3 offset;

    public float distanceZMult;
    public float headAngleOffset;
    public float headAngleMult;

    [HideInInspector] public Vector3 neck;
    [HideInInspector] public Vector3 rightWri;
    [HideInInspector] public Vector3 leftWri;
    [HideInInspector] public Vector3 leftElbow;
    [HideInInspector] public Vector3 rightElbow;
    [HideInInspector] public Vector3 head;
    [HideInInspector] public float headRotation;
    [HideInInspector] public bool rightWriIsActive;
    [HideInInspector] public bool leftWriIsActive;
    [HideInInspector] public bool leftElbowIsActive;
    [HideInInspector] public bool rightElbowIsActive;
    [HideInInspector] public bool headIsActive;
    [HideInInspector] public Vector3 neckPos;
    public int[][] debugData;

    public void SetNodes(int[][] poseData, float[] depthData)
    {
        if (poseData.Length == 0)
        {
            return;
        }

        debugData = poseData;

        //Neck
        int[] neckPosition = poseData[(int)BodyParts.neck];
        float neckZ = depthData[(int)BodyParts.neck];
        neckPos = GetNodeVector(neckPosition, neckZ, offset);
        neck = offset;


        //Head
        var headPosition = poseData[(int)BodyParts.head];
        var headPositionZ = depthData[(int)BodyParts.head];

        head = GetHeadPosition(headPosition, headPositionZ, neckPos);
        
        //Arms
        
        var rightWriPosition = poseData[(int)BodyParts.right_wri];
        var rightWriPositionZ = depthData[(int)BodyParts.right_wri];

        rightWri = GetNodeVector(rightWriPosition, rightWriPositionZ, neckPos);
        
        var leftWriPosition = poseData[(int)BodyParts.left_wri];
        var leftWriPositionZ = depthData[(int)BodyParts.left_wri];

        leftWri = GetNodeVector(leftWriPosition, leftWriPositionZ, neckPos);
        
        var rightElbowPosition = poseData[(int)BodyParts.right_elbow];
        var rightElbowPositionZ = depthData[(int)BodyParts.right_elbow];

        rightElbow = GetNodeVector(rightElbowPosition, rightElbowPositionZ, neckPos);
        
        var leftElbowPosition = poseData[(int)BodyParts.left_elbow];
        var leftElbowPositionZ = depthData[(int)BodyParts.left_elbow];

        leftElbow = GetNodeVector(leftElbowPosition, leftElbowPositionZ, neckPos);
        
        
        var rightElbowIsActivePosition = poseData[(int)BodyParts.right_elbow];
        rightElbowIsActive = DataWasDetected(rightElbowIsActivePosition);
        
        var leftElbowIsActivePosition = poseData[(int)BodyParts.left_elbow];
        leftElbowIsActive = DataWasDetected(leftElbowIsActivePosition);
        
        var rightWriIsActivePosition = poseData[(int)BodyParts.right_wri];
        rightWriIsActive = DataWasDetected(rightWriIsActivePosition);
        
        var leftWriIsActivePosition = poseData[(int)BodyParts.left_wri];
        leftWriIsActive = DataWasDetected(leftWriIsActivePosition);
        
        var headIsActivePosition = poseData[(int)BodyParts.head];
        headIsActive = DataWasDetected(headIsActivePosition);

            
        var tEyePosition = poseData[(int)BodyParts.right_eye];
        var tEyePositionZ = depthData[(int)BodyParts.right_eye];
        var rightEye = GetNodeVector(tEyePosition, tEyePositionZ, neckPos);
            
        var EyePosition = poseData[(int)BodyParts.left_eye];
        var EyePositionZ = depthData[(int)BodyParts.left_eye];
        var leftEye = GetNodeVector(EyePosition, EyePositionZ, neckPos);

        headRotation = GetEyesRotation(rightEye, leftEye);
    }

    private float GetEyesRotation(Vector3 rightEye, Vector3 leftEye)
    {
        var diff = rightEye - leftEye;
        return Mathf.Atan(diff.y / diff.x) * Mathf.Rad2Deg;
    }

    private Vector3 GetHeadPosition(int[] data, float dataZ, Vector3 neckPos)
    {
        var headpos = GetNodeVector(data, dataZ, neckPos);
        var headAngle = Mathf.Atan(headpos.x / headpos.y) * headAngleMult + headAngleOffset * Mathf.Deg2Rad;

        return new Vector3(-Mathf.Cos(headAngle), 0, -Mathf.Sin(headAngle)) + headpos;
    }

    public Vector3 GetNodeVector(int[] data, float dataZ, Vector3 offset)
    {
        var nodeVector = new Vector3(data[0] / scale.x, data[1] / scale.y, -dataZ * distanceZMult);
        nodeVector -= offset;
        return nodeVector;
    }

    private bool DataWasDetected(int[] data)
    {
        return !(data[0] == -1 && data[1] == -1);
    }
}
