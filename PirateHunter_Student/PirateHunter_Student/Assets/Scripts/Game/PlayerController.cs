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

    private Rigidbody2D _body;
    private bool _alive = true;

    private void Awake()
    {
        _body = GetComponent<Rigidbody2D>();
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

    private Vector3 GetMousePosition()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0;
        return mouseWorldPos;
    }
}
