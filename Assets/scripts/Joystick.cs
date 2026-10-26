using UnityEngine;

public class Joystick : MonoBehaviour
{
    void Update()
    {
        transform.position += Vector3.forward * Time.deltaTime * 2f;
    }
}