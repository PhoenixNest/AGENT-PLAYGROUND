using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CustomImage : MonoBehaviour
{
	[Serializable]
	public class ImageInfo
	{
		public string name;

		public Sprite sprite;

		public ImageInfo()
		{
		}

		public ImageInfo(string imageName, Sprite imageSprite)
		{
			name = imageName;
			sprite = imageSprite;
		}
	}

	[SerializeField]
	[Header("图片设置")]
	private ImageInfo[] images;

	[SerializeField]
	private int defaultImageIndex;

	[Header("事件（可选）")]
	public UnityEvent<int> OnImageChanged;

	public UnityEvent<string> OnImageChangedByName;

	private Image targetImage;

	private int currentIndex;

	public int CurrentIndex => currentIndex;

	public string CurrentImageName => IsValidIndex(currentIndex) ? images[currentIndex].name : null;

	public Sprite CurrentSprite => IsValidIndex(currentIndex) ? images[currentIndex].sprite : null;

	public int ImageCount => images != null ? images.Length : 0;

	private void Awake()
	{
		targetImage = GetComponent<Image>();
	}

	private void Start()
	{
		if (IsValidIndex(defaultImageIndex))
		{
			SetImageByIndex(defaultImageIndex);
		}
	}

	public void SetImageByIndex(int index)
	{
		if (!IsValidIndex(index))
		{
			return;
		}
		currentIndex = index;
		ApplySprite(images[index].sprite);
		if (OnImageChanged != null)
		{
			OnImageChanged.Invoke(index);
		}
		if (OnImageChangedByName != null)
		{
			OnImageChangedByName.Invoke(images[index].name);
		}
	}

	public void SetImageByName(string imageName)
	{
		int index = FindImageIndexByName(imageName);
		if (index >= 0)
		{
			SetImageByIndex(index);
		}
	}

	public void SetSpriteByName(string name, Sprite s)
	{
		int index = FindImageIndexByName(name);
		if (index < 0)
		{
			return;
		}
		images[index].sprite = s;
		if (index == currentIndex)
		{
			ApplySprite(s);
		}
	}

	public void SetSprite(Sprite sprite)
	{
		ApplySprite(sprite);
	}

	public void NextImage(bool loop = true)
	{
		if (ImageCount == 0)
		{
			return;
		}
		int next = currentIndex + 1;
		if (next >= ImageCount)
		{
			if (!loop)
			{
				return;
			}
			next = 0;
		}
		SetImageByIndex(next);
	}

	public void PreviousImage(bool loop = true)
	{
		if (ImageCount == 0)
		{
			return;
		}
		int previous = currentIndex - 1;
		if (previous < 0)
		{
			if (!loop)
			{
				return;
			}
			previous = ImageCount - 1;
		}
		SetImageByIndex(previous);
	}

	public void RandomImage()
	{
		if (ImageCount == 0)
		{
			return;
		}
		SetImageByIndex(UnityEngine.Random.Range(0, ImageCount));
	}

	private bool IsValidIndex(int index)
	{
		return images != null && index >= 0 && index < images.Length;
	}

	private int FindImageIndexByName(string imageName)
	{
		if (images == null || imageName == null)
		{
			return -1;
		}
		for (int i = 0; i < images.Length; i++)
		{
			if (images[i] != null && images[i].name == imageName)
			{
				return i;
			}
		}
		return -1;
	}

	public string[] GetAllImageNames()
	{
		if (images == null)
		{
			return new string[0];
		}
		var names = new string[images.Length];
		for (int i = 0; i < images.Length; i++)
		{
			names[i] = images[i] != null ? images[i].name : null;
		}
		return names;
	}

	public ImageInfo GetImageInfo(int index)
	{
		return IsValidIndex(index) ? images[index] : null;
	}

	public void AddImage(string name, Sprite sprite)
	{
		var list = images != null ? new List<ImageInfo>(images) : new List<ImageInfo>();
		list.Add(new ImageInfo(name, sprite));
		images = list.ToArray();
	}

	public void RemoveImageByIndex(int index)
	{
		if (!IsValidIndex(index))
		{
			return;
		}
		var list = new List<ImageInfo>(images);
		list.RemoveAt(index);
		images = list.ToArray();
		if (currentIndex > index)
		{
			currentIndex--;
		}
		else if (currentIndex == index)
		{
			currentIndex = -1;
		}
	}

	private void ApplySprite(Sprite sprite)
	{
		if (targetImage == null)
		{
			targetImage = GetComponent<Image>();
		}
		if (targetImage != null)
		{
			targetImage.sprite = sprite;
		}
	}
}
