using System.Data;
using System.IO;
using System.Text.RegularExpressions;

namespace ExDeath
{
    class DataHandler
    {
        // reads a csv file into memory
        public static DataTable ReadCsv(string path)
        {
            StreamReader sr = new StreamReader(path, System.Text.Encoding.UTF8);
            string[] headers = sr.ReadLine().Split(',');
            DataTable dt = new DataTable();

            foreach (string header in headers)
            {
                dt.Columns.Add(header);
            }

            while (!sr.EndOfStream)
            {
                string[] rows = Regex.Split(sr.ReadLine(), ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
                DataRow dr = dt.NewRow();

                for (int i = 0; i < headers.Length; i++)
                {
                    dr[i] = rows[i];
                }

                dt.Rows.Add(dr);
            }

            return dt;
        }

        public static void ReadXlsx(string path)
        {
            //
        }
    }
}
