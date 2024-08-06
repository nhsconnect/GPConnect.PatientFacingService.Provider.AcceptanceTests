using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPConnect.PatientFacingService.Provider.AcceptanceTests.Data;

namespace GPConnect.PatientFacingService.Provider.AcceptanceTests.Importers
{
    public static class PatientAccessTokenMapImporter
    {
        public static Dictionary<string, string> LoadCsv(string filename)
        {
            using (var csv = new CsvReader(new StreamReader(filename)))
            {
                csv.Configuration.RegisterClassMap<PatientAccessTokenMapConverter>();
                return csv.GetRecords<PatientAccessTokenMap>().ToDictionary(x => x.NativeNHSNumber, x => x.AccessToken);
            }
        }
    }
}
