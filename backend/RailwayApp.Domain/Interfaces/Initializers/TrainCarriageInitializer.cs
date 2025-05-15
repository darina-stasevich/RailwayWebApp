using RailwayApp.Domain.Entities;

namespace RailwayApp.Domain.Interfaces.Initializers;

public class TrainCarriageInitializer
{
    public TrainCarriageInitializer()
    {
        InitializeData();
    }

    public List<TrainType> TrainTypes { get; } = new();
    public List<CarriageTemplate> CarriageTemplates { get; } = new();

    private void InitializeData()
    {
        var nightLuxId = Guid.NewGuid();
        var fastId = Guid.NewGuid();
        var passengerId = Guid.NewGuid();
        var cityOId = Guid.NewGuid();
        var regionalSId = Guid.NewGuid();
        var eprId = Guid.NewGuid();
        var epr2Id = Guid.NewGuid();
        var dp1Id = Guid.NewGuid();
        var epmId = Guid.NewGuid();
        var epm2Id = Guid.NewGuid();
        var dp3_145Id = Guid.NewGuid();
        var dp3_152Id = Guid.NewGuid();
        var dp6Id = Guid.NewGuid();
        var epgId = Guid.NewGuid();

        TrainTypes.AddRange(new List<TrainType>
        {
            new() { Id = nightLuxId, TypeName = "Ночной Люкс" },
            new() { Id = fastId, TypeName = "Скорый" },
            new() { Id = passengerId, TypeName = "Пассажирский" },
            new() { Id = cityOId, TypeName = "Пригородный-О" },
            new() { Id = regionalSId, TypeName = "Региональный-С" },
            new() { Id = eprId, TypeName = "ЭПР" },
            new() { Id = epr2Id, TypeName = "ЭПРII" },
            new() { Id = dp1Id, TypeName = "ДП1" },
            new() { Id = epmId, TypeName = "ЭПМ" },
            new() { Id = epm2Id, TypeName = "ЭПМII" },
            new() { Id = dp3_145Id, TypeName = "ДП3 (145 мест)" },
            new() { Id = dp3_152Id, TypeName = "ДП3 (152 места)" },
            new() { Id = dp6Id, TypeName = "ДП6" },
            new() { Id = epgId, TypeName = "ЭПГ" }
        });

        for (var i = 1; i <= 10; i++)
            CarriageTemplates.Add(new CarriageTemplate
            {
                TrainTypeId = nightLuxId, CarriageNumber = i, LayoutIdentifier = $"NochnoyLux-{i}-Luxury",
                TotalSeats = 18, PriceMultiplier = 2.5m
            });

        for (var i = 1; i <= 12; i++)
            CarriageTemplates.Add(new CarriageTemplate
            {
                TrainTypeId = fastId, CarriageNumber = i, LayoutIdentifier = $"Skory-{i}-Compartment", TotalSeats = 36,
                PriceMultiplier = 2.0m
            });

        for (var i = 1; i <= 15; i++)
            CarriageTemplates.Add(new CarriageTemplate
            {
                TrainTypeId = passengerId, CarriageNumber = i, LayoutIdentifier = $"Passazhirsky-{i}-OpenPlan",
                TotalSeats = 54, PriceMultiplier = 1.2m
            });

        for (var i = 1; i <= 5; i++)
            CarriageTemplates.Add(new CarriageTemplate
            {
                TrainTypeId = cityOId, CarriageNumber = i, LayoutIdentifier = $"PrigorodnyO-{i}-Economy",
                TotalSeats = 81, PriceMultiplier = 1.0m
            });

        for (var i = 1; i <= 6; i++)
            CarriageTemplates.Add(new CarriageTemplate
            {
                TrainTypeId = regionalSId, CarriageNumber = i, LayoutIdentifier = $"RegionalnyS-{i}-Economy",
                TotalSeats = 62, PriceMultiplier = 1.1m
            });

        CarriageTemplates.AddRange(new List<CarriageTemplate>
        {
            new()
            {
                TrainTypeId = eprId, CarriageNumber = 1, LayoutIdentifier = "EPR-1-Business", TotalSeats = 52,
                PriceMultiplier = 1.5m
            },
            new()
            {
                TrainTypeId = eprId, CarriageNumber = 2, LayoutIdentifier = "EPR-2-Business", TotalSeats = 49,
                PriceMultiplier = 1.5m
            },
            new()
            {
                TrainTypeId = eprId, CarriageNumber = 3, LayoutIdentifier = "EPR-3-Business", TotalSeats = 55,
                PriceMultiplier = 1.5m
            },
            new()
            {
                TrainTypeId = eprId, CarriageNumber = 4, LayoutIdentifier = "EPR-4-Business", TotalSeats = 52,
                PriceMultiplier = 1.5m
            }
        });

        CarriageTemplates.AddRange(new List<CarriageTemplate>
        {
            new()
            {
                TrainTypeId = epr2Id, CarriageNumber = 1, LayoutIdentifier = "EPRII-1-Business", TotalSeats = 52,
                PriceMultiplier = 1.5m
            },
            new()
            {
                TrainTypeId = epr2Id, CarriageNumber = 2, LayoutIdentifier = "EPRII-2-Business", TotalSeats = 59,
                PriceMultiplier = 1.5m
            },
            new()
            {
                TrainTypeId = epr2Id, CarriageNumber = 3, LayoutIdentifier = "EPRII-3-Business", TotalSeats = 58,
                PriceMultiplier = 1.5m
            },
            new()
            {
                TrainTypeId = epr2Id, CarriageNumber = 4, LayoutIdentifier = "EPRII-4-Business", TotalSeats = 65,
                PriceMultiplier = 1.5m
            },
            new()
            {
                TrainTypeId = epr2Id, CarriageNumber = 5, LayoutIdentifier = "EPRII-5-Business", TotalSeats = 60,
                PriceMultiplier = 1.5m
            }
        });

        CarriageTemplates.Add(new CarriageTemplate
        {
            TrainTypeId = dp1Id, CarriageNumber = 1, LayoutIdentifier = "DP1-1-Economy", TotalSeats = 91,
            PriceMultiplier = 1.0m
        });

        CarriageTemplates.AddRange(new List<CarriageTemplate>
        {
            new()
            {
                TrainTypeId = epmId, CarriageNumber = 1, LayoutIdentifier = "EPM-1-FirstClass", TotalSeats = 16,
                PriceMultiplier = 2.0m
            },
            new()
            {
                TrainTypeId = epmId, CarriageNumber = 2, LayoutIdentifier = "EPM-1-SecondClass", TotalSeats = 25,
                PriceMultiplier = 1.5m
            },
            new()
            {
                TrainTypeId = epmId, CarriageNumber = 3, LayoutIdentifier = "EPM-2-SecondClass", TotalSeats = 59,
                PriceMultiplier = 1.5m
            },
            new()
            {
                TrainTypeId = epmId, CarriageNumber = 4, LayoutIdentifier = "EPM-3-SecondClass", TotalSeats = 60,
                PriceMultiplier = 1.5m
            },
            new()
            {
                TrainTypeId = epmId, CarriageNumber = 5, LayoutIdentifier = "EPM-4-SecondClass", TotalSeats = 64,
                PriceMultiplier = 1.5m
            },
            new()
            {
                TrainTypeId = epmId, CarriageNumber = 6, LayoutIdentifier = "EPM-5-SecondClass", TotalSeats = 59,
                PriceMultiplier = 1.5m
            },
            new()
            {
                TrainTypeId = epmId, CarriageNumber = 7, LayoutIdentifier = "EPM-6-SecondClass", TotalSeats = 47,
                PriceMultiplier = 1.5m
            },
            new()
            {
                TrainTypeId = epmId, CarriageNumber = 8, LayoutIdentifier = "EPM-7-SecondClass", TotalSeats = 52,
                PriceMultiplier = 1.5m
            }
        });

        CarriageTemplates.AddRange(new List<CarriageTemplate>
        {
            new()
            {
                TrainTypeId = epm2Id, CarriageNumber = 1, LayoutIdentifier = "EPMII-1-FirstClass", TotalSeats = 16,
                PriceMultiplier = 2.0m
            },
            new()
            {
                TrainTypeId = epm2Id, CarriageNumber = 2, LayoutIdentifier = "EPMII-1-SecondClass", TotalSeats = 23,
                PriceMultiplier = 1.5m
            },
            new()
            {
                TrainTypeId = epm2Id, CarriageNumber = 3, LayoutIdentifier = "EPMII-2-SecondClass", TotalSeats = 59,
                PriceMultiplier = 1.5m
            },
            new()
            {
                TrainTypeId = epm2Id, CarriageNumber = 4, LayoutIdentifier = "EPMII-3-SecondClass", TotalSeats = 69,
                PriceMultiplier = 1.5m
            },
            new()
            {
                TrainTypeId = epm2Id, CarriageNumber = 5, LayoutIdentifier = "EPMII-4-SecondClass", TotalSeats = 45,
                PriceMultiplier = 1.5m
            },
            new()
            {
                TrainTypeId = epm2Id, CarriageNumber = 6, LayoutIdentifier = "EPMII-5-SecondClass", TotalSeats = 50,
                PriceMultiplier = 1.5m
            }
        });

        CarriageTemplates.AddRange(new List<CarriageTemplate>
        {
            new()
            {
                TrainTypeId = dp3_145Id, CarriageNumber = 1, LayoutIdentifier = "DP3_145-1-Economy", TotalSeats = 52,
                PriceMultiplier = 1.0m
            },
            new()
            {
                TrainTypeId = dp3_145Id, CarriageNumber = 2, LayoutIdentifier = "DP3_145-2-Economy", TotalSeats = 41,
                PriceMultiplier = 1.0m
            },
            new()
            {
                TrainTypeId = dp3_145Id, CarriageNumber = 3, LayoutIdentifier = "DP3_145-3-Economy", TotalSeats = 52,
                PriceMultiplier = 1.0m
            }
        });

        CarriageTemplates.AddRange(new List<CarriageTemplate>
        {
            new()
            {
                TrainTypeId = dp3_152Id, CarriageNumber = 1, LayoutIdentifier = "DP3_152-1-Economy", TotalSeats = 52,
                PriceMultiplier = 1.0m
            },
            new()
            {
                TrainTypeId = dp3_152Id, CarriageNumber = 2, LayoutIdentifier = "DP3_152-2-Economy", TotalSeats = 48,
                PriceMultiplier = 1.0m
            },
            new()
            {
                TrainTypeId = dp3_152Id, CarriageNumber = 3, LayoutIdentifier = "DP3_152-3-Economy", TotalSeats = 52,
                PriceMultiplier = 1.0m
            }
        });

        CarriageTemplates.AddRange(new List<CarriageTemplate>
        {
            new()
            {
                TrainTypeId = dp6Id, CarriageNumber = 1, LayoutIdentifier = "DP6-1-Economy", TotalSeats = 48,
                PriceMultiplier = 1.0m
            },
            new()
            {
                TrainTypeId = dp6Id, CarriageNumber = 2, LayoutIdentifier = "DP6-2-Economy", TotalSeats = 42,
                PriceMultiplier = 1.0m
            },
            new()
            {
                TrainTypeId = dp6Id, CarriageNumber = 3, LayoutIdentifier = "DP6-3-Economy", TotalSeats = 64,
                PriceMultiplier = 1.0m
            },
            new()
            {
                TrainTypeId = dp6Id, CarriageNumber = 4, LayoutIdentifier = "DP6-4-Economy", TotalSeats = 64,
                PriceMultiplier = 1.0m
            },
            new()
            {
                TrainTypeId = dp6Id, CarriageNumber = 5, LayoutIdentifier = "DP6-5-Economy", TotalSeats = 46,
                PriceMultiplier = 1.0m
            },
            new()
            {
                TrainTypeId = dp6Id, CarriageNumber = 6, LayoutIdentifier = "DP6-6-Economy", TotalSeats = 48,
                PriceMultiplier = 1.0m
            }
        });

        CarriageTemplates.AddRange(new List<CarriageTemplate>
        {
            new()
            {
                TrainTypeId = epgId, CarriageNumber = 1, LayoutIdentifier = "EPG-1-Economy", TotalSeats = 65,
                PriceMultiplier = 1.0m
            },
            new()
            {
                TrainTypeId = epgId, CarriageNumber = 2, LayoutIdentifier = "EPG-2-Economy", TotalSeats = 65,
                PriceMultiplier = 1.0m
            }, // Assuming Toilet/Zone here
            new()
            {
                TrainTypeId = epgId, CarriageNumber = 3, LayoutIdentifier = "EPG-3-Economy", TotalSeats = 65,
                PriceMultiplier = 1.0m
            },
            new()
            {
                TrainTypeId = epgId, CarriageNumber = 4, LayoutIdentifier = "EPG-4-Economy", TotalSeats = 65,
                PriceMultiplier = 1.0m
            }
        });
    }
}