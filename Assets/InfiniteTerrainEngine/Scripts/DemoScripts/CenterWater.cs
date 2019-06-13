using UnityEngine;

public class CenterWater : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 camPos = Camera.main.transform.position;
        gameObject.transform.position = new Vector3(camPos.x, gameObject.transform.position.y, camPos.z);
    }
}
