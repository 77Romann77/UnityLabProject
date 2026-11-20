using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigation : MonoBehaviour
{
    [Header("Звук натискання кнопки")]
    public AudioSource uiAudioSource;
    public AudioClip buttonClickClip;

    // Завантаження сцени за її назвою з невеликою затримкою, щоб встиг прозвучати клік
    public void LoadSceneByName(string sceneName)
    {
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    // Відтворення лише звуку кнопки (можна прив'язати до будь-якої кнопки в UI)
    public void PlayClickSound()
    {
        if (uiAudioSource != null && buttonClickClip != null)
        {
            uiAudioSource.PlayOneShot(buttonClickClip);
        }
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        PlayClickSound();
        yield return new WaitForSeconds(0.15f);
        SceneManager.LoadScene(sceneName);
    }
}