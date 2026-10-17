using System;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

// Guest accounts: a real Firebase account with no email or password, tied to this browser's
// stored refresh token. Players can attach an email/password later (LinkEmailPassword), which
// keeps the same user id -- so their games and friends carry over. Same class as the other
// repository halves, so it can use the private token fields.
public partial class CorrespondenceGameRepository
{
    public const string UsernameTakenMessage = "That username is already taken.";

    const string GuestPrefKey = "correspondenceIsGuest";
    const int MaxGuestNameAttempts = 10;

    public bool IsGuest => PlayerPrefs.GetInt(GuestPrefKey, 0) == 1;

    // Call after a real email login/sign-up so the "Save your account" prompt stops showing.
    public void ClearGuestFlag()
    {
        PlayerPrefs.SetInt(GuestPrefKey, 0);
        PlayerPrefs.Save();
    }

    // Creates the anonymous account, then claims a random unique Book of Mormon name for it.
    public void SignUpAsGuest(Action onReady, Action<string> onError) =>
        StartCoroutine(SignUpAsGuestRoutine(onReady, onError));

    IEnumerator SignUpAsGuestRoutine(Action onReady, Action<string> onError)
    {
        var url = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={webApiKey}";
        var body = new JObject { ["returnSecureToken"] = true }.ToString(Formatting.None);

        using (var request = BuildRequest(url, "POST", body, withAuth: false))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Guest sign-up failed ({request.responseCode}): {request.downloadHandler.text}");
                onError?.Invoke(request.downloadHandler.text.Contains("OPERATION_NOT_ALLOWED")
                    ? "Guest play isn't turned on yet -- enable Anonymous sign-in in the Firebase console."
                    : "Couldn't create a guest account -- try again.");
                yield break;
            }

            var response = JObject.Parse(request.downloadHandler.text);
            idToken = response["idToken"]?.ToString();
            localUserId = response["localId"]?.ToString();
            StoreRefreshToken(response["refreshToken"]?.ToString());
            PlayerPrefs.SetInt(GuestPrefKey, 1);
            PlayerPrefs.Save();
        }

        // Names are unique, so a taken one just means roll again.
        for (int attempt = 0; attempt < MaxGuestNameAttempts; attempt++)
        {
            bool done = false;
            string error = null;
            ClaimUsername(GuestNames.Generate(), onSuccess: () => done = true, onError: message => { error = message; done = true; });
            yield return new WaitUntil(() => done);

            if (error == null) { onReady?.Invoke(); yield break; }
            if (error != UsernameTakenMessage) { onError?.Invoke(error); yield break; }
        }

        onError?.Invoke("Couldn't find a free name -- try again.");
    }

    // Upgrades the current (guest) account in place by attaching an email and password.
    public void LinkEmailPassword(string email, string password, Action onSuccess, Action<string> onError) =>
        StartCoroutine(LinkEmailPasswordRoutine(email, password, onSuccess, onError));

    IEnumerator LinkEmailPasswordRoutine(string email, string password, Action onSuccess, Action<string> onError)
    {
        var url = $"https://identitytoolkit.googleapis.com/v1/accounts:update?key={webApiKey}";
        var body = new JObject
        {
            ["idToken"] = idToken,
            ["email"] = email,
            ["password"] = password,
            ["returnSecureToken"] = true
        }.ToString(Formatting.None);

        using var request = BuildRequest(url, "POST", body, withAuth: false);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Link email failed ({request.responseCode}): {request.downloadHandler.text}");
            onError?.Invoke(request.downloadHandler.text.Contains("EMAIL_EXISTS")
                ? "That email already has an account."
                : FriendlyAuthError(request.downloadHandler.text));
            yield break;
        }

        var response = JObject.Parse(request.downloadHandler.text);
        idToken = response["idToken"]?.ToString();
        StoreRefreshToken(response["refreshToken"]?.ToString());
        ClearGuestFlag();
        onSuccess?.Invoke();
    }
}
