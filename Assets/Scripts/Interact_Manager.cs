using DG.Tweening;
using System.Collections;
using UnityEngine;

public class Interact_Manager : MonoBehaviour
{
    public bool isOpen;

    // Добавь эти поля
    [SerializeField] float openAngle = 90f; // Угол открытия двери

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
            StartCoroutine(OnInteract(other.transform));
    }

    public IEnumerator OnInteract(Transform activator)
    {
        yield return new WaitForSeconds(0.3f);

        // Получаем направление от двери к активатору
        Vector3 directionToActivator = (activator.position - transform.position).normalized;

        // Определяем, в какую сторону должна открываться дверь
        // Предполагаем, что forward двери - это её нормальное направление
        Vector3 doorForward = transform.forward; // или transform.right, смотря как ориентирована дверь

        // Вычисляем угол поворота в зависимости от положения активатора
        float dotProduct = Vector3.Dot(doorForward, directionToActivator);

        Vector3 targetRotation;
        if (dotProduct > 0) // Активатор спереди двери
        {
            // Открываем в противоположную сторону (назад)
            targetRotation = new Vector3(0f, isOpen ? 0f : -openAngle, 0f);
        }
        else // Активатор сзади двери
        {
            // Открываем в противоположную сторону (вперёд)
            targetRotation = new Vector3(0f, isOpen ? 0f : openAngle, 0f);
        }

        transform.DOLocalRotate(targetRotation, 1f);

        isOpen = !isOpen;

        yield return new WaitForSeconds(3f);
    }
}