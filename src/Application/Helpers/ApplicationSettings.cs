using System.Dynamic;

namespace ChessBot.Application;
public class ApplicationSettings : DynamicObject {
	public readonly IDictionary<string, object?> _dictionary;

	public ApplicationSettings(string filePath) {
		if (! File.Exists(filePath)) { File.Create(filePath); }
		_dictionary = LoadSettingsFromFile(filePath);
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