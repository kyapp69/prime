﻿using System;
using Prime.Base;
using Prime.Core;

namespace Prime.Console.Frank.Data
{
    public class DataTest
    {
        public static bool Go()
        {
            var utc = DateTime.MaxValue;
            var ctx = UserContext.Testing;
            var id = ObjectId.NewObjectId();
            var c = new DataTestModel {Id = id, DateUtc = utc, DataTestSub = new DataTestSub {Test = "ok"}};
            c.Save(ctx);

            var all = ctx.As<DataTestModel>().ToList();

            c = ctx.As<DataTestModel>().FirstOrDefault(x => x.Id == id);
            return c != null && c.DataTestSub.Test == "ok" && c.DateUtc == utc;
        }
    }
}
