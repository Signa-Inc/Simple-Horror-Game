using DG.Tweening;
using System.Collections;
using UnityEngine;

public class Interact_Manager : MonoBehaviour
{
    [SerializeField] bool curOpenState; // указываем дл€ каждой двери отдельно

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            StartCoroutine(OnInteract());
    }

    IEnumerator OnInteract()
    {
        Vector3 currentEulerAngles = transform.eulerAngles;

        if (curOpenState == true){
            transform.DORotate(new Vector3(currentEulerAngles.x, currentEulerAngles.y - 90f, currentEulerAngles.z), 1f);
            curOpenState = false;
        }
        else if (curOpenState == false){
            transform.DORotate(new Vector3(currentEulerAngles.x, currentEulerAngles.y + 90f, currentEulerAngles.z), 1f);
            curOpenState = true;
        }

        yield return new WaitForSecondsRealtime(3f);
    }
}

//ќбъ€снить 2 сокращени€ в коде и мб ещЄ одно
