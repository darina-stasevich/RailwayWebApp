using System.Collections;
using MongoDB.Driver;
using Moq;
using RailwayApp.Application.Models;
using RailwayApp.Application.Services;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.Initializers;
using RailwayApp.Domain.Interfaces.IRepositories;

namespace Tests;

[TestFixture]
public class RouteSearchServiceTest
{
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
        _mockSeatLockRepository = new Mock<ISeatLockRepository>();

        _mockCarriageSeatService = new CarriageSeatService(_mockConcreteRouteSegmentRepository.Object,
            _mockCarriageAvailabilityRepository.Object, _mockSeatLockRepository.Object);

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

    private Mock<IStationRepository> _mockStationRepository;
    private Mock<IAbstractRouteSegmentRepository> _mockAbstractRouteSegmentRepository;
    private Mock<IConcreteRouteSegmentRepository> _mockConcreteRouteSegmentRepository;
    private Mock<IAbstractRouteRepository> _mockAbstractRouteRepository;
    private Mock<IConcreteRouteRepository> _mockConcreteRouteRepository;
    private Mock<ICarriageAvailabilityRepository> _mockCarriageAvailabilityRepository;
    private Mock<ITrainRepository> _mockTrainRepository;
    private Mock<ICarriageTemplateRepository> _mockCarriageTemplateRepository;
    private Mock<ISeatLockRepository> _mockSeatLockRepository;

    private RouteSearchService _routeSearchService;

    private PriceCalculationService _mockPriceCalculationService;
    private CarriageTemplateService _mockCarriageTemplateService;
    private CarriageSeatService _mockCarriageSeatService;
    private readonly TrainCarriageInitializer _trainCarriageInitializer = new();

    private TestDataContainer _testData;

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

        var trainTypeTr1Id = container.TrainTypes.Count > 8 ? container.TrainTypes[8].Id : Guid.NewGuid();
        var trainTypeTr2Id = container.TrainTypes.Count > 9 ? container.TrainTypes[9].Id : Guid.NewGuid();
        var trainTypeTr3Id = container.TrainTypes.Count > 10 ? container.TrainTypes[10].Id : Guid.NewGuid();

        container.Trains = new List<Train>
        {
            new() { Id = "TR1", TrainTypeId = trainTypeTr1Id },
            new() { Id = "TR2", TrainTypeId = trainTypeTr2Id },
            new() { Id = "TR3", TrainTypeId = trainTypeTr3Id }
        };

        var abstractRoutes = new List<AbstractRoute>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TrainNumber = "TR1",
                ActiveDays = new List<DayOfWeek>
                {
                    DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
                    DayOfWeek.Friday, DayOfWeek.Saturday
                },

                TransferCost = 20,
                HasBeddingOption = false,
                DepartureTime = TimeSpan.FromHours(15)
            },
            new()
            {
                Id = Guid.NewGuid(),
                TrainNumber = "TR2",
                ActiveDays = new List<DayOfWeek>
                {
                    DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
                    DayOfWeek.Friday, DayOfWeek.Saturday
                },

                TransferCost = 15,
                HasBeddingOption = true,
                DepartureTime = TimeSpan.FromHours(9)
            },
            new()
            {
                Id = Guid.NewGuid(),
                TrainNumber = "TR1",
                ActiveDays = new List<DayOfWeek>
                {
                    DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
                    DayOfWeek.Friday, DayOfWeek.Saturday
                },

                TransferCost = 18,
                HasBeddingOption = false,
                DepartureTime = TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(22))
            },
            new()
            {
                Id = Guid.NewGuid(),
                TrainNumber = "TR2",
                ActiveDays = new List<DayOfWeek>
                {
                    DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
                    DayOfWeek.Friday, DayOfWeek.Saturday
                },

                TransferCost = 20,
                HasBeddingOption = true,
                DepartureTime = TimeSpan.FromHours(17).Add(TimeSpan.FromMinutes(05))
            },
            new()
            {
                Id = Guid.NewGuid(),
                TrainNumber = "TR3",
                ActiveDays = new List<DayOfWeek>
                {
                    DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
                    DayOfWeek.Friday, DayOfWeek.Saturday
                },

                TransferCost = 18,
                HasBeddingOption = true,
                DepartureTime = TimeSpan.FromHours(18).Add(TimeSpan.FromMinutes(22))
            },
            new()
            {
                Id = Guid.NewGuid(),
                TrainNumber = "TR3",
                ActiveDays = new List<DayOfWeek>
                {
                    DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
                    DayOfWeek.Friday, DayOfWeek.Saturday
                },

                TransferCost = 20,
                HasBeddingOption = true,
                DepartureTime = TimeSpan.FromHours(18).Add(TimeSpan.FromMinutes(22))
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
                FromStationId = stations[4].Id,
                ToStationId = stations[0].Id,
                FromTime = TimeSpan.FromHours(15),
                ToTime = TimeSpan.FromHours(18),
                SegmentCost = 5
            },
            new()
            {
                AbstractRouteId = abstractRoutes[1].Id,
                SegmentNumber = 1,
                FromStationId = stations[0].Id,
                ToStationId = stations[4].Id,
                FromTime = TimeSpan.FromHours(9),
                ToTime = TimeSpan.FromHours(12),
                SegmentCost = 5
            },
            new()
            {
                AbstractRouteId = abstractRoutes[2].Id,
                SegmentNumber = 1,
                FromStationId = stations[0].Id,
                ToStationId = stations[1].Id,
                FromTime = TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(22)),
                ToTime = TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(40)),
                SegmentCost = (decimal)2.5
            },
            new()
            {
                AbstractRouteId = abstractRoutes[2].Id,
                SegmentNumber = 2,
                FromStationId = stations[1].Id,
                ToStationId = stations[2].Id,
                FromTime = TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(42)),
                ToTime = TimeSpan.FromHours(9).Add(TimeSpan.FromMinutes(20)),
                SegmentCost = 3
            },
            new()
            {
                AbstractRouteId = abstractRoutes[2].Id,
                SegmentNumber = 3,
                FromStationId = stations[2].Id,
                ToStationId = stations[4].Id,
                FromTime = TimeSpan.FromHours(9).Add(TimeSpan.FromMinutes(30)),
                ToTime = TimeSpan.FromHours(10).Add(TimeSpan.FromMinutes(20)),
                SegmentCost = 2.4m
            },
            new()
            {
                AbstractRouteId = abstractRoutes[3].Id,
                SegmentNumber = 3,
                FromStationId = stations[1].Id,
                ToStationId = stations[0].Id,
                FromTime = TimeSpan.FromHours(18).Add(TimeSpan.FromMinutes(22)),
                ToTime = TimeSpan.FromHours(18).Add(TimeSpan.FromMinutes(40)),
                SegmentCost = (decimal)2.5
            },
            new()
            {
                AbstractRouteId = abstractRoutes[3].Id,
                SegmentNumber = 2,
                FromStationId = stations[2].Id,
                ToStationId = stations[1].Id,
                FromTime = TimeSpan.FromHours(17).Add(TimeSpan.FromMinutes(42)),
                ToTime = TimeSpan.FromHours(18).Add(TimeSpan.FromMinutes(20)),
                SegmentCost = 3
            },
            new()
            {
                AbstractRouteId = abstractRoutes[3].Id,
                SegmentNumber = 1,
                FromStationId = stations[4].Id,
                ToStationId = stations[2].Id,
                FromTime = TimeSpan.FromHours(17).Add(TimeSpan.FromMinutes(05)),
                ToTime = TimeSpan.FromHours(17).Add(TimeSpan.FromMinutes(40)),
                SegmentCost = 2.4m
            },

            new()
            {
                AbstractRouteId = abstractRoutes[4].Id,
                SegmentNumber = 1,
                FromStationId = stations[0].Id,
                ToStationId = stations[1].Id,
                FromTime = TimeSpan.FromHours(18).Add(TimeSpan.FromMinutes(22)),
                ToTime = TimeSpan.FromHours(18).Add(TimeSpan.FromMinutes(40)),
                SegmentCost = (decimal)2.5
            },
            new()
            {
                AbstractRouteId = abstractRoutes[4].Id,
                SegmentNumber = 2,
                FromStationId = stations[1].Id,
                ToStationId = stations[3].Id,
                FromTime = TimeSpan.FromHours(18).Add(TimeSpan.FromMinutes(42)),
                ToTime = TimeSpan.FromHours(21).Add(TimeSpan.FromMinutes(20)),
                SegmentCost = 3
            },
            new()
            {
                AbstractRouteId = abstractRoutes[4].Id,
                SegmentNumber = 3,
                FromStationId = stations[3].Id,
                ToStationId = stations[5].Id,
                FromTime = TimeSpan.FromHours(21).Add(TimeSpan.FromMinutes(30)),
                ToTime = TimeSpan.FromHours(30).Add(TimeSpan.FromMinutes(20)),
                SegmentCost = 2.4m
            },
            new()
            {
                AbstractRouteId = abstractRoutes[5].Id,
                SegmentNumber = 3,
                FromStationId = stations[1].Id,
                ToStationId = stations[0].Id,
                FromTime = TimeSpan.FromHours(18).Add(TimeSpan.FromMinutes(22)),
                ToTime = TimeSpan.FromHours(30).Add(TimeSpan.FromMinutes(40)),
                SegmentCost = (decimal)2.5
            },
            new()
            {
                AbstractRouteId = abstractRoutes[5].Id,
                SegmentNumber = 2,
                FromStationId = stations[3].Id,
                ToStationId = stations[1].Id,
                FromTime = TimeSpan.FromHours(17).Add(TimeSpan.FromMinutes(42)),
                ToTime = TimeSpan.FromHours(18).Add(TimeSpan.FromMinutes(20)),
                SegmentCost = 3
            },
            new()
            {
                AbstractRouteId = abstractRoutes[5].Id,
                SegmentNumber = 1,
                FromStationId = stations[5].Id,
                ToStationId = stations[3].Id,
                FromTime = TimeSpan.FromHours(17).Add(TimeSpan.FromMinutes(05)),
                ToTime = TimeSpan.FromHours(17).Add(TimeSpan.FromMinutes(40)),
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
                    ConcreteArrivalDate = newDate.Add(abstractRouteSegment.ToTime),
                    FromStationId = abstractRouteSegment.FromStationId,
                    ToStationId = abstractRouteSegment.ToStationId,
                    SegmentNumber = abstractRouteSegment.SegmentNumber
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
                case "TR1":
                    currentTrainTypeId = trainTypeTr1Id;
                    break;
                case "TR2":
                    currentTrainTypeId = trainTypeTr2Id;
                    break;
                case "TR3":
                    currentTrainTypeId = trainTypeTr3Id;
                    break;
                default: continue;
            }

            if (templatesByTrainTypeId.TryGetValue(currentTrainTypeId, out var relevantTemplates))
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

        return container;
    }

    private void ConfigureMocks()
    {
        // --- Настройка Station Repository ---
        _mockStationRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(),
                It.IsAny<IClientSessionHandle?>()))
            .ReturnsAsync((Guid id, IClientSessionHandle? sessionHandle) =>
            {
                return _testData.Stations.FirstOrDefault(x => x.Id == id);
            });

        _mockStationRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(),
                It.IsAny<IClientSessionHandle?>()))
            .ReturnsAsync((Guid id, IClientSessionHandle session) =>
            {
                return _testData.Stations.FirstOrDefault(x => x.Id == id);
            });

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
            .Setup(repo => repo.GetAbstractSegmentsByRouteIdAsync(It.IsAny<Guid>(),
                It.IsAny<IClientSessionHandle?>()))
            .ReturnsAsync((Guid routeId, IClientSessionHandle? session) =>
            {
                var matchingSegments = _testData.AbstractRouteSegments
                    .Where(segment => segment.AbstractRouteId == routeId)
                    .ToList();
                return matchingSegments;
            });

        _mockAbstractRouteSegmentRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(),
                It.IsAny<IClientSessionHandle?>()))
            .ReturnsAsync((Guid id, IClientSessionHandle? session) =>
            {
                var matchingSegment = _testData.AbstractRouteSegments.FirstOrDefault(s => s.Id == id);
                return matchingSegment;
            });

        // --- Настройка Abstract Route Repository ---
        _mockAbstractRouteRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(),
                It.IsAny<IClientSessionHandle?>()))
            .ReturnsAsync((Guid routeId, IClientSessionHandle? session) =>
            {
                var matchingRoute = _testData.AbstractRoutes
                    .Where(r => r.Id == routeId);
                return matchingRoute.FirstOrDefault();
            });

        // --- Настройка Concrete Route Repository ---
        _mockConcreteRouteRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(),
                It.IsAny<IClientSessionHandle?>()))
            .ReturnsAsync((Guid routeId, IClientSessionHandle? session) =>
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
            .Setup(repo => repo.GetConcreteSegmentsByConcreteRouteIdAsync(It.IsAny<Guid>(),
                It.IsAny<IClientSessionHandle?>()))
            .ReturnsAsync((Guid concreteRouteId, IClientSessionHandle? session) =>
            {
                var matchingSegments = _testData.ConcreteRouteSegments
                    .Where(cs => cs.ConcreteRouteId == concreteRouteId)
                    .ToList();
                return matchingSegments;
            });

        _mockConcreteRouteSegmentRepository
            .Setup(repo => repo.GetConcreteSegmentsByFromStationAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) =>
            {
                var matchingElements = _testData.ConcreteRouteSegments
                    .Where(cs => cs.FromStationId == id).ToList();
                return matchingElements;
            });

        _mockConcreteRouteSegmentRepository
            .Setup(repo => repo.GetConcreteSegmentsByToStationAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) =>
            {
                var matchingElements = _testData.ConcreteRouteSegments
                    .Where(cs => cs.ToStationId == id).ToList();
                return matchingElements;
            });

        // --- Настройка Carriage Availability Repository ---
        _mockCarriageAvailabilityRepository
            .Setup(repo => repo.GetByConcreteSegmentIdAsync(It.IsAny<Guid>(),
                It.IsAny<IClientSessionHandle?>()))
            .ReturnsAsync((Guid segmentId, IClientSessionHandle? session) =>
            {
                var matchingCarriages = _testData.CarriageAvailabilities
                    .Where(c => c.ConcreteRouteSegmentId == segmentId)
                    .ToList();
                return matchingCarriages;
            });

        // --- Настройка Carriage Template Repository ---
        _mockCarriageTemplateRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(),
                It.IsAny<IClientSessionHandle?>()))
            .ReturnsAsync((Guid id, IClientSessionHandle? session) =>
                _testData.CarriageTemplates.FirstOrDefault(t => t.Id == id));
        _mockCarriageTemplateRepository.Setup(repo => repo.GetByTrainTypeIdAsync(It.IsAny<Guid>(),
                It.IsAny<IClientSessionHandle?>()))
            .ReturnsAsync((Guid trainTypeId, IClientSessionHandle? session) =>
                _testData.TemplatesByTrainTypeId.TryGetValue(trainTypeId, out var templates)
                    ? templates
                    : new List<CarriageTemplate>());


        // --- Настройка Train Repository ---
        _mockTrainRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<string>(),
                It.IsAny<IClientSessionHandle?>()))
            .ReturnsAsync(
                (string id, IClientSessionHandle? session) => _testData.Trains.FirstOrDefault(t => t.Id == id));


        _mockSeatLockRepository.Setup(repo => repo.GetByRouteIdAsync(It.IsAny<Guid>(),
                It.IsAny<IClientSessionHandle?>()))
            .ReturnsAsync((Guid id, IClientSessionHandle? sessionHandle) =>
            {
                return _testData.SeatLocks.Where(sl =>
                    sl.LockedSeatInfos.Any(lsi => lsi.ConcreteRouteId == id));
            });
    }

    [Test]
    public async Task GetRouteSearchResultAsync_ValidInput_DirectRoute_ReturnsRouteSearchResult()
    {
        var fromStationId = _testData.BrestId;
        var toStationId = _testData.MinskId;
        var departureDate = DateTime.Now.AddDays(2);

        var result = (await _routeSearchService.GetRoutesAsync(new RouteSearchRequest
        {
            FromStationId = fromStationId, ToStationId = toStationId, DepartureDate = departureDate,
            IsDirectRoute = true
        })).ToList();

        Assert.That(result.Count, Is.EqualTo(2), "Ожидалось найти ровно 2 маршрута.");
        var result1 = result[0];
        Assert.That(result1.TotalDuration, Is.EqualTo(TimeSpan.FromHours(3)),
            "ожидаемое время поездки три часа");
        Assert.That(result1.DirectRoutes.Count, Is.EqualTo(1), "Должен был быть найден маршрут без пересадки");
        Assert.That(result1.MaximumTotalCost, Is.EqualTo(40m), "Стоимость маршрута должна быть 40");

        var result2 = result[1];
        Assert.That(result2.TotalDuration, Is.EqualTo(TimeSpan.FromMinutes(118)),
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

        var result = (await _routeSearchService.GetRoutesAsync(new RouteSearchRequest
        {
            FromStationId = fromStationId, ToStationId = toStationId, DepartureDate = departureDate,
            IsDirectRoute = true
        })).ToList();

        Assert.That(result.Count, Is.EqualTo(1), "Ожидалось найти ровно 1 маршрут.");
        var result1 = result[0];
        Assert.That(result1.TotalDuration, Is.EqualTo(TimeSpan.FromMinutes(38)),
            "ожидаемое время поездки 38 минут");
        Assert.That(result1.DirectRoutes.Count, Is.EqualTo(1), "Должен был быть найден маршрут без пересадки");
        Assert.That(result1.MinimalTotalCost, Is.EqualTo(31.5m), "Стоимость маршрута должна быть 31.5");
    }

    [Test]
    public async Task GetRouteSearchResultAsync_ValidInput_NotDirectRoute_ReturnsRouteSearchResult()
    {
        var fromStationId = _testData.HomelId;
        var toStationId = _testData.MinskId;
        var departureDate = DateTime.Now.AddDays(2);

        var result = (await _routeSearchService.GetRoutesAsync(new RouteSearchRequest
        {
            FromStationId = fromStationId, ToStationId = toStationId, DepartureDate = departureDate,
            IsDirectRoute = false
        })).ToList();
        Assert.That(result.Count, Is.EqualTo(1), "Должен был быть найден один маршрут с пересадкой");
        var route = result[0];
        Assert.That(route.DirectRoutes.Count(), Is.EqualTo(2), "В найденном маршруте должна быть одна пересадка");
        Assert.That(route.TotalDuration, Is.EqualTo(TimeSpan.FromHours(17).Add(TimeSpan.FromMinutes(15))),
            "Время в пути должно быть 17 часов 15 минут");
    }

    [Test]
    public async Task GetRouteSearchResultAsync_ValidInput_NotDirectRoute_ReturnsDirectRouteResult()
    {
        var fromStationId = _testData.ZhabinkaId;
        var toStationId = _testData.HomelId;
        var departureDate = DateTime.Now.AddDays(2);

        var result = (await _routeSearchService.GetRoutesAsync(new RouteSearchRequest
        {
            FromStationId = fromStationId, ToStationId = toStationId, DepartureDate = departureDate,
            IsDirectRoute = false
        })).ToList();
        Assert.That(result.Count, Is.EqualTo(1), "Должен был быть найден один маршрут с пересадкой");
        var route = result[0];
        Assert.That(route.DirectRoutes.Count(), Is.EqualTo(1), "В найденном маршруте должно не должно быть пересадок");
        Assert.That(route.TotalDuration, Is.EqualTo(TimeSpan.FromHours(11).Add(TimeSpan.FromMinutes(38))),
            "Время в пути должно быть 17 часов 15 минут");
    }

    [Test]
    public async Task GetRouteSearchResultAsync_ValidInput_NotDirectRoute_ReturnsSomeRoutesResult()
    {
        var fromStationId = _testData.BaranovichiId;
        var toStationId = _testData.HomelId;
        var departureDate = DateTime.Now.AddDays(2);

        var result = (await _routeSearchService.GetRoutesAsync(new RouteSearchRequest
        {
            FromStationId = fromStationId, ToStationId = toStationId, DepartureDate = departureDate,
            IsDirectRoute = false
        })).ToList();
        Assert.That(result.Count, Is.EqualTo(2), "Должен был быть найден два маршрута с пересадкой");
        var route1 = result[0];
        Assert.That(route1.DirectRoutes.Count(), Is.EqualTo(3), "В найденном маршруте должно должно быть 2 пересадки");
        Assert.That(route1.TotalDuration, Is.EqualTo(TimeSpan.FromHours(20).Add(TimeSpan.FromMinutes(50))),
            "Время в пути должно быть 20 часов 50 минут");
        var route2 = result[1];
        Assert.That(route2.DirectRoutes.Count(), Is.EqualTo(2), "В найденном маршруте должно должно быть 1 пересадки");
        Assert.That(route2.TotalDuration, Is.EqualTo(TimeSpan.FromHours(12).Add(TimeSpan.FromMinutes(38))),
            "Время в пути должно быть 12 часов 38 минут");
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
    public List<SeatLock> SeatLocks { get; set; } = new();

    public List<UserAccount> UserAccounts { get; set; } = new();

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
    public Guid AbstractRouteBrestHomelComplexId { get; set; }
    public Guid AbstractRouteHomelBrestComplexId { get; set; }

    public Guid ConcreteRouteBrestMinskDirectId { get; set; }
    public Guid ConcreteRouteBrestMinskComplexId { get; set; }
    public Guid ConcreteRouteMinskBrestDirectId { get; set; }
    public Guid ConcreteRouteMinskBrestComplexId { get; set; }
    public Guid ConcreteRouteBrestHomelComplexId { get; set; }
    public Guid ConcreteRouteHomelBrestComplexId { get; set; }
}