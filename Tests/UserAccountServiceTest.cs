using MongoDB.Driver;
using Moq;
using RailwayApp.Application.Models;
using RailwayApp.Application.Services;
using RailwayApp.Application.Services.PasswordHashers;
using RailwayApp.Domain.Entities;
using RailwayApp.Domain.Interfaces.IRepositories;
using RailwayApp.Domain.Statuses;

namespace Tests;

[TestFixture]
public class UserAccountServiceTest
{
    private Mock<IUserAccountRepository> _mockUserAccountRepository;

    private UserAccountService _userAccountService;

    private UserAccountTestDataContainer _testData;

    [SetUp]
    public void Setup()
    {
        _testData = GenerateTestData();

        _mockUserAccountRepository = new Mock<IUserAccountRepository>();

        ConfigureMocks();

        _userAccountService = new UserAccountService(new BCryptPasswordHasher(),
            _mockUserAccountRepository.Object);
    }

    private UserAccountTestDataContainer GenerateTestData()
    {
        var container = new UserAccountTestDataContainer();
        container.UserAccounts.Add(new UserAccount
        {
            Id = Guid.NewGuid(),
            Email = "test1@email.com",
            Surname = "Surname1",
            Name = "Name1",
            SecondName = "SecondName1",
            PhoneNumber = "+1234567890",
            BirthDate = DateTime.UtcNow.AddYears(-20),
            Gender = Gender.Female,
            Status = UserAccountStatus.Active,
            StatusChangedDate = DateTime.UtcNow,
            HashedPassword = "hashedpassword1"
        });

        return container;
    }

    private void ConfigureMocks()
    {
        _mockUserAccountRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(), 
                It.IsAny<IClientSessionHandle?>()))
            .ReturnsAsync((Guid id, IClientSessionHandle? session) => _testData.UserAccounts.FirstOrDefault(u => u.Id == id));

        _mockUserAccountRepository.Setup(repo => repo.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((string email) => _testData.UserAccounts.FirstOrDefault(u => u.Email == email));

        _mockUserAccountRepository.Setup(repo => repo.AddAsync(It.IsAny<UserAccount>(), 
                It.IsAny<IClientSessionHandle?>()))
            .ReturnsAsync((UserAccount user, IClientSessionHandle? session) =>
            {
                user.Id = Guid.NewGuid();
                _testData.UserAccounts.Add(user);
                return user.Id;
            });

        _mockUserAccountRepository.Setup(repo => repo.DeleteAsync(It.IsAny<Guid>()))
            .Returns((Guid id) =>
            {
                var result = _testData.UserAccounts.FirstOrDefault(u => u.Id == id) != null &&
                       _testData.UserAccounts.Remove(_testData.UserAccounts.First(u => u.Id == id));
                return Task.CompletedTask;
            });
        _mockUserAccountRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Guid>(), It.IsAny<UserAccount>()))
            .ReturnsAsync((Guid IDictionary, UserAccount userAccount) =>
            {
                var user = _testData.UserAccounts.FirstOrDefault(u => u.Id == IDictionary);
                if (user == null) return false;

                user.Email = userAccount.Email;
                user.Surname = userAccount.Surname;
                user.Name = userAccount.Name;
                user.SecondName = userAccount.SecondName;
                user.PhoneNumber = userAccount.PhoneNumber;
                user.BirthDate = userAccount.BirthDate;

                return true;
            });
    }

    [Test]
    public async Task CreateUserAccount_CRUD_WhenValidData()
    {
        var createUserRequest = new CreateUserAccountRequest
        {
            Email = "test2@email.com",
            Surname = "Surname1",
            Name = "Name1",
            SecondName = "SecondName1",
            PhoneNumber = "+1234567890",
            BirthDate = DateTime.UtcNow.AddYears(-20),
            Gender = Gender.Female,
            Password = "1111"
        };
        
        var userId = await _userAccountService.CreateUserAccountAsync(createUserRequest); 
        Assert.That(userId, Is.Not.EqualTo(Guid.Empty));

        var updateUserRequest = new UpdateUserAccountRequest
        {
            Surname = "Surname2",
            Name = "Name2",
            SecondName = "SecondName2",
            PhoneNumber = "+0987654321",
            BirthDate = DateTime.UtcNow.AddYears(-25)
        };
        
        var updatedUserId = await _userAccountService.UpdateUserAccountAsync(userId, updateUserRequest);
        Assert.That(updatedUserId, Is.EqualTo(userId));
        
        var updatePasswordRequest = new ChangePasswordRequest
        {
            OldPassword = "1111",
            NewPassword = "2222",
            DuplicateNewPassword = "2222"
        };
        
        var updatedPasswordId = await _userAccountService.UpdateUserPasswordAsync(userId, updatePasswordRequest);
        Assert.That(updatedPasswordId, Is.EqualTo(userId));
        
        var userAccount = await _userAccountService.DeleteUserAccountAsync(userId);
        Assert.That(userAccount, Is.EqualTo(userId));
    }
}

internal class UserAccountTestDataContainer
{
    public List<UserAccount> UserAccounts { get; set; } = new();
}