using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace RepairTech
{
    public class InfoRecord
    {
        public string CellPhone { get; set; }
        public string Hardware { get; set; }
        public string Issue { get; set; }
        public string preApproval { get; set; }
        public string CalculatedTime { get; set; }
        public string Status { get; set; }
        public string ReadPrivacy { get; set; }
    }

    public static class MainSys
    {
        public const string DB = "TrackerDB.txt";
        public const int TECHS = 2;
        public const int HOURS_PER_TECH = 8;
        public const int HOURS_PER_DEVICE = 1;

        public static void CheckIfFileExisting()
        {
            if (!File.Exists(DB)) File.Create(DB).Close();
        }

        public static List<InfoRecord> ReadAllRecords()
        {
            var records = new List<InfoRecord>();
            if (!File.Exists(DB)) return records;

            string[] lines = File.ReadAllLines(DB);
            InfoRecord currentRecord = null;

            foreach (string line in lines)
            {
                if (line.StartsWith("Customer Phone Number: "))
                {
                    currentRecord = new InfoRecord { CellPhone = line.Substring(23) };
                }
                else if (line.StartsWith("Hardware: ") && currentRecord != null) currentRecord.Hardware = line.Substring(6);
                else if (line.StartsWith("Issue: ") && currentRecord != null) currentRecord.Issue = line.Substring(8);
                else if (line.StartsWith("Defective Part Replacement pre-Approved: ") && currentRecord != null) currentRecord.preApproval = line.Substring(27);
                else if (line.StartsWith("Calculated Pick up Time: ") && currentRecord != null) currentRecord.CalculatedTime = line.Substring(28);
                else if (line.StartsWith("Status: ") && currentRecord != null) currentRecord.Status = line.Substring(8);
                else if (line.StartsWith("ReadPrivacy: ") && currentRecord != null) currentRecord.ReadPrivacy = line.Substring(8);
                else if (line == "#####" && currentRecord != null)
                {
                    records.Add(currentRecord);
                    currentRecord = null;
                }
            }
            return records;
        }

        public static void WriteAllRecords(List<InfoRecord> records)
        {
            using (StreamWriter sw = new StreamWriter(DB, false))
            {
                foreach (var record in records)
                {
                    sw.WriteLine("Customer Phone Number: " + record.CellPhone);
                    sw.WriteLine("Hardware: " + record.Hardware);
                    sw.WriteLine("Issue: " + record.Issue);
                    sw.WriteLine("Defective Part Replacement pre-Approved: " + record.preApproval);
                    sw.WriteLine("Calculated Diagnostic Time: " + record.CalculatedTime);
                    sw.WriteLine("Status: " + record.Status);
                    sw.WriteLine("ReadPrivacy: " + record.ReadPrivacy);
                    sw.WriteLine("#####");
                }
            }
        }

        public static bool RecordExisting(string CellPhone)
        {
            return SearchForRecord(CellPhone) != null;
        }

        public static InfoRecord SearchForRecord(string CellPhone)
        {
            List<InfoRecord> records = ReadAllRecords();
            foreach (var record in records)
            {
                if (record.CellPhone == CellPhone)
                {
                    return record;
                }
            }
            return null;
        }

        public static int CountRecords()
        {
            return ReadAllRecords().Count;
        }

        public static void AppendRecordToFile(string CellPhone, string Hardware, string issue, string preapproval, string calcTime, string status, string readPrivacyFlag)
        {
            using (StreamWriter sw = new StreamWriter(DB, true))
            {
                sw.WriteLine("Customer Phone Number: " + CellPhone);
                sw.WriteLine("Hardware: " + Hardware);
                sw.WriteLine("Issue: " + issue);
                sw.WriteLine("Defective Part Replacement pre-Approved: " + preapproval);
                sw.WriteLine("Calculated Pick up Time: " + calcTime);
                sw.WriteLine("Status: " + status);
                sw.WriteLine("ReadPrivacy: " + readPrivacyFlag);
                sw.WriteLine("#####");
            }
        }

        public static bool DeleteRecord(string CellPhone)
        {
            List<InfoRecord> records = ReadAllRecords();
            List<InfoRecord> newRecords = new List<InfoRecord>();
            bool deleted = false;

            foreach (var record in records)
            {
                if (record.CellPhone == CellPhone)
                {
                    deleted = true;
                }
                else
                {
                    newRecords.Add(record);
                }
            }

            if (deleted)
            {
                WriteAllRecords(newRecords);
                return true;
            }
            return false;
        }

        public static bool UpdateRecordStatus(string CellPhone, string newStatus)
        {
            List<InfoRecord> records = ReadAllRecords();
            bool updated = false;

            foreach (var record in records)
            {
                if (record.CellPhone == CellPhone)
                {
                    record.Status = newStatus;
                    updated = true;
                }
            }

            if (updated)
            {
                WriteAllRecords(records);
                return true;
            }
            return false;
        }

        public static bool SetReadPrivacyFlag(string CellPhone, string flag)
        {
            List<InfoRecord> records = ReadAllRecords();
            bool updated = false;

            foreach (var record in records)
            {
                if (record.CellPhone == CellPhone)
                {
                    record.ReadPrivacy = flag;
                    updated = true;
                }
            }

            if (updated)
            {
                WriteAllRecords(records);
                return true;
            }
            return false;
        }
    }

    public class Customer
    {
        public void Register(string CellPhone)
        {
            Console.Clear();
            Console.WriteLine("Customer Registeration\n");

            Console.WriteLine("Select Hardware Type or Press Q to Quit:");
            Console.WriteLine("1 - Windows");
            Console.WriteLine("2 - Other");
            string HardwareType = Console.ReadLine();
            if (HardwareType != null && HardwareType.ToUpper() == "Q") return;

            string Hardware = (HardwareType == "1") ? "Windows" : "Other";

            Console.WriteLine("Enter issue description or Press Q to Quit:");
            string issue = Console.ReadLine();
            if (issue != null && issue.ToUpper() == "Q") return;

            Console.WriteLine("Pre-Approve defective parts replacement if required? (Y/N) or Press Q to Quit:");
            string preapproval = Console.ReadLine();
            if (preapproval != null && preapproval.ToUpper() == "Q") return;

            while (true)
            {
                Console.WriteLine("\n1 - Submit");
                Console.WriteLine("2 - Cancel and Start all Over");
                Console.WriteLine("Q - Quit to Main Menu");
                string selection = Console.ReadLine();

                if (selection != null && selection.ToUpper() == "Q")
                {
                    return;
                }
                else if (selection == "2")
                {
                    Register(CellPhone);
                    return;
                }
                else if (selection == "1")
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid Entry!");
                }
            }

            int calcTimeHours = CalcCalculatedTime();
            string calcTimeString = FormatCalculatedTime(calcTimeHours);

            string status = "Queue";
            string readPrivacyFlag = "N";

            MainSys.AppendRecordToFile(CellPhone, Hardware, issue, preapproval, calcTimeString, status, readPrivacyFlag);

            Console.WriteLine("\nSubmitted!");
            Console.WriteLine("Calculated Pick up Time: " + calcTimeString);
            Program.WaitForInput();
        }

        public void ShowStatus(string CellPhone)
        {
            InfoRecord record = MainSys.SearchForRecord(CellPhone);
            if (record == null)
            {
                Console.WriteLine("No Info found for this Phone Number!");
            }
            else
            {
                Console.WriteLine("Current status: " + record.Status);
            }
            Program.WaitForInput();
        }

        private int CalcCalculatedTime()
        {
            int totalAppointments = MainSys.CountRecords();
            int totalCapacityHours = MainSys.TECHS * MainSys.HOURS_PER_TECH;
            int usedHours = totalAppointments * MainSys.HOURS_PER_DEVICE;

            int remainingHours = Math.Max(0, totalCapacityHours - usedHours);
            int CalculatedCompletingHours = usedHours + MainSys.HOURS_PER_DEVICE;

            return CalculatedCompletingHours;
        }

        private string FormatCalculatedTime(int hours)
        {
            DateTime completingTime = DateTime.Now.AddHours(hours);
            return completingTime.ToString("yyyy-MM-dd HH:mm");
        }
    }

    public class Admin
    {
        public void Menu()
        {
            while (true)
            {
                Console.Clear();
                int total = MainSys.CountRecords();
                Console.WriteLine("***Admin Menu***");
                Console.WriteLine("There are currently " + total + " devices in repair");
                
                Console.WriteLine("\nEnter customer Phone Number or Type ex to Exit:");
                string CellPhone = Console.ReadLine();

                if (CellPhone == "ex") return;

                Console.WriteLine("\nSelect Action for " + CellPhone + ":");
                Console.WriteLine("1 - Check Customer Out");
                Console.WriteLine("2 - Update Status to Queue");
                Console.WriteLine("3 - Update Status to Troubleshooting");
                Console.WriteLine("4 - Update Status to Complete");
                Console.WriteLine("5 - Mark ReadPrivacy and Open printout");
                Console.WriteLine("ex - Exit");
                
                string selection = Console.ReadLine();

                if (selection == "ex") return;

                if (selection == "1")
                {
                    CheckOut(CellPhone);
                }
                else if (selection == "2")
                {
                    UpdateStatus(CellPhone, "Queue");
                }
                else if (selection == "3")
                {
                    UpdateStatus(CellPhone, "Troubleshooting");
                }
                else if (selection == "4")
                {
                    UpdateStatus(CellPhone, "Complete");
                }
                else if (selection == "5")
                {
                    MarkReadPrivacy(CellPhone);
                }
                else
                {
                    Console.WriteLine("Invalid Entry!");
                    Program.WaitForInput();
                }
            }
        }

        private void CheckOut(string CellPhone)
        {
            if (MainSys.DeleteRecord(CellPhone))
            {
                Console.WriteLine("Checked out!");
            }
            else
            {
                Console.WriteLine("No logs found!");
            }
            Program.WaitForInput();
        }

        private void UpdateStatus(string CellPhone, string newStatus)
        {
            if (MainSys.UpdateRecordStatus(CellPhone, newStatus))
            {
                Console.WriteLine("Status updated to " + newStatus);
            }
            else
            {
                Console.WriteLine("No logs found.");
            }
            Program.WaitForInput();
        }

        private void MarkReadPrivacy(string CellPhone)
        {
            if (MainSys.SetReadPrivacyFlag(CellPhone, "Y"))
            {
                Console.WriteLine("Please Don't Forget to Print and Attach the Privacy Document");

            }
            else
            {
                Console.WriteLine("No logs found!");
            }
            Program.WaitForInput();
        }
    }

    class Program
    {
        const string ADMIN_CellPhone = "0000"; //Admin login (password)

        static void Main(string[] args)
        {
            MainSys.CheckIfFileExisting();
            Customer customer = new Customer();
            Admin admin = new Admin();

            while (true)
            {
                Console.Clear();
                Console.WriteLine("***Hardware Repair Shop Customer Menu***");
                Console.Write("Enter at Least 9 Digits Phone Number (No Spaces or Characters) or Press Q to Quit: ");
                string CellPhone = Console.ReadLine();

                if (CellPhone != null && CellPhone.ToUpper() == "Q" || CellPhone.Length < 4)
                {
                    Environment.Exit(0);
                }

                if (CellPhone == ADMIN_CellPhone)
                {
                    admin.Menu();
                }
                else
                {
                    if (MainSys.RecordExisting(CellPhone))
                    {
                        customer.ShowStatus(CellPhone);
                    }
                    else
                    {
                        customer.Register(CellPhone);
                    }
                }
            }
        }

        public static void WaitForInput()
        {
            Console.WriteLine("\nPress Anything to Continue");
            Console.ReadKey();
        }
    }
}