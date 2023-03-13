using Beamable.Common;
using Beamable.Editor.Login.UI.Model;
using Beamable.Editor.UI.Components;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
using static Beamable.Common.Constants.Features.LoginBase;

namespace Beamable.Editor.Login.UI.Components
{
	public class LoginBaseComponent : BeamableVisualElement
	{
		public LoginModel Model;
		public LoginManager Manager;

		public virtual string GetMessage()
		{
			return "";
		}

		public virtual bool ShowHeader => true;

		public LoginBaseComponent(string name) : base($"{COMPONENTS_PATH}/{name}/{name}")
		{

		}

		protected Promise<LoginManagerResult> AddErrorLabel(Promise<LoginManagerResult> promise, Label errorLabel)
		{
			return promise.Then(res =>
				{
					errorLabel.text = res.Error;
				})
				.Error(err =>
				{
					BeamableLogger.LogError("Failed " + err.Message);
					errorLabel.text = err.Message;
				});
		}
	}
}
