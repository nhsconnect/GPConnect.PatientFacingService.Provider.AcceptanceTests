using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPConnect.PatientFacingService.Provider.AcceptanceTests.Data;

namespace GPConnect.PatientFacingService.Provider.AcceptanceTests.Importers
{
    internal sealed class PatientAccessTokenMapConverter : CsvClassMap<PatientAccessTokenMap>
    {
        public PatientAccessTokenMapConverter()
        {
            Map(p => p.NativeNHSNumber).Name("NATIVE_NHS_NUMBER");
            Map(p => p.AccessToken).Name("ACCESS_TOKEN");
        }
    }
}
