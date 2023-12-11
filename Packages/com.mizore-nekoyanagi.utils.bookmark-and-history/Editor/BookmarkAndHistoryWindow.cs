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

        string prevSelectedPath;

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

            reorderableList = new ReorderableList( bookmark.bookmark, typeof( string ), true, false, false, false );
            reorderableList.drawElementCallback = ( rect, index, isActive, isFocused ) => {
                var path = bookmark.bookmark[index];
                DrawElement( rect, path, false, folderWidth: 280 );
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
            var selectObj = AssetDatabase.GUIDToAssetPath(GUID);
            if ( prevSelectedPath == selectObj ) {
                return;
            }
            prevSelectedPath = selectObj;
            history.AddHisotry( selectObj );
            Repaint( );
        }
        void DrawElement( Rect rowRect, string path, bool keepHistoryOrder, float folderWidth ) {
            var obj = AssetDatabase.LoadAssetAtPath<Object>( path );

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
                var icon = AssetDatabase.GetCachedIcon( path );
                rect.x += rect.width;
                rect.width = folderWidth;
                b = GUI.Button( rect, new GUIContent( fileName, icon, path ), style );
                rect.x += rect.width;
                rect.width = rowRect.xMax - rect.x;
                b |= GUI.Button( rect, new GUIContent( path, path ), styleSub );
            }
            var folder = obj as DefaultAsset;
            if ( b && obj != null ) {
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
                    prevSelectedPath = path;
                }
            }
        }
        private void OnGUI( ) {
            EditorGUILayout.LabelField( "Bookmark", EditorStyles.boldLabel );
            scroll_Bookmark = EditorGUILayout.BeginScrollView( scroll_Bookmark, GUILayout.MinHeight( 300 ) );
            reorderableList.DoLayoutList( );
            EditorGUILayout.EndScrollView( );

            EditorGUILayout.Separator( );

            EditorGUILayout.LabelField( "History", EditorStyles.boldLabel );
            scroll_History = EditorGUILayout.BeginScrollView( scroll_History );
            var list =  history.Reverse( );
            foreach ( var item in list ) {
                var rect = EditorGUILayout.GetControlRect( );
                DrawElement( rect, item, true, folderWidth: 300 );
            }
            EditorGUILayout.EndScrollView( );
            //if ( GUILayout.Button( "Save" ) ) {
            //    history.Save( );
            //    bookmark.Save( );
            //}
        }
    }
}
