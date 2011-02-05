using System.Text;

namespace Mios.Mail.Templating {
	public abstract class RazorTemplate<T> {
		public T Model { get; set; }
		private StringBuilder buffer;
		public abstract void Execute();
		public string Execute(T model) {
			Model = model;
			buffer = new StringBuilder();
			Execute();
			return buffer.ToString();
		}
		public virtual void Write(object value) {
			WriteLiteral(value);
		}
		public virtual void WriteLiteral(object value) {
			buffer.Append(value);
		}
	}
}