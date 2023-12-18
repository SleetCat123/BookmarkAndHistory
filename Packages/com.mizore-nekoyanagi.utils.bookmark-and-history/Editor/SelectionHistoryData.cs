using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MizoreNekoyanagi.PublishUtil.BookmarkAndHistory {
    [System.Serializable]
    public class SelectionHistoryData : IEnumerable<ObjectWithPath>, ISerializationCallbackReceiver {
        const string PATH_HISTORY = "BookmarkAndHistory/MizoresSelectionHistory.json";

        [System.NonSerialized] List<ObjectWithPath> historyObjects = new  List<ObjectWithPath>(CAPACITY);
        /// <summary>
        /// Serialize用
        /// </summary>
        [SerializeField] string[] history = new string[0];

        public ObjectWithPath this[int index] {
            get {
                return historyObjects[index];
            }
        }
        public int Count {
            get {
                return historyObjects.Count;
            }
        }

        const int CAPACITY = MAX_HISOTRY + 1;
        const int MAX_HISOTRY = 50;
        public void Save( ) {
            var json = JsonUtility.ToJson( this, true );
            Debug.Log( "History Saving: \n" + json );
            var dir = Path.GetDirectoryName( PATH_HISTORY );
            if ( !Directory.Exists( dir ) ) {
                Directory.CreateDirectory( dir );
            }
            File.WriteAllText( PATH_HISTORY, json );
        }
        public void Load( ) {
            if ( File.Exists( PATH_HISTORY ) ) {
                string json = File.ReadAllText(PATH_HISTORY);
                Debug.Log( "History Loading: \n" + json );
                JsonUtility.FromJsonOverwrite( json, this );
                while ( MAX_HISOTRY < historyObjects.Count ) {
                    historyObjects.RemoveAt( 0 );
                }
                historyObjects.Capacity = CAPACITY;
            }
        }
        public void AddHisotry( Object obj ) {
            if ( obj == null ) {
                return;
            }
            historyObjects.RemoveAll( v => v.Object == obj );
            while ( MAX_HISOTRY <= historyObjects.Count ) {
                historyObjects.RemoveAt( 0 );
            }
            historyObjects.Add( obj );
        }
        public void AddHisotry( string path ) {
            if ( string.IsNullOrEmpty( path ) ) {
                return;
            }
            historyObjects.RemoveAll( v => v.Path == path );
            while ( MAX_HISOTRY <= historyObjects.Count ) {
                historyObjects.RemoveAt( 0 );
            }
            historyObjects.Add( new ObjectWithPath( path ) );
        }
        public void RemoveHistory( string path ) {
            historyObjects.RemoveAll( v => v.Path == path );
        }
        public void RemoveHistory( Object obj ) {
            historyObjects.RemoveAll( v => v.Object == obj );
        }

        public IEnumerator<ObjectWithPath> GetEnumerator( ) {
            return historyObjects.GetEnumerator( );
        }

        IEnumerator IEnumerable.GetEnumerator( ) {
            return historyObjects.GetEnumerator( );
        }

        public void OnBeforeSerialize( ) {
            history = historyObjects.ConvertAll( v => v.Path ).ToArray( );
        }

        public void OnAfterDeserialize( ) {
            historyObjects.Clear( );
            foreach ( var path in history ) {
                historyObjects.Add( new ObjectWithPath( path ) );
            }
        }
    }
}