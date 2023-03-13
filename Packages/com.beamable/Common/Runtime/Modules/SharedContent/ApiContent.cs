using Beamable.Common.Api;
using Beamable.Common.Content.Validation;
using Beamable.Content;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0618

namespace Beamable.Common.Content
{
	public enum PlatformServiceType
	{
		UserMicroservice, ObjectService, BasicService
	}

	public enum PlatformWebhookRetryStrategy
	{
		None
	}

	public enum PlatformWebhookInvocationType
	{
		NonBlocking
	}

	[ContentType("api")]
	[Agnostic]
	[Serializable]
	public class ApiContent : ContentObject, ISerializationCallbackReceiver
	{
		private static readonly ApiVariable[] EMPTY_VARIABLE_SET = new ApiVariable[0];

		[ContentField("description")]
		[Tooltip("Write a summary of this api call")]
		public OptionalString Description;

		[ContentField("method")]
		[Tooltip("The http method to use")]
		[HideInInspector]
		public Method Method = Method.POST;

		[ContentField("route")]
		[Tooltip("The route information for the api call")]
		[ServiceRouteMustBeValid]
		public ServiceRoute ServiceRoute = new ServiceRoute();

		[ContentField("variables")]
		[SerializeField]
		[Tooltip("The variables that may be referenced from the route parameters")]
		private RouteVariables _variables = new RouteVariables();
		public ApiVariable[] Variables => _variables.Variables;

		[ContentField("parameters")]
		[Tooltip("The required parameters of the api call")]
		[RouteParametersMustBeValid]
		public RouteParameters Parameters = new RouteParameters();


		/// <summary>
		/// Return the set of variables that can be used on this entire class type of api callbacks.
		/// All <see cref="ApiVariable"/> will be bound to actual values from the call site of the API.
		/// If you are creating your own custom API subclass, and you are implementing this method, be careful not to include variables that are not documented by Beamable.
		/// Any variable listed in the response that isn't provided at the callsite will cause the api callback to fail.
		/// </summary>
		/// <returns></returns>
		protected virtual ApiVariable[] GetVariables()
		{
			return EMPTY_VARIABLE_SET;
		}

		public void OnBeforeSerialize()
		{
			_variables.Variables = GetVariables();
			Parameters.ApiContent = this;
		}

		public void OnAfterDeserialize()
		{
			// don't do anything special...
		}
	}

	public class ApiVariableBag : Dictionary<string, object> { }

	[Serializable]
	public class ApiVariableReference
	{
		[ContentField("name")]
		public string Name;
	}

	[Serializable]
	public class ApiVariable
	{
		[ContentField("name")]
		[Tooltip("The unique name of the variable")]
		public string Name;

		[ContentField("typeName")]
		[Tooltip("The type name of the variable")]
		public string TypeName;

		public static readonly string TYPE_NUMBER = "number";
		public static readonly string TYPE_BOOLEAN = "bool";
		public static readonly string TYPE_STRING = "string";
		public static readonly string TYPE_OBJECT = "object";

		public static string GetTypeName(Type parameterType)
		{
			switch (Type.GetTypeCode(parameterType))
			{
				case TypeCode.Boolean: return TYPE_BOOLEAN;
				case TypeCode.String: return TYPE_STRING;
				case TypeCode.Decimal:
				case TypeCode.Double:
				case TypeCode.Single:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					return TYPE_NUMBER;
				default: return TYPE_OBJECT;
			}
		}
	}

	[Serializable]
	public class OptionalApiVariableReference : Optional<ApiVariableReference> { }

	[Serializable]
	public class ServiceRoute
	{
		[ContentField("service")]
		[Tooltip("The Microservice that will be invoked for this api callback")]
		public string Service;

		[ContentField("endpoint")]
		[Tooltip("The endpoint that will be invoked for this api callback")]
		public string Endpoint;

		[ContentField("serviceTypeStr")]
		[Tooltip("The type of service call.")]
		public PlatformServiceType Type;
	}

	[Serializable]
	public class RouteVariables
	{
		[ContentField("variables")]
		[Tooltip("the variables that may be used")]
		public ApiVariable[] Variables;
	}

	[Serializable]
	public class RouteParameters
	{
		[ContentField("parameters")]
		[Tooltip("the route parameters")]
		public RouteParameter[] Parameters;

		[SerializeField]
		[HideInInspector]
		[IgnoreContentField]
		[Tooltip("a reference ot the api content ")]
		public ApiContent ApiContent;
	}

	[Serializable]
	public class RouteParameter
	{
		[ContentField("name")]
		[Tooltip("The name of this parameter")]
		public string Name;

		[ContentField("variableRef")]
		[Tooltip("If you are using a variable, which variable is this parameter bound to?")]
		public OptionalApiVariableReference variableReference;

		[ContentField("body")]
		[Tooltip("The raw json payload of this parameter")]
		public string Data;

		[ContentField("typeName")]
		[Tooltip("The type of this parameter")]
		public string TypeName;
	}

	[Serializable]
	public class OptionalApiRewardList : Optional<ListOfApiReward> { }

	[Serializable]
	public class ListOfApiReward : DisplayableList<ApiReward>
	{
		public List<ApiReward> listData = new List<ApiReward>();

		protected override IList InternalList => listData;
		public override string GetListPropertyPath() => nameof(listData);
	}

	public interface IApiReward
	{

	}

	[Serializable]
	public class ApiReward<TApi, TRef> : IApiReward
	   where TApi : ApiContent, new()
	   where TRef : ApiRef<TApi>
	{
		[ContentField("webhookSymbol")]
		[MustReferenceContent]
		[Tooltip("Some api content to invoke")]
		public TRef Api;

		[ContentField("strategy")]
		[Tooltip("The strategy that defines how the webhook is invoked")]
		public ApiInvocationStrategy Strategy;
	}

	[Serializable]
	public class ApiReward : IApiReward
	{
		[ContentField("webhookSymbol")]
		[MustReferenceContent]
		[Tooltip("Some api content to invoke")]
		public ApiRef Api;

		[ContentField("strategy")]
		[Tooltip("The strategy that defines how the webhook is invoked")]
		public ApiInvocationStrategy Strategy;
	}

	[Serializable]
	public class ApiInvocationStrategy
	{
		[ContentField("retryType")]
		[Tooltip("Control how the api callback is retried in the event it fails. Be careful! If you have any Retry Strategy other than None, then you need to make sure your method is idempotent.")]
		public PlatformWebhookRetryStrategy RetryStrategy;

		[ContentField("invocationType")]
		[Tooltip("Control if the api callback is going to block or not-block the server process. If set to Blocking, then the call must succeed before the rest of the server operation can continue. If set to NonBlocking, then the api callback's response won't affect the rest of the server operation at all.")]
		public PlatformWebhookInvocationType InvocationType;
	}

	[Serializable]
	public class RouteParameter<T> : RouteParameter
	{
		public T TypeData;
	}

	[Serializable]
	public class ApiRef<T> : ContentRef<T> where T : ApiContent, new()
	{
		public ApiRef()
		{

		}

		public ApiRef(string id)
		{
			Id = id;
		}
	}

	[Serializable]
	public class ApiRef : ApiRef<ApiContent>
	{
	}


}
