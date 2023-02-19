using System;
using UnityEngine;

namespace PrefabPainter {
    public abstract class PrefabPainterProcessor : MonoBehaviour {
#if UNITY_EDITOR
        public enum ProcessStage {
            Before,
            After
        }

        public bool isEnabled = true;

        public virtual Type typeFilter => typeof( Transform );

        public virtual void OnIntance( ProcessStage aStage, GameObject aGameObject ) { }
        public virtual void OnPosition( ProcessStage aStage, GameObject aGameObject ) { }
        public virtual void OnRotation( ProcessStage aStage, GameObject aGameObject ) { }
        public virtual void OnScale( ProcessStage aStage, GameObject aGameObject ) { }
#endif
    }
}

