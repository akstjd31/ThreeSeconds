using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceContactDetector : MonoBehaviour
{
    [SerializeField] private float contactDuration;
    [SerializeField] private bool isContact = false;
    private const float maxContactTime = 3f; // 최대 접촉 허용 시간
    [SerializeField] private Vector3 respawnPosition; // 리스폰 위치
    [SerializeField] private Quaternion respawnRotation; // 리스폰 회전
    private GUIStyle guiStyle;
    private bool isFading = false;
    private float fadeAlpha = 0f;
    private Texture2D fadeTexture;

    private void Start()
    {
        Init();
        guiStyle = new GUIStyle();
        guiStyle.fontSize = 24; // 글자 크기
        guiStyle.normal.textColor = Color.white; // 글자 색상
        guiStyle.normal.background = MakeTexture(2, 2, new Color(0.3f, 0.3f, 0.3f, 0.8f)); // 회색 배경
        guiStyle.padding = new RectOffset(10, 10, 5, 5); // 패딩 설정

        // 페이드 효과용 검은 텍스처 생성
        fadeTexture = MakeTexture(2, 2, Color.black);
    }

    private void Init()
    {
        respawnPosition = transform.position;
        respawnRotation = transform.rotation;
        contactDuration = 0f;
    }

    // 단색 텍스처를 생성하는 유틸리티 메서드
    private Texture2D MakeTexture(int width, int height, Color color)
    {
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    private void Update()
    {
        if (contactDuration >= maxContactTime && !isFading)
        {
            StartCoroutine(FadeAndRespawn());
        }

        if (isContact)
        {
            contactDuration += Time.deltaTime;
        }
    }

    public bool GameOver()
    {
        return isFading;
    }

    private IEnumerator FadeAndRespawn()
    {
        isFading = true;

        // 페이드 인
        while (fadeAlpha < 1.0f)
        {
            fadeAlpha += Time.deltaTime;
            yield return null;
        }

        // 플레이어 위치 리셋
        transform.position = respawnPosition;
        contactDuration = 0f;
        isContact = false;

        // 페이드 아웃
        while (fadeAlpha > 0f)
        {
            fadeAlpha -= Time.deltaTime;
            yield return null;
        }

        isFading = false;
        Init();
    }

    private void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("DangerousTerrain"))
        {
            isContact = true;
        }
    }

    private void OnCollisionExit(Collision col)
    {
        if (col.gameObject.CompareTag("DangerousTerrain"))
        {
            isContact = false;
        }
    }

    private void OnGUI()
    {
        // 타이머 표시 (기존 코드)
        GUI.Label(new Rect(20, 20, 300, 40), $"Contact Duration: {contactDuration:F2}", guiStyle);

        // 페이드 효과 표시
        if (isFading)
        {
            GUI.color = new Color(0, 0, 0, fadeAlpha);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), fadeTexture);
        }
    }
}
