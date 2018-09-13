using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using NodaTime.Extensions;
using NodaTime.TimeZones;

namespace TimeZoneserviceV2.Controllers
{
    [Route("[controller]")]
    public class ZonesController
    {
        private OkObjectResult okObjectResult;

        [HttpGet]
        public IActionResult Get()
        {
            List<TimeZoneInfo> timeZones = new List<TimeZoneInfo>();

            foreach (var location in TzdbDateTimeZoneSource.Default.ZoneLocations)
            {
                timeZones.Add(GetZoneInfo(location));
            }

            okObjectResult = new OkObjectResult(timeZones);
            return okObjectResult;
        }

        [HttpGet("{id}")]
        public IActionResult Get([FromRoute] string id)
        {
            if (!String.IsNullOrEmpty(id))
            {
                string timeZoneId = WebUtility.UrlDecode(id);

                var location = TzdbDateTimeZoneSource.Default.ZoneLocations.FirstOrDefault(
                    l => String.Compare(l.ZoneId, timeZoneId, StringComparison.OrdinalIgnoreCase) == 0);

                if (location != null)
                {
                    okObjectResult = new OkObjectResult(GetZoneInfo(location));
                    return okObjectResult;
                }
            }

            return new NotFoundResult();
        }

        private TimeZoneInfo GetZoneInfo(TzdbZoneLocation location)
        {
            var zone = DateTimeZoneProviders.Tzdb[location.ZoneId];

            // Get the start and end of the year in this zone
            var startOfYear = zone.AtStartOfDay(new LocalDate(2017, 1, 1));
            var endOfYear = zone.AtStrictly(new LocalDate(2018, 1, 1).AtMidnight().PlusNanoseconds(-1));

            // Get all intervals for current year
            var intervals = zone.GetZoneIntervals(startOfYear.ToInstant(), endOfYear.ToInstant()).ToList();

            // Try grab interval with DST. If none present, grab first one we can find
            var interval = intervals.FirstOrDefault(i => i.Savings.Seconds > 0) ?? intervals.FirstOrDefault();

            return new TimeZoneInfo
            {
                TimeZoneId = location.ZoneId,
                Offset = interval.StandardOffset.ToTimeSpan(),
                DstOffset = interval.WallOffset.ToTimeSpan(),
                CountryCode = location.CountryCode,
                CountryName = location.CountryName
            };
        }
    }
}
