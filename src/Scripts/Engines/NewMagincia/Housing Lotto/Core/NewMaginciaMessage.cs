using System;

namespace Server.Engines.NewMagincia;

public class NewMaginciaMessage
{
	public static readonly TimeSpan DefaultExpirePeriod = TimeSpan.FromDays(7);

	public TextDefinition Title { get; }

	public TextDefinition Body { get; }

	public string Args { get; }

	public DateTime Expires { get; }

	public bool AccountBound { get; }

	public bool Expired => Expires < DateTime.UtcNow;

	public NewMaginciaMessage(TextDefinition title, TextDefinition body)
		: this(title, body, DefaultExpirePeriod, null, false)
	{
	}

	public NewMaginciaMessage(TextDefinition title, TextDefinition body, bool accountBound)
		: this(title, body, DefaultExpirePeriod, null, accountBound)
	{
	}

	public NewMaginciaMessage(TextDefinition title, TextDefinition body, string args)
		: this(title, body, DefaultExpirePeriod, args, false)
	{
	}

	public NewMaginciaMessage(TextDefinition title, TextDefinition body, string args, bool accountBound)
		: this(title, body, DefaultExpirePeriod, args, accountBound)
	{
	}

	public NewMaginciaMessage(TextDefinition title, TextDefinition body, TimeSpan expires)
		: this(title, body, expires, null, false)
	{
	}

	public NewMaginciaMessage(TextDefinition title, TextDefinition body, TimeSpan expires, bool accountBound)
		: this(title, body, expires, null, accountBound)
	{
	}

	public NewMaginciaMessage(TextDefinition title, TextDefinition body, TimeSpan expires, string args)
		: this(title, body, expires, args, false)
	{
	}

	public NewMaginciaMessage(TextDefinition title, TextDefinition body, TimeSpan expires, string args, bool accountBound)
	{
		Title = title;
		Body = body;
		Args = args;
		Expires = DateTime.UtcNow + expires;

		AccountBound = accountBound;
	}

	public void Serialize(GenericWriter writer)
	{
		writer.Write(0);

		writer.Write(AccountBound);
		TextDefinition.Serialize(writer, Title);
		TextDefinition.Serialize(writer, Body);
		writer.Write(Expires);
		writer.Write(Args);
	}

	public NewMaginciaMessage(GenericReader reader)
	{
		var v = reader.ReadInt();

		switch (v)
		{
			case 0:
				AccountBound = reader.ReadBool();
				Title = TextDefinition.Deserialize(reader);
				Body = TextDefinition.Deserialize(reader);
				Expires = reader.ReadDateTime();
				Args = reader.ReadString();
				break;
		}
	}
}
