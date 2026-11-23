using UnityEngine;

/// <summary>
/// Zentrale Layer-Namen und Layermasks. Damit tippt man die Layer nicht an
/// jeder Stelle als Zahl oder String neu.
/// </summary>
public static class GameLayers
{
    public static readonly int Player = LayerMask.NameToLayer("Player");
    public static readonly int Enemy = LayerMask.NameToLayer("Enemy");
    public static readonly int Obstacle = LayerMask.NameToLayer("Obstacle");

    public static readonly int PlayerMask = 1 << Player;
    public static readonly int EnemyMask = 1 << Enemy;
    public static readonly int PlayerOrObstacleMask = (1 << Player) | (1 << Obstacle);
}
