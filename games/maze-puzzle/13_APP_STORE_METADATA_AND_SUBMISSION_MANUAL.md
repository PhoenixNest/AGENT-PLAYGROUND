# App Store Metadata & Submission Manual

**Everything you need to put the game on the App Store, written for someone who is not an iOS developer**

- **Document Target:** Product owner / publisher (you), and any agent helping with the release
- **Subject:** App Store listing text, privacy answers, age rating, and a click-by-click submission guide
- **Written:** 2026-10-13. Apple changes its forms and limits from time to time. Every item marked **(check Apple)** is from memory and must be checked on Apple's own page when you reach it.
- **Companion Documents:**
  - [`02_APP_STORE_SIMILARITY_AND_GUIDELINE_4_3.md`](./02_APP_STORE_SIMILARITY_AND_GUIDELINE_4_3.md) — why Apple might call the game a copy (Guideline 4.3) and how we answer
  - [`03_GAME_DESIGN_DOCUMENT.md`](./03_GAME_DESIGN_DOCUMENT.md) — what the game is
  - [`08_UNITY_MIGRATION_AND_IOS_DEPLOYMENT_PLAN.md`](./08_UNITY_MIGRATION_AND_IOS_DEPLOYMENT_PLAN.md) — how the Unity build becomes an iOS build
  - [`12_LOCALIZATION_I18N_SYSTEM.md`](./12_LOCALIZATION_I18N_SYSTEM.md) — the 17 in-game languages

---

## 0. How to Use This Document

1. Read **section 1** (what is ready, what is missing). It is short.
2. Make the **decisions in section 2**. Nothing can be submitted until they are made.
3. Copy the text from **section 3** into App Store Connect while following the steps in **section 8**.
4. Use **section 9** as the last check before you press "Submit for Review".

**Words you will meet** (full list in section 11):

- **App Store Connect (ASC):** Apple's website where you manage your app: <https://appstoreconnect.apple.com>.
- **Bundle ID:** the app's permanent internal name, like `com.yourname.orbitalsalvage`. It cannot be changed after the app exists in ASC.
- **TestFlight:** Apple's free tool to test builds on real phones before release.
- **Xcode:** Apple's Mac app that turns the Unity export into something Apple accepts.

---

## 1. Status: What Is Ready and What Is Not

Facts below were checked in the repository on 2026-10-13.

| Item                         | Status          | Detail                                                                                                                                                                                                    |
| :--------------------------- | :-------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| App icon                     | Ready           | `Assets/Icons/AppIcon.png` is 1024 × 1024, no transparency. This is the size Apple wants. Unity makes the smaller sizes from it.                                                                          |
| Devices and orientation      | Ready           | iPhone only, portrait only, minimum iOS 15.0.                                                                                                                                                             |
| Version                      | Ready (basic)   | Unity `bundleVersion` is `1.0`. The build number field is empty. Set it to `1` (section 8, steps 7 and 13).                                                                                               |
| In-game languages            | Ready           | 17 languages. All translations are agent drafts with no native review (see doc 12).                                                                                                                       |
| Money, ads, tracking         | Ready           | No ads, no in-app purchases, no accounts, no analytics code in the game's own scripts. The Toy Shop uses in-game gold coins only.                                                                         |
| App name                     | **Not decided** | The in-game logo says "Orbital Salvage / Space Trash Clean". The full name is 34 characters; Apple allows 30. See 2.1.                                                                                    |
| Bundle ID                    | **Not decided** | Unity says `io.dev.maze`. Your test build uses `com.curiomatch.test`. Neither looks like a final name. See 2.2.                                                                                           |
| Home-screen name             | **Wrong**       | Unity `productName` is still `maze-puzzle`. That is the name shown under the icon on the phone. Change it before the real build.                                                                          |
| Apple signing team           | **Missing**     | `appleDeveloperTeamID` in the Unity project is empty. The Xcode export is signed by hand today.                                                                                                           |
| Privacy Policy web page      | **Missing**     | Both legal rows in the game open `https://www.bing.com` as a placeholder. Apple requires a real, working privacy policy URL. See section 5.                                                               |
| Support web page             | **Missing**     | Apple requires a support URL. See 2.4.                                                                                                                                                                    |
| Screenshots                  | **Missing**     | None have been made in Apple's required sizes. See section 7.                                                                                                                                             |
| Encryption answer in the app | **Missing**     | The Info.plist key `ITSAppUsesNonExemptEncryption` is not set. Without it, ASC asks the question again on every build. See step 14.                                                                       |
| Kids Category parental gate  | **Open**        | The links out of the app have no "ask a parent" gate. Only needed if you choose the Kids Category. See section 6.                                                                                         |
| Credits / licence notices    | **Unchecked**   | The game uses Microsoft Fluent 3D Emoji (MIT licence) and open fonts. The MIT licence asks that its notice travels with the app. I did not find a credits screen. See section 10.                         |
| World 3–5 special mechanics  | **Open**        | Doc 10 (2026-09-30) says some mechanics of sectors 3–5 are not built at runtime. All 50 levels exist. Check before you promise anything about them in the listing. The text below avoids those mechanics. |
| Device testing               | Partial         | Tested on one iPhone X (iOS 16.7.10). Settings, Shop and Daily screens have not been opened on a phone in the new languages.                                                                              |

---

## 2. Decisions Only You Can Make

| #   | Decision                               | My recommendation                                                                                                         |
| :-- | :------------------------------------- | :------------------------------------------------------------------------------------------------------------------------ |
| 2.1 | App name and subtitle                  | Name **Orbital Salvage**, subtitle **Space Trash Clean-Up Puzzle** (details below).                                       |
| 2.2 | Final bundle ID                        | A reverse web address that you own, for example `com.yourcompany.orbitalsalvage`. Decide before step 5. It cannot change. |
| 2.3 | Account type and seller name           | Organisation account if the seller name should be a company. Individual if it can be your own name. See step 1.           |
| 2.4 | Web pages (privacy, support, optional) | One simple public page with a contact email. See 2.4.                                                                     |
| 2.5 | Kids Category: yes or no               | Start **without** the Kids Category, rating 4+. See section 6 for why, and for what changes if you say yes.               |
| 2.6 | Countries                              | All countries except mainland China for version 1.0. See step 9.                                                          |
| 2.7 | Price                                  | Free. This matches the "no ads, no purchases" promise.                                                                    |
| 2.8 | How many store languages at launch     | English only for the first submission. Add translated listings later (section 4).                                         |

### 2.1 Name and subtitle

Apple limits: name **30 characters**, subtitle **30 characters**. Spaces and punctuation count.

| Option | Name                               | Characters | Subtitle                    | Characters | Note                                    |
| :----- | :--------------------------------- | :--------- | :-------------------------- | :--------- | :-------------------------------------- |
| A      | Orbital Salvage                    | 15         | Space Trash Clean-Up Puzzle | 27         | Recommended. Matches the in-game logo.  |
| B      | Orbital Salvage: Trash Clean       | 28         | Feed Nibbles the Black Hole | 27         | Uses more of the name field for search. |
| C      | Orbital Salvage: Space Trash Clean | 34         | —                           | —          | **Too long.** Apple will refuse it.     |

Two more rules you should know:

- The name must not already be taken by another app. **I cannot check this.** ASC tells you when you create the record. Also search the App Store and the trademark registers of your main countries yourself.
- The older working title in `README.md` ("Orbital Salvage: Event Horizon") is not used in the game. Do not use it.

### 2.4 Web pages

Apple needs these links, and reviewers do open them:

| Page               | Required?    | What it must contain                                                                               |
| :----------------- | :----------- | :------------------------------------------------------------------------------------------------- |
| Privacy Policy URL | **Yes**      | The policy from section 5, on a public page that loads without a login.                            |
| Support URL        | **Yes**      | A page with a way to contact you (email is enough).                                                |
| Marketing URL      | No           | Optional. Leave empty.                                                                             |
| User Agreement URL | No (for ASC) | The game has a second legal row. Either publish a short terms page or remove the row (section 10). |

A free page on GitHub Pages, Notion (public), or any small web host works. The same privacy URL should go in ASC and in the game's "Privacy Policy" row.

---

## 3. Paste-Ready Listing Text (English)

Language to create first in ASC: **English (U.S.)**. This is the primary language.

Character counts were measured on 2026-10-13.

### 3.1 Name and subtitle

```text
Orbital Salvage
```

```text
Space Trash Clean-Up Puzzle
```

### 3.2 Promotional text (limit 170 characters, can be changed any time without a new build)

```text
Spin the rings, drop the space junk and feed Nibbles, the hungry little black hole! A cosy physics puzzle with no ads and no in-app purchases.
```

### 3.3 Description (limit 4000 characters)

```text
Something big and messy is floating around the planet, and only you can clean it up!

Meet Nibbles, a very hungry and very cute little black hole. Nibbles loves space junk! Spin the orbital rings with your finger and let old brackets, solar cells and batteries tumble through the shredders and straight into Nibbles' mouth.

HOW TO PLAY
• Turn the ring to guide the junk where you want it to go
• Roll through multiplier gates to turn a little scrap into a lot
• Feed Nibbles enough kilograms to clear the sector and earn gold coins

WHAT YOU GET
• 50 levels across 5 sectors, from easy sweeps to tricky orbits
• Infinite mode: finish all 50 levels and the cleanup never ends
• A Daily Shift every day with Bronze, Silver and Gold goals
• A free daily coin gift
• The Toy Shop: spend the gold coins you earn on Donut Rings, Nibbles Hats and new Skies
• Trophies to collect as you get better
• Soft sound effects and gentle vibration, both can be switched off
• Plays in 17 languages: English, Chinese (Simplified and Traditional), Japanese, Korean, German, French, Spanish, Italian, Portuguese, Vietnamese, Filipino, Indonesian, Uzbek, Khmer, Hindi and Arabic
• Works offline, no Wi-Fi needed

NO ADS. NO IN-APP PURCHASES. NO ACCOUNTS.
Everything in the Toy Shop is paid for with coins you earn by playing. Your progress stays on your phone. We do not collect personal information.

A quick break, a cosy evening, or a game to share with the family. Come and help Nibbles tidy up the sky!
```

Check before you paste: the line "50 levels across 5 sectors" is true as far as the data goes, but see the world 3–5 note in section 1. The "Infinite mode" line is true in the build as of 2026-10-13 (see `03_GAME_DESIGN_DOCUMENT.md` section 3.9). It replays levels 30 to 50 at random, so do not call them new levels. Nobody has proven that every one of those levels can be beaten.

### 3.4 Keywords (limit 100 characters, commas, no spaces after commas)

```text
black hole,kids,family,physics,relaxing,cute,offline,satellite,recycle,cleaning,casual,rotate
```

Rules for keywords:

- Do not repeat words that are already in the name or subtitle ("space", "trash", "puzzle", "orbital", "salvage"). Apple already indexes them.
- Do not use other apps' names or trademarks. This is a common reason for rejection.
- Single words work best. Apple combines them for you.
- "kids" and "family" are fine as search words. They do not by themselves put the app in the Kids Category.

### 3.5 What's New in This Version (limit 4000, shown for version 1.0)

```text
Welcome aboard! Nibbles is hungry and the sky needs cleaning. This is the first release.
```

### 3.6 Other fields

| ASC field                 | Value                                                                                                                                                           |
| :------------------------ | :-------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Primary language          | English (U.S.)                                                                                                                                                  |
| Primary category          | **Games**                                                                                                                                                       |
| Games subcategory 1 and 2 | **Puzzle** and **Casual**                                                                                                                                       |
| Secondary category        | Leave empty, or choose **Family** if ASC offers it for games **(check Apple)**                                                                                  |
| Copyright                 | `2026 <your company or your name>`. The text is yours to fill. The code uses `GalacticSanitation` as the Unity company name, which is not a public seller name. |
| Version number            | `1.0`                                                                                                                                                           |
| SKU                       | Any internal text only you see, for example `orbital-salvage-001`                                                                                               |
| Price                     | Free                                                                                                                                                            |
| Sign-in required          | No. There are no accounts. Do not enter a demo account.                                                                                                         |
| Game Center               | Off. The game does not use it.                                                                                                                                  |
| Content rights            | See below.                                                                                                                                                      |

**Content rights question** ("Does your app contain, show, or access third-party content?"): the game does not show other people's songs, videos or user content. It uses licensed fonts and emoji artwork whose licences allow shipping in an app. Answer **No**, and keep the licence notices (section 10) so that answer stays true.

### 3.7 Notes for the App Review team (not public)

Paste into "App Review Information > Notes":

```text
Orbital Salvage is an original single-player physics puzzle game about feeding a cartoon black hole called Nibbles with space junk.

How to test: the game needs no account and no internet. Open the game, tap Play, and turn the orbital ring with one finger to guide the junk into Nibbles. The Toy Shop, Daily Shift, Trophies and Settings are reachable from the home screen.

What makes it different from other ball-drop games: an original sci-fi story and characters, a rotating ring-and-gate puzzle with gravity-based physics, a daily mission system, a cosmetic shop paid for only with in-game coins earned by playing, and 17 in-game languages. The characters, story, sounds and level layouts are original to this game. The emoji artwork is Microsoft's MIT-licensed Fluent set and the fonts are open-licensed.

The game contains no advertising, no in-app purchases, no subscriptions, no third-party analytics and no tracking. Progress is stored only on the device.

The two links on the Settings screen (Privacy Policy and User Agreement) open in the system browser.

Contact: <your name>, <your phone>, <your email>
```

If any of those sentences is no longer true when you submit, change it. Apple checks.

---

## 4. Store Listings in Other Languages

The game itself speaks 17 languages. The **store page** is separate: each language needs its own name, subtitle, description, keywords and screenshots in ASC. English alone is enough to launch.

| Game language                    | Likely offered in ASC? | Note                                                                                                                        |
| :------------------------------- | :--------------------- | :-------------------------------------------------------------------------------------------------------------------------- |
| English                          | Yes                    | Primary.                                                                                                                    |
| Chinese (Simplified)             | Yes                    | China needs extra papers. See step 9.                                                                                       |
| Chinese (Traditional)            | Yes                    |                                                                                                                             |
| Japanese, Korean                 | Yes                    |                                                                                                                             |
| German, French, Spanish, Italian | Yes                    |                                                                                                                             |
| Portuguese                       | Yes                    | ASC separates Portugal and Brazil. The game's table is Portugal-style.                                                      |
| Vietnamese, Indonesian           | Yes                    |                                                                                                                             |
| Hindi, Arabic                    | Yes                    |                                                                                                                             |
| Filipino, Uzbek, Khmer           | **Probably not**       | I believe ASC has no listing for these **(check Apple)**. The game still speaks them; the store page falls back to English. |

I have **not** written translated store text. The reasons are honest ones: the in-game translations are already unreviewed drafts, store text is what customers read first, and every field has a character limit that must be met in that language. Recommended path: launch in English, get a native-speaker review of the in-game language you care about most, then write that store listing from the reviewed words. Ask me when you want it and name the languages.

---

## 5. Privacy

### 5.1 App Privacy answers ("nutrition label")

In ASC > **App Privacy**, answer **"No, we do not collect data from this app"**. The page then shows "Data Not Collected".

Why this should be true, and what was and was not verified:

| Claim                                                     | Verified?                                                                                                                                                                                                                               |
| :-------------------------------------------------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| The game's own scripts make no network calls              | **Yes.** A search of `Assets/Scripts` found no web request, ad, analytics or tracking code.                                                                                                                                             |
| Progress, coins, settings and language stay on the device | **Yes** by design (local settings). Nothing uploads them.                                                                                                                                                                               |
| Links open the system browser                             | **Yes.** They use `Application.OpenURL`.                                                                                                                                                                                                |
| The Unity engine adds no data collection                  | **Not verified.** The project lists Unity's built-in "Unity Analytics" and "UnityWebRequest" modules. They are Unity's own code and are not used by the game, but I have not inspected a built app's network traffic or privacy report. |

**Before you answer, do this check:** after step 15 (archive), in Xcode open the Organizer, right-click the archive, choose **Generate Privacy Report**. Read it. If it lists data collection or tracking domains, answer honestly in ASC instead. Apple compares your answers to the build.

Other privacy questions in ASC:

| Question                                            | Answer                                                                                           |
| :-------------------------------------------------- | :----------------------------------------------------------------------------------------------- |
| Does the app use the Advertising Identifier (IDFA)? | **No**                                                                                           |
| App Tracking Transparency prompt                    | Not needed. Do not add one.                                                                      |
| Does the app use encryption?                        | Answer **No** to non-exempt encryption. It has no network features. Set the plist key (step 14). |

### 5.2 Privacy Policy draft

This is a **draft for you to publish on your own page**. It is not legal advice. Have it checked, fill in the brackets, and re-read it against the final build.

```text
PRIVACY POLICY — Orbital Salvage
Effective date: [date]

Who we are
Orbital Salvage is published by [company or name]. You can reach us at [email address].

What we collect
Nothing. The game does not ask for your name, email, age, location or contacts. It has no accounts. It has no advertising, no analytics and no tracking. It does not use the advertising identifier.

What stays on your device
Your progress (levels, coins, trophies, shop items) and your settings (sound, vibration, language) are stored only on your own iPhone. We never receive them. If you delete the game, this information is deleted with it.

Children
The game is made to be safe for children. Because we collect no personal information, there is none to share, sell or delete.

Links
The Settings screen has links that open your web browser. Other websites have their own privacy policies, which we do not control.

Apple
Apple may provide us with anonymous statistics (for example crash counts) only if you have chosen to share them with app developers in your iPhone settings. They contain no personal information.

Changes
If this policy changes, the new version will be posted at this address with a new effective date.

Contact
[email address]
```

Known problem inside the app: the game's built-in privacy dialog text (code is kept, not shown in the normal flow) talks about a "web puzzle" and browser `localStorage`. That is wrong for a native iPhone app. It is not reachable today. If you ever turn that dialog back on, fix the wording first.

---

## 6. Age Rating and the Kids Category

### 6.1 Age rating

ASC > **App Information > Age Rating > Edit**. Apple shows a questionnaire. Expected answers for this game (read each question; Apple updates the list, **check Apple**):

| Topic                                       | Answer for this game                           |
| :------------------------------------------ | :--------------------------------------------- |
| Violence (cartoon, realistic, weapons)      | None. Junk is shredded; nobody is hurt.        |
| Sexual content, nudity                      | None                                           |
| Profanity, crude humour                     | None                                           |
| Horror, fear themes                         | None                                           |
| Alcohol, tobacco, drugs                     | None                                           |
| Medical or health information               | None                                           |
| Gambling, simulated gambling, contests      | None. No loot boxes, no wheels for real value. |
| Unrestricted web access                     | No. Two fixed links open the browser.          |
| User-generated content, chat, messaging     | None                                           |
| Parental controls or age assurance features | None                                           |

Expected result: **4+**. Apple calculates the final rating from your answers.

### 6.2 Kids Category: choose

The Kids Category is a special shelf in the App Store with stricter rules. It is a choice, not a rating.

| If you choose                 | What it means                                                                                                                                                                                            |
| :---------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **No (recommended for v1.0)** | Normal Games listing, rating 4+. Fewer rules. The game is still safe and child-friendly. Do not market it as "for kids" in a way that contradicts the privacy answers.                                   |
| Yes                           | You pick an age band (5 and under, 6–8, 9–11). Apple requires: no third-party analytics or ads; **a parental gate before any link that leaves the app**; no unclear data collection. Review is stricter. |

If you choose **Yes**, the open work is: build a parental gate (for example "hold for 3 seconds" or a simple sum) in front of the two legal rows, and replace the `https://www.bing.com` placeholders. If you choose **No**, replace the placeholders anyway. The reasons are in 9.

Note: the studio's own reference, `AGENT-GLOBAL-BASE/studio/casual-games/library/reference/coppa-platform-compliance.md`, discusses US child-privacy law (COPPA) and the EU rules. The game is cartoon-styled, so authorities could treat it as aimed at children. The safe position is the one already built: collect nothing. Keep it that way, and do not add ads or analytics later without redoing this section.

---

## 7. Images

### 7.1 App icon

Done: 1024 × 1024, opaque. Check the real icon in the Xcode build looks right, with no rounded corners drawn by you (Apple rounds them).

### 7.2 Screenshots (required)

| Rule              | Detail                                                                                                                                                                                                                              |
| :---------------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| How many          | 1 to 10 per language. Use 5 or 6.                                                                                                                                                                                                   |
| Which device size | The newest iPhone size ASC asks for. Today that is the **6.9-inch** size: **1320 × 2868** pixels, portrait. **(check Apple)** Smaller sizes are made from it. If ASC accepts the 6.5-inch set, **1284 × 2778**, that works as well. |
| iPad screenshots  | Not needed. The game is iPhone only.                                                                                                                                                                                                |
| Format            | PNG or JPEG, no transparency, no frames or fake phone shells needed.                                                                                                                                                                |
| Content           | Must show the real game. Apple rejects screenshots that show something the app does not do.                                                                                                                                         |

**Do not** use screenshots from the old iPhone X directly (1125 × 2436). That size is too small for the current main requirement.

Suggested set, in this order:

1. Home screen with Nibbles (the first impression)
2. A level in play with the ring and the junk
3. Nibbles full and happy ("NIBBLES IS FULL!")
4. Daily Shift screen
5. Toy Shop
6. Trophies

How to make them without a developer account: run the **Simulator** build (`OrbitalSalvage.Editor.IosExport.ExportSimulator`, see doc 08), open it in Xcode on an **iPhone Pro Max** simulator, and press **Command + S** to save a screenshot to the Desktop. Check the pixel size with the Finder's Get Info. Screenshots in other languages are optional; the store page uses one set for every language you do not replace.

I have **not** made these screenshots yet.

### 7.3 App preview video

Optional. Skip for version 1.0.

---

## 8. Step-by-Step: From Unity to "Waiting for Review"

You need a Mac with Xcode and an iPhone. The steps are in order. Do not skip ahead; some choices (name, bundle ID) cannot be undone.

### Part A — Accounts

1. **Join the Apple Developer Program.** <https://developer.apple.com/programs/enroll/>. It costs about US$99 per year **(check Apple)**. A free Apple ID can run test apps on your own phone, but **cannot** publish. Choose Individual (your name is shown as the seller) or Organisation (company name; needs a D-U-N-S number and takes days). Approval can take from minutes to a few days.
2. **Accept the agreements.** In ASC open **Business** (or "Agreements, Tax, and Banking"). Accept the **Free Apps** agreement. A free app does not need bank details.
3. **EU trader status.** If the app is available in the EU, ASC asks whether you are a "trader". It then shows your address, phone and email publicly. Choose honestly. **(check Apple)**

### Part B — Identity of the app

4. **Decide the final name and bundle ID** (sections 2.1 and 2.2).
5. **Register the bundle ID.** <https://developer.apple.com/account> > **Certificates, IDs & Profiles** > **Identifiers** > **+** > **App IDs** > **App**. Description: "Orbital Salvage". Bundle ID: **Explicit**, type your choice. Do not tick extra capabilities. The game needs none.
6. **Create the app record in ASC.** My Apps > **+** > **New App**. Platform **iOS**. Name (section 3.1). Primary language **English (U.S.)**. Bundle ID: pick the one from step 5. SKU: any text. User access: Full Access.

### Part C — Build the game

7. **In Unity:** Project Settings > Player > iOS.
   - **Product Name:** the final name (what the player sees under the icon). It is `maze-puzzle` now, which is wrong.
   - **Company Name:** your company or name.
   - **Bundle Identifier:** the one from step 5. (The test export used `com.curiomatch.test` by editing the exported Xcode project, not Unity. For the real build, set it in Unity so it is no longer a manual edit. This changes the Unity bundle id from `io.dev.maze`; do it only after you have decided.)
   - **Version:** `1.0`. **Build:** `1`.
   - **Signing Team ID:** your 10-character team ID from <https://developer.apple.com/account> > **Membership details**.
8. **Export the iOS project:** use the editor entry `OrbitalSalvage.Editor.IosExport.ExportDevice` (doc 08). It writes `games/maze-puzzle/output/maze-puzzle`.
9. **Open it in Xcode:** double-click `Unity-iPhone.xcodeproj`.

### Part D — Make a build Apple accepts

10. **Signing.** Xcode > select the project > target **Unity-iPhone** > **Signing & Capabilities**. Tick **Automatically manage signing**. Pick your **Team**. Check that the bundle identifier is the one from step 5.
11. **Run once on your phone.** Plug in the phone, unlock it, pick it as the target (top bar), press **Run**. Play the game. Check Settings, Shop and Daily Shift with your own eyes.
12. **Set the destination to "Any iOS Device (arm64)".** Top bar.
13. **Version and build numbers.** Xcode > target > **General**: Version `1.0`, Build `1`. Each upload needs a **new build number**. Add 1 each time.
14. **Encryption answer.** Open **Info.plist** in Xcode and add the key **App Uses Non-Exempt Encryption** (`ITSAppUsesNonExemptEncryption`) with the value **NO**. For a permanent fix, ask me to add it to the export script.
15. **Archive.** Menu **Product > Archive**. Wait. The **Organizer** window opens with your archive. (Here you also do the privacy report check from 5.1.)
16. **Upload.** Organizer > select the archive > **Distribute App** > **App Store Connect** > **Upload** > keep the defaults > **Upload**. It takes a few minutes. If it fails, the message names the problem; the usual causes are in section 9.3.

### Part E — Test with TestFlight

17. In ASC > your app > **TestFlight**. After upload, the build says **Processing**, then becomes ready (10 to 60 minutes). Apple sends an email.
18. **Internal testing** (no Apple review): **App Store Connect Users** group > add yourself and teammates (up to 100 people with ASC roles). They install the **TestFlight** app and get the build.
19. **External testing** (friends and family, up to 10,000) needs a short **Beta App Review** by Apple. Optional for v1.0.
20. Test on at least one real phone: first launch, kill and re-open (progress kept?), Settings > Language, a full level, Toy Shop purchase with coins, Daily Shift, no crash.

### Part F — Fill in the listing

21. ASC > your app > **App Store** tab > left menu **iOS App > 1.0 Prepare for Submission**.
22. **Screenshots:** upload for the 6.9-inch slot (section 7.2).
23. **Promotional Text, Description, Keywords, Support URL, Marketing URL (empty), Version, Copyright:** paste from section 3.
24. **Build:** click **+** next to Build and pick the build from step 16.
25. **App Information** (left menu): Name, Subtitle, Category (Games > Puzzle and Casual), **Privacy Policy URL**, Content Rights (3.6), **Age Rating** (section 6).
26. **App Privacy** (left menu): answer as in 5.1, then **Publish**.
27. **Pricing and Availability:** Price **Free**. Availability: all countries and regions **except mainland China** unless you have the papers (a government game licence and an ICP filing). Untick **Apple silicon Mac** and **Apple Vision Pro** so the iPhone-only game is not offered there (doc 08).
28. **App Review Information:** your contact name, phone, email. **Sign-in required: off.** Paste the notes from 3.7. Ask the reviewer nothing else.
29. **Version Release:** choose **Manually release this version**. After approval you press the button yourself when you are ready.

### Part G — Submit

30. Run the checklist in **section 9.1** line by line.
31. Press **Add for Review** then **Submit for Review**. Status becomes **Waiting for Review**. Typical wait is 1 to 3 days **(check Apple)**; for a first app expect the longer end.
32. **If rejected:** do not panic. Open **Resolution Center** in ASC. Apple says which rule (for example "Guideline 4.3(a)"). Read section 9.2, fix, upload a new build (build number +1), reply in the Resolution Center, and resubmit.
33. **If approved:** the status is **Pending Developer Release**. Press **Release This Version** when you want. The app appears in the store within hours.

---

## 9. Before You Press Submit

### 9.1 Pre-flight checklist

| #   | Check                                                                                                 | Done |
| :-- | :---------------------------------------------------------------------------------------------------- | :--- |
| 1   | App name is 30 characters or fewer and not already taken                                              | ☐    |
| 2   | Home-screen name is no longer `maze-puzzle`                                                           | ☐    |
| 3   | Privacy Policy URL opens a real page. The game's two legal rows no longer open `https://www.bing.com` | ☐    |
| 4   | Support URL opens a real page with your contact                                                       | ☐    |
| 5   | Every sentence in the description and review notes is true for this build                             | ☐    |
| 6   | Screenshots really show this build                                                                    | ☐    |
| 7   | Privacy answers match the Xcode privacy report                                                        | ☐    |
| 8   | Age rating questionnaire completed                                                                    | ☐    |
| 9   | Kids Category decision made. If **yes**: parental gate built                                          | ☐    |
| 10  | Licence notices for emoji and fonts included (section 10)                                             | ☐    |
| 11  | Tested through TestFlight on a real phone, no crash                                                   | ☐    |
| 12  | Version `1.0`, a unique build number, build selected                                                  | ☐    |
| 13  | Contact details in App Review Information filled in                                                   | ☐    |
| 14  | Availability: iPhone only. Mac and Vision Pro off                                                     | ☐    |

### 9.2 Common reasons for rejection, and our answer

| Reason (Guideline)                        | Why it could happen to us                                              | What to do                                                                                                  |
| :---------------------------------------- | :--------------------------------------------------------------------- | :---------------------------------------------------------------------------------------------------------- |
| 4.3 Spam / copycat                        | The ring-and-gate mechanic is common in the genre.                     | Use the review notes in 3.7. The story, characters, shop, daily shifts and art are the answer. Read doc 02. |
| 5.1.1 Privacy policy missing or broken    | Placeholder `https://www.bing.com` links.                              | Publish the real page (2.4, 5.2).                                                                           |
| 2.1 App completeness (crash, placeholder) | Placeholder links, or an untested screen.                              | Complete step 11 and step 20.                                                                               |
| 2.3 Accurate metadata                     | Description or screenshots promise something the build does not do.    | Check 9.1 items 5 and 6. Do not mention features that are not in the build.                                 |
| 1.3 Kids Category                         | Links out of the app without a parental gate (only if you chose Kids). | Build the gate or leave the Kids Category.                                                                  |
| 5.1.2 / privacy label mismatch            | Label says "no data" but the build contains a collecting SDK.          | Do the privacy report check (5.1).                                                                          |
| 4.0 Design (minimum functionality)        | Reviewer finds too little content.                                     | Mention the 50 levels, daily shifts, shop and trophies in the notes (done).                                 |

### 9.3 Upload problems you may meet

| Message                                      | Usual cause and fix                                                                            |
| :------------------------------------------- | :--------------------------------------------------------------------------------------------- |
| "No suitable application records were found" | Bundle ID in Xcode is not the one in ASC (step 5 and 6). Make them identical.                  |
| "Invalid Signature" or provisioning errors   | Wrong Team, or automatic signing is off (step 10).                                             |
| "The bundle version must be higher"          | Build number was reused. Add 1 (step 13).                                                      |
| "Missing Info.plist value" / icon errors     | Icon must be 1024 × 1024 and opaque. It is, so look at the exact key named.                    |
| "Unsupported SDK version"                    | Xcode is too old. Apple requires a recent Xcode and iOS SDK for new uploads **(check Apple)**. |

---

## 10. Open Items

These need your decision, or real data from you, or further work. None is hidden.

| #   | Item                                                                                                                                                                                                                          | Owner                    |
| :-- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :----------------------- |
| 1   | Final app name (2.1) and bundle ID (2.2)                                                                                                                                                                                      | You                      |
| 2   | Apple Developer account (paid) and the seller name                                                                                                                                                                            | You                      |
| 3   | Real URLs: privacy, support, and (if kept) terms                                                                                                                                                                              | You                      |
| 4   | Contact name, phone, email for ASC                                                                                                                                                                                            | You                      |
| 5   | Decide: Kids Category yes or no. If yes, the parental gate must be built (agent task)                                                                                                                                         | You, then agent          |
| 6   | Replace the two `https://www.bing.com` placeholders with the real links in the game                                                                                                                                           | Agent, after 3           |
| 7   | Set Unity product name, bundle ID, build number and team; remove the manual bundle-id edit from the export flow                                                                                                               | Agent, after 1           |
| 8   | Add `ITSAppUsesNonExemptEncryption = NO` to the iOS export                                                                                                                                                                    | Agent                    |
| 9   | Make the screenshots (7.2)                                                                                                                                                                                                    | Agent (Simulator) or you |
| 10  | Add a credits / licence screen or a bundled notice file for Microsoft Fluent 3D Emoji (MIT) and the open-source fonts (SIL Open Font Licence). Check each licence text. I did not verify the exact notice duty for each file. | Agent, then you          |
| 11  | Resolve doc 10's note on sectors 3–5 mechanics, or keep the listing wording on levels general                                                                                                                                 | You decide               |
| 12  | Native-speaker review of the 16 non-English game languages, then translated store text (section 4)                                                                                                                            | You                      |
| 13  | Open Settings, Shop and Daily Shift on a phone in the new languages (never done)                                                                                                                                              | Agent + you              |
| 14  | Check ASC's current list of store languages, screenshot sizes and age-rating questions (marked **check Apple**)                                                                                                               | You                      |
| 15  | Confirm the Xcode privacy report shows no data collection (5.1)                                                                                                                                                               | You or agent, at step 13 |
| 16  | Legal review of the privacy policy and, if used, the user agreement                                                                                                                                                           | You                      |

What I could **not** verify, and why: Apple's current limits, forms and review times (I have no live access, so they are from memory); whether the app name is free (needs ASC); what a built app sends over the network (needs a real archive); and the Unity engine's own privacy behaviour (needs the privacy report).

---

## 11. Glossary

| Word                 | Meaning                                                                                                |
| :------------------- | :----------------------------------------------------------------------------------------------------- |
| Archive              | A finished build packed by Xcode, ready to upload.                                                     |
| ASC                  | App Store Connect, Apple's website for your app.                                                       |
| Beta App Review      | A quick Apple check needed only for external TestFlight testers.                                       |
| Build number         | A counter. Must go up with every upload. Players do not see it.                                        |
| Bundle ID            | The app's permanent internal name.                                                                     |
| IDFA                 | The "advertising identifier" on an iPhone. The game does not use it.                                   |
| Kids Category        | A special, stricter section of the App Store for children's apps.                                      |
| Parental gate        | A small task (for example a sum) that only an adult can do, put in front of links that leave the app.  |
| Provisioning profile | A file that links your app, your Apple account and your devices. Automatic signing creates it for you. |
| SKU                  | An internal label for your app. Only you see it.                                                       |
| Team ID              | A 10-character code that identifies your Apple developer account.                                      |
| TestFlight           | Apple's testing tool for builds before release.                                                        |
| Xcode                | Apple's Mac program for building and uploading iPhone apps.                                            |
