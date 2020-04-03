using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;
namespace FC
{
    public class TagAndLayer
    {
        public class LayerName
        {
            public const string Default = "Default";
            public const string TrasnparentFX = "TransparentFX";
            public const string IgnoreRayCast = "Ignore Raycast";
            public const string Water = "Water";
            public const string UI = "UI";
            public const string Cover = "Cover";
            public const string IgnoreShot = "Ignore Shot";
            public const string CoverInvisible = "Cover Invisible";
            public const string Player = "Player";
            public const string Enemy = "Enemy";
            public const string Bound = "Bound";
            public const string Environment = "Environment";
        }

        public static int GetLayerByName(string layerName)
        {
            return LayerMask.NameToLayer(layerName);
        }

        public class TagName
        {
            public const string Untagged = "Untagged";
            public const string Player = "Player";
            public const string Enemy = "Enemy";
            public const string GameController = "GameController";
            public const string Finish = "Finish";
        }

    }
}

