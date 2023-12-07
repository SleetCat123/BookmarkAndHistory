using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class SelectionHistoryData : IEnumerable<string> {
    [SerializeField]
    List<string> history = new  List<string>(CAPACITY);
    const string PATH_HISTORY = "BookmarkAndHistory/MizoresSelectionHistory.json";

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
            while ( MAX_HISOTRY < history.Count ) {
                history.RemoveAt( 0 );
            }
            history.Capacity = CAPACITY;
        }
    }
    public void AddHisotry( string path ) {
        history.RemoveAll( v => v == path );
        history.Add( path );
        while ( MAX_HISOTRY < history.Count ) {
            history.RemoveAt( 0 );
        }
    }

    public IEnumerator<string> GetEnumerator( ) {
        return history.GetEnumerator( );
    }

    IEnumerator IEnumerable.GetEnumerator( ) {
        return history.GetEnumerator( );
    }
}