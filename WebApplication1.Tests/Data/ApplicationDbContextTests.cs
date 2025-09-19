using Xunit;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Tests.Data
{
    public class ApplicationDbContextTests : IDisposable
    {
        private readonly ApplicationDbContext _context;

        public ApplicationDbContextTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
        }

        [Fact]
        public void ApplicationDbContext_ShouldHaveAppointmentsDbSet()
        {
            // Assert
            Assert.NotNull(_context.Appointments);
        }

        [Fact]
        public async Task ApplicationDbContext_ShouldSaveAppointment()
        {
            // Arrange
            var appointment = new Appointment
            {
                Title = "Test Appointment",
                ClientName = "Test Client",
                AppointmentDate = DateTime.Now.AddHours(2),
                DurationMinutes = 60
            };

            // Act
            _context.Appointments.Add(appointment);
            var result = await _context.SaveChangesAsync();

            // Assert
            Assert.Equal(1, result);
            Assert.True(appointment.Id > 0);
        }

        [Fact]
        public async Task ApplicationDbContext_ShouldQueryAppointments()
        {
            // Arrange
            var appointment1 = new Appointment
            {
                Title = "Test Appointment 1",
                ClientName = "Test Client 1",
                AppointmentDate = DateTime.Now.AddHours(1),
                DurationMinutes = 60
            };

            var appointment2 = new Appointment
            {
                Title = "Test Appointment 2",
                ClientName = "Test Client 2",
                AppointmentDate = DateTime.Now.AddHours(2),
                DurationMinutes = 90
            };

            _context.Appointments.AddRange(appointment1, appointment2);
            await _context.SaveChangesAsync();

            // Act
            var appointments = await _context.Appointments.ToListAsync();

            // Assert
            Assert.Equal(2, appointments.Count);
            Assert.Contains(appointments, a => a.Title == "Test Appointment 1");
            Assert.Contains(appointments, a => a.Title == "Test Appointment 2");
        }

        [Fact]
        public async Task ApplicationDbContext_ShouldUpdateAppointment()
        {
            // Arrange
            var appointment = new Appointment
            {
                Title = "Original Title",
                ClientName = "Test Client",
                AppointmentDate = DateTime.Now.AddHours(2),
                DurationMinutes = 60
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Act
            appointment.Title = "Updated Title";
            appointment.UpdatedAt = DateTime.Now;
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();

            // Assert
            var updatedAppointment = await _context.Appointments.FindAsync(appointment.Id);
            Assert.Equal("Updated Title", updatedAppointment.Title);
            Assert.NotNull(updatedAppointment.UpdatedAt);
        }

        [Fact]
        public async Task ApplicationDbContext_ShouldDeleteAppointment()
        {
            // Arrange
            var appointment = new Appointment
            {
                Title = "Test Appointment",
                ClientName = "Test Client",
                AppointmentDate = DateTime.Now.AddHours(2),
                DurationMinutes = 60
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            var appointmentId = appointment.Id;

            // Act
            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            // Assert
            var deletedAppointment = await _context.Appointments.FindAsync(appointmentId);
            Assert.Null(deletedAppointment);
        }

        [Fact]
        public async Task ApplicationDbContext_ShouldHandleAppointmentStatusEnum()
        {
            // Arrange
            var appointment = new Appointment
            {
                Title = "Test Appointment",
                ClientName = "Test Client",
                AppointmentDate = DateTime.Now.AddHours(2),
                DurationMinutes = 60,
                Status = AppointmentStatus.Confirmed
            };

            // Act
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Assert
            var savedAppointment = await _context.Appointments.FindAsync(appointment.Id);
            Assert.Equal(AppointmentStatus.Confirmed, savedAppointment.Status);
        }

        [Fact]
        public async Task ApplicationDbContext_ShouldFilterAppointmentsByDate()
        {
            // Arrange
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var todayAppointment = new Appointment
            {
                Title = "Today Appointment",
                ClientName = "Test Client",
                AppointmentDate = today.AddHours(14),
                DurationMinutes = 60
            };

            var tomorrowAppointment = new Appointment
            {
                Title = "Tomorrow Appointment",
                ClientName = "Test Client",
                AppointmentDate = tomorrow.AddHours(14),
                DurationMinutes = 60
            };

            _context.Appointments.AddRange(todayAppointment, tomorrowAppointment);
            await _context.SaveChangesAsync();

            // Act
            var todayAppointments = await _context.Appointments
                .Where(a => a.AppointmentDate.Date == today)
                .ToListAsync();

            // Assert
            Assert.Single(todayAppointments);
            Assert.Equal("Today Appointment", todayAppointments[0].Title);
        }

        [Fact]
        public async Task ApplicationDbContext_ShouldFilterAppointmentsByStatus()
        {
            // Arrange
            var confirmedAppointment = new Appointment
            {
                Title = "Confirmed Appointment",
                ClientName = "Test Client",
                AppointmentDate = DateTime.Now.AddHours(2),
                DurationMinutes = 60,
                Status = AppointmentStatus.Confirmed
            };

            var scheduledAppointment = new Appointment
            {
                Title = "Scheduled Appointment",
                ClientName = "Test Client",
                AppointmentDate = DateTime.Now.AddHours(3),
                DurationMinutes = 60,
                Status = AppointmentStatus.Scheduled
            };

            _context.Appointments.AddRange(confirmedAppointment, scheduledAppointment);
            await _context.SaveChangesAsync();

            // Act
            var confirmedAppointments = await _context.Appointments
                .Where(a => a.Status == AppointmentStatus.Confirmed)
                .ToListAsync();

            // Assert
            Assert.Single(confirmedAppointments);
            Assert.Equal("Confirmed Appointment", confirmedAppointments[0].Title);
        }

        [Fact]
        public async Task ApplicationDbContext_ShouldOrderAppointmentsByDate()
        {
            // Arrange
            var appointment1 = new Appointment
            {
                Title = "Later Appointment",
                ClientName = "Test Client",
                AppointmentDate = DateTime.Now.AddHours(3),
                DurationMinutes = 60
            };

            var appointment2 = new Appointment
            {
                Title = "Earlier Appointment",
                ClientName = "Test Client",
                AppointmentDate = DateTime.Now.AddHours(1),
                DurationMinutes = 60
            };

            _context.Appointments.AddRange(appointment1, appointment2);
            await _context.SaveChangesAsync();

            // Act
            var orderedAppointments = await _context.Appointments
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();

            // Assert
            Assert.Equal(2, orderedAppointments.Count);
            Assert.Equal("Earlier Appointment", orderedAppointments[0].Title);
            Assert.Equal("Later Appointment", orderedAppointments[1].Title);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}