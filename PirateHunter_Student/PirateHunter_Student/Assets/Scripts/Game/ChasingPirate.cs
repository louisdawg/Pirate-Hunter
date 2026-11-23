using UnityEngine;

/// <summary>
/// Piratenschiff: Prüft per Raycast die Line of Sight zum Spieler, verfolgt
/// ihn, solange er sichtbar ist, und kehrt danach zum Ausgangspunkt zurück.
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
    private SpriteRenderer _alertRing;
    private Vector2 _spawnPoint;

    private void Awake()
    {
        _body = GetComponent<Rigidbody2D>();
        _spawnPoint = transform.position;

        var ring = new GameObject("AlertRing");
        ring.transform.SetParent(transform, false);
        ring.transform.localScale = Vector3.one * 1.5f;
        _alertRing = ring.AddComponent<SpriteRenderer>();
        _alertRing.sprite = SpriteFactory.Ring;
        _alertRing.color = new Color(1f, 0.25f, 0.2f, 0.8f);
        _alertRing.sortingOrder = 2;
        _alertRing.enabled = false;

        GameSession.RegisterEnemy(gameObject);
    }

    private void OnDestroy()
    {
        GameSession.UnregisterEnemy(gameObject);
    }

    private void FixedUpdate()
    {
        if (GameSession.IsGameOver)
        {
            _body.linearVelocity = Vector2.zero;
            _alertRing.enabled = false;
            return;
        }

        if (GameSession.Player == null)
        {
            _alertRing.enabled = false;
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

        _alertRing.enabled = seesPlayer;

        if (seesPlayer)
        {
            Chase(toPlayer);
        }
        else
        {
            ReturnToStart();
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
