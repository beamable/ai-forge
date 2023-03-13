using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Spew;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beamable.Editor.Modules.Hubspot
{
	public class HubspotService
	{
		private const string HUBSPOT_PORTAL_ID = "9313073";
		private const string HUBSPOT_REGISTRATION_FORM_ID = "3644073d-2ac8-4e72-a553-b393e4fa660a";

		private const string HUBSPOT_REGISTRATION_FORM_URL =
			"https://api.hsforms.com/submissions/v3/integration/submit/" + HUBSPOT_PORTAL_ID + "/" +
			HUBSPOT_REGISTRATION_FORM_ID;

		private readonly IHttpRequester _requester;

		public HubspotService(IHttpRequester requester)
		{
			_requester = requester;
		}

		/// <summary>
		/// Sends a new customer registration event to hubspot.
		/// </summary>
		/// <param name="email">the email of the developer</param>
		/// <param name="alias">the developer's new studio name / alias</param>
		public async Promise SubmitRegistrationEvent(string email, string alias)
		{
			if (!BeamableEnvironment.IsProduction)
			{
				PlatformLogger.Log($"<b>[Hubspot] ignoring registration event because this is not production.");
				return;
			}

			if (BeamableEnvironment.SdkVersion.IsReleaseCandidate)
			{
				PlatformLogger.Log($"<b>[Hubspot] ignoring registration event because this is a release candidate.");
				return;
			}

			if (string.IsNullOrEmpty(email))
			{
				throw new ArgumentNullException(nameof(email));
			}

			if (string.IsNullOrEmpty(alias))
			{
				throw new ArgumentNullException(nameof(alias));
			}

			var builder = new FormSubmissionRequestBuilder
			{
				alias = alias,
				email = email
			};
			var req = builder.Build();
			PlatformLogger.Log($"<b>[Hubspot] {req.GetFieldString()}");
			await _requester.ManualRequest<EmptyResponse>(Method.POST, HUBSPOT_REGISTRATION_FORM_URL, req);
		}
	}

	public class FormSubmissionRequestBuilder
	{
		public const string FIELD_OBJECT_TYPE_ID = "0-1";
		public const string FIELD_EMAIL = "email";
		public const string FIELD_ALIAS = "studio_name";
		public const string FIELD_DEV_TYPE = "type_of_developer";
		public const string FIELD_DEV_TYPE_VALUE = "Indie Developer";
		public const string FIELD_FEATURES = "beamable_features";
		public const string FIELD_FEATURES_VALUE = "I don't want to deal with servers or code a backend";
		public const string FIELD_FIRST_NAME = "firstname";
		public const string FIELD_FIRST_NAME_VALUE = "SDK";
		public const string FIELD_LAST_NAME = "lastname";
		public const string FIELD_LAST_NAME_VALUE = "SDK";
		public const string FIELD_SOURCE = "source";
		public const string FIELD_SOURCE_SDK_VALUE = "SDK Install";
		public const string FIELD_SOURCE_VSP_VALUE = "Unity Asset Store";
		public const string FIELD_LICENSE = "license";
		public const string FIELD_LICENSE_VALUE = "Accept License";
		public const string FIELD_TERMS_OF_USE = "terms_of_use";
		public const string FIELD_TERMS_OF_USE_VALUE = "Yes, I agree to use Beamable according to both policies";

		public string email; // email
		public string alias; // studio_name

		public FormSubmissionRequest Build()
		{
			var sourceValue = BeamableEnvironment.IsUnityVsp
				? FIELD_SOURCE_VSP_VALUE
				: FIELD_SOURCE_SDK_VALUE;
			if (!BeamableEnvironment.IsProduction)
			{
				sourceValue += $" ({BeamableEnvironment.Environment})";
			}

			var req = new FormSubmissionRequest
			{
				fields = new List<FormField>
			{
				new FormField{ objectTypeId = FIELD_OBJECT_TYPE_ID, name = FIELD_EMAIL, value = email},
				new FormField{ objectTypeId = FIELD_OBJECT_TYPE_ID, name = FIELD_ALIAS, value = alias},

				// the source must always be sent, but will either be VSP or not-VSP.
				new FormField{ objectTypeId = FIELD_OBJECT_TYPE_ID, name = FIELD_SOURCE, value = sourceValue},

				// must send terms of service
				new FormField{ objectTypeId = FIELD_OBJECT_TYPE_ID, name = FIELD_LICENSE, value = FIELD_LICENSE_VALUE},
				new FormField{ objectTypeId = FIELD_OBJECT_TYPE_ID, name = FIELD_TERMS_OF_USE, value = FIELD_TERMS_OF_USE_VALUE},

				// default values for required fields...
				new FormField{ objectTypeId = FIELD_OBJECT_TYPE_ID, name = FIELD_DEV_TYPE, value = FIELD_DEV_TYPE_VALUE},
				new FormField{ objectTypeId = FIELD_OBJECT_TYPE_ID, name = FIELD_FIRST_NAME, value = FIELD_FIRST_NAME_VALUE},
				new FormField{ objectTypeId = FIELD_OBJECT_TYPE_ID, name = FIELD_LAST_NAME, value = FIELD_LAST_NAME_VALUE},
				new FormField{ objectTypeId = FIELD_OBJECT_TYPE_ID, name = FIELD_FEATURES, value = FIELD_FEATURES_VALUE},

			}
			};

			return req;
		}
	}

	[Serializable]
	public class FormSubmissionRequest
	{
		public List<FormField> fields;

		public string GetFieldString()
		{
			return string.Join(",", fields?.Select(f => $"{f.name}={f.value}") ?? Array.Empty<string>());
		}
	}

	[Serializable]
	public class FormField
	{
		public string objectTypeId;
		public string name;
		public string value;
	}
}
