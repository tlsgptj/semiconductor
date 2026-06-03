using semi.EquipmentSimulator;

var server = new EquipmentServer(port: 5000);

await server.StartAsync();