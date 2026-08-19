mergeInto(LibraryManager.library, {
    CopyToClipboard: function (textPtr) {
        var text = UTF8ToString(textPtr);
        if (navigator.clipboard && navigator.clipboard.writeText) {
            navigator.clipboard.writeText(text);
        } else {
            // Fallback for browsers without the async Clipboard API (e.g. non-HTTPS contexts).
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
    }
});
