using UnityEngine;
using TMPro;

public class MileConverter : MonoBehaviour
{
    public TMP_InputField inputField;
    public TextMeshProUGUI outputText;

    // Метод викликається при натисканні на кнопку
    public void ConvertMiles()
    {
        //якщо поле взагалі порожнє
        if (inputField.text == "")
        {
            outputText.text = "Помилка: введіть число!";
            return;
        }

        //спроба перетворити текст у число
        try
        {
            // Зчитуємо текст і переводимо його в дійсне число (float)
            float miles = float.Parse(inputField.text);

            // Контроль 3: перевірка на від'ємне число
            if (miles < 0)
            {
                outputText.text = "Помилка: число має бути ≥ 0!";
            }
            else
            {
                // 1 сухопутна миля = 160934.4 см
                float cm = miles * 160934.4f;

                // Виводимо порахований результат
                outputText.text = "Результат: " + cm + " см";
            }
        }
        catch
        {
            // Спрацює, якщо ввели букви або некоректні символи
            outputText.text = "Помилка: введено некоректні дані!";
        }
    }
}