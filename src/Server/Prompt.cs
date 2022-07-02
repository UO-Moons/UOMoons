namespace Server.Prompts
{
	public abstract class Prompt
	{
		public IEntity Sender { get; }
		public string MessageArgs { get; }
		public virtual int MessageCliloc => 1042971;
		public virtual int MessageHue => 0;
		public virtual int TypeId => GetType().FullName!.GetHashCode();

		public Prompt()
			: this(null)
		{
		}

		public Prompt(IEntity sender)
			: this(sender, string.Empty)
		{
		}

		public Prompt(IEntity sender, string args)
		{
			Sender = sender;
			MessageArgs = args;
		}

		public virtual void OnCancel(Mobile from)
		{
		}

		public virtual void OnResponse(Mobile from, string text)
		{
		}
	}
}
