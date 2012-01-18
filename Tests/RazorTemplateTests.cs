using System.Dynamic;
using System.IO;
using Mios.Mail.Templating;
using Xunit;

namespace Tests {
	public class RazorTemplateTests {
		[Fact]
		public void CanProcessDynamicModel() {
			var factory = new RazorTemplateFactory();
			const string str = "Hello @Model.Name you are number @Model.Count in line.";
			dynamic model = new ExpandoObject();
			model.Count = 42;
			model.Name = "Bob";
			var template = factory.CreateDynamicTemplate(new StringReader(str));
			Assert.Equal("Hello Bob you are number 42 in line.", template.Execute(model) as string);
		}
	}
}
