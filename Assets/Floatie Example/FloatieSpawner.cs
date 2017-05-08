using UnityEngine;

public class FloatieSpawner : MonoBehaviour {

    public GameObject prefabToUseAsFloatie;
    public Transform attentionPoint;

    private int mode = 0;
    private Floatie floatie;

    void Start()
    {
        Debug.Log("Press any key to create a floatie");
    }

    // Update is called once per frame
    void Update () {
        if (Input.anyKeyDown)
        {
            switch (mode++ % 3)
            {
                case 0:
                    Debug.Log("Creating a floatie without attention point");
                    floatie = Floatie.Spawn(prefabToUseAsFloatie);
                    break;
                case 1:
                    Debug.Log("Set the cube in the scene as attention point, line points to it");
                    floatie.attentionPoint = attentionPoint;
                    break;
                case 2:
                    Debug.Log("Destroyed the floatie");
                    floatie.Destroy();
                    floatie = null;
                    break;
            }
        }
	}
}
