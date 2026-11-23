using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Zentraler Spielzustand: Spieler, lebende Gegner, Sieg/Niederlage und
/// Neustart. Die Gegner tragen sich selbst ein und wieder aus.
/// </summary>
public static class GameSession
{
    public static readonly List<GameObject> Enemies = new List<GameObject>();

    public static Transform Player;
    public static int TotalEnemies;
    public static bool IsGameOver;
    public static bool HasWon;
    public static bool IsReloading;

    public static int EnemiesAlive => Enemies.Count;
    public static int EnemiesDestroyed => Mathf.Max(0, TotalEnemies - Enemies.Count);

    public static void Reset()
    {
        Enemies.Clear();
        Player = null;
        TotalEnemies = 0;
        IsGameOver = false;
        HasWon = false;
        IsReloading = false;
    }

    public static void RegisterEnemy(GameObject enemy)
    {
        if (enemy != null && !Enemies.Contains(enemy))
        {
            Enemies.Add(enemy);
        }
    }

    public static void UnregisterEnemy(GameObject enemy)
    {
        Enemies.Remove(enemy);

        if (!IsReloading && !IsGameOver && TotalEnemies > 0 && Enemies.Count == 0)
        {
            HasWon = true;
        }
    }

    public static void CaptureEnemyCount()
    {
        TotalEnemies = Enemies.Count;
    }

    public static void TriggerGameOver()
    {
        if (!IsGameOver)
        {
            IsGameOver = true;
        }
    }

    public static void Restart()
    {
        IsReloading = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
