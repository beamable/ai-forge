using Beamable.Common;
using Beamable.Common.Api.Realms;
using Beamable.Editor.Login.UI.Components;
using Beamable.Editor.Login.UI.Model;
using System;
using System.Collections.Generic;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
using static Beamable.Common.Constants.Features.LoginBase;

namespace Beamable.Editor.Login.UI
{
	public class LoginManager
	{
		public Promise<LoginModel> InitializedModel { get; private set; }
		public Promise<Unit> OnComplete { get; private set; } = new Promise<Unit>();

		private NewCustomerVisualElement _newCustomerVisualElement;
		private ExistingCustomerVisualElement _existingCustomerVisualElement;
		private LegalCopyVisualElement _legalCopyVisualElement;
		private ProjectSelectVisualElement _projectSelectVisualElement;
		private NewUserVisualElement _newUserVisualElement;
		private AccountSummaryVisualElement _accountSummaryVisualElement;
		private NoRoleVisualElement _noRoleVisualElement;
		private ForgotVisualElement _forgotPasswordVisualElement;

		private Stack<LoginBaseComponent> _history = new Stack<LoginBaseComponent>();
		public LoginBaseComponent StartElement { get; private set; }

		public event Action<LoginBaseComponent> OnPageChanged;

		public Promise<LoginModel> Initialize(LoginModel model)
		{
			InitializedModel = model.Initialize().Then(initializedModel =>
			{
				//            _loginVisualElement = new Components.LoginVisualElement(){Model = initializedModel, Manager = this};
				_newCustomerVisualElement = new NewCustomerVisualElement() { Model = initializedModel, Manager = this };
				_existingCustomerVisualElement = new ExistingCustomerVisualElement() { Model = initializedModel, Manager = this };
				_legalCopyVisualElement = new LegalCopyVisualElement() { Model = initializedModel, Manager = this };
				_projectSelectVisualElement = new ProjectSelectVisualElement() { Model = initializedModel, Manager = this };
				_newUserVisualElement = new NewUserVisualElement() { Model = initializedModel, Manager = this };
				_accountSummaryVisualElement = new AccountSummaryVisualElement { Model = initializedModel, Manager = this };
				_noRoleVisualElement = new NoRoleVisualElement { Model = initializedModel, Manager = this };
				_forgotPasswordVisualElement = new ForgotVisualElement { Model = initializedModel, Manager = this };

				AssumePage(initializedModel);
			});
			return InitializedModel;
		}

		public void AssumePage(LoginModel model)
		{
			StartElement = GetStartLoginPage(model);
			GotoNewPage(StartElement);
		}

		/// <summary>
		///  Given the current state of the world, get the component we should start a login flow on.
		/// </summary>
		public LoginBaseComponent GetStartLoginPage(LoginModel model)
		{
			_history.Clear();
			var brandNewCustomerUser = !model.Customer.HasCid && !model.Customer.HasGame;
			if (brandNewCustomerUser)
			{
				return _newCustomerVisualElement;
			}

			var needsLogin = !model.Customer.HasUser;
			if (needsLogin)
			{
				return _existingCustomerVisualElement;
			}

			var needsRole = !model.Customer.HasRole;
			if (needsRole)
			{
				return _noRoleVisualElement;
			}

			var needsProject = !model.Customer.HasGame;
			if (needsProject)
			{
				return _projectSelectVisualElement;
			}

			// at this point, we don't actually want to be at a log in flow at all.
			OnComplete?.CompleteSuccess(PromiseBase.Unit);
			return _accountSummaryVisualElement;
		}

		public void GoToPreviousPage()
		{
			if (_history.Count > 1)
			{
				var current = _history.Pop();
				var previous = _history.Peek();
				OnPageChanged?.Invoke(previous);
			}
		}

		public Promise<LoginManagerResult> Logout(LoginModel model)
		{
			var b = BeamEditorContext.Default;
			b.Logout();
			model.Customer.SetUserInfo(0, null);
			AssumePage(model);
			return Promise<LoginManagerResult>.Successful(LoginManagerResult.Pass);
		}

		public Promise<LoginManagerResult> SendPasswordResetCode(LoginModel model)
		{
			var b = BeamEditorContext.Default;
			var issue = b.SendPasswordResetCode(model.Customer.Code, model.Customer.Password);
			return UseCommonErrorHandling(model, issue, new LoginErrorHandlers()
														.OnNotFound(NO_ACCOUNT_FOUND_ERROR)
														.OnBadRequest(EXCEPTION_TYPE_BADCODE, BAD_CODE_ERROR)
														.OnBadRequest(NO_ALIAS_FOUND_ERROR),
										  false
			).Then(res =>
			{
				if (!res.Success) return;
				GotoExistingCustomer();
			});
		}

		public Promise<LoginManagerResult> SendPasswordResetEmail(LoginModel model)
		{
			var b = BeamEditorContext.Default;
			var issue = b.SendPasswordReset(model.Customer.CidOrAlias, model.Customer.Email);
			return UseCommonErrorHandling(model, issue, new LoginErrorHandlers()
														.OnNotFound(NO_ACCOUNT_FOUND_ERROR)
														.OnBadRequest(NO_ALIAS_FOUND_ERROR),
										  false);
		}

		public Promise<LoginManagerResult> AttemptProjectSelect(LoginModel model, RealmView game)
		{
			var b = BeamEditorContext.Default;
			var setGame = b.SetGame(game);
			return UseCommonErrorHandling(model, setGame, new LoginErrorHandlers());
		}

		public Promise<LoginManagerResult> AttemptNewCustomer(LoginModel model)
		{
			var b = BeamEditorContext.Default;
			var newCustomer = b.CreateCustomer(model.Customer.CidOrAlias, model.Customer.Pid, model.Customer.Email,
											   model.Customer.Password);

			return UseCommonErrorHandling(model, newCustomer, new LoginErrorHandlers()
															  .OnUnknown(CUSTOMER_CREATION_UNKNOWN_ERROR)
															  .OnBadRequest(EXCEPTION_TYPE_BAD_ALIAS, BAD_ALIAS_ERROR)
															  .OnBadRequest(EXCEPTION_TYPE_BAD_GAME_NAME, BAD_GAME_NAME_ERROR)
															  .On(err => err.Status == 500 && err.Error.message == model.Customer.CidOrAlias, CID_TAKEN_ERROR)
															  );
		}

		public Promise<LoginManagerResult> AttemptNewUser(LoginModel model)
		{
			var b = BeamEditorContext.Default;
			var createUser = b.CreateUser(model.Customer.CidOrAlias, model.Customer.Email, model.Customer.Password);

			return UseCommonErrorHandling(model, createUser, new LoginErrorHandlers()
															 .OnBadRequest(EXCEPTION_TYPE_NOCID, NO_ALIAS_FOUND_ERROR)
															 .OnBadRequest(EXCEPTION_TYPE_EMAIL_TAKEN, EMAIL_TAKEN_ERROR)
			);
		}

		public Promise<LoginManagerResult> AttemptLoginExistingCustomer(LoginModel model)
		{
			var b = BeamEditorContext.Default;
			var login = b.LoginCustomer(model.Customer.CidOrAlias, model.Customer.Email, model.Customer.Password);
			return UseCommonErrorHandling(model, login, new LoginErrorHandlers()
														.OnUnauthorized(INVALID_CREDENTIALS_ERROR)
														.OnBadRequest(NO_ALIAS_FOUND_ERROR)
														.OnBadRequest(EXCEPTION_TYPE_INVALID_SCOPE, NO_ALIAS_FOUND_ERROR)
														.OnNotFound(INVALID_CREDENTIALS_ERROR)
			);
		}

		private Promise<LoginManagerResult> UseCommonErrorHandling<T>(LoginModel model, Promise<T> promise, LoginErrorHandlers handler, bool assumePage = true)
		{
			return promise.Map(_ =>
						  {
							  if (assumePage)
							  {
								  model.Initialize().Then(AssumePage);
							  }

							  return LoginManagerResult.Pass;
						  })
						  .Recover(ex =>
						  {
							  BeamableLogger.LogError(ex);
							  var message = handler.ProduceError(ex);
							  return LoginManagerResult.Failed(message);
						  })
						  .Error(BeamableLogger.LogError);
		}

		public bool HasPreviousPage() => _history.Count > 1;

		public bool IsPreviousPage<T>() where T : LoginBaseComponent
		{
			if (!HasPreviousPage()) return false;

			var curr = _history.Pop();
			var match = _history.Peek() is T;
			_history.Push(curr);
			return match;
		}

		public void GotoLogin()
		{
			GotoNewPage(_existingCustomerVisualElement);
		}

		public void GotoSummary()
		{
			OnComplete?.CompleteSuccess(PromiseBase.Unit);
			GotoNewPage(_accountSummaryVisualElement);
		}

		public void GotoForgotPassword()
		{
			GotoNewPage(_forgotPasswordVisualElement);
		}

		public void GotoExistingCustomer()
		{
			GotoNewPage(_existingCustomerVisualElement);
		}

		public void GotoLegalCopy()
		{
			GotoNewPage(_legalCopyVisualElement);
		}

		public void GotoProjectSelectVisualElement()
		{
			GotoNewPage(_projectSelectVisualElement);
		}

		public void GotoNewCustomer()
		{
			GotoNewPage(_newCustomerVisualElement);
		}

		public void GotoNewUser()
		{
			GotoNewPage(_newUserVisualElement);
		}

		public void GotoNoRole()
		{
			GotoNewPage(_noRoleVisualElement);
		}

		public void GotoNewPage(LoginBaseComponent component)
		{
			_history.Push(component);
			OnPageChanged?.Invoke(component);
		}

		public void Destroy() { }
	}

	public class LoginManagerResult
	{
		public bool Success;
		public string Error;

		public static readonly LoginManagerResult Pass = new LoginManagerResult { Success = true };

		public static LoginManagerResult Failed(string reason)
		{
			return new LoginManagerResult { Success = false, Error = reason };
		}
	}
}
