using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.VisualBasic.FileIO;
using SsdWebApi.Models;

namespace SsdWebApi
{
    public class Forecast
    {

        public Forecast() { }
        public string forecastSARIMAindex(string attribute)
        {
            //res e' il campo testo da inviare
            string res = "\"text\":\"";
            string interpreter = @"C:\Users\lollo\Anaconda3\envs\opanalytics\python.exe";
            PythonRunner runner = new PythonRunner(interpreter, "opanalytics");
            Bitmap mp = null;

            try
            {
                //faccio il parsing dell'output
                string command = $"Models/forecastStat.py {attribute}.csv";
                string list = runner.runDosCommands(command);
                string[] lines = list.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                string strmp = "";
                //prendo tutte le righe e guardo se sono da inviare alla textbox o contiene un'immagine
                //se b' allora immagine, stringa altrimenti
                foreach (string s in lines)
                {
                    if (s.StartsWith("Alert:")) res += s;
                    if (s.StartsWith("b'"))
                    {
                        strmp = s.Trim();
                        break;
                    }
                }
                strmp = strmp.Substring(strmp.IndexOf("b'"));
                //creo immagine
                res += "\",\"img\":\""+strmp+"\"";
                mp = runner.FromPythonBase64String(strmp);
                
            }
            catch
            {

            }
            return res;
        }

        public string createPortfolio() {
            string interpreter = @"C:\Users\lollo\Anaconda3\envs\opanalytics\python.exe";
            PythonRunner runner = new PythonRunner(interpreter, "opanalytics");
            try
            {
                //faccio il parsing dell'output
                string command = $"Models/test.py";
                string list = runner.runDosCommands(command);
                string[] lines = list.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                foreach (string s in lines)
                {
                    Console.WriteLine(s);
                }                
            }
            catch
            {
                Console.WriteLine("error");

            }
            return "ok";
        }

        public Dictionary<string,double> Optimize() {
            double initialPortfolioValue = 100000;

            //create list containing all the forecasted values for all the indexes
            List<List<double>> forecastIndexes = new List<List<double>>();
            var path = @"Models/forecast.csv"; 
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names, ma non c'è
                //csvParser.ReadLine();
                var index = 0;

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();
                    Console.WriteLine(fields[0]);
                    List<double> tmpList = new List<double>();
                    for (int i = 0; i < 120; i++) 
                    {
                        tmpList.Add(Double.Parse(fields[i].Replace('.',',')));
                    }
                    forecastIndexes.Add(tmpList);
                    index++;
                }
            }
            //forecastIndexes.ForEach(a => a.ForEach(Console.Write));
            //create list of all index variations over time
            List<List<double>> variations = new List<List<double>>();
            var id = 0;
            forecastIndexes.ForEach(index => 
            {
                var variationsTmp = new List<double>();
                for (int i = 0; i < index.Count; i++)
                {
                    if (i == 0)
                    {
                        variationsTmp.Add(0);
                    } else 
                    {
                        variationsTmp.Add((forecastIndexes[id][i]-forecastIndexes[id][i-1])/forecastIndexes[id][i-1]);
                    }
                }
                variations.Add(variationsTmp);
                id++;
            });

            //PSO pso = new PSO(2,16,1,1,1, variations, initialPortfolioValue);
            PSO pso = new PSO(2,16,0.25,1,1, variations, initialPortfolioValue);

            //pso.optimize(15, 7, 20, 4);
            pso.optimize(50, 7, 60, 10);

            Console.WriteLine(variations[0][100]);

            Dictionary<string,double> indexPerc = new Dictionary<string, double>();
            var indexes = new string[] {"SP_500","FTSE_MIB_","GOLD_SPOT","MSCI_EM","MSCI_EURO","All_Bonds","Us_Treasury"};
            var it = 0;
            pso.xbest.ForEach(id => 
            {
                //fai un enum
                indexPerc.Add(indexes[it++], Math.Round(id, 2));
            });

            String csv = String.Join(
                Environment.NewLine,
                indexPerc.Select(d => $"{d.Key}: {d.Value};")
            );
            System.IO.File.WriteAllText("percs.csv", csv);

            return indexPerc;
        }
    }
}
