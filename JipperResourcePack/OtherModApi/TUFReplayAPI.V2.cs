using JipperResourcePack.KeyViewerContents;

namespace JipperResourcePack.OtherModApi;

public static partial class TUFReplayAPI {
    private sealed class ProviderV2 : Provider {
        public override bool CheckUsableAPIVersion(int version) => version >= 2;

        public void OnReplayBegin(bool render) {
            Recorded = null;
            ReplayActive = true;
            _replayTicks = 0;
            SetInputSuspended(true);
        }

        public void OnReplayState(RunState state, long ticks) {
            Recorded = state;
            KeyViewer.Instance.SetReplayCounts(state);
        }
    }
}
