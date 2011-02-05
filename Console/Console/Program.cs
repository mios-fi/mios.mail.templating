using System;
using System.IO;
using System.Net.Mail;
using Mios.Mail.Templating;

namespace ConsoleApp {
	public class User {
		public string Name { get; set; }
	}
	class Program {
		static void Main() {
			new Program().Run();
		}
		public void Run() {
			var factory = new RazorEmailTemplateFactory(typeof(User).Assembly);
			var template = factory.Create<User>();
			var from = new MailAddress("info@example.com");
			var to = new MailAddress("bill@example.com");
			Console.WriteLine(template.Transform(new User { Name = "Bob" }, to, from).ToDetailedString());
			Console.WriteLine(template.Transform(new User { Name = "Bill" }, to, from).ToDetailedString());
			Console.WriteLine(template.Transform(new User { Name = "Steve" }, to, from).ToDetailedString());
		}
	}

	public static class MailMessageExtensions {
		public static string ToDetailedString(this MailMessage message) {
			string htmlSource = String.Empty;
			if(message.AlternateViews.Count>0) {
				var stream = message.AlternateViews[0].ContentStream;
				htmlSource = "Html:\n" + new StreamReader(stream).ReadToEnd();
				stream.Position = 0;
			}
			return String.Format("To: {0}\nFrom: {1}\nSubject: {2}\nBody:\n{3}", message.To[0], message.From, message.Subject,
			                     message.Body)+htmlSource;
		}
	}
}
