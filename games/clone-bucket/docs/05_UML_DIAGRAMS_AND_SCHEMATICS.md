# 05. UML Diagrams and Schematics

Diagrams model the **recovered** classes of Multi Maze 3D v4.1.0.0 (Unity 2022.3.62f2, built-in RP). Class and field names are real. Method bodies are not recoverable, so behaviour in sequence diagrams is tagged.

Tags: **[Recovered]** (from script or asset), **[Inferred]** (from names or structure), **[Unknown]** (logic to be designed or tuned).

Rebuild target: pending engine decision (2022.3 built-in vs Unity 6 URP/built-in) — see README §5.

---

## 1. Class Diagram: Level and Balls

[Recovered] Fields and signatures from `Levels/`, `Balls/`, root gate and chain scripts.

```mermaid
classDiagram
    direction LR

    class LevelsController {
        <<MonoBehaviour>>
        -LevelsConfig _config
        -IAdsService _adsService
        +Spawn(int playerProgressIndex)$ void
        +OnFillBucket(bool filledBefore, int amount)$ void
        +HandleMissionFail()$ void
        +StartNextChunk()$ void
        +UpdateStars(int perfect, int target)$ void
        +SetDynamicBallGoals(string miniLevelName, int maxPossibleBalls)$ void
        +TotalFillCount int$
        +FilledStarCount int$
        +Completed bool$
    }

    class LevelsConfig {
        <<ScriptableObject>>
        +MiniLevelView jsonBaseLevelPrefab
        +LevelView prefab
        +Vector3[] ballSpawnPositions
        +Vector3 distanceBetweenMiniLevels
        +float startBallAmountMultiplierOnContinue
        +float ballGoalPercentTarget
        +float ballGoalPercentPerfect
    }

    class LevelData {
        <<struct>>
        +List~string~ miniLevelNames
        +bool isLooping
        +int levelDifficulty
        +string backgroundMaterialName
    }

    class MiniLevelData {
        <<struct>>
        +int missionIndex
        +string levelName
        +int startBallAmount
        +int targetBallAmount
        +int perfectBallAmount
        +bool isBossLevel
        +LevelType levelType
        +List~GateType~ gateTypes
        +List~int~ gateValues
        +string mazeMaterialName
    }

    class LevelType {
        <<enumeration>>
        Prefab = 0
        Json = 1
    }

    class BallsPool {
        <<static>>
        -BallsConfig _config
        -ObjectPool~BallView~ _ballPool
        +Initialize(BallsConfig, Transform)$ void
        +PreWarm(int amount)$ void
        +Get()$ BallView
        +Release(BallView view)$ void
        +OnBallInstantiated event
        +OnBallAboutToBeDestroyed event
    }

    class BallsConfig {
        <<ScriptableObject>>
        +BallView prefab
        +float inMazeMaxSpeed
        +float outMazeMaxSpeed
        +int initialPoolAmount
        +int ignoreMultiplicatorCapacity
        +bool useBallLayers
        +int[] ballLayers
        +int[] minimumBallsForLayers
        +bool useBallValueTiers
        +int[] valuePerTier
        +int[] totalBallsForNextTier
        +float[] chancesToOverrideTier
    }

    class BallsController {
        <<MonoBehaviour>>
        -BallsConfig _androidConfig
        -BallsConfig _iosConfig
        -Dictionary _ignoredMultiplicatorsByBalls
        -Dictionary _dataByBalls
        +Spawn(Vector3, Transform)$ BallView
        +Divide(BallView)$ void
        +Despawn(BallView) void
        +SendAllToPool()$ void
        +GetBallValue(BallView)$ int
    }

    class BallView {
        <<MonoBehaviour>>
        +Rigidbody rigidbodyCached
        +Collider colliderCached
        +MeshRenderer meshRendererCached
        +Transform skinRotator
        +OnTriggerEntered event
        +OnTriggerExited event
    }

    class BallData {
        <<class>>
        +float maxSpeed
        +bool isActive
        +bool inBucket
        +bool inFinishSpace
        +bool inMaze
        +bool divisionStatus
        +int value
    }

    class BallSize {
        <<enumeration>>
        Small = 0
        Medium = 1
        Big = 2
    }

    LevelsController --> LevelsConfig : serialized _config
    LevelsController ..> LevelData : builds from table
    LevelsController ..> MiniLevelData : per mini-level
    MiniLevelData --> LevelType
    LevelsController ..> BallsController : spawns start balls
    BallsController --> BallsConfig : Android / iOS
    BallsController --> BallData : per ball
    BallsController --> BallView : tracks
    BallsPool --> BallsConfig
    BallsPool --> BallView : ObjectPool
    BallsController ..> BallsPool : Get / Release [Inferred]
    BallData --> BallSize : [Inferred] size class
```

Notes:
- `BallsPool` is a **static** class (not a `MonoBehaviour`), backed by `UnityEngine.Pool.ObjectPool<BallView>`.
- `BallsController.Get` / `Release` edges are [Inferred]: `BallsPool` exposes them, the caller is not confirmed.
- `BallSize` is linked to `BallData` as [Inferred]. The field `BallData` does not show a size field.

---

## 2. Class Diagram: Gates, Chains, Bucket, Maze

[Recovered] Fields and signatures from root scripts and `Home/.../Mazes/`.

```mermaid
classDiagram
    direction TB

    class GateType {
        <<enumeration>>
        Multiply = 0
        Subtract = 1
        Divide = 2
    }

    class GateBase {
        <<abstract MonoBehaviour>>
        +bool isMoveX
        +float MoveTime
        +bool stopped
        +float startPosX
        +GateType Type*
        +int Value*
        +SetValue(int)* void
        +Move() void
        +StopMove() void
    }

    class Multiplicator {
        +TextMeshPro Text
        +MeshRenderer visualMeshRenderer
        +MultiEffect() void
    }

    class SubGate {
        +int startValue
        +TextMeshPro ValueText
        +BoxCollider collider
        +Sub() bool
        +ResetGate() void
    }

    class DivideGate {
        -int ballsCounter
        -int divideEffectTargetCount
        -int divideEffectCurrentCount
        +Divide(BallView ball) void
    }

    class GatesController {
        <<MonoBehaviour>>
        -GatesConfig _config
        +SpawnGate(GateType, Vector3, Quaternion, Vector3, Transform) GateBase
        +GetMultiplicator(Transform)$ Multiplicator
        +GetSubGate(Transform)$ SubGate
        +GetDivideGate(Transform)$ DivideGate
    }

    class GatesConfig {
        <<ScriptableObject>>
        +SubGate substractionGatePrefab
        +Multiplicator multiplicationGatePrefab
        +DivideGate dividerGatePrefab
    }

    class MazeGateDefinition {
        <<struct>>
        +float positionX
        +float positionY
        +float rotationX
        +float rotationY
        +float rotationZ
        +string type
        +int value
        +bool isMovable
        +float moveTime
        +float size
        +GetLoopPool() List
    }

    class MazeGateTypeConfig {
        <<Serializable>>
        +GameObject prefab
        +float spawnChance
        +int minimumCount
        +int maximumCount
        +int valueMin
        +int valueMax
        +int valueStep
    }

    class Chain {
        <<MonoBehaviour>>
        +int UnlockNumber
        +TextMeshProUGUI CounterText
        +GameObject P_Lock
        +int ChainCount
        +ChainUpdate() void
        +ResetChain() void
    }

    class ChainTrigger {
        <<MonoBehaviour>>
        +Chain chain
        +OnTriggerEnter(Collider) void
        +OnTriggerExit(Collider) void
    }

    class ChainSegment {
        <<MonoBehaviour>>
        -Quaternion rotation
    }

    class Bucket {
        <<MonoBehaviour>>
        +int CurrentFillCount
        +GameObject DestroyTrigger
        +GameObject FillTrigger
        +OnFill(bool filledBefore, int amount) void
        +OnOut() void
        +Shake() void
    }

    class RotateController {
        <<MonoBehaviour>>
        +GameObject Target
        +Vector3 Axis
        +float Multiplier
        +float Damping
        +bool ScaleByTime
        +RotateAB() void
        +RotateToDefault() void
    }

    class MazeRotator {
        <<MonoBehaviour>>
        +string id
        +Transform transformCached
    }

    GateBase <|-- Multiplicator
    GateBase <|-- SubGate
    GateBase <|-- DivideGate
    GateBase --> GateType
    GatesController --> GatesConfig
    GatesController ..> GateBase : spawns
    GatesController ..> MazeGateDefinition : [Inferred] from level data
    MazeGateTypeConfig ..> GateBase : [Inferred] prefab source
    ChainTrigger --> Chain
    Chain ..> ChainSegment : [Inferred] segments of the chain
    Bucket ..> BallsController : tags BucketFill / BucketDestroy
    RotateController ..> MazeRotator : [Inferred] rotates maze
```

Notes:
- `GateBase` is abstract and has no tag of its own. The tags `Multiplicator`, `SubGate`, `DivideGate` are set on the prefabs and checked by `BallsController`.
- `MazeGateTypeConfig` is a generator config (spawn chance and count ranges). Its link to `GateBase` is [Inferred].
- `Chain` and `ChainSegment` link is [Inferred] from names only.

---

## 3. Sequence: Level Load → Play → Complete

Tags on each step. Step 4 onward is mostly [Unknown] because bodies are stubbed.

```mermaid
sequenceDiagram
    autonumber
    participant RCH as RemoteConfigHelper
    participant DB as DatabaseController
    participant LC as LevelsController
    participant BP as BallsPool
    participant BC as BallsController
    participant RC as RotateController
    participant GC as GatesController
    participant CH as ChainTrigger / Chain
    participant BK as Bucket
    participant GM as GameManager

    RCH->>DB: remote feature LevelFunnel (or bundled LevelsData.txt) [Recovered: feature name, file]
    DB-->>LC: LevelsData dataset rows [Recovered: DataSetId "LevelsData"]
    LC->>LC: Spawn(playerProgressIndex) builds MiniLevelData [Recovered signature, logic Unknown]
    LC->>BC: spawn start balls (startBallAmount) [Inferred]
    BC->>BP: Get() / PreWarm() [Recovered methods, call site Inferred]
    BP-->>BC: BallView

    loop Play
        RC->>RC: drag -> Multiplier 3.5, Damping 16 [Recovered values]
        Note over BC,GC: ball enters gate trigger (tag Multiplicator / SubGate / DivideGate)
        BC->>GC: GetMultiplicator / GetSubGate / GetDivideGate [Recovered signatures]
        GC-->>BC: gate
        BC->>BC: Multiply(view, gate, collider, parent) [Recovered signature, formula Unknown]
        BC->>BC: _ignoredMultiplicatorsByBalls guard [Recovered field]
        CH->>CH: ChainTrigger ball enter -> chain counter [Unknown behaviour]
    end

    BC->>BK: ball enters BucketFillTrigger [Recovered tag]
    BK->>LC: OnFillBucket(filledBefore, amount) [Recovered signature]
    LC->>LC: TotalFillCount / FilledStarCount update [Recovered fields, rule Unknown]
    LC->>GM: CompleteLevel() or HandleMissionFail() [Recovered methods, trigger Inferred]

    alt Completed
        GM->>GM: DelayedNextLevel(withCoinsRV) [Recovered signature]
        GM->>GM: NextLevel() [Recovered]
    else Failed
        GM->>GM: OpenFailWindow() [Recovered]
    end
```

---

## 4. Sequence: Multiplier Gate (Inferred Flow)

Entire flow is [Inferred] from field and method names. Counts and values are [Unknown].

```mermaid
sequenceDiagram
    autonumber
    participant Ball as BallView
    participant BC as BallsController
    participant Mult as Multiplicator
    participant BP as BallsPool

    Ball->>BC: OnBallTriggerEntered(view, other) [Recovered name]
    BC->>BC: check _ignoredMultiplicatorsByBalls[view] [Recovered field]
    alt not yet ignored for this collider
        BC->>Mult: read Value [Recovered property, value Unknown]
        loop Value - 1 extra balls [Inferred]
            BC->>BP: Get() [Inferred]
            BP-->>BC: BallView (clone)
            BC->>BC: Multiply() applies tier / cap rules [Unknown]
        end
        BC->>BC: add collider to ignore list [Recovered field]
    end
    Mult->>Mult: MultiEffect() [Recovered name, effect Inferred]
```

---

## 5. Sequence: Bucket Fill and Overfill

```mermaid
sequenceDiagram
    autonumber
    participant Ball as BallView
    participant BK as Bucket
    participant LC as LevelsController
    participant FX as BucketCollectFX (LeanPool)

    Ball->>BK: FillTrigger (tag BucketFillTrigger) [Recovered tag]
    BK->>BK: OnFill(filledBefore, amount) [Recovered signature]
    BK->>FX: play collect effect [Inferred, LeanPool used in BucketCollectFX.cs]
    BK->>BK: OverFillCheck() [Recovered name, threshold Unknown]
    BK->>LC: OnFillBucket(filledBefore, amount) [Recovered signature]
    Ball->>BK: DestroyTrigger (tag BucketDestroyTrigger) [Recovered tag]
    BK->>BK: OnOut() [Recovered name, effect Unknown]
```

---

## 6. Open Items

- [Unknown] All method bodies: multiply, subtract, divide arithmetic; tier override maths; star and win thresholds; chain unlock rule.
- [Unknown] Remote key to `ABTestVariables` flag mapping.
- Next step: IL2CPP builds contain no IL. To move sections 3–5 from [Inferred] to [Recovered], disassemble the relevant native methods in `libil2cpp.so` (e.g. Ghidra with Cpp2IL / Il2CppDumper symbol maps), or confirm behaviour by playtesting the shipped app.
