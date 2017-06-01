using UnityEngine;

public class FloatieSpawner : MonoBehaviour {

    public GameObject prefabToUseAsFloatie;
    public Transform attentionPoint;

    private int mode = -1;
    private int prevMode = -1;
    private Floatie floatie;

    void Start()
    {
        Debug.LogWarning("Press any key to create a floatie");
    }

    // Update is called once per frame
    void Update () {
        if (Input.anyKeyDown)
        {
            mode++;
        }

        if (mode == prevMode) return;

        switch (mode % 3)
        {
            case 0:
                Debug.Log("(1/3)\nCreating a floatie without attention point.");
                floatie = Floatie.Spawn(prefabToUseAsFloatie);
                break;
            case 1:
                Debug.Log("(2/3)\nCube was set as the attention point, line points to it. Look at the cube or press any key to dismiss the floatie.");
                floatie.attentionPoint = attentionPoint.transform;
                floatie.Dismissed.AddListener(OnDismiss);
                break;
            case 2:
                Debug.Log("(3/3)\nDestroyed the floatie.");
                floatie.Destroy();
                floatie = null;
                break;
        }

        prevMode = mode;
    }

    void OnDismiss()
    {
        Debug.Log("Destroying the floatie by looking at the attention point.");
        mode = 2;
    }
}
