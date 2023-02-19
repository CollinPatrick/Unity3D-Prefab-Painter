using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


namespace PrefabPainter.Editor {
    public class PrefabPainter : EditorWindow {
        [Serializable]
        public class PrefabGroup {
            [SerializeField] private string _groupName = "New Group";
            [SerializeField] private List<string> _assetPaths = new List<string>();
            private Dictionary<string, VisualElement> _assetPreviews = new Dictionary<string, VisualElement>();
            [NonSerialized] private VisualElement _parentElement;

            public IReadOnlyDictionary<string, VisualElement> assetPreviews => _assetPreviews;
            public string groupName => _groupName;

            public Action Save;
            public Action<string, VisualElement> OnSelect;

            public PrefabGroup( string aGroupName ) {
                _groupName = aGroupName;
                Initialize();
            }

            public void Initialize() {
                //_assetPaths = new List<string>();
                _assetPreviews = new Dictionary<string, VisualElement>();
            }

            public void Destroy() {
                foreach ( KeyValuePair<string, VisualElement> lPreview in _assetPreviews ) {
                    lPreview.Value.RemoveFromHierarchy();
                }
                _assetPaths.Clear();
                _assetPreviews.Clear();
                _parentElement.parent.RemoveFromHierarchy();
            }

            public void SetParent( VisualElement aParentElement ) {
                _parentElement = aParentElement;
            }

            public void AddAsset( GameObject aAsset ) {
                if ( aAsset == null ) return;

                string lPath = AssetDatabase.GetAssetPath( aAsset );
                if ( _assetPaths.Contains( lPath ) ) return;

                _assetPaths.Add( lPath );

                _assetPreviews.Add( lPath, CreateAssetPreview( lPath ) );

                Save?.Invoke();
            }

            public void RemoveAsset( string aAssetPath ) {
                if ( !string.IsNullOrEmpty( _assetPaths.Find( assetPath => assetPath == aAssetPath ) ) ) {
                    _assetPaths.Remove( aAssetPath );
                }
                LoadAssets();

                Save?.Invoke();
            }

            public void LoadAssets() {
                //_loadedAssets.Clear();
                foreach ( KeyValuePair<string, VisualElement> lPreview in _assetPreviews ) {
                    lPreview.Value.RemoveFromHierarchy();
                }
                _assetPreviews.Clear();
                for ( int i = 0; i < _assetPaths.Count; i++ ) {
                    GameObject lAsset = AssetDatabase.LoadAssetAtPath( _assetPaths[i], typeof( GameObject ) ) as GameObject;
                    if ( lAsset == null ) {
                        _assetPaths.Remove( _assetPaths[i] );
                    }
                    _assetPreviews.Add( _assetPaths[i], CreateAssetPreview( _assetPaths[i] ) );
                }
            }

            private VisualElement CreateAssetPreview( string aAssetPath ) {
                VisualElement lPreviewButton = prefabPreviewButtonTemplate.Instantiate();
                VisualElement lBackground = lPreviewButton.Q( "Background" );
                Button lSelectButton = lPreviewButton.Q<Button>( "SelectButton" );
                Button lRemoveButton = lPreviewButton.Q<Button>( "RemoveButton" );

                lBackground.style.backgroundImage = GetPrefabPreview( aAssetPath, 100, 100 );

                lSelectButton.clickable.clicked += () => {
                    GameObject lAsset = AssetDatabase.LoadAssetAtPath( aAssetPath, typeof( GameObject ) ) as GameObject;
                    OnSelect?.Invoke( aAssetPath, lPreviewButton );
                };

                lRemoveButton.clickable.clicked += () => {
                    RemoveAsset( aAssetPath );
                };

                _parentElement.Q( "PrefabGroup" ).Add( lPreviewButton );

                return lPreviewButton;
            }

            public static Texture2D GetPrefabPreview( string aPath, int aWidth, int aHeihgt ) {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>( aPath );
                var editor = UnityEditor.Editor.CreateEditor( prefab );
                Texture2D tex = editor.RenderStaticPreview( aPath, null, aWidth, aHeihgt );
                DestroyImmediate( editor );
                return tex;
            }
        }

        [Serializable]
        private class PrefabGroupPage {
            public string pageName = "New Page";

            public PrefabGroupPage( string aPageName ) {
                pageName = aPageName;
            }

            public List<PrefabGroup> prefabGroups = new List<PrefabGroup>();
            public PrefabGroupPage( List<PrefabGroup> aGroups ) {
                prefabGroups = aGroups;
            }
        }

        public const string EDITOR_RESOURCE_DIRECTORY = "Assets/Scripts/Utilities/Prefab Painter";
        public const string SAVE_DIRECTORY = "Assets/Resources/PrefabPainter/";
        public const string SAVE_FILE_NAME = "PrefabGroups.json";
        public const string SETTINGS_FILE_NAME = "Settings.asset";

        public static PrefabPainterSettings settings { get; private set; }

        public static VisualTreeAsset windowTemplate { get; private set; }
        public static VisualTreeAsset prefabGroupFoldoutTemplate { get; private set; }
        public static VisualTreeAsset prefabGroupTemplate { get; private set; }
        public static VisualTreeAsset prefabPreviewButtonTemplate { get; private set; }
        public static VisualTreeAsset paintSettingsPanelTemplate { get; private set; }




        private List<PrefabGroup> prefabGroups = new List<PrefabGroup>();

        private VisualElement root;
        private VisualElement window;
        private VisualElement paintSettingsPanel;
        private ScrollView paintSettingsScrollView;
        private VisualElement paintSettingsPreviewImage;
        private Label paintSettingsPreviewName;
        private VisualElement prefabGroupsPanel;
        private TextField groupNameField;
        private Button addGroupButton;
        private Button removeGroupButton;
        private ScrollView prefabGroupsScrollView;

        private string _selectedAssetPath => settings.selectedObjectPath;
        private GameObject _selectedAsset => settings.selectedObject;
        private VisualElement _selectedPreviewElement;

        private static Vector2 windowSize;

        [MenuItem( "Tools/Prefab Painter" )]
        public static void OpenWindow() {
            PrefabPainter wnd = GetWindow<PrefabPainter>();
            wnd.titleContent = new GUIContent( "Prefab Painter" );
            wnd.minSize = new Vector2( 350, 350 );
            windowSize = wnd.position.size;
        }



        public void OnGUI() {
            if ( Screen.height - 21 != windowSize.y ) {
                if ( paintSettingsPanel != null && prefabGroupsPanel != null ) {
                    windowSize = new Vector2( Screen.width, Screen.height - 21 );
                    paintSettingsPanel.style.height = new StyleLength { keyword = paintSettingsPanel.style.height.keyword, value = windowSize.y };
                    prefabGroupsPanel.style.height = new StyleLength { keyword = prefabGroupsPanel.style.height.keyword, value = windowSize.y };
                }
            }
        }

        public void CreateGUI() {
            //Get UXML templates
            windowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( $"{EDITOR_RESOURCE_DIRECTORY}/PrefabPainter.uxml" );
            prefabGroupTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( $"{EDITOR_RESOURCE_DIRECTORY}/PrefabGroupTemplate.uxml" );
            prefabGroupFoldoutTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( $"{EDITOR_RESOURCE_DIRECTORY}/PrefabGroupFoldoutTemplate.uxml" );
            prefabPreviewButtonTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( $"{EDITOR_RESOURCE_DIRECTORY}/PrefabPreviewButtonTemplate.uxml" );
            paintSettingsPanelTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( $"{EDITOR_RESOURCE_DIRECTORY}/PaintSettingsPanelTemplate.uxml" );


            // Set Element References
            root = rootVisualElement;
            window = windowTemplate.Instantiate();
            paintSettingsPanel = window.Q( "PaintSettingsPanel" );
            paintSettingsScrollView = paintSettingsPanel.Q<ScrollView>( "SettingsScrollView" );
            paintSettingsPreviewImage = paintSettingsPanel.Q( "PreviewImage" );
            paintSettingsPreviewName = paintSettingsPreviewImage.Q<Label>( "PreviewName" );

            prefabGroupsPanel = window.Q( "PrefabGroupsPanel" );
            groupNameField = prefabGroupsPanel.Q<TextField>( "GroupNameField" );
            addGroupButton = prefabGroupsPanel.Q<Button>( "AddGroupButton" );
            removeGroupButton = prefabGroupsPanel.Q<Button>( "RemoveGroupButton" );
            prefabGroupsScrollView = prefabGroupsPanel.Q<ScrollView>( "GroupsScrollView" );

            // Clear out any old data
            prefabGroups.Clear();
            prefabGroupsScrollView.Clear();
            paintSettingsScrollView.Clear();

            // Add window base to root
            ScrollView lRootView = new ScrollView();
            lRootView.Add( window );
            root.Add( lRootView );

            LoadGroups();
            LoadSettings();

            //Build Settings menu
            BuildGroupControls();
            BuildPaintSettings();
        }

        private void BuildPaintSettings() {
            VisualElement lSettingsPanel = paintSettingsPanelTemplate.Instantiate();
            SerializedObject lObject = new SerializedObject( settings );
            lSettingsPanel.Bind( lObject );

            Toggle lRandomizeRotation = lSettingsPanel.Q<Toggle>( "RandomizeRotation" );
            VisualElement lToggleRotationSettingsPanel = lSettingsPanel.Q( "ToggleRotationSettingsPanel" );

            lToggleRotationSettingsPanel.style.display = ( lRandomizeRotation.value ) ? DisplayStyle.Flex : DisplayStyle.None;
            lRandomizeRotation.RegisterValueChangedCallback( lEvent => {
                lToggleRotationSettingsPanel.style.display = ( lEvent.newValue ) ? DisplayStyle.Flex : DisplayStyle.None;
            } );

            Toggle lRandomizeScale = lSettingsPanel.Q<Toggle>( "RandomizeScale" );
            Toggle lUnifromScale = lSettingsPanel.Q<Toggle>( "UniformScale" );
            VisualElement lToggleRandomizeScalePanel = lSettingsPanel.Q( "ToggleRandomizeScalePanel" );
            VisualElement lToggleUniformScaleSettingsPanel = lSettingsPanel.Q( "ToggleUniformScaleSettingsPanel" );
            VisualElement lToggleRandomScaleSettingsPanel = lSettingsPanel.Q( "ToggleRandomScaleSettingsPanel" );

            lToggleRandomizeScalePanel.style.display = ( lRandomizeScale.value ) ? DisplayStyle.Flex : DisplayStyle.None;
            lRandomizeScale.RegisterValueChangedCallback( lEvent => {
                lToggleRandomizeScalePanel.style.display = ( lEvent.newValue ) ? DisplayStyle.Flex : DisplayStyle.None;
            } );

            lToggleUniformScaleSettingsPanel.style.display = ( lUnifromScale.value ) ? DisplayStyle.Flex : DisplayStyle.None;
            lToggleRandomScaleSettingsPanel.style.display = ( lUnifromScale.value ) ? DisplayStyle.None : DisplayStyle.Flex;
            lUnifromScale.RegisterValueChangedCallback( lEvent => {
                lToggleUniformScaleSettingsPanel.style.display = ( lEvent.newValue ) ? DisplayStyle.Flex : DisplayStyle.None;
                lToggleRandomScaleSettingsPanel.style.display = ( lEvent.newValue ) ? DisplayStyle.None : DisplayStyle.Flex;
            } );

            paintSettingsScrollView.Add( lSettingsPanel );
        }

        private void BuildGroupControls() {
            addGroupButton.clickable.clicked += () => {
                string lGroupName = groupNameField.value;

                if ( string.IsNullOrEmpty( lGroupName ) ) return;

                foreach ( PrefabGroup lGroup in prefabGroups ) {
                    if ( lGroup.groupName == lGroupName ) return;
                }

                CreateNewGroup( lGroupName );

                SaveGroups();

            };

            removeGroupButton.clickable.clicked += () => {
                string lGroupName = groupNameField.value;

                if ( string.IsNullOrEmpty( lGroupName ) ) return;

                for ( int i = 0; i < prefabGroups.Count; i++ ) {
                    if ( prefabGroups[i].groupName == lGroupName ) {
                        prefabGroups[i].Destroy();
                        prefabGroups.Remove( prefabGroups[i] );

                        break;
                    }
                }

                SaveGroups();
            };
        }

        private void CreateNewGroup( string aGroupName = "New Group" ) {
            Foldout lFoldout = prefabGroupFoldoutTemplate.Instantiate().Q<Foldout>( "Foldout" );
            VisualElement lNewGroup = prefabGroupTemplate.Instantiate();
            lFoldout.Add( lNewGroup );

            PrefabGroup lPrefabGroup = new PrefabGroup( aGroupName );
            lPrefabGroup.Save = SaveGroups;
            lPrefabGroup.OnSelect = OnSelectAsset;
            lPrefabGroup.SetParent( lNewGroup );
            lFoldout.text = lPrefabGroup.groupName;

            ObjectField lAddArea = lNewGroup.Q( "ObjectField" ) as ObjectField;
            lAddArea.RegisterValueChangedCallback( lEvent => {
                GameObject lObject = lEvent.newValue as GameObject;
                lPrefabGroup.AddAsset( lObject );
                lAddArea.value = null;
            } );

            prefabGroupsScrollView.Add( lFoldout );

            prefabGroups.Add( lPrefabGroup );

        }

        private void LoadGroup( PrefabGroup aGroup ) {
            Foldout lFoldout = prefabGroupFoldoutTemplate.Instantiate().Q<Foldout>( "Foldout" );
            VisualElement lNewGroup = prefabGroupTemplate.Instantiate();
            lFoldout.Add( lNewGroup );

            aGroup.Save = SaveGroups;
            aGroup.OnSelect = OnSelectAsset;
            aGroup.SetParent( lNewGroup );
            lFoldout.text = aGroup.groupName;

            ObjectField lAddArea = lNewGroup.Q( "ObjectField" ) as ObjectField;
            lAddArea.RegisterValueChangedCallback( lEvent => {
                GameObject lObject = lEvent.newValue as GameObject;
                aGroup.AddAsset( lObject );
                lAddArea.value = null;
            } );

            aGroup.Initialize();
            aGroup.LoadAssets();

            prefabGroupsScrollView.Add( lFoldout );
        }

        private void LoadGroups() {
            if ( !AssetDatabase.IsValidFolder( "Assets/Resources" ) ) {
                AssetDatabase.CreateFolder( "Assets", "Resources" );
                AssetDatabase.Refresh();
            }

            if ( !AssetDatabase.IsValidFolder( SAVE_DIRECTORY ) ) {
                AssetDatabase.CreateFolder( "Assets/Resources", "PrefabPainter" );
                AssetDatabase.Refresh();
            }

            if ( File.Exists( SAVE_DIRECTORY + SAVE_FILE_NAME ) ) {
                PrefabGroupPage lWrapper = JsonUtility.FromJson<PrefabGroupPage>( File.ReadAllText( SAVE_DIRECTORY + SAVE_FILE_NAME ) );
                prefabGroups = lWrapper.prefabGroups;

                for ( int i = 0; i < prefabGroups.Count; i++ ) {
                    prefabGroups[i].Initialize();
                    LoadGroup( prefabGroups[i] );
                }
            }
        }

        private void SaveGroups() {

            if ( !AssetDatabase.IsValidFolder( "Assets/Resources" ) ) {
                AssetDatabase.CreateFolder( "Assets", "Resources" );
                AssetDatabase.Refresh();
            }

            if ( !AssetDatabase.IsValidFolder( SAVE_DIRECTORY ) ) {
                AssetDatabase.CreateFolder( "Assets/Resources", "PrefabPainter" );
                AssetDatabase.Refresh();
            }

            string lJSON = JsonUtility.ToJson( new PrefabGroupPage( prefabGroups ) );
            File.WriteAllText( SAVE_DIRECTORY + SAVE_FILE_NAME, lJSON );
            AssetDatabase.Refresh();
        }

        private void LoadSettings() {
            if ( settings != null ) return;

            if ( !AssetDatabase.IsValidFolder( "Assets/Resources" ) ) {
                AssetDatabase.CreateFolder( "Assets", "Resources" );
                AssetDatabase.Refresh();
            }

            if ( !AssetDatabase.IsValidFolder( SAVE_DIRECTORY ) ) {
                AssetDatabase.CreateFolder( "Assets/Resources", "PrefabPainter" );
                AssetDatabase.Refresh();
            }

            if ( File.Exists( SAVE_DIRECTORY + SETTINGS_FILE_NAME ) ) {
                settings = AssetDatabase.LoadAssetAtPath( SAVE_DIRECTORY + SETTINGS_FILE_NAME, typeof( PrefabPainterSettings ) ) as PrefabPainterSettings;
            }
            else {
                PrefabPainterSettings lSettings = CreateInstance( typeof( PrefabPainterSettings ) ) as PrefabPainterSettings;
                AssetDatabase.CreateAsset( lSettings, SAVE_DIRECTORY + SETTINGS_FILE_NAME );
                AssetDatabase.Refresh();
                settings = AssetDatabase.LoadAssetAtPath( SAVE_DIRECTORY + SETTINGS_FILE_NAME, typeof( PrefabPainterSettings ) ) as PrefabPainterSettings;
            }

            if ( !string.IsNullOrEmpty( settings.selectedObjectPath ) ) {
                if ( settings.selectedObject != null ) {
                    paintSettingsPreviewImage.style.backgroundImage = PrefabGroup.GetPrefabPreview( settings.selectedObjectPath, 150, 150 );
                    paintSettingsPreviewName.text = settings.selectedObject.name;
                }
            }
        }

        private void Refresh() {
            SaveGroups();
            prefabGroups.Clear();
            prefabGroupsScrollView.Clear();
            LoadGroups();
        }

        private void OnSelectAsset( string aAssetPath, VisualElement aSelectedPreviewElement ) {
            GameObject lAsset = AssetDatabase.LoadAssetAtPath( aAssetPath, typeof( GameObject ) ) as GameObject;

            if ( lAsset == null ) {
                Debug.LogError( $"[{typeof( PrefabPainter )}] - Selected prefab does not exist! " );
                return;
            }

            settings.selectedObjectPath = aAssetPath;
            settings.selectedObject = lAsset;

            if ( _selectedPreviewElement != null ) {
                _selectedPreviewElement.Q( "Background" ).style.borderRightWidth = 0;
                _selectedPreviewElement.Q( "Background" ).style.borderLeftWidth = 0;
                _selectedPreviewElement.Q( "Background" ).style.borderTopWidth = 0;
                _selectedPreviewElement.Q( "Background" ).style.borderBottomWidth = 0;
            }

            paintSettingsPreviewImage.style.backgroundImage = PrefabGroup.GetPrefabPreview( aAssetPath, 150, 150 );
            paintSettingsPreviewName.text = lAsset.name;

            _selectedPreviewElement = aSelectedPreviewElement;
            _selectedPreviewElement.Q( "Background" ).style.borderRightWidth = 2;
            _selectedPreviewElement.Q( "Background" ).style.borderLeftWidth = 2;
            _selectedPreviewElement.Q( "Background" ).style.borderTopWidth = 2;
            _selectedPreviewElement.Q( "Background" ).style.borderBottomWidth = 2;
        }
    }
}
