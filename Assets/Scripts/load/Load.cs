using System.IO;
using jonson;
using jonson.reflect;
using option;

namespace load {
    public static class Load<T> {
        public static T LoadFromJson(string path, T type) {
            string input = File.ReadAllText(path);
            Result<JSONType, JSONError> gameStatsRes = Jonson.Parse(input, 1024);
            var loadGameStats =  Reflect.FromJSON(type, gameStatsRes.AsOk());

            return loadGameStats;
        }
    }
}