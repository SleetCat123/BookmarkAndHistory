using UnityEditor;
using UnityEngine;

namespace MizoreNekoyanagi.PublishUtil.BookmarkAndHistory {
    public class ObjectWithPath {
        Object obj;
        string path;
        public Object Object {
            get {
                if ( obj == null ) {
                    obj = AssetDatabase.LoadAssetAtPath<Object>( path );
                }
                return obj;
            }
        }
        public string Path {
            get {
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

        public static implicit operator ObjectWithPath( Object obj ) {
            return new ObjectWithPath( obj );
        }
    }
}
