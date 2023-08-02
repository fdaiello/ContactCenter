using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactCenter.Core.Models
{
    /**
     * This is User's Group Model.
     * Id:   index
     * Name: group name
     */
    public class WeekSchedule
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int? DepartmentId { get; set; }          // if did=0, this is a company's global schedule, but if did>0, this is special department's shedule.
        public virtual Department Department { get; }   // department descriptor
        public string SunOpen { get; set; }
        public string SunClose { get; set; }
        public string MonOpen { get; set; }
        public string MonClose { get; set; }
        public string TueOpen { get; set; }
        public string TueClose { get; set; }
        public string WedOpen { get; set; }
        public string WedClose { get; set; }
        public string ThuOpen { get; set; }
        public string ThuClose { get; set; }
        public string FriOpen { get; set; }
        public string FriClose { get; set; }
        public string SatOpen { get; set; }
        public string SatClose { get; set; }
        public string OutOfOfficeHoursPhrase { get; set; }

        // Given a day of week, and the time, returns if its open or note
        public bool IsOpen(DateTime dateTime)
		{
            // Saves string with open and closed time from schedule; format "08:00", "18:00", "08:30", "17:30"
            string openTime = string.Empty;
            string closedTime = string.Empty;

            // Saves converted hour part of schedule from string to integer
            int openHour = 0;
            int closedHour = 24;

            // Saves converted minute from schedule from string to integer
            int openMinute = 0;
            int closedMinute = 0;

            // Checks de day of week receaved as parameter, and gets open and closed time as string
            switch (dateTime.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    openTime = this.SunOpen;
                    closedTime = this.SunClose;
                    break;
                case DayOfWeek.Monday:
                    openTime = this.MonOpen;
                    closedTime = this.MonClose;
                    break;
                case DayOfWeek.Tuesday:
                    openTime = this.TueOpen;
                    closedTime = this.TueClose;
                    break;
                case DayOfWeek.Wednesday:
                    openTime = this.WedOpen;
                    closedTime = this.WedClose;
                    break;
                case DayOfWeek.Thursday:
                    openTime = this.ThuOpen;
                    closedTime = this.ThuClose;
                    break;
                case DayOfWeek.Friday:
                    openTime = this.FriOpen;
                    closedTime = this.FriClose;
                    break;
                case DayOfWeek.Saturday:
                    openTime = this.SatOpen;
                    closedTime = this.SatClose;
                    break;
            }

            
            // If there is no open or closed time saved
            if ( string.IsNullOrEmpty(openTime) || string.IsNullOrEmpty(closedTime))
			{
                // Not a valid working day, return false
                return false;
			}

            // Check if we have a valid open hour, and save it to local variable.
            if (Int32.TryParse(openTime.Split(":")[0], out int openHour0))
			{
                openHour = openHour0;
			}

            // Check if we have a valid closed hour, and save it to a local variable
            if (Int32.TryParse(closedTime.Split(":")[0], out int closedHour0))
            {
                closedHour = closedHour0;
            }

            // same to open minute
            if (openTime.Contains(":") && Int32.TryParse(openTime.Split(":")[1], out int openMinute0))
			{
                openMinute = openMinute0;
			}
            // same to closed minute
            if (closedTime.Contains(":") && Int32.TryParse(closedTime.Split(":")[1], out int closedMinute0))
            {
                closedMinute = closedMinute0;
            }

            // Gets integers hour and minute from dateTime passe as parameter - 
            int hour = dateTime.Hour;
            int minute = dateTime.Minute;

            // First check if open time is ok
            bool passedOpenHour = hour>openHour || ( hour == openHour && minute >= openMinute);

            // Then check if closed time is ok
            bool beforeClosedHour = hour < closedHour || (hour == closedHour && minute <= closedMinute);

            // Return true if is ok to open hour and to closed hour
            return (passedOpenHour && beforeClosedHour);

        }
    }
}


