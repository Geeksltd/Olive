//using Newtonsoft.Json;
//using System;

//namespace Olive.Entities.Data
//{
//    [Serializable]
//    class Serialized
//    {
//        public string Object;
//        public string TypeName;

//        public Serialized() { }

//        public Serialized(object entity, Type type = null)
//        {
//            if (type == null) type = entity.GetType();

//            Object = Jil.JSON.SerializeDynamic(entity);
//            TypeName = type.AssemblyQualifiedName;
//        }

//        internal object Extract()
//        {
//            var type = Type.GetType(TypeName);
//            return Jil.JSON.Deserialize(Object, type);
//        }
//    }
//}