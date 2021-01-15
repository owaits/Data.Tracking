using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Oarw.Data.Tracking;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Tracking.Test.TestData
{
    [TestClass]
    public class NotifyListTests
    {
        protected class NotifyTestItem : INotifyPropertyChanged
        {
            public NotifyTestItem(string value)
            {
                Value = value;
            }

            public string Value { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        [TestMethod]
        public void NotifyListSerializationTest()
        {
            NotifyList<NotifyTestItem> list = new NotifyList<NotifyTestItem>();
            list.Add(new NotifyTestItem("Test1"));
            list.Add(new NotifyTestItem("Test2"));

            var deserializedList = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(list));

            Assert.AreEqual("Test1", list[0].Value);
            Assert.AreEqual("Test2", list[1].Value);
        }
    }
}
