using System.IO;
using UnityEngine;

namespace MizoreNekoyanagi.PublishUtil.BookmarkAndHistory {
    [System.Serializable]
    public class BookmarkAndHistoryWindowSettings {
        const string PATH = "BookmarkAndHistory/Settings.json";

        public bool debug = false;

        public void Save( ) {
            var json = JsonUtility.ToJson( this, true );
            if ( debug ) {
                Debug.Log( "Settings Saving: \n" + json );
            }
            var dir = Path.GetDirectoryName( PATH );
            if ( !Directory.Exists( dir ) ) {
                Directory.CreateDirectory( dir );
            }
            File.WriteAllText( PATH, json );
        }
        public void Load( ) {
            if ( File.Exists( PATH ) ) {
                string json = File.ReadAllText(PATH);
                if ( debug ) {
                    Debug.Log( "Settings Loading: \n" + json );
                }
                JsonUtility.FromJsonOverwrite( json, this );
            }
        }
    }
}
