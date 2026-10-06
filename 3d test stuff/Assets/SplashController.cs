using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class SplashController : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Exact name of your next scene (e.g., MainMenu)")]
    [SerializeField] private string nextSceneName = "Gameplay";

    [Header("Behavior")]
    [Tooltip("Allow pressing Space, Escape, or clicking to skip?")]
    [SerializeField] private bool allowSkip = true;

    private VideoPlayer videoPlayer;
    private bool isTransitioning = false;

    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
    }

    private void Start()
    {
        // Subscribe to video completion event
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoEnd;
        }
    }

    private void Update()
    {
        // Check for skip input every frame
        if (allowSkip && !isTransitioning)
        {
            if (Input.GetKeyDown(KeyCode.Space) ||
                Input.GetKeyDown(KeyCode.Escape) ||
                Input.GetMouseButtonDown(0))
            {
                LoadNextScene();
            }
        }
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnd;
        }

        SceneManager.LoadScene("Gameplay");
    }
}