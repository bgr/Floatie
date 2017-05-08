using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floatie : MonoBehaviour {

    public float distanceFromHead = 0.5f;
    public bool drawLine = true;
    [Range(0, 1)] public float positionLerpFactor = 0.02f;
    [Range(0, 1)] public float rotationLerpFactor = 0.2f;

    public float pushBack;

    [Header("Optional")]
    public Transform attentionPoint;
    public Transform head;

    public static void Spawn(GameObject prefab, Transform attentionPoint, float distanceFromHead = 0.5f)
    {
        // TODO see if we need container object, to support animation
        var go = Instantiate(prefab);
        var floatie = go.AddComponent<Floatie>();
        floatie.attentionPoint = attentionPoint;
    }

	void Start () {
        if (!head) head = Camera.main.transform;
	}
	
	void Update () {
        var targetPos = head.position + head.forward * distanceFromHead;
        var targetLook = transform.position + head.forward;

        var lerpPos = Vector3.Lerp(transform.position, targetPos, positionLerpFactor);
        var lerpLook = Vector3.Lerp(transform.position + transform.forward, targetLook, rotationLerpFactor);

        // don't come too close to camera
        var proximity = Vector3.SqrMagnitude(lerpPos - head.position);
        pushBack = Mathf.Clamp01(1 - proximity / (distanceFromHead * distanceFromHead));
        var pushBackPos = lerpPos + head.forward * pushBack;

        transform.position = Vector3.Lerp(lerpPos, pushBackPos, 0.5f); // aggresively push back
        transform.LookAt(lerpLook, Vector3.Lerp(head.up, Vector3.up, 0.5f));
	}
}
