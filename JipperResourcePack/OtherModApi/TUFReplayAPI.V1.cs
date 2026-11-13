namespace JipperResourcePack.OtherModApi;

public static partial class TUFReplayAPI {
    private sealed class ProviderV1 : Provider {
        public override bool CheckUsableAPIVersion(int version) => version == 1;

        public void OnReplayBegin(RunState recorded, bool render) {
            Recorded = recorded;
            ReplayActive = true;
            _replayTicks = 0;
            SetInputSuspended(true);
        }
    }
}
