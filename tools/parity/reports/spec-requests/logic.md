# SPEC requests from the LOGIC track (SPEC v1.2)

None of these block the port. They are gaps in SPEC §4 that other tracks may trip over.

## 1. Chromium 153 is the numeric ground truth, not Node

SPEC §4 does not say which V8 embedding produces the goldens. They differ.

- Chromium 153 `Math.sin/cos/atan/atan2` are correctly rounded (LLVM libc). Node 24 uses fdlibm.
- Physics goldens generated under Node diverge from Chromium's by 1 ulp at about frame 17 (`debris[11].vy`, 1.4e-14). Collision chaos then changes RNG call counts by frame ~265.
- Request: state in §4 that goldens come from Chromium (Playwright) and that Node output is not comparable beyond ~1e-4.

## 2. `Math.pow` and `Math.hypot` are platform-dependent

- `Math.pow` goes through the platform `pow` plus V8 special cases. .NET `Math.Pow` matches for the one parity argument (`0.94`, `dt * 60`) but differs on about 3 in 6000 random inputs. Request: forbid new `Math.pow` uses in physics or require them to go through `JsMath.Pow` and its tests.
- `Math.hypot` needs V8's exact scaling algorithm (`JsMath.Hypot`).

## 3. IL2CPP floating-point contraction

- Mono EditMode is bit-exact. IL2CPP or any compiler that fuses `a * b + c` would change physics results and break the double-double trig.
- Request: add to §4 a rule that parity must be re-verified in a player build before sign-off, or pick a build setting that disables FMA contraction.

## 4. Mono number parsing and formatting

- Mono throws `OverflowException` for out-of-range numbers (for example `1.8E+308`), where JS returns `Infinity`. The port catches it.
- Mono's `ToString("E<n>")` and `double.Parse` are not correctly rounded. 7 of 1532 JS `String(number)` vectors differed. `JsNumber.ToString` therefore generates shortest digits itself.
- Other tracks that print or parse JS numbers through the .NET runtime (for example for JSON or UI text) need `JsNumber.ToString` and the same overflow handling.
