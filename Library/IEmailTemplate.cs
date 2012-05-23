using System.Net.Mail;

namespace Mios.Mail.Templating {
	public interface IEmailTemplate<in TModel> {
		MailMessage Transform(TModel model);
		MailMessage Transform(TModel model, MailAddress to, MailAddress from);
	}
}
