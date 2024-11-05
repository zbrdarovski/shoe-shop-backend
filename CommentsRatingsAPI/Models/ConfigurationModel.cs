namespace CartPaymentAPI.Models
{
    public class ConfigurationModel
    {
        public string MongoConnectionString { get; set; }
        public string JwtSecretKey { get; set; }

        public ConfigurationModel(IConfiguration configuration)
        {
            MongoConnectionString = configuration["MongoConnectionString"];
            JwtSecretKey = configuration["JwtSecretKey"];
        }
    }
}
