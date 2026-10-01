## Notes

- Streams: 30 recorded gameplay frames (sessions s01/s11/s21/s31/s41, each with f001/f060/f180/pause/vfx1/vfx2), reference from Chromium 153 at DPR 3, replayed through the same command stream in Unity (D3D11 verified in `unity.log`).
- `pass*` = clears the 99.0 % / 0.985 SPEC bar but not the 99.5 % / 0.99 internal target. Pause and vfx frames carry the most text and emoji, whose edges are the residual (see `Assets/Scripts/Render/DESIGN.md`, Text).
- The three failing scenes are text-only stress scenes; root cause and the request to the SPEC owner are in `tools/parity/reports/spec-requests/canvas.md`.
- Performance columns: `record` = CPU time in `player.Play` (command recording and geometry), `submit` = CPU time in `EndFrame` (buffer upload and `ExecuteCommandBuffer`), `replay` = the already recorded command buffer executed repeatedly and synced (GPU-bound cost), `frame` = whole record + submit pipeline over 20 frames after 3 warm-up frames, synced at the end. Measured in the Editor on a laptop GPU; individual streams show multi-ms noise between runs (GC, Editor activity).
