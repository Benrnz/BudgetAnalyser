using System.Collections.Generic;

namespace BudgetAnalyser.Engine.UnitTest.TestData
{
    public static class AnzChequeCsvTestData
    {
        public static IEnumerable<string> BadTestData1()
        {
            return new List<string>
            {
                "Payment,Acme Inc,Acme LLB Inc,Smith Vj,1671190,-23.40,19/06/2014,",
                "Salary,Payroll Ltd,Warner Bros,,Payroll,2000.00,19/06/2014,",
                "Payment,Water Services,Smith B,33 Queen St,4112233-02,-46.96,18/06/2014,",
                "Debit Transfer 1234-****-****-4321,june pmt -288.00,17/06/2014,", // Commas missing from this row
                "Automatic Payment,Collage fund bank-00,bank-00,Jane,Uni,-100.00,16/06/2014,",
                "Automatic Payment,General Savings,Min Savings,,,-20.00,16/06/2014,",
                "Atm Debit,Anz  1234567 Queen St,Anz  S3A1234,Queen St,Anch  123456,-80.00,16/06/2014,"
            };
        }

        public static IEnumerable<string> TestData1()
        {
            return new List<string>
            {
                "Payment,Acme Inc,Acme LLB Inc,Smith Vj,1671190,-23.40,19/06/2014,",
                "Salary,Payroll Ltd,Warner Bros,,Payroll,2000.00,19/06/2014,",
                "Payment,Water Services,Smith B,33 Queen St,4112233-02,-46.96,18/06/2014,",
                "Debit Transfer,1234-****-****-4321,june pmt,,,-288.00,17/06/2014,",
                "Automatic Payment,Collage fund bank-00,bank-00,Jane,Uni,-100.00,16/06/2014,",
                "Automatic Payment,General Savings,Min Savings,,,-20.00,16/06/2014,",
                "Atm Debit,Anz  1234567 Queen St,Anz  S3A1234,Queen St,Anch  123456,-80.00,16/06/2014,"
            };
        }

        public static IEnumerable<string> TestData2()
        {
            return new List<string>
            {
                "Payment,Acme Inc,Acme LLB Inc,Smith Vj,1671190,-23.40,19/06/2014,",
                "Salary,Payroll Ltd,Warner Bros,,Payroll,2000.00,19/06/2014,Extra data,More extra data",
                "Payment,Water Services,Smith B,33 Queen St,4112233-02,-46.96,18/06/2014,",
                "Debit Transfer,1234-****-****-4321,june pmt,,,-288.00,17/06/2014,",
                "Automatic Payment,Collage fund bank-00,bank-00,Jane,Uni,-100.00,16/06/2014,",
                "Automatic Payment,General Savings,Min Savings,,,-20.00,16/06/2014,",
                "Atm Debit,Anz  1234567 Queen St,Anz  S3A1234,Queen St,Anch  123456,-80.00,16/06/2014,"
            };
        }
    }
}