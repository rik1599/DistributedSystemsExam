namespace DroneSystemAPI
{
    public class Config2
    {
        public string SystemName { get; }
        public string RepositoryActorName { get; }

        private Config2(string systemName, string repositoryActorName)
        {
            SystemName = systemName;
            RepositoryActorName = repositoryActorName;
        }

        public static Config2 Default()
        {
            return new Config2("DroneDeliverySystem", "repository");
        }
    }
}
