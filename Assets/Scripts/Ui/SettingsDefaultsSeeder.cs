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
        int dropdownCount = 0;
        int setBooleanCount = 0;

        // Search the whole scene, not just this object's children -- the Settings
        // controls live under a different root than whatever object this is attached to.
        foreach (var root in gameObject.scene.GetRootGameObjects())
        {
            foreach (var dropdownSaver in root.GetComponentsInChildren<DropdownSaver>(includeInactive: true))
            {
                dropdownSaver.SeedIfMissing();
                dropdownCount++;
            }

            foreach (var setBoolean in root.GetComponentsInChildren<SetBoolean>(includeInactive: true))
            {
                setBoolean.SeedIfMissing();
                setBooleanCount++;
            }
        }

        Debug.Log($"[SettingsDefaultsSeeder] Seeded {dropdownCount} DropdownSaver(s) and {setBooleanCount} SetBoolean(s).");
    }
}
