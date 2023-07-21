using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using GPConnect.PatientFacingService.Provider.AcceptanceTests.Data;

namespace GPConnect.PatientFacingService.Provider.AcceptanceTests.Importers
{
    public static class NHSNoMapImporter
    {
        public static Dictionary<string, string> LoadCsv(string filename)
        {
            using (var csv = new CsvReader(new StreamReader(filename)))
            {
                csv.Configuration.RegisterClassMap<NHSNoMapConverter>();
                return csv.GetRecords<NHSNoMap>().ToDictionary(x => x.NativeNHSNumber, x => x.ProviderNHSNumber);
            }
        }
    }
}
