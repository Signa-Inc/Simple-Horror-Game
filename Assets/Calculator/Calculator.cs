using UnityEngine;
using UnityEngine.UI;

public class Calculator : MonoBehaviour
{
    [SerializeField] float firstNum = 1f;
    [SerializeField] float secondNum = 1f;
    [SerializeField] string curOperate = "+";

    [SerializeField] Text output;
    [SerializeField] InputField input_0;
    [SerializeField] InputField input_1;
    [SerializeField] InputField input_action;

    void Start()
    {
        input_0.onValueChanged.AddListener(Input_0);
        input_1.onValueChanged.AddListener(Input_1);
        input_action.onValueChanged.AddListener(Input_Action);
    }

    void Input_0(string input)
    {
        firstNum = int.Parse(input);
        Calculate();
    }

    void Input_1(string input)
    {
        secondNum = int.Parse(input);
        Calculate();
    }

    void Input_Action(string input)
    {
        curOperate = input;
        Calculate();
    }

    void Calculate()
    {
        if (curOperate == "+")
            output.text = $"{firstNum} {curOperate} {secondNum} = {firstNum + secondNum}";
        else if (curOperate == "-")
            output.text = $"{firstNum} {curOperate} {secondNum} = {firstNum - secondNum}";
        else if (curOperate == "*")
            output.text = $"{firstNum} {curOperate} {secondNum} = {firstNum * secondNum}";
        else if (curOperate == "/")
            output.text = $"{firstNum} {curOperate} {secondNum} = {firstNum / secondNum}";
    }
}

public class Operations
{
    public string plus = "+";
    public string minus = "-";
    public string multiple = "*";
    public string divide = "/";
}