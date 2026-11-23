using UnityEngine;

/// <summary>
/// Steuert das Schiff: W/S fahren vorwärts bzw. rückwärts, die Maus bestimmt
/// die Blickrichtung. Die Drehung läuft über MoveRotation, damit die Physik
/// korrekt bleibt.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Bewegung")]
    public float forwardSpeed = 5.2f;
    public float reverseSpeed = 3.2f;
    public float turnSpeed = 540f;

    [Header("Waffen")]
    public float explosiveCooldown = 0.9f;
    public float torpedoCooldown = 1.5f;

    private Rigidbody2D _body;
    private bool _alive = true;
    private float _nextExplosiveShot;
    private float _nextTorpedoShot;

    private void Awake()
    {
        _body = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (!_alive || GameSession.IsGameOver)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) && Time.time >= _nextExplosiveShot)
        {
            _nextExplosiveShot = Time.time + explosiveCooldown;
            FireExplosiveCannonball();
        }

        if (Input.GetMouseButtonDown(1) && Time.time >= _nextTorpedoShot)
        {
            _nextTorpedoShot = Time.time + torpedoCooldown;
            FireTorpedo();
        }
    }

    private void FireExplosiveCannonball()
    {
        if (!TryGetAimDirection(out Vector2 direction))
        {
            return;
        }

        Vector2 muzzle = (Vector2)transform.position + direction * 0.85f;
        GameBootstrap.Instance.SpawnExplosiveCannonball(muzzle, direction);
    }

    private void FireTorpedo()
    {
        if (!TryGetAimDirection(out Vector2 direction))
        {
            return;
        }

        Vector2 muzzle = (Vector2)transform.position + direction * 0.9f;
        GameBootstrap.Instance.SpawnTorpedo(muzzle, direction);
    }

    private bool TryGetAimDirection(out Vector2 direction)
    {
        direction = (Vector2)GetMousePosition() - (Vector2)transform.position;
        if (direction.sqrMagnitude < 0.001f)
        {
            return false;
        }

        direction.Normalize();
        return true;
    }

    private void FixedUpdate()
    {
        if (!_alive || GameSession.IsGameOver)
        {
            return;
        }

        Vector2 direction = (Vector2)GetMousePosition() - (Vector2)transform.position;
        if (direction.sqrMagnitude > 0.0001f)
        {
            // Das Schiff im Sprite-Sheet schaut nach unten. Bei Rotation 0 zeigt
            // der Bug also nach -Y; deshalb kommt auf den Winkel +90 Grad.
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f;
            float angle = Mathf.MoveTowardsAngle(_body.rotation, targetAngle, turnSpeed * Time.fixedDeltaTime);
            _body.MoveRotation(angle);
            _body.angularVelocity = 0f;
        }

        float input = 0f;
        if (Input.GetKey(KeyCode.W))
        {
            input += 1f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            input -= 1f;
        }

        float speed = input >= 0f ? forwardSpeed : reverseSpeed;
        Vector2 forward = -(Vector2)transform.up;
        _body.linearVelocity = forward * (input * speed);
    }

    /// <summary>Wird von feindlichen Treffern aufgerufen.</summary>
    public void Die()
    {
        if (!_alive)
        {
            return;
        }

        _alive = false;
        _body.linearVelocity = Vector2.zero;

        var collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        var renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }

        GameSession.Player = null;
        GameSession.TriggerGameOver();
        GameBootstrap.Instance.SpawnExplosion(transform.position);
        Destroy(gameObject, 0.05f);
    }

    private Vector3 GetMousePosition()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0;
        return mouseWorldPos;
    }
}
