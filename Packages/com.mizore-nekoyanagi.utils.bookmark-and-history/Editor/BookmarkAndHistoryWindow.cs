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
        //[SerializeField]
        //BookmarkAndHistoryWindowSettings settings = new BookmarkAndHistoryWindowSettings( );

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
        void DrawElement( string path, bool keepHistoryOrder ) {
            var obj = AssetDatabase.LoadAssetAtPath<Object>( path );

            var style = new GUIStyle( EditorStyles.label );
            style.fontSize = 13;

            var styleSub = new GUIStyle( EditorStyles.label );
            styleSub.fontSize = 11;

            var fileName = Path.GetFileName( path );

            bool b;
            using ( new EditorGUILayout.HorizontalScope( ) ) {
                var starButtonRect = EditorGUILayout.GetControlRect( GUILayout.Width( 25 ) );
                var assetRect = EditorGUILayout.GetControlRect( GUILayout.MinWidth( 200 ) );
                var fullPathrect = EditorGUILayout.GetControlRect( GUILayout.MinWidth( 700 ) );
                var rect = new Rect(starButtonRect.position, new Vector2(fullPathrect.xMax, fullPathrect.height));

                var temp_contentColor = GUI.contentColor;
                if ( Selection.activeObject == obj ) {
                    EditorGUI.DrawRect( rect, new Color( 1f, 1f, 1f, 0.1f ) );
                }
                if ( !bookmark.Contains( path ) ) {
                    GUI.contentColor = Color.white * 0.65f;
                }
                if ( GUI.Button( starButtonRect, Content_Star ) ) {
                    if ( bookmark.Contains( path ) ) {
                        bookmark.RemoveBookmark( path );
                        history.AddHisotry( path );
                    } else {
                        bookmark.AddBookmark( path );
                    }
                }
                GUI.contentColor = temp_contentColor;
                var icon = AssetDatabase.GetCachedIcon( path );
                b = GUI.Button( assetRect, new GUIContent( fileName, icon, path ), style );
                b |= GUI.Button( fullPathrect, path, styleSub );
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
            var bookmarks =  bookmark.Reverse( );
            foreach ( var item in bookmarks ) {
                DrawElement( item, false );

            }
            EditorGUILayout.EndScrollView( );

            EditorGUILayout.Separator( );

            EditorGUILayout.LabelField( "History", EditorStyles.boldLabel );
            scroll_History = EditorGUILayout.BeginScrollView( scroll_History );
            var list =  history.Reverse( );
            foreach ( var item in list ) {
                DrawElement( item, true );
            }
            EditorGUILayout.EndScrollView( );
            //if ( GUILayout.Button( "Save" ) ) {
            //    history.Save( );
            //    bookmark.Save( );
            //}
        }
    }
}
