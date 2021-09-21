using option;
using jonson;
using jonson.reflect;
using System.IO;

namespace json {
    public static class SaveLoad {
        public static JSONType GetJsonType<T>(T type) {
            return Reflect.ToJSON(type, true);
        }

        public static void WriteJson(JSONType jsonType, string path) {
            string output;
            output = Jonson.Generate(jsonType);
            File.WriteAllText(path, output);
        }

        public static T ReadJson<T>(string path, T type) {
            string input = File.ReadAllText(path);
            Result<JSONType, JSONError> gameStatsRes = Jonson.Parse(input, 1024);
            var loadGameStats =  Reflect.FromJSON(type, gameStatsRes.AsOk());

            return loadGameStats;
        }
    }
}