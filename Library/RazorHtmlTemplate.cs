using System.Text;
using System.Web;

namespace Mios.Mail.Templating {
	public abstract class RazorHtmlTemplate<T> : RazorTemplate<T> {
		public override void Write(object value) {
			if(value is IHtmlString)
				WriteLiteral(value);
			else 
				WriteLiteral(HttpUtility.HtmlEncode(value));
		}
	}
	public abstract class DynamicRazorHtmlTemplate : RazorHtmlTemplate<dynamic> {
	}
}