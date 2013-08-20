using System;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;

namespace Mios.Mail.Templating {
	public class RazorEmailTemplate<T> : IEmailTemplate<T> {
		private readonly RazorTemplate<T> subjectTemplate;
		private readonly RazorTemplate<T> textTemplate;
		private readonly RazorTemplate<T> htmlTemplate;

		public RazorEmailTemplate(RazorTemplate<T> subjectTemplate, RazorTemplate<T> textTemplate, RazorTemplate<T> htmlTemplate) {
			if(subjectTemplate == null) throw new ArgumentNullException("subjectTemplate");
			if(textTemplate == null) throw new ArgumentNullException("textTemplate");
			this.subjectTemplate = subjectTemplate;
			this.textTemplate = textTemplate;
			this.htmlTemplate = htmlTemplate;
		}
		public MailMessage Transform(T model) {
			var message = new MailMessage {
				Subject = subjectTemplate.Execute(model), 
				Body = textTemplate.Execute(model)
			};
			if(htmlTemplate!=null) {
				var htmlBody = htmlTemplate.Execute(model);
				var stream = new MemoryStream(Encoding.UTF8.GetBytes(htmlBody));
				var htmlView = new AlternateView(stream, new ContentType("text/html") { CharSet = "utf-8" });
				message.AlternateViews.Add(htmlView);
			}
			return message;
		}
		public MailMessage Transform(T model, MailAddress to, MailAddress from) {
			var message = Transform(model);
			message.To.Add(to);
			message.From = from;
			return message;
		}
	}
}