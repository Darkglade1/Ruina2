using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Debug;
using MegaCrit.Sts2.Core.Nodes.Ftue;

namespace Ruina2.Ruina2Code.Ftue;

public partial class NAllyFtue : NFtue
{
	private const string LocTable = "ftues";

	private const float BaseWidth = 1920f;

	private const float BaseHeight = 1080f;

	private static readonly Vector2 ImageAnimOffset = new Vector2(200f, 0f);

	private readonly string _ftueId;

	private readonly string[] _titleKeys;

	private readonly string[] _bodyKeys;

	private readonly string[] _imagePaths;

	private readonly int _totalPages;

	private int _currentPage;

	private Control? _contentRoot;

	private TextureRect? _image;

	private MegaRichTextLabel? _bodyText;

	private MegaLabel? _header;

	private MegaLabel? _pageCount;

	private TextureButton? _prevButton;

	private TextureButton? _nextButton;

	private Tween? _pageTurnTween;

	private Vector2 _imagePosition;

	private Vector2 _textPosition;

	public static NAllyFtue Create(string ftueId, string[] titleKeys, string[] bodyKeys, string[] imagePaths)
	{
		return new NAllyFtue(ftueId, titleKeys, bodyKeys, imagePaths);
	}

	private NAllyFtue(string ftueId, string[] titleKeys, string[] bodyKeys, string[] imagePaths)
	{
		_ftueId = ftueId;
		_titleKeys = ((titleKeys.Length != 0) ? titleKeys : new string[1] { ftueId });
		_bodyKeys = ((bodyKeys.Length != 0) ? bodyKeys : new string[1] { ftueId });
		_imagePaths = ((imagePaths.Length != 0) ? imagePaths : new string[1] { ftueId });
		_totalPages = Math.Max(1, _bodyKeys.Length);
	}

	public override void _Ready()
	{
		Name = "NAllyFtue";
		SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		MouseFilter = MouseFilterEnum.Stop;
		BuildUi();
		UpdateLayoutScale();
		ShowPage(0);
	}

	public override void _Process(double delta)
	{
		UpdateLayoutScale();
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (!IsVisibleInTree() || NDevConsole.Instance.Visible)
		{
			return;
		}
		Control control = GetViewport().GuiGetFocusOwner();
		if ((!(control is TextEdit) && !(control is LineEdit)))
		{
			if (inputEvent.IsActionPressed(MegaInput.left) && _currentPage > 0)
			{
				ShowPage(_currentPage - 1, -1);
				GetViewport().SetInputAsHandled();
			}
			else if (inputEvent.IsActionPressed(MegaInput.right) || inputEvent.IsActionPressed(MegaInput.confirm))
			{
				AdvanceOrClose();
				GetViewport().SetInputAsHandled();
			}
		}
	}

	private void BuildUi()
	{
		_contentRoot = new Control
		{
			Name = "ContentRoot",
			Size = new Vector2(BaseWidth, BaseHeight),
			MouseFilter = MouseFilterEnum.Ignore
		};
		AddChild(_contentRoot, forceReadableName: false);
		_image = new TextureRect
		{
			Name = "Image",
			Position = new Vector2(292f, 252f),
			Size = new Vector2(671f, 512f),
			CustomMinimumSize = new Vector2(671f, 512f),
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			StretchMode = TextureRect.StretchModeEnum.Scale,
			MouseFilter = MouseFilterEnum.Ignore
		};
		_contentRoot.AddChild(_image, forceReadableName: false);
		_imagePosition = _image.Position;
		_bodyText = CreateRichTextLabel("Description", 28);
		_bodyText.Position = new Vector2(1005f, 271f);
		_bodyText.Size = new Vector2(623f, 483f);
		_bodyText.CustomMinimumSize = new Vector2(623f, 483f);
		_bodyText.VerticalAlignment = VerticalAlignment.Center;
		_bodyText.AddThemeConstantOverride("line_separation", -2);
		_contentRoot.AddChild(_bodyText, forceReadableName: false);
		_textPosition = _bodyText.Position;
		_header = CreateLabel("Header", 28, new Color(0.937255f, 0.784314f, 0.317647f));
		_header.Position = new Vector2(360f, 821f);
		_header.Size = new Vector2(1200f, 60f);
		_header.CustomMinimumSize = new Vector2(1200f, 60f);
		_contentRoot.AddChild(_header, forceReadableName: false);
		_pageCount = CreateLabel("PageCount", 24, new Color(0.529412f, 0.807843f, 0.921569f));
		_pageCount.Position = new Vector2(360f, 854f);
		_pageCount.Size = new Vector2(1200f, 60f);
		_pageCount.CustomMinimumSize = new Vector2(1200f, 60f);
		_contentRoot.AddChild(_pageCount, forceReadableName: false);
		_prevButton = CreateArrowButton("LeftArrow", "res://images/packed/common_ui/settings_tiny_left_arrow.png");
		_prevButton.Position = new Vector2(40f, 476f);
		_contentRoot.AddChild(_prevButton, forceReadableName: false);
		_prevButton.Pressed += delegate
		{
			ShowPage(_currentPage - 1, -1);
		};
		_nextButton = CreateArrowButton("RightArrow", "res://images/packed/common_ui/settings_tiny_right_arrow.png");
		_nextButton.Position = new Vector2(1752f, 476f);
		_contentRoot.AddChild(_nextButton, forceReadableName: false);
		_nextButton.Pressed += AdvanceOrClose;
	}

	private void ShowPage(int pageIndex, int direction = 0)
	{
		pageIndex = Math.Clamp(pageIndex, 0, _totalPages - 1);
		_currentPage = pageIndex;
		string text = ((pageIndex < _titleKeys.Length) ? _titleKeys[pageIndex] : _titleKeys[0]);
		string text2 = ((pageIndex < _bodyKeys.Length) ? _bodyKeys[pageIndex] : _bodyKeys[^1]);
		string path = ((pageIndex < _imagePaths.Length) ? _imagePaths[pageIndex] : _imagePaths[^1]);
		if (_header != null && _bodyText != null && _pageCount != null && _image != null && _prevButton != null &&
		    _nextButton != null)
		{
			_header.Text = LocString.GetIfExists(LocTable, text)?.GetFormattedText() ?? text;
			_bodyText.Text = LocString.GetIfExists(LocTable, text2)?.GetFormattedText() ?? text2;
			_pageCount.Text = $"({_currentPage + 1}/{_totalPages})";
			_image.Texture = PreloadManager.Cache.GetAsset<Texture2D>(path);
			_prevButton.Visible = _currentPage > 0;
			_nextButton.Visible = true;
		}
		AnimatePage(direction);
	}

	private void AdvanceOrClose()
	{
		if (_currentPage >= _totalPages - 1)
		{
			_pageTurnTween?.Kill();
			CloseFtue();
		}
		else
		{
			ShowPage(_currentPage + 1, 1);
		}
	}

	private void AnimatePage(int direction)
	{
		_pageTurnTween?.Kill();
		_pageTurnTween = CreateTween().SetParallel();
		if (_image != null)
		{
			_image.Modulate = new Color(1f, 1f, 1f, 0f);
			_pageTurnTween.TweenProperty(_image, "modulate:a", 1f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
			if (direction != 0)
			{
				_pageTurnTween.TweenProperty(_image, "position", _imagePosition, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
					.From(_imagePosition + ImageAnimOffset * direction);
			}
		}
		if (_bodyText != null)
		{
			_bodyText.Modulate = new Color(1f, 1f, 1f, 0f);
			_pageTurnTween.TweenProperty(_bodyText, "modulate:a", 1f, 0.6).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Linear);
			_pageTurnTween.TweenProperty(_bodyText, "visible_ratio", 1f, 0.6).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine)
				.From(0f);
			if (direction != 0)
			{
				_pageTurnTween.TweenProperty(_bodyText, "position", _textPosition, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
					.From(_textPosition + ImageAnimOffset * direction);
			}
		}
	}

	private void UpdateLayoutScale()
	{
		if (_contentRoot != null)
		{
			Vector2 size = GetViewportRect().Size;
			float num = Mathf.Min(size.X / BaseWidth, size.Y / BaseHeight);
			_contentRoot.Scale = Vector2.One * num;
			_contentRoot.Position = (size - new Vector2(BaseWidth, BaseHeight) * num) * 0.5f;
		}
	}

	private static MegaLabel CreateLabel(string name, int fontSize, Color fontColor)
	{
		MegaLabel megaLabel = new MegaLabel
		{
			Name = name,
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
			AutoSizeEnabled = false,
			MouseFilter = MouseFilterEnum.Ignore
		};
		megaLabel.AddThemeFontOverride("font", PreloadManager.Cache.GetAsset<Font>("res://themes/kreon_regular_glyph_space_one.tres"));
		megaLabel.AddThemeFontSizeOverride("font_size", fontSize);
		megaLabel.AddThemeColorOverride("font_color", fontColor);
		megaLabel.AddThemeColorOverride("font_shadow_color", new Color(0f, 0f, 0f, 0.5f));
		megaLabel.AddThemeConstantOverride("shadow_offset_x", 3);
		megaLabel.AddThemeConstantOverride("shadow_offset_y", 2);
		return megaLabel;
	}

	private static MegaRichTextLabel CreateRichTextLabel(string name, int fontSize)
	{
		MegaRichTextLabel megaRichTextLabel = new MegaRichTextLabel
		{
			Name = name,
			AutoSizeEnabled = false,
			BbcodeEnabled = true,
			ScrollActive = false,
			MouseFilter = MouseFilterEnum.Ignore,
			FocusMode = FocusModeEnum.None,
			VisibleCharactersBehavior = TextServer.VisibleCharactersBehavior.CharsAfterShaping
		};
		megaRichTextLabel.AddThemeFontOverride("normal_font", PreloadManager.Cache.GetAsset<Font>("res://themes/kreon_regular_glyph_space_one.tres"));
		megaRichTextLabel.AddThemeFontOverride("bold_font", PreloadManager.Cache.GetAsset<Font>("res://themes/kreon_bold_glyph_space_one.tres"));
		megaRichTextLabel.AddThemeColorOverride("default_color", new Color(1f, 0.964706f, 0.886275f));
		megaRichTextLabel.AddThemeColorOverride("font_shadow_color", new Color(0f, 0f, 0f, 0.5f));
		megaRichTextLabel.AddThemeConstantOverride("shadow_offset_x", 3);
		megaRichTextLabel.AddThemeConstantOverride("shadow_offset_y", 2);
		string[] array = ["normal_font_size", "bold_font_size", "bold_italics_font_size", "italics_font_size", "mono_font_size"];
		string[] array2 = array;
		foreach (string text in array2)
		{
			megaRichTextLabel.AddThemeFontSizeOverride(text, fontSize);
		}
		return megaRichTextLabel;
	}

	private static TextureButton CreateArrowButton(string name, string texturePath)
	{
		TextureButton textureButton = new TextureButton
		{
			Name = name,
			Size = new Vector2(128f, 128f),
			CustomMinimumSize = new Vector2(128f, 128f),
			TextureNormal = PreloadManager.Cache.GetAsset<Texture2D>(texturePath),
			TextureHover = PreloadManager.Cache.GetAsset<Texture2D>(texturePath),
			TexturePressed = PreloadManager.Cache.GetAsset<Texture2D>(texturePath),
			TextureDisabled = PreloadManager.Cache.GetAsset<Texture2D>(texturePath),
			StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered,
			FocusMode = FocusModeEnum.All,
			MouseFilter = MouseFilterEnum.Stop,
			MouseDefaultCursorShape = CursorShape.PointingHand
		};
		textureButton.Modulate = new Color(1f, 0.92f, 0.38f);
		return textureButton;
	}
}