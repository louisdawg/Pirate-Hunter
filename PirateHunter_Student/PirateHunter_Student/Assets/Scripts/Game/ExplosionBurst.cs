using UnityEngine;

/// <summary>
/// Kleiner Ersatz-Effekt, falls das Explosion-Prefab nicht zugewiesen ist.
/// Ein Kreis wird größer und blendet dabei aus.
/// </summary>
public class ExplosionBurst : MonoBehaviour
{
    public float duration = 0.4f;

    private SpriteRenderer _renderer;
    private float _startScale;
    private float _endScale;
    private float _time;

    public void Initialize(Color color, float startScale, float endScale)
    {
        _renderer = GetComponent<SpriteRenderer>();
        _renderer.color = color;
        _startScale = startScale;
        _endScale = endScale;
        transform.localScale = Vector3.one * _startScale;
    }

    private void Update()
    {
        _time += Time.deltaTime;
        float progress = Mathf.Clamp01(_time / duration);
        transform.localScale = Vector3.one * Mathf.Lerp(_startScale, _endScale, progress);

        if (_renderer != null)
        {
            Color color = _renderer.color;
            color.a = 1f - progress;
            _renderer.color = color;
        }

        if (_time >= duration)
        {
            Destroy(gameObject);
        }
    }
}
