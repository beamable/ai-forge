using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Beamable.UI.Sdf
{
	[Serializable]
	public class SerializableValueObject : ISerializationCallbackReceiver
	{
		private object _value;

		[SerializeField, FormerlySerializedAs("type")]
		private string _type;

		[SerializeField, FormerlySerializedAs("json")]
		private string _json;

		public void Set(object newValue)
		{
			_value = newValue;
		}

		public object Get()
		{
			return _value;
		}

		public T Get<T>()
		{
			return (T)_value;
		}

		public void OnBeforeSerialize()
		{
			if (_value == null)
			{
				_type = _json = string.Empty;
			}
			else
			{
				_type = _value.GetType().AssemblyQualifiedName;
				_json = JsonUtility.ToJson(_value);
			}
		}

		public void OnAfterDeserialize()
		{
			if (string.IsNullOrWhiteSpace(_type) || string.IsNullOrWhiteSpace(_json))
			{
				_value = null;
			}

			var sysType = Type.GetType(_type, false);
			if (sysType == null)
			{
				_value = null;
				return;
			}

			try
			{
				if (_value != null && _value.GetType() == sysType)
				{
					JsonUtility.FromJsonOverwrite(_json, _value);
				}
				else
				{
					_value = JsonUtility.FromJson(_json, sysType);
				}
			}
			catch (Exception)
			{
				_value = null;
			}
		}

		public void ForceSerialization()
		{
			OnBeforeSerialize();
		}
	}
}
