using CarAdvisor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarAdvisor.DataAccess.Contexts;

public class CarAdvisorContext:DbContext
{
    public DbSet<Car> Cars { get; set; }
    public DbSet<CarGeneration> CarGenerations { get; set; }

    public CarAdvisorContext(DbContextOptions<CarAdvisorContext> options) : base(options)
    {
    }
    public CarAdvisorContext()
    {
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(
                "Server=DESKTOP-I5L5LLB;Database=CarAdvisor;Integrated Security=True;TrustServerCertificate=True;",
                o => o.EnableRetryOnFailure()
            );
        }
    }

}
