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

    public float weightMultiplierUp = 10;
    public float weightMultiplierDown = 3;

    public float lookWeight = 1;
    public float bodyWeight = 0.25f;
    public float headWeight = 0.9f;
    public float eyesWeight = 1;
    public float clampWeight = 1;


    public float rightHandWeight;
    public float leftHandWeight;
    public float rightElbowWeight;
    public float leftElbowWeight;


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
            rightHandWeight += Time.deltaTime * weightMultiplierUp;
            anim.SetIKPosition(AvatarIKGoal.RightHand, rightHand.position);
        }
        else
        {
            rightHandWeight -= Time.deltaTime * weightMultiplierDown;
        }

        if (leftHand != null && leftHand.gameObject.activeInHierarchy)
        {
            leftHandWeight += Time.deltaTime * weightMultiplierUp;
            anim.SetIKPosition(AvatarIKGoal.LeftHand, leftHand.position);
        }
        else
        {
            leftHandWeight -= Time.deltaTime * weightMultiplierDown;
        }

        if (rightElbow != null && rightElbow.gameObject.activeInHierarchy)
        {
            rightElbowWeight += Time.deltaTime * weightMultiplierUp;
            anim.SetIKHintPosition(AvatarIKHint.RightElbow, rightElbow.position);
        }
        else
        {
            rightElbowWeight -= Time.deltaTime * weightMultiplierDown;
        }

        if (leftElbow != null && leftElbow.gameObject.activeInHierarchy)
        {
            leftElbowWeight += Time.deltaTime * weightMultiplierUp;
            anim.SetIKHintPosition(AvatarIKHint.LeftElbow, leftElbow.position);
        }
        else
        {
            leftElbowWeight -= Time.deltaTime * weightMultiplierDown;
        }

        if (head != null && head.gameObject.activeInHierarchy)
        {
            anim.SetBoneLocalRotation(HumanBodyBones.Head, head.rotation);

            anim.SetLookAtWeight(lookWeight, bodyWeight, headWeight, eyesWeight, clampWeight);
            anim.SetLookAtPosition(head.position);
        }
        else
        {
            anim.SetLookAtWeight(0);
        }

        rightHandWeight = Mathf.Clamp01(rightHandWeight);
        leftHandWeight = Mathf.Clamp01(leftHandWeight);
        rightElbowWeight = Mathf.Clamp01(rightElbowWeight);
        leftElbowWeight = Mathf.Clamp01(leftElbowWeight);

        anim.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandWeight);
        anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandWeight);
        anim.SetIKHintPositionWeight(AvatarIKHint.RightElbow, rightElbowWeight);
        anim.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, leftElbowWeight);

    }
}
