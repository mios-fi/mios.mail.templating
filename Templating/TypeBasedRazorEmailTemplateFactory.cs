using System.IO;

namespace Mios.Mail.Templating {
	public class TypeBasedRazorEmailTemplateFactory : RazorEmailTemplateFactory {
		private readonly string templatePath;

		public TypeBasedRazorEmailTemplateFactory() : this("Templates") {
		}
		public TypeBasedRazorEmailTemplateFactory(string templatePath) {
			this.templatePath = templatePath;
		}

		public IEmailTemplate<T> Create<T>() where T : class {
			var textTemplateFileName = Path.Combine(templatePath, typeof(T).Name+".cstxt");
			var htmlTemplateFileName = Path.Combine(templatePath, typeof(T).Name+".cshtml");
			return Create<T>(textTemplateFileName, htmlTemplateFileName);
		}
	}
}