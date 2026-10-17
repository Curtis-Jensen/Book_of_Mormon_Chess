using UnityEngine;

// Small bits of hub-level state: the "Save Account" button only makes sense for guests.
public class PlayOnlineHub : MonoBehaviour
{
    public GameObject saveAccountButton;

    void OnEnable()
    {
        if (saveAccountButton != null)
            saveAccountButton.SetActive(CorrespondenceGameRepository.Instance != null && CorrespondenceGameRepository.Instance.IsGuest);
    }
}
