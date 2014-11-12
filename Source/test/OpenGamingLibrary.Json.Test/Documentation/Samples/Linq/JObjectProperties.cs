using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenGamingLibrary.Json.Linq;

namespace OpenGamingLibrary.Json.Test.Documentation.Samples.Linq
{
    public class JObjectProperties
    {
        public void Example()
        {
            #region Usage
            JObject o = new JObject
            {
                { "name1", "value1" },
                { "name2", "value2" }
            };

            foreach (JProperty property in o.Properties())
            {
                Console.WriteLine(property.Name + " - " + property.Value);
            }
            // name1 - value1
            // name2 - value2

            foreach (KeyValuePair<string, JToken> property in o)
            {
                Console.WriteLine(property.Key + " - " + property.Value);
            }
            // name1 - value1
            // name2 - value2
            #endregion
        }
    }
}