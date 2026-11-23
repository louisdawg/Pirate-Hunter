using UnityEngine;

/// <summary>
/// Kanonenkugel der feindlichen Kanonen. Trifft sie den Spieler, wird das
/// Schiff zerstört (inklusive Explosion) und die Kugel verschwindet.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Cannonball : MonoBehaviour
{
    public float lifetime = 6f;

    private Rigidbody2D _body;

    public void Initialize(Vector2 direction, float speed)
    {
        _body = GetComponent<Rigidbody2D>();
        _body.linearVelocity = direction.normalized * speed;
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponentInParent<PlayerController>();
        if (player != null)
        {
            player.Die();
            Destroy(gameObject);
            return;
        }

        if (other.gameObject.layer == GameLayers.Obstacle)
        {
            Destroy(gameObject);
        }
    }
}
