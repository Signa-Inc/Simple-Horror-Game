using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game_Manager : MonoBehaviour
{
    public static Game_Manager instance;
    public int curCodes;
    public int curBooks;
    public bool isFindedTransmitter;
    [SerializeField] TextMeshProUGUI missions;
    [SerializeField] BoxCollider escapeDoor;
    [SerializeField] TextMeshProUGUI codeText;

    void Awake() => instance = this;

    public void UpdateTextToMissions()
    {
        string radioTask;
        string codeTask;
        string bookTask;

        if (isFindedTransmitter)
            radioTask = "<s>Найти рацию</s>";
        else
            radioTask = "Найти рацию";

        if (curCodes >= 4)
            codeTask = $"<s>Найти куски кода ({curCodes}/4)</s>";
        else
            codeTask = $"Найти куски кода ({curCodes}/4)";

        if (curBooks >= 3)
            bookTask = $"<s>Найти книги ({curBooks}/3)</s>";
        else
            bookTask = $"Найти книги ({curBooks}/3)";

        //string radioTask = isFindedTransmitter ? "<s>Найти рацию</s>" : "Найти рацию";
        //string codeTask = (curCodes >= 4) ? "<s>Найти куски кода ({curCodes}/4)</s>" : $"Найти куски кода ({curCodes}/4)";
        //string bookTask = (curBooks >= 3) ? "<s>Найти книги ({curBooks}/3)</s>" : $"Найти книги ({curBooks}/3)";

        missions.text = $"Задания:\r\n{radioTask}\r\n{codeTask}\r\n{bookTask}";

        UpdateCodeText();

        if (isFindedTransmitter && curCodes >= 4 && curBooks >= 3) //Мы выполнили все миссии
        {
            missions.text = "Задания:\r\nИдите\r\nк главному\r\nвыходу";
            escapeDoor.enabled = true;
        }
    }

    public void UpdateCodeText()
    {
        if(curCodes == 0)
            codeText.text = $"Код: ????";
        else if (curCodes == 1)
            codeText.text = "Код: ??8?";
        else if (curCodes == 2)
            codeText.text = "Код: 1?8?";
        else if (curCodes == 3)
            codeText.text = "Код: 1?83";
        else if (curCodes == 4)
            codeText.text = "Код: 1983";
    }

    public void RestartScene() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
}
