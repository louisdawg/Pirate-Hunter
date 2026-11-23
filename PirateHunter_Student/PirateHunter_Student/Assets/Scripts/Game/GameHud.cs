using UnityEngine;

/// <summary>
/// Zeigt nur noch den Endscreen. Das laufende Spiel bleibt bewusst ohne HUD,
/// damit die Szene so clean wie in der Referenz aussieht.
/// </summary>
public class GameHud : MonoBehaviour
{
    private GUIStyle _headlineStyle;
    private GUIStyle _textStyle;
    private bool _stylesReady;

    private void Update()
    {
        if ((GameSession.IsGameOver || GameSession.HasWon) && Input.GetKeyDown(KeyCode.R))
        {
            GameSession.Restart();
        }
    }

    private void OnGUI()
    {
        if (!GameSession.IsGameOver && !GameSession.HasWon)
        {
            return;
        }

        EnsureStyles();

        float scale = Mathf.Max(0.75f, Screen.height / 1080f);
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));

        float width = Screen.width / scale;
        float height = Screen.height / scale;

        GUI.color = new Color(0f, 0f, 0f, 0.62f);
        GUI.DrawTexture(new Rect(0f, 0f, width, height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        string headline = GameSession.HasWon ? "SEE GESICHERT!" : "SCHIFF VERSENKT";
        string hint = GameSession.HasWon
            ? "Alle Ziele wurden zerstört."
            : "Eine Kanonenkugel hat dein Schiff getroffen.";

        GUI.Label(new Rect(0f, height * 0.5f - 110f, width, 70f), headline, _headlineStyle);
        GUI.Label(new Rect(0f, height * 0.5f - 30f, width, 34f), hint, _textStyle);
        GUI.Label(new Rect(0f, height * 0.5f + 24f, width, 34f), "R  –  Neustart", _textStyle);
    }

    private void EnsureStyles()
    {
        if (_stylesReady)
        {
            return;
        }

        _headlineStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 54,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        _headlineStyle.normal.textColor = new Color(1f, 0.9f, 0.6f);

        _textStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 22,
            alignment = TextAnchor.MiddleCenter
        };
        _textStyle.normal.textColor = Color.white;

        _stylesReady = true;
    }
}
