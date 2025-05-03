using System.Collections;
using Microsoft.AspNetCore.Mvc;
using RailwayApp.Application.Models;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;

namespace railway_service.Controllers;

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
    ILogger<SeedController> logger) : ControllerBase
{
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
            //await ticketRepository.DeleteAllAsync();
            //await userAccountRepository.DeleteAllAsync();
            await concreteRouteRepository.DeleteAllAsync();
            await abstractRouteRepository.DeleteAllAsync();
            await trainRepository.DeleteAllAsync();
            await trainTypeRepository.DeleteAllAsync();

            var stations = new List<Station>
            {
                new Station
                {
                    Name = "Брест",
                    Region = "Брестская",
                },
                new Station
                {
                    Name = "Жабинка",
                    Region = "Брестская",
                },
                new Station
                {
                    Name = "Барановичи",
                    Region = "Брестская",
                },
                new Station
                {
                    Name = "Лунинец",
                    Region = "Брестская",
                },
                new Station
                {
                    Name = "Минск",
                    Region = "Минск",
                },
                new Station
                {
                    Name = "Гомель",
                    Region = "Гомельская",
                },
            };
            foreach (var station in stations)
            { 
                await stationRepository.CreateAsync(station);
            }

            var abstractRoutes = new List<AbstractRoute>
            {
                new AbstractRoute
                {
                    TrainNumber = "TR1",
                    ActiveDays = "MTWHFSN",
                    TransferCost = 20,
                    HasBeddingOption = false,
                    DepartureTime = TimeSpan.Zero.Add(TimeSpan.FromHours(15))
                },
                new AbstractRoute
                {
                    TrainNumber = "TR2",
                    ActiveDays = "MTWHFSN",
                    TransferCost = 15,
                    HasBeddingOption = true,
                    DepartureTime = TimeSpan.Zero.Add(TimeSpan.FromHours(9))
                },
                new AbstractRoute
                {
                    TrainNumber = "TR1",
                    ActiveDays = "MTWHFSN",
                    TransferCost = 18,
                    HasBeddingOption = false,
                    DepartureTime = TimeSpan.Zero.Add(TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(22)))
                },
                new AbstractRoute
                {
                    TrainNumber = "TR2",
                    ActiveDays = "MTWHFSN",
                    TransferCost = 20,
                    HasBeddingOption = true,
                    DepartureTime = TimeSpan.Zero.Add(TimeSpan.FromHours(17).Add(TimeSpan.FromMinutes(05)))
                },
                new AbstractRoute
                {
                    TrainNumber = "TR3",
                    ActiveDays = "MTWHFSN",
                    TransferCost = 18,
                    HasBeddingOption = true,
                    DepartureTime = TimeSpan.Zero.Add(TimeSpan.FromHours(18).Add(TimeSpan.FromMinutes(22)))
                },
                new AbstractRoute
                {
                    TrainNumber = "TR3",
                    ActiveDays = "MTWHFSN",
                    TransferCost = 20,
                    HasBeddingOption = true,
                    DepartureTime = TimeSpan.Zero.Add(TimeSpan.FromHours(18).Add(TimeSpan.FromMinutes(22)))
                },
            };
            foreach (var route in abstractRoutes)
            {
                abstractRouteRepository.CreateAsync(route);
            }
            var abstractRouteSegments = new List<AbstractRouteSegment>
            {
                new AbstractRouteSegment
                {
                    AbstractRouteId = abstractRoutes[0].Id,
                    SegmentNumber = 1,
                    FromStationId = stations[0].Id,
                    ToStationId = stations[4].Id,
                    FromTime = TimeSpan.Zero.Add(TimeSpan.FromHours(15)),
                    ToTime = TimeSpan.Zero.Add(TimeSpan.FromHours(18)),
                    SegmentCost = 5
                },
                new AbstractRouteSegment
                {
                    AbstractRouteId = abstractRoutes[1].Id,
                    SegmentNumber = 1,
                    FromStationId = stations[4].Id,
                    ToStationId = stations[0].Id,
                    FromTime = TimeSpan.Zero.Add(TimeSpan.FromHours(9)),
                    ToTime = TimeSpan.Zero.Add(TimeSpan.FromHours(12)),
                    SegmentCost = 5
                },
                new AbstractRouteSegment
                {
                    AbstractRouteId = abstractRoutes[2].Id,
                    SegmentNumber = 1,
                    FromStationId = stations[0].Id,
                    ToStationId = stations[1].Id,
                    FromTime = TimeSpan.Zero.Add(TimeSpan.FromHours(8)).Add(TimeSpan.FromMinutes(22)),
                    ToTime = TimeSpan.Zero.Add(TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(40))),
                    SegmentCost = (decimal)2.5
                },
                new AbstractRouteSegment
                {
                    AbstractRouteId = abstractRoutes[2].Id,
                    SegmentNumber = 2,
                    FromStationId = stations[1].Id,
                    ToStationId = stations[2].Id,
                    FromTime = TimeSpan.Zero.Add(TimeSpan.FromHours(8)).Add(TimeSpan.FromMinutes(42)),
                    ToTime = TimeSpan.Zero.Add(TimeSpan.FromHours(9).Add(TimeSpan.FromMinutes(20))),
                    SegmentCost = 3
                },
                new AbstractRouteSegment
                {
                    AbstractRouteId = abstractRoutes[2].Id,
                    SegmentNumber = 3,
                    FromStationId = stations[2].Id,
                    ToStationId = stations[4].Id,
                    FromTime = TimeSpan.Zero.Add(TimeSpan.FromHours(9)).Add(TimeSpan.FromMinutes(30)),
                    ToTime = TimeSpan.Zero.Add(TimeSpan.FromHours(10).Add(TimeSpan.FromMinutes(20))),
                    SegmentCost = 2.4m
                },
                new AbstractRouteSegment
                {
                    AbstractRouteId = abstractRoutes[3].Id,
                    SegmentNumber = 3,
                    FromStationId = stations[1].Id,
                    ToStationId = stations[0].Id,
                    FromTime = TimeSpan.Zero.Add(TimeSpan.FromHours(18)).Add(TimeSpan.FromMinutes(22)),
                    ToTime = TimeSpan.Zero.Add(TimeSpan.FromHours(18).Add(TimeSpan.FromMinutes(40))),
                    SegmentCost = (decimal)2.5
                },
                new AbstractRouteSegment
                {
                    AbstractRouteId = abstractRoutes[3].Id,
                    SegmentNumber = 2,
                    FromStationId = stations[2].Id,
                    ToStationId = stations[1].Id,
                    FromTime = TimeSpan.Zero.Add(TimeSpan.FromHours(17)).Add(TimeSpan.FromMinutes(42)),
                    ToTime = TimeSpan.Zero.Add(TimeSpan.FromHours(18).Add(TimeSpan.FromMinutes(20))),
                    SegmentCost = 3
                },
                new AbstractRouteSegment
                {
                    AbstractRouteId = abstractRoutes[3].Id,
                    SegmentNumber = 1,
                    FromStationId = stations[4].Id,
                    ToStationId = stations[2].Id,
                    FromTime = TimeSpan.Zero.Add(TimeSpan.FromHours(17)).Add(TimeSpan.FromMinutes(05)),
                    ToTime = TimeSpan.Zero.Add(TimeSpan.FromHours(17).Add(TimeSpan.FromMinutes(40))),
                    SegmentCost = 2.4m
                },
                
                new AbstractRouteSegment
                {
                    AbstractRouteId = abstractRoutes[4].Id,
                    SegmentNumber = 1,
                    FromStationId = stations[0].Id,
                    ToStationId = stations[1].Id,
                    FromTime = TimeSpan.Zero.Add(TimeSpan.FromHours(18)).Add(TimeSpan.FromMinutes(22)),
                    ToTime = TimeSpan.Zero.Add(TimeSpan.FromHours(18).Add(TimeSpan.FromMinutes(40))),
                    SegmentCost = (decimal)2.5
                },
                new AbstractRouteSegment
                {
                    AbstractRouteId = abstractRoutes[4].Id,
                    SegmentNumber = 2,
                    FromStationId = stations[1].Id,
                    ToStationId = stations[3].Id,
                    FromTime = TimeSpan.Zero.Add(TimeSpan.FromHours(18)).Add(TimeSpan.FromMinutes(42)),
                    ToTime = TimeSpan.Zero.Add(TimeSpan.FromHours(21).Add(TimeSpan.FromMinutes(20))),
                    SegmentCost = 3
                },
                new AbstractRouteSegment
                {
                    AbstractRouteId = abstractRoutes[4].Id,
                    SegmentNumber = 3,
                    FromStationId = stations[3].Id,
                    ToStationId = stations[5].Id,
                    FromTime = TimeSpan.Zero.Add(TimeSpan.FromHours(21)).Add(TimeSpan.FromMinutes(30)),
                    ToTime = TimeSpan.Zero.Add(TimeSpan.FromHours(30).Add(TimeSpan.FromMinutes(20))),
                    SegmentCost = 2.4m
                },
                new AbstractRouteSegment
                {
                    AbstractRouteId = abstractRoutes[5].Id,
                    SegmentNumber = 3,
                    FromStationId = stations[1].Id,
                    ToStationId = stations[0].Id,
                    FromTime = TimeSpan.Zero.Add(TimeSpan.FromHours(18)).Add(TimeSpan.FromMinutes(22)),
                    ToTime = TimeSpan.Zero.Add(TimeSpan.FromHours(30).Add(TimeSpan.FromMinutes(40))),
                    SegmentCost = (decimal)2.5
                },
                new AbstractRouteSegment
                {
                    AbstractRouteId = abstractRoutes[5].Id,
                    SegmentNumber = 2,
                    FromStationId = stations[3].Id,
                    ToStationId = stations[1].Id,
                    FromTime = TimeSpan.Zero.Add(TimeSpan.FromHours(17)).Add(TimeSpan.FromMinutes(42)),
                    ToTime = TimeSpan.Zero.Add(TimeSpan.FromHours(18).Add(TimeSpan.FromMinutes(20))),
                    SegmentCost = 3
                },
                new AbstractRouteSegment
                {
                    AbstractRouteId = abstractRoutes[5].Id,
                    SegmentNumber = 1,
                    FromStationId = stations[5].Id,
                    ToStationId = stations[3].Id,
                    FromTime = TimeSpan.Zero.Add(TimeSpan.FromHours(17)).Add(TimeSpan.FromMinutes(05)),
                    ToTime = TimeSpan.Zero.Add(TimeSpan.FromHours(17).Add(TimeSpan.FromMinutes(40))),
                    SegmentCost = 2.4m
                },
            };
            foreach (var segment in abstractRouteSegments)
            {
                await abstractRouteSegmentRepository.CreateAsync(segment);
            }

            var concreteRoutes = new List<ConcreteRoute>();
            for (int i = 0; i < 7; i++)
            {
                var date = DateTime.Now.Date;
                date = date.AddDays(i);
                foreach (var abstractRoute in abstractRoutes)
                {
                    var route = new ConcreteRoute
                    {
                        AbstractRouteId = abstractRoute.Id,
                        RouteDepartureDate = date.Add(abstractRoute.DepartureTime),
                    };
                    Console.WriteLine($"time for concrete route is {route.RouteDepartureDate}");
                    await concreteRouteRepository.CreateAsync(route);
                    concreteRoutes.Add(route);
                }
            }

            var concreteRouteSegments = new List<ConcreteRouteSegment>();
            for (int i = 0; i < 7; i++)
            {
                var date = DateTime.Now.Date;
                date = date.AddDays(i);
                for (int j = 0; j < abstractRouteSegments.Count; j ++)
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
                    Console.WriteLine("concreteRouteIndex is " + concreteRouteIndex);
                    Console.WriteLine("abstractRouteSegment is " + abstractRouteSegments.Count);
                    Console.WriteLine(concreteRoutes.Count);
                    var routeSegment = new ConcreteRouteSegment
                    {
                        AbstractSegmentId = abstractRouteSegment.Id,
                        ConcreteRouteId = concreteRoutes[concreteRouteIndex].Id,
                        ConcreteDepartureDate = date.Add(abstractRouteSegment.FromTime),
                        ConcreteArrivalDate = date.Add(abstractRouteSegment.ToTime),
                    };
                    concreteRouteSegments.Add(routeSegment);
                    await concreteRouteSegmentRepository.CreateAsync(routeSegment);
                }
            }

            var carriageAvailabilities = new List<CarriageAvailability>();
            var template1Id = Guid.NewGuid();
            var template2Id = Guid.NewGuid();
            var random = new Random();
            foreach (var concreteRouteSegment in concreteRouteSegments)
            {
                Console.WriteLine("here");
                bool[] initialValues10 = {
                    random.Next()%2==0?true:false,
                    random.Next()%2==0?true:false,
                    random.Next()%2==0?true:false,
                    random.Next()%2==0?true:false,
                    random.Next()%2==0?true:false,
                    random.Next()%2==0?true:false,
                    random.Next()%2==0?true:false,
                    random.Next()%2==0?true:false,
                    random.Next()%2==0?true:false,
                    random.Next()%2==0?true:false,
                };
                bool[] initialValues7 = {
                    random.Next()%2==0?true:false,
                    random.Next()%2==0?true:false,
                    random.Next()%2==0?true:false,
                    random.Next()%2==0?true:false,
                    random.Next()%2==0?true:false,
                    random.Next()%2==0?true:false,
                    random.Next()%2==0?true:false,
                };
                var carriage1 = new CarriageAvailability
                {
                    ConcreteRouteSegmentId = concreteRouteSegment.Id,
                    CarriageTemplateId = template1Id,
                    OccupiedSeats = new BitArray(initialValues10)
                };
                var carriage2 = new CarriageAvailability
                {
                    ConcreteRouteSegmentId = concreteRouteSegment.Id,
                    CarriageTemplateId = template2Id,
                    OccupiedSeats = new BitArray(initialValues7)
                };

                var id1= await carriageAvailabilityRepository.CreateAsync(carriage1);
                var id2 = await carriageAvailabilityRepository.CreateAsync(carriage2);
                Console.WriteLine($"ok add {id1} {id2}");
                carriageAvailabilities.Add(carriage1);
                carriageAvailabilities.Add(carriage2);
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