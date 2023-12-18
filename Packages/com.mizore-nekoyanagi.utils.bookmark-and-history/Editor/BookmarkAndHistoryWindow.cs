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

        ReorderableList reorderableList;

        string prevSelectedGUID;

        Vector2 scroll_Bookmark;
        Vector2 scroll_History;

        static GUIContent Content_Star;


        private void OnDisable( ) {
            bookmark.Save( );
            history.Save( );
            //settings.Save( );
        }

        private void OnEnable( ) {
            Content_Star = EditorGUIUtility.IconContent( "Favorite" );
            bookmark.Load( );
            history.Load( );
            //settings.Load( );

            reorderableList = new ReorderableList( bookmark.Bookmark, typeof( string ), true, false, false, false );
            reorderableList.drawElementCallback = ( rect, index, isActive, isFocused ) => {
                if ( bookmark.Count <= index ) {
                    return;
                }
                var obj = bookmark[index];
                DrawElement( rect, obj, false, folderWidth: 280 );
            };
            reorderableList.headerHeight = 0;
            reorderableList.footerHeight = 0;
            reorderableList.showDefaultBackground = false;
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
        void DrawElement( Rect rowRect, ObjectWithPath objWithPath, bool keepHistoryOrder, float folderWidth ) {
            var obj = objWithPath.Object;
            var path = objWithPath.Path;

            var style = new GUIStyle( EditorStyles.label );
            style.fontSize = 13;

            var styleSub = new GUIStyle( EditorStyles.label );
            styleSub.fontSize = 11;

            var fileName = Path.GetFileName( path );

            bool b;
            using ( new EditorGUILayout.HorizontalScope( ) ) {
                var temp_contentColor = GUI.contentColor;
                if ( Selection.activeObject == obj ) {
                    EditorGUI.DrawRect( rowRect, new Color( 1f, 1f, 1f, 0.1f ) );
                }
                if ( !bookmark.Contains( path ) ) {
                    GUI.contentColor = Color.white * 0.65f;
                }
                var rect = rowRect;
                rect.width = 25;
                if ( GUI.Button( rect, Content_Star ) ) {
                    if ( bookmark.Contains( path ) ) {
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
                b = GUI.Button( rect, new GUIContent( fileName, icon, path ), style );
                rect.x += rect.width;
                rect.width = rowRect.xMax - rect.x;
                b |= GUI.Button( rect, new GUIContent( path, path ), styleSub );
            }
            var folder = obj as DefaultAsset;
            if ( b ) {
                if ( obj == null ) {
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
        }
        private void OnGUI( ) {
            // EditorGUILayout.LabelField( "Prev Selected", prevSelectedPath );

            EditorGUILayout.LabelField( "Bookmark", EditorStyles.boldLabel );
            scroll_Bookmark = EditorGUILayout.BeginScrollView( scroll_Bookmark, GUILayout.MinHeight( 300 ) );
            reorderableList.DoLayoutList( );
            EditorGUILayout.EndScrollView( );

            EditorGUILayout.Separator( );

            EditorGUILayout.LabelField( "History", EditorStyles.boldLabel );
            scroll_History = EditorGUILayout.BeginScrollView( scroll_History );
            // 逆順
            for ( int i = history.Count - 1; 0 <= i; i-- ) {
                var obj = history[i];
                var rect = EditorGUILayout.GetControlRect( );
                DrawElement( rect, obj, true, folderWidth: 300 );
            }
            EditorGUILayout.EndScrollView( );
            //if ( GUILayout.Button( "Save" ) ) {
            //    history.Save( );
            //    bookmark.Save( );
            //}
        }
    }
}
