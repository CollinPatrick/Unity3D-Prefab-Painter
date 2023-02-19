using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PrefabPainter
{
    public class PerciseRotationProcessor : PrefabPainterProcessor
    {
        [SerializeField] private ProcessStage _stage;
        [SerializeField] private Vector3 _rotation;

        public override void OnRotation( ProcessStage aStage, GameObject aGameObject ) {
            if( aStage == _stage ) {
                aGameObject.transform.rotation = Quaternion.Euler( _rotation );
            }
        }
    }
}
