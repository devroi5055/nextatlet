using AutoFixture.Kernel;
using NextAtlet.Domain.ValueObjects.Sections;

namespace NextAtlet.Application.Tests.Shared
{
    public class SectionDataSpecimentBuilder : ISpecimenBuilder
    {
        private readonly Random _random = new(); 

        public object Create(object request, ISpecimenContext context)
        {
            if (request is Type t && t == typeof(SectionData))
            {
                return _random.Next(2) == 0
                    ? new HeroSectionData()
                    : new BioSectionData(); 
            }
            return new NoSpecimen();
        }
    }
}
