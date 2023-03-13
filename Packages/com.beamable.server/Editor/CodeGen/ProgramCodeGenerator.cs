using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Beamable.Server.Editor.CodeGen
{
	public class ProgramCodeGenerator
	{
		public MicroserviceDescriptor Descriptor { get; }

		CodeCompileUnit targetUnit;
		CodeTypeDeclaration targetClass;


		public ProgramCodeGenerator(MicroserviceDescriptor descriptor)
		{
			Descriptor = descriptor;

			Descriptor = descriptor;
			targetUnit = new CodeCompileUnit();
			CodeNamespace samples = new CodeNamespace("Beamable.Server");

			samples.Imports.Add(new CodeNamespaceImport(descriptor.Type.Namespace));
			samples.Imports.Add(new CodeNamespaceImport("Beamable.Common"));
			targetClass = new CodeTypeDeclaration("Program");
			targetClass.IsClass = true;
			targetClass.TypeAttributes = TypeAttributes.Public;

			var mainmethod = new CodeMemberMethod
			{
				Attributes = MemberAttributes.Public | MemberAttributes.Static
			};
			mainmethod.Name = "Main";
			mainmethod.Parameters.Add(
				new CodeParameterDeclarationExpression(typeof(string[]), "args"));
			mainmethod.ReturnType = new CodeTypeReference(typeof(Task<int>));

			var baseType = new CodeTypeReference("global::Beamable.Server.CommandLine", new CodeTypeReference(descriptor.Type));
			//var commandLineType = new CodeTypeReferenceExpression("global::Beamable.Server.CommandLine");

			var invokeExpr = new CodeMethodInvokeExpression(
			   new CodeMethodReferenceExpression(
				  new CodeTypeReferenceExpression(baseType), // XXX: This is super brittle...
				  "Main"),
			   new CodeExpression[]
			   {
				   new CodeArgumentReferenceExpression("args")
			   });
			var returnExpr = new CodeMethodReturnStatement(invokeExpr);
			mainmethod.Statements.Add(returnExpr);

			targetClass.Members.Add(mainmethod);

			samples.Types.Add(targetClass);
			targetUnit.Namespaces.Add(samples);
		}

		public void GenerateCSharpCode(string fileName)
		{
			CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
			CodeGeneratorOptions options = new CodeGeneratorOptions();
			options.BracingStyle = "C";
			using (StreamWriter sourceWriter = new StreamWriter(fileName))
			{
				provider.GenerateCodeFromCompileUnit(
				   targetUnit, sourceWriter, options);
			}
		}

	}
}
