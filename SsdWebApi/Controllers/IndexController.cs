using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SsdWebApi.Models;
namespace SsdWebApi.Controllers
{
    [ApiController]
    [Route ("api/[controller]")]
    public class IndexController : ControllerBase
    {
        private readonly IndexContext _context;
        Persistence p;
        Forecast forecast;

        public IndexController(IndexContext context)
        {
            _context = context;
            p = new Persistence(context);
            forecast = new Forecast();
        }

        [HttpGet]
        public ActionResult<List<Index>> GetAll () =>
         _context.indici.ToList ();

        [HttpGet("{id}", Name = "GetSerie")]
        public string GetSerie(int id)
        {
            return p.readByIndex(id);

        }

        //[HttpGet("-1", Name = "Predict")]
        // POST action
        // POST: api/index/Predict
        [HttpPost]
        [Route ("[action]")]
        public string Predict()
        {
            string res = "ok";
            forecast.createPortfolio();
            return res;
        }

        // POST action
        // POST: api/index/OptimizePortfolio
        [HttpPost]
        [Route ("[action]")]
        public string OptimizePortfolio()
        {
            Dictionary<string, double> d = forecast.Optimize();
            string s = string.Join("@", d.Select(x => x.Key + " = " + x.Value).ToArray());
            s = s.Replace("@", System.Environment.NewLine);
            return s;
        }

    }
}