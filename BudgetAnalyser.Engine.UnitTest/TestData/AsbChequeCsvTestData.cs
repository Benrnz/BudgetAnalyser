namespace BudgetAnalyser.Engine.UnitTest.TestData
{
    public static class AsbChequeCsvTestData
    {
        public static IEnumerable<string> BadTestData1()
        {
            return new List<string>
            {
                "Created date / time : 24 June 2026 / 10:48:59",
                "Bank 12; Branch 3638; Account 0123486-00 (Everyday)",
                "From date 20260604",
                "To date 20260624",
                "Avail Bal : 68.76 as of 20260620",
                "Ledger Balance : 68.76 as of 20260624",
                "Date,Unique Id,Tran Type,Cheque Number,Payee,Memo,Amount",
                "",
                "2026/06/19,2026061901,EFTPOS,,BH INDIAN CUSINE AUCKLAND,EFTPOS,-26.52",
                "2026/06/20,2026062001,EFTPOS,,TIRAUMEA SUPERETTE AUCKLAND,EFTPOS,-4.72",
                "invalid data line", // This line doesn't have enough columns
                "2026/06/21,2026062101,TRANSFER,,MAIN ACCOUNT,Transfer,-100.00",
                "2026/06/22,2026062201,SALARY,,EMPLOYER DEPOSIT,Salary,2500.00",
                "2026/06/23,2026062301,DIRECT DEBIT,,UTILITY COMPANY,Bill,-150.00",
                "2026/06/24,2026062401,CHEQUE,1001,RENT PAYMENT,Cheque,-800.00"
            };
        }

        public static IEnumerable<string> TestData1()
        {
            return new List<string>
            {
                "Created date / time : 24 June 2026 / 10:48:59",
                "Bank 12; Branch 3638; Account 0123486-00 (Everyday)",
                "From date 20260604",
                "To date 20260624",
                "Avail Bal : 68.76 as of 20260620",
                "Ledger Balance : 68.76 as of 20260624",
                "Date,Unique Id,Tran Type,Cheque Number,Payee,Memo,Amount",
                "",
                "2026/06/19,2026061901,EFTPOS,,BH INDIAN CUSINE AUCKLAND,EFTPOS,-26.52",
                "2026/06/20,2026062001,EFTPOS,,TIRAUMEA SUPERETTE AUCKLAND,EFTPOS,-4.72",
                "2026/06/21,2026062101,TRANSFER,,MAIN ACCOUNT,Transfer,-100.00",
                "2026/06/22,2026062201,SALARY,,EMPLOYER DEPOSIT,Salary,2500.00",
                "2026/06/23,2026062301,DIRECT DEBIT,,UTILITY COMPANY,Bill,-150.00",
                "2026/06/24,2026062401,CHEQUE,1001,RENT PAYMENT,Cheque,-800.00",
                "2026/06/24,2026062402,CHEQUE,1002,GROCERIES STORE,Cheque,-50.00"
            };
        }

        public static IEnumerable<string> TestData2()
        {
            return new List<string>
            {
                "Created date / time : 24 June 2026 / 10:48:59",
                "Bank 12; Branch 3638; Account 0123486-00 (Everyday)",
                "From date 20260604",
                "To date 20260624",
                "Avail Bal : 68.76 as of 20260620",
                "Ledger Balance : 68.76 as of 20260624",
                "Date,Unique Id,Tran Type,Cheque Number,Payee,Memo,Amount",
                "",
                "2026/06/19,2026061901,EFTPOS,,BH INDIAN CUSINE AUCKLAND,EFTPOS,-26.52",
                "2026/06/20,2026062001,EFTPOS,,TIRAUMEA SUPERETTE AUCKLAND,EFTPOS,-4.72",
                "2026/06/21,2026062101,TRANSFER,,MAIN ACCOUNT,Transfer,-100.00",
                "2026/06/22,2026062201,SALARY,,EMPLOYER DEPOSIT,Salary,2500.00",
                "2026/06/23,2026062301,DIRECT DEBIT,,UTILITY COMPANY,Bill,-150.00",
                "2026/06/24,2026062401,CHEQUE,1001,RENT PAYMENT,Cheque,-800.00",
                "2026/06/24,2026062402,CHEQUE,1002,GROCERIES STORE,Cheque,-50.00"
            };
        }

        internal static string FirstNineLines1()
        {
            return "Created date / time : 24 June 2026 / 10:48:59\r\nBank 12; Branch 3638; Account 0123486-00 (Everyday)\r\nFrom date 20260604\r\nTo date 20260624\r\nAvail Bal : 68.76 as of 20260620\r\nLedger Balance : 68.76 as of 20260624\r\nDate,Unique Id,Tran Type,Cheque Number,Payee,Memo,Amount\r\n\r\n2026/06/19,2026061901,EFTPOS,,BH INDIAN CUSINE AUCKLAND,EFTPOS,-26.52";
        }
    }
}
