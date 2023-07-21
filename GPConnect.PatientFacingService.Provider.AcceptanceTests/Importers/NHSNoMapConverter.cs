using CsvHelper.Configuration;
using GPConnect.PatientFacingService.Provider.AcceptanceTests.Data;

namespace GPConnect.PatientFacingService.Provider.AcceptanceTests.Importers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal sealed class NHSNoMapConverter : CsvClassMap<NHSNoMap>
    {
        public NHSNoMapConverter()
        {
            Map(p => p.NativeNHSNumber).Name("NATIVE_NHS_NUMBER");
            Map(p => p.ProviderNHSNumber).Name("PROVIDER_NHS_NUMBER");
        }
    }
}