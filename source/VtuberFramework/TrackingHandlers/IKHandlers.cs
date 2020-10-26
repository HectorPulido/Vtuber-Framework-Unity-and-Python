using UnityEngine;

[RequireComponent(typeof(Animator))]
public class IKHandlers : MonoBehaviour
{
    private Animator anim;
    public Transform rightHand = null;
    public Transform leftHand = null;
    public Transform rightElbow = null;
    public Transform leftElbow = null;
    public Transform head = null;
    public Transform characterPosition;
    public Vector3 characterOffset;

    public float lookWeight = 1;
    public float bodyWeight = 0.25f;
    public float headWeight = 0.9f;
    public float eyesWeight = 1;
    public float clampWeight = 1;


    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        transform.position = characterPosition.position + characterOffset;
    }

    void OnAnimatorIK()
    {
        if (!anim)
        {
            return;
        }

        if (rightHand != null && rightHand.gameObject.activeInHierarchy)
        {
            anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            anim.SetIKPosition(AvatarIKGoal.RightHand, rightHand.position);
        }
        else
        {
            //anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
            //anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
        }

        if (leftHand != null && leftHand.gameObject.activeInHierarchy)
        {
            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            anim.SetIKPosition(AvatarIKGoal.LeftHand, leftHand.position);
        }
        else
        {
            //anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            //anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
        }

        if (rightElbow != null && rightElbow.gameObject.activeInHierarchy)
        {
            anim.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 1);
            anim.SetIKHintPosition(AvatarIKHint.RightElbow, rightElbow.position);
        }
        else
        {
            //anim.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 0);
        }

        if (leftElbow != null && leftElbow.gameObject.activeInHierarchy)
        {
            anim.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 1);
            anim.SetIKHintPosition(AvatarIKHint.LeftElbow, leftElbow.position);
        }
        else
        {
            //anim.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 1);
        }

        if (head != null && head.gameObject.activeInHierarchy)
        {
            anim.SetLookAtWeight(lookWeight, bodyWeight, headWeight, eyesWeight, clampWeight);
            anim.SetLookAtPosition(head.position);
        }
        else
        {
            //anim.SetLookAtWeight(0);
        }
    }

}
