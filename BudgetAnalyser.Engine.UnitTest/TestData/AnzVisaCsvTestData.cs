using System;
using System.Collections.Generic;

namespace BudgetAnalyser.Engine.UnitTest.TestData
{
    public static class AnzVisaCsvTestData
    {
        public static IEnumerable<string> BadTestData1()
        {
            return new List<string>
            {
                "Card,Type,Amount,Details,TransactionDate,ProcessedDate,ForeignCurrencyAmount,ConversionCharge  ",
                "4123-****-****-1234,D,32.36,Z Queen Street          Auckland      Nz ,24/06/2014,25/06/2014,   ",
                "4123-****-****-1234,D,5.00,Subway Queen Street     Auckland      Nz ,24/06/2014,24/06/2014,    ",
                "4123-****-****-1234,D,31.25,Countdown              Auckland      Nzl ,22/06/2014,23/06/2014,   ",
                "4123-****-****-1234    D  271.70   Countdown              Auckland      Nzl ,22/06/2014,23/06/2014,  ", // Some commas missing from this row
                "4123-****-****-1234,D,31.50,Auckland Best Pizza L Auckland      Nzl ,21/06/2014,23/06/2014,    ",
                "4123-****-****-1234,D,150.00,Auckland Aquarium Ltd  Orakei        Nz ,21/06/2014,23/06/2014,   ",
                "4123-****-****-1234,D,13.50,Auckland Aquarium Ltd  Orakei        Nz ,21/06/2014,23/06/2014,    ",
                "4123-****-****-1234,D,3.70,Hollywood Bakery Queen  Auckland      Nz ,20/06/2014,20/06/2014,    ",
                "4123-****-****-1234,D,4.30,Hollywood Bakery Queen  Auckland      Nz ,20/06/2014,20/06/2014,    ",
                "4123-****-****-5678,D,8.80,St. Pierre'S           Auckland      Nzl ,20/06/2014,20/06/2014,    ",
                "4123-****-****-5678,D,50.00,Mobil Queen        Auckland      Nz ,20/06/2014,20/06/2014,        ",
                "4123-****-****-5678,D,50.00,Mobil Queen        Auckland      Nz ,20/06/2014,20/06/2014,        ",
                "4123-****-****-5678,C,50.00,Mobil Queen        Auckland      Nz ,20/06/2014,20/06/2014,        "
            };
        }

        public static IEnumerable<string> TestData1()
        {
            return new List<string>
            {
                "Card,Type,Amount,Details,TransactionDate,ProcessedDate,ForeignCurrencyAmount,ConversionCharge  ",
                "4123-****-****-1234,D,32.36,Z Queen Street          Auckland      Nz ,24/06/2014,25/06/2014,   ",
                "4123-****-****-1234,D,5.00,Subway Queen Street     Auckland      Nz ,24/06/2014,24/06/2014,    ",
                "4123-****-****-1234,D,31.25,Countdown              Auckland      Nzl ,22/06/2014,23/06/2014,   ",
                "4123-****-****-1234,D,271.70,Countdown              Auckland      Nzl ,22/06/2014,23/06/2014,  ",
                "4123-****-****-1234,D,31.50,Auckland Best Pizza L Auckland      Nzl ,21/06/2014,23/06/2014,    ",
                "4123-****-****-1234,D,150.00,Auckland Aquarium Ltd  Orakei        Nz ,21/06/2014,23/06/2014,   ",
                "4123-****-****-1234,D,13.50,Auckland Aquarium Ltd  Orakei        Nz ,21/06/2014,23/06/2014,    ",
                "4123-****-****-1234,D,3.70,Hollywood Bakery Queen  Auckland      Nz ,20/06/2014,20/06/2014,    ",
                "4123-****-****-1234,D,4.30,Hollywood Bakery Queen  Auckland      Nz ,20/06/2014,20/06/2014,    ",
                "4123-****-****-5678,D,8.80,St. Pierre'S           Auckland      Nzl ,20/06/2014,20/06/2014,    ",
                "4123-****-****-5678,D,50.00,Mobil Queen        Auckland      Nz ,20/06/2014,20/06/2014,        ",
                "4123-****-****-5678,D,50.00,Mobil Queen        Auckland      Nz ,20/06/2014,20/06/2014,        ",
                "4123-****-****-5678,C,50.00,Mobil Queen        Auckland      Nz ,20/06/2014,20/06/2014,        "
            };
        }

        public static IEnumerable<string> TestData2()
        {
            return new List<string>
            {
                "Card,Type,Amount,Details,TransactionDate,ProcessedDate,ForeignCurrencyAmount,ConversionCharge  ",
                "4123-****-****-1234,D,32.36,Z Queen Street          Auckland      Nz ,24/06/2014,25/06/2014,   ",
                "4123-****-****-1234,D,5.00,Subway Queen Street     Auckland      Nz ,24/06/2014,24/06/2014,    ",
                "4123-****-****-1234,D,31.25,Countdown              Auckland      Nzl ,22/06/2014,23/06/2014,   ",
                "4123-****-****-1234,D,271.70,Countdown              Auckland      Nzl ,22/06/2014,23/06/2014,  Extra Data,More extra data",
                "4123-****-****-1234,D,31.50,Auckland Best Pizza L Auckland      Nzl ,21/06/2014,23/06/2014,    ",
                "4123-****-****-1234,D,150.00,Auckland Aquarium Ltd  Orakei        Nz ,21/06/2014,23/06/2014,   ",
                "4123-****-****-1234,D,13.50,Auckland Aquarium Ltd  Orakei        Nz ,21/06/2014,23/06/2014,    ",
                "4123-****-****-1234,D,3.70,Hollywood Bakery Queen  Auckland      Nz ,20/06/2014,20/06/2014,    ",
                "4123-****-****-1234,D,4.30,Hollywood Bakery Queen  Auckland      Nz ,20/06/2014,20/06/2014,    ",
                "4123-****-****-5678,D,8.80,St. Pierre'S           Auckland      Nzl ,20/06/2014,20/06/2014,    ",
                "4123-****-****-5678,D,50.00,Mobil Queen        Auckland      Nz ,20/06/2014,20/06/2014,        ",
                "4123-****-****-5678,D,50.00,Mobil Queen        Auckland      Nz ,20/06/2014,20/06/2014,        ",
                "4123-****-****-5678,C,50.00,Mobil Queen        Auckland      Nz ,20/06/2014,20/06/2014,        "
            };
        }

        internal static string FirstTwoLines1()
        {
            return "Card,Type,Amount,Details,TransactionDate,ProcessedDate,ForeignCurrencyAmount,ConversionCharge\r\n4323-****-****-1234,D,32.36,Z Queen Street          Auckland      Nz ,24/06/2014,25/06/2014,,\r\n";
        }
    }
}
