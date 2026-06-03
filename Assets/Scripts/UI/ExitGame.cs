using UnityEngine;

public class ExitGame : MonoBehaviour
{
    // G?i hàm này t? Button OnClick ?? thoát game
    public void QuitGame()
    {
#if UNITY_EDITOR
        // N?u ?ang ch?y trong Editor, d?ng Play mode
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // N?u build lên platform, thoát ?ng d?ng
        Application.Quit();
#endif
    }
}
