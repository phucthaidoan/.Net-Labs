using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ClosedXML.Excel;

namespace ClosedXML_Lab
{
    class Program
    {
        static void Main(string[] args)
        {
            var wb = new XLWorkbook();

            var dataSet = GetDataSet();

            // Add all DataTables in the DataSet as a worksheets
            var worksheet = wb.Worksheets.Add(GetTable("Patients"));
            var table = worksheet.Tables.FirstOrDefault();

            if (table != null)
            {
                //table.ShowAutoFilter = false;
                table.Theme = XLTableTheme.None;
                ;

            }

            wb.SaveAs("AddingDataSet.xlsx");
        }

        private static DataSet GetDataSet()
        {
            var ds = new DataSet();
            ds.Tables.Add(GetTable("Patients"));
            ds.Tables.Add(GetTable("Employees"));
            ds.Tables.Add(GetTable("Information"));
            return ds;
        }

        public static DataTable GetTable(String tableName)
        {
            DataTable table = new DataTable();
            table.TableName = tableName;
            table.Columns.Add("Dosage", typeof(int));
            table.Columns.Add("Drug", typeof(string));
            table.Columns.Add("Patient", typeof(string));
            table.Columns.Add("Date", typeof(DateTime));

            table.Rows.Add(25, "Indocin", "David", DateTime.Now);
            table.Rows.Add(50, "Enebrel", "Sam", DateTime.Now);
            table.Rows.Add(10, "Hydralazine", "Christoff", DateTime.Now);
            table.Rows.Add(21, "Combivent", "Janet", DateTime.Now);
            table.Rows.Add(100, "Dilantin", "Melanie", DateTime.Now);
            return table;
        }
    }

    class Person
    {
        public String House { get; set; }
        public String Name { get; set; }
        public int Age { get; set; }
        public DateTime? DOB => DateTime.Now;
        public decimal decimalValue => 9.375533m;
        public bool YesNo { get; set; }
        public IEnumerable<string> stringList => Enumerable.Repeat("aaa", 2);
    }

    
}
