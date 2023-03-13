using Beamable.ConsoleCommands;
using Beamable.InputManagerIntegration;
using Beamable.Service;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static Beamable.Common.Constants.URLs;

namespace Beamable.Console
{
	[HelpURL(Documentations.URL_DOC_ADMIN_FLOW)]
	public class ConsoleFlow : MonoBehaviour
	{
		public static ConsoleFlow Instance;

		// plrCode goes to a dictionary of command->callback.
		private readonly Dictionary<string, Dictionary<string, ConsoleCommand>> ConsoleCommandsByName =
			new Dictionary<string, Dictionary<string, ConsoleCommand>>();

		public Canvas canvas;
		public Text txtOutput;
		public InputField txtInput;
		public Text txtAutoCompleteSuggestion;
		private bool _isInitialized;
		private bool _showNextTick;
		private bool _isActive;
		private int _fingerCount;
		private bool _waitForRelease;
		private Vector2 _averagePositionStart;
		// private IBeamableAPI _beamable;
		private TextAutoCompleter _textAutoCompleter;
		private ConsoleHistory _consoleHistory;
		private string consoleText;
		private string _playerCode;

#if UNITY_ANDROID // webGL doesn't support the touchscreen keyboard.
        private bool _isMobileKeyboardOpened = false;
#pragma warning disable CS0649
        [SerializeField] private RectTransform consolePortrait;
        [SerializeField] private RectTransform consoleLandscape;
#pragma warning restore CS0649
#endif

#pragma warning disable CS0414
		[Space]
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
		[HideInInspector]
#endif
		[Header("Text auto complete settings")]
		[SerializeField]
		private KeyCode acceptSuggestionKey = KeyCode.Tab;

#if !ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER
		[HideInInspector]
#endif
		[Header("Text auto complete settings")]
		[SerializeField] private InputActionArg acceptSuggestionAction;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
		[Header("History settings")]
		[HideInInspector]
#endif
		[SerializeField]
		private KeyCode historyPreviousKey = KeyCode.UpArrow;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
		[HideInInspector]
#endif
		// ReSharper disable once NotAccessedField.Local
		[SerializeField] private KeyCode historyNextKey = KeyCode.DownArrow;
#if !ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER
		[HideInInspector]
#else
		[Header("History settings")]
#endif
		[SerializeField] private InputActionArg historyNextAction;
#if !ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER
		[HideInInspector]
#endif
		[SerializeField] private InputActionArg historyPreviousAction;
#pragma warning restore CS0414

		private void Start()
		{
			if (Instance)
			{
				Destroy(gameObject);
				Instance.InitializeConsole();
				return;
			}


			Instance = this;
			DontDestroyOnLoad(gameObject);
			InitializeConsole();
		}

		public void ChangePlayerContext(string newPlayerCode)
		{
			_playerCode = newPlayerCode;
			InitializeConsole();
		}

		private void Awake()
		{
			HideConsole();

#if UNITY_ANDROID || UNITY_IOS
            StartCoroutine(CheckMobileKeyboardState());
#endif
		}

		private void Update()
		{
			var _ = BeamContext.ForPlayer(_playerCode).OnReady;
			if (!_isInitialized) return;

			if (_showNextTick)
			{
				DoShow();
				_showNextTick = false;
			}

			if (ConsoleShouldToggle() && ConsoleIsEnabled()) ToggleConsole();

#if !ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER
			bool historyPrevious = Input.GetKeyDown(historyPreviousKey);
			bool historyNext = Input.GetKeyDown(historyNextKey);
			bool acceptSuggestion = Input.GetKeyDown(acceptSuggestionKey);
#else
			bool historyPrevious = historyPreviousAction.IsTriggered();
			bool historyNext = historyNextAction.IsTriggered();
			bool acceptSuggestion = acceptSuggestionAction.IsTriggered();
#endif

			if (historyPrevious)
			{
				txtInput.text = _consoleHistory.Previous();
				txtInput.caretPosition = txtInput.text.Length;
			}
			else if (historyNext)
			{
				txtInput.text = _consoleHistory.Next();
				txtInput.caretPosition = txtInput.text.Length;
			}
			else if (acceptSuggestion)
			{
				_textAutoCompleter.AcceptSuggestedCommand();
			}
		}

		private async void InitializeConsole()
		{
			_playerCode = _playerCode ?? "";
			_isInitialized = false;
			txtInput.interactable = false;

			// We want to ensure that we create the instance of the Beamable API if the console is the only thing
			// in the scene.
			// _beamable = await API.Instance;
			if (!ConsoleCommandsByName.ContainsKey(_playerCode))
			{
				ConsoleCommandsByName.Add(_playerCode, new Dictionary<string, ConsoleCommand>());
			}
			_textAutoCompleter = new TextAutoCompleter(this, ref txtInput, ref txtAutoCompleteSuggestion);
			_consoleHistory = new ConsoleHistory();

			var ctx = BeamContext.ForPlayer(_playerCode);
			await ctx.OnReady;

			if (_console != null) // clean up old events
			{
				_console.OnLog -= Log;
				_console.OnExecute -= ExecuteCommand;
				_console.OnCommandRegistered -= RegisterCommand;
			}

			_console = ctx.ServiceProvider.GetService<BeamableConsole>();
			ServiceManager.Provide<BeamableConsole>(ctx.ServiceProvider); // this exists for legacy purposes, for anyone who might be using the service manager to the console...

			_console.OnLog += Log;
			_console.OnExecute += ExecuteCommand;
			_console.OnCommandRegistered += RegisterCommand;
			try
			{
				_console.LoadCommands();
			}
			catch (Exception)
			{
				Debug.LogError("Unable to load console commands.");
			}

			_textAutoCompleter.FindMatchingCommands(txtInput.text);
			txtInput.onValueChanged.AddListener(_textAutoCompleter.FindMatchingCommands);
			txtInput.onEndEdit.AddListener(evt =>
			{
				if (txtInput.text.Length > 0) Execute(txtInput.text);
			});

			txtInput.interactable = true;
			if (canvas.isActiveAndEnabled) txtInput.Select();

			// Hacky method to prevent NullReferenceException in UnityEngine.UI.InputField.GenerateCaret
			// Delay prevents the user from interacting with the console before all UI components are configured
			// Sadly, Unity won't fix this problem
			await Task.Delay(100);

			_isInitialized = true;
			Log("Console ready");
		}

		/// <summary>
		///     Console should toggle if the toggle key was pressed OR a 3 finger swipe occurred on device.
		/// </summary>
		private bool ConsoleShouldToggle()
		{
			var shouldToggle = BeamableInput.IsActionTriggered(ConsoleConfiguration.Instance.ToggleAction);
#if !ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER
			if (shouldToggle)
				// Early out if we already know we must toggle.
				return true;

			var fingerCount = 0;
			var averagePosition = Vector2.zero;
			var touchCount = Input.touchCount;
			for (var i = 0; i < touchCount; ++i)
			{
				var touch = Input.GetTouch(i);
				if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled)
				{
					fingerCount++;
					averagePosition += touch.position;
				}
			}

			switch (fingerCount)
			{
				case 3 when !_waitForRelease:
				{
					averagePosition /= 3;
					if (_fingerCount != 3)
					{
						_averagePositionStart = averagePosition;
					}
					else
					{
						if ((_averagePositionStart - averagePosition).magnitude > 20.0f)
						{
							_waitForRelease = true;
							shouldToggle = true;
						}
					}

					break;
				}
				case 0 when _waitForRelease:
					_waitForRelease = false;
					break;
			}

			_fingerCount = fingerCount;
#endif
			return shouldToggle;
		}

		private bool ConsoleIsEnabled()
		{
#if UNITY_EDITOR
            return true;
#else
			return ConsoleConfiguration.Instance.ForceEnabled || BeamContext.ForPlayer(_playerCode).AuthorizedUser.Value.HasScope("cli:console");
#endif
		}

		private void Execute(string txt)
		{
			if (!_isActive) return;

			_consoleHistory.Push(txt);
			var parts = txt.Split(' ');
			txtInput.text = "";
			txtInput.Select();
			txtInput.ActivateInputField();
			if (parts.Length == 0) return;
			var args = new string[parts.Length - 1];
			for (var i = 1; i < parts.Length; i++) args[i - 1] = parts[i];


			var ctx = BeamContext.ForPlayer(_playerCode);
			var console = ctx.ServiceProvider.GetService<BeamableConsole>();
			// need to re-register commands because they might have been lost in a reset or restart
			console.OnLog -= Log;
			console.OnExecute -= ExecuteCommand;
			console.OnCommandRegistered -= RegisterCommand;
			console.OnLog += Log;
			console.OnExecute += ExecuteCommand;
			console.OnCommandRegistered += RegisterCommand;

			Log(console.Execute(parts[0], args));
		}

		private void RegisterCommand(BeamableConsoleCommandAttribute command, ConsoleCommandCallback callback)
		{
			foreach (var name in command.Names)
			{
				var cmd = new ConsoleCommand { Command = command, Callback = callback };
				ConsoleCommandsByName[_playerCode][name.ToLower()] = cmd;
			}
		}

		private string ExecuteCommand(string command, string[] args)
		{
			if (command == "help") return OnHelp(args);

			if (ConsoleCommandsByName[_playerCode].TryGetValue(command.ToLower(), out var cmd))
			{
				var echoLine = "> " + command;
				foreach (var arg in args) echoLine += " " + arg;

				Log(echoLine);
				return cmd.Callback(args);
			}

			return "Unknown command";
		}

		private string OnHelp(params string[] args)
		{
			if (args.Length == 0)
			{
				var builder = new StringBuilder();
				builder.AppendLine("Listing commands:");
				var uniqueCommands = new HashSet<ConsoleCommand>();
				var commands = ConsoleCommandsByName[_playerCode].Values;
				foreach (var command in commands)
				{
					if (uniqueCommands.Contains(command)) continue;

					uniqueCommands.Add(command);
					var line = $"{command.Command.Usage} - {command.Command.Description}\n";
					Debug.Log(line);
					builder.Append(line);
				}

				return builder.ToString();
			}

			var commandToGetHelpAbout = args[0].ToLower();
			if (ConsoleCommandsByName[_playerCode].TryGetValue(commandToGetHelpAbout, out var found))
				return
					$"Help information about {commandToGetHelpAbout}\n\tDescription: {found.Command.Description}\n\tUsage: {found.Command.Usage}";

			return $"Cannot find help information about {commandToGetHelpAbout}. Are you sure it is a valid command?";
		}

		public void Log(string line)
		{
			Debug.Log(line);
			consoleText += Environment.NewLine + line;
			UpdateText();
		}

		private void UpdateText()
		{
			const int verticesPerRectangle = 6;
			const int textVertexLimit = 65 * 1024;

			int resultVertexAmount = consoleText.Length * verticesPerRectangle;
			int minCharsToRemove = (resultVertexAmount - textVertexLimit) / verticesPerRectangle;

			if (minCharsToRemove > 0)
			{
				var buffSplit = consoleText.Split(Environment.NewLine.ToCharArray());
				int lines = buffSplit.Length;

				int charsRemoved = 0;
				int linesToRemove = 1;

				for (int i = 0; i < lines; i++, linesToRemove++)
				{
					if (charsRemoved > minCharsToRemove)
						break;

					charsRemoved += buffSplit[i].Length;
				}

				consoleText = string.Join(Environment.NewLine, buffSplit.Skip(linesToRemove));
			}
			txtOutput.text = consoleText;
		}

		public void ToggleConsole()
		{
			if (_isActive)
				HideConsole();
			else
				ShowConsole();
		}

		public void HideConsole()
		{
			_isActive = false;
			txtInput.DeactivateInputField();
			txtInput.text = "";
			canvas.enabled = false;
		}

		public void ShowConsole()
		{
			if (!enabled)
			{
				Debug.LogWarning("Cannot open the console, because it isn't enabled");
				return;
			}

			_showNextTick = true;
		}

		private void DoShow()
		{
			_isActive = true;
			canvas.enabled = true;
			txtInput.text = "";
			txtInput.Select();
			txtInput.ActivateInputField();
		}

		private struct ConsoleCommand
		{
			public BeamableConsoleCommandAttribute Command;
			public ConsoleCommandCallback Callback;
		}

		private WaitForSeconds _mobileCheckWaiter = new WaitForSeconds(0.1f);
		private WaitForSeconds _keyboardOpenWaiter = new WaitForSeconds(0.5f);
		private BeamableConsole _console;

		private IEnumerator CheckMobileKeyboardState()
		{
#if UNITY_ANDROID // webGL doesn't support the touchscreen keyboard.
            while (true)
            {
                if (TouchScreenKeyboard.visible && !_isMobileKeyboardOpened)
                {
                    yield return _keyboardOpenWaiter;
                    var keyboardHeight = GetKeyboardHeight() * Screen.height + 225;
                    consolePortrait.sizeDelta = new Vector2(0, -keyboardHeight);
                    consoleLandscape.sizeDelta = new Vector2(0, -keyboardHeight);
                    _isMobileKeyboardOpened = true;
                }
                else if (!TouchScreenKeyboard.visible && _isMobileKeyboardOpened)
                {
                    consolePortrait.sizeDelta = Vector2.zero;
                    consoleLandscape.sizeDelta = Vector2.zero;
                    _isMobileKeyboardOpened = false;
                }
                yield return _mobileCheckWaiter;
            }
#else
			yield return null;
#endif
		}

		private float GetKeyboardHeight()
		{
			if (Application.isEditor) return txtInput.isFocused ? 0.25f : 0;

#if UNITY_ANDROID
            using (AndroidJavaClass UnityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                if (!txtInput.isFocused)
                {
                    return 0f;
                }

                var unityPlayer = UnityClass.GetStatic<AndroidJavaObject>("currentActivity")
                    .Get<AndroidJavaObject>("mUnityPlayer");
                var view = unityPlayer.Call<AndroidJavaObject>("getView");
                var dialog = unityPlayer.Get<AndroidJavaObject>("mSoftInputDialog");

                var decorHeight = 0;
                if (view != null && dialog != null)
                {
                    if (!txtInput.shouldHideMobileInput)
                    {
                        var decorView = dialog.Call<AndroidJavaObject>("getWindow")
                            .Call<AndroidJavaObject>("getDecorView");

                        if (decorView != null)
                            decorHeight = decorView.Call<int>("getHeight");
                    }

                    using (AndroidJavaObject rect = new AndroidJavaObject("android.graphics.Rect"))
                    {
                        view.Call("getWindowVisibleDisplayFrame", rect);
                        return (float) (Screen.height - rect.Call<int>("height") + decorHeight) / Screen.height;
                    }
                }

                return 0.25f;

            }
#elif UNITY_IOS
                return TouchScreenKeyboard.area.height / Screen.height;
#else
			return 0.0f;
#endif
		}

		private class TextAutoCompleter
		{
			private readonly Dictionary<string, Dictionary<string, ConsoleCommand>> _consoleCommandsByName;
			private readonly ConsoleFlow _consoleFlow;
			private readonly InputField _inputField;
			private readonly Text _textSuggestion;
			private int _commandIndex;
			private string _currentSuggestedCommand = string.Empty;
			private List<string> _foundCommands = new List<string>();
			private string _previousInput = string.Empty;
			private bool isFinderLocked = false;

			public TextAutoCompleter(ConsoleFlow consoleFlow,
									 ref InputField inputField,
									 ref Text textSuggestion)
			{
				_consoleCommandsByName = consoleFlow.ConsoleCommandsByName;
				_consoleFlow = consoleFlow;
				_inputField = inputField;
				_textSuggestion = textSuggestion;
			}

			public void FindMatchingCommands(string input)
			{
				if (isFinderLocked)
				{
					isFinderLocked = false;
					return;
				}

				_commandIndex = 0;
				if (string.IsNullOrWhiteSpace(input))
				{
					_foundCommands = _consoleCommandsByName[_consoleFlow._playerCode].OrderBy(x => x.Key)
																   .Select(x => x.Key)
																   .ToList();

					_currentSuggestedCommand = string.Empty;
					_textSuggestion.text = string.Empty;
				}
				else
				{
					_foundCommands = input.Length < _previousInput.Length || _previousInput.Length == 0
						? _consoleCommandsByName[_consoleFlow._playerCode].Where(x => x.Key.StartsWith(input))
																		  .OrderBy(x => x.Key)
																		  .Select(x => x.Key)
																		  .ToList()
						: _foundCommands.Where(x => x.StartsWith(input))
							.ToList();

					_currentSuggestedCommand = _foundCommands.Count > 0 ? _foundCommands[0] : string.Empty;
					SuggestCommand();
				}

				_previousInput = input;
			}

			private void SuggestCommand()
			{
				if (string.IsNullOrWhiteSpace(_currentSuggestedCommand))
				{
					_textSuggestion.text = string.Empty;
					return;
				}

				_textSuggestion.text = _currentSuggestedCommand;
				_inputField.caretPosition = _currentSuggestedCommand.Length;
			}

			public void AcceptSuggestedCommand()
			{
				if (_foundCommands.Count == 0)
				{
					return;
				}

				isFinderLocked = true;
				if (_inputField.text == _currentSuggestedCommand)
				{
					NextCommand();
				}

				_inputField.text = _currentSuggestedCommand;
				_inputField.caretPosition = _currentSuggestedCommand.Length;
				_textSuggestion.text = string.Empty;
			}

			private void NextCommand()
			{
				_commandIndex++;
				if (_commandIndex > _foundCommands.Count - 1)
				{
					_commandIndex = 0;
				}

				_currentSuggestedCommand = _foundCommands[_commandIndex];
				SuggestCommand();
			}
		}

		private class ConsoleHistory
		{
			private readonly List<string> _history = new List<string>();
			private int _position = 0;

			public void Push(string text)
			{
				if (string.IsNullOrEmpty(text))
				{
					return;
				}

				_history.Add(text);
				_position = _history.Count;
			}

			public string Next()
			{
				_position++;
				if (_position < _history.Count)
				{
					return _history[_position];
				}

				_position = _history.Count;
				return string.Empty;
			}

			public string Previous()
			{
				if (_history.Count == 0)
				{
					return string.Empty;
				}

				_position--;
				if (_position < 0)
				{
					_position = 0;
				}

				return _history[_position];
			}
		}
	}
}
