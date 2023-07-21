﻿using CsvHelper.Configuration;
using GPConnect.PatientFacingService.Provider.AcceptanceTests.Data;

namespace GPConnect.PatientFacingService.Provider.AcceptanceTests.Importers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal sealed class PractitionerCodeMapConverter : CsvClassMap<PractitionerCodeMap>
    {
        public PractitionerCodeMapConverter()
        {
            Map(p => p.NativePractitionerCode).Name("NATIVE_PRACTITIONER_CODE");
            Map(p => p.ProviderPractitionerCode).Name("PROVIDER_PRACTITIONER_CODE");
        }
    }
}