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

    // navigator.clipboard.readText() needs a "clipboard-read" permission that
    // itch.io's iframe never grants, with no legacy fallback (unlike copy). So
    // instead of asking the Clipboard API for permission, this focuses a hidden
    // native input and waits for the browser's own paste event (fired when the
    // user presses Ctrl+V or right-click-pastes) -- that's driven by the user's
    // OS-level paste action, not a JS clipboard read, so it isn't gated by the
    // Permissions Policy. Result comes back via SendMessage since it's async;
    // targetObjectNamePtr must be an active GameObject with OnClipboardPasted(string)
    // and OnClipboardPasteFailed(string) methods.
    FocusPasteCatcher: function (targetObjectNamePtr) {
        var targetObjectName = UTF8ToString(targetObjectNamePtr);

        var catcher = document.getElementById("clipboardPasteCatcher");
        if (!catcher) {
            catcher = document.createElement("input");
            catcher.id = "clipboardPasteCatcher";
            catcher.type = "text";
            catcher.style.position = "fixed";
            catcher.style.opacity = "0";
            catcher.style.top = "0";
            catcher.style.left = "0";
            document.body.appendChild(catcher);
        }

        catcher.value = "";
        catcher.onpaste = function (event) {
            var text = (event.clipboardData || window.clipboardData).getData("text");
            event.preventDefault();
            catcher.blur();
            if (text) {
                SendMessage(targetObjectName, "OnClipboardPasted", text);
            } else {
                SendMessage(targetObjectName, "OnClipboardPasteFailed", "");
            }
        };

        catcher.focus();
    }
});
