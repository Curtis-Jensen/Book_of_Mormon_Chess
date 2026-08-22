mergeInto(LibraryManager.library, {
    CopyToClipboard: function (textPtr) {
        var text = UTF8ToString(textPtr);

        // Legacy fallback -- not gated by the Permissions Policy that blocks the
        // async Clipboard API in embedded iframes (e.g. itch.io on Chrome).
        function legacyCopy() {
            var textarea = document.createElement("textarea");
            textarea.value = text;
            textarea.style.position = "fixed";
            textarea.style.opacity = "0";
            document.body.appendChild(textarea);
            textarea.focus();
            textarea.select();
            document.execCommand("copy");
            document.body.removeChild(textarea);
        }

        if (navigator.clipboard && navigator.clipboard.writeText) {
            try {
                // Chrome throws synchronously (rather than rejecting) when the
                // iframe's Permissions Policy disallows clipboard-write.
                navigator.clipboard.writeText(text).catch(legacyCopy);
            } catch (e) {
                legacyCopy();
            }
        } else {
            legacyCopy();
        }
    },

    // Paste is async in the browser, so the result comes back via SendMessage
    // instead of a return value -- targetObjectNamePtr must be an active
    // GameObject with an OnClipboardPasted(string) method.
    PasteFromClipboard: function (targetObjectNamePtr) {
        var targetObjectName = UTF8ToString(targetObjectNamePtr);
        // There's no legacy fallback for reading the clipboard (execCommand("paste")
        // is blocked by browsers for security reasons), so if the Permissions Policy
        // disallows this (e.g. Chrome in an itch.io iframe), we just report failure.
        if (navigator.clipboard && navigator.clipboard.readText) {
            try {
                navigator.clipboard.readText().then(function (text) {
                    SendMessage(targetObjectName, "OnClipboardPasted", text);
                }).catch(function () {
                    SendMessage(targetObjectName, "OnClipboardPasteFailed", "");
                });
            } catch (e) {
                SendMessage(targetObjectName, "OnClipboardPasteFailed", "");
            }
        } else {
            SendMessage(targetObjectName, "OnClipboardPasteFailed", "");
        }
    }
});
