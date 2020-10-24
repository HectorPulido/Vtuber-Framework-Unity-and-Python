using UnityEngine;

public class VTuberMovement : MonoBehaviour
{
    public Transform rightWri;
    public Transform leftWri;
    public Transform rightElbow;
    public Transform leftElbow;
    public Transform neck;
    public Transform head;
    public BodyData bodyData;

    public void ShowTrackingData(int[][] data)
    {
        bodyData.SetNodes(data);

        rightWri.gameObject.SetActive(bodyData.rightWriIsActive);
        rightWri.position = bodyData.rightWri;

        leftWri.gameObject.SetActive(bodyData.leftWriIsActive);
        leftWri.position = bodyData.leftWri;

        rightElbow.gameObject.SetActive(bodyData.rightElbowIsActive);
        rightElbow.position = bodyData.rightElbow;

        leftElbow.gameObject.SetActive(bodyData.leftElbowIsActive);
        leftElbow.position = bodyData.leftElbow;

        head.gameObject.SetActive(bodyData.headIsActive);
        head.position = bodyData.head;
        
        neck.position = bodyData.neck;

    }

    private void OnDrawGizmos()
    {
        if (bodyData.debugData == null)
        {
            return;
        }

        for (int i = 0; i < bodyData.debugData.Length; i++)
        {
            if (bodyData.debugData[i][0] == -1 || bodyData.debugData[i][1] == -1)
            {
                continue;
            }

            var pos = bodyData.GetNodeVector(bodyData.debugData[i]) - bodyData.neckPos;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(pos, 0.05f);
        }
    }

}
