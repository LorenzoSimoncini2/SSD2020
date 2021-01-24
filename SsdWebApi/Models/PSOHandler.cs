using System;
using System.Collections.Generic;
using System.Linq;

namespace SsdWebApi.Models
{
    public class PSOHandler
    {
        private List<List<double>> variations;
        private double initialValue;


        public PSOHandler(List<List<double>> vars, double initV)
        {
            variations = vars;
            initialValue = initV;
        }

        double rosenbrock(double[] xvec)
        {
        double sum = 0;
        int i, dim = xvec.Length;

        for (i = 0; i < dim - 1; i++)

            sum += 100 * Math.Pow((xvec[i + 1] - Math.Pow(xvec[i], 2)), 2) + Math.Pow((1 - xvec[i]), 2);

        return sum;
        }

        public double portfolioFitness(double[] xvec)
        {
            List<List<double>> portfolio = new List<List<double>>();
            for (int i = 0; i < xvec.Length; i++)
            {
                portfolio.Add(new List<double>{initialValue/100 * xvec[i]});
                for (int j = 1; j < variations[i].Count; j++)
                {
                    portfolio[i].Add((1+variations[i][j])*portfolio[i][j-1]);
                }
            }
            //portfolio value with all the indexes summed
            List<double> cumulatedPortfolio = new List<double>();
            for (int i = 0; i < portfolio[0].Count; i++)
            {
                double tmpSum = 0;
                for (int j = 0; j < portfolio.Count; j++)
                    tmpSum += portfolio[j][i];
                cumulatedPortfolio.Add(tmpSum);
            }
            //starts from portfolio index 20 
            List<double> avg20Day = new List<double>();
            for (int i = 20; i < cumulatedPortfolio.Count; i++)
            {
                avg20Day.Add(cumulatedPortfolio.GetRange(i-20,i-1).Average());
            }
            //divide by 100 to normalize
            double ret = avg20Day.Average();
            //compute squared error
            List<double> sqError = new List<double>();
            for (int i = 20; i < cumulatedPortfolio.Count; i++)
            {
                sqError.Add(Math.Pow(cumulatedPortfolio[i]-avg20Day[i-20],2));
            }
            double risk = Math.Sqrt(sqError.Sum()/sqError.Count);
            
            return 2;
        }

         public double execPSO()
         {
            //def ndim, xmin, xmax 
            return 2;
         }
    }
}