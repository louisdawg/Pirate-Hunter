using UnityEngine;

/// <summary>
/// Torpedo des Spielers. Er sucht sich automatisch das nächste Ziel und dreht
/// sich nur langsam darauf zu, statt sofort exakt auszurichten.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Torpedo : MonoBehaviour
{
    public float speed = 6.5f;
    public float turnSpeed = 110f;
    public float lifetime = 8f;

    private Rigidbody2D _body;
    private Transform _target;
    private bool _impacted;

    public void Initialize(Vector2 direction)
    {
        _body = GetComponent<Rigidbody2D>();
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f;
        _body.rotation = angle;

        Vector2 nose = Quaternion.Euler(0f, 0f, angle) * Vector2.down;
        _body.linearVelocity = nose * speed;

        AcquireTarget();
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        if (_body == null)
        {
            return;
        }

        if (!IsValidTarget())
        {
            AcquireTarget();
        }

        if (_target != null)
        {
            Vector2 toTarget = (Vector2)_target.position - (Vector2)transform.position;
            if (toTarget.sqrMagnitude > 0.0001f)
            {
                float targetAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg + 90f;
                float angle = Mathf.MoveTowardsAngle(_body.rotation, targetAngle, turnSpeed * Time.fixedDeltaTime);
                _body.MoveRotation(angle);
                Debug.DrawLine(transform.position, _target.position, new Color(1f, 0.6f, 0.2f));
            }
        }

        Vector2 noseDirection = Quaternion.Euler(0f, 0f, _body.rotation) * Vector2.down;
        _body.linearVelocity = noseDirection * speed;
    }

    private void AcquireTarget()
    {
        _target = null;
        float nearestDistance = float.MaxValue;
        Vector2 position = transform.position;

        foreach (GameObject enemy in GameSession.Enemies)
        {
            if (enemy == null || !enemy.activeInHierarchy)
            {
                continue;
            }

            float distance = ((Vector2)enemy.transform.position - position).sqrMagnitude;
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                _target = enemy.transform;
            }
        }
    }

    private bool IsValidTarget()
    {
        return _target != null && _target.gameObject.activeInHierarchy;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_impacted || other.GetComponentInParent<PlayerController>() != null)
        {
            return;
        }

        if (other.CompareTag("Enemy"))
        {
            _impacted = true;

            GameObject enemy = other.attachedRigidbody != null
                ? other.attachedRigidbody.gameObject
                : other.gameObject;

            GameBootstrap.Instance.SpawnExplosion(enemy.transform.position);
            Destroy(enemy);
            Destroy(gameObject);
        }
        else if (other.gameObject.layer == GameLayers.Obstacle)
        {
            _impacted = true;
            GameBootstrap.Instance.SpawnExplosion(transform.position);
            Destroy(gameObject);
        }
    }
}
