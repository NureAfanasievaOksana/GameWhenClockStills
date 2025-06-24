using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BusController : MonoBehaviour
{
    [Header("Timing Settings")]
    public float idleShakeDuration = 3f;
    public float moveDuration = 5f;
    public Image fadeImage;
    public float fadeSpeed = 1f;
    public string nextSceneName = "NextScene";

    [Header("Movement Settings")]
    public float moveSpeed = 4f;
    public float endPositionX = 16f;

    [Header("Shake Settings")]
    public float shakeAmount = 0.05f;
    public float shakeSpeed = 3f;

    [Header("Wheel Settings")]
    public Transform frontWheel;
    public Transform rearWheel;
    public Transform busBody;
    public float wheelRotationSpeed = 360f;

    private Vector3 originalBusBodyLocalPosition;
    private bool isMoving = false;

    void Start()
    {
        StartCoroutine(FadeIn());
        originalBusBodyLocalPosition = busBody.localPosition;

        Invoke("StartMoving", idleShakeDuration);
        Invoke("LoadNextScene", idleShakeDuration + moveDuration);
    }

    IEnumerator FadeIn()
    {
        float alpha = 1;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
    }

    void Update()
    {
        ApplyShakeEffect();

        if (isMoving)
        {
            MoveBus();
            RotateWheels();
        }
    }

    void ApplyShakeEffect()
    {
        float shakeOffset = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;
        busBody.localPosition = new Vector3(
            originalBusBodyLocalPosition.x,
            originalBusBodyLocalPosition.y + shakeOffset,
            originalBusBodyLocalPosition.z
        );
    }

    void MoveBus()
    {
        if (transform.position.x < endPositionX)
        {
            float step = moveSpeed * Time.deltaTime;
            transform.Translate(Vector3.right * step, Space.World);
        }
    }

    void RotateWheels()
    {
        float rotation = -wheelRotationSpeed * Time.deltaTime;
        frontWheel.Rotate(0, 0, rotation, Space.Self);
        rearWheel.Rotate(0, 0, rotation, Space.Self);
    }

    void StartMoving()
    {
        isMoving = true;
    }

    void LoadNextScene()
    {
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        float alpha = 0;
        while (alpha < 1)
        {
            alpha += Time.deltaTime * fadeSpeed;
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        SceneManager.LoadScene(nextSceneName);
    }
}
