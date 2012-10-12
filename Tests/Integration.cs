using System.IO;
using System.Net.Mail;
using Mios.Mail.Templating;
using Xunit;

namespace Tests {
	public class Integration {
    [Fact(Skip="Dependencies")]
    public void CanTransformTypedMessageToEmail() {
			var factory = new TypeBasedRazorEmailTemplateFactory();
			var template = factory.Create<User>();
			var from = new MailAddress("info@example.com");
			var to = new MailAddress("bill@example.com");
			MailMessage message;
			message = template.Transform(new User { Name = "Bob" }, to, from);
			Assert.Contains(to,message.To);
		  Assert.Equal(from,message.From);
			Assert.Equal("Hello Bob", message.Subject);
			Assert.Equal("Hi there Bob, how are you today?", message.Body);
			using(var reader = new StreamReader(message.AlternateViews[0].ContentStream)) {
				var body = reader.ReadToEnd();
				Assert.Contains("<h1>Hi Bob</h1>", body);
				Assert.Contains("<title>Hello Bob</title>", body);
			}
		}
    [Fact(Skip="Dependencies")]
    public void CanTransformDynamicMessageToEmail() {
			var factory = new RazorEmailTemplateFactory();
			var template = factory.Create("Templates\\User.cstxt","Templates\\User.cshtml");
			var from = new MailAddress("info@example.com");
			var to = new MailAddress("bill@example.com");
			MailMessage message;
			message = template.Transform(new User {Name = "Bob"}, to, from);
			Assert.Contains(to, message.To);
			Assert.Equal(from, message.From);
			Assert.Contains("Hello Bob", message.Subject);
      Assert.Equal("Hi there Bob, how are you today?", message.Body);
      using(var reader = new StreamReader(message.AlternateViews[0].ContentStream)) {
        var body = reader.ReadToEnd();
        Assert.Contains("<h1>Hi Bob</h1>", body);
        Assert.Contains("<title>Hello Bob</title>", body);
      }
    }
		public class User {
			public string Name { get; set; }
		}
	}
}
