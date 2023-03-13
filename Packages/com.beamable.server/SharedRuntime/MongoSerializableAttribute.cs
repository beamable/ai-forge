using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Serialization;

namespace Beamable.Server
{
	[AttributeUsage(AttributeTargets.Class)]
	public class MongoSerializableAttribute : Attribute, IBsonClassMapAttribute
	{
		public void Apply(BsonClassMap cm)
		{
			FieldInfo bsonIDField = FindBsonIdField(cm.ClassType);

			if (bsonIDField != null)
			{
				if (bsonIDField.DeclaringType != cm.ClassType && !BsonClassMap.IsClassMapRegistered(bsonIDField.DeclaringType))
				{
					var cm_base = new BsonClassMap(bsonIDField.DeclaringType);
					cm_base.AutoMap();
					cm_base.MapIdField(bsonIDField.Name).SetSerializer(new StringSerializer(BsonType.ObjectId)).SetIgnoreIfDefault(true);
					BsonClassMap.RegisterClassMap(cm_base);
					cm_base.Freeze();
				}
			}

			// set bsonID attribute for DeclaringType if it's needed

			if (bsonIDField != null && bsonIDField.DeclaringType == cm.ClassType)
				cm.MapIdField(bsonIDField.Name).SetSerializer(new StringSerializer(BsonType.ObjectId)).SetIgnoreIfDefault(true);

			// Iterate throght properites and unmap them

			PropertyInfo[] props = cm.ClassType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

			if (props.Length > 0)
			{
				foreach (PropertyInfo propertyInfo in props)
					cm.UnmapProperty(propertyInfo.Name);
			}

			FieldInfo[] fields = cm.ClassType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

			if (fields.Length > 0)
			{
				foreach (FieldInfo fieldInfo in fields)
				{
					if (!fieldInfo.IsDefined(typeof(CompilerGeneratedAttribute), false)) // we don't want to k__backingfield
					{
						// Set field as serializable if has SerializableAttribute

						if (fieldInfo.GetCustomAttribute(typeof(SerializeField)) != null)
							cm.MapField(fieldInfo.Name);
						else if (!((FieldInfo)fieldInfo).IsPublic)
							cm.UnmapField(fieldInfo.Name);

						// Set new member name if has FormerlySerializedAsAttribute

						if (fieldInfo.GetCustomAttribute(typeof(FormerlySerializedAsAttribute)) is FormerlySerializedAsAttribute formerlySerializedAttr)
							cm.GetMemberMap(fieldInfo.Name).SetElementName(formerlySerializedAttr.oldName);
					}
				}
			}

			cm.Freeze();
		}

		private FieldInfo FindBsonIdField(Type t)
		{
			var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

			foreach (var field in t.GetFields(flags))
			{
				if (field.GetCustomAttribute(typeof(BsonIdAttribute)) != null)
					return field;
			}

			if (t.BaseType != null)
				return FindBsonIdField(t.BaseType);

			return null;
		}
	}
}
