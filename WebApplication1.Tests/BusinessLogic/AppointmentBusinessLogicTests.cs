using Xunit;
using WebApplication1.Models;

namespace WebApplication1.Tests.BusinessLogic
{
    public class AppointmentBusinessLogicTests
    {
        [Fact]
        public void Appointment_SchedulingConflict_ShouldBeDetectable()
        {
            // Arrange
            var appointment1 = new Appointment
            {
                AppointmentDate = new DateTime(2024, 6, 15, 10, 0, 0),
                DurationMinutes = 60
            };

            var appointment2 = new Appointment
            {
                AppointmentDate = new DateTime(2024, 6, 15, 10, 30, 0),
                DurationMinutes = 60
            };

            // Act & Assert
            // appointment1: 10:00 - 11:00
            // appointment2: 10:30 - 11:30
            // These overlap from 10:30 to 11:00
            Assert.True(HasTimeConflict(appointment1, appointment2));
        }

        [Fact]
        public void Appointment_NoSchedulingConflict_ShouldBeDetectable()
        {
            // Arrange
            var appointment1 = new Appointment
            {
                AppointmentDate = new DateTime(2024, 6, 15, 10, 0, 0),
                DurationMinutes = 60
            };

            var appointment2 = new Appointment
            {
                AppointmentDate = new DateTime(2024, 6, 15, 11, 0, 0),
                DurationMinutes = 60
            };

            // Act & Assert
            // appointment1: 10:00 - 11:00
            // appointment2: 11:00 - 12:00
            // These don't overlap (back-to-back is OK)
            Assert.False(HasTimeConflict(appointment1, appointment2));
        }

        [Theory]
        [InlineData(AppointmentStatus.Scheduled, AppointmentStatus.Confirmed, true)]
        [InlineData(AppointmentStatus.Confirmed, AppointmentStatus.InProgress, true)]
        [InlineData(AppointmentStatus.InProgress, AppointmentStatus.Completed, true)]
        [InlineData(AppointmentStatus.Scheduled, AppointmentStatus.Cancelled, true)]
        [InlineData(AppointmentStatus.Completed, AppointmentStatus.Scheduled, false)]
        [InlineData(AppointmentStatus.Cancelled, AppointmentStatus.Confirmed, false)]
        public void Appointment_StatusTransition_ShouldFollowBusinessRules(
            AppointmentStatus currentStatus, 
            AppointmentStatus newStatus, 
            bool isValidTransition)
        {
            // Act
            var result = IsValidStatusTransition(currentStatus, newStatus);

            // Assert
            Assert.Equal(isValidTransition, result);
        }

        [Fact]
        public void Appointment_WorkingHours_ShouldBeEnforced()
        {
            // Arrange
            var workingHoursStart = new TimeSpan(8, 0, 0);
            var workingHoursEnd = new TimeSpan(18, 0, 0);

            var validAppointment = new Appointment
            {
                AppointmentDate = DateTime.Today.Add(new TimeSpan(14, 0, 0)),
                DurationMinutes = 60
            };

            var invalidAppointment = new Appointment
            {
                AppointmentDate = DateTime.Today.Add(new TimeSpan(19, 0, 0)),
                DurationMinutes = 60
            };

            // Act & Assert
            Assert.True(IsWithinWorkingHours(validAppointment, workingHoursStart, workingHoursEnd));
            Assert.False(IsWithinWorkingHours(invalidAppointment, workingHoursStart, workingHoursEnd));
        }

        [Fact]
        public void Appointment_WeekendScheduling_ShouldBeHandled()
        {
            // Arrange
            var saturday = GetNextDayOfWeek(DayOfWeek.Saturday);
            var sunday = GetNextDayOfWeek(DayOfWeek.Sunday);
            var monday = GetNextDayOfWeek(DayOfWeek.Monday);

            var saturdayAppointment = new Appointment
            {
                AppointmentDate = saturday.AddHours(10),
                DurationMinutes = 60
            };

            var sundayAppointment = new Appointment
            {
                AppointmentDate = sunday.AddHours(10),
                DurationMinutes = 60
            };

            var mondayAppointment = new Appointment
            {
                AppointmentDate = monday.AddHours(10),
                DurationMinutes = 60
            };

            // Act & Assert
            Assert.False(IsWeekday(saturdayAppointment.AppointmentDate));
            Assert.False(IsWeekday(sundayAppointment.AppointmentDate));
            Assert.True(IsWeekday(mondayAppointment.AppointmentDate));
        }

        [Fact]
        public void Appointment_BufferTime_ShouldBeConsidered()
        {
            // Arrange
            var bufferMinutes = 15;
            var appointment1 = new Appointment
            {
                AppointmentDate = new DateTime(2024, 6, 15, 10, 0, 0),
                DurationMinutes = 60
            };

            var appointment2 = new Appointment
            {
                AppointmentDate = new DateTime(2024, 6, 15, 11, 10, 0),
                DurationMinutes = 60
            };

            // Act & Assert
            // appointment1: 10:00 - 11:00 (+ 15 min buffer = 11:15)
            // appointment2: 11:10 - 12:10
            // Buffer time overlaps
            Assert.True(HasTimeConflictWithBuffer(appointment1, appointment2, bufferMinutes));
        }

        [Theory]
        [InlineData(15)] // 15 minutes minimum
        [InlineData(30)]
        [InlineData(45)]
        [InlineData(60)]
        [InlineData(120)]
        [InlineData(480)] // 8 hours maximum
        public void Appointment_ValidDurations_ShouldBeAccepted(int duration)
        {
            // Act & Assert
            Assert.True(IsValidDuration(duration));
        }

        [Theory]
        [InlineData(10)]  // Below minimum
        [InlineData(5)]
        [InlineData(500)] // Above maximum
        [InlineData(600)]
        public void Appointment_InvalidDurations_ShouldBeRejected(int duration)
        {
            // Act & Assert
            Assert.False(IsValidDuration(duration));
        }

        [Fact]
        public void Appointment_PastDateScheduling_ShouldBeHandled()
        {
            // Arrange
            var pastAppointment = new Appointment
            {
                AppointmentDate = DateTime.Now.AddDays(-1),
                DurationMinutes = 60
            };

            var futureAppointment = new Appointment
            {
                AppointmentDate = DateTime.Now.AddDays(1),
                DurationMinutes = 60
            };

            // Act & Assert
            Assert.False(IsFutureAppointment(pastAppointment));
            Assert.True(IsFutureAppointment(futureAppointment));
        }

        [Fact]
        public void Appointment_DailyLimits_ShouldBeEnforced()
        {
            // Arrange
            var date = DateTime.Today.AddDays(1);
            var maxAppointmentsPerDay = 8;
            var existingAppointments = new List<Appointment>();

            // Create appointments for the day
            for (int i = 0; i < maxAppointmentsPerDay; i++)
            {
                existingAppointments.Add(new Appointment
                {
                    AppointmentDate = date.AddHours(9 + i),
                    DurationMinutes = 60
                });
            }

            var newAppointment = new Appointment
            {
                AppointmentDate = date.AddHours(18),
                DurationMinutes = 60
            };

            // Act & Assert
            Assert.False(CanScheduleAppointment(newAppointment, existingAppointments, maxAppointmentsPerDay));
        }

        // Helper methods for business logic
        private bool HasTimeConflict(Appointment appointment1, Appointment appointment2)
        {
            var start1 = appointment1.AppointmentDate;
            var end1 = start1.AddMinutes(appointment1.DurationMinutes);
            var start2 = appointment2.AppointmentDate;
            var end2 = start2.AddMinutes(appointment2.DurationMinutes);

            return start1 < end2 && start2 < end1;
        }

        private bool HasTimeConflictWithBuffer(Appointment appointment1, Appointment appointment2, int bufferMinutes)
        {
            var start1 = appointment1.AppointmentDate;
            var end1 = start1.AddMinutes(appointment1.DurationMinutes + bufferMinutes);
            var start2 = appointment2.AppointmentDate.AddMinutes(-bufferMinutes);
            var end2 = appointment2.AppointmentDate.AddMinutes(appointment2.DurationMinutes);

            return start1 < end2 && start2 < end1;
        }

        private bool IsValidStatusTransition(AppointmentStatus current, AppointmentStatus target)
        {
            return (current, target) switch
            {
                (AppointmentStatus.Scheduled, AppointmentStatus.Confirmed) => true,
                (AppointmentStatus.Scheduled, AppointmentStatus.Cancelled) => true,
                (AppointmentStatus.Scheduled, AppointmentStatus.Rescheduled) => true,
                (AppointmentStatus.Confirmed, AppointmentStatus.InProgress) => true,
                (AppointmentStatus.Confirmed, AppointmentStatus.Cancelled) => true,
                (AppointmentStatus.Confirmed, AppointmentStatus.Rescheduled) => true,
                (AppointmentStatus.InProgress, AppointmentStatus.Completed) => true,
                (AppointmentStatus.InProgress, AppointmentStatus.Cancelled) => true,
                (AppointmentStatus.Rescheduled, AppointmentStatus.Scheduled) => true,
                (AppointmentStatus.Rescheduled, AppointmentStatus.Confirmed) => true,
                _ => false
            };
        }

        private bool IsWithinWorkingHours(Appointment appointment, TimeSpan workingStart, TimeSpan workingEnd)
        {
            var appointmentTime = appointment.AppointmentDate.TimeOfDay;
            var appointmentEnd = appointmentTime.Add(TimeSpan.FromMinutes(appointment.DurationMinutes));

            return appointmentTime >= workingStart && appointmentEnd <= workingEnd;
        }

        private bool IsWeekday(DateTime date)
        {
            return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
        }

        private bool IsValidDuration(int duration)
        {
            return duration >= 15 && duration <= 480;
        }

        private bool IsFutureAppointment(Appointment appointment)
        {
            return appointment.AppointmentDate > DateTime.Now;
        }

        private bool CanScheduleAppointment(Appointment newAppointment, List<Appointment> existingAppointments, int maxPerDay)
        {
            var appointmentsOnSameDay = existingAppointments
                .Count(a => a.AppointmentDate.Date == newAppointment.AppointmentDate.Date);

            return appointmentsOnSameDay < maxPerDay;
        }

        private DateTime GetNextDayOfWeek(DayOfWeek dayOfWeek)
        {
            var today = DateTime.Today;
            int daysUntilTarget = ((int)dayOfWeek - (int)today.DayOfWeek + 7) % 7;
            if (daysUntilTarget == 0) daysUntilTarget = 7; // Next week if today is the target day
            return today.AddDays(daysUntilTarget);
        }
    }
}