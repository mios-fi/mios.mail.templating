using System;
using System.CodeDom;
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
		public IList<string> NamespaceImports { get; protected set; }
		public IList<string> AssemblyReferences { get; protected set; }
		public RazorTemplateFactory() {
			NamespaceImports = new List<string>();
			AssemblyReferences = new List<string>();
		}
		public RazorTemplate<T> CreateTemplate<T>(TextReader sourceReader) {
			return CreateTemplateImpl<RazorTemplate<T>>(sourceReader, NamespaceImports, AssemblyReferences);
		}
		public RazorHtmlTemplate<T> CreateHtmlTemplate<T>(TextReader sourceReader) {
			return CreateTemplateImpl<RazorHtmlTemplate<T>>(sourceReader, NamespaceImports, AssemblyReferences);
		}
		public DynamicRazorTemplate CreateDynamicTemplate(TextReader sourceReader) {
			return CreateTemplateImpl<DynamicRazorTemplate>(sourceReader, NamespaceImports, AssemblyReferences);
		}
		public DynamicRazorHtmlTemplate CreateDynamicHtmlTemplate(TextReader sourceReader) {
			return CreateTemplateImpl<DynamicRazorHtmlTemplate>(sourceReader, NamespaceImports, AssemblyReferences);
		}

		private static T CreateTemplateImpl<T>(TextReader sourceReader, IEnumerable<string> namespaceImports, IEnumerable<string> assemblyReferences) where T : class {
			var host = new RazorEngineHost(new CSharpRazorCodeLanguage());
			host.DefaultBaseClass = typeof(T).FullName;
			host.NamespaceImports.Add("System");
			foreach(var ns in namespaceImports)
				host.NamespaceImports.Add(ns);
			var result = new RazorTemplateEngine(host).GenerateCode(sourceReader);
			var templateAssembly = CompileTemplate(result.GeneratedCode, assemblyReferences);
			var typeName = host.DefaultNamespace+"."+host.DefaultClassName;
			return InstantiateTemplate<T>(templateAssembly, typeName);
		}

		private static Assembly CompileTemplate(CodeCompileUnit codeCompileUnit, IEnumerable<string> assemblyReferences) {
			var codeProvider = new CSharpCodeProvider();
			var parameters = new CompilerParameters(
				AppDomain.CurrentDomain.GetAssemblies()
					.Where(t => !t.IsDynamic)
					.Concat(new[] { Assembly.Load("Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a") })
					.Concat(assemblyReferences.Select(t => Assembly.Load(t)))
					.Select(t => t.Location)
					.Distinct()
					.ToArray()
			);
			parameters.GenerateInMemory = true;
			var results = codeProvider.CompileAssemblyFromDom(parameters,codeCompileUnit);
			if(results.Errors.HasErrors) {
				var err = results.Errors
					.OfType<CompilerError>()
					.First(ce => !ce.IsWarning);
				throw new RazorTemplateFactoryException(
					"Error Compiling Template: ({0}, {1}) {2}",
					err.Line, err.Column, err.ErrorText);
			}
			return results.CompiledAssembly;
		}

		private static T InstantiateTemplate<T>(Assembly templateAssembly, string typeName) where T: class {
			var templateType = templateAssembly.GetType(typeName);
			if(templateType == null) {
				throw new RazorTemplateFactoryException(
					"Could not find requested type {0} in assembly {1}", 
					typeName, templateAssembly.FullName);
			}
			var template = Activator.CreateInstance(templateType) as T;
			if(template == null) {
				throw new RazorTemplateFactoryException(
					"Could not construct template or it does not inherit from {0}",
					typeof(T).Name);
			}
			return template;
		}
	}
}