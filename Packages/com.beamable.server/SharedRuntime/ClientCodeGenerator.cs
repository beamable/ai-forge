using Beamable.Common;
using Beamable.Common.Api.Auth;
using Beamable.Common.Dependencies;
using Beamable.Server;
using Beamable.Server.Editor;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Beamable.Server.Generator
{
	public class ClientCodeGenerator
	{
		private const string MicroserviceClients_TypeName = "MicroserviceClients";
		private const string MicroserviceClient_TypeName = "MicroserviceClient";

		public MicroserviceDescriptor Descriptor { get; }

		/// <summary>
		/// Define the compile unit to use for code generation.
		/// </summary>
		CodeCompileUnit targetUnit;

		/// <summary>
		/// The only class in the compile unit. This class contains 2 fields,
		/// 3 properties, a constructor, an entry point, and 1 simple method.
		/// </summary>
		private CodeTypeDeclaration targetClass;

		private CodeTypeDeclaration parameterClass;

		private CodeTypeDeclaration extensionClass;

		private string TargetClassName => $"{Descriptor.Name}Client";
		private string TargetParameterClassName => GetTargetParameterClassName(Descriptor);
		private string TargetExtensionClassName => $"ExtensionsFor{Descriptor.Name}Client";

		private List<CallableMethodInfo> _callableMethods = new List<CallableMethodInfo>();

		public const string PARAMETER_STRING = "Parameter";
		public const string CLIENT_NAMESPACE = "Beamable.Server.Clients";

		private string ExtensionClassToFind => $"public class {TargetExtensionClassName}";
		private string ExtensionClassToReplace => $"public static class {TargetExtensionClassName}";

		public static string GetTargetParameterClassName(MicroserviceDescriptor descriptor) =>
			$"MicroserviceParameters{descriptor.Name}Client";

		public static string GetParameterClassName(Type parameterType) => $"{PARAMETER_STRING}{parameterType.GetTypeString().Replace(".", "_")}";

		public static Type GetDataWrapperTypeForParameter(MicroserviceDescriptor descriptor, Type parameterType)
		{
			var name =
				$"{CLIENT_NAMESPACE}.{GetTargetParameterClassName(descriptor)}+{GetParameterClassName(parameterType)}, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
			var t = Type.GetType(name, true, true);
			return t;
		}

		/// <summary>
		/// Define the class.
		/// </summary>
		/// <param name="serviceObject"></param>
		public ClientCodeGenerator(MicroserviceDescriptor descriptor)
		{
			Descriptor = descriptor;
			targetUnit = new CodeCompileUnit();
			CodeNamespace samples = new CodeNamespace(CLIENT_NAMESPACE);

			samples.Imports.Add(new CodeNamespaceImport("System"));
			samples.Imports.Add(new CodeNamespaceImport("Beamable.Platform.SDK"));
			samples.Imports.Add(new CodeNamespaceImport("Beamable.Server"));

			targetClass = new CodeTypeDeclaration(TargetClassName);
			targetClass.IsClass = true;
			targetClass.TypeAttributes =
				TypeAttributes.Public | TypeAttributes.Sealed;
			targetClass.BaseTypes.Add(new CodeTypeReference(MicroserviceClient_TypeName));
			targetClass.Members.Add(new CodeConstructor()
			{
				Attributes = MemberAttributes.Public,
				Parameters = { new CodeParameterDeclarationExpression(new CodeTypeReference("BeamContext"), "context = null") },
				BaseConstructorArgs = { new CodeArgumentReferenceExpression("context") }
			});



			parameterClass = new CodeTypeDeclaration(TargetParameterClassName);
			parameterClass.IsClass = true;
			parameterClass.TypeAttributes = TypeAttributes.NotPublic | TypeAttributes.Sealed | TypeAttributes.Serializable;
			// parameterClass.BaseTypes.Add();

			targetClass.Comments.Add(new CodeCommentStatement($"<summary> A generated client for <see cref=\"{Descriptor.Type.FullName}\"/> </summary", true));

			extensionClass = new CodeTypeDeclaration(TargetExtensionClassName);
			extensionClass.IsClass = true;
			extensionClass.TypeAttributes = TypeAttributes.Public;
			extensionClass.CustomAttributes = new CodeAttributeDeclarationCollection
			{
				new CodeAttributeDeclaration(new CodeTypeReference(typeof(BeamContextSystemAttribute)))
			};

			AddServiceNameInterface();
			AddFederatedLoginInterfaces();
			AddFederatedInventoryInterfaces();

			var registrationMethod = new CodeMemberMethod
			{
				Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Static
			};
			registrationMethod.CustomAttributes.Add(
				new CodeAttributeDeclaration(new CodeTypeReference(typeof(RegisterBeamableDependenciesAttribute))));
			registrationMethod.Name = "RegisterService";
			registrationMethod.Parameters.Add(
				new CodeParameterDeclarationExpression(typeof(IDependencyBuilder), "builder"));
			registrationMethod.Statements.Add(new CodeMethodInvokeExpression
			{
				Method = new CodeMethodReferenceExpression(
					new CodeArgumentReferenceExpression("builder"),
					nameof(IDependencyBuilder.AddScoped),
					new CodeTypeReference[] {
					  new CodeTypeReference(TargetClassName) })
			});

			var extensionMethod = new CodeMemberMethod()
			{
				Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Static
			};
			extensionMethod.Name = Descriptor.Name;
			extensionMethod.Parameters.Add(
				new CodeParameterDeclarationExpression($"this Beamable.Server.{MicroserviceClients_TypeName}", "clients"));
			extensionMethod.Statements.Add(new CodeMethodReturnStatement(new CodeMethodInvokeExpression
			{
				Method = new CodeMethodReferenceExpression(
					new CodeArgumentReferenceExpression("clients"),
					"GetClient",
					new CodeTypeReference[] {
					  new CodeTypeReference(TargetClassName)
					})
			}));
			extensionMethod.ReturnType = new CodeTypeReference(TargetClassName);

			extensionClass.Members.Add(registrationMethod);
			extensionClass.Members.Add(extensionMethod);

			samples.Types.Add(targetClass);
			samples.Types.Add(parameterClass);
			samples.Types.Add(extensionClass);
			targetUnit.Namespaces.Add(samples);

			// need to scan and get methods.
			var allMethods = descriptor.Type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			var allParameterTypes = new HashSet<Type>();
			foreach (var method in allMethods)
			{
				var clientCallable = method.GetCustomAttribute<CallableAttribute>();
				if (clientCallable == null)
				{
					continue;
				}

				var callable = new CallableMethodInfo
				{
					MethodInfo = method,
					ClientCallable = clientCallable
				};
				_callableMethods.Add(callable);

				AddCallableMethod(callable, allParameterTypes);
			}

			foreach (var parameterType in allParameterTypes)
			{
				AddParameterClass(parameterType);
			}

		}

		void AddFederatedLoginInterfaces()
		{
			var interfaces = Descriptor.Type.GetInterfaces();
			foreach (var type in interfaces)
			{
				if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IFederatedLogin<>))
				{
					var genericType = type.GetGenericArguments()[0];
					var baseReference = new CodeTypeReference(typeof(ISupportsFederatedLogin<>));
					baseReference.TypeArguments.Add(new CodeTypeReference(genericType));
					targetClass.BaseTypes.Add(baseReference);
				}
			}
		}

		void AddFederatedInventoryInterfaces()
		{
			var interfaces = Descriptor.Type.GetInterfaces();
			foreach (var type in interfaces)
			{
				if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IFederatedInventory<>))
				{
					var genericType = type.GetGenericArguments()[0];
					var baseReference = new CodeTypeReference(typeof(ISupportsFederatedInventory<>));
					baseReference.TypeArguments.Add(new CodeTypeReference(genericType));
					targetClass.BaseTypes.Add(baseReference);
				}
			}
		}

		void AddServiceNameInterface()
		{
			targetClass.BaseTypes.Add(new CodeTypeReference(typeof(IHaveServiceName)));


			var nameProperty = new CodeMemberProperty();
			nameProperty.Type = new CodeTypeReference(typeof(string));
			nameProperty.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			nameProperty.Name = nameof(IHaveServiceName.ServiceName);
			nameProperty.HasGet = true;
			nameProperty.HasSet = false;

			var returnStatement = new CodeMethodReturnStatement(new CodePrimitiveExpression(Descriptor.Name));
			nameProperty.GetStatements.Add(returnStatement);
			targetClass.Members.Add(nameProperty);
		}

		void AddParameterClass(Type parameterType)
		{
			var wrapper = new CodeTypeDeclaration(GetParameterClassName(parameterType));
			wrapper.IsClass = true;
			wrapper.CustomAttributes.Add(
				new CodeAttributeDeclaration(new CodeTypeReference(typeof(SerializableAttribute))));
			wrapper.TypeAttributes = TypeAttributes.NotPublic | TypeAttributes.Sealed | TypeAttributes.Serializable;
			wrapper.BaseTypes.Add(
				new CodeTypeReference("MicroserviceClientDataWrapper", new CodeTypeReference(parameterType)));

			parameterClass.Members.Add(wrapper);
		}

		void AddCallableMethod(CallableMethodInfo info, HashSet<Type> parameterTypes)
		{
			// Declaring a ToString method
			CodeMemberMethod genMethod = new CodeMemberMethod();
			genMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			genMethod.Name = info.MethodInfo.Name;

			// the input arguments...
			var serializationFields = new List<string>();
			var methodParams = info.MethodInfo.GetParameters();
			for (var i = 0; i < methodParams.Length; i++)
			{
				var methodParam = methodParams[i];
				var paramType = methodParam.ParameterType;
				var paramName = methodParam.Name;
				parameterTypes.Add(paramType);
				genMethod.Parameters.Add(new CodeParameterDeclarationExpression(paramType, paramName));

				var serializationFieldName = $"serialized_{paramName}";
				var declare = new CodeParameterDeclarationExpression(typeof(string), serializationFieldName);
				serializationFields.Add(serializationFieldName);

				var serializeInvoke = new CodeMethodInvokeExpression(
					new CodeMethodReferenceExpression(
						new CodeThisReferenceExpression(),
						"SerializeArgument",
						new CodeTypeReference[]
						{
						  new CodeTypeReference(paramType),
						}), new CodeExpression[]
					{
					new CodeArgumentReferenceExpression(paramName),
					});

				var assignment = new CodeAssignStatement(declare, serializeInvoke);
				genMethod.Statements.Add(assignment);
			}


			// add some docstrings to the method.
			genMethod.Comments.Add(new CodeCommentStatement("<summary>", true));
			genMethod.Comments.Add(new CodeCommentStatement($"Call the {info.MethodInfo.Name} method on the {Descriptor.Name} microservice", true));

			genMethod.Comments.Add(new CodeCommentStatement($"<see cref=\"{Descriptor.Type.FullName}.{info.MethodInfo.Name}\"/>", true));
			genMethod.Comments.Add(new CodeCommentStatement("</summary>", true));

			// the return type needs to be wrapped up inside a Promise.
			var promiseType = typeof(Promise<>);
			var resultType = info.MethodInfo.ReturnType;
			if (resultType == typeof(void) || resultType == typeof(Promise))
			{
				resultType = typeof(Unit);
			}
			else if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Promise<>))
			{
				resultType = resultType.GetGenericArguments()[0];
			}

			var isAsync = null != info.MethodInfo.GetCustomAttribute<AsyncStateMachineAttribute>();
			if (isAsync)
			{
				if (typeof(Task).IsAssignableFrom(resultType) && resultType.IsGenericType)
				{
					// oh, its an async call...
					resultType = resultType.GetGenericArguments()[0];
				}
			}

			var genericPromiseType = promiseType.MakeGenericType(resultType);
			genMethod.ReturnType = new CodeTypeReference(genericPromiseType);

			// Declaring a return statement for method ToString.
			var returnStatement = new CodeMethodReturnStatement();

			var servicePath = info.ClientCallable.PathName;
			var serviceName = Descriptor.Name;
			if (string.IsNullOrEmpty(servicePath))
			{
				servicePath = info.MethodInfo.Name;
			}

			// servicePath = $"micro_{Descriptor.Name}/{servicePath}"; // micro is the feature name, so we don't accidently stop out an existing service.


			var serializedFieldVariableName = "serializedFields";
			var fieldDeclare = new CodeParameterDeclarationExpression(typeof(string[]), serializedFieldVariableName);
			var fieldReferences = serializationFields.Select(f => new CodeVariableReferenceExpression(f)).ToArray();
			var fieldCreate = new CodeArrayCreateExpression(typeof(string[]), fieldReferences);

			genMethod.Statements.Add(new CodeAssignStatement(fieldDeclare, fieldCreate));

			var requestInvokeExpr = new CodeMethodInvokeExpression(
				new CodeMethodReferenceExpression(
					new CodeThisReferenceExpression(),
					"Request",
					new CodeTypeReference[]
					{
					  new CodeTypeReference(resultType),
					}),
				new CodeExpression[]
				{
                  // first argument is the service name
                  new CodePrimitiveExpression(serviceName),

                  // second argument is the path.
                  new CodePrimitiveExpression(servicePath),

                  // third argument is an array of pre-serialized json structures
                  new CodeVariableReferenceExpression(serializedFieldVariableName),
				});

			returnStatement.Expression = requestInvokeExpr;


			//returnStatement.ex
			genMethod.Statements.Add(returnStatement);
			targetClass.Members.Add(genMethod);
		}

		public string GetCSharpCodeString()
		{
			CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
			CodeGeneratorOptions options = new CodeGeneratorOptions();
			options.BracingStyle = "C";
			var sb = new StringBuilder();
			using (var sourceWriter = new StringWriter(sb))
			{
				provider.GenerateCodeFromCompileUnit(
					targetUnit, sourceWriter, options);
				sourceWriter.Flush();
				var source = sb.ToString();
				source = source.Replace(ExtensionClassToFind, ExtensionClassToReplace);
				return source;
			}
		}

		public void GenerateCSharpCode(string fileName)
		{
			File.WriteAllText(fileName, GetCSharpCodeString());
		}

		public class CallableMethodInfo
		{
			public MethodInfo MethodInfo;
			public CallableAttribute ClientCallable;
		}
	}
}
