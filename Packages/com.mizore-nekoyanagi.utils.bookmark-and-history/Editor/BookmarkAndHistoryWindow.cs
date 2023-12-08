using System.IO;
using System.Linq;
using UnityEditor;
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
        string prevSelectedPath;

        Vector2 scroll_Bookmark;
        Vector2 scroll_History;

        static GUIContent Content_Star;


        private void OnDisable( ) {
            bookmark.Save( );
            history.Save( );
        }

        private void OnEnable( ) {
            Content_Star = EditorGUIUtility.IconContent( "Favorite" );
            bookmark.Load( );
            history.Load( );
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
        void DrawElement( string item ) {
            var style = new GUIStyle( EditorStyles.label );
            style.fontSize = 13;

            var styleSub = new GUIStyle( EditorStyles.label );
            styleSub.fontSize = 11;

            var fileName = Path.GetFileName( item );

            bool b;
            using ( new EditorGUILayout.HorizontalScope( ) ) {
                Rect rect;
                rect = EditorGUILayout.GetControlRect( GUILayout.Width( 25 ) );

                var temp_color = GUI.contentColor;
                if ( !bookmark.Contains( item ) ) {
                    GUI.contentColor = Color.white * 0.65f;
                }
                if ( GUI.Button( rect, Content_Star ) ) {
                    if ( bookmark.Contains( item ) ) {
                        bookmark.RemoveBookmark( item );
                        history.AddHisotry( item );
                    } else {
                        bookmark.AddBookmark( item );
                    }
                }
                GUI.contentColor = temp_color;
                rect = EditorGUILayout.GetControlRect( GUILayout.MinWidth( 200 ) );
                var icon = AssetDatabase.GetCachedIcon( item );
                b = GUI.Button( rect, new GUIContent( fileName, icon, item ), style );
                rect = EditorGUILayout.GetControlRect( GUILayout.MinWidth( 700 ) );
                b |= GUI.Button( rect, item, styleSub );
            }

            if ( b ) {
                var obj = AssetDatabase.LoadAssetAtPath<Object>( item );
                if ( obj != null ) {
                    if ( Selection.activeObject == obj ) {
                        AssetDatabase.OpenAsset( obj );
                    } else {
                        Selection.activeObject = obj;
                        EditorGUIUtility.PingObject( obj );
                        //prevSelectedPath = item;
                    }
                }
            }
        }
        private void OnGUI( ) {
            EditorGUILayout.LabelField( "Bookmark", EditorStyles.boldLabel );
            scroll_Bookmark = EditorGUILayout.BeginScrollView( scroll_Bookmark, GUILayout.MinHeight( 300 ) );
            var bookmarks =  bookmark.Reverse( );
            foreach ( var item in bookmarks ) {
                DrawElement( item );

            }
            EditorGUILayout.EndScrollView( );

            EditorGUILayout.Separator( );

            EditorGUILayout.LabelField( "History", EditorStyles.boldLabel );
            scroll_History = EditorGUILayout.BeginScrollView( scroll_History );
            var list =  history.Reverse( );
            foreach ( var item in list ) {
                DrawElement( item );
            }
            EditorGUILayout.EndScrollView( );
            //if ( GUILayout.Button( "Save" ) ) {
            //    history.Save( );
            //    bookmark.Save( );
            //}
        }
    }
}
