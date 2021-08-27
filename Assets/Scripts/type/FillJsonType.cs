using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using option;
using jonson;
using jonson.reflect;
using System.IO;

namespace type {
    public static class FillJsonType {
        public static JSONType GetJsonType<T>(T type) {
            return Reflect.ToJSON(type, true);
        }
    }
}

