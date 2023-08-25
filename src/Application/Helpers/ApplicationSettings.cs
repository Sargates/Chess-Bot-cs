using System.Dynamic;
using ChessBot.Helpers;

namespace ChessBot.Application;
public class ApplicationSettings : DynamicObject {
	private static dynamic? singleton;
	public readonly IDictionary<string, object?> _dictionary;

	private ApplicationSettings() {
		string path = FileHelper.GetResourcePath("settings.txt");
		if (! File.Exists(path)) { File.Copy(FileHelper.GetResourcePath("default.txt"), path); }
		_dictionary = LoadSettingsFromFile(path);
	}

	public static dynamic Get() {
		singleton ??= new ApplicationSettings();
		return singleton;
	}

	public static void ResetDefaultSettings() {
		File.Delete(FileHelper.GetResourcePath("settings.txt"));
		// singleton = new ApplicationSettings();
		Console.WriteLine("Settings reset to default, relaunch application to take effect");
	}

	public override bool TrySetMember(SetMemberBinder binder, object? value) {
        _dictionary[binder.Name] = value;
        return true;
    }

	public override bool TryGetMember(GetMemberBinder binder, out object? result) {
		if (_dictionary.TryGetValue(binder.Name, out result)) { return true; }
		else { result = null; return true; }
	}

	public static IDictionary<string, object?> LoadSettingsFromFile(string filePath) {
		IDictionary<string, object?> settings = new Dictionary<string, object?>();

		string[] lines = File.ReadAllLines(filePath);

		foreach (string line in lines) {
			Console.WriteLine(line);
			string[] parts = line.Split('=');
			if (parts.Length >= 2) {
				string key = parts[0].Trim();
				string value = parts[1].Trim();

				if (float.TryParse(value, out float floatValue)) {
					settings[key] = floatValue;
				} else
				if (int.TryParse(value, out int intValue)) {
					settings[key] = intValue;
				}
				else {
					settings[key] = value;
				}
			}
		}

		return settings;
	}
}