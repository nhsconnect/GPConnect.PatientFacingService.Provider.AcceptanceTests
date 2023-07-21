namespace GPConnect.PatientFacingService.Provider.AcceptanceTests.Steps
{
    using System.Collections.Generic;
    using System.Linq;
    using Context;
    using Helpers;
    using Hl7.Fhir.Model;
    using TechTalk.SpecFlow;

    [Binding]
    internal class TeardownSteps : Steps
    {
        private static HttpContext _httpContext;
       
        private static PatientSteps _patientSteps;
       
        private static bool appointmentCreated;

        public TeardownSteps(
            HttpContext httpContext,

            PatientSteps patientSteps 
           
            )
        {
            _httpContext = httpContext;
            
            _patientSteps = patientSteps;
          
        }
        [BeforeScenario]
        public void SetFlagToFalse()
        {
            appointmentCreated = false;
        }

        [AfterScenario]
        public void Dummy()
        {
            
        }

        [AfterTestRun]
        public static void CancelCreatedAppointments()
        {
            if (AppSettingsHelper.TeardownEnabled && appointmentCreated == true)
            {
                StoreAllCreatedAppointments();
              
            }
        }

        public static void AppointmentCreated()
        {
            appointmentCreated = true;
        }

      
       

        private static void StoreAllCreatedAppointments()
        {
            var patients = GlobalContext.PatientNhsNumberMap;

        }

        private static void StorePatientCreatedAppointments(string nhsNumber, List<Appointment> patientAppointments)
        {
            if (GlobalContext.CreatedAppointments == null)
            {
                GlobalContext.CreatedAppointments = new Dictionary<string, List<Appointment>>();
            }

            GlobalContext.CreatedAppointments.Add(nhsNumber, patientAppointments);
        }

      
    }
}
