using System;
using System.Collections.Concurrent;
using SkyHook;
using TUFReplay.SDK;
using UnityEngine;

namespace JipperResourcePack.KeyViewerContents;

public partial class KeyViewer {
    private static volatile bool _replaying;
    private static long _replayOrigin, _nativeOffset;
    private static KeyCountData _replayCounts;
    private readonly object _inputGate = new();
    private readonly int[] _releaseIndex = new int[1];
    private ulong _replayDirtyKeys, _replayChangedKeys;
    // Queue and clock calibration belong to one native-input epoch.
    private sealed class KeyEventQueue : ConcurrentQueue<KeyEvent> {
        internal long OffsetTicks = NoEventOffset;
    }
    internal void ApplyReplayInput(int key, bool pressed, double elapsedTime) {
        if(!_replaying || !Enabled) return;
        KeyLabel label = VersionSafe.UnityKeyToSkyHookKey((KeyCode) key);
        if(label == KeyLabel.Unknown) return;
        // Recorded Unity keys have no native key code. The SDK calls this on Unity's thread.
        ProcessKeyEvent(new KeyEvent(label, null, pressed, _replayOrigin + (long) (elapsedTime * TimeSpan.TicksPerSecond)));
    }

    internal void BeginReplay() {
        if(!Enabled || _replaying) return;
        lock(_inputGate) {
            _replayOrigin = CurrentTicks - (long) (TUFReplaySession.ElapsedTime * TimeSpan.TicksPerSecond);
            ReleaseHeldKeys();
            while(_pressTimes != null && _pressTimes.TryDequeue(out _)) { }
            _replayCounts = new KeyCountData();
            Array.Copy(KeyCountData.Instance.Count, _replayCounts.Count, _replayCounts.Count.Length);
            _replayCounts.TotalCount = KeyCountData.Instance.TotalCount;
            _replaying = true;
            _eventQueue = new KeyEventQueue();
        }
    }

    internal void EndReplay() {
        if(!_replaying) return;
        lock(_inputGate) {
            ReleaseHeldKeys();
            while(_pressTimes != null && _pressTimes.TryDequeue(out _)) { }
            _nativeOffset = CurrentTicks - NativeTicks;
            _replayCounts = null;
            _eventQueue = new KeyEventQueue();
            _replaying = false;
            _replayDirtyKeys |= _replayChangedKeys;
            RefreshReplayKeys();
            _replayChangedKeys = 0;
            Updater.RefreshDisplay();
        }
    }

    internal void RefreshReplayKeys() {
        if(_replayDirtyKeys == 0) return;
        _replayChangedKeys |= _replayDirtyKeys;
        KeyCountData counts = _replaying ? _replayCounts : KeyCountData.Instance;
        for(int i = 0; i < Keys.Length; i++) {
            if((_replayDirtyKeys & (1UL << i)) == 0 || Keys[i] == null) continue;
            Keys[i].UpdateRequestKey(_keyState[i]);
            if(i < HandOutIndex) {
                int countIndex = i == 9 && Settings.KeyViewerStyle == KeyviewerStyle.Key10 ? 10 : i;
                Keys[i].Value.Text = counts.Count[countIndex].ToString();
            }
        }
        _replayDirtyKeys = 0;
    }
}
