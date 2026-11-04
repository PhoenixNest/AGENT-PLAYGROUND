# Third-party notices: AudioDsp

The files in this directory are a C# port of the Web Audio rendering code in Chromium (Blink). The algorithms,
constants, interpolation thresholds, coefficient formulas and processing order follow the Chromium sources listed below
(Chromium `main`, `third_party/blink/renderer/`). The port was checked against Chromium 153.0.8010.12 renders.

| C# file | Chromium source |
| --- | --- |
| `AudioParam.cs` | `modules/webaudio/audio_param_handler.cc` and `.h` (AudioParamTimeline: event insertion, `ValuesForFrameRange`, linear/exponential/target/value processing, `HasSampleAccurateValues`) |
| `PeriodicWave.cs` | `platform/audio/periodic_wave.cc`, `modules/webaudio/periodic_wave_handler.cc` (4096-point tables, 3 bands per octave, partial culling, normalization, table selection) |
| `OscillatorNode.cs` | `modules/webaudio/oscillator_handler.cc` (k-rate and a-rate phase increments, 2/3/5-point interpolation thresholds, start-frame offset) |
| `AudioNode.cs` | `modules/webaudio/audio_handler.cc`, `audio_scheduled_source_handler.cc` (once-per-quantum processing, silence propagation, start/stop frame scheduling) |
| `GainNode.cs` | `modules/webaudio/gain_handler.cc` |
| `BiquadFilterNode.cs` | `platform/audio/biquad.cc`, `modules/webaudio/biquad_filter_handler.cc` (coefficients, per-quantum vs per-sample updates, tail time) |
| `DynamicsCompressorNode.cs` | `platform/audio/dynamics_compressor.cc`, `modules/webaudio/dynamics_compressor_handler.cc` (pre-delay, knee curve, adaptive release, metering) |
| `AudioBufferSourceNode.cs` | `modules/webaudio/audio_buffer_source_handler.cc` (fast path, interpolated path, loop handling) |
| `AudioContextDsp.cs`, `Common.cs` | `modules/webaudio/offline_audio_destination_handler.cc`, `platform/audio/audio_bus.cc`, `audio_utilities.cc` (quantum loop, bus summing and mixing, time-to-frame rounding, dB conversions) |

Chromium's own files carry one of two BSD-style notices; both apply to the port.

## Google / Apple notice (older Blink platform/audio files such as biquad.cc, dynamics_compressor.cc, periodic_wave.cc, audio_bus.cc, audio_utilities.cc)

```text
Copyright (C) 2010, 2011, 2012 Google Inc. All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:

1.  Redistributions of source code must retain the above copyright
    notice, this list of conditions and the following disclaimer.
2.  Redistributions in binary form must reproduce the above copyright
    notice, this list of conditions and the following disclaimer in the
    documentation and/or other materials provided with the distribution.
3.  Neither the name of Apple Computer, Inc. ("Apple") nor the names of
    its contributors may be used to endorse or promote products derived
    from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY APPLE AND ITS CONTRIBUTORS "AS IS" AND ANY
EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL APPLE OR ITS CONTRIBUTORS BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
```

## The Chromium Authors notice (modules/webaudio handler files)

```text
Copyright 2015 The Chromium Authors

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are
met:

   * Redistributions of source code must retain the above copyright
notice, this list of conditions and the following disclaimer.
   * Redistributions in binary form must reproduce the above
copyright notice, this list of conditions and the following disclaimer
in the documentation and/or other materials provided with the
distribution.
   * Neither the name of Google LLC nor the names of its
contributors may be used to endorse or promote products derived from
this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
"AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
```

Math routines that Chromium takes from the platform libm (`sin`, `cos`, `log`, `expf`, `powf`, `asin`) are replaced by
`Libm.cs`, an original double-precision implementation built from Taylor series and Cody-Waite reduction. Unity's Mono returns
transcendental results that differ from .NET by up to ~1e-8 relative, which flips the compressor's `k` bisection, so the
runtime math is not used. Float intermediates in the compressor curve are rounded explicitly because Mono keeps wider
intermediates.
