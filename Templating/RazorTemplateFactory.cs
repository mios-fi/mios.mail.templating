using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web.Razor;
using Microsoft.CSharp;

namespace Mios.Mail.Templating {
	[Serializable]
	public class RazorTemplateFactoryException : Exception {
		public RazorTemplateFactoryException() {}
		public RazorTemplateFactoryException(string messageFormat, params object[] parameters) : base(String.Format(messageFormat,parameters)) { }
		public RazorTemplateFactoryException(string message) : base(message) { }
		public RazorTemplateFactoryException(string message, Exception inner) : base(message, inner) { }
		protected RazorTemplateFactoryException( SerializationInfo info, StreamingContext context) : base(info, context) {}
	}

	public class RazorTemplateFactory {

		private IList<Assembly> referencedAssemblies;
		public IList<Assembly> ReferencedAssemblies { 
			get { return referencedAssemblies; } 
		}

		public RazorTemplateFactory() {
			referencedAssemblies = new List<Assembly> {
				typeof(RazorTemplateFactory).Assembly
			};
		}

		public RazorTemplate<T> CreateTemplate<T>(TextReader sourceReader) where T : class {
			// Generate code for the template
			var engine = CreateEngine<T>();
			var razorResult = engine.GenerateCode(sourceReader);
			var codeProvider = new CSharpCodeProvider();

			// Generate the code and put it in the text box:
			using(var sw = new StringWriter()) {
				codeProvider.GenerateCodeFromCompileUnit(razorResult.GeneratedCode, sw, new CodeGeneratorOptions());
				Debug.WriteLine(sw.GetStringBuilder().ToString());
			}

			// Compile the generated code into an assembly
			var results = codeProvider.CompileAssemblyFromDom(
			  new CompilerParameters(
			    referencedAssemblies.Select(t=>t.CodeBase.Replace("file:///", "").Replace("/", "\\")).ToArray()
				),
			  razorResult.GeneratedCode
			);

			// Check for compilation errors
			if(results.Errors.HasErrors) {
				var err = results.Errors
					.OfType<CompilerError>()
					.First(ce => !ce.IsWarning);
				throw new RazorTemplateFactoryException(
					"Error Compiling Template: ({0}, {1}) {2}",
					err.Line, err.Column, err.ErrorText);
			}

			// Get the template type
			var typ = results.CompiledAssembly.GetType("RazorOutput.Template");
			if(typ == null) {
				throw new RazorTemplateFactoryException(
					"Could not find generated type in assembly {0}", 
					results.CompiledAssembly.FullName);
			}

			// Create instance
			var newTemplate = Activator.CreateInstance(typ) as RazorTemplate<T>;
			if(newTemplate == null) {
				throw new RazorTemplateFactoryException(
					"Could not construct template or it does not inherit from {0}",
					typeof(RazorTemplate<T>).Name);
			}
			return newTemplate;
		}

		protected static RazorTemplateEngine CreateEngine() {
			return CreateEngine<Object>();
		}
		protected static RazorTemplateEngine CreateEngine<T>() {
			var host = new RazorEngineHost(new CSharpRazorCodeLanguage()) {
				DefaultBaseClass = typeof(RazorTemplate<T>).FullName,
				DefaultNamespace = "RazorOutput",
				DefaultClassName = "Template",
			};
			host.NamespaceImports.Add("System");
			return new RazorTemplateEngine(host);
		}
	}
}