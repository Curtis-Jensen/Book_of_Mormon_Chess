mergeInto(LibraryManager.library, {
  // Asks for permission the first time; safe to call repeatedly. Must run from a click
  // handler on some browsers, so the game calls it when the player taps Play Online.
  RequestBrowserNotificationPermission: function () {
    if (typeof Notification === 'undefined') return;
    if (Notification.permission === 'default') Notification.requestPermission();
  },

  ShowBrowserNotification: function (titlePtr, bodyPtr) {
    if (typeof Notification === 'undefined' || Notification.permission !== 'granted') return;
    var n = new Notification(UTF8ToString(titlePtr), { body: UTF8ToString(bodyPtr), tag: 'bom-chess-turn' });
    n.onclick = function () { window.focus(); n.close(); };
  }
});
