using System.Diagnostics;
using CustomSerializer;

using MongoDB.Driver.Linq;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Statuses;

namespace TestSerializer;

public class JsonSerializerTests
{
    private CustomJsonSerializer.CustomJsonSerializer _serializer;
    
    [SetUp]
    public void Setup()
    {
        _serializer = new CustomJsonSerializer.CustomJsonSerializer();
    }

    [Test]
    public void TestStationsSerializer()
    {
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
            }
        };

        var serializedData = _serializer.Serialize(stations);
        Debug.WriteLine(serializedData);
        var deserializedStations = _serializer.Deserialize<List<Station>>(serializedData);
        Assert.That(deserializedStations.Count, Is.EqualTo(stations.Count), "Неверное количество десериализованных объектов");

        for(int i = 0; i < stations.Count; i++)
        {
            Assert.That(deserializedStations[i].Name, Is.EqualTo(stations[i].Name), $"Неверное имя станции на позиции {i}");
            Assert.That(deserializedStations[i].Region, Is.EqualTo(stations[i].Region), $"Неверная область станции на позиции {i}");
        }
    }
    
    [Test]
    public void TicketSerializer()
    {
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            RouteId = Guid.NewGuid(),
            UserAccountId = Guid.NewGuid(),
            StartSegmentNumber = 1,
            EndSegmentNumber = 2,
            DepartureDate = DateTime.Now.AddDays(1),
            ArrivalDate = DateTime.Now.AddDays(1).AddHours(3),
            Price = 26,
            PassengerData = new PassengerData
            {
                Surname = "Иванов",
                FirstName = "Иван",
                SecondName = null,
                Gender = Gender.Male,
                BirthDate = new DateOnly(1990, 1, 1),
                PassportNumber = "AB1234567"
            },
            Carriage = 1,
            Seat = 4,
            HasBedLinenSet = false,
            PurchaseTime = DateTime.Now,
            Status = TicketStatus.Payed 
        };
        
        var serialized = _serializer.Serialize(ticket);
        var deserializedTicket = _serializer.Deserialize<Ticket>(serialized);
        Assert.That(deserializedTicket.Id, Is.EqualTo(ticket.Id), "Id не совпадает");
        Assert.That(deserializedTicket.RouteId, Is.EqualTo(ticket.RouteId), "RouteId не совпадает");
        Assert.That(deserializedTicket.UserAccountId, Is.EqualTo(ticket.UserAccountId), "UserAccountId не совпадает");
        Assert.That(deserializedTicket.StartSegmentNumber, Is.EqualTo(ticket.StartSegmentNumber), "StartSegmentNumber не совпадает");
        Assert.That(deserializedTicket.EndSegmentNumber, Is.EqualTo(ticket.EndSegmentNumber), "EndSegmentNumber не совпадает");
        Assert.That(deserializedTicket.DepartureDate, Is.EqualTo(ticket.DepartureDate.ToUniversalTime()), "дата отправления не совпадает");
        Assert.That(deserializedTicket.ArrivalDate, Is.EqualTo(ticket.ArrivalDate.ToUniversalTime()), "дата прибытия не совпадает");
        Assert.That(deserializedTicket.Price, Is.EqualTo(ticket.Price).Within(0.01), "цена не совпадает");
        Assert.That(deserializedTicket.PassengerData.Surname, Is.EqualTo(ticket.PassengerData.Surname), "фамилия не совпадает");
        Assert.That(deserializedTicket.PassengerData.FirstName, Is.EqualTo(ticket.PassengerData.FirstName), "имя не совпадает");
        Assert.That(deserializedTicket.PassengerData.SecondName, Is.EqualTo(ticket.PassengerData.SecondName), "отчество не совпадает");
        Assert.That(deserializedTicket.PassengerData.Gender, Is.EqualTo(ticket.PassengerData.Gender), "Пол не совпадает");
        Assert.That(deserializedTicket.PassengerData.BirthDate, Is.EqualTo(ticket.PassengerData.BirthDate), "дата рождения не совпадает");
        Assert.That(deserializedTicket.PassengerData.PassportNumber, Is.EqualTo(ticket.PassengerData.PassportNumber), "номер паспорта не совпадает");
        Assert.That(deserializedTicket.Carriage, Is.EqualTo(ticket.Carriage), "вагон не совпадает");
        Assert.That(deserializedTicket.Seat, Is.EqualTo(ticket.Seat), "место не совпадает");
        Assert.That(deserializedTicket.HasBedLinenSet, Is.EqualTo(ticket.HasBedLinenSet), "HasBedLinenSet не совпадает");
        Assert.That(deserializedTicket.PurchaseTime, Is.EqualTo(ticket.PurchaseTime.ToUniversalTime()), "время покупки не совпадает");
    }

    [Test]
    public void AbstractRouteTest()
    {
        var route = new AbstractRoute()
        {
            ActiveDays = new List<DayOfWeek> { DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Tuesday },
            DepartureTime = TimeSpan.FromHours(12),
            TransferCost = 10.5m,
            HasBeddingOption = true,
            IsActive = true,
            TrainNumber = "1234",
            Id = Guid.NewGuid()
        };
        
        var serialized = _serializer.Serialize(route);
        var deserializedRoute = _serializer.Deserialize<AbstractRoute>(serialized);
        
        Assert.That(deserializedRoute.ActiveDays.Count, Is.EqualTo(route.ActiveDays.Count), "Количество активных дней не совпадает");
        for (int i = 0; i < route.ActiveDays.Count; i++)
        {
            Assert.That(deserializedRoute.ActiveDays[i], Is.EqualTo(route.ActiveDays[i]), $"Активный день на позиции {i} не совпадает");
        }
        Assert.That(deserializedRoute.DepartureTime, Is.EqualTo(route.DepartureTime), "Время отправления не совпадает");
        Assert.That(deserializedRoute.TransferCost, Is.EqualTo(route.TransferCost), "Стоимость пересадки не совпадает");
        Assert.That(deserializedRoute.HasBeddingOption, Is.EqualTo(route.HasBeddingOption), "Опция постельного белья не совпадает");
        Assert.That(deserializedRoute.IsActive, Is.EqualTo(route.IsActive), "Активность маршрута не совпадает");
        Assert.That(deserializedRoute.TrainNumber, Is.EqualTo(route.TrainNumber), "Номер поезда не совпадает");
        Assert.That(deserializedRoute.Id, Is.EqualTo(route.Id), "Id маршрута не совпадает");
        
    }
}