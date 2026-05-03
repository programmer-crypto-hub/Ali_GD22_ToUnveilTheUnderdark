using System;
using UnityEngine;

/*
 * LevelSequenceData
 * Usage: contains the ordered list of level scene names for the vertical slice run.
 * Reason for usage: to have a single source of truth for the level progression order, which can be easily edited in the Unity Editor.
 * As an SO with string references, it avoids hardcoding scene names in multiple places and allows designers to change the level order without code changes.
 */
[CreateAssetMenu(
    fileName = "LevelSequenceData",
    menuName = "Game Data/Levels/Level Sequence",
    order = 0)]
public class LevelSequenceData : ScriptableObject
{
    [Tooltip("Ordered list of gameplay scenes for the vertical slice run.")]
    [SerializeField] private string[] levelSceneNames = { SceneNames.GameScene };

    public int LevelCount => levelSceneNames != null ? levelSceneNames.Length : 0;

    /// <summary>
    /// Safely tries to get the scene name for a given level index.
    /// </summary>
    public bool TryGetLevelSceneName(int index, out string sceneName)
    {
        sceneName = null;

        if (levelSceneNames == null || index < 0 || index >= levelSceneNames.Length)
            return false;

        string value = levelSceneNames[index];
        if (string.IsNullOrWhiteSpace(value))
            return false;

        sceneName = value.Trim();
        return true;
    }

    /// <summary>
    /// Finds the index of a given scene name in the level sequence. Returns -1 if not found or if input is invalid.
    /// </summary>
    public int FindLevelIndex(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName) || levelSceneNames == null)
            return -1;

        for (int i = 0; i < levelSceneNames.Length; i++)
        {
            if (string.Equals(levelSceneNames[i], sceneName, StringComparison.Ordinal))
                return i;
        }

        return -1;
    }
}