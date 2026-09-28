using System;
using System.Collections.Generic;
using UnityEngine;

namespace OrbitalSalvage.Data
{
    [Serializable]
    public class DividerData
    {
        public float rMin = 85f;
        public float rMax = 155f;
        public float angle = 0f;
        public bool isInner = false;
        public bool isMoving = false;
        public float speed = 0f;
        public float range = 0f;
    }

    [Serializable]
    public class GateData
    {
        public string type = "MULTIPLY"; // MULTIPLY, ADD, BOOSTER, FLAPPER
        public int value = 2;
        public float angle = 0f;
        public float rMin = 85f;
        public float rMax = 160f;
        public bool isInner = false;
        public string label = "x2";
    }

    [Serializable]
    public class PinData
    {
        public float r = 120f;
        public float angle = 0f;
        public float restitution = 0.85f;
        public bool isInner = false;
    }

    [Serializable]
    public class FlapData
    {
        public float angle = 0f;
        public float rMin = 85f;
        public float rMax = 155f;
        public int openDir = 1; // 1 = clockwise, -1 = counter-clockwise
        public bool isInner = false;
    }

    [Serializable]
    public class SectorDefinition
    {
        public string id = "level_1";
        public int levelNumber = 1;
        public int worldIndex = 1;
        public string name = "Sector 1";
        public string code = "LVL 1";
        public string description = "";
        public int quota = 20;
        public int initialDebrisCount = 10;
        public string[] initialDebrisTypes = new string[] { "titanium" };
        public float ringRadius = 155f;
        public float corridorWidth = 36f;
        public float innerRadius = 82f;
        public float apertureAngle = 0.98f;
        public bool concentricRings = false;
        public float innerRingRadius = 90f;
        public float innerApertureAngle = 0.85f;
        public List<DividerData> dividers = new List<DividerData>();
        public List<GateData> gates = new List<GateData>();
        public List<PinData> deflectorPins = new List<PinData>();
        public List<FlapData> oneWayFlaps = new List<FlapData>();
    }

    [Serializable]
    public class SectorListWrapper
    {
        public List<SectorDefinition> sectors = new List<SectorDefinition>();
    }

    [Serializable]
    public class WorldDefinition
    {
        public string id = "world_1";
        public int worldIndex = 1;
        public string title = "Bubblegum Belt";
        public string tagline = "Orbital Gravity Basics & Clean Flow";
        public string icon = "🍬";
        public string color = "#f472b6";
        public int startLevel = 1;
        public int endLevel = 10;
    }
}
