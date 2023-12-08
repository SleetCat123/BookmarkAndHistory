using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MizoreNekoyanagi.PublishUtil.BookmarkAndHistory {
    [System.Serializable]
    public class BookmarkData : IEnumerable<string> {
        const string PATH_BOOKMARK = "BookmarkAndHistory/MizoresBookmark.json";
        [SerializeField]
        List<string> bookmark = new  List<string>();

        public void Save( ) {
            var json = JsonUtility.ToJson( this, true );
            Debug.Log( "Bookmark Saving: \n" + json );
            var dir = Path.GetDirectoryName( PATH_BOOKMARK );
            if ( !Directory.Exists( dir ) ) {
                Directory.CreateDirectory( dir );
            }
            File.WriteAllText( PATH_BOOKMARK, json );
        }
        public void Load( ) {
            if ( File.Exists( PATH_BOOKMARK ) ) {
                string json = File.ReadAllText(PATH_BOOKMARK);
                Debug.Log( "Bookmark Loading: \n" + json );
                JsonUtility.FromJsonOverwrite( json, this );
            }
        }

        public bool Contains( string path ) {
            return bookmark.Contains( path );
        }
        public void AddBookmark( string path ) {
            bookmark.RemoveAll( v => v == path );
            bookmark.Add( path );
        }
        public void RemoveBookmark( string path ) {
            bookmark.RemoveAll( v => v == path );
        }
        public void ToggleBookmark( string path ) {
            if ( bookmark.Contains( path ) ) {
                RemoveBookmark( path );
            } else {
                AddBookmark( path );
            }
        }

        public IEnumerator<string> GetEnumerator( ) {
            return bookmark.GetEnumerator( );
        }

        IEnumerator IEnumerable.GetEnumerator( ) {
            return bookmark.GetEnumerator( );
        }
    }
}
