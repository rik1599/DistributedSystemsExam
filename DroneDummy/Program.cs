using UI.Verbs;

var options = new DroneOptions()
{
    Xstart = 0,
    Ystart = 0,
    Xend = 200,
    Yend = 200,
    Speed = 5.0f,
    Protocol = Protocol.tcp,
    Hostname = "localhost",
    Port = 8080
};

DroneOptions.Run(options);
