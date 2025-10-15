using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace MoreStags {
    public class StagData {
        public static List<StagData> allStags = new();
        public static Dictionary<string, StagData> dataByRoom = new();
        private static int posNum = 1;

        public string name;
        public string scene;
        public string region;
        public int cost;
        public bool leftSide;
        public bool isCursed = false;
        public int positionNumber;
        public JsonVector2 _bellCoords;
        public float _transitionX;
        public JsonVector2 _stagCoords;
        public Vector3 bellPosition;
        public Vector3 transitionPosition;
        public Vector3 stagPosition;
        public JsonVector2 _markerPosition;
        public Vector2 markerPosition;
        public bool isVanilla = false;
        public string[] objectsToRemove;
        public string[] enemiesToRemove;
        //public float stagScale;

        public void translate() {
            allStags.Add(this);
            positionNumber = posNum++;
            dataByRoom.Add(scene, this);
            if(!isVanilla) {
                bellPosition = new Vector3(_bellCoords.x, _bellCoords.y + 1.1327f, 0.009f);
                transitionPosition = new Vector3(_transitionX, _bellCoords.y - 0.26f, 0.2f);
                stagPosition = new Vector3(_stagCoords.x, _stagCoords.y + 1.9f, 0.02f);
            }
            if(_markerPosition == null) {
                _markerPosition = new JsonVector2() { x = 1860, y = 570 };
            }
            markerPosition = new Vector2(_markerPosition.x / 431f - 4.28f, -_markerPosition.y / 297f + 4.2f);
            if(objectsToRemove == null)
                objectsToRemove = [];
            if(enemiesToRemove == null)
                enemiesToRemove = [];
        }

        public bool isActive(LocalData ld) {
            return ld.activeStags.Contains(this);
        }

        public override bool Equals(object obj) {
            if(obj is StagData data) {
                return name == data.name;
            }
            return false;
        }

        public override int GetHashCode() {
            return Tuple.Create(name, scene, region, cost).GetHashCode();
        }
    }

    public class ParseJson {
        private readonly string _jsonFilePath;
        private readonly Stream _jsonStream;
        private bool isPath;

        public ParseJson(string jsonFilePath) {
            _jsonFilePath = jsonFilePath;
            isPath = true;
        }

        public ParseJson(Stream jsonStream) {
            _jsonStream = jsonStream;
            isPath = false;
        }

        public List<T> parseFile<T>() {
            if(isPath) {
                using StreamReader reader = new(_jsonFilePath);
                var json = reader.ReadToEnd();
                List<T> values = JsonConvert.DeserializeObject<List<T>>(json);
                return values;
            }
            else {
                using StreamReader reader = new(_jsonStream);
                var json = reader.ReadToEnd();
                List<T> values = JsonConvert.DeserializeObject<List<T>>(json);
                return values;
            }
        }
    }

    public class JsonVector2 {
        public float x;
        public float y;
    }
}
