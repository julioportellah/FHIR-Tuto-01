using System;
using System.Collections.Generic;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;

namespace fhir_cs_tutorial_01
{
    public static class Program
    {
        //private const string _fhirServer = "http://vonk.fire.ly";//"http://hapi.fhir.org/baseR4/";
        
        private static readonly Dictionary<string,string> _fhirServers = new Dictionary<string,string>()
        {
            {"PublicVonk","http://vonk.fire.ly"},
            {"PublicHAPI","http://hapi.fhir.org/baseR4/"},
            {"Local","http://vonk.fire.ly"},
        };

        private static readonly string _fhirServer = _fhirServers["PublicVonk"];


        static int Main(string[] args)
        {
            FhirClient fhirClient = new FhirClient(_fhirServer)
            {
                Settings ={
                    PreferredFormat = ResourceFormat.Json,
                    PreferredReturn = Prefer.ReturnRepresentation
                    }
            };
            CreatePatient(fhirClient, "Juan", "Perez");
            List<Patient> patients = GetPatients(fhirClient);
            System.Console.WriteLine($"Found {patients.Count} patients!");
            return 0;
        }


        static void UpdatePatient(FhirClient fhirClient, Patient patient)
        {
            patient.Telecom.Add(new ContactPoint(){
               System =  ContactPoint.ContactPointSystem.Phone,
               Value = "123.456.789",
               Use = ContactPoint.ContactPointUse.Home
            });
            patient.Gender = AdministrativeGender.Unknown;
            fhirClient.Update<Patient>(patient);
        }

        static void DeletePatient(FhirClient fhirClient, string id)
        {
            if(string.IsNullOrEmpty(id))
            {
                throw new ArgumentException(nameof(id));
            }
            fhirClient.Delete($"Patient/{id}");
        }

        static Patient ReadPatient(FhirClient fhirClient, string id)
        {
            if(string.IsNullOrEmpty(id)) throw new ArgumentException(nameof(id));
            return fhirClient.Read<Patient>($"Patient/{id}");
        }

        static void CreatePatient(FhirClient fhirClient, string familyName, string givenName)
        {
            Patient toCreatePatient = new Patient()
            {
                Name = new List<HumanName>()
                {
                    new HumanName()
                    {
                        Family = familyName,
                        Given = new List<string>(){givenName},
                    },
                },
                BirthDateElement=new Date(1994, 01, 02),

            };
            Patient patientCreated = fhirClient.Create<Patient>(toCreatePatient);
            System.Console.WriteLine($"PatientCreated with the Id: {patientCreated.Id}");
        }

        static List<Patient> GetPatients(FhirClient fhirClient, string[] patientCriteria = null, int maxPatients=20, bool onlyWithEncounters=false)
        {
            List<Patient> patients = new List<Patient>();
            Bundle patientBundle;
            if ((patientCriteria == null) || (patientCriteria.Length == 0))
            {
                patientBundle = fhirClient.Search<Patient>();
            }
            else
            {
                patientBundle = fhirClient.Search<Patient>(patientCriteria);
            }

            
            int patientNumber = 0;
            List<string> patientsWithEncounters = new List<string>();


            while(patientBundle != null)
            {
                Console.WriteLine($"Patient Bundle.Total:{patientBundle.Total} Entry count:{patientBundle.Entry.Count}");
            
                foreach (Bundle.EntryComponent entry in patientBundle.Entry)
                {
                    System.Console.WriteLine($"- Entry:{patientNumber,3}{entry.FullUrl} ");
                    if (entry.Resource != null)
                    {
                        Patient patient = (Patient)entry.Resource;
                        System.Console.WriteLine($" -  Id:{patient.Id}");

                        Bundle encounterBundle = fhirClient.Search<Encounter>(
                            new string[]{
                                $"patient=Patient/{patient.Id}"
                                }
                            );
                        if (onlyWithEncounters && (encounterBundle.Total == 0)){
                            continue;
                        }
                        if (patients.Count > maxPatients) break;
                        patients.Add(patient);

                        patientsWithEncounters.Add(patient.Id);
                        System.Console.WriteLine("**************************");
                        if (patient.Name.Count >= 0)
                        {
                            System.Console.WriteLine($" - Name: {patient.Name[0].ToString()}");
                        }

                        if (encounterBundle.Total > 0)
                        {
                            System.Console.WriteLine($" - Encounters Total {encounterBundle.Total} Entry count: {encounterBundle.Entry.Count}");
                        }
                        System.Console.WriteLine("**************************");
                    }

                    patientNumber++;
                }
                if (patients.Count > maxPatients) break;

                patientBundle = fhirClient.Continue(patientBundle);
                
            }
            return patients;
        }
        // static void Main(string[] args)
        // {
        //     FhirClient fhirClient = new FhirClient(_fhirServer)
        //     {
        //         Settings ={
        //             PreferredFormat = ResourceFormat.Json,
        //             PreferredReturn = Prefer.ReturnRepresentation
        //             }
        //     };
        //     Bundle patientBundle = fhirClient.Search<Patient>(new string[] { "name=jo" });

            

        //     int patientNumber = 0;
        //     List<string> patientsWithEncounters = new List<string>();


        //     while(patientBundle != null)
        //     {
        //         Console.WriteLine($"Total:{patientBundle.Total} Entry count:{patientBundle.Entry.Count}");
            
        //         foreach (Bundle.EntryComponent entry in patientBundle.Entry)
        //         {
        //             System.Console.WriteLine($"- Entry:{patientNumber,3}{entry.FullUrl} ");
        //             if (entry.Resource != null)
        //             {
        //                 Patient patient = (Patient)entry.Resource;
        //                 System.Console.WriteLine($" -  Id:{patient.Id}");

        //                 Bundle encounterBundle = fhirClient.Search<Encounter>(
        //                     new string[]{
        //                         $"patient=Patient/{patient.Id}"
        //                         }
        //                     );
        //                 if (encounterBundle.Total == 0){
        //                     continue;
        //                 }
        //                 patientsWithEncounters.Add(patient.Id);
        //                 System.Console.WriteLine("**************************");
        //                 if (patient.Name.Count >= 0)
        //                 {
        //                     System.Console.WriteLine($" - Name: {patient.Name[0].ToString()}");
        //                 }
        //                 System.Console.WriteLine($" - Encounters Total {encounterBundle.Total} Entry count: {encounterBundle.Entry.Count}");
        //                 System.Console.WriteLine("**************************");
        //             }

        //             patientNumber++;
        //         }
        //         patientBundle = fhirClient.Continue(patientBundle);
                
        //     }

        // }
    }
}
