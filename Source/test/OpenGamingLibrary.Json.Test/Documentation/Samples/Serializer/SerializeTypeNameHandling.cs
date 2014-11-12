using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenGamingLibrary.Json.Test.Documentation.Samples.Serializer
{
    public class SerializeTypeNameHandling
    {
        #region Types
        public abstract class Business
        {
            public string Name { get; set; }
        }

        public class Hotel : Business
        {
            public int Stars { get; set; }
        }

        public class Stockholder
        {
            public string FullName { get; set; }
            public IList<Business> Businesses { get; set; }
        }
        #endregion

        public void Example()
        {
            #region Usage
            Stockholder stockholder = new Stockholder
            {
                FullName = "Steve Stockholder",
                Businesses = new List<Business>
                {
                    new Hotel
                    {
                        Name = "Hudson Hotel",
                        Stars = 4
                    }
                }
            };

            string jsonTypeNameAll = JsonConvert.SerializeObject(stockholder, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            Console.WriteLine(jsonTypeNameAll);
            // {
            //   "$type": "OpenGamingLibrary.Json.Samples.Stockholder, OpenGamingLibrary.Json.Test",
            //   "FullName": "Steve Stockholder",
            //   "Businesses": {
            //     "$type": "System.Collections.Generic.List`1[[OpenGamingLibrary.Json.Samples.Business, OpenGamingLibrary.Json.Test]], mscorlib",
            //     "$values": [
            //       {
            //         "$type": "OpenGamingLibrary.Json.Samples.Hotel, OpenGamingLibrary.Json.Test",
            //         "Stars": 4,
            //         "Name": "Hudson Hotel"
            //       }
            //     ]
            //   }
            // }

            string jsonTypeNameAuto = JsonConvert.SerializeObject(stockholder, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            Console.WriteLine(jsonTypeNameAuto);
            // {
            //   "FullName": "Steve Stockholder",
            //   "Businesses": [
            //     {
            //       "$type": "OpenGamingLibrary.Json.Samples.Hotel, OpenGamingLibrary.Json.Test",
            //       "Stars": 4,
            //       "Name": "Hudson Hotel"
            //     }
            //   ]
            // }

            // for security TypeNameHandling is required when deserializing
            Stockholder newStockholder = JsonConvert.DeserializeObject<Stockholder>(jsonTypeNameAuto, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            Console.WriteLine(newStockholder.Businesses[0].GetType().Name);
            // Hotel
            #endregion
        }
    }
}