using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MizoreNekoyanagi.PublishUtil.BookmarkAndHistory {
    [System.Serializable]
    public class BookmarkData : IEnumerable<ObjectWithPath>, ISerializationCallbackReceiver {
        const string PATH_BOOKMARK = "BookmarkAndHistory/MizoresBookmark.json";

        [System.NonSerialized]
        public BookmarkAndHistoryWindowSettings settings;

        [System.NonSerialized] List<ObjectWithPath> bookmarkObjects = new List<ObjectWithPath>( );
        /// <summary>
        /// Serialize用
        /// </summary>
        [SerializeField] string[] bookmark = new string[0];

        [System.NonSerialized]
        Dictionary<string, string> bookmarkLabelTable = new Dictionary<string, string>( );
        /// <summary>
        /// Serialize用
        /// </summary>
        [SerializeField] string[] bookmarkLabels = new string[0];

        public List<ObjectWithPath> Bookmark {
            get {
                return bookmarkObjects;
            }
        }
        public ObjectWithPath this[int index] {
            get {
                return bookmarkObjects[index];
            }
        }
        public int Count {
            get {
                return bookmarkObjects.Count;
            }
        }

        public void Save( ) {
            var json = JsonUtility.ToJson( this, true );
            if ( settings.debug ) {
                Debug.Log( "Bookmark Saving: \n" + json );
            }
            var dir = Path.GetDirectoryName( PATH_BOOKMARK );
            if ( !Directory.Exists( dir ) ) {
                Directory.CreateDirectory( dir );
            }
            File.WriteAllText( PATH_BOOKMARK, json );
        }
        public void Load( ) {
            if ( File.Exists( PATH_BOOKMARK ) ) {
                string json = File.ReadAllText(PATH_BOOKMARK);
                if ( settings.debug ) {
                    Debug.Log( "Bookmark Loading: \n" + json );
                }
                JsonUtility.FromJsonOverwrite( json, this );
            }
        }

        public void SetLabel( string path, string label ) {
            bookmarkLabelTable[path] = label;
        }
        public string GetLabel( string path ) {
            if ( bookmarkLabelTable.TryGetValue( path, out var label ) ) {
                return label;
            } else {
                return Path.GetFileName( path );
            }
        }
        public bool HasLabel( string path ) {
            return bookmarkLabelTable.ContainsKey( path );
        }
        public void RemoveLabel( string path ) {
            bookmarkLabelTable.Remove( path );
        }

        public bool Contains( string path ) {
            return bookmarkObjects.Any( v => v.Path == path );
        }
        public bool Contains( Object obj ) {
            return bookmarkObjects.Contains( obj );
        }
        public void AddSeparator( int index ) {
            bookmarkObjects.Insert( index, new ObjectWithPath( ) );
        }
        public void AddSeparator( ) {
            bookmarkObjects.Add( new ObjectWithPath( ) );
        }
        public void RemoveAt( int index ) {
            bookmarkObjects.RemoveAt( index );
        }
        public void AddBookmark( string path ) {
            bookmarkObjects.RemoveAll( v => v.Path == path );
            bookmarkObjects.Add( new ObjectWithPath( path ) );
        }
        public void AddBookmark( Object obj ) {
            bookmarkObjects.RemoveAll( v => v.Object == obj );
            bookmarkObjects.Add( obj );
        }
        public void RemoveBookmark( string path ) {
            bookmarkObjects.RemoveAll( v => v.Path == path );
        }
        public void RemoveBookmark( Object obj ) {
            bookmarkObjects.RemoveAll( v => v.Object == obj );
        }
        public void ToggleBookmark( string path ) {
            if ( Contains( path ) ) {
                RemoveBookmark( path );
            } else {
                AddBookmark( path );
            }
        }
        public void ToggleBookmark( Object obj ) {
            if ( Contains( obj ) ) {
                RemoveBookmark( obj );
            } else {
                AddBookmark( obj );
            }
        }

        public IEnumerator<ObjectWithPath> GetEnumerator( ) {
            return bookmarkObjects.GetEnumerator( );
        }

        IEnumerator IEnumerable.GetEnumerator( ) {
            return bookmarkObjects.GetEnumerator( );
        }

        public void OnBeforeSerialize( ) {
            bookmark = bookmarkObjects.Select( x => x.Path ).ToArray( );
            bookmarkLabels = bookmarkLabelTable.Select( x => x.Key + "\n" + x.Value ).ToArray( );
        }

        public void OnAfterDeserialize( ) {
            bookmarkObjects.Clear( );
            foreach ( var path in bookmark ) {
                bookmarkObjects.Add( new ObjectWithPath( path ) );
            }

            bookmarkLabelTable.Clear( );
            foreach ( var label in bookmarkLabels ) {
                var split = label.Split( '\n' );
                if ( split.Length == 2 ) {
                    bookmarkLabelTable.Add( split[0], split[1] );
                } else {
                    Debug.LogError( "BookmarkLabel Deserialize Error: " + label );
                }
            }
        }
    }
}
