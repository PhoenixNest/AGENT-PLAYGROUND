# Third-Party Notices

Pinned copies are produced by `tools/parity/scripts/fetch-assets.mjs` and synced into this project by `OrbitalSalvage.Editor.ParityAssetBuilder.BuildAll`. Hashes for every file are in `tools/parity/assets/manifest.json`.

| Component | License | Source | Pinned commit | License text |
| --- | --- | --- | --- | --- |
| Fredoka (static instances wght 400/600/700 of `Fredoka[wdth,wght].ttf`) | SIL OFL 1.1 | https://github.com/google/fonts/tree/9710da1eacb3be272583c3224dcb70f9da6eadbb/ofl/fredoka | `9710da1eacb3be272583c3224dcb70f9da6eadbb` | `Assets/Licenses/Fredoka-OFL.txt` |
| Nunito (static instances wght 700/800/900 of `Nunito[wght].ttf`) | SIL OFL 1.1 | https://github.com/google/fonts/tree/9710da1eacb3be272583c3224dcb70f9da6eadbb/ofl/nunito | `9710da1eacb3be272583c3224dcb70f9da6eadbb` | `Assets/Licenses/Nunito-OFL.txt` |
| Noto Sans Symbols 2 (fallback; `NotoSansSymbols2-Regular.ttf`) | SIL OFL 1.1 | https://github.com/google/fonts/tree/9710da1eacb3be272583c3224dcb70f9da6eadbb/ofl/notosanssymbols2 | `9710da1eacb3be272583c3224dcb70f9da6eadbb` | `Assets/Licenses/NotoSansSymbols2-OFL.txt` |
| Noto Sans Symbols (fallback; static instance wght 400 of `NotoSansSymbols[wght].ttf`) | SIL OFL 1.1 | https://github.com/google/fonts/tree/9710da1eacb3be272583c3224dcb70f9da6eadbb/ofl/notosanssymbols | `9710da1eacb3be272583c3224dcb70f9da6eadbb` | `Assets/Licenses/NotoSansSymbols-OFL.txt` |
| Microsoft Fluent UI Emoji (3D PNGs in `Assets/Resources/Emojis/3D`) | MIT | https://github.com/microsoft/fluentui-emoji/tree/1ffb34c752ecf5d402f04cfb4b392c77f57c54bc | `1ffb34c752ecf5d402f04cfb4b392c77f57c54bc` | `Assets/Licenses/Fluent-LICENSE.txt` |

Static font instances are derived from the variable sources with `fontTools.varLib.instancer`. Only the faces Google Fonts serves for the web prototype are shipped (Fredoka 400/600/700, Nunito 700/800/900); other weights are resolved at runtime by `OrbitalSalvage.Render.ParityFonts` using CSS font matching.
