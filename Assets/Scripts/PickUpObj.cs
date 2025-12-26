using DG.Tweening;
using System.Collections;
using UnityEngine;

public class PickUpObj : MonoBehaviour
{
    public PickUpObj_Manager.Type type;
    public Interact_Manager case_Obj;

    public IEnumerator OnPickUp(GameObject keyToInteract)
    {
        PickUpObj_Manager.instance.FPM.enabled = false;
        GetComponent<Collider>().enabled = false;

        transform.DOMove(PickUpObj_Manager.instance.point.position, 0.5f);

        yield return new WaitForSeconds(0.5f);

        transform.SetParent(PickUpObj_Manager.instance.point);
        PickUpObj_Manager.instance.FPM.enabled = true;
        gameObject.SetActive(false);
        keyToInteract.SetActive(false);
    }
}
