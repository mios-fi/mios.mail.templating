using System;
using System.IO;
using System.Reflection;

namespace Mios.Mail.Templating {
	public class RazorEmailTemplateFactory {
		private readonly RazorTemplateFactory factory;
		public Func<Type, string, string> NameMapping { get; set; }

		public RazorEmailTemplateFactory(params Assembly[] referencedAssemblies) 
			: this("Templates", referencedAssemblies) {
		}
		public RazorEmailTemplateFactory(string templatePath, params Assembly[] referencedAssemblies) {
			factory = new RazorTemplateFactory();
			foreach(var assembly in referencedAssemblies) {
				factory.ReferencedAssemblies.Add(assembly);
			}
			NameMapping = (type, extension) => Path.Combine(templatePath, type.Name + ".cs" + extension);
		}

		public IEmailTemplate<T> Create<T>() where T : class {
			// Load subject and text templates (required)
			RazorTemplate<T> subjectTemplate, textTemplate;
			var textTemplateFilename = NameMapping.Invoke(typeof(T), "txt");
			using(var reader = GetReader(textTemplateFilename)) {
				if(reader==null) return null;
				var subjectLine = reader.ReadLine();
				subjectTemplate = factory.CreateTemplate<T>(new StringReader(subjectLine ?? String.Empty));
				textTemplate = factory.CreateTemplate<T>(reader);
			}

			// Load optional html template if defined
			RazorTemplate<T> htmlTemplate;
			var htmlTemplateFilename = NameMapping.Invoke(typeof(T),"html");
			using(var reader = GetReader(htmlTemplateFilename)) {
				htmlTemplate = reader!=null ? factory.CreateTemplate<T>(reader) : null;
			}

			return new RazorEmailTemplate<T>(subjectTemplate, textTemplate, htmlTemplate);
		}
		protected virtual TextReader GetReader(string name) {
			if(!File.Exists(name)) return null;
			return new StreamReader(File.OpenRead(name));
		}
	}
}