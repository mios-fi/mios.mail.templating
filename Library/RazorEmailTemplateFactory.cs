using System;
using System.IO;

namespace Mios.Mail.Templating {
	public class RazorEmailTemplateFactory {
		public RazorTemplateFactory templateFactory;
		public RazorEmailTemplateFactory() : this(new RazorTemplateFactory()) {
		}
		public RazorEmailTemplateFactory(RazorTemplateFactory templateFactory) {
			this.templateFactory = templateFactory;
		}
		public IEmailTemplate<dynamic> Create(string textTemplateFileName, string htmlTemplateFileName) {
			return Create(templateFactory.CreateDynamicTemplate, templateFactory.CreateDynamicHtmlTemplate, textTemplateFileName, htmlTemplateFileName);
		}
		public IEmailTemplate<T> Create<T>(string textTemplateFileName, string htmlTemplateFileName) {
			var factory = new RazorTemplateFactory();
			return Create(templateFactory.CreateTemplate<T>, templateFactory.CreateHtmlTemplate<T>, textTemplateFileName,htmlTemplateFileName);
		}
		private IEmailTemplate<T> Create<T>(Func<TextReader,RazorTemplate<T>> textFactory, Func<TextReader,RazorHtmlTemplate<T>> htmlFactory, string textTemplateFileName, string htmlTemplateFileName) {
			// Load subject and body from required text template
			RazorTemplate<T> subjectTemplate, textTemplate;
			using(var reader = OpenTemplateReader(textTemplateFileName)) {
				if(reader==null) {
					throw new FileNotFoundException(textTemplateFileName);
				}
				var subjectLine = reader.ReadLine();
				subjectTemplate = textFactory(new StringReader(subjectLine ?? String.Empty));
				textTemplate = textFactory(reader);
			}
			// Load optional html template if defined
			RazorHtmlTemplate<T> htmlTemplate;
			using(var reader = OpenTemplateReader(htmlTemplateFileName)) {
				htmlTemplate = reader!=null ? htmlFactory(reader) : null;
			}
			return new RazorEmailTemplate<T>(subjectTemplate, textTemplate, htmlTemplate);
		}
		protected virtual TextReader OpenTemplateReader(string fileName) {
			return !String.IsNullOrWhiteSpace(fileName) && File.Exists(fileName) 
				? new StreamReader(File.OpenRead(fileName)) 
				: null;
		}
	}
}