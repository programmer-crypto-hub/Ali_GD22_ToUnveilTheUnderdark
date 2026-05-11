using UnityEngine;
using Fusion;

public static class GameSettings
{
    public const string SoundPrefKey = "settings_sound";
    public const string MusicPrefKey = "settings_music";
    public const string FullscreenPrefKey = "settings_fullscreen";

    public const float DefaultSound = 1f;
    public const float DefaultMusic = 1f;
    public const int DefaultFullscreen = 1;

    public const string PlayerNamePrefKey = "settings_player_name";
    public const string GameCodePrefKey = "settings_game_code";
    public const string DefaultPlayerName = "Player";
    public const int DefaultGameCode = 000000;

    public struct Data
    {
        public float Sound;
        public float Music;
        public bool Fullscreen;
        public string PlayerName;
        public int GameCode;
    }

    public static Data Load()
    {
        return new Data
        {
            Sound = Mathf.Clamp01(PlayerPrefs.GetFloat(SoundPrefKey, DefaultSound)),
            Music = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicPrefKey, DefaultMusic)),
            Fullscreen = PlayerPrefs.GetInt(FullscreenPrefKey, DefaultFullscreen) == 1,
            PlayerName = PlayerPrefs.GetString(PlayerNamePrefKey, DefaultPlayerName),
            GameCode = PlayerPrefs.GetInt(GameCodePrefKey, DefaultGameCode)
        };
    }

    public static void Apply(Data data, AudioSource[] soundSources = null, AudioSource[] musicSources = null)
    {
        ApplySound(data.Sound, soundSources);
        ApplyMusic(data.Music, musicSources);
        ApplyFullscreen(data.Fullscreen);
    }

    public static void SetSound(float value, AudioSource[] soundSources = null)
    {
        Data data = Load();
        data.Sound = Mathf.Clamp01(value);
        Save(data);
        ApplySound(data.Sound, soundSources);
    }

    public static void SetMusic(float value, AudioSource[] musicSources = null)
    {
        Data data = Load();
        data.Music = Mathf.Clamp01(value);
        Save(data);
        ApplyMusic(data.Music, musicSources);
    }

    public static void SetFullscreen(bool value)
    {
        Data data = Load();
        data.Fullscreen = value;
        Save(data);
        ApplyFullscreen(data.Fullscreen);
    }

    public static void SetPlayerName(string playerName)
    {
        Data data = Load();
        data.PlayerName = playerName;
        Save(data);
        ApplyPlayerName(data.PlayerName);
    }

    public static void SetGameCode(int gameCode)
    {
        Data data = Load();
        data.GameCode = gameCode;
        Save(data);
        // Здесь нет прямого применения в рантайме, так что Apply не вызываем
    }

    private static void Save(Data data)
    {
        PlayerPrefs.SetFloat(SoundPrefKey, Mathf.Clamp01(data.Sound));
        PlayerPrefs.SetFloat(MusicPrefKey, Mathf.Clamp01(data.Music));
        PlayerPrefs.SetInt(FullscreenPrefKey, data.Fullscreen ? 1 : 0);
        PlayerPrefs.SetString(PlayerNamePrefKey, data.PlayerName);
        PlayerPrefs.SetInt(GameCodePrefKey, data.GameCode);
        PlayerPrefs.Save();
    }

    private static void ApplySound(float value, AudioSource[] soundSources)
    {
        ApplyVolume(value, useLoopSources: false, explicitSources: soundSources);
    }

    private static void ApplyMusic(float value, AudioSource[] musicSources)
    {
        ApplyVolume(value, useLoopSources: true, explicitSources: musicSources);
    }

    private static void ApplyVolume(float value, bool useLoopSources, AudioSource[] explicitSources)
    {
        if (explicitSources != null && explicitSources.Length > 0)
        {
            for (int i = 0; i < explicitSources.Length; i++)
            {
                if (explicitSources[i] != null)
                    explicitSources[i].volume = value;
            }

            return;
        }

        AudioSource[] sceneSources = Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < sceneSources.Length; i++)
        {
            AudioSource source = sceneSources[i];
            if (source != null && source.loop == useLoopSources)
                source.volume = value;
        }
    }

    private static void ApplyFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
    private static void ApplyPlayerName(string playerName)
    {
        PlayerStats.Instance.PlayerName = playerName;
        // Здесь можно добавить логику для обновления UI или других компонентов, если нужно
    }
}