using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace TripNumberUpdate
{
    internal class Program
    {
        #region Private Fields

        private const int HeadcodeCharDeduction = 32;

        #endregion Private Fields

        #region Private Methods

        private static string GetCharactersCode(string headcode)
        {
            var result = default(string);

            if (!string.IsNullOrWhiteSpace(headcode))
            {
                var given = headcode.Trim();

                var transfer = new StringBuilder();

                foreach (var character in given)
                {
                    var number = (int)character - HeadcodeCharDeduction;
                    transfer.Append(number);
                }

                result = transfer.ToString();
            }

            return result;
        }

        private static string GetDates(IEnumerable<Import> imports)
        {
            var result = new StringBuilder();

            var ordereds = imports
                .OrderBy(i => i.RUNNING_DAY).ToArray();

            foreach (var ordered in ordereds)
            {
                if (result.Length > 0)
                {
                    result.Append(", ");
                }

                var date = DateTime.ParseExact(
                    s: ordered.RUNNING_DAY,
                    format: "yyyyMMdd",
                    provider: CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");

                result.Append($"to_date('{date}', 'YYYY-MM-DD')");
            }

            return $"({result})";
        }

        private static IEnumerable<string> GetSqls(IEnumerable<Import> imports)
        {
            var dateGroups = imports
                .GroupBy(i => new { i.OLD_ID, i.KEY }).ToArray();

            foreach (var dateGroup in dateGroups)
            {
                var dates = GetDates(dateGroup);

                var tripNumberOld = GetCharactersCode(dateGroup.First().OLD_ID);
                var tripNumberNew = GetCharactersCode(dateGroup.First().KEY);

                var result = $"update fpl_fahrt set externefahrtnummer = {tripNumberNew} where intnum_fahrt in " +
                    $"(select intnum_fahrt from fpl_kalendertag kt join fpl_betriebstag bt on kt.intnum_betriebstag = bt.intnum_betriebstag " +
                    $"join fpl_fahrt f on bt.intnum_betriebstag = f.intnum_betriebstag and kt.intnum_fahrplanversion = f.intnum_fahrplanversion " +
                    $"where f.externefahrtnummer = {tripNumberOld} and kt.datum in {dates});";

                yield return result;
            }
        }

        private static void Main(string[] args)
        {
            var imports = default(IEnumerable<Import>);

            using (var reader = new StreamReader(args[0]))
            using (var csv = new CsvReader(
                reader: reader,
                culture: CultureInfo.InvariantCulture))
            {
                csv.Configuration.HasHeaderRecord = true;
                csv.Configuration.Delimiter = ";";
                imports = csv.GetRecords<Import>().ToArray();
            }

            var sqls = GetSqls(imports)
                .Distinct().ToArray();

            using (StreamWriter outputFile = new StreamWriter(args[1]))
            {
                foreach (string sql in sqls)
                    outputFile.WriteLine(sql);
            }
        }

        #endregion Private Methods
    }
}