using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PrefabPainter.Editor {
    [CustomEditor( typeof( PrefabPainterController ) )]
    public class PrefabPainterControllerEditor : UnityEditor.Editor {
        private PrefabPainterController obj;
        private int controlId;
        private bool remove;

        private void OnEnable() {
            controlId = GUIUtility.GetControlID( FocusType.Passive );
        }

        public override void OnInspectorGUI() {
            //if ( PrefabPainter.settings == null ) {
                if ( GUILayout.Button( "Open Prefab Painter" ) ) {
                    PrefabPainter.OpenWindow();
                }
            //}
            DrawDefaultInspector();
            //base.OnInspectorGUI();
        }

        void OnSceneGUI() {
            if ( PrefabPainter.settings == null ) return;
            obj = ( PrefabPainterController )target;

            UpdateMousePosition();

            remove = Event.current.shift;

            if ( Event.current.button == 0 ) {
                switch ( Event.current.type ) {
                    case EventType.MouseDown:

                        if ( remove ) {
                            Erase();
                        }
                        else {
                            Paint();
                        }

                        GUIUtility.hotControl = controlId;
                        Event.current.Use();
                        break;
                    case EventType.MouseDrag:
                        GUIUtility.hotControl = controlId;
                        Event.current.Use();
                        break;
                    case EventType.MouseUp:
                        GUIUtility.hotControl = controlId;
                        Event.current.Use();
                        break;
                }
            }

        }

        private void UpdateMousePosition() {
            Camera cam = SceneView.lastActiveSceneView.camera;

            Vector3 mousepos = Event.current.mousePosition;
            mousepos.z = -cam.worldToCameraMatrix.MultiplyPoint( obj.transform.position ).z;
            mousepos.y = Screen.height - mousepos.y - 36.0f;
            //mousepos = cam.ScreenToWorldPoint( mousepos );

            Ray ray = cam.ScreenPointToRay( mousepos );

            RaycastHit[] lHits = Physics.RaycastAll( ray, 1000.0f );
            bool lFoundPaintableSurface = false;
            foreach ( RaycastHit hit in lHits ) {
                if ( PrefabPainter.settings.ignoreLayers == ( PrefabPainter.settings.ignoreLayers | ( 1<< hit.transform.gameObject.layer ) ) ) continue;
                //if ( hit.transform.tag == PrefabPainter.settings.painterTag ) continue;
                obj.mousePositionWorld = hit.point;
                obj.mouseNormal = hit.normal;

                lFoundPaintableSurface = true;
                break;
            }

            if ( lFoundPaintableSurface ) {
                obj.canPaint = true;
            }
            else {
                obj.canPaint = false;
                return;
            }

            Handles.color = new Color( 0, 0, 0.5f, 0.7f );
            Handles.DrawSolidDisc( obj.mousePositionWorld + Vector3.up / 10, obj.mouseNormal, PrefabPainter.settings.brushSize / 2 );

            SceneView.RepaintAll();
        }

        private void Paint() {
            if ( !obj.canPaint ) return;

            float lArea = Mathf.PI * Mathf.Pow( PrefabPainter.settings.brushSize / 2, 2 );
            int lTotalObjects = ( int )( lArea * PrefabPainter.settings.brushWeight );
            lTotalObjects = Mathf.Clamp( lTotalObjects, 1, PrefabPainter.settings.maxObjects );

            for ( int i = 0; i < lTotalObjects; i++ ) {
                CreatePrefab();
            }
        }

        private void Erase() {
            if ( !obj.canPaint ) return;

            float lArea = Mathf.PI * Mathf.Pow( PrefabPainter.settings.brushSize / 2, 2 );
            int lMaxRemovableObjects = ( int )( lArea * PrefabPainter.settings.brushWeight );
            lMaxRemovableObjects = Mathf.Clamp( lMaxRemovableObjects, 1, PrefabPainter.settings.maxObjects );

            Collider[] lColliders = Physics.OverlapSphere( obj.mousePositionWorld, PrefabPainter.settings.brushSize / 2 );

            List<GameObject> lValidObjects = new List<GameObject>();
            foreach ( Collider collider in lColliders ) {
                if ( collider.tag == PrefabPainter.settings.painterTag && collider.transform.parent == obj.parentObject ) {
                    lValidObjects.Add( collider.gameObject );
                }
            }

            int lSkipInterval = ( lValidObjects.Count <= lMaxRemovableObjects ) ? 1 : lValidObjects.Count / lMaxRemovableObjects;
            for ( int i = 0; i < lValidObjects.Count; i += lSkipInterval ) {
                DestroyImmediate( lValidObjects[i] );
            }
        }

        private void CreatePrefab() {
            if ( !obj.canPaint ) return;

            Vector2 lRandomPoint = Random.insideUnitCircle * ( PrefabPainter.settings.brushSize / 2 );
            Vector3 lNewPosition = obj.mousePositionWorld + new Vector3( lRandomPoint.x, 0, lRandomPoint.y );

            Ray ray = new Ray( lNewPosition + Vector3.up, Vector3.down );
            RaycastHit[] lHits = Physics.RaycastAll( ray, 10 );
            bool lValidLocation = false;
            RaycastHit lHitData = default;
            foreach ( RaycastHit hit in lHits ) {
                if ( PrefabPainter.settings.ignoreLayers == ( PrefabPainter.settings.ignoreLayers | ( 1 << hit.transform.gameObject.layer ) ) ) continue;
                //if ( hit.transform.tag == PrefabPainter.settings.painterTag ) continue;
                lValidLocation = true;
                lHitData = hit;
                break;
            }

            if ( !lValidLocation ) return;

            GameObject lNewPrefab = PrefabUtility.InstantiatePrefab( PrefabPainter.settings.selectedObject, ( obj.parentObject == null ) ? obj.transform : obj.parentObject ) as GameObject;
            lNewPrefab.tag = ( PrefabPainter.settings.painterTag != "Untagged" ) ? PrefabPainter.settings.painterTag : lNewPrefab.tag;
            OnInstance( PrefabPainterProcessor.ProcessStage.Before, lNewPrefab );
            //lNewPrefab.transform.rotation = Quaternion.FromToRotation( lNewPrefab.transform.up, -lHitData.normal );

            lNewPrefab.transform.position = lHitData.point;
            OnPosition( PrefabPainterProcessor.ProcessStage.After, lNewPrefab );

            OnRotation( PrefabPainterProcessor.ProcessStage.Before, lNewPrefab );
            if ( PrefabPainter.settings.randomizeRotation ) {
                Vector3 lRot = lNewPrefab.transform.localEulerAngles;
                bool lNegative = ( Random.Range( 0, 1 ) > 0 ) ? true : false;
                float lRandomXRot = Random.Range( PrefabPainter.settings.xRandomRotation.x, PrefabPainter.settings.xRandomRotation.y );
                lRandomXRot = ( lNegative ) ? -lRandomXRot : lRandomXRot;

                lNegative = ( Random.Range( 0, 1 ) > 0 ) ? true : false;
                float lRandomYRot = Random.Range( PrefabPainter.settings.yRandomRotation.x, PrefabPainter.settings.yRandomRotation.y );
                lRandomYRot = ( lNegative ) ? -lRandomYRot : lRandomYRot;

                lNegative = ( Random.Range( 0, 1 ) > 0 ) ? true : false;
                float lRandomZRot = Random.Range( PrefabPainter.settings.zRandomRotation.x, PrefabPainter.settings.zRandomRotation.y );
                lRandomZRot = ( lNegative ) ? -lRandomZRot : lRandomZRot;

                lRot += new Vector3( lRandomXRot, lRandomYRot, lRandomZRot );

                lNewPrefab.transform.rotation = Quaternion.Euler( lRot );
            }
            OnRotation( PrefabPainterProcessor.ProcessStage.After, lNewPrefab );

            OnScale( PrefabPainterProcessor.ProcessStage.Before, lNewPrefab );
            if ( PrefabPainter.settings.randomizeScale ) {
                Vector3 lScale = lNewPrefab.transform.localScale;

                if ( PrefabPainter.settings.uniformScale ) {
                    float lRandomScale = Random.Range( PrefabPainter.settings.uniformRandomScale.x, PrefabPainter.settings.uniformRandomScale.y );

                    lScale *= lRandomScale;
                }
                else {
                    float lRandomXScale = Random.Range( PrefabPainter.settings.xRandomScale.x, PrefabPainter.settings.xRandomScale.y );
                    float lRandomYScale = Random.Range( PrefabPainter.settings.yRandomScale.x, PrefabPainter.settings.yRandomScale.y );
                    float lRandomZScale = Random.Range( PrefabPainter.settings.zRandomScale.x, PrefabPainter.settings.zRandomScale.y );

                    lScale.x *= lRandomXScale;
                    lScale.y *= lRandomYScale;
                    lScale.z *= lRandomZScale;
                }

                lNewPrefab.transform.localScale = lScale;
            }
            OnScale( PrefabPainterProcessor.ProcessStage.After, lNewPrefab );

            OnInstance( PrefabPainterProcessor.ProcessStage.After, lNewPrefab );
        }

        private void OnInstance( PrefabPainterProcessor.ProcessStage aStage, GameObject aObject ) {
            foreach ( PrefabPainterProcessor lProcessor in obj.processors ) {
                if ( lProcessor.isEnabled == false ) continue;
                if ( aObject.GetComponent( lProcessor.typeFilter ) == null ) continue;
                lProcessor.OnIntance( aStage, aObject );
            }
        }

        private void OnPosition( PrefabPainterProcessor.ProcessStage aStage, GameObject aObject ) {
            foreach ( PrefabPainterProcessor lProcessor in obj.processors ) {
                if ( lProcessor.isEnabled == false ) continue;
                if ( aObject.GetComponent( lProcessor.typeFilter ) == null ) continue;
                lProcessor.OnPosition( aStage, aObject );
            }
        }

        private void OnRotation( PrefabPainterProcessor.ProcessStage aStage, GameObject aObject ) {
            foreach ( PrefabPainterProcessor lProcessor in obj.processors ) {
                if ( lProcessor.isEnabled == false ) continue;
                if ( aObject.GetComponent( lProcessor.typeFilter ) == null ) continue;
                lProcessor.OnRotation( aStage, aObject );
            }
        }

        private void OnScale( PrefabPainterProcessor.ProcessStage aStage, GameObject aObject ) {
            foreach ( PrefabPainterProcessor lProcessor in obj.processors ) {
                if ( lProcessor.isEnabled == false ) continue;
                if ( aObject.GetComponent( lProcessor.typeFilter ) == null ) continue;
                lProcessor.OnScale( aStage, aObject );
            }
        }
    }
}
