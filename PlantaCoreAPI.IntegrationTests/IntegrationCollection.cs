using Xunit;

namespace PlantaCoreAPI.IntegrationTests;

[CollectionDefinition("Integration", DisableParallelization = true)]
public class IntegrationCollection : ICollectionFixture<Infrastructure.SharedAuthFixture> { }
