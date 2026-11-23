using UnityEngine;

/// <summary>
/// Piratenschiff: Prüft per Raycast die Line of Sight zum Spieler, verfolgt
/// ihn, solange er sichtbar ist, und kehrt danach zum Ausgangspunkt zurück.
/// Der Alarm-Ring ist ein Sprite aus dem Art-Ordner.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class ChasingPirate : MonoBehaviour
{
    [Header("Sicht")]
    public float sightRange = 9f;

    [Header("Bewegung")]
    public float chaseSpeed = 3.4f;
    public float returnSpeed = 2.6f;
    public float turnSpeed = 130f;

    private Rigidbody2D _body;
    private SpriteRenderer _alertMarker;
    private Vector2 _spawnPoint;

    private void Awake()
    {
        _body = GetComponent<Rigidbody2D>();
        _spawnPoint = transform.position;
        GameSession.RegisterEnemy(gameObject);
    }

    private void OnDestroy()
    {
        GameSession.UnregisterEnemy(gameObject);
    }

    /// <summary>Legt das Art-Sprite für den Alarm-Ring über dem Schiff fest.</summary>
    public void SetAlertSprite(Sprite sprite)
    {
        if (sprite == null)
        {
            return;
        }

        if (_alertMarker == null)
        {
            var marker = new GameObject("AlertMarker");
            marker.transform.SetParent(transform, false);
            marker.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            marker.transform.localScale = Vector3.one * 1.15f;
            _alertMarker = marker.AddComponent<SpriteRenderer>();
            _alertMarker.sortingOrder = 6;
        }

        _alertMarker.sprite = sprite;
        _alertMarker.color = new Color(1f, 0.3f, 0.25f, 0.9f);
        _alertMarker.enabled = false;
    }

    private void FixedUpdate()
    {
        if (GameSession.IsGameOver)
        {
            _body.linearVelocity = Vector2.zero;
            SetAlert(false);
            return;
        }

        if (GameSession.Player == null)
        {
            SetAlert(false);
            ReturnToStart();
            return;
        }

        Vector2 toPlayer = (Vector2)GameSession.Player.position - (Vector2)transform.position;
        float distance = toPlayer.magnitude;

        bool seesPlayer = false;
        if (distance > 0.01f && distance <= sightRange)
        {
            Vector2 direction = toPlayer / distance;

            // Player + Obstacle: Inseln unterbrechen die Sichtlinie.
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                direction,
                distance,
                GameLayers.PlayerOrObstacleMask);

            seesPlayer = hit.collider != null && hit.collider.CompareTag("Player");
            if (seesPlayer)
            {
                Debug.DrawLine(transform.position, hit.point, Color.yellow);
            }
        }

        SetAlert(seesPlayer);

        if (seesPlayer)
        {
            Chase(toPlayer);
        }
        else
        {
            ReturnToStart();
        }
    }

    private void SetAlert(bool visible)
    {
        if (_alertMarker != null)
        {
            _alertMarker.enabled = visible;
        }
    }

    private void Chase(Vector2 toPlayer)
    {
        Vector2 direction = toPlayer.normalized;
        SteerTowards(direction);

        Vector2 nose = -(Vector2)transform.up;
        float alignment = Mathf.Clamp01(Vector2.Dot(nose, direction));
        _body.linearVelocity = direction * (chaseSpeed * Mathf.Max(0.35f, alignment));
    }

    private void ReturnToStart()
    {
        Vector2 toHome = _spawnPoint - (Vector2)transform.position;
        if (toHome.magnitude < 0.2f)
        {
            _body.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direction = toHome.normalized;
        SteerTowards(direction);
        _body.linearVelocity = direction * returnSpeed;
    }

    private void SteerTowards(Vector2 direction)
    {
        // Das Schiff-Sprite schaut nach unten, daher +90 Grad auf den Winkel.
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f;
        float angle = Mathf.MoveTowardsAngle(_body.rotation, targetAngle, turnSpeed * Time.fixedDeltaTime);
        _body.MoveRotation(angle);
        _body.angularVelocity = 0f;
    }
}
