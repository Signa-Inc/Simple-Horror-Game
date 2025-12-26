using TMPro;
using UnityEngine;

public class Escape_Door : MonoBehaviour
{
    [SerializeField] GameObject winPanel;
    [SerializeField] TMP_InputField codeInputField;
    [SerializeField] GameObject codeObj;

    string correctCode = "1983";

    void OnTriggerEnter(Collider other)
    {
        codeInputField.onValueChanged.AddListener(CheckCode); // Либо можем кинуть на кнопку, чтобы было более красивее

        FirstPersonLook.instance.SetCursor(true);

        codeObj.SetActive(true);
    }
    void OnTriggerExit(Collider other)
    {
        FirstPersonLook.instance.SetCursor(false);

        codeObj.SetActive(false);
    }

    public void CheckCode(string text)
    {
        if (text == correctCode)
            winPanel.SetActive(true);
    }
}
