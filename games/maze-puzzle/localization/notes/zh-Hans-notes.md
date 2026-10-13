# Simplified Chinese notes — zh-Hans (简体中文)

Mainland wording, address 你. No native speaker has reviewed this table. The 30-key sample is at `/tmp/maze-i18n/review/review-zh-Hans.md` and is not marked done.

## Coverage

| | Count |
| --- | ---: |
| spec keys | 355 |
| str keys | 81 |
| validator errors | 0 |
| visible warnings | 45 |
| suppressed fit warnings | 2 |
| accepted identical copies | 0 |

Visible warnings: `expansion` 44, `font-pending` 1. The 44 expansion warnings are copies of English that the brief says to keep (kg lines, logo lines, emoji, and strings with no letters). Each is 1.0× English, and the zh-Hans warn line is 0.88×, so a copy always warns. They cannot be shortened. The font warning is `font chain pending: ZCOOLKuaiLe-Regular.ttf`. The glyph check did not run.

Suppressed fit warnings: `LVL 11` and `LVL 15`. `zh-Hans-fit-exceptions.json` lists `LVL 1`–`LVL 50` because the glossary word is 关卡. Only those two chips are wider than the captured box today (33px vs 31px and 33px). No `identical-ok` file: every copied string is already allowed (unit, brand, or no letters).

## Doubtful lines

| English | Choice | Alternative |
| --- | --- | --- |
| `Silver Shift • Sector Hazard` | 银班次•扇区难关 | 银班次•扇区险情 (险情 is scarier) |
| `Cyber Orbit 1` | 电子轨道1 | 赛博轨道1 (trendy) |
| `Lost In Void` | 掉进虚空 | 迷失虚空 |
| `DONUT FLOAT` | 甜甜圈浮圈 | 甜甜圈漂浮 |
| `Party Popper Hole` | 派对拉炮洞 | 派对礼花洞 |
| `First Nom Nom!` | 首次啊呜！ | 首次啊呜啊呜！ (about 2px over the 96px title) |
| `NIBBLES IS FULL! YAY!` | Nibbles吃饱啦！耶！ | Nibbles吃饱啦！太好了！ (over the 217px title) |
| `PLAY MISSION` | 开始清理 | 开始任务 |
| `1,000 items recycled! You are the ultimate cosmic cleaner!` | 你是最棒的宇宙清洁工！ | 你是最棒的宇宙清洁高手！ |
| `SHIFT INCOMPLETE!` | 班次还没完成！ | 班次还差一点点！ |
| `Ratchet clicks, pops, bursts` | 啪啪声、爆开声和砰砰声 | 咔嗒声 (咔 and 嗒 are missing from the level-1 font file) |

`Nibbles 帽子` keeps the glossary space. A space also sits before `+` in `领取 +100`, because a Chinese character counts as a letter and would hide the `+` from the unit check.

Parentheses stay ASCII so the fragment counts match (`解锁(500`, `奖杯：每日值日星(+500`). Full-width `“”` are not in `zcool-sc-l1.ttf`, so this table does not use them.

`目标：100 kg` is about 4px wider than `Goal: 100 kg` because `：` is full-width. It is still inside the 65px box.

## Glossary

Checked terms are in the lines that contain them. No glossary warnings.

Not used as written, and why:

| Item | What I did |
| --- | --- |
| 啊呜啊呜 | Used in the long black-hole line. The `First Nom Nom!` title cannot hold it. |
| 漩涡 | Wrote 旋涡. 漩 is GB2312 but missing from `zcool-sc-l1.ttf`. |
| 失败 | The fail banner does not say defeat. It says 班次还没完成. |
| 首页 | Home is 主页, the glossary word. 首页 was not needed. |

Same gift, two glossary words, both kept: allowance is 零用钱, stipend is 每日津贴.

Wanted additions, already applied here: recycling coins and gameplay coins are 金币; skins and costumes are both 装扮; cosmic cleaner is 宇宙清洁工; Backup Pod and Auxiliary Scrap Pod are both 备用舱.

## English that looks wrong

- `Pure Gesture Mobile Web Puzzle` calls this Unity game a web puzzle.
- The privacy body says browser `localStorage`, cookies, and tracking SDKs for a game that also says it is offline.
- Three names for the coins: gold coins, recycling coins, gameplay coins.
- Two names for one pod: Backup Pod and Auxiliary Scrap Pod.
- `skins` in the reset line, `costumes` in the shop.
- `big anime eyes`.
- `14h 59m`.
- `Unlock (1200` and `Trophy: Daily Duty Star (+500` are cut on purpose. The next run continues them.

## Glyphs

Checked against `/tmp/maze-i18n/fonts/out/zcool-sc-l1.ttf` (3,755 han, level 1). Two han are GB2312 and not in that file. The full ZCOOL KuaiLe face should have them; the subset needs to include them:

| Character | Where | Why it stays |
| --- | --- | --- |
| 莓 U+8393 | 草莓, 莓果旋涡 | The candy word. No ordinary substitute. |
| 浏 U+6D4F | 浏览器localStorage | Mainland word for browser. |

`←` is in the Python GB2312 codec and not in this font file. `▶` and `✓` are not in the file and not in that codec. The brief says to keep all three. `•` U+2022 is in the font file and not in GB2312. GB2312 A1A4 decodes as kana `・` U+30FB, which the validator rejects, so the separator stays `•`. Emoji are not in this font file. They stay because the brief says to keep them.
