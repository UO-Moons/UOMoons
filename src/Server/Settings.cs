using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Server
{
	public class Settings
	{
		public static readonly Settings Configuration = new("settings.ini");

		public Dictionary<string, Dictionary<string, Entry>> Values { get; set; } = new(StringComparer.OrdinalIgnoreCase);

		public string Filename { get; set; }

		public Settings(string name)
		{
			Filename = name;
			Init();
		}

		public sealed class Entry
		{
			public string Section { get; set; }
			public string Key { get; set; }
			public string Value { get; set; }

			public Entry(string section, string key, string value)
			{
				Section = section;
				Key = key;
				Value = value;
			}
		}

		private void Init()
		{
			if (!Directory.Exists(Core.BaseDirectory))
			{
				_ = Directory.CreateDirectory(Core.BaseDirectory);
			}

			try
			{
				LoadFile(Path.Combine(Core.BaseDirectory, Filename));
			}
			catch (Exception e)
			{
				Utility.WriteConsole(ConsoleColor.Red, $"Failed to load settings {e.Message}");

				Console.WriteLine("Press any key to exit...");
				_ = Console.ReadKey();

				Core.Kill(false);

				return;
			}

			if (Core.Debug)
			{
				Utility.WriteConsole(ConsoleColor.Cyan, "\n[Server Settings]");
				foreach (KeyValuePair<string, Dictionary<string, Entry>> setting in Values)
				{
					foreach (KeyValuePair<string, Entry> entrie in setting.Value)
					{
						Utility.WriteConsole(ConsoleColor.Cyan, $"[{setting.Key}] {entrie.Value.Key}={entrie.Value.Value}");
					}
				}
				Console.WriteLine();
			}
		}

		private void LoadFile(string path)
		{
			FileInfo info = new(path);

			if (!info.Exists)
			{
				throw new FileNotFoundException();
			}

			string[] lines = File.ReadAllLines(info.FullName);
			string section = "";
			for (int i = 0; i < lines.Length; i++)
			{
				string line = lines[i].Trim();

				if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//") || line.StartsWith("#"))
				{
					continue;
				}

				if (line.StartsWith("["))
				{
					section = line.TrimStart('[').TrimEnd(']').Trim();
					continue;
				}

				int io = line.IndexOf('=');
				if (io < 0)
				{
					throw new FormatException($"Bad format at line {i + 1}");
				}

				string key = line[..io];
				string val = line[(io + 1)..];

				if (string.IsNullOrWhiteSpace(key))
				{
					throw new NullReferenceException($"Key can not be null at line {i + 1}");
				}

				key = key.Trim();

				if (string.IsNullOrEmpty(val))
				{
					val = null;
				}

				if (Values.TryGetValue(section, out Dictionary<string, Entry> entries))
				{
					entries[key] = new Entry(section, key, val);
				}
				else
				{
					Dictionary<string, Entry> newEntries = new()
					{
						{ key, new Entry(section, key, val) }
					};
					Values.Add(section, newEntries);
				}
			}
		}

		private string InternalGet(string section, string key)
		{
			string result = null;
			if (!Values.TryGetValue(section, out Dictionary<string, Entry> sec) || sec == null) return null;
			if (sec.TryGetValue(key, out var entry) && entry != null)
			{
				result = entry.Value;
			}

			return result;
		}

		public T Get<T>(string section, string key)
		{
			string returnValue = InternalGet(section, key);
			if (string.IsNullOrEmpty(returnValue))
			{
				Utility.WriteConsole(ConsoleColor.Red, $"[Settings] Failed to get {key} value in {section} section");
			}
			return ConvertValue<T>(InternalGet(section, key));
		}

		public T Get<T>(string section, string key, T defaultValue)
		{
			return string.IsNullOrEmpty(InternalGet(section, key)) ? defaultValue : ConvertValue<T>(InternalGet(section, key));
		}

		private static T ConvertValue<T>(string value)
		{
			return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
		}
	}
}
