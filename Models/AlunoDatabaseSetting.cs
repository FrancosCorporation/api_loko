namespace first_api.Models
{
    public class AlunoDatabaseSetting : IAlunoDatabaseSetting
    {
        public string AlunoCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public interface IAlunoDatabaseSetting
    {
        string AlunoCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}