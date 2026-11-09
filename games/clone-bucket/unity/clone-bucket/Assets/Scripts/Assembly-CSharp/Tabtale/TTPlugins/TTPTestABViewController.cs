using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tabtale.TTPlugins
{
	public class TTPTestABViewController : MonoBehaviour
	{
		private string _selectedExperiment;

		private string _selectedVariant;

		private readonly Dictionary<string, GameObject> _buttons;

		private GameObject _panelConsole;

		private GameObject _buttonBack;

		private GameObject _buttonHide;

		private GameObject _buttonSet;

		private void Start()
		{
		}

		public void ReloadData()
		{
		}

		public void Show()
		{
		}

		private void OnBackClicked()
		{
		}

		private void OnMinimizeConsoleClicked()
		{
		}

		private void ShowAllExperiments()
		{
		}

		private void ShowAllVariants(string[] variantNames)
		{
		}

		private void ShowButtonsList(string[] configurationNames, Action<string> action)
		{
		}

		private void SetActive(string experimentName)
		{
		}
	}
}
