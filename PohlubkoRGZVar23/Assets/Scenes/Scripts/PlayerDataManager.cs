using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

// Клас для зберігання даних одного гравця
[Serializable]
public class PlayerProfile
{
    public string playerName;
    public int balance;

    public PlayerProfile(string name, int initialBalance)
    {
        playerName = name;
        balance = initialBalance;
    }
}

// Обгортка для серіалізації списку гравців у JSON
[Serializable]
public class PlayerDatabase
{
    public List<PlayerProfile> players = new List<PlayerProfile>();
}

public class PlayerDataManager : MonoBehaviour
{
    [Header("Елементи UI для профілів")]
    public TMP_Dropdown playerDropdown;
    public TMP_InputField newPlayerInput;
    public TMP_Text leaderboardText;

    [Header("Налаштування за замовчуванням")]
    public int defaultStartingBalance = 100;

    private const string SAVE_KEY = "SlotMachine_PlayersDB";
    private const string LAST_PLAYER_KEY = "SlotMachine_LastPlayerIndex";

    private PlayerDatabase database = new PlayerDatabase();
    public PlayerProfile CurrentPlayer { get; private set; }

    // Подія, яка сповіщає контролер гри про зміну гравця
    public event Action OnPlayerChanged;

    private void Awake()
    {
        LoadDatabase();
    }

    private void Start()
    {
        RefreshDropdown();
        
        // Відновлюємо останнього обраного гравця
        int savedIndex = PlayerPrefs.GetInt(LAST_PLAYER_KEY, 0);
        if (savedIndex >= 0 && savedIndex < database.players.Count)
        {
            playerDropdown.value = savedIndex;
        }
        
        SelectPlayer(playerDropdown.value);
        UpdateLeaderboardUI();
    }

    // Завантаження бази гравців із PlayerPrefs
    private void LoadDatabase()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            string json = PlayerPrefs.GetString(SAVE_KEY);
            database = JsonUtility.FromJson<PlayerDatabase>(json);
        }

        // Якщо база порожня (перший запуск), створюємо стандартного гравця
        if (database == null || database.players == null || database.players.Count == 0)
        {
            database = new PlayerDatabase();
            database.players.Add(new PlayerProfile("Гравець 1", defaultStartingBalance));
            SaveDatabase();
        }
    }

    // Збереження бази гравців у PlayerPrefs у форматі JSON
    public void SaveDatabase()
    {
        string json = JsonUtility.ToJson(database);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
        UpdateLeaderboardUI();
    }

    // Оновлення випадаючого списку Dropdown
    private void RefreshDropdown()
    {
        playerDropdown.ClearOptions();
        List<string> options = new List<string>();
        
        foreach (var p in database.players)
        {
            options.Add($"{p.playerName} (${p.balance})");
        }
        
        playerDropdown.AddOptions(options);
    }

    // Метод викликається при виборі гравця у Dropdown
    public void SelectPlayer(int index)
    {
        if (index < 0 || index >= database.players.Count) return;

        CurrentPlayer = database.players[index];
        PlayerPrefs.SetInt(LAST_PLAYER_KEY, index);
        PlayerPrefs.Save();

        OnPlayerChanged?.Invoke();
    }

    // Метод для кнопки створення нового гравця
    public void CreateNewPlayer()
    {
        string enteredName = newPlayerInput.text.Trim();
        if (string.IsNullOrEmpty(enteredName)) return;

        // Перевірка, чи не існує вже гравець із таким ім'ям
        if (database.players.Any(p => p.playerName.Equals(enteredName, StringComparison.OrdinalIgnoreCase)))
        {
            newPlayerInput.text = "";
            return;
        }

        PlayerProfile newProfile = new PlayerProfile(enteredName, defaultStartingBalance);
        database.players.Add(newProfile);
        SaveDatabase();

        newPlayerInput.text = "";
        RefreshDropdown();

        // Одразу перемикаємося на новоствореного гравця
        int newIndex = database.players.Count - 1;
        playerDropdown.value = newIndex;
        SelectPlayer(newIndex);
    }

    // Оновлення балансу поточного гравця та синхронізація списку
    public void UpdateCurrentPlayerBalance(int newBalance)
    {
        if (CurrentPlayer == null) return;

        CurrentPlayer.balance = Mathf.Max(0, newBalance);
        SaveDatabase();

        // Оновлюємо текст поточного пункту у Dropdown без скидання вибору
        int currentIndex = playerDropdown.value;
        if (currentIndex >= 0 && currentIndex < playerDropdown.options.Count)
        {
            playerDropdown.options[currentIndex].text = $"{CurrentPlayer.playerName} (${CurrentPlayer.balance})";
            playerDropdown.RefreshShownValue();
        }
    }

    // Формування міні-таблиці рекордів (Топ-3 гравці)
    private void UpdateLeaderboardUI()
    {
        if (leaderboardText == null) return;

        var topPlayers = database.players
            .OrderByDescending(p => p.balance)
            .Take(3)
            .ToList();

        string board = "<b>Топ гравців:</b>\n";
        for (int i = 0; i < topPlayers.Count; i++)
        {
            board += $"{i + 1}. {topPlayers[i].playerName}: ${topPlayers[i].balance}\n";
        }
        leaderboardText.text = board;
    }
    
    // Метод для кнопки видалення поточного гравця
    public void DeleteCurrentPlayer()
    {
        if (CurrentPlayer == null || database.players.Count == 0) return;

        int indexToRemove = playerDropdown.value;
        database.players.RemoveAt(indexToRemove);

        // Якщо видалили останнього гравця, створюємо базового, щоб гра не залишилася порожньою
        if (database.players.Count == 0)
        {
            database.players.Add(new PlayerProfile("Гравець 1", defaultStartingBalance));
        }

        SaveDatabase();
        RefreshDropdown();

        // Обираємо попередній доступний профіль
        int nextIndex = Mathf.Clamp(indexToRemove - 1, 0, database.players.Count - 1);
        playerDropdown.value = nextIndex;
        SelectPlayer(nextIndex);
    }
}