using System;
using LiteDB;

namespace Prime.Core
{
    public class PublicFast
    {
        public static void Delete<T>(ObjectId id)
        {
            PublicContext.I.GetCollection<T>().Delete(id);
        }

        public static T GetCreate<T>(ObjectId id, Func<T> create = null, Func<T,bool> confirm = null) where T : IModelBase
        {
            var e = PublicContext.I.As<T>().FirstOrDefault(x => x.Id == id);
            if (e != null && confirm?.Invoke(e)!=false)
                return e;

            if (create == null)
                return default;

            e = create();

            if (e == null)
                return e;

            e.Id = id;
            e.SavePublic();
            return e;
        }

        public static T GetCreate<T>(Func<T> create = null, Func<T, bool> confirm = null) where T : IModelBase
        {
            var e = PublicContext.I.As<T>().FirstOrDefault();
            if (e != null && confirm?.Invoke(e) != false)
                return e;

            if (create == null)
                return default;

            e = create();

            if (e == null)
                return e;

            e.SavePublic();
            return e;
        }
    }
}