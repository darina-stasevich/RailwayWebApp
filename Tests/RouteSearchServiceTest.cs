using System.Collections;
using Moq;
using RailwayApp.Application.Services;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.Initializers;
using RailwayApp.Domain.Interfaces.IRepositories;

namespace Tests;

[TestFixture]
public class RouteSearchServiceTest
{
    private Mock<IStationRepository> _mockStationRepository;
    private Mock<IAbstractRouteSegmentRepository> _mockAbstractRouteSegmentRepository;
    private Mock<IConcreteRouteSegmentRepository> _mockConcreteRouteSegmentRepository;
    private Mock<IAbstractRouteRepository> _mockAbstractRouteRepository;
    private Mock<IConcreteRouteRepository> _mockConcreteRouteRepository;
    private Mock<ICarriageAvailabilityRepository> _mockCarriageAvailabilityRepository;
    private Mock<ITrainRepository> _mockTrainRepository;
    private Mock<ICarriageTemplateRepository> _mockCarriageTemplateRepository;
    
    private RouteSearchService _routeSearchService;
    
    private PriceCalculationService _mockPriceCalculationService;
    private CarriageTemplateService _mockCarriageTemplateService;
    private CarriageSeatService _mockCarriageSeatService;
    private TrainCarriageInitializer _trainCarriageInitializer = new TrainCarriageInitializer();

    private TestDataContainer _testData;

    [SetUp]
    public void Setup()
    {
        _testData = GenerateTestData();

        _mockStationRepository = new Mock<IStationRepository>();
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

        _routeSearchService = new RouteSearchService(_mockStationRepository.Object,
            _mockAbstractRouteSegmentRepository.Object, _mockConcreteRouteSegmentRepository.Object,
            _mockPriceCalculationService, _mockCarriageSeatService);
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
        container.BrestHomelComplexId = abstractRoutes[4].Id;
        container.HomelBrestComplexId = abstractRoutes[5].Id;

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
        var random = new Random();

        var firstDayConcreteRouteIds = container.ConcreteRoutes
            .Where(cr => cr.RouteDepartureDate.Date == date)
            .Select(cr => cr.Id)
            .ToHashSet();

        var firstDayConcreteSegments = container.ConcreteRouteSegments
            .Where(cs => firstDayConcreteRouteIds.Contains(cs.ConcreteRouteId));
        
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
                    var initialValues = new bool[template.TotalSeats];
                    for (var i = 0; i < template.TotalSeats; i++)
                        initialValues[i] = random.Next(0, 2) == 1;
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
        }

        return container;
    }

    private void ConfigureMocks()
    {
        // --- Настройка Station Repository ---
        foreach (var station in _testData.Stations)
            _mockStationRepository.Setup(repo => repo.GetByIdAsync(station.Id))
                .ReturnsAsync(station);

        _mockStationRepository.Setup(repo => repo.GetByIdAsync(It.IsNotIn(_testData.Stations.Select(s => s.Id))))
            .ReturnsAsync((Station)null);

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
    public async Task GetRouteSearchResultAsync_ValidInput_DirectRoute_ReturnsRouteSearchResult()
    {
        var fromStationId = _testData.BrestId;
        var toStationId = _testData.MinskId;
        var departureDate = DateTime.Now.AddDays(2);

        var result = await _routeSearchService.GetRoutesAsync(fromStationId, toStationId, departureDate, true);

        Assert.That(result.Count, Is.EqualTo(2), "Ожидалось найти ровно 2 маршрута.");
        var result1 = result[0];
        Assert.That(result1.TotalDuration, Is.EqualTo(TimeSpan.Zero.Add(TimeSpan.FromHours(3))),
            "ожидаемое время поездки три часа");
        Assert.That(result1.DirectRoutes.Count, Is.EqualTo(1), "Должен был быть найден маршрут без пересадки");
        Assert.That(result1.MaximumTotalCost, Is.EqualTo(50m), "Стоимость маршрута должна быть 50");

        var result2 = result[1];
        Assert.That(result2.TotalDuration, Is.EqualTo(TimeSpan.Zero.Add(TimeSpan.FromMinutes(118))),
            "ожидаемое время поездки 118 минут");
        Assert.That(result2.DirectRoutes.Count, Is.EqualTo(1), "Должен был быть найден маршрут без пересадки");
        Assert.That(result2.MinimalTotalCost, Is.EqualTo(38.85m), "Стоимость маршрута должна быть 38.85");
    }

    [Test]
    public async Task GetRouteSearchResultAsync_ValidInput_DirectRoute_NotFullRoute_ReturnsRouteSearchResult()
    {
        var fromStationId = _testData.ZhabinkaId;
        var toStationId = _testData.BaranovichiId;
        var departureDate = DateTime.Now.AddDays(2);

        var result = await _routeSearchService.GetRoutesAsync(fromStationId, toStationId, departureDate, true);

        Assert.That(result.Count, Is.EqualTo(1), "Ожидалось найти ровно 1 маршрут.");
        var result1 = result[0];
        Assert.That(result1.TotalDuration, Is.EqualTo(TimeSpan.Zero.Add(TimeSpan.FromMinutes(38))),
            "ожидаемое время поездки 38 минут");
        Assert.That(result1.DirectRoutes.Count, Is.EqualTo(1), "Должен был быть найден маршрут без пересадки");
        Assert.That(result1.MinimalTotalCost, Is.EqualTo(31.5m), "Стоимость маршрута должна быть 31.5");
    }
}

internal class TestDataContainer
{
    public List<Station> Stations { get; set; } = new();
    public List<AbstractRoute> AbstractRoutes { get; set; } = new();
    public List<AbstractRouteSegment> AbstractRouteSegments { get; set; } = new();
    public List<ConcreteRoute> ConcreteRoutes { get; set; } = new();
    public List<ConcreteRouteSegment> ConcreteRouteSegments { get; set; } = new();
    public List<CarriageAvailability> CarriageAvailabilities { get; set; } = new();
    
    public List<TrainType> TrainTypes { get; set; } = new();
    
    public List<Train> Trains { get; set; } = new();
    
    public List<CarriageTemplate> CarriageTemplates { get; set; } = new();
    
    public Dictionary<Guid, List<CarriageTemplate>> TemplatesByTrainTypeId { get; set; } = new();

    public Guid BrestId { get; set; }
    public Guid ZhabinkaId { get; set; }
    public Guid BaranovichiId { get; set; }
    public Guid LuninetsId { get; set; }
    public Guid MinskId { get; set; }
    public Guid HomelId { get; set; }

    public Guid AbstractRouteBrestMinskDirectId { get; set; }
    public Guid AbstractRouteBrestMinskComplexId { get; set; }
    public Guid AbstractRouteMinskBrestDirectId { get; set; }
    public Guid AbstractRouteMinskBrestComplexId { get; set; }
    public Guid BrestHomelComplexId { get; set; }
    public Guid HomelBrestComplexId { get; set; }
}