﻿namespace GPConnect.PatientFacingService.Provider.AcceptanceTests.Context
{
    using System;
    using System.Collections.Generic;
    using Data;
    using Helpers;
    using Hl7.Fhir.Model;

    public static class GlobalContext
    {
        private static readonly GlobalContextHelper GlobalContextHelper = new GlobalContextHelper();

        private static class Context
        {
            public const string kTraceDirectory = "traceDirectory";
        }

        public static string TraceDirectory
        {
            get { return GlobalContextHelper.GetValue<string>(Context.kTraceDirectory); }
            set { GlobalContextHelper.SaveValue(Context.kTraceDirectory, value); }
        }

        // Data
        public static List<RegisterPatient> RegisterPatients { get; set; }
        public static Dictionary<string, string> PractionerCodeMap { get; set; }
        public static Dictionary<string, string> PatientNhsNumberMap { get; set; }

        public static Dictionary<string, string> PatientAccessTokenMap { get; set; }

        public static string PatientAccessToken { get; set; }
        public static Dictionary<string, string> OdsCodeMap { get; set; }

        public static Guid TestRunId { get; set; }
        public static int ScenarioIndex { get; set; }
        public static string PreviousScenarioTitle { get; set; }

        public static Dictionary<string, string> LocationLogicalIdentifierMap { get; set; }

        public static Dictionary<string, List<Appointment>> CreatedAppointments { get; set; }

        //Reporting
        public static List<FileBasedReportEntry> FileBasedReportList { get; set; }

        public class FileBasedReportEntry
        {
            public DateTime TestRunDateTime;
            public string Testname;
            public string TestResult;
            public string FailureMessage;
            public string FullTestNameAndParams;
            public string TestParams;
        }

        public static int CountTestRunPassed { get; set; }
        public static int CountTestRunFailed { get; set; }

        public static string DocumentURL { get; set; }
        public static byte[] DocumentContent { get; set; }
        public static string DocumentID { get; set; }
        public static string DocumentContentType { get; set; }
        public static string X_Request_ID { get; set; }

        public static string X_Correlation_ID {get; set;}
    }
}