using StudyTimer.Core.Exceptions;
using StudyTimer.Core.Services;

namespace StudyTimer.Tests;

public class CatalogServicesTests
{
    [Fact]
    public void StudentService_CrudAndSearch_Works()
    {
        var store = new StudyDataStore();
        var service = new StudentService(store);

        var s1 = service.Create("Mia", 4);
        var s2 = service.Create("Noah", 5);
        var updated = service.Update(s2.Id, "Noah R", 6, true);

        Assert.Equal("Noah R", updated.Name);
        Assert.Single(service.Search("Mia"));
        Assert.Single(service.Search("6"));

        service.Delete(s1.Id);
        Assert.Throws<NotFoundException>(() => service.GetById(s1.Id));
    }

    [Fact]
    public void SubjectService_DuplicateName_Throws()
    {
        var store = new StudyDataStore();
        var service = new SubjectService(store);

        service.Create("Math", "Algebra");

        Assert.Throws<ValidationException>(() => service.Create("math", "Geometry"));
    }
}
