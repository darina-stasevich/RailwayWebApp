using System.Collections;
using Microsoft.AspNetCore.Mvc;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.Initializers;
using RailwayApp.Domain.Interfaces.IRepositories;

namespace RailwayApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeedController(
    IAbstractRouteRepository abstractRouteRepository,
    IAbstractRouteSegmentRepository abstractRouteSegmentRepository,
    ICarriageAvailabilityRepository carriageAvailabilityRepository,
    ICarriageTemplateRepository carriageTemplateRepository,
    IConcreteRouteRepository concreteRouteRepository,
    IConcreteRouteSegmentRepository concreteRouteSegmentRepository,
    IStationRepository stationRepository,
    ITicketRepository ticketRepository,
    ITrainRepository trainRepository,
    ITrainTypeRepository trainTypeRepository,
    IUserAccountRepository userAccountRepository,
    ILogger<SeedController> logger
) : ControllerBase
{
    private readonly TrainCarriageInitializer _trainCarriageInitializer = new();

    [HttpPost]
    public async Task<IActionResult> SeedDatabase()
    {
        logger.LogInformation("Starting database seed process...");
        try
        {
            await carriageAvailabilityRepository.DeleteAllAsync();
            await carriageTemplateRepository.DeleteAllAsync();
            await concreteRouteSegmentRepository.DeleteAllAsync();
            await abstractRouteSegmentRepository.DeleteAllAsync();
            await stationRepository.DeleteAllAsync();
            await ticketRepository.DeleteAllAsync();
            await userAccountRepository.DeleteAllAsync();
            await concreteRouteRepository.DeleteAllAsync();
            await abstractRouteRepository.DeleteAllAsync();
            await trainRepository.DeleteAllAsync();
            await trainTypeRepository.DeleteAllAsync();

            foreach (var carriageTemplate in _trainCarriageInitializer.CarriageTemplates)
                await carriageTemplateRepository.AddAsync(carriageTemplate);

            foreach (var type in _trainCarriageInitializer.TrainTypes) await trainTypeRepository.AddAsync(type);

            var stations = new List<Station>
            {
                new()
                {
                    Name = "Брест",
                    Region = "Брестская"
                },
                new()
                {
                    Name = "Жабинка",
                    Region = "Брестская"
                },
                new()
                {
                    Name = "Барановичи",
                    Region = "Брестская"
                },
                new()
                {
                    Name = "Лунинец",
                    Region = "Брестская"
                },
                new()
                {
                    Name = "Минск",
                    Region = "Минск"
                },
                new()
                {
                    Name = "Гомель",
                    Region = "Гомельская"
                }
            };
            foreach (var station in stations) await stationRepository.AddAsync(station);
            var trains = new List<Train>
            {
                new()
                {
                    Id = "TR1",
                    TrainTypeId = _trainCarriageInitializer.TrainTypes[8].Id
                },
                new()
                {
                    Id = "TR2",
                    TrainTypeId = _trainCarriageInitializer.TrainTypes[9].Id
                },
                new()
                {
                    Id = "TR3",
                    TrainTypeId = _trainCarriageInitializer.TrainTypes[10].Id
                }
            };
            foreach (var train in trains) await trainRepository.AddAsync(train);

            var abstractRoutes = new List<AbstractRoute>
            {
                new()
                {
                    TrainNumber = "TR1",
                    ActiveDays = "MTWHFSN",
                    TransferCost = 20,
                    HasBeddingOption = false,
                    DepartureTime = TimeSpan.FromHours(15)
                },
                new()
                {
                    TrainNumber = "TR2",
                    ActiveDays = "MTWHFSN",
                    TransferCost = 15,
                    HasBeddingOption = true,
                    DepartureTime = TimeSpan.FromHours(9)
                },
                new()
                {
                    TrainNumber = "TR1",
                    ActiveDays = "MTWHFSN",
                    TransferCost = 18,
                    HasBeddingOption = false,
                    DepartureTime = TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(22))
                },
                new()
                {
                    TrainNumber = "TR2",
                    ActiveDays = "MTWHFSN",
                    TransferCost = 20,
                    HasBeddingOption = true,
                    DepartureTime = TimeSpan.FromHours(17).Add(TimeSpan.FromMinutes(05))
                },
                new()
                {
                    TrainNumber = "TR3",
                    ActiveDays = "MTWHFSN",
                    TransferCost = 18,
                    HasBeddingOption = true,
                    DepartureTime = TimeSpan.FromHours(18).Add(TimeSpan.FromMinutes(22))
                },
                new()
                {
                    TrainNumber = "TR3",
                    ActiveDays = "MTWHFSN",
                    TransferCost = 20,
                    HasBeddingOption = true,
                    DepartureTime = TimeSpan.FromHours(18).Add(TimeSpan.FromMinutes(22))
                }
            };
            foreach (var route in abstractRoutes) abstractRouteRepository.AddAsync(route);
            var abstractRouteSegments = new List<AbstractRouteSegment>
            {
                new()
                {
                    AbstractRouteId = abstractRoutes[0].Id,
                    SegmentNumber = 1,
                    FromStationId = stations[0].Id,
                    ToStationId = stations[4].Id,
                    FromTime = TimeSpan.FromHours(15),
                    ToTime = TimeSpan.FromHours(18),
                    SegmentCost = 5
                },
                new()
                {
                    AbstractRouteId = abstractRoutes[1].Id,
                    SegmentNumber = 1,
                    FromStationId = stations[4].Id,
                    ToStationId = stations[0].Id,
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
            foreach (var segment in abstractRouteSegments) await abstractRouteSegmentRepository.AddAsync(segment);

            var applicationLocalTimeZone = TimeZoneInfo.CreateCustomTimeZone(
                "AppFixedUTC+3",
                TimeSpan.FromHours(3),
                "App Fixed UTC+3",
                "App Fixed UTC+3"
            );

            var concreteRoutes = new List<ConcreteRoute>();
            for (var i = 0; i < 7; i++)
            {
                var todayInApplicationLocalTimeZone =
                    TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, applicationLocalTimeZone).Date;
                var operationDate =
                    todayInApplicationLocalTimeZone.AddDays(i); // Это DateOnly по сути, в локальном времени приложения

                foreach (var abstractRoute in abstractRoutes)
                {
                    var localDepartureDateTime = new DateTime(operationDate.Year, operationDate.Month,
                        operationDate.Day,
                        abstractRoute.DepartureTime.Hours,
                        abstractRoute.DepartureTime.Minutes,
                        abstractRoute.DepartureTime.Seconds,
                        DateTimeKind.Unspecified);

                    var utcDepartureDateTime =
                        TimeZoneInfo.ConvertTimeToUtc(localDepartureDateTime, applicationLocalTimeZone);

                    var route = new ConcreteRoute
                    {
                        AbstractRouteId = abstractRoute.Id,
                        RouteDepartureDate = utcDepartureDateTime
                    };

                    Console.WriteLine(
                        $"Local departure time is {localDepartureDateTime}, Stored UTC departure time is {route.RouteDepartureDate}");
                    await concreteRouteRepository.AddAsync(route);
                    concreteRoutes.Add(route);
                }
            }

            var concreteRouteSegments = new List<ConcreteRouteSegment>();

            for (var i = 0; i < 7; i++)
            {
                var date = DateTime.Now.Date;
                date = date.AddDays(i);
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
                        ConcreteDepartureDate = date.Add(abstractRouteSegment.FromTime),
                        ConcreteArrivalDate = date.Add(abstractRouteSegment.ToTime),
                        FromStationId = abstractRouteSegment.FromStationId,
                        ToStationId = abstractRouteSegment.ToStationId,
                        SegmentNumber = abstractRouteSegment.SegmentNumber
                    };
                    concreteRouteSegments.Add(routeSegment);
                    await concreteRouteSegmentRepository.AddAsync(routeSegment);
                }
            }

            var trainTypeTr1Id = _trainCarriageInitializer.TrainTypes[8].Id;
            var trainTypeTr2Id = _trainCarriageInitializer.TrainTypes[9].Id;
            var trainTypeTr3Id = _trainCarriageInitializer.TrainTypes[10].Id;

            var templatesByTrainTypeId = _trainCarriageInitializer.CarriageTemplates
                .GroupBy(t => t.TrainTypeId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var random = new Random();
            var carriageAvailabilities = new List<CarriageAvailability>();
            var firstDayConcreteSegments = concreteRouteSegments.Skip(14).Take(14);
            foreach (var concreteRouteSegment in firstDayConcreteSegments)
            {
                var concreteRoute = concreteRoutes.FirstOrDefault(cr => cr.Id == concreteRouteSegment.ConcreteRouteId);
                if (concreteRoute == null)
                {
                    Console.WriteLine(
                        $"Warning: ConcreteRoute not found for ConcreteSegmentId {concreteRouteSegment.Id}");
                    continue;
                }

                var abstractRoute = abstractRoutes.FirstOrDefault(ar => ar.Id == concreteRoute.AbstractRouteId);
                if (abstractRoute == null)
                {
                    Console.WriteLine($"Warning: AbstractRoute not found for ConcreteRouteId {concreteRoute.Id}");
                    continue;
                }

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
                    default:
                        Console.WriteLine(
                            $"Warning: Unknown TrainNumber {abstractRoute.TrainNumber} for AbstractRouteId {abstractRoute.Id}");
                        continue;
                }

                if (!templatesByTrainTypeId.TryGetValue(currentTrainTypeId, out var relevantTemplates))
                {
                    Console.WriteLine(
                        $"Warning: No CarriageTemplates found for TrainTypeId {currentTrainTypeId} (TrainNumber {abstractRoute.TrainNumber})");
                    continue;
                }

                foreach (var template in relevantTemplates)
                {
                    var initialValues = new bool[template.TotalSeats];
                    for (var i = 0; i < template.TotalSeats; i++)
                        initialValues[i] = random.Next(0, 2) == 1;
                    var occupiedSeatsArray = new BitArray(initialValues);

                    var carriageAvailability = new CarriageAvailability
                    {
                        ConcreteRouteSegmentId = concreteRouteSegment.Id,
                        CarriageTemplateId = template.Id,
                        OccupiedSeats = occupiedSeatsArray
                    };

                    var createdId = await carriageAvailabilityRepository.AddAsync(carriageAvailability);
                    carriageAvailabilities.Add(carriageAvailability); // Добавляем в локальный список (если нужно)

                    Console.WriteLine(
                        $"OK add CarriageAvailability (ID: {createdId}) for Segment {concreteRouteSegment.Id}, Template {template.LayoutIdentifier} ({template.Id})");
                }
            }

            logger.LogInformation("Database seed process completed successfully.");
            return Ok("Database seeded successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred during database seeding.");
            return StatusCode(500, "Internal Server Error");
        }
    }
}