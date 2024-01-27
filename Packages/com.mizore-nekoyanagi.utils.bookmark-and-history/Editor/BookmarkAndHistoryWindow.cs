using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MizoreNekoyanagi.PublishUtil.BookmarkAndHistory {
    public class BookmarkAndHistoryWindow : EditorWindow {
        [MenuItem( "Mizore/Bookmark And History" )]
        public static void ShowWindow( ) {
            var window = (BookmarkAndHistoryWindow)EditorWindow.GetWindow(typeof(BookmarkAndHistoryWindow));
            window.Show( );
        }

        [SerializeField]
        SelectionHistoryData history = new SelectionHistoryData();
        [SerializeField]
        BookmarkData bookmark = new BookmarkData();
        [SerializeField]
        BookmarkAndHistoryWindowSettings settings = new BookmarkAndHistoryWindowSettings( );

        ReorderableList reorderableList_Bookmark;
        ReorderableList reorderableList_History;

        string prevSelectedGUID;

        int dragSeparatorIndex = -1;

        Vector2 scroll_Bookmark;
        Vector2 scroll_History;

        static GUIContent Content_Star;

        Object openWhenNextUpdate;
        int openWhenNextUpdateFrame;

        string edittingLabelPath;
        string edittingLabelValue;

        private void OnDisable( ) {
            Save( );
        }
        void Save( ) {
            settings.Save( );
            bookmark.settings = settings;
            bookmark.Save( );
            history.settings = settings;
            history.Save( );
        }

        void drawElementCallback( ReorderableList reorderableList, bool invert, bool keepHistoryOrder, Rect rowRect, int index, bool isActive, bool isFocused ) {
            var list = reorderableList.list as List<ObjectWithPath>;
            if ( list.Count <= index ) {
                return;
            }
            ObjectWithPath obj;
            if ( invert ) {
                obj = list[list.Count - index - 1];
            } else {
                obj = list[index];
            }
            if ( obj.IsSeparator ) {
                var rect = rowRect;
                using ( new EditorGUILayout.HorizontalScope( ) ) {
                    rect.width = 25;
                    if ( GUI.Button( rect, "-" ) ) {
                        list.RemoveAt( index );
                    }
                    rect.x += rect.width;
                    rect.width = rowRect.width - rect.width;
                    EditorGUI.LabelField( rect, string.Empty, GUI.skin.button );
                }
            } else {
                var result = DrawElement( rowRect, obj, keepHistoryOrder, folderWidth: 280 );
                if ( result.dragAndDropTarget ) {
                    reorderableList.index = index;
                }
            }
        }
        public float elementHeightCallback( ReorderableList reorderableList, int index ) {
            var list = reorderableList.list as List<ObjectWithPath>;
            var obj = list[index];
            if ( obj.IsSeparator ) {
                return 15;
            } else {
                return EditorGUIUtility.singleLineHeight;
            }
        }

        private void Load( ) {
            settings.Load( );
            bookmark.settings = settings;
            bookmark.Load( );
            history.settings = settings;
            history.Load( );
        }
        private void OnEnable( ) {
            titleContent = new GUIContent( "Bookmark And History" );
            minSize = new Vector2( 300, 300 );

            Content_Star = EditorGUIUtility.IconContent( "Favorite" );
            Load( );

            reorderableList_Bookmark = new ReorderableList(
                elements: bookmark.Bookmark,
                elementType: typeof( ObjectWithPath ),
                draggable: true,
                displayHeader: false,
                displayAddButton: true,
                displayRemoveButton: true
                );
            reorderableList_Bookmark.drawElementCallback = ( rowRect, index, isActive, isFocused ) => drawElementCallback(
                reorderableList: reorderableList_Bookmark,
                invert: false,
                keepHistoryOrder: false,
                rowRect: rowRect,
                index: index,
                isActive: isActive,
                isFocused: isFocused
                );
            reorderableList_Bookmark.elementHeightCallback = ( index ) => elementHeightCallback( reorderableList_Bookmark, index );
            reorderableList_Bookmark.onAddCallback = ( list ) => {
                bookmark.AddSeparator( list.index );
            };
            reorderableList_Bookmark.onRemoveCallback = ( list ) => {
                bookmark.RemoveAt( list.index );
            };
            reorderableList_Bookmark.headerHeight = 0;
            reorderableList_Bookmark.footerHeight = 0;
            reorderableList_Bookmark.showDefaultBackground = false;

            reorderableList_History = new ReorderableList(
                elements: history.History,
                elementType: typeof( ObjectWithPath ),
                draggable: false,
                displayHeader: false,
                displayAddButton: false,
                displayRemoveButton: false
                );
            reorderableList_History.drawElementCallback = ( rowRect, index, isActive, isFocused ) => drawElementCallback(
                reorderableList: reorderableList_History,
                invert: true,
                keepHistoryOrder: true,
                rowRect: rowRect,
                index: index,
                isActive: isActive,
                isFocused: isFocused
                );
            reorderableList_History.elementHeightCallback = ( index ) => elementHeightCallback( reorderableList_History, index );
            reorderableList_History.headerHeight = 0;
            reorderableList_History.footerHeight = 0;
            reorderableList_History.showDefaultBackground = false;

            EditorSceneManager.sceneClosed += OnSceneClosed;
        }
        private void OnDestroy( ) {
            EditorSceneManager.sceneClosed -= OnSceneClosed;
        }

        void OnSceneClosed( Scene scene ) {
            // 直前に開いていたシーンを履歴に追加
            history.AddHisotry( scene.path );
        }
        private void Update( ) {
            if ( openWhenNextUpdate != null ) {
                openWhenNextUpdateFrame--;
                if ( openWhenNextUpdateFrame <= 0 ) {
                    AssetDatabase.OpenAsset( openWhenNextUpdate );
                    history.AddHisotry( AssetDatabase.GetAssetPath( openWhenNextUpdate ) );
                    openWhenNextUpdate = null;
                }
            }
            if ( Selection.assetGUIDs.Length == 0 ) {
                return;
            }
            var GUID = Selection.assetGUIDs[0];
            if ( prevSelectedGUID != GUID ) {
                prevSelectedGUID = GUID;
                var selectObjPath = AssetDatabase.GUIDToAssetPath(GUID);
                history.AddHisotry( selectObjPath );
                Repaint( );
            }
        }
        void SetOpenWhenNextUpdate( Object obj ) {
            // すぐにOpenAssetしてもフォルダが開かないので何フレームか待つ
            openWhenNextUpdate = obj;
            openWhenNextUpdateFrame = 2;
        }
        class DrawElementResult {
            public bool clicked;
            public bool dragAndDropTarget;
        }
        DrawElementResult DrawElement( Rect rowRect, ObjectWithPath objWithPath, bool keepHistoryOrder, float folderWidth ) {
            var obj = objWithPath.Object;
            var path = objWithPath.Path;

            var styleSub = new GUIStyle( EditorStyles.label );
            styleSub.fontSize = 11;

            DrawElementResult result = new DrawElementResult( );
            using ( new EditorGUILayout.HorizontalScope( ) ) {
                var temp_contentColor = GUI.contentColor;
                if ( Selection.objects.Contains( obj ) ) {
                    EditorGUI.DrawRect( rowRect, new Color( 1f, 1f, 1f, 0.1f ) );
                }
                bool contains = bookmark.Contains( path );
                if ( !contains ) {
                    GUI.contentColor = Color.white * 0.65f;
                }
                var rect = rowRect;
                rect.width = 25;
                if ( GUI.Button( rect, Content_Star ) ) {
                    if ( contains ) {
                        // Shiftキーが押されていたら確認メッセージを表示せずに削除
                        bool remove = false;
                        if ( Event.current.shift ) {
                            remove = true;
                        } else {
                            // 確認メッセージ
                            var label = bookmark.GetLabel( path );
                            var text = $"{label}\n({path})\nをブックマークから削除しますか？";
                            remove = EditorUtility.DisplayDialog( "Remove Bookmark", text, "Remove", "Cancel" );
                        }
                        if ( remove ) {
                            bookmark.RemoveBookmark( path );
                            history.AddHisotry( path );
                        }
                    } else {
                        bookmark.AddBookmark( path );
                    }
                }
                GUI.contentColor = temp_contentColor;
                rect.x += rect.width;
                rect.width = folderWidth;
                Texture icon = null;
                if ( obj != null ) {
                    icon = AssetDatabase.GetCachedIcon( path );
                }
                if ( edittingLabelPath != null && edittingLabelPath == path ) {
                    EditorGUI.BeginChangeCheck( );
                    edittingLabelValue = EditorGUI.DelayedTextField( rect, edittingLabelValue );
                    if ( EditorGUI.EndChangeCheck( ) ) {
                        bookmark.SetLabel( path, edittingLabelValue );
                        edittingLabelPath = null;
                        edittingLabelValue = null;
                    } else if ( Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Escape ) {
                        // EnterキーかEscキーが押されたら終了
                        edittingLabelPath = null;
                        edittingLabelValue = null;
                        Repaint( );
                    }
                } else {
                    var label = bookmark.GetLabel( path );
                    EditorGUI.LabelField( rect, new GUIContent( label, icon, path ) );
                    if ( Event.current.type == EventType.MouseDown && rect.Contains( Event.current.mousePosition ) ) {
                        // 左クリックで選択
                        result.clicked = true;
                        edittingLabelPath = null;
                        edittingLabelValue = null;
                        Event.current.Use( );
                    } else if ( Event.current.type == EventType.ContextClick && rect.Contains( Event.current.mousePosition ) ) {
                        result.clicked = false;
                        edittingLabelPath = null;
                        edittingLabelValue = null;
                        // 右クリックでラベルを変更
                        var menu = new GenericMenu( );
                        menu.AddItem( new GUIContent( "Edit Label" ), false, ( ) => {
                            edittingLabelPath = path;
                            edittingLabelValue = label;
                        } );
                        if ( bookmark.HasLabel( path ) ) {
                            menu.AddItem( new GUIContent( "Remove Label" ), false, ( ) => {
                                bookmark.RemoveLabel( path );
                            } );
                        } else {
                            menu.AddDisabledItem( new GUIContent( "Remove Label" ) );
                        }
                        menu.ShowAsContext( );
                        Event.current.Use( );
                    }
                }
                rect.x += rect.width;
                rect.width = rowRect.xMax - rect.x;
                EditorGUI.LabelField( rect, new GUIContent( path, path ), styleSub );
                if ( Event.current.type == EventType.MouseDown && rect.Contains( Event.current.mousePosition ) ) {
                    result.clicked = true;
                    edittingLabelPath = null;
                    edittingLabelValue = null;
                    Event.current.Use( );
                }
            }
            if ( result.clicked ) {
                if ( obj == null ) {
                    // オブジェクトが存在しない場合は履歴から削除
                    history.RemoveHistory( path );
                } else {
                    var folder = obj as DefaultAsset;
                    if ( folder != null ) {
                        Selection.activeObject = obj;
                        SetOpenWhenNextUpdate( obj );
                    } else if ( Selection.activeObject == obj ) {
                        AssetDatabase.OpenAsset( obj );
                        history.AddHisotry( path );
                    } else {
                        Selection.activeObject = obj;
                        EditorGUIUtility.PingObject( obj );
                        if ( keepHistoryOrder ) {
                            var GUID = AssetDatabase.AssetPathToGUID( path );
                            prevSelectedGUID = GUID;
                        }
                    }
                }
            }
            DragDrop( rowRect, path, obj, result );
            return result;
        }
        void DragDrop( Rect rowRect, string path, Object obj, DrawElementResult result ) {
            if ( obj == null ) {
                return;
            }
            if ( !rowRect.Contains( Event.current.mousePosition ) ) {
                return;
            }
            var evt = Event.current;
            //// prefabかfbxの場合、DragAndDropに追加
            //if ( evt.type == EventType.DragUpdated ) {
            //    var extension = Path.GetExtension( path );
            //    if ( extension == ".prefab" || extension == ".fbx" ) {
            //        DragAndDrop.PrepareStartDrag( );
            //        DragAndDrop.paths = new string[] { path };
            //        DragAndDrop.objectReferences = new Object[] { obj };
            //        DragAndDrop.StartDrag( "DragAndDrop" );
            //        evt.Use( );
            //        return;
            //    }
            //}
            var folder = obj as DefaultAsset;
            if ( folder != null ) {
                // ObjectがDefaultAsset（フォルダ）の場合、選択中のAssetをドラッグ＆ドロップで格納できるようにする
                if ( evt.type == EventType.DragUpdated ) {
                    // Ctrlキーが押されていたらコピー
                    if ( evt.control ) {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    } else {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                    }
                    result.dragAndDropTarget = true;

                    evt.Use( );
                    return;
                } else if ( evt.type == EventType.DragPerform ) {
                    DragAndDrop.AcceptDrag( );
                    Selection.activeObject = obj;
                    var folderPath = path;
                    var dropObjects = DragAndDrop.objectReferences;
                    bool skipAll = false;
                    foreach ( var dropObject in dropObjects ) {
                        if ( dropObject == obj ) {
                            continue;
                        }
                        if ( !EditorUtility.IsPersistent( dropObject ) ) {
                            continue;
                        }
                        var fromPath = AssetDatabase.GetAssetPath( dropObject );
                        var assetName = Path.GetFileName( fromPath );
                        var newPath = Path.Combine( folderPath, assetName );
                        if ( evt.control ) {
                            // Ctrlキーが押されていたらコピー
                            newPath = AssetDatabase.GenerateUniqueAssetPath( newPath );
                            AssetDatabase.CopyAsset( fromPath, newPath );
                            Debug.Log( $"CopyAsset: \nFrom: {fromPath}\nTo: {newPath}" );
                            var newObject = AssetDatabase.LoadAssetAtPath<Object>( newPath );
                            EditorGUIUtility.PingObject( newObject );

                        } else {
                            // 移動
                            if ( fromPath == newPath ) {
                                continue;
                            }
                            if ( File.Exists( newPath ) ) {
                                if ( skipAll ) {
                                    continue;
                                }
                                // 確認メッセージ
                                var text = $"{newPath}\nには同名のファイルが存在しています。\n両方のファイルを保持するか、スキップするか選択してください。\n\n";
                                var choice = EditorUtility.DisplayDialogComplex( "Move Asset", text + newPath, "Keep Both", "Skip", "Skip All" );
                                switch ( choice ) {
                                    case 0:
                                        // 両方保持
                                        newPath = AssetDatabase.GenerateUniqueAssetPath( newPath );
                                        break;
                                    case 1:
                                        // スキップ
                                        continue;
                                    case 2:
                                        // 全てスキップ
                                        skipAll = true;
                                        continue;
                                }
                            }
                            AssetDatabase.MoveAsset( fromPath, newPath );
                            Debug.Log( $"MoveAsset: \nFrom: {fromPath}\nTo: {newPath}" );
                            EditorGUIUtility.PingObject( dropObject );
                        }
                    }
                    result.dragAndDropTarget = true;
                    evt.Use( );
                    return;
                }
            }
        }
        private void OnGUI( ) {
            // EditorGUILayout.LabelField( "Prev Selected", prevSelectedPath );

            if ( settings.debug ) {
                EditorGUILayout.LabelField( "Debug Mode Enabled", EditorStyles.boldLabel );
                using ( new EditorGUILayout.HorizontalScope( ) ) {
                    if ( GUILayout.Button( "Save" ) ) {
                        Save( );
                    }
                    if ( GUILayout.Button( "Load" ) ) {
                        Load( );
                    }
                }
            }

            EditorGUILayout.LabelField( "Bookmark", EditorStyles.boldLabel );
            var height = Mathf.Clamp( settings.bookmarkHeight, 50, position.height - 200 );
            scroll_Bookmark = EditorGUILayout.BeginScrollView( scroll_Bookmark, GUILayout.MinHeight( height ) );
            reorderableList_Bookmark.DoLayoutList( );
            EditorGUILayout.EndScrollView( );

            var separatorRect = EditorGUILayout.GetControlRect( GUILayout.Height( 10 ) );
            EditorGUIUtility.AddCursorRect( separatorRect, MouseCursor.ResizeVertical );
            // Bookmarkの高さを変更
            if ( Event.current.type == EventType.MouseDrag ) {
                if ( separatorRect.Contains( Event.current.mousePosition )){
                    dragSeparatorIndex = 0;
                    Event.current.Use( );
                }
                if ( dragSeparatorIndex == 0 ) {
                    settings.bookmarkHeight += Event.current.delta.y;
                    Event.current.Use( );
                }
            } else if ( Event.current.type == EventType.DragExited ) {
                dragSeparatorIndex = -1;
                Event.current.Use( );
            }

            EditorGUILayout.LabelField( "History", EditorStyles.boldLabel );
            scroll_History = EditorGUILayout.BeginScrollView( scroll_History );
            reorderableList_History.DoLayoutList( );
            EditorGUILayout.EndScrollView( );

            //if ( GUILayout.Button( "Save" ) ) {
            //    history.Save( );
            //    bookmark.Save( );
            //}
        }
    }
}
