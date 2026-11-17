using System.Globalization;
using UnityEngine;
using TMPro;

public class MileConverter : MonoBehaviour
{
    public TMP_InputField inputField;
    public TextMeshProUGUI outputText;

    // Метод викликається при натисканні на кнопку
    public void ConvertMiles()
    {
        if (inputField == null || outputText == null)
        {
            return;
        }

        // Якщо поле порожнє або містить лише пробіли
        if (string.IsNullOrWhiteSpace(inputField.text))
        {
            outputText.text = "Помилка: введіть число!";
            return;
        }

        // Уніфікуємо десятковий роздільник (кома або крапка) незалежно від локалі ОС
        string normalized = inputField.text.Trim().Replace(',', '.');

        // Намагаємося перетворити текст у дійсне число (float) в InvariantCulture
        if (!float.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out float miles)
            || float.IsNaN(miles)
            || float.IsInfinity(miles))
        {
            // Спрацює, якщо ввели букви, NaN, Infinity або некоректні символи
            outputText.text = "Помилка: введено некоректні дані!";
            return;
        }

        // Контроль 3: перевірка на від'ємне число
        if (miles < 0)
        {
            outputText.text = "Помилка: число має бути ≥ 0!";
            return;
        }

        // 1 сухопутна миля = 160934.4 см
        float cm = miles * 160934.4f;
        if (float.IsInfinity(cm))
        {
            outputText.text = "Помилка: число занадто велике!";
            return;
        }

        // Виводимо порахований результат
        outputText.text = "Результат: " + cm.ToString(CultureInfo.InvariantCulture) + " см";
    }
}
