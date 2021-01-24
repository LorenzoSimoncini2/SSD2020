using System;
using System.Collections.Generic;
using System.Linq;

namespace SsdWebApi.Models
{
    public class PSO 
    {
        double xmin, xmax, c0, c1, c2;

        double fitbest;

        public List<double> xbest = new List<double>();
        private List<List<double>> variations;
        private double initialValue;

        public PSO(double _xmin, double _xmax, double _c0, double _c1, double _c2, List<List<double>> va, double iV)
        {
            xmin = _xmin;
            xmax = _xmax;
            fitbest = double.MinValue;
            c0 = _c0;
            c1 = _c1;
            c2 = _c2;
            variations = va;
            initialValue = iV;
        }

        double paraboloid(double[] xvec)
        {
            double sum = 0;
            int i;

            for (i = 0; i < xvec.Length; i++)
                sum += Math.Pow(xvec[i], 2);

            return sum;
        }

        public double portfolioFitness(double[] xvec)
        {
            double w1 = 1.6;
            double w2 = 1.2;
            double normRet = 103497;
            double normRis = 729;

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
                avg20Day.Add(cumulatedPortfolio.GetRange(i-20,20).Average());
            }
            //divide by 100 to normalize
            List<double> last20avg = avg20Day.GetRange(avg20Day.Count-21,20);
            double ret = last20avg.Average();
            //compute squared error
            List<double> sqError = new List<double>();
            for (int i = 20; i < cumulatedPortfolio.Count; i++)
            {
                sqError.Add(Math.Pow(cumulatedPortfolio[i]-avg20Day[i-20],2));
            }
            double risk = Math.Sqrt(sqError.Sum()/sqError.Count);

            return w1 * (ret/normRet) - w2* (risk/normRis);
        }

        public double optimize(int nparts, int ndim, int niter, int nhoodSize) 
        {
            double r1, r2;
            Random rand = new Random(550);
            Random randAlt = new Random();

            //init particles 
            PSOParticle[] parts = new PSOParticle[nparts];
            for (int i = 0; i < parts.Length; i++)
            {
                //create single particle
                var p = new PSOParticle();
                p.X = new double[ndim];
                p.V = new double[ndim];
                p.PersonalBest = new double[ndim];
                p.LocalBest = new double[ndim];
                double sum = 0;
                for (int j = 0; j < ndim; j++)
                {
                    /*if (j !=  ndim-1)
                    {
                        p.X[j] = rand.NextDouble()*(xmax-xmin)+xmin;
                        
                    } else 
                    {
                        double sum = 0; 
                        for (int w = 0; w < ndim-1; w++)
                        {
                            sum += p.X[w];
                        }
                        p.X[j] = 100-sum;
                    }*/
                    p.X[j] = rand.NextDouble()*(xmax-xmin)+xmin;
                    if (j == ndim-1 && (sum + p.X[j] > 100))
                        p.X[j] = 100 - sum;
                    sum += p.X[j];
                    //togli sopra
                    p.PersonalBest[j] = p.X[j];
                    p.LocalBest[j] = p.X[j];
                    p.V[j] = (rand.NextDouble()-rand.NextDouble())*0.5*(xmax-xmin)-xmin;
                    
                }
                double rest = 100 - sum;
                if (rest > 0) 
                {
                    for (int j = 0; j < ndim; j++)
                    {
                        p.X[j] += rest/ndim;
                    }
                } 
                p.Fit = portfolioFitness(p.X);
                p.FitBest = p.Fit;
                //neighborhood
                p.neighboursIDs = new int[nhoodSize];
                for (int j = 0; j < nhoodSize; j++)
                {
                    var id = 0;
                    id = rand.Next(nparts);
                    if (j != 0)
                    {
                        while (Array.Exists(p.neighboursIDs, _ => _ == id)) 
                        {
                            id = rand.Next(nparts);
                        }
                    }
                    p.neighboursIDs[j] = id;
                }
                parts[i] = p;
            }

            //optimize
            for (int it = 0; it < niter; it++)
            {
                Console.WriteLine("---- iter n "+it + " FIT: "+fitbest);
                for (int i = 0; i < nparts; i++)
                {
                    double sum = 0;
                    for (int j = 0; j < ndim; j++)
                    {
                        r1 = c1 * rand.NextDouble();
                        r2 = c2 * rand.NextDouble();

                        //update vel 
                        parts[i].V[j] = c0 * parts[i].V[j] +
                        r1 * (parts[i].PersonalBest[j] - parts[i].X[j]) + 
                        r2 * (parts[i].LocalBest[j] - parts[i].X[j]); 
                        //update x
                        parts[i].X[j] += parts[i].V[j];

                        if(parts[i].X[j] < xmin)
                        {
                            parts[i].X[j] = xmin;
                            parts[i].V[j] = - parts[i].V[j];
                        } else if (parts[i].X[j] > xmax)
                        {
                            parts[i].X[j] = xmax;
                            parts[i].V[j] = - parts[i].V[j];
                        }

                        if (j == ndim-1 && (sum + parts[i].X[j] > 100))
                            parts[i].X[j] = 100 - sum;
                        sum += parts[i].X[j];

                        /*if(j == ndim-1) 
                        {
                            double sum = 0; 
                            for (int w = 0; w < ndim-1; w++)
                            {
                                sum += parts[i].X[w];
                            }
                            //p.X[j] = 100-sum;
                            double rest = 100 - sum;
                            if (parts[i].X[j] > rest)
                                parts[i].X[j] = rest;
                            else if (rest > parts[i].X[j])
                            {
                                //check which index has the best percentage and max it
                                double bestVal = 0;
                                int bestId = 0;
                                for (int id = 0; id < parts[i].X.Length; id++)
                                {
                                    if (parts[i].X[id] > bestVal)
                                    {
                                        bestVal = parts[i].X[id];
                                        bestId = id;
                                    }                                
                                }
                                parts[i].X[bestId] += (rest-parts[i].X[j]);
                            }
                        }*/
                    }
                    double rest = 100 - sum;
                    if (rest > 0) 
                    {
                        for (int j = 0; j < ndim; j++)
                        {
                            parts[i].X[j] += rest/ndim;
                        }
                    } 
                    parts[i].Fit = portfolioFitness(parts[i].X);

                    //qua
                    if (parts[i].Fit > parts[i].FitBest)
                    {
                        parts[i].FitBest = parts[i].Fit;
                        for (int j = 0; j < ndim; j++)
                        {
                            parts[i].PersonalBest[j] = parts[i].X[j];
                        }
                        parts[i].FitLocalBest = double.MaxValue;
                        for (int j = 0; j < nhoodSize; j++)
                        {
                            if (parts[parts[i].neighboursIDs[j]].Fit > parts[i].FitBest)
                            {
                                parts[i].FitLocalBest = parts[parts[i].neighboursIDs[j]].Fit;
                                for (int k = 0; k < ndim; k++)
                                    parts[i].LocalBest[k] = parts[parts[i].neighboursIDs[j]].X[k];
                            }
                        }
                        if (parts[i].Fit > fitbest)
                        {
                            fitbest = parts[i].Fit;
                            xbest.Clear();
                            for (int j = 0; j < ndim; j++)
                                xbest.Add(parts[i].X[j]);
                        }
                    }
                }
            }
            return fitbest;
        }
    }
}