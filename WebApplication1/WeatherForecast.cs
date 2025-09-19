namespace WebApplication1
{
    /// <summary>
    /// Model pentru prognoza meteo - utilizat pentru testare
    /// </summary>
    public class WeatherForecast
    {
        public DateOnly Date { get; set; }

        public int TemperatureC { get; set; }

        /// <summary>
        /// Temperatura în Fahrenheit calculat? corect din Celsius
        /// Formula: F = (C × 9/5) + 32
        /// </summary>
        public int TemperatureF => (int)Math.Round((TemperatureC * 9.0 / 5.0) + 32);

        public string? Summary { get; set; }
        
        /// <summary>
        /// Descriere extins? pentru condi?iile meteo
        /// </summary>
        public string GetDetailedDescription()
        {
            var temp = TemperatureC;
            var description = temp switch
            {
                < -10 => "Foarte frig",
                < 0 => "Frig",
                < 10 => "R?coros",
                < 20 => "Pl?cut",
                < 30 => "Cald",
                _ => "Foarte cald"
            };
            
            return $"{description} - {temp}°C ({TemperatureF}°F)";
        }
        
        /// <summary>
        /// Verific? dac? condi?iile meteo sunt favorabile pentru ITP
        /// </summary>
        public bool IsSuitableForInspection()
        {
            return TemperatureC >= -5 && TemperatureC <= 35 && 
                   !Summary?.ToLower().Contains("storm") == true &&
                   !Summary?.ToLower().Contains("blizzard") == true;
        }
    }
}
