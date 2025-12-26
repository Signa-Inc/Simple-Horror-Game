using UnityEngine;

public class PickUpObj_Manager : MonoBehaviour
{
    public static PickUpObj_Manager instance;
    public Transform point;
    public FirstPersonMovement FPM;

    void Awake() => instance = this;

    public enum Type
    {
        Transmitter,
        Book,
        Code
    }
}
