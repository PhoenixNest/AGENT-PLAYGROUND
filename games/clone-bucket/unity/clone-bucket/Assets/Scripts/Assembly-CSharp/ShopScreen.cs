using System;
using System.Collections.Generic;
using DG.Tweening;
using Levels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopScreen : MonoBehaviour
{
	[SerializeField]
	private ShopConfig[] _configs;

	[NonSerialized]
	private ShopConfig _config;

	[SerializeField]
	private List<ShopItem> _items;

	public GameObject UnlockBtn;

	public GameObject NotAvaliableUnlockBtn;

	public TextMeshProUGUI NotAvaliableUnlockBtnText;

	public TextMeshProUGUI GemsCounter;

	public GameObject comingsoon;

	public Image GemCustomImage;

	public GemAnimationEffect gemAnimationEffect;

	private ShopItem _currentItem;

	private readonly List<int> _lockedItems = new List<int>();

	private bool _started;

	private bool _pageLock;

	private bool _isUnlocking;

	private int _unlockItemCount;

	private Sequence _notAvailableSequence;

	private void Awake()
	{
		_config = SkinManager.PickVariant(_configs, SkinManager.ParseVariant(RemoteConfigHelper.GetString(SkinManager.ShopVariantKey, "0")));
	}

	private void Start()
	{
	}

	private void OnEnable()
	{
		InitData();
		OpenShop();
	}

	private void OnDisable()
	{
		DOTween.Kill(this);
		_notAvailableSequence?.Kill();
	}

	private void HandleGameMusic(bool pauseMusic)
	{
	}

	public void InitData()
	{
		_started = true;
		_unlockItemCount = 0;
		_lockedItems.Clear();

		List<SkinBallItem> skins = _config != null ? _config.balls : null;
		int skinCount = skins != null ? skins.Count : 0;
		if (_items == null)
		{
			return;
		}

		string selected = PlayerPrefs.GetString(SkinManager.SelectedSkinKey, string.Empty);
		for (int i = 0; i < _items.Count; i++)
		{
			ShopItem item = _items[i];
			if (item == null)
			{
				continue;
			}

			bool hasSkin = i < skinCount && skins[i] != null;
			item.gameObject.SetActive(hasSkin);
			if (!hasSkin)
			{
				continue;
			}

			SkinBallItem skin = skins[i];
			item.ItemName = skin.Name;
			if (item.ballImg != null)
			{
				item.ballImg.sprite = skin.icon;
			}

			item.OnClicked = OnSelect;

			if (skin.Name == selected)
			{
				ChangeSelector(item);
			}

			bool owned = PlayerPrefs.GetInt(skin.Name, 0) == 1;
			if (owned)
			{
				_unlockItemCount++;
			}
			else
			{
				_lockedItems.Add(i);
			}

			item.SetLocked(!owned);
		}
	}

	private void OpenShop()
	{
		SetGems(CurrentGems());
	}

	private void UpdateButtons()
	{
		int total = _config != null ? _config.balls?.Count ?? 0 : 0;
		bool allUnlocked = _unlockItemCount >= total && total > 0;
		bool canAfford = CurrentGems() >= PriceOrZero();

		if (comingsoon != null)
		{
			comingsoon.SetActive(allUnlocked);
		}

		if (UnlockBtn != null)
		{
			UnlockBtn.SetActive(!allUnlocked && canAfford);
		}

		if (NotAvaliableUnlockBtn != null)
		{
			NotAvaliableUnlockBtn.SetActive(!allUnlocked && !canAfford);
		}
	}

	public void Close()
	{
		AudioManager.instance?.PlayButtonClick();
		gameObject.SetActive(false);
	}

	public void UnlockRandom()
	{
		if (_pageLock || _isUnlocking || _config == null || GameManager.instance == null || _lockedItems.Count == 0)
		{
			return;
		}

		int price = _config.price;
		if (GameManager.instance.GemCounter < price)
		{
			NotAbaliableUnlockBtnClick();
			return;
		}

		_isUnlocking = true;
		_pageLock = true;

		int pick = UnityEngine.Random.Range(0, _lockedItems.Count);
		int cardIndex = _lockedItems[pick];
		_lockedItems.RemoveAt(pick);

		SkinBallItem skin = cardIndex < _config.balls.Count ? _config.balls[cardIndex] : null;
		if (skin != null)
		{
			GameManager.instance.HandleBeforeSpend(price);
			GameManager.instance.GemCounter -= price;
			UnlockSkin(skin, true);
		}
		else
		{
			_lockedItems.Add(cardIndex);
		}

		_isUnlocking = false;
		_pageLock = false;
	}

	private void HideAllSelections()
	{
		if (_items == null)
		{
			return;
		}

		for (int i = 0; i < _items.Count; i++)
		{
			if (_items[i] != null)
			{
				_items[i].SetSelected(false);
			}
		}
	}

	private void ChangeSelector(ShopItem item)
	{
		HideAllSelections();
		_currentItem = item;
		if (item != null)
		{
			item.SetSelected(true, true);
		}
	}

	public void OnSelect(ShopItem shopItem)
	{
		if (shopItem == null || SkinManager.instance == null || !SkinManager.instance.IsOwned(shopItem.ItemName))
		{
			return;
		}

		ChangeSelector(shopItem);

		SkinBallItem skin = SkinManager.instance.GetBall(shopItem.ItemName);
		if (skin == null || GameManager.instance == null)
		{
			return;
		}

		PlayerPrefs.SetString(SkinManager.SelectedSkinKey, shopItem.ItemName);
		GameManager.instance.CurrentBallSkin = skin;
		LevelsController.RequestSpawnInitialBalls();
	}

	public void NotAbaliableUnlockBtnClick()
	{
		if (NotAvaliableUnlockBtnText != null)
		{
			NotAvaliableUnlockBtnText.text = PriceOrZero().ToString();
		}
	}

	public void SetGems(int value)
	{
		if (GemsCounter != null)
		{
			GemsCounter.text = UIUtils.ToSI(value);
		}

		UpdateButtons();
	}

	public void CarsTabClick()
	{
	}

	private void UnlockSkin(SkinBallItem skin, bool withGemEffect)
	{
		if (SkinManager.instance == null || !SkinManager.instance.MarkOwned(skin.Name))
		{
			return;
		}

		ShopItem card = FindCard(skin.Name);
		if (card != null)
		{
			card.PlayUnlockAnim();
			ChangeSelector(card);
		}

		if (withGemEffect)
		{
			if (gemAnimationEffect != null)
			{
				gemAnimationEffect.Play();
			}

			AudioManager.instance?.PlayGem();
		}

		_unlockItemCount++;

		if (GameManager.instance != null)
		{
			GameManager.instance.SaveData(GameManager.instance.CurrentLevel);
		}

		SetGems(CurrentGems());
	}

	private ShopItem FindCard(string skinName)
	{
		if (_items == null)
		{
			return null;
		}

		for (int i = 0; i < _items.Count; i++)
		{
			if (_items[i] != null && _items[i].ItemName == skinName)
			{
				return _items[i];
			}
		}

		return null;
	}

	private static int CurrentGems()
	{
		return GameManager.instance != null ? GameManager.instance.GemCounter : 0;
	}

	private int PriceOrZero()
	{
		return _config != null ? _config.price : 0;
	}
}
