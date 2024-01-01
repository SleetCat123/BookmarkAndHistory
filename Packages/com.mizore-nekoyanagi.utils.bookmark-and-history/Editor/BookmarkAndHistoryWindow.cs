using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MizoreNekoyanagi.PublishUtil.BookmarkAndHistory {
    public class BookmarkAndHistoryWindow : EditorWindow {
        [MenuItem( "Mizore/Bookmark And History" )]
        public static void ShowWindow( ) {
            var window = (BookmarkAndHistoryWindow)EditorWindow.GetWindow(typeof(BookmarkAndHistoryWindow));
            window.titleContent = new GUIContent( "Bookmark And History" );
            window.Show( );
        }

        [SerializeField]
        SelectionHistoryData history = new SelectionHistoryData();
        [SerializeField]
        BookmarkData bookmark = new BookmarkData();
        //[SerializeField]
        //BookmarkAndHistoryWindowSettings settings = new BookmarkAndHistoryWindowSettings( );

        ReorderableList reorderableList_Bookmark;
        ReorderableList reorderableList_History;

        string prevSelectedGUID;

        Vector2 scroll_Bookmark;
        Vector2 scroll_History;

        static GUIContent Content_Star;


        private void OnDisable( ) {
            bookmark.Save( );
            history.Save( );
            //settings.Save( );
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
                    reorderableList.Select( index );
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

        private void OnEnable( ) {
            Content_Star = EditorGUIUtility.IconContent( "Favorite" );
            bookmark.Load( );
            history.Load( );
            //settings.Load( );

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
        }

        private void Update( ) {
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
        class DrawElementResult {
            public bool clicked;
            public bool dragAndDropTarget;
        }
        DrawElementResult DrawElement( Rect rowRect, ObjectWithPath objWithPath, bool keepHistoryOrder, float folderWidth ) {
            var obj = objWithPath.Object;
            var path = objWithPath.Path;

            var style = new GUIStyle( EditorStyles.label );
            style.fontSize = 13;

            var styleSub = new GUIStyle( EditorStyles.label );
            styleSub.fontSize = 11;

            var fileName = Path.GetFileName( path );

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
                        bookmark.RemoveBookmark( path );
                        history.AddHisotry( path );
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
                result.clicked = GUI.Button( rect, new GUIContent( fileName, icon, path ), style );
                rect.x += rect.width;
                rect.width = rowRect.xMax - rect.x;
                result.clicked |= GUI.Button( rect, new GUIContent( path, path ), styleSub );
            }
            var folder = obj as DefaultAsset;
            if ( result.clicked ) {
                if ( obj == null ) {
                    // オブジェクトが存在しない場合は履歴から削除
                    history.RemoveHistory( path );
                } else {
                    if ( folder != null ) {
                        Selection.activeObject = obj;
                        AssetDatabase.OpenAsset( obj );
                    } else if ( Selection.activeObject == obj ) {
                        AssetDatabase.OpenAsset( obj );
                    } else {
                        Selection.activeObject = obj;
                        EditorGUIUtility.PingObject( obj );
                    }
                    if ( keepHistoryOrder ) {
                        var GUID = AssetDatabase.AssetPathToGUID( path );
                        prevSelectedGUID = GUID;
                    }
                }
            }
            // ObjectがDefaultAsset（フォルダ）の場合、選択中のAssetをドラッグ＆ドロップで格納できるようにする
            if ( folder != null && rowRect.Contains( Event.current.mousePosition ) ) {
                var evt = Event.current;
                if ( evt.type == EventType.DragUpdated ) {
                    // Ctrlキーが押されていたらコピー
                    if ( evt.control ) {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    } else {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                    }
                    result.dragAndDropTarget = true;

                    evt.Use( );
                } else if ( evt.type == EventType.DragPerform ) {
                    DragAndDrop.AcceptDrag( );
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
                                if ( skipAll){
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
                }
            }
            return result;
        }
        private void OnGUI( ) {
            // EditorGUILayout.LabelField( "Prev Selected", prevSelectedPath );

            EditorGUILayout.LabelField( "Bookmark", EditorStyles.boldLabel );
            scroll_Bookmark = EditorGUILayout.BeginScrollView( scroll_Bookmark, GUILayout.MinHeight( 300 ) );
            reorderableList_Bookmark.DoLayoutList( );
            EditorGUILayout.EndScrollView( );

            EditorGUILayout.Separator( );

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
