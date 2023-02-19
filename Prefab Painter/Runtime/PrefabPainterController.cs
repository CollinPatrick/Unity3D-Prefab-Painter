using System.Collections.Generic;
using UnityEngine;

namespace PrefabPainter {

    public class PrefabPainterController : MonoBehaviour {
#if UNITY_EDITOR
        [HideInInspector] public Vector3 mousePositionWorld;
        [HideInInspector] public Vector3 mouseNormal;
        [HideInInspector] public Vector3 viewDirection;
        [HideInInspector] public bool canPaint = false;

        public Transform parentObject;
        public List<PrefabPainterProcessor> processors;
#endif
    }
}