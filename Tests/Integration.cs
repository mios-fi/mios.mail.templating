using System.IO;
using System.Net.Mail;
using Mios.Mail.Templating;
using Should;
using Xunit;

namespace Tests {
	public class Integration {
		[Fact]
		public void CanTransformTypedMessageToEmail() {
			var factory = new TypeBasedRazorEmailTemplateFactory();
			var template = factory.Create<User>();
			var from = new MailAddress("info@example.com");
			var to = new MailAddress("bill@example.com");
			MailMessage message;
			message = template.Transform(new User { Name = "Bob" }, to, from);
			message.To.ShouldContain(to);
			message.From.ShouldEqual(from);
			message.Subject.ShouldEqual("Hello Bob");
			message.Body.ShouldEqual("Hi there Bob, how are you today?");
			using(var reader = new StreamReader(message.AlternateViews[0].ContentStream)) {
				var body = reader.ReadToEnd();
				body.ShouldContain("<h1>Hi Bob</h1>");
				body.ShouldContain("<title>Hello Bob</title>");
			}
		}
		[Fact]
		public void CanTransformDynamicMessageToEmail() {
			var factory = new RazorEmailTemplateFactory();
			var template = factory.Create("Templates\\User.cstxt","Templates\\User.cshtml");
			var from = new MailAddress("info@example.com");
			var to = new MailAddress("bill@example.com");
			MailMessage message;
			message = template.Transform(new User {Name = "Bob"}, to, from);
			message.To.ShouldContain(to);
			message.From.ShouldEqual(from);
			message.Subject.ShouldEqual("Hello Bob");
			message.Body.ShouldEqual("Hi there Bob, how are you today?");
			using(var reader = new StreamReader(message.AlternateViews[0].ContentStream)) {
				var body = reader.ReadToEnd();
				body.ShouldContain("<h1>Hi Bob</h1>");
				body.ShouldContain("<title>Hello Bob</title>");
			}
		}
		public class User {
			public string Name { get; set; }
		}
	}
}
