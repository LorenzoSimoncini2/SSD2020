namespace SsdWebApi.Models 
{
    public class PSOParticle
    {
        public PSOParticle()
        {}

        public double[] X {get; set;} //position

        public double[] PersonalBest {get; set;}

        public double[] LocalBest {get; set;}

        public double[] V {get;set;} //velocity

        public double Fit{get; set;}

        public double FitBest{get; set;}

        public double FitLocalBest{get; set;}

        public int[] neighboursIDs {get; set;}

    }
}