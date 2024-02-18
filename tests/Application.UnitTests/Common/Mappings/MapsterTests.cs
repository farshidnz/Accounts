using FluentAssertions;
using Mapster;
using Accounts.Application.Common.Mappings;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Accounts.Application.UnitTests.Common.Mappings
{
    public class SourceDomainModel
    {
        public SourceDomainModel(string firstName, string surname)
        {
            FirstName = firstName;
            Surname = surname;
        }

        public string FirstName { get; }
        public string Surname { get; }
    }

    public class TargetViewModel : IMapFrom<SourceDomainModel>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }

        public void Mapping(TypeAdapterConfig config)
        {
            config.ForType<SourceDomainModel, TargetViewModel>()
                .Map(dest => dest.LastName, src => src.Surname)
                .Map(dest => dest.FullName, src => $"{src.FirstName} {src.Surname}");
        }
    }

    public class SourceViewModel : IMapTo<TargetViewModel>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }

        public void Mapping(TypeAdapterConfig config)
        {
            config.ForType<SourceDomainModel, TargetViewModel>()
                .Map(dest => dest.LastName, src => src.Surname)
                .Map(dest => dest.FullName, src => $"{src.FirstName} {src.Surname}");
        }
    }



    [TestFixture]
    public class MapsterTests
    {
        public MapsterTests()
        {
            TypeAdapterConfig.GlobalSettings.Apply(new MappingProfile(Assembly.GetExecutingAssembly()));
        }

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ShouldMapDomainToViewModel()
        {
            var src = new SourceDomainModel("Homer", "Simpson");

            var dst = src.Adapt<TargetViewModel>();

            dst.FirstName.Should().Be(src.FirstName);
            dst.LastName.Should().Be(src.Surname);
            dst.FullName.Should().Be($"{src.FirstName} {src.Surname}");
        }

        [Test]
        public void ShouldMap_SourceViewModel_To_TargetViewModel()
        {
            var src = new SourceViewModel
            {
                FirstName = "Leo",
                FullName = "Leo Messi"
            };
            var dst = src.Adapt<TargetViewModel>();

            dst.FirstName.Should().Be(src.FirstName);            
            dst.FullName.Should().Be($"{src.FullName}");
        }
    }
}