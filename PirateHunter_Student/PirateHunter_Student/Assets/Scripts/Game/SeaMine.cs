using UnityEngine;

/// <summary>
/// Eigenes Feature: Seeminen. Sie erkennen den Spieler per OverlapCircle,
/// zünden nach einer kurzen Lunte und lösen eine Kettenreaktion aus.
/// </summary>
public class SeaMine : MonoBehaviour
{
    [Header("Erkennung")]
    public float triggerRadius = 1.6f;
    public float fuseTime = 0.45f;
    public float chainRadius = 2.5f;

    private SpriteRenderer _light;
    private SpriteRenderer _warningRing;
    private Vector3 _basePosition;
    private float _phase;
    private float _fuse = -1f;
    private bool _detonated;

    private void Awake()
    {
        _basePosition = transform.position;
        BuildVisuals();
    }

    private void BuildVisuals()
    {
        transform.localScale = Vector3.one * 1.15f;

        var body = gameObject.AddComponent<SpriteRenderer>();
        body.sprite = SpriteFactory.Mine;
        body.sortingOrder = 2;

        var warning = new GameObject("WarningRing");
        warning.transform.SetParent(transform, false);
        warning.transform.localScale = Vector3.one * (triggerRadius * 2f);
        _warningRing = warning.AddComponent<SpriteRenderer>();
        _warningRing.sprite = SpriteFactory.Ring;
        _warningRing.color = new Color(1f, 0.4f, 0.2f, 0.25f);
        _warningRing.sortingOrder = 1;
        _warningRing.enabled = false;

        var light = new GameObject("BlinkingLight");
        light.transform.SetParent(transform, false);
        light.transform.localPosition = new Vector3(0f, 0.05f, 0f);
        light.transform.localScale = Vector3.one * 0.22f;
        _light = light.AddComponent<SpriteRenderer>();
        _light.sprite = SpriteFactory.Circle;
        _light.color = new Color(1f, 0.25f, 0.2f);
        _light.sortingOrder = 3;
    }

    private void Update()
    {
        _phase += Time.deltaTime;
        transform.position = _basePosition + Vector3.up * Mathf.Sin(_phase * 1.8f) * 0.08f;

        float blink = 0.35f + 0.65f * Mathf.PingPong(_phase * 2f, 1f);
        _light.color = new Color(1f, 0.25f, 0.2f, blink);

        if (_detonated)
        {
            return;
        }

        if (PlayerInRange())
        {
            _warningRing.enabled = true;
            float alpha = 0.35f + 0.4f * Mathf.PingPong(_phase * 6f, 1f);
            _warningRing.color = new Color(1f, 0.35f, 0.2f, alpha);

            if (_fuse < 0f)
            {
                _fuse = 0f;
            }

            _fuse += Time.deltaTime;
            if (_fuse >= fuseTime)
            {
                Detonate();
            }
        }
        else
        {
            _fuse = -1f;
            _warningRing.enabled = false;
        }
    }

    private bool PlayerInRange()
    {
        if (GameSession.Player == null)
        {
            return false;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, triggerRadius, GameLayers.PlayerMask);
        foreach (Collider2D hit in hits)
        {
            if (hit.GetComponentInParent<PlayerController>() != null)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Zündet die Mine und alle Minen in ihrer Kettenreaktion.</summary>
    public void Detonate()
    {
        if (_detonated)
        {
            return;
        }

        _detonated = true;
        GameBootstrap.Instance.SpawnExplosion(transform.position);

        if (GameSession.Player != null)
        {
            float distance = Vector2.Distance(GameSession.Player.position, transform.position);
            if (distance <= triggerRadius + 0.5f)
            {
                var player = GameSession.Player.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.Die();
                }
            }
        }

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, chainRadius);
        foreach (Collider2D hit in colliders)
        {
            var mine = hit.GetComponentInParent<SeaMine>();
            if (mine != null && mine != this)
            {
                mine.Detonate();
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0.2f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
}
