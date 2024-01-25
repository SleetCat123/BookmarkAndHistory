using UnityEditor;
using UnityEngine;

namespace MizoreNekoyanagi.PublishUtil.BookmarkAndHistory {
    public class ObjectWithPath {
        const string SEPARATOR = "|";
        Object obj;
        string path;
        public bool IsSeparator {
            get {
                return path == SEPARATOR;
            }
        }
        public Object Object {
            get {
                if ( IsSeparator ) {
                    return null;
                }
                if ( obj == null ) {
                    obj = AssetDatabase.LoadAssetAtPath<Object>( path );
                }
                return obj;
            }
        }
        public string Path {
            get {
                if ( IsSeparator ) {
                    return path;
                }
                if ( obj != null ) {
                    path = AssetDatabase.GetAssetPath( obj );
                }
                return path;
            }
        }
        public ObjectWithPath( Object obj ) {
            this.obj = obj;
        }
        public ObjectWithPath( string path ) {
            this.path = path;
        }
        public ObjectWithPath( ) {
            this.path = SEPARATOR;
        }

        public static implicit operator ObjectWithPath( Object obj ) {
            return new ObjectWithPath( obj );
        }
    }
}
