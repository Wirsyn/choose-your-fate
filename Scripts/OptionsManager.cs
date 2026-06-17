using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsManager : MonoBehaviour
{
    [Header("Elementy UI")]
    public Slider volumeSlider;
    public Toggle fullscreenToggle;
    public TMP_Dropdown resolutionDropdown;

    private List<Resolution> filteredResolutions;

    void Start()
    {
        InitializeResolutions();
        LoadSettings();
    }

    private void InitializeResolutions()
    {
        if (resolutionDropdown == null) return;

        Resolution[] allResolutions = Screen.resolutions;
        filteredResolutions = new List<Resolution>();
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResIndex = 0;

        for (int i = 0; i < allResolutions.Length; i++)
        {
            string option = allResolutions[i].width + " x " + allResolutions[i].height;

            if (!options.Contains(option))
            {
                options.Add(option);
                filteredResolutions.Add(allResolutions[i]);

                if (allResolutions[i].width == Screen.width && allResolutions[i].height == Screen.height)
                {
                    currentResIndex = filteredResolutions.Count - 1;
                }
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionPreference", currentResIndex);
        resolutionDropdown.RefreshShownValue();
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("VolumePreference", volume);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("FullscreenPreference", isFullscreen ? 1 : 0);
    }

    public void SetResolution(int resolutionIndex)
    {
        if (filteredResolutions == null || filteredResolutions.Count <= resolutionIndex) return;

        Resolution resolution = filteredResolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        PlayerPrefs.SetInt("ResolutionPreference", resolutionIndex);
    }

    public void ResetToDefaults()
    {
        SetVolume(1f);
        if (volumeSlider != null) volumeSlider.value = 1f;

        SetFullscreen(true);
        if (fullscreenToggle != null) fullscreenToggle.isOn = true;

        if (filteredResolutions != null && filteredResolutions.Count > 0)
        {
            // Szukamy specyficznie 1920x1080 jako domyœlnej
            int defaultResIndex = filteredResolutions.Count - 1; // Fallback na najwy¿sz¹

            for (int i = 0; i < filteredResolutions.Count; i++)
            {
                if (filteredResolutions[i].width == 1920 && filteredResolutions[i].height == 1080)
                {
                    defaultResIndex = i;
                    break;
                }
            }

            SetResolution(defaultResIndex);

            if (resolutionDropdown != null)
            {
                resolutionDropdown.value = defaultResIndex;
                resolutionDropdown.RefreshShownValue();
            }
        }

        PlayerPrefs.Save();
        Debug.Log("Ustawienia przywrócone do domyœlnych (1920x1080)!");
    }

    private void LoadSettings()
    {
        if (PlayerPrefs.HasKey("VolumePreference"))
        {
            float savedVolume = PlayerPrefs.GetFloat("VolumePreference");
            AudioListener.volume = savedVolume;
            if (volumeSlider != null) volumeSlider.value = savedVolume;
        }
        else if (volumeSlider != null)
        {
            volumeSlider.value = 1f;
        }

        if (PlayerPrefs.HasKey("FullscreenPreference"))
        {
            bool isFullscreen = PlayerPrefs.GetInt("FullscreenPreference") == 1;
            Screen.fullScreen = isFullscreen;
            if (fullscreenToggle != null) fullscreenToggle.isOn = isFullscreen;
        }
        else if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
        }
    }
}