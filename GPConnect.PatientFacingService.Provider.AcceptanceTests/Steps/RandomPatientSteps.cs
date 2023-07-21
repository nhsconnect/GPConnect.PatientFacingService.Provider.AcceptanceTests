﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPConnect.PatientFacingService.Provider.AcceptanceTests.Steps
{
    internal class RandomPatientSteps

    {
// removed patients 9,10 and 15 github ref 115
// removed patients 11,12 github ref 144
        private static IReadOnlyList<string> patients = new List<string>() {
            "patient1", "patient2", "patient3","patient4","patient5","patient6",
            "patient7", "patient8", "patient13", "patient16","patient17"
        };

        public static string ReturnRandomPatient()
        {
            Random rnd = new Random();
            int patientIndex = rnd.Next(patients.Count);
            return patients[patientIndex];
        }
    }
}