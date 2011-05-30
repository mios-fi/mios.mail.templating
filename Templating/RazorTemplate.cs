using System.Text;

namespace Mios.Mail.Templating {
	public abstract class RazorTemplate<T> {
		public T Model { get; set; }
		protected StringBuilder Buffer = new StringBuilder();
		public abstract void Execute();
		public string Execute(T model) {
			Model = model;
			Buffer.Clear();
			Execute();
			return Buffer.ToString();
		}
		public virtual void Write(object value) {
			WriteLiteral(value);
		}
		public virtual void WriteLiteral(object value) {
			Buffer.Append(value);
		}
	}
	public abstract class DynamicRazorTemplate : RazorTemplate<dynamic> {
	}
}