using UnityEngine;

public class Floatie : MonoBehaviour {

    public float distanceFromHead = 0.5f;
    public bool drawLine = false;
    [Range(0, 1)] public float positionLerpFactor = 0.02f;
    [Range(0, 1)] public float rotationLerpFactor = 0.2f;

    public float lineWidth = 0.001f;
    public Color lineColor = Color.gray;

    [Header("Optional")]
    public Transform attentionPoint;
    public Transform head;
    public Transform lineStartPoint;

    private LineRenderer line;
    private Material lineMaterial;


    public static void Spawn(GameObject prefab, Transform attentionPoint, float distanceFromHead = 0.5f)
    {
        // TODO see if we need container object, to support animation
        var go = Instantiate(prefab);
        var floatie = go.AddComponent<Floatie>();
        floatie.attentionPoint = attentionPoint;
    }

	void Start () {
        if (!head) head = Camera.main.transform;

        // try to find starting point of the line via hardcoded name, fallback to center
        if (!lineStartPoint) lineStartPoint = transform.Find("line start point");
        if (!lineStartPoint) lineStartPoint = transform;

        line = gameObject.AddComponent<LineRenderer>();
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        lineMaterial = new Material(Shader.Find("Standard"));
        line.material = lineMaterial;
	}
	
	void Update () {
        UpdateFloatie();
        UpdateLine();
	}

    void UpdateFloatie()
    {
        var targetPos = head.position + head.forward * distanceFromHead;
        var targetLook = transform.position + head.forward;

        var lerpPos = Vector3.Lerp(transform.position, targetPos, positionLerpFactor);
        var lerpLook = Vector3.Lerp(transform.position + transform.forward, targetLook, rotationLerpFactor);

        // don't come too close to camera
        var proximity = Vector3.SqrMagnitude(lerpPos - head.position);
        var pushBack = Mathf.Clamp01(1 - proximity / (distanceFromHead * distanceFromHead));
        var pushBackPos = lerpPos + head.forward * pushBack;

        transform.position = Vector3.Lerp(lerpPos, pushBackPos, 0.4f); // aggresively push back
        var up = Vector3.Lerp(head.up, Vector3.up, 0.5f); // prevents spinning when looking straight up/down
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
}
