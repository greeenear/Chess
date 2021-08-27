using System.IO;
using jonson;

namespace save {
    public static class Save {
        public static void WriteJson(JSONType jsonType, string path) {
            string output;
            output = Jonson.Generate(jsonType);
            File.WriteAllText(path, output);
        }
    }
}
