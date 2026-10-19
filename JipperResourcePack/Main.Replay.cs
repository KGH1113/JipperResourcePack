using JipperResourcePack.KeyViewerContents;
using JipperResourcePack.OverlayContents;
using TUFReplay.SDK;
using UnityEngine;

namespace JipperResourcePack;

public partial class Main {
    private TUFReplayConnection _replay;
    private bool _replayExcluded;

    private void ConnectReplay() {
        Canvas hiddenKeys = null, hiddenOverlay = null;
        _replay = new TUFReplayConnection("JipperResourcePack", "JipperResourcePack");
        _replay.OnSessionStarted += context => {
            PlayCount.ClearReplayData();
            KeyViewer.Instance?.BeginReplay();
            Overlay.Instance?.StopComboAnimation();
        };
        _replay.OnPresentationChanged += SetPresentation;
        _replay.OnSessionEnded += () => {
            SetPresentation(true);
            KeyViewer.Instance?.EndReplay();
            Overlay.Instance?.StopComboAnimation();
            PlayCount.ClearReplayData();
        };
        _replay.OnKeyEvent += input => KeyViewer.Instance?.ApplyReplayInput(input.Key, input.IsPressed, input.ElapsedTime);
        _replay.OnFrameRequested += request => {
            if(KeyViewer.Instance?.Enabled == true) {
                KeyViewer.Instance.RefreshReplayKeys();
                KeyViewer.Instance.Updater.RefreshDisplay();
                if(KeyViewer.RainManager.enabled) KeyViewer.RainManager.RefreshDisplay();
            }
            Overlay.Instance?.RefreshCombo();
            _replay.CompleteFrame(request);
        };

        void SetPresentation(bool visible) {
            _replayExcluded = !visible;
            if(visible) {
                if(hiddenKeys) hiddenKeys.enabled = true;
                if(hiddenOverlay) hiddenOverlay.enabled = true;
                hiddenKeys = hiddenOverlay = null;
                return;
            }
            if(KeyViewer.Instance?.KeyViewerObject && KeyViewer.Instance.KeyViewerObject.GetComponent<Canvas>().enabled)
                (hiddenKeys = KeyViewer.Instance.KeyViewerObject.GetComponent<Canvas>()).enabled = false;
            if(Overlay.Instance?.Canvas && Overlay.Instance.Canvas.enabled)
                (hiddenOverlay = Overlay.Instance.Canvas).enabled = false;
        }
    }
}
