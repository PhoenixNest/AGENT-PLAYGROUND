using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class WP17_LevelSweepPlayTests
{
	private const float SceneTimeout = 60f;

	private const float FlowTimeout = 20f;

	private const float NextTimeout = 60f;

	private const float AttemptGameLimit = 180f;

	private const float StallSeconds = 6f;

	private const int ReversalsBeforeOmegaChange = 3;

	private const float EarlyRotationSeconds = 2f;

	private const int EarlyRotationFrames = 15;

	private const float RotationThresholdDeg = 5f;

	private const float PlayTimeScale = 3f;

	private const float CircleRadiusFraction = 0.3f;

	private const float RotatePhaseSeconds = 4f;

	private const float HoldPhaseSeconds = 2f;

	private const float LostBelowBucketMargin = 2f;

	private const float LostFromCentreDistance = 30f;

	private const int AttemptsPerLevel = 2;

	private const int MouseIndex = -2;

	private static string DefaultReportDirectory
	{
		get { return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "..", "output", "diagnostics", "sweep")); }
	}

	private const string ReportFileName = "level-sweep.md";

	private static readonly float[] Omegas = { 90f, 45f, 180f };

	private const BindingFlags AnyMember = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
		| BindingFlags.Instance | BindingFlags.FlattenHierarchy;

	private const BindingFlags AnyInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

	private static readonly Type GameManagerType = GameType("GameManager");

	private static readonly Type LevelsControllerType = GameType("Levels.LevelsController");

	private static readonly Type MiniLevelViewType = GameType("Levels.MiniLevelView");

	private static readonly Type BallsControllerType = GameType("Balls.BallsController");

	private static readonly Type FailLevelScreenType = GameType("FailLevelScreen");

	private static readonly Type RotateControllerType = GameType("RotateController");

	private static readonly Type LeanTouchType = FindType("Lean.Touch.LeanTouch");

	private readonly List<string> _errorLines = new List<string>();

	private readonly List<LevelRecord> _records = new List<LevelRecord>();

	private string _reportDirectory = DefaultReportDirectory;

	private int _sweepLevel;

	private int _attemptsTotal;

	private int _restarts;

	private int _transitions;

	private int _rotationFailures;

	[SetUp]
	public void SetUp()
	{
		PlayerPrefs.DeleteAll();
		PlayerPrefs.Save();
		Time.timeScale = 1f;
		ClearLeanTouchFingers();
		Application.logMessageReceived += OnLog;
	}

	[TearDown]
	public void TearDown()
	{
		Application.logMessageReceived -= OnLog;
		Time.timeScale = 1f;
		PlayerPrefs.DeleteAll();
		PlayerPrefs.Save();
	}

	[UnityTest, Explicit, Category("LevelSweep"), Timeout(int.MaxValue)]
	public IEnumerator PlayEveryLevelThroughTheRealLoop()
	{
		_reportDirectory = ReportDirectory();
		yield return BootToMain();

		int total = (int)Member(LevelsControllerType, null, "TotalLevelCount");
		int last = Math.Min(total, MaxLevels());
		Assert.That(last, Is.GreaterThan(0), "At least one live level must exist.");
		WriteReport(total, last, false);

		bool opened = false;
		string enteredBy = "select";
		for (int level = Math.Max(1, StartLevel()); level <= last; level++)
		{
			_sweepLevel = level;
			var record = new LevelRecord { Level = level };
			_records.Add(record);

			if (!opened)
			{
				var started = new Result();
				yield return OpenLevel(level, started);
				opened = started.Ok;
				if (!opened)
				{
					record.Entry = "select";
					AddNote(record, "level did not start");
					WriteReport(total, last, false);
					enteredBy = "select";
					continue;
				}
			}

			record.Entry = enteredBy;
			ReadLevelInfo(record);

			bool won = false;
			for (int attemptNo = 1; attemptNo <= AttemptsPerLevel; attemptNo++)
			{
				var attempt = new Attempt();
				yield return PlayAttempt(attempt);
				_attemptsTotal++;

				record.Outcomes.Add(attempt.Outcome);
				string rotation = attempt.RotationOk ? "yes" : "NO (" + attempt.RotationDeg.ToString("0.0") + " deg)";
				if (attempt.SubTarget != null)
				{
					rotation += " [Targets[0] " + attempt.SubDeg.ToString("0.0") + " deg]";
				}
				record.Rotation.Add(rotation);
				if (!attempt.RotationOk)
				{
					_rotationFailures++;
					AddNote(record, "attempt " + attemptNo + " rotation diagnostics: " + attempt.Diagnostics);
					if (!string.IsNullOrEmpty(attempt.UiAtStart))
					{
						AddNote(record, "attempt " + attemptNo + " pointer start over UI: " + attempt.UiAtStart);
					}
				}
				if (attempt.Outcome == "TIMEOUT")
				{
					AddNote(record, "attempt " + attemptNo + " TIMEOUT " + attempt.TimeoutSummary);
				}
				record.MaxFill = Math.Max(record.MaxFill, attempt.MaxFill);
				record.MaxBalls = Math.Max(record.MaxBalls, attempt.MaxBalls);
				record.GameSeconds += attempt.GameSeconds;
				Time.timeScale = 1f;
				WriteReport(total, last, false);

				if (attempt.Outcome == "WIN")
				{
					won = true;
					record.WonOnAttempt = attemptNo;
					break;
				}
				if (attemptNo == AttemptsPerLevel)
				{
					break;
				}

				var restarted = new Result();
				if (attempt.Outcome == "FAIL")
				{
					yield return RestartAfterFail(restarted);
				}
				else
				{
					yield return RestartInGame(restarted);
				}
				if (!restarted.Ok)
				{
					AddNote(record, "restart did not return to a started level");
					break;
				}
				_restarts++;
			}

			if (won)
			{
				enteredBy = "select";
				if (level < last)
				{
					var next = new Result();
					yield return ProceedToNextLevel(level, next);
					opened = next.Ok;
					if (next.Ok)
					{
						_transitions++;
						enteredBy = "Next";
					}
					else
					{
						AddNote(record, "Next did not start level " + (level + 1));
					}
				}
			}
			else
			{
				AddNote(record, "not won in " + AttemptsPerLevel + " attempts");
				opened = false;
				enteredBy = "select";
			}

			WriteReport(total, last, false);
		}

		Time.timeScale = 1f;
		WriteReport(total, last, true);

		Assert.IsEmpty(_errorLines, "Error, exception or assert logs during the sweep. See " + Path.Combine(_reportDirectory, ReportFileName));
		Assert.AreEqual(0, _rotationFailures, "Rotation input must visibly rotate the maze in the first 2 s of every attempt.");
	}

	private void OnLog(string condition, string stackTrace, LogType type)
	{
		if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert)
		{
			return;
		}
		_errorLines.Add("L" + _sweepLevel + " [" + type + "] " + condition + "\n" + FirstFrames(stackTrace, 3));
	}

	private static string FirstFrames(string stackTrace, int count)
	{
		if (string.IsNullOrEmpty(stackTrace))
		{
			return "    (no stack trace)";
		}
		var frames = new List<string>();
		foreach (string raw in stackTrace.Split('\n'))
		{
			string line = raw.Trim();
			if (line.Length == 0)
			{
				continue;
			}
			frames.Add("    at " + line);
			if (frames.Count == count)
			{
				break;
			}
		}
		return string.Join("\n", frames);
	}

	private static void AddNote(LevelRecord record, string text)
	{
		record.Note = string.IsNullOrEmpty(record.Note) ? text : record.Note + "; " + text;
	}

	private static IEnumerator OpenLevel(int level, Result result)
	{
		result.Ok = false;
		Time.timeScale = 1f;
		object game = Instance();
		Call(GameManagerType, game, "SaveData", level);
		SetMember(GameManagerType, game, "CurrentLevel", level);
		Call(GameManagerType, game, "Reset", false);

		Component main = MainScreen() as Component;
		if (main != null && !main.gameObject.activeSelf)
		{
			main.gameObject.SetActive(true);
		}

		var wait = new Result();
		yield return Until(() => CurrentRotateController() != null && IsActive(MainScreen()), FlowTimeout, wait);
		if (!wait.Ok)
		{
			yield break;
		}
		yield return StartFromMain(result);
	}

	private static IEnumerator StartFromMain(Result result)
	{
		result.Ok = false;
		var wait = new Result();
		yield return Until(() => IsActive(MainScreen()), FlowTimeout, wait);
		if (!wait.Ok || !TapToStart())
		{
			yield break;
		}
		yield return Until(() => (bool)Member(GameManagerType, Instance(), "GameStarted"), FlowTimeout, wait);
		if (!wait.Ok)
		{
			yield break;
		}
		yield return Until(() => !IsActive(MainScreen()), FlowTimeout, wait);
		if (!wait.Ok)
		{
			yield break;
		}
		yield return Until(() => CurrentRotateController() != null, FlowTimeout, wait);
		result.Ok = wait.Ok;
	}

	private static IEnumerator RestartAfterFail(Result result)
	{
		result.Ok = false;
		object fail = FailScreen();
		var wait = new Result();
		yield return Until(() => IsActive(FailScreen()), FlowTimeout, wait);
		if (!wait.Ok)
		{
			yield break;
		}

		Button restart = FindButtonCalling((Component)fail, "OnClickRestartLevel");
		if (restart != null)
		{
			yield return Until(() => restart.gameObject.activeInHierarchy && restart.interactable, FlowTimeout, wait);
			if (!wait.Ok)
			{
				yield break;
			}
			restart.onClick.Invoke();
		}
		else
		{
			Call(FailLevelScreenType, fail, "OnClickRestartLevel");
		}

		yield return Until(() => !IsActive(FailScreen()), FlowTimeout, wait);
		if (!wait.Ok)
		{
			yield break;
		}
		yield return StartFromMain(result);
	}

	private static IEnumerator RestartInGame(Result result)
	{
		result.Ok = false;
		Time.timeScale = 1f;
		object gameScreen = Member(GameManagerType, Instance(), "gameScreen");
		Button restart = gameScreen != null ? Member(gameScreen.GetType(), gameScreen, "RestartBtn") as Button : null;
		if (restart == null)
		{
			yield break;
		}

		var wait = new Result();
		yield return Until(() => restart.interactable, FlowTimeout, wait);
		if (!wait.Ok)
		{
			yield break;
		}
		restart.onClick.Invoke();
		yield return Until(() => IsActive(MainScreen()), FlowTimeout, wait);
		if (!wait.Ok)
		{
			yield break;
		}
		yield return StartFromMain(result);
	}

	private static IEnumerator ProceedToNextLevel(int level, Result result)
	{
		result.Ok = false;
		Time.timeScale = 1f;
		object complete = Member(GameManagerType, Instance(), "completeLevelScreen");
		var wait = new Result();
		yield return Until(() => complete != null && ((Component)complete).gameObject.activeSelf, NextTimeout, wait);
		if (!wait.Ok)
		{
			yield break;
		}

		GameObject nextButton = (GameObject)Member(complete.GetType(), complete, "NextLevelBtn");
		yield return Until(() => nextButton != null && nextButton.activeSelf, NextTimeout, wait);
		if (!wait.Ok)
		{
			yield break;
		}

		Button next = FindButtonCalling((Component)complete, "OnClickNextLevel");
		if (next != null)
		{
			next.onClick.Invoke();
		}
		else
		{
			Call(complete.GetType(), complete, "OnClickNextLevel");
		}

		yield return Until(() => (int)Member(GameManagerType, Instance(), "CurrentLevel") == level + 1 && IsActive(MainScreen()),
			FlowTimeout, wait);
		if (!wait.Ok)
		{
			yield break;
		}
		yield return StartFromMain(result);
	}

	private static IEnumerator PlayAttempt(Attempt attempt)
	{
		Time.timeScale = PlayTimeScale;
		Component rotate = CurrentRotateController();
		GameObject target = rotate != null ? Member(RotateControllerType, rotate, "Target") as GameObject : null;
		attempt.StartRotation = target != null ? target.transform.rotation : Quaternion.identity;
		attempt.SubTarget = FirstSubTarget(rotate);
		attempt.SubStart = attempt.SubTarget != null ? attempt.SubTarget.rotation : Quaternion.identity;
		object ballsController = UnityEngine.Object.FindAnyObjectByType(BallsControllerType);

		float game = 0f;
		float cycle = 0f;
		float lastProgress = 0f;
		int lastFill = -1;
		int reversals = 0;
		int omegaIndex = 0;
		float direction = 1f;
		float angle = 0f;
		bool releaseNext = false;
		bool measured = false;
		int frames = 0;
		int dragFrames = 0;
		attempt.UiAtStart = BlockingUiAt(CirclePointer(0f));

		while (true)
		{
			if ((bool)Member(GameManagerType, Instance(), "LevelCompleted"))
			{
				attempt.Outcome = "WIN";
				break;
			}
			if (IsActive(FailScreen()))
			{
				attempt.Outcome = "FAIL";
				break;
			}
			if (game >= AttemptGameLimit)
			{
				attempt.Outcome = "TIMEOUT";
				break;
			}

			int fill = (int)Member(LevelsControllerType, null, "TotalFillCount");
			attempt.MaxFill = Math.Max(attempt.MaxFill, fill);
			if (ballsController != null)
			{
				attempt.MaxBalls = Math.Max(attempt.MaxBalls, (int)Member(BallsControllerType, ballsController, "_currentBallCount"));
			}

			if (fill != lastFill)
			{
				lastFill = fill;
				lastProgress = game;
				reversals = 0;
			}
			else if (game - lastProgress >= StallSeconds)
			{
				direction = -direction;
				releaseNext = true;
				reversals++;
				lastProgress = game;
				if (reversals >= ReversalsBeforeOmegaChange && omegaIndex < Omegas.Length - 1)
				{
					omegaIndex++;
					reversals = 0;
				}
			}

			rotate = CurrentRotateController();
			Component lean = LeanFor(rotate);
			if (lean != null)
			{
				((Behaviour)lean).enabled = false;
				bool rotating = cycle < RotatePhaseSeconds;
				Vector2? pointer = null;
				if (releaseNext || !rotating)
				{
					releaseNext = false;
				}
				else
				{
					angle += Omegas[omegaIndex] * direction * Time.deltaTime;
						dragFrames++;
					pointer = CirclePointer(angle);
				}
				StepLean(lean, pointer);
			}

			if (!measured && game >= EarlyRotationSeconds && dragFrames >= EarlyRotationFrames)
			{
				attempt.FramesElapsed = frames;
				attempt.DragFrames = dragFrames;
				Measure(attempt, rotate);
				measured = true;
			}

			yield return null;
			game += Time.deltaTime;
			cycle = (cycle + Time.deltaTime) % (RotatePhaseSeconds + HoldPhaseSeconds);
			frames++;
		}

		if (!measured)
		{
			attempt.FramesElapsed = frames;
			attempt.DragFrames = dragFrames;
			Measure(attempt, rotate);
		}
		attempt.GameSeconds = game;
		if (attempt.Outcome == "TIMEOUT")
		{
			attempt.TimeoutSummary = TimeoutSummary(ballsController, rotate);
		}

		Component finalLean = LeanFor(CurrentRotateController());
		if (finalLean != null)
		{
			StepLean(finalLean, null);
		}
	}

	private static void Measure(Attempt attempt, Component rotate)
	{
		GameObject target = rotate != null ? Member(RotateControllerType, rotate, "Target") as GameObject : null;
		if (target == null)
		{
			attempt.RotationDeg = -1f;
			attempt.RotationOk = false;
		}
		else
		{
			attempt.RotationDeg = Quaternion.Angle(attempt.StartRotation, target.transform.rotation);
			attempt.RotationOk = attempt.RotationDeg > RotationThresholdDeg;
		}
		if (attempt.SubTarget != null)
		{
			attempt.SubDeg = Quaternion.Angle(attempt.SubStart, attempt.SubTarget.rotation);
		}
		if (!attempt.RotationOk)
		{
			attempt.Diagnostics = RotationDiagnostics(rotate, attempt);
		}
	}

	private static string RotationDiagnostics(Component rotate, Attempt attempt)
	{
		var parts = new List<string>();
		parts.Add("frames elapsed=" + attempt.FramesElapsed + ", drag frames=" + attempt.DragFrames);
		IList fingers = LeanTouchType != null ? (IList)Member(LeanTouchType, null, "Fingers") : null;
		parts.Add("LeanTouch.Fingers.Count=" + (fingers != null ? fingers.Count : -1));
		if (fingers != null && fingers.Count > 0)
		{
			object finger = fingers[0];
			Type fingerType = finger.GetType();
			parts.Add("Fingers[0].Set=" + Member(fingerType, finger, "Set"));
			parts.Add("Fingers[0].StartedOverGui=" + Member(fingerType, finger, "StartedOverGui"));
		}
		if (rotate == null)
		{
			parts.Add("RotateController=missing");
		}
		else
		{
			parts.Add("RotateController.enabled=" + ((Behaviour)rotate).enabled);
			parts.Add("Multiplier=" + Member(RotateControllerType, rotate, "Multiplier"));
			parts.Add("Damping=" + Member(RotateControllerType, rotate, "Damping"));
			var remaining = (Quaternion)Member(RotateControllerType, rotate, "remainingDelta");
			parts.Add("remainingDelta=" + Quaternion.Angle(remaining, Quaternion.identity).ToString("0.00") + " deg");
		}
		parts.Add("Camera.main=" + (Camera.main != null));
		parts.Add("Time.timeScale=" + Time.timeScale);
		if (attempt.SubTarget != null)
		{
			parts.Add("Targets[0] turned " + attempt.SubDeg.ToString("0.0") + " deg");
		}
		return string.Join(", ", parts);
	}

	private static Transform FirstSubTarget(Component rotate)
	{
		if (rotate == null)
		{
			return null;
		}
		var targets = Member(RotateControllerType, rotate, "Targets") as IList;
		if (targets == null || targets.Count == 0 || targets[0] == null)
		{
			return null;
		}
		object entry = targets[0];
		return Member(entry.GetType(), entry, "Target") as Transform;
	}

	private static string TimeoutSummary(object ballsController, Component rotate)
	{
		var inMaze = new Tally();
		var inFinishSpace = new Tally();
		var inBucket = new Tally();
		var other = new Tally();
		var lost = new Tally();

		object view = Member(LevelsControllerType, null, "MiniLevelView");
		Component bucket = view != null ? Member(MiniLevelViewType, view, "bucket") as Component : null;
		GameObject centre = rotate != null ? Member(RotateControllerType, rotate, "Target") as GameObject : null;
		float bucketY = bucket != null ? bucket.transform.position.y : float.NegativeInfinity;
		Vector3 centrePosition = centre != null ? centre.transform.position : Vector3.zero;

		var data = ballsController != null
			? (System.Collections.IDictionary)Member(BallsControllerType, ballsController, "_dataByBalls")
			: null;
		if (data != null)
		{
			foreach (System.Collections.DictionaryEntry entry in data)
			{
				var ball = entry.Key as Component;
				if (ball == null || !ball.gameObject.activeInHierarchy)
				{
					continue;
				}
				object ballData = entry.Value;
				Type ballType = ballData.GetType();
				if (!(bool)Member(ballType, ballData, "isActive"))
				{
					continue;
				}
				int value = (int)Member(ballType, ballData, "value");
				bool maze = (bool)Member(ballType, ballData, "inMaze");
				bool finish = (bool)Member(ballType, ballData, "inFinishSpace");
				bool bucketFlag = (bool)Member(ballType, ballData, "inBucket");
				if (maze)
				{
					inMaze.Add(value);
				}
				if (finish)
				{
					inFinishSpace.Add(value);
				}
				if (bucketFlag)
				{
					inBucket.Add(value);
				}
				if (!maze && !finish && !bucketFlag)
				{
					other.Add(value);
				}
				Vector3 position = ball.transform.position;
				if (position.y < bucketY - LostBelowBucketMargin || Vector3.Distance(position, centrePosition) > LostFromCentreDistance)
				{
					lost.Add(value);
				}
			}
		}

		int missing = (int)Member(LevelsControllerType, null, "TotalBallCountInMiniLevel")
			- (int)Member(LevelsControllerType, null, "TotalFillCount");
		return "outstanding: inMaze " + inMaze.Text + ", inFinishSpace " + inFinishSpace.Text
			+ ", inBucket " + inBucket.Text + ", other " + other.Text
			+ ", lost " + lost.Text + " (below bucket y-" + LostBelowBucketMargin + " or over " + LostFromCentreDistance
			+ " from maze centre), TotalBallCountInMiniLevel-TotalFillCount=" + missing;
	}

	private static Vector2? CirclePointer(float angleDeg)
	{
		Camera cam = Camera.main;
		Component view = Member(LevelsControllerType, null, "MiniLevelView") as Component;
		if (cam == null || view == null)
		{
			return null;
		}
		Vector3 centre = cam.WorldToScreenPoint(view.transform.position);
		float radius = Screen.width * CircleRadiusFraction;
		float rad = angleDeg * Mathf.Deg2Rad;
		return new Vector2(centre.x + Mathf.Cos(rad) * radius, centre.y + Mathf.Sin(rad) * radius);
	}

	private static Component LeanFor(Component rotate)
	{
		if (rotate != null && LeanTouchType != null)
		{
			Component attached = rotate.gameObject.GetComponent(LeanTouchType);
			if (attached != null)
			{
				return attached;
			}
		}
		IList instances = LeanInstances();
		return instances != null && instances.Count > 0 ? instances[0] as Component : null;
	}

	private static void ReadLevelInfo(LevelRecord record)
	{
		object data = Member(LevelsControllerType, null, "CurrentLevelData");
		var minis = new List<string>();
		var targets = new List<string>();
		var startBalls = new List<string>();
		object names = data != null ? Member(data.GetType(), data, "miniLevelNames") : null;
		if (names is IEnumerable enumerable)
		{
			foreach (object name in enumerable)
			{
				string miniName = name.ToString();
				minis.Add(miniName);
				targets.Add(SafeInt(() => Call(LevelsControllerType, null, "GetTargetBallAmount", miniName)));
				object mini = Call(LevelsControllerType, null, "GetMiniLevelData", miniName);
				startBalls.Add(mini != null ? SafeInt(() => Member(mini.GetType(), mini, "startBallAmount")) : "?");
			}
		}
		record.Minis = string.Join(", ", minis);
		record.Targets = string.Join(", ", targets);
		record.StartBalls = string.Join(", ", startBalls);
	}

	private static string SafeInt(Func<object> read)
	{
		try
		{
			return Convert.ToInt32(read()).ToString();
		}
		catch (Exception)
		{
			return "?";
		}
	}

	private void WriteReport(int total, int last, bool complete)
	{
		var sb = new StringBuilder();
		sb.AppendLine("# Level sweep report");
		sb.AppendLine();
		sb.AppendLine("Updated " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | live levels " + total + " | swept 1 to " + last
			+ " | " + (complete ? "complete" : "in progress"));
		sb.AppendLine();
		sb.AppendLine("Attempts " + _attemptsTotal + " | restarts " + _restarts + " | Next transitions " + _transitions
			+ " | rotation failures " + _rotationFailures + " | error, exception and assert logs " + _errorLines.Count);
		sb.AppendLine();
		sb.AppendLine("| Level | Entered by | Mini-level(s) | Target | Start balls | Attempts | Outcomes | Won on | Max bucket fill | Max balls | Game s | Rotation in first 2 s | Note |");
		sb.AppendLine("|---|---|---|---|---|---|---|---|---|---|---|---|---|");
		foreach (LevelRecord r in _records)
		{
			sb.AppendLine("| " + r.Level + " | " + r.Entry + " | " + r.Minis + " | " + r.Targets + " | " + r.StartBalls
				+ " | " + r.Outcomes.Count + " | " + string.Join(", ", r.Outcomes)
				+ " | " + (r.WonOnAttempt > 0 ? r.WonOnAttempt.ToString() : "never")
				+ " | " + r.MaxFill + " | " + r.MaxBalls + " | " + r.GameSeconds.ToString("0.0")
				+ " | " + string.Join(", ", r.Rotation) + " | " + r.Note + " |");
		}
		sb.AppendLine();
		sb.AppendLine("## Errors, exceptions and asserts");
		sb.AppendLine();
		if (_errorLines.Count == 0)
		{
			sb.AppendLine("None.");
		}
		foreach (string line in _errorLines)
		{
			sb.AppendLine("```text");
			sb.AppendLine(line);
			sb.AppendLine("```");
		}

		Directory.CreateDirectory(_reportDirectory);
		File.WriteAllText(Path.Combine(_reportDirectory, ReportFileName), sb.ToString());
	}

	private static string ReportDirectory()
	{
		string value = Environment.GetEnvironmentVariable("SWEEP_REPORT_DIR");
		return string.IsNullOrWhiteSpace(value) ? DefaultReportDirectory : value;
	}

	private static int StartLevel()
	{
		string value = Environment.GetEnvironmentVariable("SWEEP_START_LEVEL");
		return int.TryParse(value, out int parsed) && parsed > 0 ? parsed : 1;
	}

	private static string BlockingUiAt(Vector2? point)
	{
		EventSystem eventSystem = EventSystem.current;
		if (!point.HasValue || eventSystem == null)
		{
			return "";
		}
		var pointer = new PointerEventData(eventSystem) { position = point.Value };
		var results = new List<RaycastResult>();
		eventSystem.RaycastAll(pointer, results);
		var names = new List<string>();
		foreach (RaycastResult hit in results)
		{
			if (hit.gameObject == null)
			{
				continue;
			}
			Graphic graphic = hit.gameObject.GetComponent<Graphic>();
			if (graphic != null && graphic.raycastTarget)
			{
				names.Add(hit.gameObject.name);
			}
		}
		return string.Join(", ", names);
	}

	private static int MaxLevels()
	{
		string value = Environment.GetEnvironmentVariable("SWEEP_MAX_LEVELS");
		return int.TryParse(value, out int parsed) && parsed > 0 ? parsed : int.MaxValue;
	}

	private static IEnumerator BootToMain()
	{
		var persisted = Member(BallsControllerType, null, "_instance") as UnityEngine.Object;
		SceneManager.LoadScene(persisted != null ? "Main" : "Loading");

		var wait = new Result();
		yield return Until(() => SceneManager.GetActiveScene().name == "Main", SceneTimeout, wait);
		Assert.That(wait.Ok, Is.True, "Main did not load.");
		yield return Until(LevelReady, FlowTimeout, wait);
		Assert.That(wait.Ok, Is.True, "Level 1 did not spawn.");
	}

	private static bool LevelReady()
	{
		return Instance() != null
			&& Member(LevelsControllerType, null, "MiniLevelView") != null
			&& FirstMiniLevelName() != null;
	}

	private static IEnumerator Until(Func<bool> condition, float seconds, Result result)
	{
		result.Ok = false;
		float deadline = Time.realtimeSinceStartup + seconds;
		while (!condition())
		{
			if (Time.realtimeSinceStartup > deadline)
			{
				yield break;
			}
			yield return null;
		}
		result.Ok = true;
	}

	private static string FirstMiniLevelName()
	{
		object data = Member(LevelsControllerType, null, "CurrentLevelData");
		object names = Member(data.GetType(), data, "miniLevelNames");
		if (names == null)
		{
			return null;
		}
		var list = (List<string>)names;
		return list.Count > 0 ? list[0] : null;
	}

	private static object Instance()
	{
		return Member(GameManagerType, null, "instance");
	}

	private static object FailScreen()
	{
		return Member(GameManagerType, Instance(), "failLevelScreen");
	}

	private static object MainScreen()
	{
		return Member(GameManagerType, Instance(), "mainScreen");
	}

	private static bool IsActive(object o)
	{
		GameObject go = o is GameObject g ? g : (o as Component)?.gameObject;
		return go != null && go.activeSelf;
	}

	private static Component CurrentRotateController()
	{
		object view = Member(LevelsControllerType, null, "MiniLevelView");
		return view == null ? null : Member(MiniLevelViewType, view, "rotateController") as Component;
	}

	private static Button FindButtonCalling(Component root, string methodName)
	{
		Button[] buttons = root != null
			? root.GetComponentsInChildren<Button>(true)
			: UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include);
		foreach (Button button in buttons)
		{
			for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
			{
				if (button.onClick.GetPersistentMethodName(i) == methodName)
				{
					return button;
				}
			}
		}
		return null;
	}

	private static bool TapToStart()
	{
		EventSystem eventSystem = EventSystem.current;
		if (eventSystem == null)
		{
			return false;
		}
		foreach (EventTrigger trigger in UnityEngine.Object.FindObjectsByType<EventTrigger>(FindObjectsInactive.Include))
		{
			foreach (EventTrigger.Entry entry in trigger.triggers)
			{
				if (entry.eventID != EventTriggerType.PointerDown)
				{
					continue;
				}
				for (int i = 0; i < entry.callback.GetPersistentEventCount(); i++)
				{
					if (entry.callback.GetPersistentMethodName(i) == "StartPlayClick")
					{
						entry.callback.Invoke(new PointerEventData(eventSystem));
						return true;
					}
				}
			}
		}
		return false;
	}

	private static IList LeanInstances()
	{
		return LeanTouchType != null ? (IList)Member(LeanTouchType, null, "Instances") : null;
	}

	private static void StepLean(object instance, Vector2? pointer)
	{
		var pointers = new List<KeyValuePair<int, Vector2>>();
		if (pointer.HasValue)
		{
			pointers.Add(new KeyValuePair<int, Vector2>(MouseIndex, pointer.Value));
		}

		MethodInfo method = LeanTouchType.GetMethod("ProcessFrame", AnyInstance);
		Assert.IsNotNull(method, "LeanTouch.ProcessFrame not found");
		method.Invoke(instance, new object[] { pointers, Time.unscaledDeltaTime });
	}

	private static void ClearLeanTouchFingers()
	{
		if (LeanTouchType == null)
		{
			return;
		}
		((IList)Member(LeanTouchType, null, "Fingers")).Clear();
		((IList)Member(LeanTouchType, null, "InactiveFingers")).Clear();
	}

	private static Type GameType(string fullName)
	{
		Type type = Type.GetType(fullName + ", Assembly-CSharp");
		if (type == null)
		{
			throw new TypeLoadException("Game type not found: " + fullName);
		}
		return type;
	}

	private static Type FindType(string fullName)
	{
		foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
		{
			Type t = asm.GetType(fullName, false);
			if (t != null)
			{
				return t;
			}
		}
		return null;
	}

	private static void SetMember(Type type, object target, string name, object value)
	{
		PropertyInfo property = type.GetProperty(name, AnyMember);
		if (property != null)
		{
			property.SetValue(target, value);
			return;
		}

		FieldInfo field = type.GetField(name, AnyMember);
		Assert.IsNotNull(field, "Missing member " + name);
		field.SetValue(target, value);
	}

	private static object Member(Type type, object target, string name)
	{
		PropertyInfo property = type.GetProperty(name, AnyMember);
		if (property != null)
		{
			return property.GetValue(target);
		}

		FieldInfo field = type.GetField(name, AnyMember);
		if (field != null)
		{
			return field.GetValue(target);
		}

		throw new MissingMemberException(type.FullName, name);
	}

	private static object Call(Type type, object target, string name, params object[] args)
	{
		foreach (MethodInfo method in type.GetMethods(AnyMember))
		{
			if (method.Name != name || method.GetParameters().Length != args.Length)
			{
				continue;
			}

			try
			{
				return method.Invoke(target, args);
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException ?? ex;
			}
		}

		throw new MissingMethodException(type.FullName, name);
	}

	private sealed class Result
	{
		public bool Ok;
	}

	private sealed class Tally
	{
		public int Count;

		public int Value;

		public void Add(int value)
		{
			Count++;
			Value += value;
		}

		public string Text
		{
			get { return Count + " (value " + Value + ")"; }
		}
	}

	private sealed class Attempt
	{
		public string Outcome = "TIMEOUT";

		public int MaxFill;

		public int MaxBalls;

		public float GameSeconds;

		public int FramesElapsed;

		public int DragFrames;

		public float RotationDeg = -1f;

		public bool RotationOk;

		public string UiAtStart = "";

		public Quaternion StartRotation = Quaternion.identity;

		public Transform SubTarget;

		public Quaternion SubStart = Quaternion.identity;

		public float SubDeg = -1f;

		public string Diagnostics = "";

		public string TimeoutSummary = "";
	}

	private sealed class LevelRecord
	{
		public int Level;

		public string Entry = "";

		public string Minis = "";

		public string Targets = "";

		public string StartBalls = "";

		public readonly List<string> Outcomes = new List<string>();

		public readonly List<string> Rotation = new List<string>();

		public int WonOnAttempt;

		public int MaxFill;

		public int MaxBalls;

		public float GameSeconds;

		public string Note = "";
	}
}
