using System.Collections;
using System.Runtime.Serialization;
using Moq;
using RailwayApp.Application.Models;
using RailwayApp.Application.Services;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.Initializers;
using RailwayApp.Domain.Interfaces.IRepositories;

namespace Tests;

[TestFixture]
public class CarriageServiceTest
{
    private Mock<IAbstractRouteSegmentRepository> _mockAbstractRouteSegmentRepository;
    private Mock<IConcreteRouteSegmentRepository> _mockConcreteRouteSegmentRepository;
    private Mock<IAbstractRouteRepository> _mockAbstractRouteRepository;
    private Mock<IConcreteRouteRepository> _mockConcreteRouteRepository;
    private Mock<ICarriageAvailabilityRepository> _mockCarriageAvailabilityRepository;
    private Mock<ITrainRepository> _mockTrainRepository;
    private Mock<ICarriageTemplateRepository> _mockCarriageTemplateRepository;
    
    private CarriageService _carriageService;
    
    private PriceCalculationService _mockPriceCalculationService;
    private CarriageTemplateService _mockCarriageTemplateService;
    private CarriageSeatService _mockCarriageSeatService;
    private readonly TrainCarriageInitializer _trainCarriageInitializer = new TrainCarriageInitializer();

    private TestDataContainer _testData;

    [SetUp]
    public void Setup()
    {
        _testData = GenerateTestData();

        _mockAbstractRouteSegmentRepository = new Mock<IAbstractRouteSegmentRepository>();
        _mockConcreteRouteSegmentRepository = new Mock<IConcreteRouteSegmentRepository>();
        _mockAbstractRouteRepository = new Mock<IAbstractRouteRepository>();
        _mockConcreteRouteRepository = new Mock<IConcreteRouteRepository>();
        _mockCarriageAvailabilityRepository = new Mock<ICarriageAvailabilityRepository>();
        _mockTrainRepository = new Mock<ITrainRepository>();
        _mockCarriageTemplateRepository = new Mock<ICarriageTemplateRepository>();

        _mockCarriageSeatService = new CarriageSeatService(_mockConcreteRouteRepository.Object, _mockAbstractRouteRepository.Object,
            _mockAbstractRouteSegmentRepository.Object, _mockConcreteRouteSegmentRepository.Object,
            _mockCarriageAvailabilityRepository.Object);
        
        _mockCarriageTemplateService = new CarriageTemplateService(_mockConcreteRouteRepository.Object,
            _mockAbstractRouteRepository.Object, _mockTrainRepository.Object, _mockCarriageTemplateRepository.Object);

        _mockPriceCalculationService = new PriceCalculationService(_mockConcreteRouteRepository.Object,
            _mockAbstractRouteRepository.Object, _mockAbstractRouteSegmentRepository.Object,
            _mockCarriageTemplateRepository.Object, _mockCarriageTemplateService);
        
        ConfigureMocks();

        _carriageService = new CarriageService(_mockCarriageSeatService, _mockPriceCalculationService,
            _mockCarriageTemplateService);
    }

    private TestDataContainer GenerateTestData()
    {
        var container = new TestDataContainer();
        var date = DateTime.Now.AddDays(2).Date;

        container.TrainTypes = _trainCarriageInitializer.TrainTypes;
        container.CarriageTemplates = _trainCarriageInitializer.CarriageTemplates;
        var templatesByTrainTypeId = container.CarriageTemplates
            .GroupBy(t => t.TrainTypeId)
            .ToDictionary(g => g.Key, g => g.ToList());
        container.TemplatesByTrainTypeId = templatesByTrainTypeId;
        
        var stations = new List<Station>
        {
            new() { Id = Guid.NewGuid(), Name = "Брест", Region = "Брестская" },
            new() { Id = Guid.NewGuid(), Name = "Жабинка", Region = "Брестская" },
            new() { Id = Guid.NewGuid(), Name = "Барановичи", Region = "Брестская" },
            new() { Id = Guid.NewGuid(), Name = "Лунинец", Region = "Брестская" },
            new() { Id = Guid.NewGuid(), Name = "Минск", Region = "Минск" },
            new() { Id = Guid.NewGuid(), Name = "Гомель", Region = "Гомельская" }
        };
        container.Stations.AddRange(stations);

        container.BrestId = stations[0].Id;
        container.ZhabinkaId = stations[1].Id;
        container.BaranovichiId = stations[2].Id;
        container.LuninetsId = stations[3].Id;
        container.MinskId = stations[4].Id;
        container.HomelId = stations[5].Id;
        
        Guid trainTypeTr1Id = container.TrainTypes.Count > 8 ? container.TrainTypes[8].Id : Guid.NewGuid();
        Guid trainTypeTr2Id = container.TrainTypes.Count > 9 ? container.TrainTypes[9].Id : Guid.NewGuid();
        Guid trainTypeTr3Id = container.TrainTypes.Count > 10 ? container.TrainTypes[10].Id : Guid.NewGuid();

        container.Trains = new List<Train>
        {
            new Train { Number = "TR1", TrainTypeId = trainTypeTr1Id },
            new Train { Number = "TR2", TrainTypeId = trainTypeTr2Id },
            new Train { Number = "TR3", TrainTypeId = trainTypeTr3Id }
        };

        var abstractRoutes = new List<AbstractRoute>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TrainNumber = "TR1",
                ActiveDays = "MTWHFSN",
                TransferCost = 20,
                HasBeddingOption = false,
                DepartureTime = TimeSpan.Zero.Add(TimeSpan.FromHours(15))
            },
            new()
            {
                Id = Guid.NewGuid(),
                TrainNumber = "TR2",
                ActiveDays = "MTWHFSN",
                TransferCost = 15,
                HasBeddingOption = true,
                DepartureTime = TimeSpan.Zero.Add(TimeSpan.FromHours(9))
            },
            new()
            {
                Id = Guid.NewGuid(),
                TrainNumber = "TR1",
                ActiveDays = "MTWHFSN",
                TransferCost = 18,
                HasBeddingOption = false,
                DepartureTime = TimeSpan.Zero.Add(TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(22)))
            },
            new()
            {
                Id = Guid.NewGuid(),
                TrainNumber = "TR2",
                ActiveDays = "MTWHFSN",
                TransferCost = 20,
                HasBeddingOption = true,
                DepartureTime = TimeSpan.Zero.Add(TimeSpan.FromHours(17).Add(TimeSpan.FromMinutes(05)))
            },
            new()
            {
                Id = Guid.NewGuid(),
                TrainNumber = "TR3",
                ActiveDays = "MTWHFSN",
                TransferCost = 18,
                HasBeddingOption = true,
                DepartureTime = TimeSpan.Zero.Add(TimeSpan.FromHours(18).Add(TimeSpan.FromMinutes(22)))
            },
            new()
            {
                Id = Guid.NewGuid(),
                TrainNumber = "TR3",
                ActiveDays = "MTWHFSN",
                TransferCost = 20,
                HasBeddingOption = true,
                DepartureTime = TimeSpan.Zero.Add(TimeSpan.FromHours(18).Add(TimeSpan.FromMinutes(22)))
            }
        };

        container.AbstractRoutes.AddRange(abstractRoutes);
        container.AbstractRouteBrestMinskDirectId = abstractRoutes[0].Id;
        container.AbstractRouteBrestMinskComplexId = abstractRoutes[2].Id;
        container.AbstractRouteMinskBrestDirectId = abstractRoutes[1].Id;
        container.AbstractRouteMinskBrestComplexId = abstractRoutes[3].Id;
        container.AbstractRouteBrestHomelComplexId = abstractRoutes[4].Id;
        container.AbstractRouteHomelBrestComplexId = abstractRoutes[5].Id;

        var abstractRouteSegments = new List<AbstractRouteSegment>
        {
            new()
            {
                AbstractRouteId = abstractRoutes[0].Id,
                SegmentNumber = 1,
                FromStationId = stations[0].Id,
                ToStationId = stations[4].Id,
                FromTime = TimeSpan.Zero.Add(TimeSpan.FromHours(15)),
                ToTime = TimeSpan.Zero.Add(TimeSpan.FromHours(18)),
                SegmentCost = 5
            },
            new()
            {
                AbstractRouteId = abstractRoutes[1].Id,
                SegmentNumber = 1,
                FromStationId = stations[4].Id,
                ToStationId = stations[0].Id,
                FromTime = TimeSpan.Zero.Add(TimeSpan.FromHours(9)),
                ToTime = TimeSpan.Zero.Add(TimeSpan.FromHours(12)),
                SegmentCost = 5
            },
            new()
            {
                AbstractRouteId = abstractRoutes[2].Id,
                SegmentNumber = 1,
                FromStationId = stations[0].Id,
                ToStationId = stations[1].Id,
                FromTime = TimeSpan.Zero.Add(TimeSpan.FromHours(8)).Add(TimeSpan.FromMinutes(22)),
                ToTime = TimeSpan.Zero.Add(TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(40))),
                SegmentCost = (decimal)2.5
            },
            new()
            {
                AbstractRouteId = abstractRoutes[2].Id,
                SegmentNumber = 2,
                FromStationId = stations[1].Id,
                ToStationId = stations[2].Id,
                FromTime = TimeSpan.Zero.Add(TimeSpan.FromHours(8)).Add(TimeSpan.FromMinutes(42)),
                ToTime = TimeSpan.Zero.Add(TimeSpan.FromHours(9).Add(TimeSpan.FromMinutes(20))),
                SegmentCost = 3
            },
            new()
            {
                AbstractRouteId = abstractRoutes[2].Id,
                SegmentNumber = 3,
                FromStationId = stations[2].Id,
                ToStationId = stations[4].Id,
                FromTime = TimeSpan.Zero.Add(TimeSpan.FromHours(9)).Add(TimeSpan.FromMinutes(30)),
                ToTime = TimeSpan.Zero.Add(TimeSpan.FromHours(10).Add(TimeSpan.FromMinutes(20))),
                SegmentCost = 2.4m
            },
            new()
            {
                AbstractRouteId = abstractRoutes[3].Id,
                SegmentNumber = 3,
                FromStationId = stations[1].Id,
                ToStationId = stations[0].Id,
                FromTime = TimeSpan.Zero.Add(TimeSpan.FromHours(18)).Add(TimeSpan.FromMinutes(22)),
                ToTime = TimeSpan.Zero.Add(TimeSpan.FromHours(18).Add(TimeSpan.FromMinutes(40))),
                SegmentCost = (decimal)2.5
            },
            new()
            {
                AbstractRouteId = abstractRoutes[3].Id,
                SegmentNumber = 2,
                FromStationId = stations[2].Id,
                ToStationId = stations[1].Id,
                FromTime = TimeSpan.Zero.Add(TimeSpan.FromHours(17)).Add(TimeSpan.FromMinutes(42)),
                ToTime = TimeSpan.Zero.Add(TimeSpan.FromHours(18).Add(TimeSpan.FromMinutes(20))),
                SegmentCost = 3
            },
            new()
            {
                AbstractRouteId = abstractRoutes[3].Id,
                SegmentNumber = 1,
                FromStationId = stations[4].Id,
                ToStationId = stations[2].Id,
                FromTime = TimeSpan.Zero.Add(TimeSpan.FromHours(17)).Add(TimeSpan.FromMinutes(05)),
                ToTime = TimeSpan.Zero.Add(TimeSpan.FromHours(17).Add(TimeSpan.FromMinutes(40))),
                SegmentCost = 2.4m
            },

            new()
            {
                AbstractRouteId = abstractRoutes[4].Id,
                SegmentNumber = 1,
                FromStationId = stations[0].Id,
                ToStationId = stations[1].Id,
                FromTime = TimeSpan.Zero.Add(TimeSpan.FromHours(18)).Add(TimeSpan.FromMinutes(22)),
                ToTime = TimeSpan.Zero.Add(TimeSpan.FromHours(18).Add(TimeSpan.FromMinutes(40))),
                SegmentCost = (decimal)2.5
            },
            new()
            {
                AbstractRouteId = abstractRoutes[4].Id,
                SegmentNumber = 2,
                FromStationId = stations[1].Id,
                ToStationId = stations[3].Id,
                FromTime = TimeSpan.Zero.Add(TimeSpan.FromHours(18)).Add(TimeSpan.FromMinutes(42)),
                ToTime = TimeSpan.Zero.Add(TimeSpan.FromHours(21).Add(TimeSpan.FromMinutes(20))),
                SegmentCost = 3
            },
            new()
            {
                AbstractRouteId = abstractRoutes[4].Id,
                SegmentNumber = 3,
                FromStationId = stations[3].Id,
                ToStationId = stations[5].Id,
                FromTime = TimeSpan.Zero.Add(TimeSpan.FromHours(21)).Add(TimeSpan.FromMinutes(30)),
                ToTime = TimeSpan.Zero.Add(TimeSpan.FromHours(30).Add(TimeSpan.FromMinutes(20))),
                SegmentCost = 2.4m
            },
            new()
            {
                AbstractRouteId = abstractRoutes[5].Id,
                SegmentNumber = 3,
                FromStationId = stations[1].Id,
                ToStationId = stations[0].Id,
                FromTime = TimeSpan.Zero.Add(TimeSpan.FromHours(18)).Add(TimeSpan.FromMinutes(22)),
                ToTime = TimeSpan.Zero.Add(TimeSpan.FromHours(30).Add(TimeSpan.FromMinutes(40))),
                SegmentCost = (decimal)2.5
            },
            new()
            {
                AbstractRouteId = abstractRoutes[5].Id,
                SegmentNumber = 2,
                FromStationId = stations[3].Id,
                ToStationId = stations[1].Id,
                FromTime = TimeSpan.Zero.Add(TimeSpan.FromHours(17)).Add(TimeSpan.FromMinutes(42)),
                ToTime = TimeSpan.Zero.Add(TimeSpan.FromHours(18).Add(TimeSpan.FromMinutes(20))),
                SegmentCost = 3
            },
            new()
            {
                AbstractRouteId = abstractRoutes[5].Id,
                SegmentNumber = 1,
                FromStationId = stations[5].Id,
                ToStationId = stations[3].Id,
                FromTime = TimeSpan.Zero.Add(TimeSpan.FromHours(17)).Add(TimeSpan.FromMinutes(05)),
                ToTime = TimeSpan.Zero.Add(TimeSpan.FromHours(17).Add(TimeSpan.FromMinutes(40))),
                SegmentCost = 2.4m
            }
        };
        container.AbstractRouteSegments.AddRange(abstractRouteSegments);

        var concreteRoutes = new List<ConcreteRoute>();
        for (var i = 0; i < 7; i++)
        {
            var newDate = date.AddDays(i);
            foreach (var abstractRoute in abstractRoutes)
            {
                var route = new ConcreteRoute
                {
                    AbstractRouteId = abstractRoute.Id,
                    RouteDepartureDate = newDate.Add(abstractRoute.DepartureTime)
                };
                concreteRoutes.Add(route);
            }
        }
        
        container.ConcreteRouteBrestMinskDirectId = concreteRoutes[0].Id;
        container.ConcreteRouteBrestMinskComplexId = concreteRoutes[2].Id;
        container.ConcreteRouteMinskBrestDirectId = concreteRoutes[1].Id;
        container.ConcreteRouteMinskBrestComplexId = concreteRoutes[3].Id;
        container.ConcreteRouteBrestHomelComplexId = concreteRoutes[4].Id;
        container.ConcreteRouteHomelBrestComplexId = concreteRoutes[5].Id;

        container.ConcreteRoutes.AddRange(concreteRoutes);

        var concreteRouteSegments = new List<ConcreteRouteSegment>();
        for (var i = 0; i < 7; i++)
        {
            var newDate = date.AddDays(i);
            for (var j = 0; j < abstractRouteSegments.Count; j++)
            {
                var abstractRouteSegment = abstractRouteSegments[j];
                var concreteRouteIndex = i * 6;
                if (j % 14 == 0)
                    concreteRouteIndex += 0;
                else if (j % 14 == 1)
                    concreteRouteIndex += 1;
                else if (j % 14 <= 4)
                    concreteRouteIndex += 2;
                else if (j % 14 <= 7)
                    concreteRouteIndex += 3;
                else if (j % 14 <= 10)
                    concreteRouteIndex += 4;
                else if (j % 14 <= 13)
                    concreteRouteIndex += 5;
                else
                    concreteRouteIndex = 6;
                var routeSegment = new ConcreteRouteSegment
                {
                    AbstractSegmentId = abstractRouteSegment.Id,
                    ConcreteRouteId = concreteRoutes[concreteRouteIndex].Id,
                    ConcreteDepartureDate = newDate.Add(abstractRouteSegment.FromTime),
                    ConcreteArrivalDate = newDate.Add(abstractRouteSegment.ToTime)
                };
                concreteRouteSegments.Add(routeSegment);
            }
        }

        container.ConcreteRouteSegments.AddRange(concreteRouteSegments);

        container.CarriageAvailabilities = new List<CarriageAvailability>();
        
        var firstDayConcreteRouteIds = container.ConcreteRoutes
            .Where(cr => cr.RouteDepartureDate.Date == date)
            .Select(cr => cr.Id)
            .ToHashSet();

        var firstDayConcreteSegments = container.ConcreteRouteSegments
            .Where(cs => firstDayConcreteRouteIds.Contains(cs.ConcreteRouteId));
        
        int idx = 0;

        foreach (var concreteSegment in firstDayConcreteSegments)
        {
            var concreteRoute = container.ConcreteRoutes.First(cr => cr.Id == concreteSegment.ConcreteRouteId);
            var abstractRoute = container.AbstractRoutes.First(ar => ar.Id == concreteRoute.AbstractRouteId);

            Guid currentTrainTypeId;
            switch (abstractRoute.TrainNumber)
            {
                case "TR1": currentTrainTypeId = trainTypeTr1Id; break;
                case "TR2": currentTrainTypeId = trainTypeTr2Id; break;
                case "TR3": currentTrainTypeId = trainTypeTr3Id; break;
                default: continue;
            }
            
            if (templatesByTrainTypeId.TryGetValue(currentTrainTypeId, out var relevantTemplates))
            {
                foreach (var template in relevantTemplates)
                {
                    bool isSegmentEven = idx % 2 == 0;
                    var initialValues = new bool[template.TotalSeats];
                    if (isSegmentEven)
                    {
                        for (int i = 0; i < template.TotalSeats; i++)
                        {
                            initialValues[i] = i % 2 == 0;
                        }
                    }
                    else
                    {
                        int half = (template.TotalSeats + 1) / 2;
                        for (int i = 0; i < template.TotalSeats; i++)
                        {
                            initialValues[i] = i < half;
                        }
                    }

                    var occupiedSeatsArray = new BitArray(initialValues);

                    var carriageAvailability = new CarriageAvailability
                    {
                        Id = Guid.NewGuid(),
                        ConcreteRouteSegmentId = concreteSegment.Id,
                        CarriageTemplateId = template.Id,
                        OccupiedSeats = occupiedSeatsArray
                    };
                    container.CarriageAvailabilities.Add(carriageAvailability);
                }
            }

            idx++;
        }

        return container;
    }

    private void ConfigureMocks()
    {
        // --- Настройка Abstract Route Segment Repository ---
        _mockAbstractRouteSegmentRepository
            .Setup(repo => repo.GetAbstractSegmentsByFromStationAsync(It.IsAny<Guid>())) 
            .ReturnsAsync((Guid fromStationId) =>
            {
                var matchingSegments = _testData.AbstractRouteSegments
                    .Where(segment => segment.FromStationId == fromStationId)
                    .ToList();
                return matchingSegments;
            });

        _mockAbstractRouteSegmentRepository
            .Setup(repo => repo.GetAbstractSegmentsByToStationAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid toStationId) =>
            {
                var matchingSegments = _testData.AbstractRouteSegments
                    .Where(segment => segment.ToStationId == toStationId)
                    .ToList();
                return matchingSegments;
            });

        _mockAbstractRouteSegmentRepository
            .Setup(repo => repo.GetAbstractSegmentsByRouteIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid routeId) =>
            {
                var matchingSegments = _testData.AbstractRouteSegments
                    .Where(segment => segment.AbstractRouteId == routeId)
                    .ToList();
                return matchingSegments;
            });

        // --- Настройка Abstract Route Repository ---
        _mockAbstractRouteRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid routeId) =>
            {
                var matchingRoute = _testData.AbstractRoutes
                    .Where(r => r.Id == routeId);
                return matchingRoute.FirstOrDefault();
            });

        // --- Настройка Concrete Route Repository ---
        _mockConcreteRouteRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid routeId) =>
            {
                var matchingRouteSegments = _testData.ConcreteRoutes
                    .Where(r => r.Id == routeId);
                return matchingRouteSegments.FirstOrDefault();
            });

        // --- Настройка Concrete Route Segment Repository ---
        _mockConcreteRouteSegmentRepository
            .Setup(repo => repo.GetConcreteSegmentByAbstractSegmentIdAsync(It.IsAny<Guid>(), It.IsAny<DateTime>()))
            .ReturnsAsync((Guid abstractSegmentId, DateTime departureDate) =>
            {
                var matchingSegment = _testData.ConcreteRouteSegments
                    .FirstOrDefault(cs =>
                        cs.AbstractSegmentId == abstractSegmentId &&
                        cs.ConcreteDepartureDate.Date == departureDate.Date);
                return matchingSegment;
            });

        _mockConcreteRouteSegmentRepository
            .Setup(repo => repo.GetConcreteSegmentsByAbstractSegmentIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid abstractSegmentId) =>
            {
                var matchingSegments = _testData.ConcreteRouteSegments
                    .Where(cs => cs.AbstractSegmentId == abstractSegmentId)
                    .ToList();
                return matchingSegments;
            });

        _mockConcreteRouteSegmentRepository
            .Setup(repo => repo.GetConcreteSegmentsByConcreteRouteIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid concreteRouteId) => 
            {
                var matchingSegments = _testData.ConcreteRouteSegments
                    .Where(cs => cs.ConcreteRouteId == concreteRouteId)
                    .ToList();
                return matchingSegments;
            });

        // --- Настройка Carriage Availability Repository ---
        _mockCarriageAvailabilityRepository
            .Setup(repo => repo.GetByConcreteSegmentIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid segmentId) =>
            {
                var matchingCarriages = _testData.CarriageAvailabilities
                    .Where(c => c.ConcreteRouteSegmentId == segmentId)
                    .ToList();
                return matchingCarriages;
            });
        
        // --- Настройка Carriage Template Repository ---
        _mockCarriageTemplateRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => _testData.CarriageTemplates.FirstOrDefault(t => t.Id == id));
        _mockCarriageTemplateRepository.Setup(repo => repo.GetByTrainTypeIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid trainTypeId) => _testData.TemplatesByTrainTypeId.TryGetValue(trainTypeId, out var templates) ? templates : new List<CarriageTemplate>());


        // --- Настройка Train Repository ---
        _mockTrainRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => _testData.Trains.FirstOrDefault(t => t.Number == id));


    }

    [Test]
    public async Task GetAllCarriagesShorInfoFromRoute_ValidInput()
    {
        // one segment in path
        var concreteRouteId = _testData.ConcreteRouteBrestMinskDirectId;
        // train for route
        var trainId = _testData.Trains.FirstOrDefault(x => x.Number == "TR1")!.TrainTypeId;
        var trainTemplates = _testData.TemplatesByTrainTypeId[trainId].OrderBy(t => t.CarriageNumber).ToList();
        var result = await _carriageService.GetAllCarriagesInfo(concreteRouteId, 1, 1);
        Assert.That(result.Count, Is.EqualTo(8), "amount of carriages must be 8");
        int idx = 0;
        foreach (var shortInfo in result)
        {
            Assert.That(shortInfo.CarriageNumber, Is.EqualTo(++idx), "carriage number must be in increasing order");
            Assert.That(shortInfo.AvailableSeats, Is.EqualTo((trainTemplates[idx - 1].TotalSeats +1 ) / 2), $"invalid amount of free seats in carriage {idx}");
            Console.WriteLine(shortInfo.AvailableSeats);
        }
    }

    [Test]
    public async Task GetAllCarriagesDetailedInfoFromRoute_ValidInput()
    {
        var concreteRouteId = _testData.ConcreteRouteBrestMinskDirectId;
        // one segment in path

        var results = new List<DetailedCarriageInfoDto>();
        for (int i = 0; i < 8; i++)
        {
            var carriageResult = await _carriageService.GetCarriageInfo(concreteRouteId, 1, 1, i + 1);
            results.Add(carriageResult);
            Console.WriteLine($"carriage {i + 1} --- total seats = {carriageResult.TotalSeats}");
            foreach (var seat in carriageResult.AvailableSeats)
            {
                Console.WriteLine(seat);
            }

            if (i % 2 == 0)
            {
                var badSeats = carriageResult.AvailableSeats.Where(x => x > (carriageResult.TotalSeats + 1) / 2);
                Assert.That(badSeats.Count(), Is.EqualTo(0), $"bad seats found in carriage {i + 1}");
            } 
            else
            {
                var badSeats = carriageResult.AvailableSeats.Where(x => x % 2 == 0);
                Assert.That(badSeats.Count(), Is.EqualTo(0), $"bad seats found in carriage {i + 1}");
            }
        }
        
        Assert.That(results.Count, Is.EqualTo(8), "amount of carriages must be 8");
    }
    
    [Test]
    public async Task GetAllCarriagesShorInfoFromRoute_FewSegments_ValidInput()
    {
        // three segments in path
        var concreteRouteId = _testData.ConcreteRouteBrestHomelComplexId;
        // train for route
        var trainId = _testData.Trains.FirstOrDefault(x => x.Number == "TR3")!.TrainTypeId;
        var trainTemplates = _testData.TemplatesByTrainTypeId[trainId].OrderBy(t => t.CarriageNumber).ToList();
        var result = await _carriageService.GetAllCarriagesInfo(concreteRouteId, 1, 1);
        Assert.That(result.Count, Is.EqualTo(3), "amount of carriages must be 3");
        int idx = 0;
        foreach (var shortInfo in result)
        {
            Assert.That(shortInfo.CarriageNumber, Is.EqualTo(++idx), "carriage number must be in increasing order");
            Assert.That(shortInfo.AvailableSeats, Is.EqualTo((trainTemplates[idx - 1].TotalSeats +1 ) / 2), $"invalid amount of free seats in carriage {idx}");
            Console.WriteLine(shortInfo.AvailableSeats);
        }
    }

    [Test]
    public async Task GetAllCarriagesDetailedInfoFromRoute_FewSegments_ValidInput()
    {
        var concreteRouteId = _testData.ConcreteRouteBrestHomelComplexId;
        // three segments in path

        var results = new List<DetailedCarriageInfoDto>();
        for (int i = 0; i < 3; i++)
        {
            var carriageResult = await _carriageService.GetCarriageInfo(concreteRouteId, 1, 3, i + 1);
            results.Add(carriageResult);
            Console.WriteLine($"carriage {i + 1} --- total seats = {carriageResult.TotalSeats}");
            foreach (var seat in carriageResult.AvailableSeats)
            {
                Console.WriteLine(seat);
            }

            var badSeats = carriageResult.AvailableSeats.Where(x => x > (carriageResult.TotalSeats + 1) / 2);
            Assert.That(badSeats.Count(), Is.EqualTo(0), $"bad seats found in carriage {i + 1}"); 
            badSeats = carriageResult.AvailableSeats.Where(x => x % 2 == 0);
            Assert.That(badSeats.Count(), Is.EqualTo(0), $"bad seats found in carriage {i + 1}");
        }
        
        Assert.That(results.Count, Is.EqualTo(3), "amount of carriages must be 8");
    }

}
