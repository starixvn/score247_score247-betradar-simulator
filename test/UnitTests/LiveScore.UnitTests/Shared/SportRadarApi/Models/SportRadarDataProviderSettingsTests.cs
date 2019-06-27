namespace LiveScore.UnitTests.Shared.SportRadarApi.Models
{
    using System;
    using System.Collections.Generic;
    using LiveScore.Shared.SportRadarApi.Models;
    using Xunit;

    public class SportRadarDataProviderSettingsTests
    {
        private readonly SportRadarDataProviderSettings sportSettings;
        public SportRadarDataProviderSettingsTests()
        {
            sportSettings = new SportRadarDataProviderSettings();
        }

        [Fact]
        public void GetSportSetting_Empty_ThrowsException()
        {
            sportSettings.Sports = new List<SportSetting>();

            var exception = Assert.Throws<InvalidOperationException>(() => sportSettings.GetSportSetting(3));

            Assert.Equal("Missing Setting for sportId 3", exception.Message);
        }

        [Fact]
        public void GetSportSetting_HasValue_ReturnsAssigned()
        {
            sportSettings.Sports = new List<SportSetting>
            {
                new SportSetting{Id = 3, Name = "Match"}
            };

            var setting = sportSettings.GetSportSetting(3);

            Assert.NotNull(setting);
            Assert.Equal("Match", setting.Name);
        }
    }
}
