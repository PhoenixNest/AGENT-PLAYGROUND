# Glossary — Orbital Salvage: Space Trash Clean

Controlled vocabulary for the 16 target languages. English is the source. These recommendations are drafts by the localisation lead, not a native-speaker sign-off. Confidence is the lead's confidence in the Chinese, Japanese, Korean, and European rows. Every translated row is marked **native review recommended** for Khmer, Uzbek, Filipino, and Hindi.

The machine-readable block at the bottom is what `tools/validate_table.py` reads. A `check: yes` term produces a **warning** (never an error) when the English string contains the term and the translation does not contain the recommendation. A second block, `alias` then `term`, maps another English surface onto a term id (`LVL` onto `level`). A match inside a do-not-translate string or a brand string does not count. `space trash` inside the logo `SPACE TRASH CLEAN` is not a request for the space-trash word.

## How to read a row

`dnt: yes` means do-not-translate: copy the first English form. `check` is the validator flag. Use one term everywhere it applies. Do not invent a synonym for a checked term.

## Tone

Playful, warm, short, upbeat, safe for children. No slang that will date. No sarcasm. No scare words for a failed run. Nibbles is a friend who is hungry, not a monster.

## Do-not-translate

Nibbles. The logo lines ORBITAL, SALVAGE, and SPACE TRASH CLEAN. The sentence name Orbital Salvage, until the app name is chosen. The unit kg. Endonyms in the language picker (English, Deutsch, Français, and the rest of the registry, including the leading `✓ `). Strings with no letters (digits, `×`, `▶`, `/`). `localStorage` stays as that token inside the privacy sentence.

A word that is already correct in the target language stays identical to English, but it does not go in `doNotTranslate`. That list is the English list and `merge_table.py` recopies it. Write the key in `localization/notes/<code>-identical-ok.json` with a reason. German `Hangar` is a loanword and German `Reset` is a common UI word. The same file is how French keeps `Hangar` and Indonesian keeps `level` or `orbit` when the whole string is already the right word.

## False friends / things not to localise

kg, ×, ▶, ←, /, %, +, Western digits 0-9, `{0}` `{1}`, endonyms, Nibbles, ORBITAL, SALVAGE, SPACE TRASH CLEAN, localStorage, and version tokens such as v1.2.0. The arrow ← stays left-pointing in Arabic for now; flag it to the lead before mirroring.

## Address and formality

| Language | Choice                                                                                                                                                                                               |
| -------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| de       | du. Not Sie.                                                                                                                                                                                         |
| fr       | tu (France). Not vous.                                                                                                                                                                               |
| es       | tú, always. Neutral Latin-American vocabulary: computadora, celular, aplicación. No vosotros, no ordenador, no vos.                                                                                  |
| it       | tu.                                                                                                                                                                                                  |
| pt-PT    | tu for the child. European lexicon: ecrã, telemóvel. Not Brazilian você as the default, and not tela or celular.                                                                                     |
| vi       | bạn, or drop the pronoun. Not mày.                                                                                                                                                                   |
| fil      | ka. Standard Filipino. A little Taglish is fine for a kid; do not leave a whole sentence in English.                                                                                                 |
| id       | kamu. Not Anda, not kau.                                                                                                                                                                             |
| uz       | sen. Latin script only.                                                                                                                                                                              |
| hi       | तुम. Not आप, not तू. Devanagari. Western digits.                                                                                                                                                     |
| ar       | Modern Standard Arabic, kid-friendly, little or no tashkeel. Not a dialect. RTL. Western digits.                                                                                                     |
| ja       | Sentences use です/ます. Buttons and fragments are short nouns or the polite stem, without だ・である and without slang. です/ます is the choice for kids: clear, warm, and not a teacher's lecture. |
| ko       | 해요체 (해요, 이에요). Not 합니다체, not 해체. Labels can be nouns.                                                                                                                                  |
| zh-Hans  | 你. Mainland wording (软件, 屏幕, 视频).                                                                                                                                                             |
| zh-Hant  | 你. Taiwan wording (軟體, 螢幕, 影片, 首頁). Not mainland terms.                                                                                                                                     |
| km       | Polite kid tone. Not the royal register. Native review.                                                                                                                                              |

## Numbers, units, emoji

Digits stay Western 0-9 in every language, including Arabic, Hindi, and Chinese. The game formats numbers with invariant rules, so a German player still sees `1.5`, not `1,5`. Do not introduce a decimal comma. kg stays kg. Keep `×`, `/`, `%`, `+`, and the digit sequence. Emoji and `✓` stay exactly as in the source, in the same relative position (a trailing check stays trailing). Do not add emoji.

## Punctuation, quotes, capitals

Checked against Fredoka, Nunito, and the two Noto symbol fonts on 2026-10-12.

- French: put U+00A0 (no-break space) before `! ? : ;`. U+202F is the modern preference and is missing from every shipped face. The cmap check skips whitespace, so a punctuation warning forbids U+202F. U+00A0 is in Fredoka and Nunito. Guillemets « » with U+00A0 inside.
- German: no extra space before `! ?`. Nouns take a capital. Quotes are „..." (U+201E … U+201C), which are in Fredoka.
- Spanish: curly quotes “” (U+201C … U+201D). No required space before `! ?` (that space is a Spain-only habit we are not using).
- Portuguese (Portugal): « » with U+00A0 inside.
- Italian: « ».
- Chinese and Japanese: full-width `。！？、` for CJK clauses, and no space before them. Half-width for digits, kg, and Latin names. Those full-width marks are not in the Latin fonts; they belong to the pending CJK faces. The glyph check does not run for zh/ja until those faces ship.
- Korean: half-width `. ! ?` with no space before them.
- Arabic: Arabic comma U+060C and question mark U+061F. Those marks are not in the Latin fonts; Cairo is still pending, so write them anyway.
- Capitals: translate in normal casing. German nouns stay capitalised. When the CSV says `uppercase`, the renderer uppercases Latin at runtime (`ß` becomes `SS`). Do not pre-uppercase. Caseless scripts ignore that transform, so a Latin token such as kg stays as written. When the English key is already all caps but `uppercase` is `none` (BRONZE, PLAY), the table text is what the player sees: write the case you want on screen.
- Do not abbreviate with a period to save space. Pick a shorter word.

## Terms

One line each. The translations are in the table below, not here.

- Nibbles — mascot name. Do not translate.
- ORBITAL, SALVAGE, SPACE TRASH CLEAN — logo. Do not translate.
- Orbital Salvage — product name in sentences. Do not translate yet.
- kg — the mass unit. Do not translate.
- scrap — debris Nibbles eats.
- gold coins — the coin balance.
- sector — one stage.
- world — a group of stages.
- level — a numbered stage. German keeps Level.
- cosmic janitor — the player's job.
- cleanup — the work of the game.
- black hole — what Nibbles is.
- ring — the spinning toy.
- donut — the candy shape.
- singularity — a black-hole costume.
- Nibbles Hats — hat shop category. Keep Nibbles.
- Donut Rings — ring shop category.
- Skies — backdrop shop category.
- trophy — a prize in the trophy room.
- daily operations — the daily-jobs screen.
- shift — one daily job.
- bronze, silver, gold — the three shift tiers. Gold here is not gold coins.
- master bounty — the reward for all three shifts.
- backup pod — the spare pod on a failed run. English has two names for the same thing: the button says Backup Pod (`Launch Backup Pod`) and the toast says Auxiliary Scrap Pod (`Auxiliary Scrap Pod Deployed`). Pick this one glossary word for both. Do not invent a second word for the toast.
- stipend — the free daily coins.
- allowance — that gift on the claim banner.
- debrief — the success summary.
- defeat — a failed run, said kindly.
- Toy Shop — the costume shop.
- snacks — trash as food.
- orbit — where the trash is.
- space trash — the junk.
- map — a world map.
- chute — a candy-slide level.
- cadet — the beginner rank.
- claim — take a reward.
- cleared — finished.
- equipped — worn now.
- prize — a reward line.
- home — the main screen.
- pause — a break in the mission.
- care package — a gift bundle.
- wear — put a costume on.
- unlock — spend coins on a costume.
- nom nom — Nibbles' eating noise.

```text
term	en	dnt	check	confidence	native-review	definition	zh-Hans	zh-Hant	ja	ko	de	fr	es	it	pt-PT	vi	fil	id	uz	km	hi	ar
nibbles	Nibbles	yes	yes	high		Name of the hungry black-hole mascot. Never translate, never spell phonetically.	Nibbles	Nibbles	Nibbles	Nibbles	Nibbles	Nibbles	Nibbles	Nibbles	Nibbles	Nibbles	Nibbles	Nibbles	Nibbles	Nibbles	Nibbles	Nibbles
orbital	ORBITAL	yes	yes	high		Logo word. The app name is not decided. Leave this all-caps title untouched.	ORBITAL	ORBITAL	ORBITAL	ORBITAL	ORBITAL	ORBITAL	ORBITAL	ORBITAL	ORBITAL	ORBITAL	ORBITAL	ORBITAL	ORBITAL	ORBITAL	ORBITAL	ORBITAL
salvage	SALVAGE	yes	yes	high		Logo word. Leave this all-caps title untouched.	SALVAGE	SALVAGE	SALVAGE	SALVAGE	SALVAGE	SALVAGE	SALVAGE	SALVAGE	SALVAGE	SALVAGE	SALVAGE	SALVAGE	SALVAGE	SALVAGE	SALVAGE	SALVAGE
space-trash-clean	SPACE TRASH CLEAN	yes	yes	high		Logo line under the title. Leave this all-caps line untouched. Translate taglines around it.	SPACE TRASH CLEAN	SPACE TRASH CLEAN	SPACE TRASH CLEAN	SPACE TRASH CLEAN	SPACE TRASH CLEAN	SPACE TRASH CLEAN	SPACE TRASH CLEAN	SPACE TRASH CLEAN	SPACE TRASH CLEAN	SPACE TRASH CLEAN	SPACE TRASH CLEAN	SPACE TRASH CLEAN	SPACE TRASH CLEAN	SPACE TRASH CLEAN	SPACE TRASH CLEAN	SPACE TRASH CLEAN
orbital-salvage	Orbital Salvage	yes	yes	high		Product name in sentences and the version footer. Leave the name untouched until the app name is chosen.	Orbital Salvage	Orbital Salvage	Orbital Salvage	Orbital Salvage	Orbital Salvage	Orbital Salvage	Orbital Salvage	Orbital Salvage	Orbital Salvage	Orbital Salvage	Orbital Salvage	Orbital Salvage	Orbital Salvage	Orbital Salvage	Orbital Salvage	Orbital Salvage
kg	kg	yes	yes	high		Mass unit. Always the Latin letters kg. Do not write 公斤, кг, or kilo.	kg	kg	kg	kg	kg	kg	kg	kg	kg	kg	kg	kg	kg	kg	kg	kg
scrap	scrap	no	yes	med	km uz fil hi	Debris Nibbles eats. Not the coin balance. Short and concrete.	废料	廢料	スクラップ	고철	Schrott	ferraille	chatarra	rottami	sucata	phế liệu	sirang-bakal	rongsokan	temir parcha	សំណល់ដែក	कबाड़	خردة
gold-coins	gold coins	no	yes	med	km uz fil hi	Spendable currency. The UI also says recycling coins and gameplay coins; use this term for the balance.	金币	金幣	ゴールドコイン	골드 코인	Goldmünzen	pièces d'or	monedas de oro	monete d'oro	moedas de ouro	xu vàng	gintong barya	koin emas	oltin tangalar	កាក់មាស	सोने के सिक्के	عملات ذهبية
sector	sector/sectors	no	yes	med	km uz fil hi	One playable stage. Choose Sector, Restart Sector. Not the whole world.	扇区	扇區	セクター	섹터	Sektor	secteur	sector	settore	setor	khu vực	sektor	sektor	sektor	ផ្នែក	सेक्टर	قطاع
world	world/worlds	no	yes	high	km uz fil hi	A themed group of stages. World 1, Bubblegum Belt.	世界	世界	ワールド	월드	Welt	monde	mundo	mondo	mundo	thế giới	mundo	dunia	dunyo	ពិភព	दुनिया	عالم
level	level/levels	no	yes	med	km uz fil hi	A numbered stage inside a world, in the reset copy. German keeps the game word Level.	关卡	關卡	レベル	레벨	Level	niveau	nivel	livello	nível	màn	antas	level	bosqich	កម្រិត	स्तर	مرحلة
cosmic-janitor	cosmic janitor	no	yes	low	km uz fil hi	The player's cute job. Warm, not a real profession title.	宇宙清洁工	宇宙清潔員	宇宙のおそうじ屋さん	우주 청소부	kosmischer Hausmeister	concierge cosmique	conserje cósmico	custode cosmico	auxiliar cósmico de limpeza	lao công vũ trụ	kosmikong tagalinis	petugas kebersihan kosmik	koinot farroshi	អ្នកសម្អាតអវកាស	अंतरिक्ष सफाईकर्मी	عامل نظافة كوني
cleanup	cleanup	no	no	med	km uz fil hi	The job of swallowing trash. Verb or noun, whichever the sentence needs.	清理	清理	おそうじ	청소	Aufräumen	nettoyage	limpieza	pulizia	limpeza	dọn dẹp	paglilinis	bersih-bersih	tozalash	ការសម្អាត	सफाई	تنظيف
black-hole	black hole	no	yes	high	km uz fil hi	What Nibbles is. Cute, not a science lecture. Hindi uses the kid-familiar loan; कृष्ण विवर is the textbook word.	黑洞	黑洞	ブラックホール	블랙홀	Schwarzes Loch	trou noir	agujero negro	buco nero	buraco negro	hố đen	itim na butas	lubang hitam	qora tuynuk	ប្រហោងខ្មៅ	ब्लैक होल	ثقب أسود
ring	ring/rings	no	no	med	km uz fil hi	The spinning chute the player turns. Also the costume slot Donut Rings.	环	環	リング	링	Ring	anneau	anillo	anello	anel	vòng	singsing	cincin	halqa	ចិញ្ចៀន	छल्ला	حلقة
donut	donut	no	no	med	km uz fil hi	The candy shape of the rings. es uses LatAm dona. fr keeps donut for the costume, not beignet.	甜甜圈	甜甜圈	ドーナツ	도넛	Donut	donut	dona	ciambella	donut	bánh vòng	donat	donat	donut	ដូណាត់	डोनट	دونات
singularity	singularity	no	no	low	km uz fil hi	Shop word for a black-hole look. The Kessler Singularity is a proper place name; keep the same word.	奇点	奇點	シンギュラリティ	특이점	Singularität	singularité	singularidad	singolarità	singularidade	điểm kỳ dị	singularidad	singularitas	singulyarlik	ស៊ីងហ្គុយឡារីតេ	सिंगुलैरिटी	تفرد
nibbles-hats	Nibbles Hats	no	yes	med	km uz fil hi	Shop category. Keep Nibbles. Translate Hats.	Nibbles 帽子	Nibbles 帽子	Nibblesのぼうし	Nibbles 모자	Nibbles-Hüte	chapeaux de Nibbles	gorros de Nibbles	cappelli di Nibbles	chapéus do Nibbles	mũ Nibbles	mga sumbrero ni Nibbles	topi Nibbles	Nibbles qalpoqlari	មួក Nibbles	Nibbles की टोपियाँ	قبعات Nibbles
donut-rings	Donut Rings	no	no	med	km uz fil hi	Shop category for ring costumes.	甜甜圈环	甜甜圈環	ドーナツリング	도넛 링	Donut-Ringe	anneaux donut	anillos de dona	anelli ciambella	anéis donut	vòng bánh vòng	mga singsing na donat	cincin donat	donut halqalari	ចិញ្ចៀនដូណាត់	डोनट छल्ले	حلقات دونات
skies	Skies/skies	no	no	med	km uz fil hi	Shop category for backdrops.	天空	天空	そら	하늘	Himmel	ciels	cielos	cieli	céus	bầu trời	kalangitan	langit	osmonlar	មេឃ	आसमान	سماوات
trophy	trophy/trophies	no	yes	med	km uz fil hi	A prize in the trophy room. Daily Duty Star is one trophy.	奖杯	獎盃	トロフィー	트로피	Trophäe	trophée	trofeo	trofeo	troféu	cúp	tropeo	trofi	kubok	ពាន	ट्रॉफी	كأس
daily-operations	daily operations	no	yes	med	km uz fil hi	The daily-jobs screen. Cute work, not a military briefing.	每日任务	每日任務	まいにちのおしごと	일일 작전	Tägliche Einsätze	opérations du jour	operaciones diarias	operazioni giornaliere	operações diárias	nhiệm vụ hằng ngày	araw-araw na gawain	tugas harian	kunlik ishlar	ការងារប្រចាំថ្ងៃ	रोज़ के काम	المهام اليومية
shift	shift/shifts	no	yes	med	km uz fil hi	One of the three daily jobs: bronze, silver, gold.	班次	班次	シフト	근무	Schicht	service	turno	turno	turno	ca	shift	giliran	smena	វេន	पाली	وردية
bronze	bronze	no	no	med	km uz fil hi	Easiest daily shift. A metal colour, not a person's name.	铜	銅	ブロンズ	브론즈	Bronze	bronze	bronce	bronzo	bronze	đồng	tanso	perunggu	bronza	សំរឹទ្ធ	कांस्य	برونزي
silver	silver	no	no	med	km uz fil hi	Middle daily shift.	银	銀	シルバー	실버	Silber	argent	plata	argento	prata	bạc	pilak	perak	kumush	ប្រាក់	चाँदी	فضي
gold	gold	no	no	med	km uz fil hi	Hardest daily shift. Do not reuse the gold-coins phrase here.	金	金	ゴールド	골드	Gold	or	oro	oro	ouro	vàng	ginto	emas	oltin	មាស	स्वर्ण	ذهبي
master-bounty	master bounty	no	no	low	km uz fil hi	The bonus for finishing all three shifts. Master All-Clear Bounty.	大师赏金	大師賞金	マスターほうしゅう	마스터 포상	Meisterprämie	prime de maître	recompensa maestra	taglia maestra	prémio mestre	thưởng bậc thầy	gantimpalang master	hadiah master	usta mukofoti	រង្វាន់អ្នកជំនាញ	मास्टर इनाम	مكافأة الخبير
backup-pod	backup pod	no	yes	low	km uz fil hi	The spare pod offered on a failed run. Helpful, not scary. English says Backup Pod on the button and Auxiliary Scrap Pod on the toast. Use this one word for both.	备用舱	備用艙	バックアップポッド	예비 포드	Reservekapsel	capsule de secours	cápsula de reserva	capsula di riserva	cápsula de reserva	khoang dự phòng	ekstrang pod	pod cadangan	zaxira kapsula	កន្សោមបម្រុង	बैकअप पॉड	حجرة احتياط
stipend	stipend	no	yes	med	km uz fil hi	The free daily coin gift. FREE DAILY STIPEND. Same family as allowance.	每日津贴	每日津貼	まいにちのてあて	일일 용돈	Tagesprämie	allocation du jour	asignación diaria	paghetta giornaliera	subsídio diário	trợ cấp hằng ngày	araw-araw na baon	uang saku harian	kunlik nafaqa	ប្រាក់ឧបត្ថម្ភប្រចាំថ្ងៃ	रोज़ का भत्ता	مخصص يومي
allowance	allowance	no	no	med	km uz fil hi	The same gift on the claim banner. Allowance Claimed. Pocket money, not a salary.	零用钱	零用錢	おこづかい	용돈	Taschengeld	argent de poche	dinero de bolsillo	paghetta	mesada	tiền tiêu vặt	baon	uang jajan	choʻntak puli	លុយហោប៉ៅ	जेब खर्च	مصروف
debrief	debrief	no	no	med	km uz fil hi	The success summary after a run. Cheerful, short.	结算	結算	結果報告	결과 보고	Abschluss	bilan	resumen	riepilogo	resumo	tổng kết	ulat	laporan	hisobot	សង្ខេប	सारांश	ملخص
defeat	defeat	no	no	med	km uz fil hi	A failed run. Say it kindly. The banner is SHIFT INCOMPLETE, not an insult.	失败	失敗	しっぱい	실패	daneben	échec	fallo	sconfitta	falha	thua	talo	gagal	yutqazish	ចាញ់	हार	خسارة
toy-shop	Toy Shop	no	yes	med	km uz fil hi	Where costumes are unlocked with coins. No real money.	玩具店	玩具店	おもちゃやさん	장난감 가게	Spielzeugladen	boutique de jouets	tienda de juguetes	negozio di giocattoli	loja de brinquedos	cửa hàng đồ chơi	tindahan ng laruan	toko mainan	oʻyinchoq doʻkoni	ហាងរបស់លេង	खिलौना दुकान	متجر الألعاب
snacks	snack/snacks	no	no	med	km uz fil hi	Trash treated as food for Nibbles. Snacks Collected, Snacks Eaten.	点心	點心	おやつ	간식	Snacks	goûters	golosinas	merendine	lanches	đồ ăn vặt	meryenda	camilan	yengil taom	អាហារសម្រន់	स्नैक्स	وجبات خفيفة
orbit	orbit	no	no	high	km uz fil hi	The place being cleaned. The space orbit, an orbit level name.	轨道	軌道	きどう	궤도	Umlaufbahn	orbite	órbita	orbita	órbita	quỹ đạo	orbit	orbit	orbita	គន្លង	कक्षा	مدار
space-trash	space trash	no	yes	med	km uz fil hi	The junk in the title's meaning. space junk in body copy uses the same term.	太空垃圾	太空垃圾	うちゅうゴミ	우주 쓰레기	Weltraummüll	déchets de l'espace	basura espacial	spazzatura spaziale	lixo espacial	rác vũ trụ	basura sa kalawakan	sampah luar angkasa	koinot chiqindisi	សំរាមអវកាស	अंतरिक्ष कचरा	نفايات الفضاء
map	map/maps	no	no	med	km uz fil hi	A world map. CLEANUP MAPS.	地图	地圖	マップ	맵	Karte	carte	mapa	mappa	mapa	bản đồ	mapa	peta	xarita	ផែនទី	नक्शा	خريطة
chute	chute/chutes	no	no	med	km uz fil hi	A candy slide level. Bubblegum Chute, Moon Chute. Keep the candy word translated too.	滑道	滑道	シュート	슈트	Rutsche	toboggan	tobogán	scivolo	escorrega	máng trượt	padulas	perosotan	sirpanchiq	ផ្លូវរអិល	फिसलपट्टी	زحليقة
cadet	cadet	no	no	med	km uz fil hi	The beginner rank. Tidy Space Cadet, Cadet Sweep.	学员	學員	カデット	생도	Kadett	cadet	cadete	cadetto	cadete	học viên	kadete	kadet	kadet	សិស្ស	कैडेट	طالب
claim	claim	no	no	med	km uz fil hi	Button verb for taking a free reward. Claim +100.	领取	領取	受け取る	받기	Abholen	récupérer	reclamar	riscuoti	receber	nhận	kunin	klaim	olish	ទទួល	लो	اجمع
cleared	cleared	no	no	med	km uz fil hi	A finished shift or stage. 2 / 3 CLEARED. The digits stay; this word changes.	已通过	已通過	クリア	클리어	Geschafft	réussi	superado	superato	concluído	đã qua	nalampasan	selesai	oʻtildi	បានឆ្លង	पूरा	تم
equipped	equipped	no	no	med	km uz fil hi	The costume that is on. Badge text.	已装备	已裝備	装備中	장착됨	Angelegt	équipé	equipado	indossato	equipado	đã trang bị	suot na	terpasang	taqilgan	បានពាក់	लगा है	مجهز
prize	prize	no	no	med	km uz fil hi	The reward line on a result. Not a contest trophy unless the string says trophy.	奖品	獎品	ごほうび	상품	Preis	prix	premio	premio	prémio	giải thưởng	premyo	hadiah	sovrin	រង្វាន់	इनाम	جائزة
home	home	no	no	med	km uz fil hi	Return to Home. The main screen.	主页	首頁	ホーム	홈	Start	accueil	inicio	inizio	início	trang chủ	tahanan	beranda	uy	ទំព័រដើម	होम	الرئيسية
pause	pause	no	no	med	km uz fil hi	Mission paused. A break, not a failure.	暂停	暫停	いちじていし	일시정지	Pause	pause	pausa	pausa	pausa	tạm dừng	pahinga	jeda	tanaffus	ផ្អាក	रोकें	إيقاف
care-package	care package	no	no	low	km uz fil hi	A gift bundle. Fragment keys may hold only this phrase.	关怀礼包	關懷禮包	ケアパッケージ	돌봄 상자	Carepaket	colis cadeau	paquete de regalo	pacco regalo	cabaz	gói quà	pakete ng alaga	paket bingkisan	sovgʻa qutisi	កញ្ចប់អំណោយ	केयर पैकेज	صندوق هدية
wear	wear	no	no	med	km uz fil hi	Button that puts a costume on.	穿上	穿上	きる	입기	Anziehen	porter	poner	indossa	vestir	mặc	isuot	pakai	kiyish	ពាក់	पहनें	ارتد
unlock	unlock	no	no	med	km uz fil hi	Button that spends coins on a costume. Unlock (1200. The number stays in its own run.	解锁	解鎖	かいじょ	잠금 해제	Freischalten	débloquer	desbloquear	sblocca	desbloquear	mở khóa	buksan	buka	ochish	ដោះសោ	अनलॉक	افتح
nom-nom	nom nom	no	no	low	km uz fil hi	Nibbles' eating noise. A cute local sound is fine. Keeping nom nom is also fine.	啊呜啊呜	啊嗚啊嗚	もぐもぐ	냠냠	mampf mampf	miam miam	ñam ñam	gnam gnam	nham nham	nham nham	nyam nyam	nyam nyam	ham ham	ញ៉ាំញ៉ាំ	नम नम	نام نام
```

## Aliases

Surfaces the headword does not spell. `validate_table.py` treats each alias as the term id, with the same `check` flag. Abbreviations are exact (`LVL` does not match inside another word). `space junk` is the body-copy form of `space trash`. `Auxiliary Scrap Pod` is the toast name of `backup pod`.

```text
alias	term
LVL	level
space junk	space-trash
Auxiliary Scrap Pod	backup-pod
```
