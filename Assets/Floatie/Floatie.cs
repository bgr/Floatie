using UnityEngine;
using UnityEngine.Events;

public class Floatie : MonoBehaviour {

    public float distanceFromHead = 0.6f;
    public bool drawLine = false;

    [Tooltip("Maps the angle between camera forward direction and direction to current floatie position (x-axis [0, 180]) to lerp factor of floatie centering movement (y-axis [0, 1])")]
    public AnimationCurve angleToPositionLerp;

    [Tooltip("Maps the angle between camera forward direction and direction to current floatie position (x-axis [0, 180]) to lerp factor of floatie re-aligning (x & y) rotation (y-axis [0, 1])")]
    public AnimationCurve angleToRotationLerp;

    [Tooltip("Roll (z-rotation) lerp")]
    [Range(0, 1)] public float rollLerp = 0.2f;

    [Tooltip("Target percentage to reorient the floatie towards world up direction (as opposed to camera's up direction)")]
    [Range(0, 1)] public float worldUpRotationTargetPercentage = 0.2f;

    [Tooltip("Amount to move the floatie towards the attention point in order to draw attention and cause the user to rotate the head towards it")]
    [Range(0, 1)] public float offsetFactor = 0.25f;

    public bool spawnInFrontOfCam = true;

    [Tooltip("Destroy floatie after camera had been approximately looking at attention point for this amount of time")]
    public float dismissTime = 1.2f;

    [Tooltip("Conic angle to consider that camera is looking at the attention point")]
    public float dismissAngle = 20f;

    public float lineWidth = 0.001f;
    public Color lineColor = Color.gray;

    [Tooltip("Time to wait before calling Destroy on floatie game object")]
    public float waitBeforeDestroy = 1f;

    [Header("Optional")]

    public Transform attentionPoint;
    public Transform head;
    public Transform lineStartPoint;

    [Tooltip("Fired when floatie has been destroyed as a result of looking at the attention point")]
    public UnityEvent Dismissed = new UnityEvent();

    protected LineRenderer line;
    protected Material lineMaterial;
    protected float countdown;
    protected bool destroyingInProgress = false;


    public static Floatie Spawn(GameObject prefab, Transform attentionPoint = null, float distanceFromHead = 0.5f, bool spawnInFrontOfCam = true)
    {
        var go = Instantiate(prefab);

        var floatie = go.GetComponent<Floatie>();
        if (!floatie) go.AddComponent<Floatie>();
        if (floatie.attentionPoint == null) floatie.attentionPoint = attentionPoint;
        floatie.spawnInFrontOfCam = spawnInFrontOfCam;
        return floatie;
    }

    public void Destroy()
    {
        if (destroyingInProgress) return;
        destroyingInProgress = true;

        OnAboutToBeDestroyed();
        Destroy(gameObject, waitBeforeDestroy);
    }

    public virtual void OnAboutToBeDestroyed()
    {
        // override this method with custom logic e.g. if you need to play a destroying
        // animation (used in combination with waitBeforeDestroy to allow animation to finish)
    }

    void Start ()
    {
        if (!head) head = Camera.main.transform;

        // try to find starting point of the line via hardcoded name, fallback to center
        if (!lineStartPoint) lineStartPoint = transform.Find("line start point");
        if (!lineStartPoint) lineStartPoint = transform;

        line = gameObject.AddComponent<LineRenderer>();
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        lineMaterial = new Material(Shader.Find("Standard"));
        line.material = lineMaterial;

        if (spawnInFrontOfCam)
        {
            transform.position = head.position + head.forward * distanceFromHead;
            transform.LookAt(transform.position + head.forward, Vector3.up);
        }

        countdown = dismissTime;
    }

    void Update ()
    {
        UpdateFloatie();
        UpdateLine();
        if (attentionPoint) UpdateDismiss();
    }

    void UpdateFloatie()
    {
        var straightAheadDirectionMult = head.forward * distanceFromHead;
        var targetPos = head.position + straightAheadDirectionMult;
        var targetLook = transform.position + head.forward;

        // position lerp factor depends on the angle between a point straight ahead and current floatie position
        var headToFloatieRotation = Quaternion.FromToRotation(head.forward, transform.position - head.position);
        var angleDiff = Mathf.Clamp(Quaternion.Angle(Quaternion.identity, headToFloatieRotation), 0, 180);
        var positionLerpFactor = angleToPositionLerp.Evaluate(angleDiff);
        var rotationLerpFactor = angleToRotationLerp.Evaluate(angleDiff);

        // slightly offset in the direction of attention point
        var attentionDirection = attentionPoint ? attentionPoint.position - head.position : straightAheadDirectionMult;
        var rotToPointOnSphere = Quaternion.FromToRotation(head.forward, attentionDirection);
        var lerpRot = Quaternion.Lerp(Quaternion.identity, rotToPointOnSphere, offsetFactor);
        targetPos = lerpRot * (head.forward) + head.position;

        var lerpPos = Vector3.Lerp(transform.position, targetPos, positionLerpFactor);
        var lerpLook = Vector3.Lerp(transform.position + transform.forward, targetLook, rotationLerpFactor);

        // don't come too close to camera
        var proximity = Vector3.SqrMagnitude(lerpPos - head.position);
        var pushBack = Mathf.Clamp01(1 - proximity / (distanceFromHead * distanceFromHead));
        var pushBackPos = lerpPos + head.forward * pushBack;

        transform.position = Vector3.Lerp(lerpPos, pushBackPos, 0.4f); // aggresively push back
        var targetUp = Vector3.Lerp(head.up, Vector3.up, worldUpRotationTargetPercentage); // also prevents flipping when looking straight up/down
        var up = Vector3.Lerp(transform.up, targetUp, rollLerp);
        transform.LookAt(lerpLook, up);
    }

    void UpdateLine()
    {
        if (!drawLine || !lineStartPoint || !attentionPoint)
        {
            line.enabled = false;
            return;
        }
        line.enabled = true;

        lineMaterial.color = lineColor;
        lineMaterial.SetColor("_EmissionColor", lineColor);
#if UNITY_5_5_OR_NEWER
        line.startColor = lineColor;
        line.endColor = lineColor;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
#else
        line.SetColors(lineColor, lineColor);
        line.SetWidth(lineWidth, lineWidth);
#endif

        line.SetPosition(0, lineStartPoint.position);
        line.SetPosition(1, attentionPoint.position);
    }

    void UpdateDismiss()
    {
        if (destroyingInProgress) return;

        var headToFloatieRotation = Quaternion.FromToRotation(head.forward, attentionPoint.position - transform.position);
        var angleDiff = Mathf.Clamp(Quaternion.Angle(Quaternion.identity, headToFloatieRotation), 0, 180);

        if (angleDiff <= dismissAngle)
        {
            countdown -= Time.deltaTime;
        }

        if (countdown <= 0)
        {
            Dismissed.Invoke();
            Destroy();
        }
    }
}
