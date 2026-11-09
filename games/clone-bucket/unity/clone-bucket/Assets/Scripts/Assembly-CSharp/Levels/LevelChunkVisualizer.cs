using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Levels
{
	public class LevelChunkVisualizer : MonoBehaviour
	{
		private const float BackgroundStep = 0.2f;

		private const float CheckmarkPopScale = 2f;

		private const float CheckmarkPopDuration = 0.3f;

		private const float SettleDuration = 0.2f;

		[FormerlySerializedAs("visuals")]
		[SerializeField]
		private LevelChunkVisual[] _visuals;

		[SerializeField]
		private Image _backgroundImage;

		private void Awake()
		{
			LevelsController.OnLevelSpawned += ActivateVisuals;
			LevelsController.OnChunkAboutToBeCompleted += ActivateCheckmark;
			LevelsController.OnChunkCompleted += ActivateDot;
			GameScreen.OnClickContinue += ActivateCheckmark;
		}

		private void OnDestroy()
		{
			LevelsController.OnLevelSpawned -= ActivateVisuals;
			LevelsController.OnChunkAboutToBeCompleted -= ActivateCheckmark;
			LevelsController.OnChunkCompleted -= ActivateDot;
			GameScreen.OnClickContinue -= ActivateCheckmark;
		}

		internal static float BackgroundPixelsPerUnit(int chunkCount)
		{
			return chunkCount * BackgroundStep + 1f;
		}

		internal static bool IsVisible(int chunkCount)
		{
			return chunkCount > 1;
		}

		private void ActivateVisuals()
		{
			List<string> names = LevelsController.CurrentLevelData.miniLevelNames;
			int count = names != null ? names.Count : 0;
			gameObject.SetActive(IsVisible(count));
			if (_backgroundImage != null)
			{
				_backgroundImage.pixelsPerUnitMultiplier = BackgroundPixelsPerUnit(count);
			}
			if (_visuals == null)
			{
				return;
			}

			for (int i = 0; i < _visuals.Length; i++)
			{
				LevelChunkVisual visual = _visuals[i];
				if (visual == null)
				{
					continue;
				}
				bool used = i < count;
				visual.gameObject.SetActive(used);
				if (!used)
				{
					continue;
				}
				bool boss = LevelsController.GetMiniLevelData(names[i]).isBossLevel;
				SetActive(visual.checkmark, false);
				SetActive(visual.current, false);
				SetScale(visual.checkmark, Vector3.zero);
				SetScale(visual.current, Vector3.zero);
				SetActive(visual.regular, !boss);
				SetActive(visual.final, boss);
			}

			LevelChunkVisual first = VisualAt(0);
			if (first != null)
			{
				SetActive(first.current, true);
				SetScale(first.current, Vector3.one);
			}
		}

		private void ActivateCheckmark()
		{
			LevelChunkVisual visual = VisualAt(LevelsController.MiniLevelIndex);
			if (visual == null || visual.checkmark == null || visual.checkmark.activeSelf)
			{
				return;
			}
			visual.checkmark.SetActive(true);
			Transform mark = visual.checkmark.transform;
			mark.DOScale(Vector3.one * CheckmarkPopScale, CheckmarkPopDuration);
			DOVirtual.DelayedCall(CheckmarkPopDuration, () =>
			{
				if (mark != null)
				{
					mark.DOScale(Vector3.one, SettleDuration);
				}
			});
		}

		private void ActivateDot()
		{
			LevelChunkVisual visual = VisualAt(LevelsController.MiniLevelIndex);
			if (visual == null || visual.current == null)
			{
				return;
			}
			visual.current.SetActive(true);
			visual.current.transform.DOScale(Vector3.one, SettleDuration);
		}

		private LevelChunkVisual VisualAt(int index)
		{
			return _visuals != null && index >= 0 && index < _visuals.Length ? _visuals[index] : null;
		}

		private static void SetActive(GameObject target, bool isActive)
		{
			if (target != null && target.activeSelf != isActive)
			{
				target.SetActive(isActive);
			}
		}

		private static void SetScale(GameObject target, Vector3 scale)
		{
			if (target != null)
			{
				target.transform.localScale = scale;
			}
		}
	}
}
