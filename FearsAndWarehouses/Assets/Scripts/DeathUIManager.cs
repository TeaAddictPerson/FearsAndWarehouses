using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DeathUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image deathScreenImage;
    [SerializeField] private Button respawnButton;
    [SerializeField] private TextMeshProUGUI deathText;
    [SerializeField] private RespawnSpawner playerSpawner;

    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float elementDelay = 0.3f;

    private GameObject player;
    private FirstPersonController playerController;

    private PhantomGhost phantomGhost;
    private GameObject phantomGhostObject;

    private void Start()
    {
        if (deathScreenImage == null || respawnButton == null || deathText == null)
        {
            Debug.LogError("Не назначены UI компоненты!");
            return;
        }

        deathScreenImage.gameObject.SetActive(false);
        respawnButton.gameObject.SetActive(false);
        deathText.gameObject.SetActive(false);

        respawnButton.onClick.AddListener(RespawnPlayer);

        if (playerSpawner == null)
            playerSpawner = FindObjectOfType<RespawnSpawner>();

        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerController = player.GetComponent<FirstPersonController>();

        phantomGhost = FindObjectOfType<PhantomGhost>();
        if (phantomGhost != null)
            phantomGhostObject = phantomGhost.gameObject;
    }

    public void ShowDeathScreen()
    {
        Debug.Log("Показываем экран смерти");
        StartCoroutine(ShowDeathScreenElements());

        // Показываем курсор и разблокируем его, чтобы UI кнопка реагировала на клики
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (phantomGhostObject != null)
            StartCoroutine(DisablePhantomWithDelay(10f));
    }

    private IEnumerator ShowDeathScreenElements()
    {
        deathScreenImage.gameObject.SetActive(true);
        yield return StartCoroutine(FadeImage(deathScreenImage, 0, 1));

        yield return new WaitForSeconds(elementDelay);
        deathText.gameObject.SetActive(true);
        deathText.text = "Вы погибли!";
        yield return StartCoroutine(FadeText(deathText, 0, 1));

        yield return new WaitForSeconds(elementDelay);
        respawnButton.gameObject.SetActive(true);

        Image buttonImage = respawnButton.GetComponent<Image>();
        TextMeshProUGUI buttonText = respawnButton.GetComponentInChildren<TextMeshProUGUI>();

        if (buttonImage != null)
            yield return StartCoroutine(FadeImage(buttonImage, 0, 1));
        if (buttonText != null)
            yield return StartCoroutine(FadeText(buttonText, 0, 1));
    }

    private IEnumerator FadeImage(Image image, float startAlpha, float targetAlpha)
    {
        Color color = image.color;
        float elapsedTime = 0f;

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeInDuration);
            image.color = color;
            yield return null;
        }

        color.a = targetAlpha;
        image.color = color;
    }

    private IEnumerator FadeText(TextMeshProUGUI text, float startAlpha, float targetAlpha)
    {
        Color color = text.color;
        float elapsedTime = 0f;

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeInDuration);
            text.color = color;
            yield return null;
        }

        color.a = targetAlpha;
        text.color = color;
    }

    private IEnumerator DisablePhantomWithDelay(float delay)
    {
        Debug.Log($"Призрак отключится через {delay} секунд");
        yield return new WaitForSeconds(delay);
        if (phantomGhostObject != null)
        {
            phantomGhostObject.SetActive(false);
            Debug.Log("Призрак отключен после задержки");
        }
    }

    private void RespawnPlayer()
    {
        Debug.Log("Нажата кнопка респавна!");

        if (player == null)
        {
            Debug.LogError("Игрок не найден!");
            return;
        }

        if (playerController == null)
        {
            Debug.LogError("Контроллер игрока не найден!");
            return;
        }

        if (playerSpawner == null)
        {
            Debug.LogError("PlayerSpawner не найден!");
            return;
        }

        Transform spawnPoint = playerSpawner.GetNextRespawnPoint();
        if (spawnPoint == null)
        {
            Debug.LogError("Не удалось получить точку спавна!");
            return;
        }

        player.transform.position = spawnPoint.position;
        player.transform.rotation = spawnPoint.rotation;

        playerController.RestoreHealth();

        player.SetActive(true);

        if (phantomGhostObject != null)
        {
            phantomGhostObject.SetActive(true);
            Debug.Log("Призрак включен после возрождения игрока");
        }

        deathScreenImage.gameObject.SetActive(false);
        respawnButton.gameObject.SetActive(false);
        deathText.gameObject.SetActive(false);

        ResetElementsAlpha();

        // Скрываем и блокируем курсор обратно после респавна
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Debug.Log("Игрок успешно возрожден");
    }


    private void ResetElementsAlpha()
    {
        Color color;

        color = deathScreenImage.color;
        color.a = 1f;
        deathScreenImage.color = color;

        color = deathText.color;
        color.a = 1f;
        deathText.color = color;

        Image buttonImage = respawnButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            color = buttonImage.color;
            color.a = 1f;
            buttonImage.color = color;
        }

        TextMeshProUGUI buttonText = respawnButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            color = buttonText.color;
            color.a = 1f;
            buttonText.color = color;
        }
    }

    private void OnDestroy()
    {
        if (respawnButton != null)
            respawnButton.onClick.RemoveListener(RespawnPlayer);
    }
}
