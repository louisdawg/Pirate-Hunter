using UnityEngine;

/// <summary>
/// Minimales HUD ohne UI-Paket: Steuerung, Gegnerzähler und Endscreen.
/// Wird zur Laufzeit vom GameBootstrap an das Bootstrap-Objekt gehängt.
/// </summary>
public class GameHud : MonoBehaviour
{
    private Texture2D _panel;
    private Sprite _crosshair;
    private GUIStyle _titleStyle;
    private GUIStyle _labelStyle;
    private GUIStyle _smallStyle;
    private GUIStyle _centerStyle;
    private GUIStyle _centerSmallStyle;
    private bool _stylesReady;

    private void Awake()
    {
        _panel = new Texture2D(1, 1, TextureFormat.RGBA32, false)
        {
            hideFlags = HideFlags.DontSave
        };
        _panel.SetPixel(0, 0, new Color(0.02f, 0.08f, 0.12f, 0.72f));
        _panel.Apply();

        _crosshair = SpriteFactory.Ring;
    }

    private void Update()
    {
        if ((GameSession.IsGameOver || GameSession.HasWon) && Input.GetKeyDown(KeyCode.R))
        {
            GameSession.Restart();
        }
    }

    private void OnGUI()
    {
        EnsureStyles();

        float scale = Mathf.Max(0.75f, Screen.height / 1080f);
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));

        float width = Screen.width / scale;
        float height = Screen.height / scale;

        DrawControlPanel();
        DrawStatusPanel(width);
        DrawCrosshair(scale);

        if (GameSession.IsGameOver || GameSession.HasWon)
        {
            DrawEndScreen(width, height);
        }
        else
        {
            GUI.Label(
                new Rect(width * 0.5f - 360f, height - 56f, 720f, 30f),
                "W / S  fahren     ·     Maus  zielen     ·     LMB  Explosivkanone",
                _centerSmallStyle);
        }
    }

    private void DrawControlPanel()
    {
        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(28f, 28f, 520f, 236f), _panel);

        GUI.Label(new Rect(48f, 38f, 480f, 46f), "PIRATE HUNTER", _titleStyle);
        GUI.Label(new Rect(50f, 86f, 480f, 24f), "Raycasts & OverlapCircle", _smallStyle);

        GUI.Label(new Rect(48f, 124f, 480f, 26f), "W  /  S      vorwärts / rückwärts", _labelStyle);
        GUI.Label(new Rect(48f, 156f, 480f, 26f), "Maus         Schiff dorthin drehen", _labelStyle);
        GUI.Label(new Rect(48f, 188f, 480f, 26f), "LMB          Explosivkanone (2 m)", _labelStyle);
    }

    private void DrawStatusPanel(float width)
    {
        GUI.DrawTexture(new Rect(width - 328f, 28f, 300f, 132f), _panel);

        GUI.Label(new Rect(width - 306f, 40f, 260f, 30f), $"ZIELE: {GameSession.EnemiesAlive}", _labelStyle);
        GUI.Label(new Rect(width - 306f, 76f, 260f, 30f), $"VERSENKT: {GameSession.EnemiesDestroyed}", _labelStyle);
        GUI.Label(new Rect(width - 306f, 114f, 260f, 24f), "R  –  Neustart", _smallStyle);
    }

    private void DrawCrosshair(float scale)
    {
        if (GameSession.IsGameOver)
        {
            return;
        }

        Vector3 mouse = Input.mousePosition;
        float x = mouse.x / scale;
        float y = (Screen.height - mouse.y) / scale;

        GUI.color = new Color(1f, 0.85f, 0.3f, 0.85f);
        GUI.DrawTexture(new Rect(x - 14f, y - 14f, 28f, 28f), _crosshair.texture);
        GUI.color = Color.white;
    }

    private void DrawEndScreen(float width, float height)
    {
        GUI.color = new Color(0f, 0f, 0f, 0.62f);
        GUI.DrawTexture(new Rect(0f, 0f, width, height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        string headline = GameSession.HasWon ? "SEE GESICHERT!" : "SCHIFF VERSENKT";
        string hint = GameSession.HasWon
            ? "Alle Ziele wurden zerstört."
            : "Eine Kanonenkugel hat dein Schiff getroffen.";

        GUI.Label(new Rect(0f, height * 0.5f - 110f, width, 70f), headline, _centerStyle);
        GUI.Label(new Rect(0f, height * 0.5f - 30f, width, 34f), hint, _centerSmallStyle);
        GUI.Label(new Rect(0f, height * 0.5f + 24f, width, 34f), "R  –  Neustart", _centerSmallStyle);
    }

    private void EnsureStyles()
    {
        if (_stylesReady)
        {
            return;
        }

        _titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 36,
            fontStyle = FontStyle.Bold
        };
        _titleStyle.normal.textColor = new Color(1f, 0.86f, 0.4f);

        _labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20
        };
        _labelStyle.normal.textColor = new Color(0.9f, 0.96f, 1f);

        _smallStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16
        };
        _smallStyle.normal.textColor = new Color(0.68f, 0.82f, 0.9f);

        _centerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 54,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        _centerStyle.normal.textColor = new Color(1f, 0.9f, 0.6f);

        _centerSmallStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 22,
            alignment = TextAnchor.MiddleCenter
        };
        _centerSmallStyle.normal.textColor = Color.white;

        _stylesReady = true;
    }
}
