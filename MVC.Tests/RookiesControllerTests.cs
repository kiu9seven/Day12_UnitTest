using System;
using System.Collections.Generic;
using System.Linq;
using Day12.Controllers;
using Day12.Services;
using Day5_MVC_core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace MVC.Tests;

public class RookiesControllerTests
{
    private Mock<ILogger<RookiesController>> _loggerMock;
    private Mock<IPersonService> _personServiceMock;
    static List<Person> _people = new List<Person>
    {
         new Person
        {
                FirstName = "Phuong",
                LastName = "Nguyen Nam",
                Gender = "Male",
                DateOfBirth = new DateTime(2001, 1, 22),
                PhoneNumber = "",
                BirthPlace = "Phu Tho",
                IsGraduated = false
        },
        new Person
        {
                FirstName = "Phuong",
                LastName = "Nguyen Hoai",
                Gender = "Male",
                DateOfBirth = new DateTime(2001, 1, 22),
                PhoneNumber = "",
                BirthPlace = "Phu Tho",
                IsGraduated = false
        },
        new Person
        {
                FirstName = "Phuong",
                LastName = "Nguyen Ngoc",
                Gender = "Male",
                DateOfBirth = new DateTime(2001, 1, 22),
                PhoneNumber = "",
                BirthPlace = "Phu Tho",
                IsGraduated = false
        }
    };
    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<RookiesController>>();
        _personServiceMock = new Mock<IPersonService>();
        //Setup
        _personServiceMock.Setup(x => x.GetAll()).Returns(_people);
    }

    [Test]
    public void Index_ReturnsViewResult_WithAlistOfPeople()
    {
        //Setup
        // _personServiceMock.Setup(x => x.GetAll()).Returns(_people);

        //Arange
        var controller = new RookiesController(_loggerMock.Object, _personServiceMock.Object);
        var expectedCount = _people.Count;
        //Act
        var result = controller.Index();
        //Assert
        Assert.IsInstanceOf<ViewResult>(result, "Invalid return type!");

        var view = (ViewResult)result;
        Assert.IsAssignableFrom<List<Person>>(view.ViewData.Model, "Invalid view data model.");

        var model = view.ViewData.Model as List<Person>;
        Assert.IsNotNull(model, "View data model must not be NULL");
        Assert.AreEqual(expectedCount, model?.Count, "model counnt is not equal to expected count..");

        // var firstPerSon = model?.First();
        // Assert.AreEqual("Nguyen Phuong Nam", firstPerSon?.FullName, "FullName is ot equals!");

    }
    [Test]
    public void Detail_RightIndex_ReturnsViewResult_WithAPerson()
    {
        //Setup
        const int index = 2;
        _personServiceMock.Setup(x => x.GetOne(index)).Returns(_people[index - 1]);
        var expected = _people[index - 1];
        //Arange
        var controller = new RookiesController(_loggerMock.Object, _personServiceMock.Object);
        //Act
        var result = controller.Detail(index);
        //Assert
        Assert.IsInstanceOf<ViewResult>(result, "Invalid return type!");

        var view = (ViewResult)result;
        Assert.IsAssignableFrom<Person>(view.ViewData.Model, "Invalid view data model.");

        var model = view.ViewData.Model as Person;
        Assert.IsNotNull(model, "View data model must not be NULL");
        Assert.AreEqual(expected, model, "model is not equal to expected ..");
    }
    [Test]
    public void Detail_InvalidIndex_ReturnsNotFoundObjectResult_WithStringMessage()
    {
        //Setup
        const int index = 50;
        const string message = "Index out of range.";
        _personServiceMock.Setup(x => x.GetOne(index)).Throws(new IndexOutOfRangeException(message));
        // var expected = _people[index - 1];
        //Arange
        var controller = new RookiesController(_loggerMock.Object, _personServiceMock.Object);
        //Act
        var result = controller.Detail(index);
        //Assert
        Assert.IsInstanceOf<NotFoundObjectResult>(result, "Invalid return type!");
        
        var view = result as NotFoundObjectResult;
        Assert.IsNotNull(view, "View data model must not be NULL");
        Assert.IsInstanceOf<string>(view?.Value, "Invalid data type.");

        Assert.AreEqual(message, view?.Value?.ToString(), "Not equals!!!!");
    }
    [Test]
    public void Detail_InvalidIndex_ThrowException()
    {
        //Setup
        const int index = - 1;
        const string message = "Index must be greater tha zero.";
        _personServiceMock.Setup(x => x.GetOne(index)).Throws(new ArgumentException(message));
        // var expected = _people[index - 1];
        //Arange
        var controller = new RookiesController(_loggerMock.Object, _personServiceMock.Object);
        //Act
        //  var result = controller.Detail(index);
        
        //Assert
        var exception = Assert.Throws<ArgumentException>(() => controller.Detail(index));
        Assert.IsNotNull(exception, "Exception must not be NULL");
        Assert.AreEqual(message, exception?.Message, "Not equals!!!");
    }
    [Test]
    public void Create_InvalidModel_ReturnsRedirectToAction()
    {
        //Arrange]
        var person = new Person
        {
                FirstName = "Phuong",
                LastName = "Nguyen Hoang",
                Gender = "Male",
                DateOfBirth = new DateTime(2001, 1, 22),
                PhoneNumber = "",
                BirthPlace = "Phu Tho",
                IsGraduated = false
        };
        _personServiceMock.Setup(x => x.Create(person))
        .Callback<Person>((Person p) =>
        {
            _people.Add(p);
        });
        var controller = new RookiesController(_loggerMock.Object, _personServiceMock.Object);
        var expected = _people.Count + 1;

        //Act
        var result =controller.Create(person);

        //Assert
        Assert.IsInstanceOf<RedirectToActionResult>(result, "Invalid return type.");
        
        var view = (RedirectToActionResult)result;
        Assert.AreEqual("Index", view.ActionName, "Invalid action name!");

        var actual = _people.Count;
        Assert.AreEqual(expected, actual, "Error!!!");

        Assert.AreEqual(person, _people.Last(), "Not equals!!");

    }
    [Test]
    public void Create_ValidModel_ReturnsView_WithErrorInModelState()
    {
        const string key = "ERROR";
        const string message = "Invalid Model!!!!";
        //Arrange]
        var controller = new RookiesController(_loggerMock.Object, _personServiceMock.Object);
        controller.ModelState.AddModelError(key , message);

        //Act
        var result =controller.Create(null);

        //Assert
        Assert.IsInstanceOf<ViewResult>(result, "Invalid return type.");
        
        var view  = (ViewResult)result;
        var modelState = view.ViewData.ModelState;

        Assert.IsFalse(modelState.IsValid, "Invalid model state");
        Assert.AreEqual(1, modelState.ErrorCount, "");
        modelState.TryGetValue(key, out var value);
        var error = value?.Errors.First().ErrorMessage;
        Assert.AreEqual(message , error);
    }
    [TearDown]
    public void TearDown()
    {

    }
    
}