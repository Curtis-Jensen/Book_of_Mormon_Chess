using UnityEngine;

// Writes each Settings control's own configured value into PlayerPrefs as soon as the
// menu scene loads, even if that control's panel is currently SetActive(false) and its
// own Awake/OnEnable hasn't run yet. This makes the Settings UI the single source of
// truth for defaults -- nothing else in the codebase should hardcode a default value
// for anything a DropdownSaver or SetBoolean already manages.
//
// Attach this to an object that is active at scene load (e.g. the menu's root Canvas).
public class SettingsDefaultsSeeder : MonoBehaviour
{
    void Awake()
    {
        foreach (var dropdownSaver in GetComponentsInChildren<DropdownSaver>(includeInactive: true))
        {
            dropdownSaver.SeedIfMissing();
        }

        foreach (var setBoolean in GetComponentsInChildren<SetBoolean>(includeInactive: true))
        {
            setBoolean.SeedIfMissing();
        }
    }
}
