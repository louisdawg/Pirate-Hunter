using UnityEngine;

/// <summary>
/// Folgt dem Spieler weich und begrenzt die Kamera auf die Spielwelt, damit
/// man nie über den Rand der Karte hinausschaut.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    public float followSharpness = 8f;

    private Transform _target;
    private Camera _camera;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    public void SetTarget(Transform target)
    {
        _target = target;
        if (_target != null)
        {
            transform.position = GetDesiredPosition();
        }
    }

    private void LateUpdate()
    {
        if (_target == null)
        {
            return;
        }

        transform.position = Vector3.Lerp(
            transform.position,
            GetDesiredPosition(),
            1f - Mathf.Exp(-followSharpness * Time.deltaTime));
    }

    private Vector3 GetDesiredPosition()
    {
        float halfHeight = _camera.orthographicSize;
        float halfWidth = halfHeight * _camera.aspect;

        float clampX = Mathf.Max(0f, GameBootstrap.WorldHalfWidth - halfWidth);
        float clampY = Mathf.Max(0f, GameBootstrap.WorldHalfHeight - halfHeight);

        return new Vector3(
            Mathf.Clamp(_target.position.x, -clampX, clampX),
            Mathf.Clamp(_target.position.y, -clampY, clampY),
            -10f);
    }
}
