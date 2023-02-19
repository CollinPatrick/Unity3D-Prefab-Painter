using UnityEngine;

namespace PrefabPainter.Editor {
    public class PrefabPainterSettings : ScriptableObject {

        public string selectedObjectPath;
        public GameObject selectedObject;

        public float brushSize = 1f;
        public float brushWeight = 1f;
        public int maxObjects = 1;
        public LayerMask ignoreLayers;
        public string painterTag;

        public bool randomizeRotation = false;
        public Vector2 xRandomRotation = new Vector2( 0, 180 );
        public Vector2 yRandomRotation = new Vector2( 0, 180 );
        public Vector2 zRandomRotation = new Vector2( 0, 180 );

        public bool randomizeScale = false;
        public bool uniformScale = false;
        public Vector2 uniformRandomScale = new Vector2( 1, 1.5f );
        public Vector2 xRandomScale = new Vector2( 1, 1.5f );
        public Vector2 yRandomScale = new Vector2( 1, 1.5f );
        public Vector2 zRandomScale = new Vector2( 1, 1.5f );
    }
}
