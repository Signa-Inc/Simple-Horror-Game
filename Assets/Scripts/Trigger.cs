using UnityEngine;

public class Trigger : MonoBehaviour
{
    [SerializeField] GameObject keyToInteract;
    bool isCanPressKey_OTI;
    bool isCanPressKey_PUG;
    GameObject curObj;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obj To Interact"))
        {
            curObj = other.gameObject;
            isCanPressKey_OTI = true;
            keyToInteract.SetActive(true);
        }
        else if (other.CompareTag("Pick Up Obj"))
        {
            if (other.GetComponent<PickUpObj>().case_Obj.isOpen || other.GetComponent<PickUpObj>().case_Obj == null)
            {
                curObj = other.gameObject;
                isCanPressKey_PUG = true;
                keyToInteract.SetActive(true);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Obj To Interact"))
        {
            curObj = null;
            isCanPressKey_OTI = false;
            keyToInteract.SetActive(false);
        }
        else if (other.CompareTag("Pick Up Obj"))
        {
            curObj = null;
            isCanPressKey_PUG = false;
            keyToInteract.SetActive(false);
        }
    }

    void Update()
    {
        if (isCanPressKey_OTI && Input.GetKeyUp(KeyCode.E))
        {
            Interact_Manager interManager = curObj.GetComponent<Interact_Manager>();
            interManager.StartCoroutine(interManager.OnInteract(curObj.transform));
            curObj = null;
        }

        else if (isCanPressKey_PUG && Input.GetKeyUp(KeyCode.E))
        {
            PickUpObj pickUpObj = curObj.GetComponent<PickUpObj>();
            pickUpObj.StartCoroutine(pickUpObj.OnPickUp(keyToInteract));

            if (pickUpObj.type == PickUpObj_Manager.Type.Code)
                Game_Manager.instance.curCodes++;
            else if (pickUpObj.type == PickUpObj_Manager.Type.Book)
                Game_Manager.instance.curBooks++;
            else if (pickUpObj.type == PickUpObj_Manager.Type.Transmitter)
                Game_Manager.instance.isFindedTransmitter = true;

            Game_Manager.instance.UpdateTextToMissions();
            curObj = null;
        }
    }
}
