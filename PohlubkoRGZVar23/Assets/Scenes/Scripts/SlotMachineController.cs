using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotMachineController : MonoBehaviour
{
    [Header("Посилання на менеджер профілів")]
    public PlayerDataManager playerManager;

    [Header("Віконця барабанів (3 елементи TextMeshPro)")]
    public TMP_Text[] slotTexts;

    [Header("Елементи інтерфейсу гри")]
    public TMP_Text balanceText;
    public TMP_InputField betInput;
    public TMP_Text statusText;
    public Button startButton;
    public Button maxBetButton;

    [Header("Екран Game Over")]
    public GameObject gameOverPanel;

    [Header("Звуковий супровід")]
    public AudioSource audioSource;
    public AudioClip spinTickClip;
    public AudioClip reelStopClip;
    public AudioClip winClip;
    public AudioClip jackpotClip;
    public AudioClip loseClip;

    [Header("Налаштування анімації прокручування")]
    public float spinStepDelay = 0.06f; // Швидкість зміни цифр
    public int baseSpinTicks = 12;      // Кількість перемикань для першого барабана

    private bool isSpinning = false;
    private const int MIN_BET = 1;

    private void OnEnable()
    {
        if (playerManager != null)
            playerManager.OnPlayerChanged += SyncUIWithPlayer;
    }

    private void OnDisable()
    {
        if (playerManager != null)
            playerManager.OnPlayerChanged -= SyncUIWithPlayer;
    }

    private void Start()
    {
        if (string.IsNullOrEmpty(betInput.text))
            betInput.text = "10";

        SyncUIWithPlayer();
    }

    // Оновлення інтерфейсу при зміні гравця або балансу
    public void SyncUIWithPlayer()
    {
        if (playerManager.CurrentPlayer == null) return;

        int balance = playerManager.CurrentPlayer.balance;
        balanceText.text = $"Ваші гроші: $ {balance}";

        // Перевірка умови програшу (баланс менший за мінімальну ставку)
        if (balance < MIN_BET)
        {
            gameOverPanel.SetActive(true);
            startButton.interactable = false;
        }
        else
        {
            gameOverPanel.SetActive(false);
            startButton.interactable = !isSpinning;
            statusText.text = "Встановіть ставку та натисніть Start!";
        }
    }

    // Кнопка швидкої максимальної ставки (Max Bet)
    public void SetMaxBet()
    {
        if (isSpinning || playerManager.CurrentPlayer == null) return;
        betInput.text = playerManager.CurrentPlayer.balance.ToString();
    }

    // Обробник натискання кнопки "Start"
    public void OnStartSpinClicked()
    {
        if (isSpinning || playerManager.CurrentPlayer == null) return;

        // Зчитування та валідація ставки
        if (!int.TryParse(betInput.text, out int bet) || bet < MIN_BET)
        {
            statusText.text = $"Мінімальна ставка — ${MIN_BET}!";
            return;
        }

        int currentBalance = playerManager.CurrentPlayer.balance;
        if (bet > currentBalance)
        {
            statusText.text = "Недостатньо коштів для такої ставки!";
            return;
        }

        StartCoroutine(SpinReelsCoroutine(bet));
    }

    // Корутина почергового обертання та зупинки барабанів
    private IEnumerator SpinReelsCoroutine(int bet)
    {
        isSpinning = true;
        startButton.interactable = false;
        maxBetButton.interactable = false;
        betInput.interactable = false;
        playerManager.playerDropdown.interactable = false;

        statusText.text = "Барабани обертаються...";

        // Генеруємо фінальні значення від 1 до 7 включно (верхня межа в Random.Range для int ексклюзивна, тому 8)
        int[] finalResults = new int[3];
        for (int i = 0; i < 3; i++)
        {
            finalResults[i] = Random.Range(1, 8);
        }

        // Почергова зупинка кожного з 3 віконець
        for (int reelIndex = 0; reelIndex < 3; reelIndex++)
        {
            int ticks = baseSpinTicks + (reelIndex * 8); // Кожен наступний барабан крутиться довше
            for (int t = 0; t < ticks; t++)
            {
                // Відображаємо випадкові проміжні цифри на ще активних барабанах
                for (int activeReel = reelIndex; activeReel < 3; activeReel++)
                {
                    slotTexts[activeReel].text = Random.Range(1, 8).ToString();
                }

                PlaySound(spinTickClip);
                yield return new WaitForSeconds(spinStepDelay);
            }

            // Фіксуємо фінальне число для поточного барабана
            slotTexts[reelIndex].text = finalResults[reelIndex].ToString();
            PlaySound(reelStopClip);
        }

        // Підрахунок результату після зупинки всіх трьох віконець
        EvaluateSpinResult(finalResults, bet);

        isSpinning = false;
        maxBetButton.interactable = true;
        betInput.interactable = true;
        playerManager.playerDropdown.interactable = true;

        // Перевірка, чи залишилися кошти у гравця
        int updatedBalance = playerManager.CurrentPlayer.balance;
        if (updatedBalance < MIN_BET)
        {
            statusText.text = "Гра закінчена! У вас не залишилося грошей.";
            gameOverPanel.SetActive(true);
            startButton.interactable = false;
        }
        else
        {
            startButton.interactable = true;
        }
    }

    // Розрахунок виграшу за комбінаціями
    private void EvaluateSpinResult(int[] r, int bet)
    {
        int balance = playerManager.CurrentPlayer.balance;

        // 1. Перевірка на Джекпот (7-7-7)
        if (r[0] == 7 && r[1] == 7 && r[2] == 7)
        {
            int winAmount = bet * 10;
            balance += winAmount;
            statusText.text = $"ДЖЕКПОТ 7-7-7! Ви виграли ${winAmount}!";
            PlaySound(jackpotClip != null ? jackpotClip : winClip);
        }
        // 2. Перевірка на 3 однакові цифри
        else if (r[0] == r[1] && r[1] == r[2])
        {
            int winAmount = bet * 5;
            balance += winAmount;
            statusText.text = $"Три однакові ({r[0]}-{r[1]}-{r[2]})! Ви виграли ${winAmount}!";
            PlaySound(winClip);
        }
        // 3. Перевірка на 2 однакові цифри
        else if (r[0] == r[1] || r[1] == r[2] || r[0] == r[2])
        {
            int winAmount = bet * 2;
            balance += winAmount;
            statusText.text = $"Два збіги! Ви виграли ${winAmount}!";
            PlaySound(winClip);
        }
        // 4. Програш — знімаємо ставку
        else
        {
            balance -= bet;
            statusText.text = $"Не пощастило. Знято ставку ${bet}.";
            PlaySound(loseClip);
        }

        // Зберігаємо новий баланс гравця та оновлюємо екран
        playerManager.UpdateCurrentPlayerBalance(balance);
        balanceText.text = $"Ваші гроші: $ {playerManager.CurrentPlayer.balance}";
    }

    // Рестарт гри при Game Over (нараховуємо стартовий капітал поточному гравцю)
    public void RestartGameForCurrentPlayer()
    {
        if (playerManager.CurrentPlayer == null) return;

        playerManager.UpdateCurrentPlayerBalance(playerManager.defaultStartingBalance);
        betInput.text = "10";
        SyncUIWithPlayer();
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}