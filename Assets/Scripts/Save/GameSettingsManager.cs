using UnityEngine;

public static class GameSettingsManager
{
    private const string KeyLanguage = "settings_language";
    private const string KeyBGMVolume = "settings_bgm_volume";
    private const string KeySFXVolume = "settings_sfx_volume";
    private const string KeyBGMMute = "settings_bgm_mute";
    private const string KeySFXMute = "settings_sfx_mute";

    private const float DefaultBGMVolume = 0.5f;
    private const float DefaultSFXVolume = 0.7f;
    private const string DefaultLanguage = "en";

    public static string GetLanguage(string fallback = null)
    {
        return PlayerPrefs.GetString(KeyLanguage, fallback ?? DefaultLanguage);
    }

    public static void SetLanguage(string language)
    {
        PlayerPrefs.SetString(KeyLanguage, language);
        PlayerPrefs.Save();
    }

    public static float GetBGMVolume()
    {
        return PlayerPrefs.GetFloat(KeyBGMVolume, DefaultBGMVolume);
    }

    public static void SetBGMVolume(float volume)
    {
        PlayerPrefs.SetFloat(KeyBGMVolume, Mathf.Clamp01(volume));
        PlayerPrefs.Save();
    }

    public static float GetSFXVolume()
    {
        return PlayerPrefs.GetFloat(KeySFXVolume, DefaultSFXVolume);
    }

    public static void SetSFXVolume(float volume)
    {
        PlayerPrefs.SetFloat(KeySFXVolume, Mathf.Clamp01(volume));
        PlayerPrefs.Save();
    }

    public static bool GetBGMMute()
    {
        return PlayerPrefs.GetInt(KeyBGMMute, 0) == 1;
    }

    public static void SetBGMMute(bool muted)
    {
        PlayerPrefs.SetInt(KeyBGMMute, muted ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static bool GetSFXMute()
    {
        return PlayerPrefs.GetInt(KeySFXMute, 0) == 1;
    }

    public static void SetSFXMute(bool muted)
    {
        PlayerPrefs.SetInt(KeySFXMute, muted ? 1 : 0);
        PlayerPrefs.Save();
    }
}
